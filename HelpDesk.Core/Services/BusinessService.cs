using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Core.Services
{
    public class BusinessService : IBusinessService
    {
        private readonly IUnitOfWork _unitOfWork;
        public BusinessService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IEnumerable<Empresa>> GetBusinessesByUserAccess(int userId)
        {
            var user = await _unitOfWork.UserRepository.GetById(userId);
            var businesses = new List<Empresa>();
            if (user.Acceso == "ROOT")
            {
                var result = _unitOfWork.BusinessRepository.GetAll().OrderByDescending(x => x.Id);
                businesses.AddRange(result);
            }
            else
            {
                var businesss = await _unitOfWork.BusinessRepository.GetById(user.IdEmpresa);
                businesses.Add(businesss);
            }
            return businesses.AsEnumerable();
        }

        public async Task<Empresa> CreateBusinness(int userId, Empresa business)
        {
            var user = await _unitOfWork.UserRepository.GetById(userId);
            if(user.Acceso != "ROOT")
            {
                throw new Exception("this account isn't available to create a new business");
            }
            business.Habilitado = true;
            await _unitOfWork.BusinessRepository.Add(business);
            await _unitOfWork.SaveChangeAsync();
            return business;
        }

        public async Task<bool> UpdateBusiness(int userId, Empresa business)
        {
            if(userId <= 0)
            {
                throw new Exception("An userId valid is required to do the operation");
            }
            var user = await _unitOfWork.UserRepository.GetById(userId);
            if (user.Acceso != "ROOT")
            {
                if (user.Acceso == "ADMINISTRADOR")
                {
                    if(business.Id != user.IdEmpresa)
                    {
                        throw new Exception("User doesn't see its scheme");
                    }
                }
                else
                {
                    throw new Exception("this account isn't available to modify this business");
                }
            }
            _unitOfWork.BusinessRepository.Update(business);
            await _unitOfWork.SaveChangeAsync();

            return true;
        }

        public async Task<Empresa> GetById(int businessId)
        {
            return await _unitOfWork.BusinessRepository.GetById(businessId);
        }
    }
}
