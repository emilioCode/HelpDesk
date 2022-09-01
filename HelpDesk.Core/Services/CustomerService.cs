using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Core.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        public CustomerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Cliente> CreateCustomer(Cliente customer)
        {
            customer.Habilitado = true;
            await _unitOfWork.CustomerRepository.Add(customer);
            await _unitOfWork.SaveChangeAsync();
            return customer;
        }

        public async Task<IEnumerable<Cliente>> GetCustomersByIdAndCondition(int id, string option)
        {
            if(option is null)
            {
                throw new Exception("the option is required");
            }
            option = option.ToLower();
            var customers = new List<Cliente>();

            if (option == "unique")
            {
                var customer = await _unitOfWork.CustomerRepository.GetById(id);
                customers.Add(customer);
            }
            else if (option == "*" || option == "all")
            {
                var user = await _unitOfWork.UserRepository.GetById(id);
                var statusClient = false;
                switch (user.Acceso)
                {
                    case "ROOT":
                    case "ADMINISTRADOR":
                        break;
                    case "MODERADOR":
                    case "TECNICO":
                    default:
                        statusClient = true;
                        break;
                }
                var rangeOfClients = _unitOfWork.CustomerRepository.GetByBusinessId(user.IdEmpresa, statusClient);
                customers.AddRange(rangeOfClients);
                
            }
            return customers.OrderByDescending(x => x.Id);

        }

        public async Task<bool> UpdateCustomer(Cliente customer, int userId)
        {
            var user = await _unitOfWork.UserRepository.GetById(userId);

            if (user is null)
            {
                throw new Exception($"the user is required");
            }

            customer.IdEmpresa = user.IdEmpresa;

            _unitOfWork.CustomerRepository.Update(customer);
            await _unitOfWork.SaveChangeAsync();
            return true;
        }
    }
}
