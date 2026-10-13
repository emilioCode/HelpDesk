using HelpDesk.Core.Constants;
using HelpDesk.Core.CustomEntities;
using HelpDesk.Core.Entities;
using HelpDesk.Core.Exceptions;
using HelpDesk.Core.Interfaces;
using HelpDesk.Core.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUser _currentUser;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ILoginAttemptTracker _attempts;

        public UserService(IUnitOfWork unitOfWork, ICurrentUser currentUser, IPasswordHasher passwordHasher, ILoginAttemptTracker attempts)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _passwordHasher = passwordHasher;
            _attempts = attempts;
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
            _currentUser.RequireLevel(RoleLevel.Admin);
            if (_currentUser.IsAuthenticated)
            {
                // Only ROOT creates users for other companies, and nobody grants a role above their own.
                if (RoleLevel.Of(_currentUser.Role) < RoleLevel.Root)
                {
                    user.IdEmpresa = _currentUser.CompanyId;
                }
                EnsureCanGrant(user.Acceso);
            }

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
                    throw new Exception(Messages.AccountExists(user.CuentaUsuario));
                }
            });

            user.Contrasena = _passwordHasher.Hash(user.Contrasena);
            user.Habilitado = true;
            await _unitOfWork.UserRepository.Add(user);
            await _unitOfWork.SaveChangeAsync();

            return user;
        }

        public async Task<bool> UpdateUSer(Usuario user, int userCreatorId)
        {
            var actorId = _currentUser.ResolveUserId(userCreatorId);
            var userCreator = await _unitOfWork.UserRepository.GetById(actorId);
            var usuario = await _unitOfWork.UserRepository.GetById(user.Id);
            if (usuario is null)
            {
                throw new Exception("the user is required");
            }

            var isRoot = userCreator.Acceso == Roles.Root;
            var isAdmin = RoleLevel.Of(userCreator.Acceso) >= RoleLevel.Admin;
            var editsItself = usuario.Id == userCreator.Id;
            if (_currentUser.IsAuthenticated && !editsItself)
            {
                // Managing other users: administrators in their company, never above their own role.
                _currentUser.RequireLevel(RoleLevel.Admin);
                if (!isRoot)
                {
                    _currentUser.RequireCompany(usuario.IdEmpresa);
                    if (RoleLevel.Of(usuario.Acceso) > RoleLevel.Of(userCreator.Acceso))
                    {
                        throw new AccessDeniedException();
                    }
                }
            }

            var userAccounts = _unitOfWork.UserRepository.GetByBusinessId(usuario.IdEmpresa);
            var userAccountRepeated = userAccounts.Where(e => e.CuentaUsuario == user.CuentaUsuario).Count() > 1;
            if (userAccountRepeated)
            {
                throw new Exception(Messages.AccountExists(user.CuentaUsuario));
            }

            usuario.Nombre = user.Nombre == "" ? null : user.Nombre;
            usuario.NumDocumento = user.NumDocumento == "" ? null : user.NumDocumento;
            usuario.Correo = user.Correo == "" ? null : user.Correo;
            usuario.Image = user.Image;
            if (!string.IsNullOrEmpty(user.Contrasena))
            {
                usuario.Contrasena = _passwordHasher.Hash(user.Contrasena);
            }

            // Account, role, company and status are administrative: a user editing their own profile cannot change them.
            var canManage = !_currentUser.IsAuthenticated || (isAdmin && !editsItself) || isRoot;
            if (canManage)
            {
                if (isRoot && user.CuentaUsuario != null && user.CuentaUsuario != "") usuario.CuentaUsuario = user.CuentaUsuario;
                if (_currentUser.IsAuthenticated) EnsureCanGrant(user.Acceso);
                usuario.Acceso = user.Acceso == "" ? null : user.Acceso;
                if (!_currentUser.IsAuthenticated || isRoot) usuario.IdEmpresa = user.IdEmpresa;
                usuario.Habilitado = user.Habilitado;
            }

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
            var user = await GetUserById(_currentUser.ResolveUserId(id));
            condition = condition.ToLower();
            var users = _unitOfWork.UserRepository.GetByBusinessId(user.IdEmpresa).Where(x => x.Acceso != Roles.Root);
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
                    case Roles.Root:
                        var list = _unitOfWork.UserRepository.GetAll();
                        result.AddRange(list);
                        break;
                    case Roles.Moderator:
                    case Roles.Admin:
                        result.AddRange(users);
                        break;
                    case Roles.Technician:
                    default:
                        result = null;
                        break;
                }
            }
            return result.AsEnumerable();
        }

        public async Task<UserIdentity> LoginAsync(string account, string password)
        {
            if (_attempts.IsLocked(account))
            {
                throw new TooManyAttemptsException();
            }

            var businesses = _unitOfWork.BusinessRepository.GetAll().ToList();
            var candidates = _unitOfWork.UserRepository.GetAll()
                .Where(u => u.CuentaUsuario == account && u.Habilitado == true)
                .ToList();

            foreach (var usuario in candidates)
            {
                var empresa = businesses.FirstOrDefault(b => b.Id == usuario.IdEmpresa);
                if (empresa == null || !(empresa.Habilitado == true || usuario.Acceso == Roles.Root))
                {
                    continue;
                }
                if (!_passwordHasher.Verify(password, usuario.Contrasena))
                {
                    continue;
                }

                if (!_passwordHasher.IsHashed(usuario.Contrasena))
                {
                    // Legacy plain-text password: store its hash now that the user has proven they know it.
                    var tracked = await _unitOfWork.UserRepository.GetById(usuario.Id);
                    tracked.Contrasena = _passwordHasher.Hash(password);
                    _unitOfWork.UserRepository.Update(tracked);
                    await _unitOfWork.SaveChangeAsync();
                }

                _attempts.Reset(account);
                return new UserIdentity
                {
                    Id = usuario.Id,
                    Nombre = usuario.Nombre,
                    Acceso = usuario.Acceso,
                    IdEmpresa = usuario.IdEmpresa,
                    Empresa = empresa.RazonSocial,
                    UserName = usuario.CuentaUsuario,
                    Image = usuario.Image,
                    Logo = empresa.Image
                };
            }

            _attempts.RecordFailure(account);
            return null;
        }

        public async Task<bool> ValidateUserAccount(int IdEmpresa, string CuentaUsuario)
        {
            _currentUser.RequireLevel(RoleLevel.Admin);
            if (_currentUser.IsAuthenticated && RoleLevel.Of(_currentUser.Role) < RoleLevel.Root)
            {
                IdEmpresa = _currentUser.CompanyId;
            }
            var users = _unitOfWork.UserRepository.GetByBusinessId(IdEmpresa);
            var response = users.Where(uac => uac.CuentaUsuario == CuentaUsuario).Count() == 0;
            return response;
        }

        private void EnsureCanGrant(string role)
        {
            if (RoleLevel.Of(role) > RoleLevel.Of(_currentUser.Role))
            {
                throw new AccessDeniedException();
            }
        }
    }
}
