using AP.Reports.Interface;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using Pic = DocumentFormat.OpenXml.Drawing.Pictures;
using DocumentFormat.OpenXml.Office2010.Word.DrawingShape;
using BottomBorder = DocumentFormat.OpenXml.Wordprocessing.BottomBorder;
using ConditionalFormatting = AP.Reports.Utils.ConditionalFormatting;
using GridColumn = DocumentFormat.OpenXml.Wordprocessing.GridColumn;
using InsideHorizontalBorder = DocumentFormat.OpenXml.Wordprocessing.InsideHorizontalBorder;
using InsideVerticalBorder = DocumentFormat.OpenXml.Wordprocessing.InsideVerticalBorder;
using LeftBorder = DocumentFormat.OpenXml.Wordprocessing.LeftBorder;
using NonVisualGraphicFrameDrawingProperties = DocumentFormat.OpenXml.Drawing.Wordprocessing.NonVisualGraphicFrameDrawingProperties;
using Paragraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using Picture = DocumentFormat.OpenXml.Drawing.Picture;
using RightBorder = DocumentFormat.OpenXml.Wordprocessing.RightBorder;
using Run = DocumentFormat.OpenXml.Wordprocessing.Run;
using Table = DocumentFormat.OpenXml.Wordprocessing.Table;
using TableCell = DocumentFormat.OpenXml.Wordprocessing.TableCell;
using TableCellProperties = DocumentFormat.OpenXml.Wordprocessing.TableCellProperties;
using TableGrid = DocumentFormat.OpenXml.Wordprocessing.TableGrid;
using TableProperties = DocumentFormat.OpenXml.Wordprocessing.TableProperties;
using TableRow = DocumentFormat.OpenXml.Wordprocessing.TableRow;
using TableStyle = DocumentFormat.OpenXml.Wordprocessing.TableStyle;
using Text = DocumentFormat.OpenXml.Wordprocessing.Text;
using TopBorder = DocumentFormat.OpenXml.Wordprocessing.TopBorder;
using AP.Reports.Utils;
using System.Data;
using System.Windows.Forms;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Spreadsheet;
using BlipFill = DocumentFormat.OpenXml.Drawing.BlipFill;
using Drawing = DocumentFormat.OpenXml.Wordprocessing.Drawing;
using NonVisualDrawingProperties = DocumentFormat.OpenXml.Drawing.NonVisualDrawingProperties;
using ShapeProperties = DocumentFormat.OpenXml.Drawing.ShapeProperties;

namespace AP.Reports.AutoDocumets
{
    public class Word : ITextGraphicsReport
    {
        #region Fields
        private OpenXmlElement _setElement;
        private string _path;
        private WordprocessingDocument _document;
        private Stream _stream;
        private OpenSettings _openSettings;
        private readonly bool _homeDocument = false;
        #endregion

        #region Properties
        /// <summary>
        /// Выделенный элемент документа
        /// </summary>
        public OpenXmlElement SetElement
        {
            get
            {
                if(_setElement == null)
                {
                    _setElement = _document?.MainDocumentPart.Document.Descendants<Paragraph>().Last();
                }
                return _setElement;
            }
            private set
            {
                _setElement = value;
            }
        }
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
        #endregion

        public Word()
        {

        }
        public Word(Stream stream)
        {
            _stream = stream;
            Init();
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
            foreach(var text in document.Descendants<Text>())
            {
                if(Regex.IsMatch(sFind, @"(^|\s)" + sFind + "(^|$)"))
                {
                    text.Text = text.Text.Replace(sFind, sReplace);
                }
            }
        }
        public void FindStringAndReplace(string sFind, string sReplace)
        {
            var document = _document.MainDocumentPart.Document;
            foreach(var text in document.Descendants<Text>())
            {
                if(Regex.IsMatch(sFind, @"(^|\s)" + sFind + "(^|$)"))
                {
                    text.Text = text.Text.Replace(sFind, sReplace);
                    return;
                }
            }
        }
        public void NewDocument()
        {
            _document = null;
            _path = null;
            _stream = null;
            Init();
            //Фактически Init() не создает новый документ (если путь не равен null то программа откроет
            //ранее открытый документ).т.е. при
            //    OpenDocument(@"C:\example.docx");
            //    Save();
            //    NewDocument();
            //функция не приведет к созданию документа.
            //Поэтому добавил обнуление 
        }

        public void OpenDocument(string sPath)
        {
            Path = sPath;
            Init();
        }

