using OAIM.Application.Data;
using OAIM.Application.DTO;
using OAIM.Domain.Interfaces;

namespace OAIM.Application.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly IRepository<Supplier, int> _supplierRepository;
        private IUnitOfWork _unitOfWork;

        public SupplierService(IRepository<Supplier, int> supplierRepository, IUnitOfWork unitOfWork)
        {
            _supplierRepository = supplierRepository;
            _unitOfWork = unitOfWork;
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
            Supplier result = await _supplierRepository.AddAsync(supplier);
            await _unitOfWork.SaveChangesAsync();
            return result;
        }


        public async Task<bool> DeleteSupplierAsync(int id)
        {
            bool result = await _supplierRepository.Delete(id);
            await _unitOfWork.SaveChangesAsync();
            return result;
        }

        public async Task<PagedResult<Supplier>> GetAllSuppliersAsync(int pageNumber, int pageSize)
        {
            pageSize = pageSize > 100 ? 100 : pageSize;
            var query = _supplierRepository
           .GetAll(null)
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

        public Task<List<Supplier>> GetListSupplier()
        {
            return _supplierRepository
                .GetAll(null)
                .AsNoTracking()
                .OrderBy(s => s.Name)
                .Select(s => new Supplier { Id = s.Id, Name = s.Name })
                .ToListAsync();
        }

        public async Task<Supplier> GetSupplierByIdAsync(int id)
        {
            Supplier supplier = await _supplierRepository.GetByIdAsync(id);
            return supplier;
        }

        public async Task<Supplier> UpdateSupplier(int id, SupplierDto updateSupplierDto)
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
            await _supplierRepository.Update(supplier);
            await _unitOfWork.SaveChangesAsync();
            return supplier;
        }

    }
}
