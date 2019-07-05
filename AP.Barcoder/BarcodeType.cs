using AP.Utils.Data;

namespace AP.Barcoder
{


    public enum BarcodeType
    {
        [StringValue("Aztec")] Aztec,
        [StringValue("Codabar")] Codabar,
        [StringValue("Code 128")] Code128,
        [StringValue("Code 39")] Code39,
        [StringValue("Code 93")] Code93,
        [StringValue("DataMatrix")] DataMatrix,
        [StringValue("EAN 8")] Ean8,
        [StringValue("EAN 13")] Ean13,
        [StringValue("PDF417")] Pdf417,
        [StringValue("QR Code")] Qr,
        [StringValue("2 of 5")] TwoOfFive,
        [StringValue("2 of 5(interleaved)")] TwoOfFiveInterleaved
    }
}