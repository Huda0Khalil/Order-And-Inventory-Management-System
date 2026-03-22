using System.Text;

namespace OAIM.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IRepository<Order> _orderRepository;
        private IRepository<Product> _productRepository;
        private IUnitOfWork _unitOfWork;
        public OrderService(IRepository<Order> orderRepository, IRepository<Product> productRepository, IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<Order> CreateOrderAsync(CreateOrderDto orderDto)
        {
            if (orderDto.Items == null || !orderDto.Items.Any())
                throw new Exception("Order must contain at least one item.");

            using var transaction = await _unitOfWork.BeginTransactionAsync();//search about using and block
            try
            {
                var order = new Order
                {
                    CustomerId = orderDto.CustomerId,
                    OrderDate = DateTime.UtcNow,
                    Items = new List<OrderItem>()
                };
                decimal totalAmount = 0;
                foreach (var item in orderDto.Items)
                {
                    var product = await _productRepository.GetByIdAsync(item.ProductId);
                    if (product == null)
                        throw new Exception($"Product with ID {item.ProductId} not found.");
                    if (product.StockQuantity < item.Quantity)
                        throw new Exception($"Insufficient stock for product {product.Name}.");
                    var orderItem = new OrderItem
                    {
                        ProductId = product.Id,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price
                    };
                    totalAmount += item.Quantity * product.Price;
                    order.Items.Add(orderItem);
                }
                order.TotalAmount = totalAmount;
                var result = await _orderRepository.AddAsync(order);
                await transaction.CommitAsync();
                return result;
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                throw new Exception("Concurrency conflict occurred while creating the order. Please try again.");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }



        }

        public void DeleteOrder(int orderId)
        {
            throw new NotImplementedException();
        }

        public async Task<PagedResult<Order>> GetAllOrders(int pageNumber, int pageSize)
        {
            pageSize = pageSize > 100 ? 100 : pageSize;
            var query = _orderRepository
           .GetAll()
           .AsNoTracking()
           .Include(o => o.Items)
           .Include(o => o.Customer);
            var totalCount = await query.CountAsync();
            var items = await query
                        .OrderBy(o => o.Id)
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

        public Order GetOrderById(int orderId)
        {
            var order = _orderRepository.Find(o => o.Id == orderId, includes: i => i.Items);
            return order;
        }
        public async Task<Order> UpdateOrder(int id, CreateOrderDto orderDto)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var order = GetOrderById(id);
                if (order == null)
                    throw new KeyNotFoundException($"Order with ID {id} not found.");

                order.CustomerId = orderDto.CustomerId;

                // Map existing items by ProductId
                var existingItems = order.Items.ToDictionary(i => i.ProductId);

                // Map incoming items by ProductId
                var incomingItems = orderDto.Items.ToDictionary(i => i.ProductId);

                var productIIds = existingItems.Keys.Union(incomingItems.Keys).Distinct().ToList();
                var products = _productRepository.GetAll()
                    .Where(p => productIIds.Contains(p.Id))
                    .ToDictionary(p => p.Id);
                decimal totalAmount = 0;

                // 1️⃣ Handle Updated & New Items
                foreach (var incoming in incomingItems)
                {
                    var product = products[incoming.Key];
                    if (product == null)
                        throw new KeyNotFoundException($"Product with ID {incoming.Key} not found.");

                    var newQty = incoming.Value.Quantity;

                    if (existingItems.TryGetValue(incoming.Key, out var existingItem))
                    {
                        // 🔹 Existing product in order → calculate delta
                        var delta = newQty - existingItem.Quantity;

                        if (delta > 0 && product.StockQuantity < delta)
                            throw new InvalidOperationException(
                                $"Insufficient stock for product '{product.Name}'. Available: {product.StockQuantity}");

                        product.StockQuantity -= delta; // if delta negative → stock increases
                        existingItem.Quantity = newQty;
                        existingItem.UnitPrice = product.Price;
                    }
                    else
                    {
                        // 🔹 New product added to order
                        if (product.StockQuantity < newQty)
                            throw new InvalidOperationException(
                                $"Insufficient stock for product '{product.Name}'. Available: {product.StockQuantity}");

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
                        throw new KeyNotFoundException($"Product with ID {removedProductId} not found.");

                    // Return full quantity to stock
                    product.StockQuantity += removedItem.Quantity;

                    order.Items.Remove(removedItem);
                }

                order.TotalAmount = totalAmount;

                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                return order;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        //public async Task<Order> UpdateOrder(int id, CreateOrderDto orderDto)
        //{
        //    using var transaction = _unitOfWork.BeginTransactionAsync();
        //    try
        //    {
        //        Order order = await _orderRepository.GetByIdAsync(id);
        //        if (order == null)
        //            throw new Exception($"Order with ID {id} not found.");
        //        order.CustomerId = orderDto.CustomerId;
        //        var existingItems = order.Items.ToDictionary(i => i.ProductId);
        //        var incomingItems = orderDto.Items.ToDictionary(i => i.ProductId);
        //        decimal totalAmount = 0;
        //        foreach (var incomingItem in incomingItems)
        //        {
        //            var product = await _productService.GetProductByIdAsync(incomingItem.Key);
        //            if (product == null)
        //                throw new Exception($"Product with ID {incomingItem.Key} not found.");
        //            if (product.StockQuantity < incomingItem.Value.Quantity)
        //                throw new Exception($"Insufficient stock for product {product.Name}.");
        //            var newQty = incomingItem.Value.Quantity;
        //            if (existingItems.TryGetValue(incomingItem.Key, out var existingItem))
        //            {
        //                existingItem.Quantity = incomingItem.Value.Quantity;
        //                existingItem.UnitPrice = product.Price;
        //            }
        //            else
        //            {
        //                var orderItem = new OrderItem
        //                {
        //                    ProductId = product.Id,
        //                    Quantity = incomingItem.Value.Quantity,
        //                    UnitPrice = product.Price
        //                };
        //                order.Items.Add(orderItem);
        //            }
        //            totalAmount += incomingItem.Value.Quantity * product.Price;

        //        }

        //    }
        //    catch
        //    {

        //    }


        //    ////order.OrderDate = DateTime.UtcNow;
        //    //order.Items.Clear();

        //    //foreach (var item in orderDto.Items)
        //    //{
        //    //    var product = await _productService.GetProductByIdAsync(item.ProductId);
        //    //    if (product == null)
        //    //        throw new Exception($"Product with ID {item.ProductId} not found.");
        //    //    if (product.StockQuantity < item.Quantity)
        //    //        throw new Exception($"Insufficient stock for product {product.Name}.");
        //    //    var orderItem = new OrderItem
        //    //    {
        //    //        ProductId = product.Id,
        //    //        Quantity = item.Quantity,
        //    //        UnitPrice = product.Price
        //    //    };
        //    //    totalAmount += item.Quantity * product.Price;
        //    //    order.Items.Add(orderItem);
        //    //}
        //    //order.TotalAmount = totalAmount;
        //    //var result = await _orderRepository.Update(order);
        //    //return result;
        //}
    }
}
