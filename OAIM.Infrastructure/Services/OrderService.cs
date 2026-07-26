
using AutoMapper;
using OAIM.Application.Data;
using OAIM.Application.DTO;
using OAIM.Domain.Interfaces;
using Serilog;

namespace OAIM.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IRepository<Order, int> _orderRepository;
        private IRepository<Product, int> _productRepository;
        private IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public OrderService(IRepository<Order, int> orderRepository, IRepository<Product, int> productRepository, IUnitOfWork unitOfWork, ILogger logger, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }
        public async Task<OrderResponseDto> CreateOrderAsync(CreateOrderDto orderDto)
        {
            if (orderDto.Items == null || !orderDto.Items.Any())
            {
                _logger.Warning("CreateOrder failed: No items provided for Customer {CustomerId}", orderDto.CustomerId);
                throw new Exception("Order must contain at least one item.");
            }
            _logger.Information("Starting order creation for Customer {CustomerId} with {ItemCount} items",
                orderDto.CustomerId, orderDto.Items.Count);

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var order = new Order
                {
                    CreatedById = (Guid)orderDto.CreatedById,
                    UpdatedById = null,
                    UpdateDate = null,
                    CustomerId = orderDto.CustomerId,
                    OrderDate = DateTime.Now,
                    Items = new List<OrderItem>(),
                    CustomerType = orderDto.CustomerType
                };

                decimal totalAmount = 0;

                var incomingItems = orderDto.Items.ToDictionary(i => i.ProductId);

                var products = await _productRepository.GetAll(null)
                    .Where(p => incomingItems.Keys.Contains(p.Id))
                    .ToDictionaryAsync(p => p.Id);

                foreach (var item in orderDto.Items)
                {
                    if (!products.TryGetValue(item.ProductId, out var product))
                    {
                        _logger.Warning($"Product not found: ProductId {item.ProductId}");
                        throw new Exception($"Product with ID {item.ProductId} not found.");
                    }

                    if (product.StockQuantity < item.Quantity)
                    {
                        _logger.Warning($"Insufficient stock for Product {product.Id}. Available: {product.StockQuantity}, Requested: {item.Quantity}");

                        throw new Exception($"Insufficient stock for product {product.Name}.");
                    }

                    var orderItem = new OrderItem
                    {
                        ProductId = product.Id,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price
                    };

                    _logger.Information($"Reducing stock for Product {product.Id} by {item.Quantity}",
                        product.Id, item.Quantity);

                    product.StockQuantity -= item.Quantity;

                    totalAmount += item.Quantity * product.Price;
                    order.Items.Add(orderItem);
                }

                order.TotalAmount = totalAmount;

                var result = await _orderRepository.AddAsync(order);
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.Information($"Order {result.Id} created successfully for Customer {orderDto.CustomerId} with Total {result.TotalAmount} at {result.OrderDate} by {orderDto.CreatedById}");
                var dto = _mapper.Map<OrderResponseDto>(order);
                dto.CreatedByName = orderDto.CreatedByName;
                return dto;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync();

                _logger.Error(ex,
                    $"Concurrency conflict while creating order for Customer {orderDto.CustomerId}");

                throw new Exception("Concurrency conflict occurred while creating the order. Please try again.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                _logger.Error(ex,
                    $"Unexpected error while creating order for Customer {orderDto.CustomerId} at {DateTime.Now}");
                throw;
            }
        }
        public async Task DeleteOrder(int orderId, string user)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var order = await GetOrderByIdAsync(orderId);
                if (order == null)
                {
                    _logger.Warning($"Attempted to delete non-existent Order {orderId}");
                    throw new KeyNotFoundException($"Order with ID {orderId} not found.");
                }
                var productIds = order.Items.Select(i => i.ProductId).ToList();

                var products = await _productRepository.GetAll(null)
                    .Where(p => productIds.Contains(p.Id))
                    .ToDictionaryAsync(p => p.Id);

                var itemsToRemove = new List<OrderItem>();

                foreach (var item in order.Items)
                {
                    if (products.TryGetValue(item.ProductId, out var product))
                    {
                        product.StockQuantity += item.Quantity;

                        _logger.Information(
                            $"Restored stock for Product {product.Id} by {item.Quantity} due to Order {order.Id} deletion");

                        itemsToRemove.Add(item);
                    }
                }

                foreach (var item in itemsToRemove)
                {
                    order.Items.Remove(item);
                }
                await _orderRepository.Delete(orderId);
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();
                _logger.Information($"Order {orderId} deleted successfully at {DateTime.Now} by {user}");

            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.Error(ex, $"Concurrency conflict while deleting Order {orderId}");
                throw new Exception("Concurrency conflict occurred while deleting the order. Please try again.");
                await transaction.RollbackAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.Error(ex, $"Error occurred while deleting Order {orderId} at {DateTime.Now}");
                throw;
            }
        }

        public async Task<PagedResult<Order>> GetAllOrders(int pageNumber, int pageSize)
        {
            pageSize = pageSize > 100 ? 100 : pageSize;
            var query = _orderRepository
           .GetAll(includes: new[]
            {
               nameof(Order.Items), nameof(Order.Customer), nameof(Order.CreatedBy), nameof(Order.UpdatedBy)
            }).AsNoTracking();
          
            var totalCount = await query.CountAsync();
            var items = await query
                        .OrderBy(o => o.Id).
                        OrderBy(o => o.OrderDate)
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();
            return new PagedResult<Order>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<Order> GetOrderByIdAsync(int orderId)

        {
            return await _orderRepository
                .FindAsync(o => o.Id == orderId, includes: [i => i.Items, i => i.UpdatedBy, i => i.CreatedBy, i => i.Customer]);
        }
        public async Task<OrderResponseDto> UpdateOrder(int id, CreateOrderDto orderDto)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var order = await GetOrderByIdAsync(id);
                if (order == null)
                    throw new KeyNotFoundException($"Order with ID {id} not found.");

                order.CustomerId = orderDto.CustomerId;
                order.UpdatedById = orderDto.UpdatedById;
                order.UpdateDate = DateTime.Now;
                order.CreatedById = order.CreatedById;

                // Map existing items by ProductId
                var existingItems = order.Items.ToDictionary(i => i.ProductId);

                // Map incoming items by ProductId
                var incomingItems = orderDto.Items.ToDictionary(i => i.ProductId);

                var productIIds = existingItems.Keys.Union(incomingItems.Keys).Distinct().ToList();
                var products = await _productRepository.GetAll(null)
                    .Where(p => productIIds.Contains(p.Id))
                    .ToDictionaryAsync(p => p.Id);
                var itemsToRemove = new List<OrderItem>();
                decimal totalAmount = 0;


                // Handle Updated & New Items
                foreach (var incoming in incomingItems)
                {
                    var product = products[incoming.Key];
                    if (product == null)
                    {
                        _logger.Warning($"Product not found during order update: ProductId {incoming.Key}");
                        throw new KeyNotFoundException($"Product with ID {incoming.Key} not found.");
                    }

                    var newQty = incoming.Value.Quantity;

                    if (existingItems.TryGetValue(incoming.Key, out var existingItem))
                    {
                        if (newQty < 0)
                        {
                            _logger.Warning($"Invalid quantity for Product {product.Id} during order update. Quantity must be greater than zero. Provided: {newQty}");
                            throw new InvalidOperationException($"Quantity for product '{product.Name}' must be greater than zero.");
                        }
                        if (newQty == 0)
                        {
                            // Handle as removal
                            product.StockQuantity += existingItem.Quantity;
                            _logger.Information($"Restored stock for Product {product.Id} by {existingItem.Quantity} due to quantity set to zero in Order {order.Id}");
                            itemsToRemove.Add(existingItem);
                            continue; // skip to next item
                        }
                        // 🔹 Existing product in order → calculate delta
                        var delta = newQty - existingItem.Quantity;

                        if (delta > 0 && product.StockQuantity < delta)
                        {
                            _logger.Warning($"Insufficient stock for Product {product.Id} during order update. Available: {product.StockQuantity}, Required additional: {delta}");
                            throw new InvalidOperationException(
                                     $"Insufficient stock for product '{product.Name}'. Available: {product.StockQuantity}");

                        }

                        product.StockQuantity -= delta; // if delta negative → stock increases
                        existingItem.Quantity = newQty;
                        existingItem.UnitPrice = product.Price;
                    }
                    else
                    {
                        // 🔹 New product added to order
                        if (product.StockQuantity < newQty)
                        {
                            _logger.Warning($"Insufficient stock for Product {product.Id} during order update. Available: {product.StockQuantity}, Required: {newQty}");
                            throw new InvalidOperationException(
                                    $"Insufficient stock for product '{product.Name}'. Available: {product.StockQuantity}");

                        }

                        product.StockQuantity -= newQty;

                        order.Items.Add(new OrderItem
                        {
                            ProductId = product.Id,
                            Quantity = newQty,
                            UnitPrice = product.Price
                        });
                    }

                    totalAmount += newQty * product.Price;
                }

                // 2️⃣ Handle Removed Items
                var removedItems = existingItems.Keys.Except(incomingItems.Keys);
                foreach (var removedProductId in removedItems)
                {
                    var removedItem = existingItems[removedProductId];

                    var product = products[removedProductId];
                    if (product == null)
                    {
                        _logger.Warning($"Product not found during order update (removal): ProductId {removedProductId}");
                        throw new KeyNotFoundException($"Product with ID {removedProductId} not found.");
                    }

                    // Return full quantity to stock
                    product.StockQuantity += removedItem.Quantity;
                    _logger.Information($"Restored stock for Product {product.Id} by {removedItem.Quantity} due to item removal in Order {order.Id}");
                    itemsToRemove.Add(removedItem);
                }
                foreach (var item in itemsToRemove)
                {
                    order.Items.Remove(item);
                }
                order.TotalAmount = totalAmount;
                if (order.Items.Count == 0)
                {
                    _logger.Warning($"Order {order.Id} has no items after update. Consider deleting the order instead of updating.");
                    _orderRepository.Delete(order.Id);
                }
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();
                _logger.Information($"Order {order.Id} updated successfully for Customer {order.CustomerId} with Total {order.TotalAmount} at {DateTime.Now}");
                var dto = _mapper.Map<OrderResponseDto>(order);
                dto.UpdatedByName = orderDto.UpdatedByName;
                dto.CreatedByName = orderDto.CreatedByName;
                return dto;
            }
            catch
            {
                _logger.Error($"Error occurred while updating Order {id} for Customer {orderDto.CustomerId} at {DateTime.Now}");
                await transaction.RollbackAsync();
                throw;
            }
        }

        public Task<List<Order>> GetOrdersByUserIdAsync(Guid userId, int pageNumber, int pageSize)
        {
            var query = _orderRepository
            .GetAll(includes: new[]
            {
                nameof(Order.Items), nameof(Order.Customer), nameof(Order.CreatedBy), nameof(Order.UpdatedBy)
            }).AsNoTracking()
                .Include(o => o.Items)
                .Include(o => o.Customer)
                .Include(o => o.CreatedBy)
                .Include(o => o.UpdatedBy)
                .Where(o => o.CreatedById == userId);
            return query
                        .OrderBy(o => o.Id)
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();
        }

       
       
    }
}
