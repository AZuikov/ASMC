using AP.Reports.Interface;
using AP.Reports.Utils;
//using DocumentFormat.OpenXml;
//using DocumentFormat.OpenXml.Packaging;
//using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DevExpress.XtraRichEdit;
using DevExpress.XtraRichEdit.API.Native;


namespace AP.Reports.AutoDocumets
{
    public class Word : Document, ITextGraphicsReport, IDisposable
    {    
        private RichEditDocumentServer _documentServer;
        private DocumentPosition _documentPosition;
        private DocumentRange _documentRange;
        private DevExpress.XtraRichEdit.API.Native.Document _document;
        /// <summary>
        /// форматы файлов
        /// </summary>
        public enum FileFormat
        {
            /// <summary>
            /// Документ
            /// </summary>
            Docx,
            /// <summary>
            /// Шаблон
            /// </summary>
            Dotx,
            /// <summary>
            /// С поддержкой макросов
            /// </summary>
            Docm
        }
        public DocumentRange DocumentRange
        {
            get { return _documentRange ?? (_documentRange = _document?.Range); }
            private set
            {
                _documentRange = value;
            }
        }
        public DocumentPosition DocumentPosition
        {
            get
            {
                if (_documentPosition == null)
                {
                    _documentPosition = _document?.Selection.End;
                } 
                return _documentPosition;
            }
            private set
            {
                _documentPosition = value;
            }
        }

        private string _path;
        /// <summary>
        /// Путь к файлу
        /// </summary>
        public string Path
        {
            get
            {
                return _path;
            }
            set
            {
                if(ValidPath(value))
                {
                    _path = value;
                }
                else
                {
                    throw new FormatException("Недопустимый формат файла.");
                }
            }
        }
        /// <summary>
        /// Возвращает результат проверки пути к файлу
        /// </summary>
        /// <param name="path">Проверяемый путь</param>
        /// <returns>Возвращает true если путь и формат файла допустимы</returns>
        private bool ValidPath(string path)
        {
           
            if(string.IsNullOrEmpty(path))
            {
                throw new FileNotFoundException("Путь к файлу не указан.");
            }
            var extension = System.IO.Path.GetExtension(path);
            foreach(var ff in Enum.GetValues(typeof(FileFormat)))
            {
                if(extension.Equals("." + ff.ToString().ToLower()))
                {
                    return true;
                }
            }
            return false;
        }
        public void Close()
        {
            _documentServer = null;
            _document = null;
            _documentPosition = null;
            _documentRange = null;
        }

        public void FillsTableToBookmark(string bm, DataTable dt, bool del = false, ConditionalFormatting cf = default(ConditionalFormatting))
        {
            DocumentRange = _document.Bookmarks[bm]?.Range;
            var table = _document.Tables.Get(DocumentRange)?.First();
            if(dt.Rows.Count < 1)
            {   
                _document.BeginUpdate();
                _document.Tables.Remove(table);
                _document.EndUpdate();
                return;
            }   
            FillingTable(table, dt, cf);
            SetConditionalFormatting(table, dt, cf);
        }

        public void FindStringAndAllReplace(string sFind, string sReplace)
        {
            _document.BeginUpdate();
            while (FindStringSetDocumentPosition(sFind) > 0)
            {
                InsertText(sReplace);
                _document.Delete(DocumentRange);
            }  
            _document.EndUpdate();
        }

        public void FindStringAndAllReplaceImage(string sFind, Bitmap image, float scale = 1)
        {
            _document.BeginUpdate();
            while(FindStringSetDocumentPosition(sFind) > 0)
            {
                InsertImage(image, scale);
                _document.Delete(DocumentRange);
            }    
            _document.EndUpdate();
        }
        private int FindStringSetDocumentPosition(string sFind)
        {

            var foundTotal = _document.FindAll(new Regex(PatternFindText(sFind)), _document.Range);
            if(foundTotal.Length > 0)
            {
                DocumentRange = foundTotal.First();
                DocumentPosition = DocumentRange.Start;
            }    
            return foundTotal.Length;
        }
        public void FindStringAndReplace(string sFind, string sReplace)
        {
            _document.BeginUpdate();
            if(FindStringSetDocumentPosition(sFind) > 0)
            {
                _document.InsertText(DocumentPosition, sReplace);
                _document.Delete(DocumentRange);
            }
            _document.EndUpdate();
        }


