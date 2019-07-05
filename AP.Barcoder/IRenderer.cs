using System.IO;

namespace AP.Barcoder.Renderers
{
    public interface IRenderer
    {
        void Render(IBarcode barcode, Stream outputStream);
    }
}
