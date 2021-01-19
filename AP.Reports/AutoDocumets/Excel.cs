using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AP.Reports.Helps;
using AP.Reports.Interface;
using AP.Reports.Utils;
using AP.Utils.Data;
using ClosedXML.Excel;

namespace AP.Reports.AutoDocumets
{
    public class Excel : Document, IMsOfficeReport, IExcel, IDisposable
    {
        public enum FileFormat
        {
            /// <summary>
            ///     Документ
            /// </summary>
            [AP.Utils.Data.StringValue("xlsx")] Xlsx
        }

        private IXLCell _currentCell;
        private string _filePath;
        private XLWorkbook _workbook;

        public void Dispose()
        {
            Close();
            _workbook?.Dispose();
        }

        public SheetOption SheetOption { get; set; }
        public Array Formats => Enum.GetValues(typeof(FileFormat));

        /// <inheritdoc />
        public string Path
        {
            get => _filePath;
            set
            {
                if (ValidPath(value))
                    _filePath = value;
                else
                    throw new FormatException("Недопустимый формат файла.");
            }
        }

        /// <inheritdoc />
        public void InsertFieldInHeader(string code)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void InsertField(string code)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void FindStringInHeaderAndAllReplaceField(string sFind, string sCode)
        {
            XLHFPredefinedText code;
            if (Enum.TryParse(sCode, true, out code))
            {
                var headerFooter = _currentCell.Worksheet.PageSetup.Header;
                ChangeHeaderFooterTextToCode(headerFooter.Left, sFind, code);
                ChangeHeaderFooterTextToCode(headerFooter.Center, sFind, code);
                ChangeHeaderFooterTextToCode(headerFooter.Right, sFind, code);
                headerFooter = _currentCell.Worksheet.PageSetup.Footer;
                ChangeHeaderFooterTextToCode(headerFooter.Left, sFind, code);
                ChangeHeaderFooterTextToCode(headerFooter.Center, sFind, code);
                ChangeHeaderFooterTextToCode(headerFooter.Right, sFind, code);
                //_currentCell.Worksheet.Workbook.RecalculateAllFormulas();
            }
        }

        /// <inheritdoc />
        public void FindStringInHeaderAndAllReplace(string sFind, string sReplace)
        {
            var headerFooter = _currentCell.Worksheet.PageSetup.Header;
            ChangeHeaderFooterText(headerFooter.Left, sFind, sReplace);
            ChangeHeaderFooterText(headerFooter.Center, sFind, sReplace);
            ChangeHeaderFooterText(headerFooter.Right, sFind, sReplace);
            headerFooter = _currentCell.Worksheet.PageSetup.Footer;
            ChangeHeaderFooterText(headerFooter.Left, sFind, sReplace);
            ChangeHeaderFooterText(headerFooter.Center, sFind, sReplace);
            ChangeHeaderFooterText(headerFooter.Right, sFind, sReplace);
            _currentCell.Worksheet.Workbook.RecalculateAllFormulas();
        }

        /// <summary>
        ///     Добавляет закладку на текущую клетку
        /// </summary>
        public void AddBookmarkToCell(string name)
        {
            if (_workbook != null)
                if (Regex.IsMatch(name, PatternBookmark, RegexOptions.IgnoreCase) && name.Length < 254)
                    _currentCell?.Worksheet.Workbook.NamedRanges.Add(name, _currentCell.AsRange());
                else
                    throw new ArgumentException("Имя метки должно начинаться с буквы или символа подчеркивания,"
                                                + " и может содержать только буквы, цифры и символы подчеркивания");
        }

        /// <summary>
        ///     Вставляет изображение на метку и масштабирует его
        /// </summary>
        /// <param name="sFind"></param>
        /// <param name="image"></param>
        /// <param name="factor"></param>
        /// <param name="relativeToOriginal"></param>
        public void FindStringAndAllReplaceImage(string sFind, Bitmap image, double factor, bool relativeToOriginal)
        {
            FindCellAndDo(
                sFind,
                (cell, worksheet) => { InsertImageToCell(cell, image, factor, relativeToOriginal); },
                true);
        }

