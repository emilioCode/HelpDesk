using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Core.Services
{
    public class DeviceService : IDeviceService
    {
        private readonly IUnitOfWork _unitOfWork;
        public DeviceService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Equipo> AddDevice(Equipo device)
        {
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
                throw new Exception("Field(s) required!");
            }
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
            var devices = _unitOfWork.DeviceRepository.GetDevicesByTicketId(ticketId);
            return devices;
        }

        public async Task<bool> DeleteDevide(int deviceId)
        {
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
                throw new Exception("Field(s) required!");
            }
            device.Habilitado = true;
            _unitOfWork.DeviceRepository.Update(device);
            await _unitOfWork.SaveChangeAsync();
            return device;
        }
    }
}
