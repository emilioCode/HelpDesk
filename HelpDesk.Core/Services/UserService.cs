using HelpDesk.Core.CustomEntitties;
using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IEnumerable<Usuario> GetUsers()
        {
            return _unitOfWork.UserRepository.GetAll();
        }

        public async Task<Usuario> GetUserById(int id)
        {
            return await _unitOfWork.UserRepository.GetById(id);
        }

        public async Task<Usuario> InsertUSer(Usuario user)
        {
            var userAccounts = _unitOfWork.UserRepository.GetByBusinessId(user.IdEmpresa).ToList();
            int quantity = userAccounts.Count();
            var business = await _unitOfWork.BusinessRepository.GetById(user.IdEmpresa);
            var limit = business.Limit ?? 1;

            if (quantity >= limit)
            {
                throw new Exception($"It isn't available to add users. Your limit is {limit}");
            }

            userAccounts.ForEach(e =>
            {
                if (e.CuentaUsuario.Equals(user.CuentaUsuario))
                {
                    throw new Exception($"the userAccount <b>{user.CuentaUsuario}</b> already exists");
                }
            });

            user.Habilitado = true;
            await _unitOfWork.UserRepository.Add(user);
            await _unitOfWork.SaveChangeAsync();

            return user;
        }

        public async Task<bool> UpdateUSer(Usuario user, int userCreatorId)
        {
            var userAccounts = _unitOfWork.UserRepository.GetByBusinessId(user.IdEmpresa);

            var userAccountRepeated = userAccounts.Where(e => e.CuentaUsuario == user.CuentaUsuario).Count() > 1;

            if (userAccountRepeated)
            {
                throw new Exception($"the userAccount <b>{user.CuentaUsuario}</b> already exists");
            }

            var userCreator = await _unitOfWork.UserRepository.GetById(userCreatorId);
            var usuario = await _unitOfWork.UserRepository.GetById(user.Id);
            usuario.Nombre = user.Nombre == "" ? null : user.Nombre;
            usuario.NumDocumento = user.NumDocumento == "" ? null : user.NumDocumento;
            if (userCreator.Acceso == "ROOT" && user.CuentaUsuario != null && user.CuentaUsuario != "") usuario.CuentaUsuario = user.CuentaUsuario;
            usuario.Contrasena = user.Contrasena == "" ? null : user.Contrasena;
            usuario.Acceso = user.Acceso == "" ? null : user.Acceso;
            usuario.Correo = user.Correo == "" ? null : user.Correo;
            usuario.IdEmpresa = user.IdEmpresa;
            usuario.Image = user.Image;
            usuario.Habilitado = user.Habilitado;
            _unitOfWork.UserRepository.Update(usuario);
            await _unitOfWork.SaveChangeAsync();
            return true;
        }

        public async Task<bool> Delete(int id)
        {
            await _unitOfWork.UserRepository.Delete(id);
            await _unitOfWork.SaveChangeAsync();
            return true;
        }

        public async Task<IEnumerable<Usuario>> GetUsersByIdAndCondition(int id, string condition)
        {
            List <Usuario> result = new List<Usuario>();
            var user = await GetUserById(id);
            condition = condition.ToLower();
            var users = _unitOfWork.UserRepository.GetByBusinessId(user.IdEmpresa).Where(x => x.Acceso != "ROOT");
            if (condition == "unique")
            {
                result.Add(user);
                return result;
            }
            
            if (condition == "just name")
            {
                var list = users.Select(e => new Usuario { Id = e.Id, Nombre = e.Nombre });
                result.AddRange(list);
            }
            else // '*' or 'all'
            {
                switch (user.Acceso)
                {
                    case "ROOT":
                        var list = _unitOfWork.UserRepository.GetAll();
                        result.AddRange(list);
                        break;
                    case "MODERADOR":
                    case "ADMINISTRADOR":
                        result.AddRange(users);
                        break;
                    case "TECNICO":
                    default:
                        result = null;
                        break;
                }
            }
            return result.AsEnumerable();
        }

        public UserIdentity GetLoginByCredentials(Usuario login)
        {
            var users = _unitOfWork.UserRepository.GetAll().ToList();
            var businesses = _unitOfWork.BusinessRepository.GetAll().ToList();

            var userCredentials = from usuario in users
                       join empresa in businesses on usuario.IdEmpresa equals empresa.Id
                       where usuario.CuentaUsuario == login.CuentaUsuario
                       && usuario.Contrasena == login.Contrasena
                       && empresa.Id == usuario.IdEmpresa && usuario.Habilitado == true && (empresa.Habilitado == true || usuario.Acceso == "ROOT")
                       select new UserIdentity
                       {
                           Id = usuario.Id,
                           Nombre = usuario.Nombre,
                           Acceso = usuario.Acceso,
                           IdEmpresa  = usuario.IdEmpresa,
                           Empresa = empresa.RazonSocial,
                           UserName = usuario.CuentaUsuario,
                           Image = usuario.Image,
                           Logo = empresa.Image
                       };
            var result = userCredentials.Where(x => x.UserName.Equals(login.CuentaUsuario)).FirstOrDefault();
            return result;
        }

        public async Task<bool> ValidateUserAccount(int IdEmpresa, string CuentaUsuario)
        {
            var users = _unitOfWork.UserRepository.GetByBusinessId(IdEmpresa);
            var response = users.Where(uac => uac.CuentaUsuario == CuentaUsuario).Count() == 0;
            return response;

        }
    }
}