        /// <summary>
        ///     Вставляет изображение и масштабирует его
        /// </summary>
        /// <param name="image"></param>
        /// <param name="factor"></param>
        /// <param name="relativeToOriginal"></param>
        public void InsertImage(Bitmap image, double factor, bool relativeToOriginal)
        {
            if (_workbook != null)
            {
                if (_currentCell == null) MoveEnd();
                InsertImageToCell(_currentCell, image, factor, relativeToOriginal);
            }
        }

        /// <summary>
        ///     Переместиться в конец листа
        /// </summary>
        public void MoveSheetEnd()
        {
            if (_workbook != null) _currentCell = _currentCell?.Worksheet.LastRowUsed().RowBelow(1).FirstCell();
        }

        /// <summary>
        ///     Переместиться в начало листа
        /// </summary>
        public void MoveSheetHome()
        {
            if (_workbook != null) _currentCell = _currentCell?.Worksheet.FirstCell();
        }


        /// <summary>
        ///     Устанавливает курсор на ячейку
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="workSheet"></param>
        public void MoveToCell(int row, int column, string workSheet)
        {
            var currentWorkSheet = _workbook.Worksheet(workSheet);
            if (currentWorkSheet != null) _currentCell = currentWorkSheet.Cell(row, column);
        }

        /// <summary>
        ///     Устанавливает курсор на ячейку
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        public void MoveToCell(int row, int column)
        {
            if (_workbook != null) _currentCell = _currentCell?.Worksheet.Cell(row, column);
        }

        /// <summary>
        ///     Устанавливает курсор на ячейку
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="workSheet"></param>
        public void MoveToCell(string cell, string workSheet)
        {
            var currentWorkSheet = _workbook.Worksheet(workSheet);
            if (currentWorkSheet != null) _currentCell = currentWorkSheet.Cell(cell);
        }
        /// <summary>
        ///     Устанавливает курсор на ячейку
        /// </summary>
        /// <param name="cell"></param>
        public void MoveToCell(string cell)
        {
            if (_currentCell.Worksheet != null) _currentCell = _currentCell.Worksheet.Cell(cell);
        }

        /// <summary>
        ///     Создает новый документ с заданными названиями страниц
        /// </summary>
        /// <param name="sheets"></param>
        public void NewDocument(IEnumerable<string> sheets)
        {
            _workbook = new XLWorkbook();
            foreach (var sheet in sheets) _workbook.Worksheets.Add(sheet);
            MoveEnd();
        }

        /// <summary>
        ///     Осуществляет автоподбор высоты ячейки
        /// </summary>
        /// <param name="cell"></param>
        private void AutoHeightToMergedCell(IXLCell cell)
        {
            return;
            //Функция не реализована ввиду бага библиотеки ClosedXML - функция Row.AdjustToContents()
            //не учитывает свойство Style.Alignment.WrapText = true, то есть 
            //автоподбор высоты ячейки всегда изменяет высоту для отображения ОДНОЙ строки текста.
            if (!cell.IsMerged()) return;
            if (!IsItFirstCellOfMergedRange(cell)) return;
            if (cell.Address.ToString() != "K15") return;
            var mergetRegion = GetMergeRange(cell);
            var savedWidth = cell.Worksheet.Column(cell.Address.ColumnNumber).Width;
            double widthOfMergedRegion = 0;
            foreach (var column in mergetRegion.Columns())
                widthOfMergedRegion += cell.Worksheet.Column(column.ColumnNumber()).Width;
            cell.Worksheet.Column(cell.Address.ColumnNumber).Width = widthOfMergedRegion;
            mergetRegion.Unmerge();
            mergetRegion.Style.Alignment.WrapText = true;
            cell.Worksheet.Row(cell.Address.RowNumber).ClearHeight();
            var hieght = cell.Worksheet.Row(cell.Address.RowNumber).Height;
            //cell.Worksheet.Row(cell.Address.RowNumber).AdjustToContents();
            //mergetRegion.Merge();
            //cell.Worksheet.Row(cell.Address.RowNumber).Height = hieght;
            //cell.Worksheet.Column(cell.Address.ColumnNumber).Width = savedWidth;
        }

