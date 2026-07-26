using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OAIM.Application.DTO;

namespace OAIM.API.Controllers
{
    [Authorize] 
    [Route("api/[controller]")]
    [ApiController]
    public class SupplierController : ControllerBase
    {
        private readonly ISupplierService _supplierService;
        public SupplierController(ISupplierService supplierService)
        {
            _supplierService = supplierService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateSupplier(SupplierDto supplier)
        {
            try
            {
                var result = await _supplierService.CreateSupplierAsync(supplier);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetAllSuppliers(int pageNumber, int pageSize)
        {
            var result = await _supplierService.GetAllSuppliersAsync(pageNumber, pageSize);
            if ((result == null))
            {
                return NotFound("No suppliers found.");
            }
            return Ok(result);
        }
        [HttpGet("getList")]
        public async Task<IActionResult> GetAllSuppliers()
        {
            var result = await _supplierService.GetListSupplier();
            if ((result == null))
            {
                return NotFound("No suppliers found.");
            }
            return Ok(result);
        }
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetSupplierById(int id)
        {
            var result = await _supplierService.GetSupplierByIdAsync(id);
            if (result == null)
            {
                return NotFound($"Supplier with id {id} not found.");
            }
            return Ok(result);
        }
        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> UpdateSupplier(int id, SupplierDto supplier)
        {
            try
            {
                var result = await _supplierService.UpdateSupplier(id, supplier);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteSupplier(int id)
        {
            var result = await _supplierService.DeleteSupplierAsync(id);
            if (!result)
            {
                return NotFound($"Supplier with id {id} not found.");
            }
            return Ok($"Supplier with id {id} deleted");
        }
    }
}
