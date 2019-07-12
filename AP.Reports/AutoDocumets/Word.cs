using AP.Reports.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DevExpress.XtraRichEdit;
using DevExpress.XtraRichEdit.API.Native;
using AP.Utils.Data;

namespace AP.Reports.AutoDocumets
{
    public class Word : Document, IMsOffice, IDisposable
    {
        private RichEditDocumentServer _documentServer;
        private DocumentPosition _documentPosition;
        private DocumentRange _documentRange;
        private DevExpress.XtraRichEdit.API.Native.Document _document;
        private SubDocument _subDocument;

        private DocumentPosition _documentPositionHeader;
        private DocumentRange _documentRangeHeader;

        /// <summary>
        /// форматы файлов
        /// </summary>
        public enum FileFormat
        {
            /// <summary>
            /// Документ
            /// </summary>
            [StringValue("docx")] Docx,

            /// <summary>
            /// Шаблон
            /// </summary>
            [StringValue("dotx")] Dotx,

            /// <summary>
            /// С поддержкой макросов
            /// </summary>
            [StringValue("docm")] Docm
        }

        public DocumentRange DocumentRange
        {
            get { return _documentRange ?? (_documentRange = _document?.Range); }
            private set { _documentRange = value; }
        }

        public DocumentRange DocumentRangeHeader
        {
            get { return _documentRangeHeader ?? (_documentRangeHeader = _subDocument?.Range); }
            private set { _documentRangeHeader = value; }
        }

        public DocumentPosition DocumentPositionHeader
        {
            get
            {
                if (_documentPositionHeader == null)
                {
                    _documentPositionHeader = _document?.Selections.First().End;
                }

                return _documentPositionHeader;
            }
            private set { _documentPositionHeader = value; }
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
            private set { _documentPosition = value; }
        }

        private string _path;

        /// <inheritdoc />
        public string Path
        {
            get { return _path; }
            set
            {
                if (ValidPath(value))
                {
                    _path = value;
                }
                else
                {
                    throw new FormatException("Недопустимый формат файла.");
                }
            }
        }

        /// <inheritdoc />
        public Array Formats
        {
            get { return Enum.GetValues(typeof(FileFormat)); }
        }

        /// <summary>
        /// Возвращает результат проверки пути к файлу
        /// </summary>
        /// <param name="path">Проверяемый путь</param>
        /// <returns>Возвращает true если путь и формат файла допустимы</returns>
        private bool ValidPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new FileNotFoundException("Путь к файлу не указан.");
            }