        public void FindStringAndReplaceImage(string sFind, Bitmap image, float scale = 1)
        {
            _document.BeginUpdate();
            if(FindStringSetDocumentPosition(sFind) > 0)
            {
                InsertImage(image, scale);
               _document.Delete(DocumentRange);
            }
            _document.EndUpdate();
        }

        public void InsertImage(Bitmap image, float scale = 1)
        {
            var shapes=_document.Images.Insert(DocumentPosition, image);
            shapes.ScaleX = scale;
            shapes.ScaleY = scale;
        }

        public void InsertImageToBookmark(string bm, Bitmap image, float scale = 1)
        {

            DocumentPosition = _document.Bookmarks[bm]?.Range.Start;
            InsertImage(image, scale);
        }

        public void InsertNewTableToBookmark(string bm, DataTable dt, ConditionalFormatting cf = default(ConditionalFormatting))
        {    
            DocumentPosition = _document.Bookmarks[bm]?.Range.Start;
            InsertTable(dt, cf);
        }

        public void InsertTable(DataTable dt, ConditionalFormatting cf = default(ConditionalFormatting))
        {
            var table= _document.Tables.Create(DocumentPosition, dt.Rows.Count, dt.Columns.Count);
            table.PreferredWidthType = WidthType.Auto;
            FillingTable(table, dt, cf);
            SetConditionalFormatting(table, dt, cf);

            //  throw new NotImplementedException();
        }

        /// <summary>
        /// Заполняет таблицу
        /// </summary>
        /// <param name="tab">Принемает заполняемую таблицу</param>
        /// <param name="dt">Принимает таблицу с данными</param>
        /// <param name="cf">Форматирование таблицы</param>
        public void FillingTable(Table tab, DataTable dt,
            ConditionalFormatting cf = default(ConditionalFormatting))
        {
            while(tab.Rows.Count < dt.Rows.Count)
            {
                tab.Rows.Append();
            }

            foreach (var row in tab.Rows)
            {
                foreach (var cell in row.Cells)
                {
                    DocumentPosition = cell.Range.Start;
                    if (cell.ColumnSpan<2)
                    {   
                        InsertText(dt.Rows[row.Index][cell.Index].ToString());
                    }
                }
            }
           
            //throw new NotImplementedException();
        }
        /// <summary>
        /// Устанавливает заливку ячеек таблицы по правилу ConditionalFormatting
        /// </summary>
        /// <param name="tab">Таблица Word</param>
        /// <param name="dt">Таблица DataTable</param>
        /// <param name="cf">Правила</param>
        private void SetConditionalFormatting(Table tab, DataTable dt,
            ConditionalFormatting cf = default(ConditionalFormatting))
        {
            int formatingColumn = dt.Columns.IndexOf(cf.NameColumn);
            //string columnList = null;
            foreach(var row in tab.Rows)
            {
                
                if(!DoesItNeedToSetCondition (_document.GetText(row.Cells[formatingColumn].ContentRange), cf))
                {
                    continue;
                }

                if(cf.Region == ConditionalFormatting.RegionAction.Cell)
                {
                    row[formatingColumn].BackgroundColor = cf.Color;
                }
                if(cf.Region != ConditionalFormatting.RegionAction.Row)
                {
                    continue;
                }

                foreach(var cell in row.Cells)
                {
                    cell.BackgroundColor = cf.Color;  
                }
            }
        }
 
