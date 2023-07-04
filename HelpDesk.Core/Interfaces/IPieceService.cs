using HelpDesk.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HelpDesk.Core.Interfaces
{
    public interface IPieceService
    {
        IEnumerable<Piezas> GetPieces(int ticketId, int businessId);
        Task<Piezas> AddPiece(Piezas piece);
        Task<Piezas> UpdatePiece(Piezas piece);
        Task<bool> DeletePiece(int pieceId);
    }
}
