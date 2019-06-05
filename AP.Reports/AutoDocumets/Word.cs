using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using AP.Reports.Interface;
using AP.Reports.Utils;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DataTable = System.Data.DataTable;
using Paragraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;

namespace AP.Reports.AutoDocumets
{
    public class Word : ITextGraphicsReport
    {
        private string _path;
        /// <summary>
        /// Путь к файлу
        /// </summary>
        public string Path
        {
            get { return _path;}
            private set
            {
                if (validPath(value))
                {
                    _path = value;
                }

            }
        }

        private bool validPath(string path)
        {
            if (path == null)
            {
                throw new FileNotFoundException("Путь к файлу не указан.");
            }
            if (!System.IO.Path.GetExtension(path).Equals(@".docx"))
            {
                throw new FormatException("Формат файла не .docx");
            }
            return true;
        }
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
                try
                {
                    _document = WordprocessingDocument.Open(Path, true, _openSettings);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
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
                mainPart.Document = new Document();
                mainPart.Document.AppendChild(new Body());
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

        public void FindStringAndAllReplace(string sFind, string sReplace)
        {
            var document = _document.MainDocumentPart.Document;
            foreach (var text in document.Descendants<Text>())
            {
                if (Regex.IsMatch(sFind, @"(^|\s)" + sFind + "(^|$)"))
                {
                    text.Text = text.Text.Replace(sFind, sReplace);
                }
            }
        }

        public void FindStringAndReplace(string sFind, string sReplace)
        {
            var document = _document.MainDocumentPart.Document;
            foreach (var text in document.Descendants<Text>())
            {
                if (Regex.IsMatch(sFind, @"(^|\s)" + sFind + "(^|$)"))
                {
                    text.Text = text.Text.Replace(sFind, sReplace);
                    return;
                }
            }
        }

        public void NewDocument()
        {
            Init();
        }

        public void OpenDocument(string sPath)
        {
            Path = sPath;
            try
            {
                Init();
            }
            catch (FileNotFoundException ex)
            {
                throw new FileNotFoundException(ex.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public void MergeDocuments(string pathdoc)
        {
            if (_document == null) return;
            if (!validPath(pathdoc)) return;
            var altChunId = "alt" + (pathdoc.GetHashCode() ^ new Random().Next());
            var mainPart = _document.MainDocumentPart;
            AlternativeFormatImportPart chumk;
            try
            {
                 chumk = mainPart.AddAlternativeFormatImportPart(AlternativeFormatImportPartType.WordprocessingML, altChunId);
            }
            catch (Exception)
            {
                Thread.Sleep(100);
                altChunId= "alt" + (pathdoc.GetHashCode() ^ new Random().Next());
                chumk = mainPart.AddAlternativeFormatImportPart(AlternativeFormatImportPartType.WordprocessingML, altChunId);
            }
           
            try
            {
                using (var fileStream = File.Open(pathdoc, FileMode.Open))
                {
                    chumk.FeedData(fileStream);
                }
            }
            catch (PathTooLongException)
            {
                throw;
            }
            catch (IOException)
            {
                return;
            }

            AltChunk altChunk = new AltChunk {Id = altChunId};
            mainPart.Document.Body.InsertAfter(altChunk,
                mainPart.Document.Body.Elements<Paragraph>().Last());
            _document.Save();
        }

        public void MergeDocuments(IEnumerable<string> pathdoc)
        {
            foreach (var path in pathdoc.Reverse())
            {
                MergeDocuments(path);
            }
        }

        public void FindStringAndAllReplaceImage(string sFind, Bitmap image)
        {
            throw new NotImplementedException();
        }

        public void FindStringAndReplaceImage(string sFind, Bitmap image)
        {
            throw new NotImplementedException();
        }

        public void NewDocumentTemp(string templatePath)
        {
            throw new NotImplementedException();
        }

        public void FillsTableToBookmark(string bm, DataTable dt, bool del = false,
            ConditionalFormatting cf = default(ConditionalFormatting))
        {
            //var bookMarks = FindBookmarks(_document.MainDocumentPart.Document);
            //foreach (var bookMark in bookMarks)
            //{
            //    if (!bookMark.Key.Contains(bm)) continue;
            //    var runElement = new Run(new Text("fds"));
            //    bookMark.Value.(runElement);
            //    break;
            //}
            //_document.Save();
            throw new NotImplementedException();
        }

        public void InsertNewTableToBookmark(string bm, DataTable dt,
            ConditionalFormatting cf = default(ConditionalFormatting))
        {
          
            throw new NotImplementedException();
        }

        public void InsertTextToBookmark(string bm, string text)
        {
            var bookMarks = FindBookmarks();
            foreach (var bookMark in bookMarks)
            {
                if (!bookMark.Key.Contains(bm)) continue;
                var runElement = new Run(new Text(text));
                bookMark.Value.InsertAfterSelf(runElement);
                break;
            }
        }


        public void InsertImageToBookmark(string bm, Bitmap image)
        {
<<<<<<< HEAD
            //if(_homeDocument)
            //{
            //    SetElement.FirstChild.InsertAfterSelf((new Run(new Text(text))));
            //}
            if(SetElement.Descendants<Text>()?.Count() <= 0)
            {
                SetElement.AppendChild((new Run(new Text(text))));
            }

            //throw new NotImplementedException();
=======
            throw new NotImplementedException();
>>>>>>> e38150c2f10fc91d9ce8dfab9946e6cb1ca6b1d9
        }

        public void InsertImage(Bitmap image)
        {
<<<<<<< HEAD
            SetElement.AppendChild((new Run(GenerateTable(dt.Columns.Count, dt.Rows.Count))));
            var table = SetElement.Parent.Descendants<Table>().Last();

            // ReSharper disable once PossiblyMistakenUseOfParamsMethod
            FillingTable(table, dt, cf);
            SetConditionalFormatting(table, dt, cf);
           
            //throw new NotImplementedException();
=======
            throw new NotImplementedException();
>>>>>>> e38150c2f10fc91d9ce8dfab9946e6cb1ca6b1d9
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public void InsertText(string text)
        {
            _document?.MainDocumentPart.Document.Body.AppendChild(new Paragraph().AppendChild(new Run().AppendChild(new Text(text))));
          
        }

        public void InsertTable(DataTable dt, ConditionalFormatting cf)
        {
            var table = GenerateTable(5, 2);
            _document?.MainDocumentPart.Document.Body.Append(table);
            throw new NotImplementedException();
        }

        public void MoveEnd()
        {
            throw new NotImplementedException();
        }

        public void MoveHome()
        {
            throw new NotImplementedException();
        }

        #endregion

        /// <summary>
        /// Возвращает все закладки в документе
        /// </summary>
        public Dictionary<string, BookmarkEnd> FindBookmarks()
        {
            return FindBookmarks(_document.MainDocumentPart.Document);
        }

        /// <summary>
        /// Возвращает все закладки в документе
        /// </summary>
        /// <param name="documentPart">Элемент Word-документа</param>
        /// <param name="outs">конечный результат</param>
        /// <param name="bStartWithNoEnds">Cловарь, который будет содержать только начало закладок</param>
        /// <returns></returns>
        private static Dictionary<string, BookmarkEnd> FindBookmarks(OpenXmlElement documentPart,
            Dictionary<string, BookmarkEnd> outs = null, Dictionary<string, string> bStartWithNoEnds = null)
        {
            if (outs == null)
            {
                outs = new Dictionary<string, BookmarkEnd>();
            }

            if (bStartWithNoEnds == null)
            {
                bStartWithNoEnds = new Dictionary<string, string>();
            }

            foreach (var docElement in documentPart.Elements())
            {
                switch (docElement)
                {
                    case BookmarkStart _:
                        var bookmarkStart = docElement as BookmarkStart;
                        // ReSharper disable once PossibleNullReferenceException
                        bStartWithNoEnds.Add(bookmarkStart.Id, bookmarkStart.Name);
                        break;
                    case BookmarkEnd _:
                        var bookmarkEnd = docElement as BookmarkEnd;
                        foreach (var startName in bStartWithNoEnds)
                        {
                            // ReSharper disable once PossibleNullReferenceException
                            if (bookmarkEnd.Id == startName.Key)
                                outs.Add(startName.Value, bookmarkEnd);
                        }

                        break;
                }

                FindBookmarks(docElement, outs, bStartWithNoEnds);
            }

            return outs;
        }

        /// <summary>
        /// Возвращает сренерированию талицу
        /// </summary>
        /// <param name="columns">количестов столбцов</param>
        /// <param name="rows">количестов строк</param>
        /// <param name="border">тип линий ячеек</param>
        /// <returns></returns>
        private Table GenerateTable(int columns, int rows, BorderValues border = BorderValues.Apples)
        {
            Table table1 = new Table();
            // ReSharper disable once PossiblyMistakenUseOfParamsMethod
            table1.Append(GenerateTableProperties(border));
            // ReSharper disable once PossiblyMistakenUseOfParamsMethod
            table1.Append(GenerateTableGrid(columns));
            for (var i = 0; i < rows; i++)
            {
                var tablerow = new TableRow() {RsidTableRowAddition = "00763B81", RsidTableRowProperties = "00763B81"};
                for (var count = 0; count < columns; count++)
                {
                    // ReSharper disable once PossiblyMistakenUseOfParamsMethod
                    tablerow.Append(GenerateTableCell());
                }

                // ReSharper disable once PossiblyMistakenUseOfParamsMethod
                table1.Append(tablerow);
            }

            return table1;
        }

        /// <summary>
        /// Возвращает сгенерированную яцейку для талицы
        /// </summary>
        /// <returns></returns>
        private static TableCell GenerateTableCell()
        {
            TableCell tableCell1 = new TableCell();

            TableCellProperties tableCellProperties1 = new TableCellProperties();
            TableCellWidth tableCellWidth1 = new TableCellWidth() {Type = TableWidthUnitValues.Auto};

            // ReSharper disable once PossiblyMistakenUseOfParamsMethod
            tableCellProperties1.Append(tableCellWidth1);
            Paragraph paragraph1 =
                new Paragraph() {RsidParagraphAddition = "00763B81", RsidRunAdditionDefault = "00763B81"};

            // ReSharper disable once PossiblyMistakenUseOfParamsMethod
            tableCell1.Append(tableCellProperties1);
            // ReSharper disable once PossiblyMistakenUseOfParamsMethod
            tableCell1.Append(paragraph1);
            return tableCell1;
        }

        /// <summary>
        /// Возращает сгенерированный TableGrid
        /// </summary>
        /// <param name="columns">Количество столбцов</param>
        /// <returns></returns>
        private static TableGrid GenerateTableGrid(int columns)
        {
            TableGrid tableGrid1 = new TableGrid();

            for (int i = 0; i < columns; i++)
            {
                // ReSharper disable once PossiblyMistakenUseOfParamsMethod
                tableGrid1.Append(new GridColumn() {Width = "222"});
            }

            return tableGrid1;
        }

        /// <summary>
        /// Возвращает сгенерированые свойства для таблицы
        /// </summary>
        /// <param name="border">Вид границ яцеек, по умолчанию обычные линии</param>
        /// <returns></returns>
        private TableProperties GenerateTableProperties(BorderValues border = BorderValues.Apples)
        {
            var tableProperties1 = new TableProperties();
            var tableStyle1 = new TableStyle() {Val = "a3",};
            var tableWidth1 = new TableWidth() {Type = TableWidthUnitValues.Auto};
            var tableLook1 = new TableLook()
            {
                Val = "04A0",
                FirstRow = true,
                LastRow = false,
                FirstColumn = true,
                LastColumn = false,
                NoHorizontalBand = false,
                NoVerticalBand = true
            };
            var tableBorders = new TableBorders(
                new TopBorder() {Val = new EnumValue<BorderValues>(border)},
                new BottomBorder() {Val = new EnumValue<BorderValues>(border)},
                new LeftBorder() {Val = new EnumValue<BorderValues>(border)},
                new RightBorder() {Val = new EnumValue<BorderValues>(border)},
                new InsideHorizontalBorder() {Val = new EnumValue<BorderValues>(border)},
                new InsideVerticalBorder() {Val = new EnumValue<BorderValues>(border)}
            );


            // ReSharper disable once PossiblyMistakenUseOfParamsMethod
            tableProperties1.Append(tableWidth1);
            // ReSharper disable once PossiblyMistakenUseOfParamsMethod
            tableProperties1.Append(tableBorders);
            // ReSharper disable once PossiblyMistakenUseOfParamsMethod
            tableProperties1.Append(tableStyle1);
            // ReSharper disable once PossiblyMistakenUseOfParamsMethod
            tableProperties1.Append(tableLook1);
            return tableProperties1;
        }
    }
}