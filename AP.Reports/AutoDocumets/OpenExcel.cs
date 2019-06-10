using System;
using AP.Reports.Interface;
using System.Collections.Generic;
using AP.Reports.Utils;
using System.Data;
using System.Drawing;

namespace AP.Reports.AutoDocumets
{
    public class OpenExcel : ITextGraphicsReport, IDisposable
    {
        //private XLWorkbook _workbook;
        //private string _filePath;
        //private IXLCell _currentCell;

        public string Path => throw new NotImplementedException();

        string IReport.Path { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        #region ITextGraphicsReport methods

        #region File manipulation (Open, close, save, etc.)
        public void Close()
        {
            throw new NotImplementedException();
        }

        public void NewDocument()
        {
            throw new NotImplementedException();
        }

        public void NewDocumentTemp(string templatePath)
        {
            throw new NotImplementedException();
        }

        public void OpenDocument(string sPath)
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public void SaveAs(string pathToSave)
        {
            throw new NotImplementedException();
        }

        public void MergeDocuments(string pathdoc)
        {
            throw new NotImplementedException();
        }

        public void MergeDocuments(IEnumerable<string> pathdoc)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region  Inserts by bookmark
        public void FillsTableToBookmark(string bm, DataTable dt, bool del = false, ConditionalFormatting cf = default(ConditionalFormatting))
        {
            throw new NotImplementedException();
        }

        public void InsertImageToBookmark(string bm, Bitmap image)
        {
            throw new NotImplementedException();
        }

        public void InsertNewTableToBookmark(string bm, DataTable dt, ConditionalFormatting cf = default(ConditionalFormatting))
        {
            throw new NotImplementedException();
        }

        public void InsertTextToBookmark(string bm, string text)
        {
            throw new NotImplementedException();
        }


        #endregion

        #region Inserts

        public void InsertImage(Bitmap image)
        {
            throw new NotImplementedException();
        }


        public void InsertTable(DataTable dt, ConditionalFormatting cf = default(ConditionalFormatting))
        {
            throw new NotImplementedException();
        }

        public void InsertText(string text)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Find strings and...
        public void FindStringAndAllReplace(string sFind, string sReplace)
        {
            throw new NotImplementedException();
        }

        public void FindStringAndAllReplaceImage(string sFind, Bitmap image)
        {
            throw new NotImplementedException();
        }

        public void FindStringAndReplace(string sFind, string sReplace)
        {
            throw new NotImplementedException();
        }

        public void FindStringAndReplaceImage(string sFind, Bitmap image)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Navigation

        public void MoveEnd()
        {
            throw new NotImplementedException();
        }

        public void MoveHome()
        {
            throw new NotImplementedException();
        }
        #endregion

        #endregion

        #region  IDisposable method

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void FindStringAndAllReplaceImage(string sFind, Bitmap image, float scale = 1)
        {
            throw new NotImplementedException();
        }

        public void FindStringAndReplaceImage(string sFind, Bitmap image, float scale = 1)
        {
            throw new NotImplementedException();
        }

        public void InsertImageToBookmark(string bm, Bitmap image, float scale = 1)
        {
            throw new NotImplementedException();
        }

        public void InsertImage(Bitmap image, float scale = 1)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region private OpenExcel methods
        #endregion

        #region public OpenExcel methods
        #endregion
    }
}