            var extension = System.IO.Path.GetExtension(path);
            foreach (var ff in Enum.GetValues(typeof(FileFormat)))
            {
                if (extension.Equals("." + ((Enum) ff).GetStringValue()))
                {
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc />
        public void Close()
        {
            _documentServer = null;
            _document = null;
            _documentPosition = null;
            _documentRange = null;
        }

        /// <inheritdoc />
        public void FillTableToBookmark(string bm, DataTable dt, bool del = false,
            ConditionalFormatting cf = default(ConditionalFormatting))
        {
            if (string.IsNullOrEmpty(bm) || dt == null) return;
            DocumentRange = _document.Bookmarks[bm]?.Range;
            if (_document?.Tables == null || _document.Tables.Get(DocumentRange).Count <= 0) return;

            var table = _document.Tables.Get(DocumentRange).First();
            if (dt.Rows.Count < 1)
            {
                _document.BeginUpdate();
                _document.Tables.Remove(table);
                _document.EndUpdate();
                return;
            }

            FillingTable(table, dt, cf);
            SetConditionalFormatting(table, dt, cf);
        }

        /// <inheritdoc />
        public void FindStringAndAllReplace(string sFind, string sReplace)
        {
            if (string.IsNullOrEmpty(sFind) || sReplace == null)
                return;
            _document.BeginUpdate();
            while (FindStringSetDocumentPosition(sFind) > 0)
            {    
                InsertText(sReplace);
                _document.Delete(DocumentRange);   
            }
            _document.EndUpdate();
        }


        /// <inheritdoc />
        public void FindStringInHeaderAndAllReplace(string sFind, string sReplace)
        {
            if (string.IsNullOrEmpty(sFind) || sReplace == null)
                return;
            while (FindStringToHeaderSetDocumentPosition(sFind) > 0)
            {
                var section = _document.Sections.First();
                _subDocument = section.BeginUpdateHeader(HeaderFooterType.First);
                _subDocument.InsertText(DocumentPositionHeader, sReplace);
                _subDocument.Delete(DocumentRangeHeader);
                section.EndUpdateHeader(_subDocument);
                section.DifferentFirstPage = true;
            }
        }

        /// <inheritdoc />
        public void FindStringInHeaderAndAllReplaceFiled(string sFind, string sCode)
        {
            if (string.IsNullOrEmpty(sFind) || sCode == null)
                return;
            while (FindStringToHeaderSetDocumentPosition(sFind) > 0)
            {
                var section = _document.Sections.First();
                _subDocument = section.BeginUpdateHeader(HeaderFooterType.First);
                InsertFiled(sCode);
                _subDocument.Delete(DocumentRangeHeader);
                section.EndUpdateHeader(_subDocument);
                section.DifferentFirstPage = true;
            }
        }

        /// <inheritdoc />
        public void InsertFiledInHeader(string code)
        {
            _subDocument.Fields.Create(DocumentPositionHeader, code);
        }

        /// <inheritdoc />
        public void InsertFiled(string code)
        {
            _document.Fields.Create(DocumentPosition, code);
        }

        /// <inheritdoc />
        public void FindStringAndAllReplaceImage(string sFind, Bitmap image, float scale = 1)
        {
            if (string.IsNullOrEmpty(sFind) || image == null) return;
            _document.BeginUpdate();
            while (FindStringSetDocumentPosition(sFind) > 0)
            {
                InsertImage(image, scale);
                _document.Delete(DocumentRange);
            }

            _document.EndUpdate();
        }

        private int FindStringSetDocumentPosition(string sFind)
        {
            var foundTotal = _document.FindAll(new Regex(PatternFindText(sFind)), _document.Range);
            if (foundTotal.Length <= 0) return foundTotal.Length;
            DocumentRange = foundTotal.First();
            DocumentPosition = DocumentRange.Start;
            return foundTotal.Length;
        }

        private int FindStringToHeaderSetDocumentPosition(string sFind)
        {
            _subDocument = _document.Sections.First().BeginUpdateHeader(HeaderFooterType.First);
            var foundTotal = _subDocument.FindAll(new Regex(PatternFindText(sFind)), _subDocument.Range);
            if (foundTotal.Length <= 0) return foundTotal.Length;
            DocumentRangeHeader = foundTotal.First();
            DocumentPositionHeader = DocumentRangeHeader.Start;
            return foundTotal.Length;
        }

        /// <inheritdoc />
        public void FindStringAndReplace(string sFind, string sReplace)
        {
            if (string.IsNullOrEmpty(sFind) || sReplace == null) return;
            _document.BeginUpdate();
            if (FindStringSetDocumentPosition(sFind) > 0)
            {
                _document.InsertText(DocumentPosition, sReplace);
                _document.Delete(DocumentRange);
            }

            _document.EndUpdate();
        }

        /// <inheritdoc />
        public void FindStringAndReplaceImage(string sFind, Bitmap image, float scale = 1)
        {
            if (string.IsNullOrEmpty(sFind) || image == null)
                return;
            _document.BeginUpdate();
            if (FindStringSetDocumentPosition(sFind) > 0)
            {
                InsertImage(image, scale);
                _document.Delete(DocumentRange);
            }

            _document.EndUpdate();
        }

        /// <inheritdoc />
        public void InsertImage(Bitmap image, float scale = 1)
        {
            var shapes = _document.Images.Insert(DocumentPosition, image);
            shapes.ScaleX = scale;
            shapes.ScaleY = scale;
        }

        /// <inheritdoc />
        public void InsertImageToBookmark(string bm, Bitmap image, float scale = 1)
        {
            if (string.IsNullOrEmpty(bm) || image == null) return;
            DocumentPosition = _document.Bookmarks[bm]?.Range.Start;
            InsertImage(image, scale);
        }

        /// <inheritdoc />
        public void InsertNewTableToBookmark(string bm, DataTable dt,
            ConditionalFormatting cf = default(ConditionalFormatting))
        {
            if (string.IsNullOrEmpty(bm) || dt == null) return;
            DocumentPosition = _document.Bookmarks[bm]?.Range.Start;
            InsertTable(dt, cf);
        }

        /// <inheritdoc />
        public void InsertTable(DataTable dt, ConditionalFormatting cf = default(ConditionalFormatting))
        {
            if (dt == null) return;
            var table = _document.Tables.Create(DocumentPosition, dt.Rows.Count, dt.Columns.Count);
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
            if (tab == null || dt == null) return;
            AppenedRow(tab, dt);
            var insertDataToRow = false;
            var rowInsertCount = 0;
            _document.BeginUpdate();
            foreach (var row in tab.Rows)
            {
                if (rowInsertCount == dt.Rows.Count)
                {
                    break;
                }

                foreach (var cell in row.Cells)
                {
                    DocumentRange = cell.Range;
                    DocumentPosition = DocumentRange.Start;
                    if (_document.GetText(DocumentRange).Length > 2) continue;
                    insertDataToRow = true;
                    if (cell.Index < dt.Columns.Count)
                    {
                        InsertText(dt.Rows[rowInsertCount][cell.Index].ToString());
                    }
                }

                if (insertDataToRow)
                {
                    rowInsertCount++;
                }

                insertDataToRow = false;
            }

            _document.EndUpdate();
        }

        private void AppenedRow(Table tab, DataTable dt)
        {
            _document.BeginUpdate();
            while (tab.Rows.Count < dt.Rows.Count)
            {
                tab.Rows.Append();
            }

            var rows = tab.Rows.Count;
            for (var index = 0; index < rows; index++)
            {
                var row = tab.Rows[index];
                foreach (var cell in row.Cells)
                {
                    DocumentRange = cell.Range;
                    if (cell.ColumnSpan > 1)
                        continue;
                    if (_document.GetText(DocumentRange).Length <= 2) continue;
                    tab.Rows.Append();
                    break;
                }
            }

            _document.EndUpdate();
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
            var formatingColumn = dt.Columns.IndexOf(cf.NameColumn);
            if (cf.NameColumn == null) return;
            foreach (var row in tab.Rows)
            {
                if (!DoesItNeedToSetCondition(_document.GetText(row.Cells[formatingColumn].ContentRange), cf))
                {
                    continue;
                }

                if (cf.Region == ConditionalFormatting.RegionAction.Cell)
                {
                    row[formatingColumn].BackgroundColor = cf.Color;
                }

                if (cf.Region != ConditionalFormatting.RegionAction.Row)
                {
                    continue;
                }

                foreach (var cell in row.Cells)
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
            if (double.TryParse(conditional.Value, out var conditionValue)
                && double.TryParse(cellValue, out var tableValue))
            {
                switch (conditional.Condition)
                {
                    case ConditionalFormatting.Conditions.Equal:
                        if (Math.Abs(tableValue - conditionValue) < float.Epsilon)
                        {
                            return true;
                        }

                        return false;
                    case ConditionalFormatting.Conditions.Less:
                        if (tableValue < conditionValue)
                        {
                            return true;
                        }

                        return false;
                    case ConditionalFormatting.Conditions.LessOrEqual:
                        if (tableValue <= conditionValue)
                        {
                            return true;
                        }

                        return false;
                    case ConditionalFormatting.Conditions.More:
                        if (tableValue > conditionValue)
                        {
                            return true;
                        }

                        return false;
                    case ConditionalFormatting.Conditions.MoreOrEqual:
                        if (tableValue >= conditionValue)
                        {
                            return true;
                        }

                        return false;
                    case ConditionalFormatting.Conditions.NotEqual:
                        if (Math.Abs(tableValue - conditionValue) > float.Epsilon)
                        {
                            return true;
                        }

                        return false;
                }
            }
            //Если не числа - сравниваем как строки
            else
            {
                switch (conditional.Condition)
                {
                    case ConditionalFormatting.Conditions.Equal:
                        if (cellValue == conditional.Value)
                        {
                            return true;
                        }

                        return false;
                    case ConditionalFormatting.Conditions.NotEqual:
                        if (cellValue != conditional.Value)
                        {
                            return true;
                        }

                        return false;
                }
            }

            return false;
        }

        /// <inheritdoc />
        public void InsertText(string text)
        {
            _document.BeginUpdate();
            _document.InsertText(DocumentPosition, text);
            _document.EndUpdate();
        }

        /// <inheritdoc />
        public void InsertTextToBookmark(string bm, string text)
        {
            var bookmarks = _document.Bookmarks[bm];
            if (bookmarks == null)
            {
                throw new ArgumentException(bm + " закладка не существует");
            }

            DocumentPosition = _document.Bookmarks[bm].Range.Start;
        }

        /// <inheritdoc />
        public void MergeDocuments(string pathdoc)
        {
            if (_documentServer?.Document == null || string.IsNullOrEmpty(pathdoc)) return;

            using (var reds = new RichEditDocumentServer())
            {
                reds.LoadDocument(pathdoc, DevExpress.XtraRichEdit.DocumentFormat.OpenXml);
                _document.AppendDocumentContent(reds.Document.Range);
            }
        }

        /// <inheritdoc />
        public void MergeDocuments(IEnumerable<string> pathdoc)
        {
            foreach (var doc in pathdoc)
            {
                MergeDocuments(doc);
            }
        }

        /// <inheritdoc />
        public void MoveEnd()
        {
            _documentPosition = _document?.Range.End;
        }

        /// <inheritdoc />
        public void MoveHome()
        {
            _documentPosition = _document?.Range.Start;
        }

        /// <inheritdoc />
        public void NewDocument()
        {
            _documentServer = new RichEditDocumentServer();
            _document = _documentServer.Document;
        }

        /// <inheritdoc />
        public void NewDocumentTemp(string templatePath)
        {
            if (ValidPath(templatePath))
            {
                _documentServer = new RichEditDocumentServer();
                _documentServer.LoadDocumentTemplate(templatePath, DevExpress.XtraRichEdit.DocumentFormat.OpenXml);
                _document = _documentServer.Document;
            }
        }

        /// <inheritdoc />
        public void OpenDocument(string sPath)
        {
            Path = sPath;
            _documentServer = new RichEditDocumentServer();
            _documentServer.LoadDocument(Path, DevExpress.XtraRichEdit.DocumentFormat.OpenXml);
            _document = _documentServer.Document;
        }

        /// <inheritdoc />
        public void Save()
        {
            if (string.IsNullOrEmpty(Path))
            {
                _document.SaveDocument(Path, DevExpress.XtraRichEdit.DocumentFormat.OpenXml);
            }
        }

        /// <inheritdoc />
        public void SaveAs(string pathToSave)
        {
            if (!string.IsNullOrEmpty(pathToSave))
            {
                _documentServer.SaveDocument(pathToSave, DevExpress.XtraRichEdit.DocumentFormat.OpenXml);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _documentServer?.Dispose();
        }
    }
}