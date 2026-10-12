using AutoMapper;
using HelpDesk.Core.DTOs;
using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using HelpDesk.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using HelpDesk.Core.Constants;

namespace HelpDesk.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        private readonly IDeviceService _deviceService;
        private readonly IMapper _mapper;
        public DeviceController(IDeviceService deviceService, IMapper mapper)
        {
            _deviceService = deviceService;
            _mapper = mapper;
        }
        // GET: api/Device/1
        [HttpGet("{ticketId}")]
        public IActionResult Get(int ticketId)
        {
            var devices = _deviceService.GetDevicesByTicketId(ticketId);
            var devicesDto = _mapper.Map<IEnumerable<EquipoDto>>(devices);
            return Ok(devicesDto);
        }

        // POST: api/Device
        [HttpPost("[action]")]
        public async Task<IActionResult> PostOne([FromBody] EquipoDto deviceDto)
        {
            var device = _mapper.Map<Equipo>(deviceDto);
            device = await _deviceService.AddDevice(device);
            deviceDto = _mapper.Map<EquipoDto>(device);
            var response = ObjectResponse.Saved(Messages.Created, deviceDto);
            return Ok(response);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> PostArray([FromBody] List<EquipoDto> devicesDto)
        {
            var devices = _mapper.Map<List<Equipo>>(devicesDto);
            devices = await _deviceService.AddRangeDevice(devices);
            var deviceDtos = _mapper.Map<IEnumerable<EquipoDto>>(devices);
            var response = ObjectResponse.Saved(Messages.Created, deviceDtos);
            return Ok(response);
        }

        // PUT: api/Device
        [HttpPost("[action]")]
        public async Task<IActionResult> Put([FromBody] EquipoDto deviceDto)
        {
            var device = _mapper.Map<Equipo>(deviceDto);
            device = await _deviceService.ModifyDevice(device);
            deviceDto = _mapper.Map<EquipoDto>(device);
            var response = ObjectResponse.Saved(Messages.Modified, deviceDto);
            return Ok(response);
        }

        // DELETE: api/Delete/5
        [HttpPost("[action]/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _deviceService.DeleteDevide(id);
            var response = ObjectResponse.Saved(Messages.Modified, result);
            return Ok(response);
        }
    }
}
