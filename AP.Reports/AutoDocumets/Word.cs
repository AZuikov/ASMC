using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AP.Reports.Interface;
using AP.Reports.Utils;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Office.Interop.Word;
using DataTable = System.Data.DataTable;
using Paragraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;

namespace AP.Reports.AutoDocumets
{
    public class Word:ITextGraphicsReport
    {
        public string Path { get; private set; }
        private WordprocessingDocument _document;
        private Stream _stream;
        private OpenSettings _openSettings;

        public Word(Stream stream)
        {
            _stream = stream;
            Init();
        }

        public Word()
        {
        }
        
        private void Init()
        {
            _openSettings = new OpenSettings {AutoSave = false};

            if (Path != null)
            {
                _document = WordprocessingDocument.Open(Path, true, _openSettings);
            }
            else if (_stream != null)
            {
                _document = WordprocessingDocument.Open(_stream, true, _openSettings);
            }
            else
            {
                _stream = new MemoryStream();
                _document = WordprocessingDocument.Create(_stream, WordprocessingDocumentType.Document, false);
                MainDocumentPart mainPart = _document.AddMainDocumentPart();
                mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
                Body body = mainPart.Document.AppendChild(new Body());
            }
        }

        #region IGrapsReport
        public void SaveAs(string pathToSave)
        {
            Path = pathToSave;
            _document?.SaveAs(Path);
        }
        public void Close()
        {
            _document?.Close();
            _stream?.Close();
        }
        public void FindStringAndAllReplac(string sFind, string sReplac)
        {
            FindStringAndReplac(sFind, sReplac);
            throw new NotImplementedException();
        }
        public void FindStringAndReplac(string sFind, string sReplac, bool invert = true)
        {
            var document = _document.MainDocumentPart.Document;
            foreach (var text in document.Descendants<Text>())
            {
                if (text.Text.Contains(sFind))
                {
                    text.Text= text.Text.Replace(sFind, sReplac);
                }
            }
            throw new NotImplementedException();
        }
        public void NewDocument()
        {
            Init();
        }
        public void OpenDocument(string sPath)
        {
            Path = sPath;
            Init();
        }
        public void Save()
        {
            throw new NotImplementedException();
        }

        public void InsertText(string text)
        {
            throw new NotImplementedException();
        }

        public void MergeDocuments(string pathdoc)
        {
            if (_document==null) return;
           var altChunId = "alt"+ pathdoc.GetHashCode().ToString();
            MainDocumentPart mainPart = _document.MainDocumentPart;
            AlternativeFormatImportPart chumk =
                mainPart.AddAlternativeFormatImportPart(AlternativeFormatImportPartType.WordprocessingML, altChunId);
            using (FileStream fileStream = File.Open(pathdoc,FileMode.Open))
            {

                chumk.FeedData(fileStream);
            }
 
            AltChunk altChunk = new AltChunk();
            altChunk.Id = altChunId;
            mainPart.Document.Body.InsertAfter(altChunk,
                mainPart.Document.Body.Elements<Paragraph>().Last());
        }

        public void MergeDocuments(IEnumerable<string> pathdoc)
        {
            throw new NotImplementedException();
            foreach (var path in pathdoc)
            {
                MergeDocuments(path);
            }
        }

        public void MoveEnd()
        {
            throw new NotImplementedException();
        }

        public void MoveHome()
        {
            throw new NotImplementedException();
        }

        public void FindStringAndAllReplacImage(string sFind, Bitmap image)
        {
            throw new NotImplementedException();
        }

        public void FindStringAndReplacImage(string sFind, string image, bool invert = true)
        {
            throw new NotImplementedException();
        }

        public void NewDocumentTemp(string templatePath)
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

        public void InsertImageToBookmark(Bitmap image, string bm)
        {
            throw new NotImplementedException();
        }

        public void InsertImage(Bitmap image)
        {
            throw new NotImplementedException();
        }

        public void FindStringAndReplaceImage(string sFind, string image)
        {
            throw new NotImplementedException();
        }

        public void FindStringAndAllReplace(string sFind, string sReplac)
        {
            throw new NotImplementedException();
        }

        public void FindStringAndReplace(string sFind, string sReplac, bool invert = true)
        {
            throw new NotImplementedException();
        }


        #endregion

    }
}