        /// <summary>
        ///     Заменяет текст в переданном объекте колонтитула
        /// </summary>
        /// <param name="hf"></param>
        /// <param name="sFind"></param>
        /// <param name="sReplace"></param>
        private void ChangeHeaderFooterText(IXLHFItem hf, string sFind, string sReplace)
        {
            var str = hf.GetText(XLHFOccurrence.FirstPage);
            hf.Clear();
            hf.AddText(str.Replace(sFind, sReplace));
        }

        /// <summary>
        ///     Заменяет текст на преустановленный текст в переданном объекте колонтитула
        /// </summary>
        /// <param name="hf"></param>
        /// <param name="sFind"></param>
        /// <param name="prefText"></param>
        private void ChangeHeaderFooterTextToCode(IXLHFItem hf, string sFind, XLHFPredefinedText prefText)
        {
            var str = hf.GetText(XLHFOccurrence.FirstPage);
            var strs = str.Split(new[] {sFind}, StringSplitOptions.None);
            hf.Clear();
            for (var i = 0; i < strs.Length; i++)
            {
                hf.AddText(strs[i]);
                if (i != strs.Length - 1)
                    hf.AddText(prefText);
            }
        }

        /// <summary>
        ///     Ищет клетку с ключевым словом и вызывает делегат
        /// </summary>
        /// <param name="sFind"></param>
        /// <param name="cellOperator"></param>
        /// <param name="forAll"></param>
        private void FindCellAndDo(string sFind, CellOperator cellOperator, bool forAll)
        {
            if (_workbook != null)
            {
                var worksheets = _workbook.Worksheets.ToArray();
                var isOperatedAlready = false;
                foreach (var worsheet in worksheets)
                {
                    foreach (var cell in worsheet.Cells())
                    {
                        if (cell.HasFormula) continue;
                        if (!Regex.IsMatch(cell.Value.ToString(), @"\b" + sFind + @"\b")) continue;
                        cellOperator(cell, worsheet);
                        isOperatedAlready = true;
                        if (forAll == false) break;
                    }

                    if (forAll == false && isOperatedAlready) break;
                }
            }
        }

        /// <summary>
        ///     Если клетка входит в объединение - возвращает значение первой клетки объединения
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        private string GetMargedCellValue(IXLCell cell)
        {
            if (cell.IsMerged() == false) return cell.Value.ToString();
            foreach (var margedRange in cell.Worksheet.MergedRanges)
                if (margedRange.Contains(cell))
                    return margedRange.FirstCell().Value.ToString();
            return "";
        }

        /// <summary>
        ///     Возвращает регион, соответствующий смерженной области клетки
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        private IXLRange GetMergeRange(IXLCell cell)
        {
            foreach (var margedRange in cell.Worksheet.MergedRanges)
                if (margedRange.Contains(cell))
                    return margedRange;
            return cell.AsRange();
        }

        /// <summary>
        ///     Возвращает ячейку, находящуюся справа от заданной
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        private IXLCell GetRightCell(IXLCell cell)
        {
            return cell.Worksheet.Cell(
                cell.Address.RowNumber,
                GetMergeRange(cell).LastCell().Address.ColumnNumber + 1);
        }

        /// <summary>
        ///     Вставляет таблицу в область со смерженными ячейками
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        private IXLRange InsertDataWithMerge(IXLCell cell, DataTable dt)
        {
            var firstCell = cell;
            for (var i = 0; i < dt.Rows.Count; i++)
            {
                for (var j = 0; j < dt.Columns.Count; j++)
                {
                    cell.Value = dt.Rows[i][j];
                    if (j != dt.Columns.Count - 1) cell = GetRightCell(cell);
                }

                if (i != dt.Rows.Count - 1)
                    cell = cell.Worksheet.Cell(firstCell.Address.RowNumber + i + 1, firstCell.Address.ColumnNumber);
            }

            var lastCell = GetMergeRange(cell).LastCell();
            return cell.Worksheet.Range(firstCell, lastCell);
        }

        /// <summary>
        ///     Вставляет изображение на клетку
        /// </summary>
        private void InsertImageToCell(IXLCell cell, Bitmap image, double factor, bool relativeToOriginal)
        {
            cell.Value = "";
            var insertedPicture = cell.Worksheet.AddPicture(image);
            insertedPicture.MoveTo(cell);
            insertedPicture.Scale(factor, relativeToOriginal);
        }

