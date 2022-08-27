using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public async Task InsertUSer(Usuario user)
        {
            //await _unitOfWork.UserRepository.Add(user);
            //await _unitOfWork.SaveChangeAsync();
            #region Legacy Code
            //ObjectResponse res;
            //try
            //{
            //    if (req.Nombre == null || req.Nombre == "" ||
            //        req.CuentaUsuario == null || req.CuentaUsuario == "" ||
            //        req.Contrasena == null || req.Contrasena == "" ||
            //        req.IdEmpresa == null || req.IdEmpresa <= 0)
            //    {
            //        res = new ObjectResponse
            //        {
            //            code = "2",
            //            title = "Validation errors",
            //            icon = "warning",
            //            message = "missing some field to complete",
            //            data = null
            //        };
            //        return new JsonResult(res);
            //    }
            //    int quantity = context.Usuarios.Where(e => e.IdEmpresa == req.IdEmpresa).Count();
            //    var limit = context.Empresas.Where(b => b.Id == req.IdEmpresa).Select(b => b.Limit).SingleOrDefault();
            //    limit = limit == null ? 1 : limit;
            //    if (quantity >= limit)
            //    {
            //        res = new ObjectResponse
            //        {
            //            code = "2",
            //            title = "No fue posible agregar usuario",
            //            icon = "warning",
            //            message = $"Las cuentas de usuario que puedes tener como máximo registrados en la plataforma es de {limit}.\nPara reclamaciones favor dirigirse a Términos y condiciones.",
            //            data = null
            //        };
            //        return new JsonResult(res);
            //    }

            //    List<Usuario> userAccounts = context.Usuarios.Where(uac => uac.IdEmpresa == req.IdEmpresa && uac.Id != req.Id).ToList();

            //    for (int i = 0; i < userAccounts.Count; i++)
            //    {
            //        if (userAccounts[i].CuentaUsuario.Equals(req.CuentaUsuario))
            //        {
            //            res = new ObjectResponse
            //            {
            //                code = "2",
            //                title = "Validation errors",
            //                icon = "warning",
            //                message = $"the userAccount <b>{req.CuentaUsuario}</b> already exists",
            //                data = req
            //            };
            //            return new JsonResult(res);
            //        }
            //    }
            //    req.Habilitado = true;
            //    context.Entry(req).State = Microsoft.EntityFrameworkCore.EntityState.Added;
            //    context.SaveChanges();

            //    var data = context.Usuarios.Where(e => e.Nombre == req.Nombre
            //    && e.NumDocumento == req.NumDocumento && e.CuentaUsuario == req.CuentaUsuario && e.Acceso == req.Acceso
            //    && e.Correo == req.Correo && e.Contrasena == req.Contrasena).SingleOrDefault();
            //    res = new ObjectResponse
            //    {
            //        code = "1",
            //        title = "Saved",
            //        icon = "success",
            //        message = "has been saved successfully",
            //        data = data
            //    };

            //}
            //catch (Exception e)
            //{
            //    res = new ObjectResponse
            //    {
            //        code = "0",
            //        title = "Error",
            //        icon = "error",
            //        message = e.Message,
            //        data = null
            //    };
            //}

            //return new JsonResult(res);
            #endregion Legacy Code
        }

        public async Task<bool> UpdateUSer(Usuario user)
        {
            _unitOfWork.UserRepository.Update(user);
           await _unitOfWork.SaveChangeAsync();
            return true;
        }

        public async Task<bool> Delete(int id)
        {
            await _unitOfWork.UserRepository.Delete(id);
            await _unitOfWork.SaveChangeAsync();
            return true;
        }

        public async Task<List<Usuario>> GetUsersByIdAndCondition(int id, string condition)
        {
            List < Usuario > result = new List<Usuario>();
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
            return result;
        }
    }
}
