
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
           .GetAll(null)
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
        
        public async Task<List<Customer>> GetListCustomer()
        {
            return await _customerRepository
                .GetAll(null)
                .AsNoTracking()
                .OrderBy(c => c.FirstName)
                .Select(c => new Customer
                {
                    Id = c.Id,
                    FirstName = c.FirstName, LastName = c.LastName
                })
                .ToListAsync();
        }

        public async Task<Customer> UpdateCustomer(int id, CustomerDto customerDto)
        {
            Customer customer = await _customerRepository.GetByIdAsync(id);
            customer.FirstName = customerDto.FirstName;
            customer.LastName = customerDto.LastName;
            customer.Email = customerDto.Email;
            customer.Address = customerDto.Address;
            customer.PhoneNumber = customerDto.PhoneNumber;
            await _unitOfWork.SaveChangesAsync();
            return customer;
        }

       
     }
}
