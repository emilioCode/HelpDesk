using HelpDesk.Infrastructure.Interfaces;
using QRCoder;

namespace HelpDesk.Infrastructure.Services
{
    public class QrCodeService : IQrCodeService
    {
        public string ToSvg(string text)
        {
            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(text, QRCodeGenerator.ECCLevel.M);
            var svg = new SvgQRCode(data);
            return svg.GetGraphic(4);
        }
    }
}
