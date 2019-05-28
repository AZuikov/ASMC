using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AP.Reports.Interface;
using AP.Reports.Utils;
using ClosedXML.Excel;

namespace AP.Reports.AutoDocumets
{
    public class Excel : ITextGraphicsReport, IDisposable
    {
        private XLWorkbook _workbook;
        private string _filePath;
        private IXLCell _currentCell;

        public string Path => throw new NotImplementedException();

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

        public void Close()
        {
            _workbook.Dispose();
        }

        public void FillsTableToBookmark(string bm, DataTable dt, bool del = false,
            ConditionalFormatting cf = default(ConditionalFormatting))
        {
            if (_workbook != null)
            {
                IXLCell cell = _workbook.Cell(bm);
                if (cell != null)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        for (int j = 0; j < dt.Columns.Count; j++)
                        {
                            //Если ячейка пуста или разрешено удалять данные
                            if (cell.Worksheet.Cell(
                                    cell.Address.RowNumber + i,
                                    cell.Address.ColumnNumber + j).Value.ToString() == "" || del)
                            {
                                cell.Worksheet.Cell(
                                    cell.Address.RowNumber + i,
                                    cell.Address.ColumnNumber + j).Value = dt.Rows[i].ItemArray[j];
                            }
                        }
                    }

                    var endOfRange = cell.Worksheet.Cell(
                        cell.Address.RowNumber + dt.Rows.Count,
                        cell.Address.ColumnNumber + dt.Columns.Count);
                    var range = cell.Worksheet.Range(cell, endOfRange);
                    SetCondition(range, dt.Columns.IndexOf(cf.NameColumn), cf);
                }
                else throw new NullReferenceException();
            }
        }

        public void FindStringAndAllReplace(string sFind, string sReplace)
        {
            FindCellAndDo(
                sFind,
                (cell, worksheet) =>
                {
                    cell.Value = Regex.Replace(cell.Value.ToString(), @"\b" + sFind + @"\b", sReplace);
                },
                true);
        }

        public void FindStringAndReplace(string sFind, string sReplace)
        {
            FindCellAndDo(
                sFind,
                (cell, worksheet) =>
                {
                    cell.Value = Regex.Replace(cell.Value.ToString(), @"\b" + sFind + @"\b", sReplace);
                },
                false);
        }

        public void FindStringAndAllReplaceImage(string sFind, Bitmap image)
        {
            FindCellAndDo(
                sFind,
                (cell, worksheet) =>
                {
                    InsertImageToCell(cell, image, 1, true);
                },
                true);
        }

        public void FindStringAndReplaceImage(string sFind, Bitmap image)
        {
            FindCellAndDo(
                sFind,
                (cell, worksheet) =>
                {
                    InsertImageToCell(cell, image, 1, true);
                },
                false);
        }

        public void InsertImage(Bitmap image)
        {
            InsertImage(image, 1, true);
        }

        public void InsertImageToBookmark(string bm, Bitmap image)
        {
            if (_workbook != null)
            {
                IXLCell cell = _workbook.Cell(bm);
                if (cell != null)
                {
                    InsertImageToCell(cell, image, 1, true);
                }
                else throw new NullReferenceException();
            }
        }

        public void InsertNewTableToBookmark(string bm, DataTable dt,
            ConditionalFormatting cf = default(ConditionalFormatting))
        {
            if (_workbook != null)
            {
                IXLCell cell = _workbook.Cell(bm);
                if (cell != null)
                {
                    cell.Value = "";
                    InsertTableToCell(cell, dt, cf);
                }
                else throw new NullReferenceException();
            }
        }

        public void InsertTable(DataTable dt, ConditionalFormatting cf)
        {
            if (_workbook != null)
            {
                if (_currentCell == null)
                {
                    MoveEnd();
                }

                InsertTableToCell(_currentCell, dt, cf);
                _currentCell = _currentCell.Worksheet.LastRowUsed().RowBelow().FirstCell();
            }
        }

        public void InsertText(string text)
        {
            if (_workbook != null)
            {
                if (_currentCell == null)
                {
                    MoveEnd();
                }

                if (_currentCell.Value.ToString() != "")
                {
                    var savedAdress = _currentCell.Address;
                    _currentCell.Worksheet.Row(_currentCell.Address.RowNumber).AsRange().InsertRowsAbove(1);
                    _currentCell = _currentCell.Worksheet.Cell(savedAdress);
                }

                _currentCell.Value = text;
            }
        }

        public void InsertTextToBookmark(string bm, string text)
        {
            if (_workbook != null)
            {
                IXLCell cell = _workbook.Cell(bm);
                if (cell != null)
                {
                    cell.Value = text;
                }
                else throw new NullReferenceException();
            }
        }

        public void MergeDocuments(string pathdoc)
        {
            if (pathdoc == null)
            {
                throw new FileNotFoundException("Путь к файлу не указан.");
            }

            if (!System.IO.Path.GetExtension(pathdoc).Equals(@".xlsx"))
            {
                throw new FormatException("Формат файла не .xlsx");
            }

            using (XLWorkbook mergeSourse = new XLWorkbook(pathdoc))
            {

                if (_workbook != null)
                {
                    var mergeSoursesWorksheets = mergeSourse.Worksheets.ToList();
                    while (mergeSoursesWorksheets.Count != 0)
                    {
                        //объявляем область для переноса
                        if (mergeSoursesWorksheets.First().LastCellUsed() != null)
                        {
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
                                targetCell = (currenlSheet.LastRowUsed() != null)
                                    ? currenlSheet.LastRowUsed().RowBelow(1).Cell(1)
                                    : currenlSheet.Cell(1, 1);
                            }
                            //Если листа с таким же именем нет - создаем новый лист
                            else
                            {
                                targetCell = _workbook.AddWorksheet(mergeSoursesWorksheets.First().Name).Cell(1, 1);
                            }
                            ////копируем данные

                            //Копируем "Проверка данных"
                            foreach (var dataValidation in mergeSoursesWorksheets.First().DataValidations.ToList())
                            {
                                //добавляем условие в на лист
                                targetCell.Worksheet.DataValidations.Add(dataValidation);
                                MatchCollection matchs =
                                    Regex.Matches(targetCell.Worksheet.DataValidations.Last().Value, @"[0-9]+");
                                //Сохраняем только уникальные числа
                                //Hashtable uniqMathes = new Hashtable();
                                Dictionary<string, string> uniqMathes = new Dictionary<string, string>();
                                for (int i = 0; i < matchs.Count; i++)
                                {
                                    uniqMathes.Add(matchs[i].Value, matchs[i].Value);
                                }

                                //Упорядочиваем по убыванию
                                List<string> uniqMathesList = uniqMathes.Values.ToList();
                                uniqMathesList.Sort();
                                uniqMathesList.Reverse();

                                foreach (var match in uniqMathesList)
                                {
                                    string replaceFrom = match;
                                    string replaceTo =
                                        (Int32.Parse(match) + targetCell.Address.RowNumber - 1).ToString();

                                    targetCell.Worksheet.DataValidations.Last().Value =
                                        Regex.Replace(targetCell.Worksheet.DataValidations.Last().Value,
                                            replaceFrom, replaceTo);
                                }
                            }

                            //Картинки
                            Random rnd = new Random();
                            foreach (var pic in mergeSoursesWorksheets.First().Pictures.ToList())
                            {
                                var insertedPic = pic.Duplicate();
                                //обходим запрет на одинаковые имена
                                while (targetCell.Worksheet.Pictures.Contains(insertedPic.Name))
                                {
                                    insertedPic.Name = "Picture" + rnd.Next(1000).ToString();
                                }

                                insertedPic = insertedPic.CopyTo(targetCell.Worksheet);
                                //вычисляем смещение
                                IXLCell moveTo = targetCell.Worksheet.Cell(
                                    pic.TopLeftCell.Address.RowNumber + targetCell.Address.RowNumber - 1,
                                    pic.TopLeftCell.Address.ColumnNumber
                                );
                                //смещаем
                                insertedPic.MoveTo(moveTo);
                            }

                            //Ячейки
                            targetCell.Value = rangeToCopy;

                        }

                        //удаляем лист из списка листов, подготовленных к копированию
                        mergeSoursesWorksheets.Remove(mergeSoursesWorksheets.First());
                    }
                }
            }
        }

        public void NewDocument()
        {
            NewDocument(new [] {"Лист1"});
        }

        public void NewDocumentTemp(string templatePath)
        {
            if (templatePath == null)
            {
                throw new FileNotFoundException("Путь к файлу не указан.");
            }

            if (!System.IO.Path.GetExtension(templatePath).Equals(@".xltx"))
            {
                throw new FormatException("Формат файла не .xltx");
            }

            _filePath = null;
            _workbook = new XLWorkbook(templatePath);
            MoveEnd();
        }

        public void OpenDocument(string sPath)
        {
            if (sPath == null)
            {
                throw new FileNotFoundException("Путь к файлу не указан.");
            }

            if (!System.IO.Path.GetExtension(sPath).Equals(@".xlsx"))
            {
                throw new FormatException("Формат файла не .xlsx");
            }

            _filePath = sPath;
            _workbook = new XLWorkbook(sPath);
            MoveEnd();
        }

        public void Save()
        {
            if (_filePath != null)
            {
                _workbook?.SaveAs(_filePath);
            }
        }

        public void SaveAs(string pathToSave)
        {
            if (pathToSave == null)
            {
                throw new FileNotFoundException("Путь к файлу не указан.");
            }

            if (!System.IO.Path.GetExtension(pathToSave).Equals(@".xlsx"))
            {
                throw new FormatException("Формат файла не .xlsx");
            }
            if (_workbook != null)
            {
                _filePath = pathToSave;
                _workbook.SaveAs(pathToSave);
            }
        }

        public void MergeDocuments(IEnumerable<string> pathdoc)
        {
            foreach (var str in pathdoc)
            {
                MergeDocuments(str);
            }
        }

        public void MoveEnd()
        {
            if (_workbook != null)
            {
                _currentCell = (_workbook.Worksheets.Last().LastRowUsed() != null)?
                   _workbook.Worksheets.Last().LastRowUsed().RowBelow(1).FirstCell():
                    _currentCell = _workbook.Worksheets.Last().FirstCell();
            }
        }

        public void MoveHome()
        {
            if (_workbook != null)
            {
                _currentCell = _workbook.Worksheets.First().FirstCell();
            }
        }

        #endregion

        private void InsertTableToCell(IXLCell cell, DataTable dt, ConditionalFormatting cf)
        {
            ShiftForATable(dt, ref cell); //Сдвигает строки
            var range = cell.InsertData(dt);
            if (dt.TableName != "")
            {
                //обходим запрет на одинаковые имена
                int tableNumber = 1;
                string uniqName = string.Copy(dt.TableName);
                while (cell.Worksheet.NamedRanges.Contains(uniqName))
                {
                    uniqName = dt.TableName + "_" + (tableNumber++).ToString();
                }

                cell.Worksheet.NamedRanges.Add(uniqName, range);
            }
            range.Style.Fill.BackgroundColor = XLColor.White;
            range.Style.Font.FontColor = XLColor.Black;
            range.Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);
            range.Style.Border.SetOutsideBorder(XLBorderStyleValues.Medium);

            SetCondition(range, dt.Columns.IndexOf(cf.NameColumn), cf);
        }

        /// <summary>
        /// Ищет клетку с ключевым словом и вызывает делегат
        /// </summary>
        /// <param name="sFind"></param>
        /// <param name="cellOperator"></param>
        /// <param name="forAll"></param>
        private void FindCellAndDo(string sFind, CellOperator cellOperator, bool forAll)
        {
            if (_workbook != null)
            {
                var worksheets = _workbook.Worksheets.ToArray();
                bool isOperatedAlready = false;
                //for (int i = 0; i < worksheets.Length; i++)
                //{
                //    foreach (var cell in worksheets[i].Cells())
                foreach (var worsheet in worksheets)
                {
                    foreach (var cell in worsheet.Cells())

                    {
                        if (Regex.IsMatch(cell.Value.ToString(), @"\b" + sFind + @"\b"))
                        {
                            cellOperator(cell, worsheet);
                            isOperatedAlready = true;
                            if (forAll == false)
                            {
                                break;
                            }
                        }
                    }

                    if (forAll == false && isOperatedAlready)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Переместиться в конец листа
        /// </summary>
        public void MoveSheetEnd()
        {
            if (_workbook != null)
            {
                _currentCell = _currentCell?.Worksheet.LastRowUsed().RowBelow(1).FirstCell();
            }
        }

        /// <summary>
        /// Переместиться в начало листа
        /// </summary>
        public void MoveSheetHome()
        {
            if (_workbook != null)
            {
                _currentCell = _currentCell?.Worksheet.FirstCell();
            }
        }

        /// <summary>
        /// Устанавливает заливку таблицы по набору правил
        /// </summary>
        /// <param name="range"></param>
        /// <param name="formatingColumn"></param>
        /// <param name="conditional"></param>
        private void SetCondition(IXLRange range, int formatingColumn, ConditionalFormatting conditional)
        {
            for (int i = 1; i <= range.RowCount(); i++)
            {
                //Если оба данных - числа, то сравниваем их как числа
                if (double.TryParse(conditional.Value, out var conditionValue)
                    && double.TryParse(GetMargedCellValue(range.Cell(i, formatingColumn)), out var tableValue))
                {
                    switch (conditional.Condition)
                    {
                        case ConditionalFormatting.Conditions.Equal:
                            if ( Math.Abs(tableValue - conditionValue)<float.MinValue)
                            {
                                SetConditionToRegion(range, i, formatingColumn, conditional);
                            }
                            break;
                        case ConditionalFormatting.Conditions.Less:
                            if (tableValue < conditionValue)
                            {
                                SetConditionToRegion(range, i, formatingColumn, conditional);
                            }
                            break;
                        case ConditionalFormatting.Conditions.LessOrEqual:
                            if (tableValue <= conditionValue)
                            {
                                SetConditionToRegion(range, i, formatingColumn, conditional);
                            }
                            break;
                        case ConditionalFormatting.Conditions.More:
                            if (tableValue > conditionValue)
                            {
                                SetConditionToRegion(range, i, formatingColumn, conditional);
                            }
                            break;
                        case ConditionalFormatting.Conditions.MoreOrEqual:
                            if (tableValue >= conditionValue)
                            {
                                SetConditionToRegion(range, i, formatingColumn, conditional);
                            }
                            break;
                        case ConditionalFormatting.Conditions.NotEqual:
                            if (Math.Abs(tableValue - conditionValue) > float.MinValue)
                            {
                                SetConditionToRegion(range, i, formatingColumn, conditional);
                            }
                            break;
                    }
                }
                //Если не числа - сравниваем как строки
                else
                {
                    switch (conditional.Condition)
                    {
                        case ConditionalFormatting.Conditions.Equal:
                            if ( GetMargedCellValue(range.Cell(i, formatingColumn)) == conditional.Value)
                            {
                                SetConditionToRegion(range, i, formatingColumn, conditional);
                            }
                            break;
                        case ConditionalFormatting.Conditions.NotEqual:
                            if (GetMargedCellValue(range.Cell(i, formatingColumn)) != conditional.Value)
                            {
                                SetConditionToRegion(range, i, formatingColumn, conditional);
                            }
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Заливает строку или ячейку цветом выделения
        /// </summary>
        /// <param name="range"></param>
        /// <param name="row"></param>
        /// <param name="formatingColumn"></param>
        /// <param name="conditional"></param>
        private void SetConditionToRegion(IXLRange range, int row, int formatingColumn,
            ConditionalFormatting conditional)
        {
            IXLRange rangeToFormating = 
                (conditional.Region == ConditionalFormatting.RegionAction.Cell)?
                range.Range(row, formatingColumn, row, formatingColumn):
                range.Range(row, 1, row, range.ColumnCount());
            
            rangeToFormating.Style.Fill.BackgroundColor = XLColor.FromColor(conditional.Color);
        }

        /// <summary>
        /// Если клетка входит в объединение - возвращает значение первой клетки объединения
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        private string GetMargedCellValue(IXLCell cell)
        {
            if (cell.IsMerged() == false)
            {
                return cell.Value.ToString();
            }
            foreach (var margedRange in cell.Worksheet.MergedRanges)
            {
                if (margedRange.Contains(cell))
                {
                    return margedRange.FirstCell().Value.ToString();
                }
            }
            return "";
        }

        /// <summary>
        /// Устанавливает курсор на ячейку
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="workSheet"></param>
        public void MoveToCell(int row, int column, string workSheet)
        {
            IXLWorksheet currentWorkSheet = _workbook.Worksheet(workSheet);
            if (currentWorkSheet != null)
            {
                _currentCell = currentWorkSheet.Cell(row, column);
            }
        }

        /// <summary>
        /// Устанавливает курсор на ячейку
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        public void MoveToCell(int row, int column)
        {
            if (_workbook != null)
            {
                _currentCell = _currentCell?.Worksheet.Cell(row, column);
            }
        }

        /// <summary>
        /// Устанавливает курсор на ячейку
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="workSheet"></param>
        public void MoveToCell(string cell, string workSheet)
        {
            IXLWorksheet currentWorkSheet = _workbook.Worksheet(workSheet);
            if (currentWorkSheet != null)
            {
                _currentCell = currentWorkSheet.Cell(cell);
            }
        }

        /// <summary>
        /// Вставляет изображение и масштабирует его
        /// </summary>
        /// <param name="image"></param>
        /// <param name="factor"></param>
        /// <param name="relativeToOriginal"></param>
        public void InsertImage(Bitmap image, double factor, bool relativeToOriginal)
        {
            if (_workbook != null)
            {
                if (_currentCell == null)
                {
                    MoveEnd();
                }
                InsertImageToCell(_currentCell, image, factor, relativeToOriginal);
            }
        }

        /// <summary>
        /// Вставляет изображение на метку и масштабирует его
        /// </summary>
        /// <param name="sFind"></param>
        /// <param name="image"></param>
        /// <param name="factor"></param>
        /// <param name="relativeToOriginal"></param>
        public void FindStringAndAllReplaceImage(string sFind, Bitmap image, double factor, bool relativeToOriginal)
        {
            FindCellAndDo(
                sFind,
                (cell, worksheet) =>
                {
                    InsertImageToCell(cell, image, factor, relativeToOriginal);
                },
                true);
        }

        /// <summary>
        /// Вставляет изображение на клетку
        /// </summary>
        private void InsertImageToCell(IXLCell cell, Bitmap image, double factor, bool relativeToOriginal)
        {
            cell.Value = "";
            var insertedPicture = cell.Worksheet.AddPicture(image);
            insertedPicture.MoveTo(cell);
            insertedPicture.Scale(factor, relativeToOriginal);
        }

        /// <summary>
        /// Вставляет изображение с масштабом
        /// </summary>
        /// <param name="bm"></param>
        /// <param name="image"></param>
        /// <param name="factor"></param>
        /// <param name="relativeToOriginal"></param>
        public void InsertImageToBookmark(string bm, Bitmap image, double factor, bool relativeToOriginal)
        {
            if (_workbook != null)
            {
                IXLCell cell = _workbook.Cell(bm);
                if (cell != null)
                {
                    InsertImageToCell(cell, image, factor, relativeToOriginal);
                }
                else throw new NullReferenceException();
            }
        }

        /// <summary>
        /// Вставляет изображение с масштабом
        /// </summary>
        /// <param name="sFind"></param>
        /// <param name="image"></param>
        /// <param name="factor"></param>
        /// <param name="relativeToOriginal"></param>
        public void FindStringAndReplaceImage(string sFind, Bitmap image, double factor, bool relativeToOriginal)
        {
            FindCellAndDo(
                sFind,
                (cell, worksheet) =>
                {
                    InsertImageToCell(cell, image, factor, relativeToOriginal);
                },
                false);
        }

        /// <summary>
        /// Сдвигает строки для вставки таблицы
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="cell"></param>
        private void ShiftForATable(DataTable dt, ref IXLCell cell)
        {
            if (cell.Worksheet.Row(cell.Address.RowNumber).IsEmpty())
            {
                var savedAdress = cell.Address;
                cell.Worksheet.Row(cell.Address.RowNumber).AsRange().InsertRowsAbove(dt.Rows.Count - 1);
                cell = cell.Worksheet.Cell(savedAdress);
            }
        }

        /// <summary>
        /// Создает новый документ с заданными названиями страниц
        /// </summary>
        /// <param name="sheets"></param>
        public void NewDocument(IEnumerable<string> sheets)
        {
            _workbook = new XLWorkbook();
            foreach (var sheet in sheets)
            {
                _workbook.Worksheets.Add(sheet);
            }
            MoveEnd();
        }

        /// <summary>
        /// Добавляет закладку на текущую клетку
        /// </summary>
        public void AddBookmarkToCell(string name)
        {
            if (_workbook != null)
            {
                string pattern = @"[_, a-z, а-я][_,a-z,а-я,0-9]*";
                if (Regex.IsMatch(name, pattern, RegexOptions.IgnoreCase) && name.Length<254)
                {
                    _currentCell?.Worksheet.Workbook.NamedRanges.Add(name, _currentCell.AsRange());
                }
                else
                {
                    throw new ArgumentException("Имя метки должно начинаться с буквы или символа подчеркивания,"
                    + " и может содержать только буквы, цифры и символы подчеркивания");
                }
            }
        }


        public void Dispose()
        {
            _workbook?.Dispose();
        }

    }
}