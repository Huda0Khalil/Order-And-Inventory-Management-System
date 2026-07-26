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
    public interface ICustomerService
    {
        Task<Customer> GetCustomerById(int id);
        Task<PagedResult<Customer>> GetAllCustomersAsync(int pageNumber, int pageSize);
        public Task<List<Customer>> GetListCustomer();
        Task<Customer> CreateCustomerAsync(CustomerDto customerDto);
        Task<bool> DeleteCustomerAsync(int id);
        Task<Customer> UpdateCustomer(int id, CustomerDto customerDto);
    }
}
