using HelpDesk.Core.Constants;
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
    public class DeviceService : IDeviceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUser _currentUser;
        public DeviceService(IUnitOfWork unitOfWork, ICurrentUser currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Equipo> AddDevice(Equipo device)
        {
            _currentUser.EnsureTicketInCompany(_unitOfWork, device.IdSolicitud);
            if (_currentUser.IsAuthenticated) device.IdEmpresa = _currentUser.CompanyId;
            device.Habilitado = true;
            await _unitOfWork.DeviceRepository.Add(device);
            await _unitOfWork.SaveChangeAsync();
            return device;
        }

        public async Task<List<Equipo>> AddRangeDevice(List<Equipo> devices)
        {
            if (devices.Where(e => e.Marca == "" || e.Marca == null ||
                e.Descripcion == "" || e.Descripcion == null || e.Modelo == "" || e.Modelo == null ||
                e.NoSerial == "" || e.NoSerial == null || e.FallaReportada == null || e.FallaReportada == "").Count() > 0)
            {
                throw new Exception(Messages.FieldsRequired);
            }
            devices.ForEach(x =>
            {
                _currentUser.EnsureTicketInCompany(_unitOfWork, x.IdSolicitud);
                if (_currentUser.IsAuthenticated) x.IdEmpresa = _currentUser.CompanyId;
            });
            devices.ForEach(async x =>
            {
                x.Habilitado = true;
                await _unitOfWork.DeviceRepository.Add(x);
            });
            if(devices.Count() > 0) await _unitOfWork.SaveChangeAsync();
            return devices;
        }

        public IEnumerable<Equipo> GetDevicesByTicketId(int ticketId)
        {
            _currentUser.EnsureTicketInCompany(_unitOfWork, ticketId);
            var devices = _unitOfWork.DeviceRepository.GetDevicesByTicketId(ticketId);
            return devices;
        }

        public async Task<bool> DeleteDevide(int deviceId)
        {
            RequireOwnCompany(_unitOfWork.DeviceRepository.GetAll().FirstOrDefault(x => x.Id == deviceId));
            await _unitOfWork.DeviceRepository.Delete(deviceId);
            await _unitOfWork.SaveChangeAsync();
            return true;
        }
        
        public async Task<Equipo> ModifyDevice(Equipo device)
        {
            if (device.Marca == "" || device.Marca == null ||
              device.Descripcion == "" || device.Descripcion == null || device.Modelo == "" || device.Modelo == null ||
              device.NoSerial == "" || device.NoSerial == null || device.FallaReportada == null || device.FallaReportada == "")
            {
                throw new Exception(Messages.FieldsRequired);
            }
            if (_currentUser.IsAuthenticated)
            {
                var persisted = _unitOfWork.DeviceRepository.GetAll().FirstOrDefault(x => x.Id == device.Id);
                RequireOwnCompany(persisted);
                device.IdEmpresa = persisted.IdEmpresa;
                device.IdSolicitud = persisted.IdSolicitud;
            }
            device.Habilitado = true;
            _unitOfWork.DeviceRepository.Update(device);
            await _unitOfWork.SaveChangeAsync();
            return device;
        }

        private void RequireOwnCompany(Equipo device)
        {
            if (!_currentUser.IsAuthenticated) return;
            if (device == null) throw new AccessDeniedException();
            _currentUser.RequireCompany(device.IdEmpresa);
        }
    }
}
