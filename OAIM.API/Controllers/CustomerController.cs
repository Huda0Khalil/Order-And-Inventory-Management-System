using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OAIM.Application.DTO;

namespace OAIM.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateCustomer(CustomerDto customerDto)
        {
            try
            {
                var result = await _customerService.CreateCustomerAsync(customerDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetAllCustomers(int pageNumber, int pageSize)
        {
            var result = await _customerService.GetAllCustomersAsync(pageNumber, pageSize);
            if ((result == null))
            {
                return NotFound("No Customers found.");
            }
            return Ok(result);
        }
        [HttpGet("getList")]
        public async Task<IActionResult> GetAllCustomers()
        {
            var result = await _customerService.GetListCustomer();
            if ((result == null))
            {
                return NotFound("No Customers found.");
            }
            return Ok(result);
        }
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetCustomerById(int id)
        {
            var result = await _customerService.GetCustomerById(id);
            if (result == null)
            {
                return NotFound($"Customer with id {id} not found.");
            }
            return Ok(result);
        }
        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, CustomerDto Customer)
        {
            try
            {
                var result = await _customerService.UpdateCustomer(id, Customer);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var result = await _customerService.DeleteCustomerAsync(id);
            if (!result)
            {
                return NotFound($"Customer with id {id} not found.");
            }
            return Ok($"Customer with id {id} deleted");
        }


    }
}
