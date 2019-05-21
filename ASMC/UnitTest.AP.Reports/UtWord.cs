using System;
using System.Collections.Generic;
using AP.Reports.AutoDocumets;
using Microsoft.Office.Interop.Word;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest.AP.Reports
{
    [TestClass]
    public class UtWord
    {
        private Word dsaas;
        [TestMethod]
        public void TestMethod1()
        {
            dsaas=new Word();
            dsaas.NewDocument();
            dsaas.SaveAs(@"C:\Users\02tav01\Pictures\График.docx");
            dsaas.Close();
        }
        [TestMethod]
        public void TestMethod2()
        {
            dsaas = new Word();
            dsaas.OpenDocument(@"C:\Users\02tav01\Documents\Документ Microsoft Word1.docx");
            dsaas.MergeDocuments(@"C:\Users\02tav01\Documents\Документ Microsoft Word2.docx");
            dsaas.SaveAs(@"C:\Users\02tav01\Documents\Документ Microsoft Word3.docx");
            dsaas.Close();
        }
        [TestMethod]
        public void TestMethod3()
        {
            List<string> test = new List<string>()
            {
                @"C:\Users\02tav01\Documents\Документ Microsoft Word2.docx",
                @"C:\Users\02tav01\Documents\Документ Microsoft Word4.docx",
                @"C:\Users\02tav01\Documents\Документ Microsoft Word5.docx",
            };
            dsaas = new Word();
            dsaas.OpenDocument(@"C:\Users\02tav01\Documents\Документ Microsoft Word1.docx");
            dsaas.MergeDocuments(test);
            dsaas.SaveAs(@"C:\Users\02tav01\Documents\Документ Microsoft Word3.docx");
            dsaas.Close();
        }
    }
}
