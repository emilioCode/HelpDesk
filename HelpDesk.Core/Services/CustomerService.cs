using HelpDesk.Core.Entities;
using HelpDesk.Core.Exceptions;
using HelpDesk.Core.Interfaces;
using HelpDesk.Core.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HelpDesk.Core.Constants;

namespace HelpDesk.Core.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUser _currentUser;
        public CustomerService(IUnitOfWork unitOfWork, ICurrentUser currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Cliente> CreateCustomer(Cliente customer)
        {
            if (_currentUser.IsAuthenticated) customer.IdEmpresa = _currentUser.CompanyId;
            customer.Habilitado = true;
            await _unitOfWork.CustomerRepository.Add(customer);
            await _unitOfWork.SaveChangeAsync();
            return customer;
        }

        public async Task<IEnumerable<Cliente>> GetCustomersByIdAndCondition(int id, string option)
        {
            if(option is null)
            {
                throw new Exception(Messages.OptionRequired);
            }
            option = option.ToLower();
            var customers = new List<Cliente>();

            if (option == "unique")
            {
                var customer = await _unitOfWork.CustomerRepository.GetById(id);
                if (customer != null) _currentUser.RequireCompany(customer.IdEmpresa);
                customers.Add(customer);
            }
            else if (option == "*" || option == "all")
            {
                var user = await _unitOfWork.UserRepository.GetById(_currentUser.ResolveUserId(id));
                var statusClient = false;
                switch (user.Acceso)
                {
                    case Roles.Root:
                    case Roles.Admin:
                        break;
                    case Roles.Moderator:
                    case Roles.Technician:
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
            var user = await _unitOfWork.UserRepository.GetById(_currentUser.ResolveUserId(userId));

            if (user is null)
            {
                throw new Exception($"the user is required");
            }

            if (_currentUser.IsAuthenticated)
            {
                var existing = _unitOfWork.CustomerRepository.GetAll().FirstOrDefault(x => x.Id == customer.Id);
                if (existing == null) throw new AccessDeniedException();
                _currentUser.RequireCompany(existing.IdEmpresa);
                // Enabling or disabling a customer is an administrator action.
                if (existing.Habilitado != customer.Habilitado) _currentUser.RequireLevel(RoleLevel.Admin);
            }

            customer.IdEmpresa = user.IdEmpresa;

            _unitOfWork.CustomerRepository.Update(customer);
            await _unitOfWork.SaveChangeAsync();
            return true;
        }
    }
}