        public void MergeDocuments(string pathdoc)
        {
            if(_document == null)
            {
                return;
            }
            if(!ValidPath(pathdoc))
            {
                throw new FormatException("Недопустимый формат файла.", new Exception());
            }
            string altChunId = null;
            var mainPart = _document.MainDocumentPart;

            AlternativeFormatImportPart chumk = null;
            bool idIsUnique = false;
            while(!idIsUnique)
            {
                try
                {
                    altChunId = "alt" + (pathdoc.GetHashCode() ^ new Random().Next());
                    chumk = mainPart.AddAlternativeFormatImportPart(AlternativeFormatImportPartType.WordprocessingML,
                        altChunId);
                    idIsUnique = true;
                }
                catch(Exception)
                {
                    Thread.Sleep(100);
                }
            }
            try
            {
                using(var fileStream = File.Open(pathdoc, FileMode.Open))
                {
                    chumk.FeedData(fileStream);
                }
            }
            catch(IOException ex)
            {
                throw new IOException("Невозможно открыть файл для объединения", ex);
            }
            var altChunk = new AltChunk { Id = altChunId };
            mainPart.Document.Body.InsertAfter(altChunk,
                mainPart.Document.Body.Elements<Paragraph>().Last());
            _document.Save();
        }
        public void MergeDocuments(IEnumerable<string> pathdoc)
        {
            foreach(var path in pathdoc.Reverse())
            {
                MergeDocuments(path);
            }
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
            var imagePart = _document.MainDocumentPart.AddImagePart(ImagePartType.Jpeg);
            image.Save(imagePart.GetStream(FileMode.Open), System.Drawing.Imaging.ImageFormat.Jpeg);
            AddImage(_document.MainDocumentPart.GetIdOfPart(imagePart), scale);
        }

        public void NewDocumentTemp(string templatePath)
        {
            throw new NotImplementedException();
        }

        public void FillsTableToBookmark(string bm, DataTable dt, bool del = false,
            ConditionalFormatting cf = default(ConditionalFormatting))
        {
            var bookMarks = FindBookmarks();
            foreach(var bookMark in bookMarks)
            {
                if(!bookMark.Key.Contains(bm))
                {
                    continue;
                }
                SetElement = bookMark.Value.Parent;
                FillingTable((Table)SetElement, dt, cf);
                break;
            }
        }

        public void InsertNewTableToBookmark(string bm, DataTable dt,
            ConditionalFormatting cf = default(ConditionalFormatting))
        {
            var bookMarks = FindBookmarks();
            foreach(var bookMark in bookMarks)
            {
                if(!bookMark.Key.Contains(bm))
                {
                    continue;
                }
                var runElement = new Run();
                bookMark.Value.InsertAfterSelf(runElement);
                SetElement = bookMark.Value.Parent.Descendants<Run>().First();
                InsertTable(dt, cf);
                break;
            }
        }

        public void InsertTextToBookmark(string bm, string text)
        {
            var bookMarks = FindBookmarks();
            foreach(var bookMark in bookMarks)
            {
                if(!bookMark.Key.Contains(bm))
                {
                    continue;
                }
                var runElement = new Run();
                bookMark.Value.InsertAfterSelf(runElement);
                SetElement = bookMark.Value.Parent.Descendants<Run>().First();
                InsertText(text);
                break;
            }
        }

        public void Save()
        {
            _document.Save();
        }

        public void InsertText(string text)
        {
            //if(_homeDocument)
            //{
            //    SetElement.FirstChild.InsertAfterSelf((new Run(new Text(text))));
            //}
            if(SetElement.Descendants<Text>()?.Count() <= 0)
            {
                SetElement.AppendChild((new Run(new Text(text))));
            }

            //throw new NotImplementedException();
        }

        public void InsertTable(DataTable dt,
            ConditionalFormatting cf = default(ConditionalFormatting))
        {
            var table = GenerateTable(dt.Columns.Count, dt.Rows.Count);

            // ReSharper disable once PossiblyMistakenUseOfParamsMethod
            _document.MainDocumentPart.Document.Body.Append(table);
            FillingTable(table, dt, cf);
            SetConditionalFormatting(table, dt, cf);
            //Добавляем пустой параграф, чтобы таблицы не "слипались"
            var paragraph1 = new Paragraph()
            {
                RsidParagraphAddition = "00252BDD",
                RsidRunAdditionDefault = "00252BDD"
            };
            // ReSharper disable once PossiblyMistakenUseOfParamsMethod
            _document.MainDocumentPart.Document.Body.Append(paragraph1);
            //throw new NotImplementedException();
        }

