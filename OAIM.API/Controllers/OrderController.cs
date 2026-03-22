using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OAIM.Application.DTO;

namespace OAIM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromHeader(Name = "tenant")] string tenant,[FromBody] CreateOrderDto orderDto)
        {
            try
            {
                var order = await _orderService.CreateOrderAsync(orderDto);
                return Ok(order);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetAllOrders([FromHeader(Name = "tenant")] string tenant,int pageNumber, int pageSize)
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
        public IActionResult GetOrderById([FromHeader(Name = "tenant")] string tenant,int id)
        {
            var result = _orderService.GetOrderById(id);
            if (result == null)
            {
                return NotFound($"Order with Id {id} was not found.");
            }
            return Ok(result);
        }
        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> UpdateOrder([FromHeader(Name = "tenant")] string tenant, int id, [FromBody] CreateOrderDto orderDto)
        {
            try
            {  
                var result = await _orderService.UpdateOrder(id,orderDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
    }