        /// <summary>
        ///     Вставляет таблицу в клетку
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="dt"></param>
        /// <param name="cf"></param>
        private void InsertTableToCell(IXLCell cell, DataTable dt,
            ConditionalFormatting cf = default(ConditionalFormatting))
        {
            ShiftForATable(dt, ref cell); //Сдвигает строки
            MergeForATable(ref dt, cell); //Повторяет мердж, который был в первой строке
            //var range = cell.InsertData(dt);
            var range = InsertDataWithMerge(cell, dt);
            if (dt.TableName != "")
            {
                //обходим запрет на одинаковые имена
                var tableNumber = 1;
                var uniqName = string.Copy(dt.TableName);
                while (cell.Worksheet.NamedRanges.Contains(uniqName)) uniqName = dt.TableName + "_" + tableNumber++;

                cell.Worksheet.NamedRanges.Add(uniqName, range);
            }

            range.Style.Fill.BackgroundColor = XLColor.White;
            range.Style.Font.FontColor = XLColor.Black;
            range.Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);
            range.Style.Border.SetOutsideBorder(XLBorderStyleValues.Medium);

            SetCondition(range, dt.Columns.IndexOf(cf.NameColumn), cf);
        }

        /// <summary>
        ///     Является ли ячека первой в своем мердж-регионе?
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        private bool IsItFirstCellOfMergedRange(IXLCell cell)
        {
            return cell.Address.ToString() == GetMergeRange(cell).FirstCell().Address.ToString();
        }

        /// <summary>
        ///     Мержит клетки по аналогии с первой строкой
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="cell"></param>
        private void MergeForATable(ref DataTable dt, IXLCell cell)
        {
            var savedAdress = cell.Address;
            var correctedColumnsCount = dt.Columns.Count;
            for (var i = 0; i < correctedColumnsCount; i++)
                if (cell.IsMerged() == false)
                {
                }
                else
                {
                    var startColumn = 0;
                    var endColumn = 0;
                    //Находим, в какой мерж входит клетка
                    foreach (var margedRange in cell.Worksheet.MergedRanges)
                        if (margedRange.Contains(cell))
                        {
                            startColumn = margedRange.RangeAddress.FirstAddress.ColumnNumber;
                            endColumn = margedRange.RangeAddress.LastAddress.ColumnNumber;
                            break;
                        }

                    //Во всех строках таблицы мержим соответствующий столбец
                    for (var currentRow = cell.Address.RowNumber;
                        currentRow < cell.Address.RowNumber + dt.Rows.Count;
                        currentRow++)
                    {
                        var range = cell.Worksheet.Range(
                            cell.Worksheet.Cell(currentRow, startColumn),
                            cell.Worksheet.Cell(currentRow, endColumn)
                        ).Merge();
                    }

                    //Добавляем в таблицу пустые столбцы
                    i += endColumn - startColumn;
                    correctedColumnsCount += endColumn - startColumn;
                    cell = GetRightCell(cell);
                }
        }

