using Microsoft.VisualStudio.TestTools.UnitTesting;
using AP.Reports.AutoDocumets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using Microsoft.Office.Interop.Word;
using System = Microsoft.Office.Interop.Word.System;

namespace UnitTest.AP.Reports
{
    [TestClass]
    public class UtExcel
    {
        private Excel _excel;

        [TestMethod]
        public void TestMethodReplaceAll()
        {
            using (_excel = new Excel())
            {
                _excel.OpenDocument(@"C:\Users\02ias01\Documents\Tests\TestReplace.xlsx");
                _excel.FindStringAndAllReplace("Слово", "Воробей");
                _excel.FindStringAndAllReplaceImage("Картинка", new Bitmap(@"C:\Users\02ias01\Documents\Tests\Image.jpg"));
                _excel.SaveAs(@"C:\Users\02ias01\Documents\Tests\TestReplaceAllResult.xlsx");
            }
        }

        [TestMethod]
        public void TestMethodReplace()
        {
            using (_excel = new Excel())
            {
                _excel.OpenDocument(@"C:\Users\02ias01\Documents\Tests\TestReplace.xlsx");
                _excel.FindStringAndReplace("Слово", "Воробей");
                _excel.FindStringAndReplaceImage("Картинка", new Bitmap(@"C:\Users\02ias01\Documents\Tests\Image.jpg"));
                _excel.SaveAs(@"C:\Users\02ias01\Documents\Tests\TestReplaceResult.xlsx");
            }
        }

        [TestMethod]
        public void TestMethodReplaceByBookmark()
        {
            using (_excel = new  Excel())
            {
                _excel.OpenDocument(@"C:\Users\02ias01\Documents\Tests\TestReplace.xlsx");
                _excel.InsertImageToBookmark("диапазон_для_картинки", new Bitmap(@"C:\Users\02ias01\Documents\Tests\Image.jpg"));
                _excel.InsertTextToBookmark("диапазон_для_текста", "Текст, вставленный из программы");
                //_excel.FillsTableToBookmark("диапазон_для_текста", new DataTable());
                _excel.SaveAs(@"C:\Users\02ias01\Documents\Tests\TestReplaceByBookMarkResult.xlsx");
            }
        }
    }
}