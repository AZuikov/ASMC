using System.Runtime.InteropServices;
//using AP.Reports.MSInterop;
using Microsoft.Office.Interop.Excel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace UnitTest.AP.Reports
{
    //[TestClass]
    //public class UtExcelInterop
    //{
    //    private ExcelInterop excelApp;
    //    /// <summary>
    //    /// Проверяет что создается экземпляр приложения, в случаи его отсутствия
    //    /// </summary>
    //    [TestMethod]
    //    public void ApplicationNotNull()
    //    {
    //        using (excelApp = new ExcelInterop())
    //        {
    //            Assert.IsNotNull(excelApp.Application);
    //        }
          
    //    }
    //    /// <summary>
    //    /// Проверяет, что создает книгу в случае ее отсутствия
    //    /// </summary>
    //    [TestMethod]
    //    public void WorkbooksNotNull()
    //    {
    //        using (excelApp = new ExcelInterop())
    //        {
    //            Assert.IsNotNull(excelApp.Workbooks);
    //        }
    //    }
    //    /// <summary>
    //    /// Проверка, что верно происходит присваивание
    //    /// </summary>
    //    [TestMethod]
    //    public void ApplicationEqual()
    //    {
    //        var mock = new Mock<Application>();
    //        mock.Setup(a => a.Application).Returns(new Application());
    //        excelApp = new ExcelInterop {Application = mock.Object};
    //        Assert.AreEqual(excelApp.Application, mock.Object);
    //    }

    //    /// <summary>
    //    /// Проверяет, что создает страницу в случае ее отсутствия
    //    /// </summary>
    //    [TestMethod]
    //    public void WorkbookNotNull()
    //    {

    //        var mock = new Mock<Application>();
    //        mock.Setup(a => a.Application).Returns(new Application());
    //       // mock.Setup(a => a.Workbook).Returns( new Application().Workbooks.Add(null));

    //        using (excelApp = new ExcelInterop{Application =(Application)Marshal.GetActiveObject("Excel.Application") })
    //        {
    //            Assert.IsNotNull(excelApp.Workbook);
    //        }
    //    }
    //    [TestMethod]
    //    public void WorkbookNotNull1()
    //    {
    //        using (excelApp = new ExcelInterop())
    //        {
    //            Assert.IsNotNull(excelApp.Workbook);
    //        }
    //    }
    //    [TestMethod]
    //    public void WorkbookEqual()
    //    {
    //        //var mock = new Mock<Workbook>();
    //        //mock.Setup(a => a.Application.Workbooks.Add(null)).Returns(new Workbook());

    //        //var a = new Application();
           
    //        //using (excelApp = new ExcelInterop())
    //        //{
    //        //    excelApp.Workbook = a.Workbooks.Add();
    //        //    //Assert.AreEqual(excelApp.Workbook, mock.Object);
    //        //}
    //    }
    //}
}
