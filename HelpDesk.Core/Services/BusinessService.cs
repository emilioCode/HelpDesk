using HelpDesk.Core.Constants;
using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using HelpDesk.Core.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Core.Services
{
    public class BusinessService : IBusinessService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUser _currentUser;
        private readonly ISecretProtector _secretProtector;
        public BusinessService(IUnitOfWork unitOfWork, ICurrentUser currentUser, ISecretProtector secretProtector)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _secretProtector = secretProtector;
        }
        public async Task<IEnumerable<Empresa>> GetBusinessesByUserAccess(int userId)
        {
            var user = await _unitOfWork.UserRepository.GetById(_currentUser.ResolveUserId(userId));
            var businesses = new List<Empresa>();
            if (user.Acceso == Roles.Root)
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
            var user = await _unitOfWork.UserRepository.GetById(_currentUser.ResolveUserId(userId));
            if(user.Acceso != Roles.Root)
            {
                throw new Exception("this account isn't available to create a new business");
            }
            business.Contrasena = _secretProtector.Protect(business.Contrasena);
            business.Habilitado = true;
            await _unitOfWork.BusinessRepository.Add(business);
            await _unitOfWork.SaveChangeAsync();
            return business;
        }

        public async Task<bool> UpdateBusiness(int userId, Empresa business)
        {
            userId = _currentUser.ResolveUserId(userId);
            if(userId <= 0)
            {
                throw new Exception("An userId valid is required to do the operation");
            }
            var user = await _unitOfWork.UserRepository.GetById(userId);
            if (user.Acceso != Roles.Root)
            {
                if (user.Acceso == Roles.Admin)
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

            var existing = _unitOfWork.BusinessRepository.GetAll().FirstOrDefault(x => x.Id == business.Id);
            if (existing != null)
            {
                // The mail password is never returned to the client: an empty value keeps the stored one.
                business.Contrasena = string.IsNullOrEmpty(business.Contrasena)
                    ? existing.Contrasena
                    : _secretProtector.Protect(business.Contrasena);
                if (user.Acceso != Roles.Root)
                {
                    // The user limit and the enabled flag are only managed by ROOT.
                    business.Limit = existing.Limit;
                    business.Habilitado = existing.Habilitado;
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
