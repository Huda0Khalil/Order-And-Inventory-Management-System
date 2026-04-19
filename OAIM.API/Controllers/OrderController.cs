using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OAIM.Application.DTO;
using OAIM.Domain.Entities;
using Serilog;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OAIM.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;
        string userId = "admin";
        public OrderController(IOrderService orderService, ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }
        [Authorize(Roles = "Employee")]
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromHeader(Name = "tenant")] string tenant, [FromBody] CreateOrderDto orderDto)
        {
            try
            {
                orderDto.CreatedById = Guid.Parse(User.FindFirst("domainUserId")?.Value);
                orderDto.CreatedByName = User.FindFirst(ClaimTypes.Name)?.Value;
                var order = await _orderService.CreateOrderAsync(orderDto);
                return Ok(order);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetAllOrders([FromHeader(Name = "tenant")] string tenant, int pageNumber, int pageSize)
        {
            var result = await _orderService.GetAllOrders(pageNumber, pageSize);
            if ((result == null))
            {
                return NotFound("No orders found.");
            }
            return Ok(result);
        }
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetOrderById([FromHeader(Name = "tenant")] string tenant, int id)
        {
            var result = await _orderService.GetOrderByIdAsync(id);
            if (result == null)
            {
                return NotFound($"Order with Id {id} was not found.");
            }
            return Ok(result);
        }
        [HttpGet]
        [Route("userOrders")]
        public async Task<IActionResult> GetOrdersByUserId([FromHeader(Name = "tenant")] string tenant, string userId,int pageNumber, int pageSize)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID not found in token.");
            }
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var EmpId = User.FindFirst("domainUserId")?.Value;

            if (userId.Equals(EmpId, StringComparison.OrdinalIgnoreCase) || role == "Admin")
            {
                var result = await _orderService.GetOrdersByUserIdAsync(Guid.Parse(userId), pageNumber, pageSize);
                if (result == null || !result.Any())
                {
                    return NotFound("No orders found for the user.");
                }
                return Ok(result);
            }
            else
            {
                return Forbid("You are not authorized to view these orders.");
            }
        }
        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> UpdateOrder([FromHeader(Name = "tenant")] string tenant, int id, [FromBody] CreateOrderDto orderDto)
        {
            try
            {
                orderDto.UpdatedById = Guid.Parse(User.FindFirst("domainUserId")?.Value);
                orderDto.UpdatedByName = User.FindFirst(ClaimTypes.Name)?.Value;
                var result = await _orderService.UpdateOrder(id, orderDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteOrder([FromHeader(Name = "tenant")] string tenant, int id)
        {
            try
            {
                string user = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
                await _orderService.DeleteOrder(id,user);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
    }
