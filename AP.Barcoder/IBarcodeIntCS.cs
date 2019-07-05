namespace AP.Barcoder
{
    public interface IBarcodeIntCS : IBarcode
    {
        int Checksum { get; }
    }
}
