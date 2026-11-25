namespace HelpDesk.Infrastructure.Interfaces
{
    public interface IQrCodeService
    {
        // The QR code of a text, as an SVG that can be placed in a page.
        string ToSvg(string text);
    }
}