        public void MoveEnd()
        {
            MoveToDocument();
            // ReSharper disable once PossiblyMistakenUseOfParamsMethod
            SetElement.Append(new Paragraph());
            SetElement = SetElement.Descendants<Paragraph>().Last();
        }

        public void MoveHome()
        {
            MoveToDocument();
            SetElement = SetElement.Descendants<Paragraph>().First();
            //throw new NotImplementedException();
        }
        #endregion  

        #region public methods
        /// <summary>
        /// Возвращает все закладки в документе
        /// </summary>
        public Dictionary<string, BookmarkEnd> FindBookmarks()
        {
            return FindBookmarks(_document.MainDocumentPart.Document);
        }

        /// <summary>
        /// Заполняет таблицу
        /// </summary>
        /// <param name="tab">Принемает заполняемую таблицу</param>
        /// <param name="dt">Принимает таблицу с данными</param>
        public void FillingTable(Table tab, DataTable dt,
            ConditionalFormatting cf = default(ConditionalFormatting))
        {
            var rowsRef = tab.Descendants<TableRow>().Count();
            //if(columsRef < dt.Columns.Count)
            //{
            //    // ReSharper disable once PossiblyMistakenUseOfParamsMethod
            //    tab.Append(GenerateTableGrid(dt.Columns.Count - columsRef));
            //}
            if(rowsRef < dt.Rows.Count)
            {
                tab = generateRow(tab, dt.Rows.Count - rowsRef);
            }
            var a = 0;
            for(var i = 0; i < tab.Descendants<TableRow>().Count(); i++)
            {
                for(var j = 0; j < tab.Descendants<GridColumn>().Count(); j++)
                {
                    try
                    {
                        SetElement = tab.Descendants<TableCell>().ToArray()[a].Descendants<Paragraph>().First();
                        InsertText(dt.Rows[i][j].ToString());

                        if(tab.Descendants<TableCell>().ToArray()[a].Descendants<GridSpan>().Any())
                        {
                            j += tab.Descendants<TableCell>().ToArray()[a].Descendants<GridSpan>().First().Val - 1;
                        }
                    }
                    catch(Exception)
                    {
                        // ignored
                    }
                    a++;
                }
            }
            //throw new NotImplementedException();
        }
        #endregion


        #region private methods



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

            var allRows = tab.Descendants<TableRow>();