        /// <summary>
        ///     Устанавливает заливку таблицы по набору правил
        /// </summary>
        /// <param name="range"></param>
        /// <param name="formatingColumn"></param>
        /// <param name="conditional"></param>
        private void SetCondition(IXLRange range, int formatingColumn, ConditionalFormatting conditional)
        {
            for (var i = 1; i <= range.RowCount(); i++)
                //Если оба данных - числа, то сравниваем их как числа
                if (double.TryParse(conditional.Value, out var conditionValue)
                    && double.TryParse(GetMargedCellValue(range.Cell(i, formatingColumn)), out var tableValue))
                    switch (conditional.Condition)
                    {
                        case ConditionalFormatting.Conditions.Equal:

                            if (Math.Abs(tableValue - conditionValue) < float.Epsilon)

                                if (Math.Abs(tableValue - conditionValue) < float.MinValue)

                                    SetConditionToRegion(range, i, formatingColumn, conditional);
                            break;
                        case ConditionalFormatting.Conditions.Less:
                            if (tableValue < conditionValue)
                                SetConditionToRegion(range, i, formatingColumn, conditional);
                            break;
                        case ConditionalFormatting.Conditions.LessOrEqual:
                            if (tableValue <= conditionValue)
                                SetConditionToRegion(range, i, formatingColumn, conditional);
                            break;
                        case ConditionalFormatting.Conditions.More:
                            if (tableValue > conditionValue)
                                SetConditionToRegion(range, i, formatingColumn, conditional);
                            break;
                        case ConditionalFormatting.Conditions.MoreOrEqual:
                            if (tableValue >= conditionValue)
                                SetConditionToRegion(range, i, formatingColumn, conditional);
                            break;
                        case ConditionalFormatting.Conditions.NotEqual:

                            if (Math.Abs(tableValue - conditionValue) > float.Epsilon)

                                if (Math.Abs(tableValue - conditionValue) > float.MinValue)

                                    SetConditionToRegion(range, i, formatingColumn, conditional);
                            break;
                    }
                //Если не числа - сравниваем как строки
                else
                    switch (conditional.Condition)
                    {
                        case ConditionalFormatting.Conditions.Equal:
                            if (GetMargedCellValue(range.Cell(i, formatingColumn)) == conditional.Value)
                                SetConditionToRegion(range, i, formatingColumn, conditional);
                            break;
                        case ConditionalFormatting.Conditions.NotEqual:
                            if (GetMargedCellValue(range.Cell(i, formatingColumn)) != conditional.Value)
                                SetConditionToRegion(range, i, formatingColumn, conditional);
                            break;
                    }
        }

        /// <summary>
        ///     Заливает строку или ячейку цветом выделения
        /// </summary>
        /// <param name="range"></param>
        /// <param name="row"></param>
        /// <param name="formatingColumn"></param>
        /// <param name="conditional"></param>
        private void SetConditionToRegion(IXLRange range, int row, int formatingColumn,
            ConditionalFormatting conditional)
        {
            var rangeToFormating =
                conditional.Region == ConditionalFormatting.RegionAction.Cell
                    ? range.Range(row, formatingColumn, row, formatingColumn)
                    : range.Range(row, 1, row, range.ColumnCount());

            rangeToFormating.Style.Fill.BackgroundColor = XLColor.FromColor(conditional.Select);
            
        }

        /// <summary>
        ///     Сдвигает строки для вставки таблицы
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="cell"></param>
        private void ShiftForATable(DataTable dt, ref IXLCell cell)
        {
            if (dt.Rows.Count < 2) return;
            var savedAdress = cell.Address;
            cell.Worksheet.Row(cell.Address.RowNumber).AsRange().InsertRowsBelow(dt.Rows.Count - 1);
            cell = cell.Worksheet.Cell(savedAdress);
        }


        /// <summary>
        ///     Возвращает результат проверки пути к файлу
        /// </summary>
        /// <param name="path">Проверяемый путь</param>
        /// <returns>Возвращает true если путь и формат файла допустимы</returns>
        private bool ValidPath(string path)
        {
            if (string.IsNullOrEmpty(path)) throw new FileNotFoundException("Путь к файлу не указан.");

            var extension = System.IO.Path.GetExtension(path);
            foreach (var ff in Enum.GetValues(typeof(FileFormat)))
                if (extension.Equals("." + ((Enum) ff).GetStringValue()))
                    return true;
            return false;
        }


        private delegate void CellOperator(IXLCell cell, IXLWorksheet worksheet);

        #region ctors

        public Excel()
        {
            _workbook = null;
            _filePath = null;
            _currentCell = null;
        }

        public Excel(IEnumerable<string> sheets)
        {
            NewDocument(sheets);
        }

        #endregion


        #region IGraphsReport

        /// <inheritdoc />
        public void Close()
        {
            _workbook.Dispose();
        }

