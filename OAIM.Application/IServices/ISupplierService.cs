using OAIM.Application.Data;
using OAIM.Application.DTO;
using OAIM.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAIM.Application.IServices
{
    public interface ISupplierService
    {
        Task<PagedResult<Supplier>> GetAllSuppliersAsync(int pageNumber, int pageSize);
        public Task<Supplier> GetSupplierByIdAsync(int id);
        public Task<Supplier> CreateSupplierAsync(SupplierDto supplier);
        public Task<Supplier> UpdateSupplier(int id, SupplierDto updateSupplierDto);
        public Task<bool> DeleteSupplierAsync(int id);

    }
}
