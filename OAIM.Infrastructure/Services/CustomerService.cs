
using OAIM.Application.Data;
using OAIM.Application.DTO;
using OAIM.Domain.Interfaces;

namespace OAIM.Application.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IRepository<Customer, int> _customerRepository;
        private IUnitOfWork _unitOfWork;

        public CustomerService(IRepository<Customer, int> customerRepository, IUnitOfWork unitOfWork)
        {
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Customer> CreateCustomerAsync(CustomerDto customerDto)
        {
            Customer customer = new Customer
            {
                FirstName = customerDto.FirstName,
                LastName = customerDto.LastName,
                Email = customerDto.Email,
                PhoneNumber = customerDto.PhoneNumber,
                Address = customerDto.Address,
                CustomerType = customerDto.CustomerType
            };
            Customer createdCustomer = await _customerRepository.AddAsync(customer);
            await _unitOfWork.SaveChangesAsync();
            return customer;
        }


        public async Task<bool> DeleteCustomerAsync(int id)
        {
            bool result = await _customerRepository.Delete(id);
            await _unitOfWork.SaveChangesAsync();
            return result;
        }

        public async Task<PagedResult<Customer>> GetAllCustomersAsync(int pageNumber, int pageSize)
        {
            pageSize = pageSize > 100 ? 100 : pageSize;
            var query = _customerRepository
           .GetAll()
           .AsNoTracking();
            var totalCount = await query.CountAsync();
            var items = await query
                        .OrderBy(s => s.Id)
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();
            return new PagedResult<Customer>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<Customer> GetCustomerById(int id)
        {
            Customer customer = await _customerRepository.GetByIdAsync(id);
            return customer;
        }

        public async Task<Customer> UpdateCustomer(int id, CustomerDto customerDto)
        {
            Customer customer = new Customer
            {
                Id = id,
                FirstName = customerDto.FirstName,
                LastName = customerDto.LastName,
                Email = customerDto.Email,
                PhoneNumber = customerDto.PhoneNumber,
                Address = customerDto.Address,
                CustomerType = customerDto.CustomerType
            };
            _customerRepository.Update(customer);
            await _unitOfWork.SaveChangesAsync();
            return customer;
        }

       
        Task<PagedResult<Customer>> ICustomerService.GetAllCustomersAsync(int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }
    }
}
