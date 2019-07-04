using Barcoder.Utils;

namespace Barcoder
{
    public class Base1DCodeIntCS : Base1DCode, IBarcodeIntCS
    {
        internal Base1DCodeIntCS(BitList bitList, string kind, string content, int checksum)
            : base(bitList, kind, content)
        {
            Checksum = checksum;
        }

        public int Checksum { get; }
    }
}
