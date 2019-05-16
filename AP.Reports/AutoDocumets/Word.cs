using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AP.Reports.Interface;
using AP.Reports.Utils;

namespace AP.Reports.AutoDocumets
{
    public class Word:IReport
    {

        #region IReport
        public void SaveAs(string pathToSave)
        {
            throw new NotImplementedException();
        }

        public void Close()
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

        public void InsertTable(DataTable dt)
        {
            throw new NotImplementedException();
        }

        public void FillsTableToBookmark(DataTable dt, string bm, bool del = false,
            ConditionalFormatting cf = default(ConditionalFormatting))
        {
            throw new NotImplementedException();
        }

        public void InsertNewTableToBookmark(DataTable dt, string bm, ConditionalFormatting cf = default(ConditionalFormatting))
        {
            throw new NotImplementedException();
        }

        public void InsertTextToBookmark(string text, string bm)
        {
            throw new NotImplementedException();
        }

        public void InsertText(string text)
        {
            throw new NotImplementedException();
        }

        public void InsertImageToBookmark(Bitmap image, string bm)
        {
            throw new NotImplementedException();
        }

        public void InsertImage(Bitmap image)
        {
            throw new NotImplementedException();
        }

        public void MergeDocuments(string pathdoc)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
