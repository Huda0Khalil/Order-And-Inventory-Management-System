using Microsoft.EntityFrameworkCore;
using OAIM.Application.Data;
using OAIM.Application.DTO;
using OAIM.Application.IServices;
using OAIM.Domain.Entities;
using OAIM.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAIM.Application.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly IRepository<Supplier> _supplierRepository;
        public SupplierService(IRepository<Supplier> supplierRepository)
        {
            _supplierRepository = supplierRepository;
        }
        public async Task<Supplier> CreateSupplierAsync(SupplierDto createSupplierDto)
        {
            Supplier supplier = new Supplier
            {
                Name = createSupplierDto.Name,
                ContactEmail = createSupplierDto.ContactEmail,
                PhoneNumber = createSupplierDto.PhoneNumber,
                Address = createSupplierDto.Address,
                TenantId = createSupplierDto.TenantId

            };
            Supplier result =  await _supplierRepository.AddAsync(supplier);
            return result;

        }

        public async Task<bool> DeleteSupplierAsync(int id)
        {
            bool result = await _supplierRepository.Delete(id);
            return result;
        }

        public async Task<PagedResult<Supplier>> GetAllSuppliersAsync(int pageNumber, int pageSize)
        {
            pageSize = pageSize > 100 ? 100 : pageSize;
            var query = _supplierRepository
           .GetAll()
           .AsNoTracking();
            var totalCount = await query.CountAsync();
            var items = await query
                        .OrderBy(s => s.Name)
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();
            return new PagedResult<Supplier>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<Supplier> GetSupplierByIdAsync(int id)
        {
            Supplier supplier = await _supplierRepository.GetByIdAsync(id);
            return supplier;
        }

        public Task<Supplier> UpdateSupplier(int id, SupplierDto updateSupplierDto)
        {
            Supplier supplier = new Supplier
            {
                Id = id,
                Name = updateSupplierDto.Name,
                ContactEmail = updateSupplierDto.ContactEmail,
                PhoneNumber = updateSupplierDto.PhoneNumber,
                Address = updateSupplierDto.Address,
                TenantId = updateSupplierDto.TenantId
            };
            var supplierUpdated = _supplierRepository.Update(supplier);
            return supplierUpdated;
        }
    }
}