            //string columnList = null;
            foreach(var row in allRows)
            {
                var cellArray = row.Descendants<TableCell>().ToArray();
                if(DoesItNeedToSetCondition(cellArray[formatingColumn].InnerText, cf))
                {
                    if(cf.Region == ConditionalFormatting.RegionAction.Cell)
                    {
                        SetBackgroungColorToCell(cellArray[formatingColumn], GetColorRrggbbString(cf.Color));
                    }
                    if(cf.Region == ConditionalFormatting.RegionAction.Row)
                    {
                        foreach(var cell in cellArray)
                        {
                            SetBackgroungColorToCell(cell, GetColorRrggbbString(cf.Color));
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Добавляет изображение
        /// </summary>
        /// <param name="relationshipId">ID ImagePart, содержащей рисунок</param>
        /// <param name="scale">Масштаб отрисовки</param>
        /// <returns></returns>
        private void AddImage(string relationshipId, float scale = 1)
        {
            //Получаем Часть с изображением
            ImagePart imagePart = (ImagePart)_document.MainDocumentPart.GetPartById(relationshipId);
            //Получаем изображение из части
            Bitmap image = new Bitmap(imagePart.GetStream());
            //Масштабируем
            int coef = 9524; //Получено империческим пересчетом с реальных примеров
            Int64Value ImageX = (Int64Value)(image.Width * scale * coef);
            Int64Value ImageY = (Int64Value)(image.Height * scale * coef);

            var element =
               new Drawing(
                   new DW.Inline(
                       new DW.Extent() { Cx = ImageX, Cy = ImageY },
                       new DW.EffectExtent()
                       {
                           LeftEdge = 0L,
                           TopEdge = 0L,
                           RightEdge = 0L,
                           BottomEdge = 0L
                       },
                       new DW.DocProperties()
                       {
                           Id = (UInt32Value)1U,
                           Name = "Picture 1"
                       },
                       new NonVisualGraphicFrameDrawingProperties(
                           new A.GraphicFrameLocks() { NoChangeAspect = true }
                           ),
                       new A.Graphic(
                           new A.GraphicData(
                               new Pic.Picture(
                                   new Pic.NonVisualPictureProperties(
                                       new Pic.NonVisualDrawingProperties()
                                       {
                                           Id = (UInt32Value)1U,
                                           Name = "New Image.Jpg"
                                       },
                                       new Pic.NonVisualPictureDrawingProperties()
                                       ),
                                   new Pic.BlipFill(
                                       new A.Blip(
                                           new A.BlipExtensionList(
                                               new A.BlipExtension()
                                               {
                                                   Uri = "{28A0092B-C50C-407E-A947-70E740481C1C}"

                                               }))
                                       {
                                           Embed = relationshipId,
                                           CompressionState = A.BlipCompressionValues.Print

                                       },
                                   new A.Stretch(new A.FillRectangle())),
                               new Pic.ShapeProperties(
                                   new A.Transform2D(
                                       new A.Offset() { X = 0L, Y = 0L },
                                       new A.Extents() { Cx = ImageX, Cy = ImageY }
                                       ),
                                   new A.PresetGeometry(
                                       new A.AdjustValueList()
                                       )
                                   {
                                       Preset = A.ShapeTypeValues.Rectangle
                                   }
                                   )))
                           {
                               Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture"
                           }))
                   {
                       DistanceFromTop = (UInt32Value)0U,
                       DistanceFromBottom = (UInt32Value)0U,
                       DistanceFromLeft = (UInt32Value)0U,
                       DistanceFromRight = (UInt32Value)0U,
                       EditId = "50D07946"
                   });

            _document?.MainDocumentPart.Document.Body.AppendChild(new Paragraph(new Run(element)));
        }

        /// <summary>
        /// Преобразует цвет в строку типа RRGGBB
        /// </summary>
        /// <param name="color">Цвет</param>
        /// <returns></returns>
        private string GetColorRrggbbString(System.Drawing.Color color)
        {
            return color.R.ToString("x2") + color.G.ToString("x2") + color.B.ToString("x2");

        }

        /// <summary>
        /// Инициализация документа
        /// </summary>
        private void Init()
        {
            _openSettings = new OpenSettings { AutoSave = false };
            if(Path != null)
            {
                try
                {
                    _document = WordprocessingDocument.Open(Path, true, _openSettings);
                }
                catch(FileFormatException e)
                {
                    throw new FileFormatException("Файл пуст", e);
                }
            }
            else if(_stream != null)
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
                mainPart.Document.Body.AppendChild(new Paragraph());
            }
        }
        /// <summary>
        /// Переходит на Document (самый верхний элемент документа)
        /// </summary>
        private void MoveToDocument()
        {
            while(SetElement.Parent != null)
            {
                SetElement = SetElement.Parent;
            }
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
            if(outs == null)
            {
                outs = new Dictionary<string, BookmarkEnd>();
            }
            if(bStartWithNoEnds == null)
            {
                bStartWithNoEnds = new Dictionary<string, string>();
            }
            foreach(var docElement in documentPart.Elements())
            {
                switch(docElement)
                {
                    case BookmarkStart _:
                        var bookmarkStart = docElement as BookmarkStart;
                        // ReSharper disable once PossibleNullReferenceException
                        bStartWithNoEnds.Add(bookmarkStart.Id, bookmarkStart.Name);
                        break;
                    case BookmarkEnd _:
                        var bookmarkEnd = docElement as BookmarkEnd;
                        foreach(var startName in bStartWithNoEnds)
                        {
                            // ReSharper disable once PossibleNullReferenceException
                            if(bookmarkEnd.Id == startName.Key)
                            {
                                outs.Add(startName.Value, bookmarkEnd);
                            }
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
            return generateRow(table1, rows);
        }
        private Table generateRow(Table table, int rows)
        {
            var colums = table.Descendants<GridColumn>().Count();
            for(var i = 0; i < rows; i++)
            {
                var tablerow = new TableRow() { RsidTableRowAddition = "00763B81", RsidTableRowProperties = "00763B81" };
                for(var count = 0; count < colums; count++)
                {
                    // ReSharper disable once PossiblyMistakenUseOfParamsMethod
                    tablerow.Append(GenerateTableCell());
                }
                // ReSharper disable once PossiblyMistakenUseOfParamsMethod
                table.Append(tablerow);
            }
            return table;
        }

        /// <summary>
        /// Возвращает сгенерированную ячейку для талицы
        /// </summary>
        /// <returns></returns>
        private static TableCell GenerateTableCell()
        {
            var tableCell1 = new TableCell();

            var tableCellProperties1 = new TableCellProperties();

            var tableCellWidth1 = new TableCellWidth() { Type = TableWidthUnitValues.Auto };
            // ReSharper disable once PossiblyMistakenUseOfParamsMethod
            tableCellProperties1.Append(tableCellWidth1);
            var paragraph1 = new Paragraph() { RsidParagraphAddition = "00763B81", RsidRunAdditionDefault = "00763B81" };
            var paragraphProperties1 = new ParagraphProperties();
            var spacingBetweenLines1 = new SpacingBetweenLines() { After = "0" };
            var justification1 = new Justification() { Val = JustificationValues.Center };
            var tableCellVerticalAlignment1 = new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center };

            // ReSharper disable once PossiblyMistakenUseOfParamsMethod
            paragraphProperties1.Append(spacingBetweenLines1);
            // ReSharper disable once PossiblyMistakenUseOfParamsMethod
            paragraphProperties1.Append(justification1);
            // ReSharper disable once PossiblyMistakenUseOfParamsMethod
            paragraphProperties1.Append(tableCellVerticalAlignment1);
            // ReSharper disable once PossiblyMistakenUseOfParamsMethod
            tableCell1.Append(tableCellProperties1);
            // ReSharper disable once PossiblyMistakenUseOfParamsMethod
            paragraph1.Append(paragraphProperties1);
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
            var tableGrid1 = new TableGrid();
            for(int i = 0; i < columns; i++)
            {
                // ReSharper disable once PossiblyMistakenUseOfParamsMethod
                tableGrid1.Append(new GridColumn() /*{ Width = ((int)(4000 / columns)).ToString() }*/);
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
            var tableStyle1 = new TableStyle() { Val = "a3", };
            var tableWidth1 = new TableWidth() { Width = "5000", Type = TableWidthUnitValues.Pct };
            var tableJustification1 = new TableJustification() { Val = TableRowAlignmentValues.Center };
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
                new TopBorder() { Val = new EnumValue<BorderValues>(border) },
                new BottomBorder() { Val = new EnumValue<BorderValues>(border) },
                new LeftBorder() { Val = new EnumValue<BorderValues>(border) },
                new RightBorder() { Val = new EnumValue<BorderValues>(border) },
                new InsideHorizontalBorder() { Val = new EnumValue<BorderValues>(border) },
                new InsideVerticalBorder() { Val = new EnumValue<BorderValues>(border) }
            );
            // ReSharper disable once PossiblyMistakenUseOfParamsMethod
            tableProperties1.Append(tableWidth1);
            // ReSharper disable once PossiblyMistakenUseOfParamsMethod
            tableProperties1.Append(tableBorders);
            // ReSharper disable once PossiblyMistakenUseOfParamsMethod
            tableProperties1.Append(tableStyle1);
            // ReSharper disable once PossiblyMistakenUseOfParamsMethod
            tableProperties1.Append(tableLook1);
            // ReSharper disable once PossiblyMistakenUseOfParamsMethod
            tableProperties1.Append(tableJustification1);
            return tableProperties1;
        }

        /// <summary>
        /// Возвращает результат проверки пути к файлу
        /// </summary>
        /// <param name="path">Проверяемый путь</param>
        /// <returns>Возвращает true если путь и формат файла допустимы</returns>
        private bool ValidPath(string path)
        {
            var extension = System.IO.Path.GetExtension(path);
            if(extension == "")
                throw new FileNotFoundException("Путь к файлу не указан.");
            foreach(var ff in Enum.GetValues(typeof(FileFormat)))
            {
                if(extension.Equals("." + ff.ToString().ToLower()))
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Определяет, соответствует ли ячейка таблицы условию из набора правил ConditionalFormatting
        /// </summary>
        /// <param name="cellValue">Значение для сравнения</param>
        /// <param name="conditional">Правила сравнения</param>
        private bool DoesItNeedToSetCondition(string cellValue, ConditionalFormatting conditional)
        {
            double conditionValue;//Значение value из conditional, перепарсенное в double
            double tableValue;//Сравниваемое значение из таблицы, перепарсенное в double
            //Если оба данных - числа, то сравниваем их как числа
            if(double.TryParse(conditional.Value, out conditionValue)
                && double.TryParse(cellValue, out tableValue))
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

        /// <summary>
        /// Устанавливает заливку ячейки
        /// </summary>
        /// <param name="cell">Ячейка таблицы</param>
        /// <param name="color">Цвет в формате RRGGBB</param>
        private void SetBackgroungColorToCell(TableCell cell, string color)
        {
            var tableCellProperties = cell.Descendants<TableCellProperties>().First();
            var shading1 = new Shading()
            {
                Val = ShadingPatternValues.Clear,
                Color = "auto",
                Fill = color
            };
            // ReSharper disable once PossiblyMistakenUseOfParamsMethod
            tableCellProperties.Append(shading1);
        }

        #endregion

    }
}