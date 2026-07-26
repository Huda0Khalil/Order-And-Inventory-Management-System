using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using OAIM.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OAIM.Infrastructure.Services
{
    public class DashboardService : IDashboardService
    {
        //private readonly IDistributedCache _redis;
        private readonly IRepository<Order, int> _orderRepository;
        private readonly IRepository<Product, int> _productRepository;
        private readonly IRepository<Customer, int> _customerRepository;
        //private readonly IRepository<Product, int> _productRepository;

        public DashboardService(IRepository<Order,int> orderRepository, IRepository<Product, int> productRepository,
            IRepository<Customer,int> customerRepository)
        {
            //_redis = redis;
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _customerRepository = customerRepository;
        }

        public async Task<DashboardDto> GetDashboard()
        {
            //var cacheData = await _redis.GetAsync("dashboard");

            //DashboardDto? cache = null;
            //if (cacheData != null)
            //{
            //    cache = JsonSerializer.Deserialize<DashboardDto>(cacheData);
            //}

            //if (cache != null)
            //    return cache;

            var dashboard = await BuildDashboard();

            var serializedDashboard = JsonSerializer.SerializeToUtf8Bytes(dashboard);
            //await _redis.SetAsync(
            //    "dashboard",
            //    serializedDashboard,
            //    new DistributedCacheEntryOptions
            //    {
            //        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
            //    });

            return (DashboardDto)dashboard;
        }
        public Task<int> GetOrdersToday()
        {
            return _orderRepository.GetAll(null).Where(x => x.OrderDate.Date == DateTime.Today).CountAsync();
        }
        public Task<int> GetProducts()
        {
            return _productRepository.GetAll(null).CountAsync();
        }
        public Task<int> GetCustomers()
        {
            return _customerRepository.GetAll(null).CountAsync();
        }
        public async Task<List<RecentOrderDto>> GetRecentOrders()
        {
            var recentOrders = await _orderRepository.GetAll(null)
                .OrderByDescending(x => x.OrderDate)
                .Take(10)
                .Select(order => new RecentOrderDto
                {
                    Id = order.Id,
                    OrderNumber = order.Id.ToString(), // Assuming OrderNumber is derived from Id
                    //CustomerName = order.Customer != null ? order.Customer.Name : "Unknown", // Assuming Customer has a Name property
                    Type = order.CustomerType.ToString(), // Assuming CustomerType is an enum
                    Amount = order.TotalAmount,
                    Status = "Pending", // Replace with actual status if available
                    OrderDate = order.OrderDate
                })
                .ToListAsync();

            return recentOrders;
        }
        public async Task<decimal> GetRevenueToday()
        {
           return await _orderRepository.GetAll(null)
            .Where(x => x.OrderDate.Date == DateTime.Today)
            .SumAsync(x => x.TotalAmount);
        }
        public async Task<List<LowStockProductDto>> GetLowStockProducts()
        {
            var lowStockProducts = await _productRepository.GetAll(null)
                .Where(x => x.StockQuantity <= 10)
                .OrderBy(x => x.StockQuantity)
                .Take(10)
                .Select(product => new LowStockProductDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    SKU = product.Barcode, // Assuming Barcode maps to SKU
                    Stock = product.StockQuantity,
                    MinimumStock = 10 // Assuming a fixed minimum stock threshold
                })
                .ToListAsync();

            return lowStockProducts;
        }
        public async Task<DashboardDto> BuildDashboard()
        {
            var orders = await GetOrdersToday();
            var revenue = await GetRevenueToday();
            var products = await GetProducts();
            var customers = await GetCustomers();
            var recentOrders = await GetRecentOrders();
            var lowStock = await GetLowStockProducts();

            return new DashboardDto
            {
                TotalOrdersToday = orders,
                RevenueToday = revenue,
                TotalProducts = products,
                ActiveCustomers = customers,
                RecentOrders = recentOrders,
                LowStockProducts = lowStock
            };
         }
    }
}
