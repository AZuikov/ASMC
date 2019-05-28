using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Data;
using DataTable = System.Data.DataTable;
using AP.Reports.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AP.Reports.AutoDocumets;

namespace UnitTest.AP.Reports
{
    [TestClass]
    public class UtOpenExcel
    {
        private OpenExcel _excel;
        private string _pathToTestFolder = @"C:\Users\02ias01\Documents\Tests\OpenExcel";
        private string[] _pages = new[] { "Bookmark", "Insert", "Replase" };



    }
}