        /// <summary>
        /// Определяет, соответствует ли ячейка таблицы условию из набора правил ConditionalFormatting
        /// </summary>
        /// <param name="cellValue">Значение для сравнения</param>
        /// <param name="conditional">Правила сравнения</param>
        private bool DoesItNeedToSetCondition(string cellValue, ConditionalFormatting conditional)
        {
            //Если оба данных - числа, то сравниваем их как числа
            if(double.TryParse(conditional.Value, out var conditionValue)
                && double.TryParse(cellValue, out var tableValue))
            {
                switch(conditional.Condition)
                {
                    case ConditionalFormatting.Conditions.Equal:
                        if(Math.Abs(tableValue - conditionValue) < float.Epsilon)
                        {
                            return true;
                        }
                        return false;
                    case ConditionalFormatting.Conditions.Less:
                        if(tableValue < conditionValue)
                        {
                            return true;
                        }
                        return false;
                    case ConditionalFormatting.Conditions.LessOrEqual:
                        if(tableValue <= conditionValue)
                        {
                            return true;
                        }
                        return false;
                    case ConditionalFormatting.Conditions.More:
                        if(tableValue > conditionValue)
                        {
                            return true;
                        }
                        return false;
                    case ConditionalFormatting.Conditions.MoreOrEqual:
                        if(tableValue >= conditionValue)
                        {
                            return true;
                        }
                        return false;
                    case ConditionalFormatting.Conditions.NotEqual:
                        if(Math.Abs(tableValue - conditionValue) > float.Epsilon)
                        {
                            return true;
                        }
                        return false;
                }
            }
            //Если не числа - сравниваем как строки
            else
            {
                switch(conditional.Condition)
                {
                    case ConditionalFormatting.Conditions.Equal:
                        if(cellValue == conditional.Value)
                        {
                            return true;
                        }
                        return false;
                    case ConditionalFormatting.Conditions.NotEqual:
                        if(cellValue != conditional.Value)
                        {
                            return true;
                        }
                        return false;
                }
            }
            return false;
        }
        public void InsertText(string text)
        {     
            _document.InsertText(DocumentPosition, text);
        }

        public void InsertTextToBookmark(string bm, string text)
        {
            var bookmarks = _document.Bookmarks[bm];
            if (bookmarks == null)
            {
                throw new ArgumentException(bm +" закладка не существует");
            }
            DocumentPosition = _document.Bookmarks[bm].Range.Start;
        
        }

        public void MergeDocuments(string pathdoc)
        {
            if (_documentServer?.Document == null)
            {
               return;
            }

            using(var reds = new RichEditDocumentServer())
            {
                reds.LoadDocument(pathdoc, DevExpress.XtraRichEdit.DocumentFormat.OpenXml);
                _document.AppendDocumentContent(reds.Document.Range);
            }
        }

        public void MergeDocuments(IEnumerable<string> pathdoc)
        {
            foreach (var doc in pathdoc)
            {
                MergeDocuments(doc);
            }
        }

        public void MoveEnd()
        {
            _documentPosition = _document?.Range.End;
        }

        public void MoveHome()
        {   
            _documentPosition = _document?.Range.Start;
        }

        public void NewDocument()
        {
            _documentServer = new RichEditDocumentServer();
            _document = _documentServer.Document; 
        }

        public void NewDocumentTemp(string templatePath)
        {
            if(ValidPath(templatePath))
            {
                _documentServer = new RichEditDocumentServer();
                _documentServer.LoadDocumentTemplate(templatePath, DevExpress.XtraRichEdit.DocumentFormat.OpenXml);
                _document = _documentServer.Document;
            }
        }

        public void OpenDocument(string sPath)
        {
            Path = sPath;
            _documentServer = new RichEditDocumentServer(); 
            _documentServer.LoadDocument(Path, DevExpress.XtraRichEdit.DocumentFormat.OpenXml);
            _document = _documentServer.Document;
        }

        public void Save()
        {
            if (string.IsNullOrEmpty(Path))
            {
                _document.SaveDocument(Path, DevExpress.XtraRichEdit.DocumentFormat.OpenXml);
            }
           
        }

        public void SaveAs(string pathToSave)
        {
            if (!string.IsNullOrEmpty(pathToSave))
            {
                _document?.SaveDocument(pathToSave, DevExpress.XtraRichEdit.DocumentFormat.OpenXml);
            }
        }

        public void Dispose()
        {
            _documentServer?.Dispose();
        }
    }

}