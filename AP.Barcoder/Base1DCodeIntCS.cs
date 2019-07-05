using AP.Barcoder.Utils;

namespace AP.Barcoder
{
    public class Base1DCodeIntCS : Base1DCode, IBarcodeIntCS
    {
        internal Base1DCodeIntCS(BitList bitList, BarcodeType kind, string content, int checksum)
            : base(bitList, kind, content)
        {
            Checksum = checksum;
        }

        public int Checksum { get; }
    }
}
