using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using AP.Reports.AutoDocumets;
using AP.Reports.MSInterop;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace UnitTest.AP.Reports
{
    [TestClass]
    public class UtWord
    {
        private Word word;
        [DataRow(@"321321")]
        [DataRow(@"вфывфыdsadasDac")]
        [DataRow(@"≥®")]
        [DataRow(@"C°")]
        [TestMethod]
        public void InsertText(string value)
        {
            word=new Word();
            word.NewDocument();
            word.InsertText($"{value}");
            Assert.AreEqual(word.SetElement.LastChild.InnerText, $"{value}");
        }
        [TestMethod]
        [DataRow(@"C:\Документ Microsoft Word1.docx")]
        [DataRow(@"C:\Документ Microsoft Word1.dotx")]
        [DataRow(@"C:\Документ Microsoft Word1.docm")]
        [DataRow(@"C:\Документ Microsoft Word1.dfocm.docm")]
        public void CheckPathToDocument(string path)
        {
            word = new Word {Path = $"{path}"};
        }  
        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        [DataRow(@"C:\Документ Microsoft Word1.Docx")]
        [DataRow(@"C:\Документ Microsoft Word1.d0tx")]
        [DataRow(@"C:\Документ Microsoft Word1.doc")]
        [DataRow(@"C:\Документ Microsoft Word1.dfocm.txt")]
        public void CheckPathToDocumentFormatException(string path)
        {
            word = new Word { Path = $"{path}" };
        }    
        [TestMethod]
        public void SetMoveHome()
        {
            word = new Word(); 
            word.NewDocument();
            word.MoveHome();
            Assert.IsTrue(word.SetElement.Parent is Body); 
        }
        [TestMethod]
        public void SetMoveEnd()
        {
            word = new Word();
            word.NewDocument();
            word.MoveEnd();
            Assert.IsTrue(word.SetElement is Paragraph);
        }

        [TestMethod]
        public void TestMethod2()
        {
            word = new Word();
            word.OpenDocument(@"C:\Users\02tav01\Documents\Документ Microsoft Word1.docx");
            var dataTable = new DataTable();
            for(var i = 0; i < 6; i++)
            {
                dataTable.Columns.Add("a" + i);
            }
            DataRow[] dataRow = new DataRow[10];
            for(var i = 0; i < 10; i++)
            {
                dataRow[i] = dataTable.NewRow();
                for(var j = 0; j < 6; j++)
                {
                    dataRow[i][j] = i + j.ToString();
                }
                dataTable.Rows.Add(dataRow[i]);
            }
            word.FillsTableToBookmark("test1",dataTable);
          
            word.SaveAs(@"C:\Users\02tav01\Documents\Документ Microsoft Word3.docx");
            word.Close();
        }
    }

}
