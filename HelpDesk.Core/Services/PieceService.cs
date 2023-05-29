using HelpDesk.Core.DTOs;
using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.Services
{
    public class PieceService : IPieceService
    {
        private readonly IUnitOfWork _unitOfWork;
        public PieceService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IEnumerable<Piezas> GetPieces(int ticketId, int businessId)
        {
            var parts = _unitOfWork.PieceRepository.GetAll().Where(e => e.IdSolicitud == ticketId && e.IdEmpresa == businessId);
            return parts;
        }

        public async Task<Piezas> AddPiece(Piezas piece)
        {

            piece.Habilitado = true;
            await _unitOfWork.PieceRepository.Add(piece);
            await _unitOfWork.SaveChangeAsync();
            return piece;
        }

        public async Task<Piezas> UpdatePiece(Piezas piece)
        {
            piece.Habilitado = true;
            _unitOfWork.PieceRepository.Update(piece);
            await _unitOfWork.SaveChangeAsync();
            return piece;
        }

        public async Task<bool> DeletePiece(int pieceId)
        {
            await _unitOfWork.PieceRepository.Delete(pieceId);
            await _unitOfWork.SaveChangeAsync();
            return true;
        }
    }
}
