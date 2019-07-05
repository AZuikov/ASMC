﻿using System;
using AP.Barcoder.Utils;
using AP.Barcoder;
using AP.Utils.Data;

namespace AP.Barcoder
{
    public class Base1DCode : IBarcode
    {
        private readonly BitList _bitList;

        internal Base1DCode(
            BitList bitList, BarcodeType kind, string content)
        {
            _bitList = bitList;
            Content = content;
            Bounds = new Bounds(_bitList.Length, 1);
            Metadata = new Metadata(kind.GetStringValue(), 1);
        }

        public string Content { get; }

        public Bounds Bounds { get; }

        public bool At(int x, int y)
        {
            if (y != 0) throw new ArgumentException("Should be 0 for 1D barcode", nameof(y));
            if (x < 0 || x >= _bitList.Length) throw new IndexOutOfRangeException($"Value of x out of range");
            return _bitList.GetBit(x);
        }

        public Metadata Metadata { get; }
    }
}
