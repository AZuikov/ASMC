using Microsoft.VisualStudio.TestTools.UnitTesting;
using AP.Reports.AutoDocumets;
using System.Drawing;

namespace UnitTest.AP.Reports
{
    [TestClass]
    public class UtExcel
    {
        private Excel _excel;

        [TestMethod]
        public void TestMethodCreateTable()
        {
            using (_excel = new Excel())
            {
                _excel.Test();
                _excel.SaveAs(@"C:\Users\02ias01\Documents\Tests\Test.xlsx");
            }
        }

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
                _excel = new Excel();
                _excel.OpenDocument(@"C:\Users\02ias01\Documents\Tests\TestReplace.xlsx");
                _excel.FindStringAndReplace("Слово", "Воробей");
                _excel.FindStringAndReplaceImage("Картинка", new Bitmap(@"C:\Users\02ias01\Documents\Tests\Image.jpg"));
                _excel.SaveAs(@"C:\Users\02ias01\Documents\Tests\TestReplaceResult.xlsx");
            }
        }

    }
}