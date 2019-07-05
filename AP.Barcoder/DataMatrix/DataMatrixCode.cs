using AP.Barcoder.Utils;
using AP.Utils.Data;

namespace AP.Barcoder.DataMatrix
{
    public sealed class DataMatrixCode : IBarcode
    {
        private readonly CodeSize _size;
        private readonly BitList _data;

        internal DataMatrixCode(CodeSize size)
        {
            _size = size;
            Bounds = new Bounds(size.Columns, size.Rows);
            Metadata = new Metadata(BarcodeType.DataMatrix.GetStringValue(), 2);
            _data = new BitList(size.Rows * size.Columns);
        }

        internal void Set(int x, int y, bool value)
            => _data.SetBit(x * _size.Rows + y, value);

        internal bool Get(int x, int y)
            => _data.GetBit(x * _size.Rows + y);

        public string Content { get; internal set; }

        public Bounds Bounds { get; }

        public int MarginXLeft => 5;
        public int MarginXRight => 5;
        public int MarginYTop => 5;
        public int MarginYBottom => 5;

        public Metadata Metadata { get; }

        public bool At(int x, int y) => Get(x, y);
    }
}
