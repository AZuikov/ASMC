using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using AP.Reports.Interface;
using AP.Reports.Utils;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Packaging;

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

        //=========================================================
        public void FillsTableToBookmark(string bm, DataTable dt, bool del = false, ConditionalFormatting cf = default(ConditionalFormatting))
        {
            throw new NotImplementedException();
        }

        public void FindStringAndAllReplace(string sFind, string sReplace)
        {
            FindCellAndDo(
                sFind,
                (cell, worksheet) => { cell.Value = Regex.Replace(cell.Value.ToString(), @"\b" + sFind + @"\b", sReplace); },
                true);
        }

        public void FindStringAndReplace(string sFind, string sReplace)
        {
            FindCellAndDo(
                sFind,
                (cell, worksheet) => { cell.Value = Regex.Replace(cell.Value.ToString(), @"\b" + sFind + @"\b", sReplace); },
                false);
        }

        public void FindStringAndAllReplaceImage(string sFind, Bitmap image)
        {
            FindCellAndDo(
                sFind,
                (cell, worksheet) =>
                    {
                        cell.Value = "";
                        worksheet.AddPicture(image).MoveTo(cell);
                    },
                true);
        }

        public void FindStringAndReplaceImage(string sFind, Bitmap image)
        {
            FindCellAndDo(
                sFind,
                (cell, worksheet) =>
                {
                    cell.Value = "";
                    worksheet.AddPicture(image).MoveTo(cell);
                },
                false);
        }

        public void InsertImage(Bitmap image)
        {
            if (_workbook != null)
            {
                _currentCell?.Worksheet.AddPicture(image).MoveTo(_currentCell);
            }
        }

        public void InsertImageToBookmark(string bm, Bitmap image)
        {
            if (_workbook != null)
            {
                IXLCell cell =_workbook.Cell(bm);
                if (cell != null)
                {
                    cell.Value = "";
                    cell.Worksheet.AddPicture(image).MoveTo(cell);
                }
                else throw new NullReferenceException();
            }
        }

        public void InsertNewTableToBookmark(string bm, DataTable dt, ConditionalFormatting cf = default(ConditionalFormatting))
        {
            if (_workbook != null)
            {
                IXLCell cell = _workbook.Cell(bm);
                if (cell != null)
                {
                    cell.Value = "";
                    var range = cell.InsertData(dt);
                    range.Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);
                    range.Style.Border.SetOutsideBorder(XLBorderStyleValues.Medium);

                    SetCondition(range, dt.Columns.IndexOf(cf.NameColumn), cf);
                }
                else throw new NullReferenceException();
            }
        }

        public void InsertTable(DataTable dt)
        {
            throw new NotImplementedException();
        }

        public void InsertText(string text)
        {
            if (_workbook != null && _currentCell != null)
            {
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

        //??????????????????????????????????????????????????????????????????????
        public void MergeDocuments(string pathdoc)
        {
            XLWorkbook mergeSourse;
            try
            {
                mergeSourse = new XLWorkbook(pathdoc);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (_workbook != null)
            {
                var mergeSoursesWorksheets = mergeSourse.Worksheets.ToList();
                while (mergeSoursesWorksheets.Count != 0)
                {
                    var rangeToCopy = mergeSoursesWorksheets.First().Range(
                            mergeSoursesWorksheets.First().Cell(1, 1),
                            mergeSoursesWorksheets.First().LastCellUsed()
                            );
                    IXLCell targetCell;
                    if (_workbook.Worksheets.Contains(mergeSoursesWorksheets.First().Name))
                    {
                        targetCell = _workbook.Worksheets
                            .Worksheet(mergeSoursesWorksheets.First().Name)
                            .LastRowUsed().RowBelow(1).Cell(1);
                    }
                    else
                    {
                        targetCell = _workbook.AddWorksheet(mergeSoursesWorksheets.First().Name).Cell(1, 1);
                    }

                    if (targetCell == null || rangeToCopy == null)
                    {
                        MessageBox.Show("???");
                    }

                    targetCell.Value = rangeToCopy;
                    mergeSoursesWorksheets.Remove(mergeSoursesWorksheets.First());
                }
            }
        }

        public void NewDocument()
        {
            NewDocument(new string[] { "Лист1" });
        }

        public void NewDocumentTemp(string templatePath)
        {
            try
            {
                _filePath = null;
                _workbook = new XLWorkbook(templatePath);
                MoveEnd();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void OpenDocument(string sPath)
        {
            try
            {
                _filePath = sPath;
                _workbook = new XLWorkbook(sPath);
                MoveEnd();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Save()
        {
            if (_filePath != null && _workbook != null)
            {
                _workbook.SaveAs(_filePath);
            }
        }

        public void SaveAs(string pathToSave)
        {
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
                _currentCell = _workbook.Worksheets.Last().LastRowUsed().RowBelow(1).FirstCell();
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

        private void FindCellAndDo(string sFind, CellOperator cellOperator, bool forAll)
        {
            if (_workbook != null)
            {
                var worksheets = _workbook.Worksheets.ToArray();
                bool isOperatedAlready = false;
                for (int i = 0; i < worksheets.Length; i++)
                {
                    foreach (var cell in worksheets[i].Cells())
                    {
                        if (Regex.IsMatch(cell.Value.ToString(), @"\b" + sFind + @"\b"))
                        {
                            cellOperator(cell, worksheets[i]);
                            isOperatedAlready = true;
                            if (forAll == false)
                            {
                                break;
                            }
                        }
                    }

                    if (forAll == false && isOperatedAlready == true)
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
                double conditionValue;
                double tableValue;
                //Если оба данных - числа
                if(double.TryParse(conditional.Value, out conditionValue) 
                        && double.TryParse(range.Cell(i, formatingColumn).Value.ToString(), out tableValue))
                {
                    switch (conditional.Condition)
                    {
                        case ConditionalFormatting.Conditions.Equal:
                            if (tableValue == conditionValue)
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
                            if (tableValue != conditionValue)
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
                            if (range.Cell(i, formatingColumn).ToString() == conditional.Value)
                            {
                                SetConditionToRegion(range, i, formatingColumn, conditional);
                            }

                            break;
                        case ConditionalFormatting.Conditions.NotEqual:
                            if (range.Cell(i, formatingColumn).ToString() != conditional.Value)
                            {
                                SetConditionToRegion(range, i, formatingColumn, conditional);
                            }

                            break;
                    }
                }
            }
        }

        private void SetConditionToRegion(IXLRange range, int row, int formatingColumn, ConditionalFormatting conditional)
        {
            IXLRange rangeToFormating;
            if (conditional.Region == ConditionalFormatting.RegionAction.Cell)
            {
                rangeToFormating = range.Range(row, formatingColumn, row, formatingColumn);
            }
            else
            {
                rangeToFormating = range.Range(row, 1, row, range.ColumnCount());
            }
            rangeToFormating.Style.Fill.BackgroundColor = XLColor.FromColor(conditional.Color);
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

        public void Dispose()
        {
            _workbook.Dispose();
        }

    }
}