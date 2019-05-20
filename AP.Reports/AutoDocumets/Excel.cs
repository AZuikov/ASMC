using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AP.Reports.Interface;
using AP.Reports.Utils;
using ClosedXML.Excel;

namespace AP.Reports.AutoDocumets
{
    class Excel : IGraphsReport, IDisposable
    {
        private XLWorkbook _workbook;

        #region ctors
        public Excel()
        {
            _workbook = new XLWorkbook();
        }
        #endregion

        #region IGraphsReport

        public void Close()
        {
            _workbook.Dispose();
        }

        public void FillsTableToBookmark(DataTable dt, string bm, bool del = false, ConditionalFormatting cf = default(ConditionalFormatting))
        {
            throw new NotImplementedException();
        }

        public void FindStringAndAllReplac(string sFind, string sReplac)
        {
            throw new NotImplementedException();
        }

        public void FindStringAndAllReplacImage(string sFind, Bitmap image)
        {
            throw new NotImplementedException();
        }

        public void FindStringAndReplac(string sFind, string sReplac, bool invert = true)
        {
            throw new NotImplementedException();
        }

        public void FindStringAndReplacImage(string sFind, string image, bool invert = true)
        {
            throw new NotImplementedException();
        }

        public void InsertImage(Bitmap image)
        {
            throw new NotImplementedException();
        }

        public void InsertImageToBookmark(Bitmap image, string bm)
        {
            throw new NotImplementedException();
        }

        public void InsertNewTableToBookmark(DataTable dt, string bm, ConditionalFormatting cf = default(ConditionalFormatting))
        {
            throw new NotImplementedException();
        }

        public void InsertText(string text)
        {
            throw new NotImplementedException();
        }

        public void InsertTextToBookmark(string text, string bm)
        {
            throw new NotImplementedException();
        }

        public void MergeDocuments(string pathdoc)
        {
            throw new NotImplementedException();
        }

        public void NewDocument()   //???
        {
            throw new NotImplementedException();
        }

        public void NewDocumentTemp(string templatePath)   //??
        {
            throw new NotImplementedException();
        }

        public void OpenDocument(string sPath)
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            var deal = new FolderBrowserDialog();
            var result = deal.ShowDialog();
            if (result == DialogResult.OK)
            {
                _workbook.SaveAs(deal.SelectedPath);
                Process.Start(deal.SelectedPath);
            }
        }

        public void SaveAs(string pathToSave)
        {
            _workbook.SaveAs(pathToSave);
        }
        #endregion

        public void Dispose()
        {
            _workbook.Dispose();
        }

      
    }
}