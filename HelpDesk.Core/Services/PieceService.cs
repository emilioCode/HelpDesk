using HelpDesk.Core.Entities;
using HelpDesk.Core.Exceptions;
using HelpDesk.Core.Interfaces;
using HelpDesk.Core.Security;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Core.Services
{
    public class PieceService : IPieceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUser _currentUser;
        public PieceService(IUnitOfWork unitOfWork, ICurrentUser currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }
        public IEnumerable<Piezas> GetPieces(int ticketId, int businessId)
        {
            _currentUser.EnsureTicketInCompany(_unitOfWork, ticketId);
            if (_currentUser.IsAuthenticated) businessId = _currentUser.CompanyId;
            var parts = _unitOfWork.PieceRepository.GetAll().Where(e => e.IdSolicitud == ticketId && e.IdEmpresa == businessId);
            return parts;
        }

        public async Task<Piezas> AddPiece(Piezas piece)
        {
            _currentUser.EnsureTicketInCompany(_unitOfWork, piece.IdSolicitud);
            if (_currentUser.IsAuthenticated) piece.IdEmpresa = _currentUser.CompanyId;
            piece.Habilitado = true;
            await _unitOfWork.PieceRepository.Add(piece);
            await _unitOfWork.SaveChangeAsync();
            return piece;
        }

        public async Task<Piezas> UpdatePiece(Piezas piece)
        {
            if (_currentUser.IsAuthenticated)
            {
                var persisted = _unitOfWork.PieceRepository.GetAll().FirstOrDefault(x => x.Id == piece.Id);
                if (persisted == null) throw new AccessDeniedException();
                _currentUser.RequireCompany(persisted.IdEmpresa);
                piece.IdEmpresa = persisted.IdEmpresa;
                piece.IdSolicitud = persisted.IdSolicitud;
            }
            piece.Habilitado = true;
            _unitOfWork.PieceRepository.Update(piece);
            await _unitOfWork.SaveChangeAsync();
            return piece;
        }

        public async Task<bool> DeletePiece(int pieceId)
        {
            if (_currentUser.IsAuthenticated)
            {
                var persisted = _unitOfWork.PieceRepository.GetAll().FirstOrDefault(x => x.Id == pieceId);
                if (persisted == null) throw new AccessDeniedException();
                _currentUser.RequireCompany(persisted.IdEmpresa);
            }
            await _unitOfWork.PieceRepository.Delete(pieceId);
            await _unitOfWork.SaveChangeAsync();
            return true;
        }
    }
}