        /// <inheritdoc />
        public void FillTableToBookmark(string bm, DataTable dt, bool del = false,
            ConditionalFormatting cf = default(ConditionalFormatting))
        {
            if (_workbook == null) return;
            var cell = _workbook.Cell(bm);
            if (cell != null)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                for (var j = 0; j < dt.Columns.Count; j++)
                    //Если ячейка пуста или разрешено удалять данные
                    if (cell.Worksheet.Cell(
                            cell.Address.RowNumber + i,
                            cell.Address.ColumnNumber + j).Value.ToString() == "" || del)
                        cell.Worksheet.Cell(
                            cell.Address.RowNumber + i,
                            cell.Address.ColumnNumber + j).Value = dt.Rows[i].ItemArray[j];

                var endOfRange = cell.Worksheet.Cell(
                    cell.Address.RowNumber + dt.Rows.Count,
                    cell.Address.ColumnNumber + dt.Columns.Count);
                var range = cell.Worksheet.Range(cell, endOfRange);
                SetCondition(range, dt.Columns.IndexOf(cf.NameColumn), cf);
            }
            else
            {
                throw new NullReferenceException();
            }
        }

        /// <inheritdoc />
        public void FindStringAndAllReplace(string sFind, string sReplace)
        {
            FindCellAndDo(
                sFind,
                (cell, worksheet) =>
                {
                    cell.Value = Regex.Replace(cell.Value.ToString(), PatternFindText(sFind), sReplace);
                    AutoHeightToMergedCell(cell);
                },
                true);
        }

        /// <inheritdoc />
        public void FindStringAndReplace(string sFind, string sReplace)
        {
            FindCellAndDo(
                sFind,
                (cell, worksheet) =>
                {
                    cell.Value = Regex.Replace(cell.Value.ToString(), PatternFindText(sFind), sReplace);
                },
                false);
        }

        /// <inheritdoc />
        public void FindStringAndAllReplaceImage(string sFind, Bitmap image, float scale = 1)
        {
            FindCellAndDo(
                sFind,
                (cell, worksheet) => { InsertImageToCell(cell, image, scale, true); },
                true);
        }

        /// <inheritdoc />
        public void FindStringAndReplaceImage(string sFind, Bitmap image, float scale = 1)
        {
            FindCellAndDo(
                sFind,
                (cell, worksheet) => { InsertImageToCell(cell, image, scale, true); },
                false);
        }

        /// <inheritdoc />
        public void InsertImage(Bitmap image, float scale = 1)
        {
            if (_workbook != null)
            {
                if (_currentCell == null) MoveEnd();
                InsertImageToCell(_currentCell, image, scale, true);
            }
        }

        /// <inheritdoc />
        public void InsertImageToBookmark(string bm, Bitmap image, float scale = 1)
        {
            if (_workbook != null)
            {
                var cell = _workbook.Cell(bm);
                if (cell != null)
                    InsertImageToCell(cell, image, scale, true);
                else throw new NullReferenceException();
            }
        }

        /// <inheritdoc />
        public void InsertNewTableToBookmark(string bm, DataTable dt,
            ConditionalFormatting cf = default(ConditionalFormatting))
        {
            if (_workbook != null)
            {
                var cell = _workbook.Cell(bm);
                if (cell != null)
                {
                    cell.Value = "";
                    InsertTableToCell(cell, dt, cf);
                }
                else
                {
                    throw new NullReferenceException();
                }
            }
        }

        /// <inheritdoc />
        public void InsertTable(DataTable dt, ConditionalFormatting cf = default(ConditionalFormatting))
        {
            if (_workbook != null)
            {
                if (_currentCell == null) MoveEnd();

                InsertTableToCell(_currentCell, dt, cf);
                _currentCell = _currentCell.Worksheet.LastRowUsed().RowBelow().FirstCell();
            }
        }

        /// <inheritdoc />
        public void InsertText(string text)
        {
            if (_workbook != null)
            {
                if (_currentCell == null) MoveEnd();

                if (_currentCell.Value.ToString() != "")
                {
                    var savedAdress = _currentCell.Address;
                    _currentCell.Worksheet.Row(_currentCell.Address.RowNumber).AsRange().InsertRowsAbove(1);
                    _currentCell = _currentCell.Worksheet.Cell(savedAdress);
                }

                _currentCell.Value = text;
            }
        }

        /// <inheritdoc />
        public void InsertTextToBookmark(string bm, string text)
        {
            if (_workbook != null)
            {
                var cell = _workbook.Cell(bm);
                if (cell != null)
                    cell.Value = text;
                else throw new NullReferenceException();
            }
        }

        /// <inheritdoc />
        public void MergeDocuments(string pathdoc)
        {
            if (pathdoc == null) throw new FileNotFoundException("Путь к файлу не указан.");

            if (!System.IO.Path.GetExtension(pathdoc).Equals(@".xlsx"))
                throw new FormatException("Формат файла не .xlsx");

            using (var mergeSourse = new XLWorkbook(pathdoc))
            {
                if (_workbook != null)
                {
                    var mergeSoursesWorksheets = mergeSourse.Worksheets.ToList();
                    while (mergeSoursesWorksheets.Count != 0)
                    {
                        //объявляем область для переноса
                        if (mergeSoursesWorksheets.First().LastCellUsed() != null)
                        {
                            //TODO : Функцию LOGчисло  растягивает по ячейкам
                            var rangeToCopy = mergeSoursesWorksheets.First().Range(
                                mergeSoursesWorksheets.First().Cell(1, 1),
                                mergeSoursesWorksheets.First().LastCellUsed()
                            );

                            //Определяем ячейку для вставки
                            IXLCell targetCell;
                            //Если лист с таким именем уже есть в документе
                            if (_workbook.Worksheets.Contains(mergeSoursesWorksheets.First().Name))
                            {
                                var currenlSheet = _workbook.Worksheets.Worksheet(mergeSoursesWorksheets.First().Name);
                                targetCell = currenlSheet.LastRowUsed() != null
                                    ? currenlSheet.LastRowUsed().RowBelow(1).Cell(1)
                                    : currenlSheet.Cell(1, 1);
                            }
                            //Если листа с таким же именем нет - создаем новый лист
                            else
                            {
                                targetCell = _workbook.AddWorksheet(mergeSoursesWorksheets.First().Name).Cell(1, 1);
                            }
                            //targetCell.Worksheet.PageSetup.PageOrientation = mergeSoursesWorksheets.First().Worksheet.PageSetup.PageOrientation;


                            foreach (var property in targetCell.Worksheet.PageSetup.GetType().GetProperties())
                                if (property.CanRead && property.CanWrite)
                                    mergeSoursesWorksheets.First().Worksheet.PageSetup.GetType().GetProperties()
                                        .First(q => string.Equals(q.Name, property.Name))
                                        .SetValue(targetCell.Worksheet.PageSetup,
                                            property.GetValue(mergeSoursesWorksheets.First().Worksheet.PageSetup));
                            ////копируем данные

                            //Копируем "Проверка данных"
                            foreach (var dataValidation in mergeSoursesWorksheets.First().DataValidations.ToList())
                            {
                                //добавляем условие в на лист
                                targetCell.Worksheet.DataValidations.Add(dataValidation);
                                var matchs =
                                    Regex.Matches(targetCell.Worksheet.DataValidations.Last().Value, @"[0-9]+");
                                //Сохраняем только уникальные числа
                                //Hashtable uniqMathes = new Hashtable();
                                var uniqMathes = new Dictionary<string, string>();
                                for (var i = 0; i < matchs.Count; i++)
                                    if (!uniqMathes.ContainsKey(matchs[i].Value))
                                        uniqMathes.Add(matchs[i].Value, matchs[i].Value);

                                //Упорядочиваем по убыванию
                                var uniqMathesList = uniqMathes.Values.ToList();
                                uniqMathesList.Sort();
                                uniqMathesList.Reverse();

                                foreach (var match in uniqMathesList)
                                {
                                    var replaceFrom = match;
                                    var replaceTo =
                                        (int.Parse(match) + targetCell.Address.RowNumber - 1).ToString();

                                    targetCell.Worksheet.DataValidations.Last().Value =
                                        Regex.Replace(targetCell.Worksheet.DataValidations.Last().Value,
                                            replaceFrom, replaceTo);
                                }
                            }

                            //Картинки
                            var rnd = new Random();
                            foreach (var pic in mergeSoursesWorksheets.First().Pictures.ToList())
                            {
                                var insertedPic = pic.Duplicate();
                                //обходим запрет на одинаковые имена
                                while (targetCell.Worksheet.Pictures.Contains(insertedPic.Name))
                                    insertedPic.Name = "Picture" + rnd.Next(1000);

                                insertedPic = insertedPic.CopyTo(targetCell.Worksheet);
                                //вычисляем смещение
                                var moveTo = targetCell.Worksheet.Cell(
                                    pic.TopLeftCell.Address.RowNumber + targetCell.Address.RowNumber - 1,
                                    pic.TopLeftCell.Address.ColumnNumber
                                );
                                //смещаем
                                insertedPic.MoveTo(moveTo);
                            }


                            targetCell.Value = rangeToCopy;
                            foreach(var columns in rangeToCopy.Columns())
                                targetCell.Worksheet.Column(columns.ColumnNumber()).Width = mergeSoursesWorksheets
                                    .First().Worksheet.Column(columns.ColumnNumber()).Width;
                            foreach(var columns in rangeToCopy.Rows())
                                targetCell.Worksheet.Column(columns.RowNumber()).Width = mergeSoursesWorksheets
                                    .First().Worksheet.Column(columns.RowNumber()).Width;
                            foreach(var cell in rangeToCopy.Cells())
                                targetCell.Worksheet.Cell(cell.Address).Style = mergeSoursesWorksheets
                                    .First().Worksheet.Cell(cell.Address).Style;
                        }

                        //удаляем лист из списка листов, подготовленных к копированию
                        mergeSoursesWorksheets.Remove(mergeSoursesWorksheets.First());
                    }
                }
            }
        }

        /// <inheritdoc />
        public void NewDocument()
        {
            NewDocument(new[] {"Лист1"});
        }

        /// <inheritdoc />
        public void NewDocumentTemp(string templatePath)
        {
            if (templatePath == null) throw new FileNotFoundException("Путь к файлу не указан.");

            if (!System.IO.Path.GetExtension(templatePath).Equals(@".xltx"))
                throw new FormatException("Формат файла не .xltx");

            _filePath = null;
            _workbook = new XLWorkbook(templatePath);
            MoveEnd();
        }

        /// <inheritdoc />
        public void OpenDocument(string sPath)
        {
            if (sPath == null) throw new FileNotFoundException("Путь к файлу не указан.");

            if (!System.IO.Path.GetExtension(sPath).Equals(@".xlsx"))
                throw new FormatException("Формат файла не .xlsx");

            _filePath = sPath;
            _workbook = new XLWorkbook(sPath);
            SheetOption = new SheetOption
            {
                Landscape = _workbook.Worksheets.First().PageSetup.PageOrientation == XLPageOrientation.Landscape
            };
            MoveEnd();
        }

        /// <inheritdoc />
        public void Save()
        {
            if (_filePath != null) _workbook?.SaveAs(_filePath);
        }

        /// <inheritdoc />
        public void SaveAs(string pathToSave)
        {
            if (pathToSave == null) throw new FileNotFoundException("Путь к файлу не указан.");

            if (!System.IO.Path.GetExtension(pathToSave).Equals(@".xlsx"))
                throw new FormatException("Формат файла не .xlsx");
            if (_workbook != null)
            {
                _filePath = pathToSave;
                _workbook.SaveAs(pathToSave);
            }
        }

        /// <inheritdoc />
        public void MergeDocuments(IEnumerable<string> pathdoc)
        {
            foreach (var str in pathdoc) MergeDocuments(str);
        }

        /// <inheritdoc />
        public void MoveEnd()
        {
            if (_workbook != null)
                _currentCell = _workbook.Worksheets.Last().LastRowUsed() != null
                    ? _workbook.Worksheets.Last().LastRowUsed().RowBelow(1).FirstCell()
                    : _currentCell = _workbook.Worksheets.Last().FirstCell();
        }

        /// <inheritdoc />
        public void MoveHome()
        {
            if (_workbook != null) _currentCell = _workbook.Worksheets.First().FirstCell();
        }

        #endregion

        /// <inheritdoc />
        public void MoveColumnEnd()
        {
            if (_workbook != null) _currentCell = _currentCell?.Worksheet.LastColumnUsed().FirstCell();
        }

        /// <inheritdoc />
        public void SelectRange(string leftTopCorner, string BottomRightCorner)
        {
            _currentCell.Worksheet.Range(leftTopCorner, BottomRightCorner);
        }

        /// <inheritdoc />
        public IXLCell Cell
        {
            get => _currentCell;
        }
    }
}