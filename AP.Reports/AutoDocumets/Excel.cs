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

        private delegate void CellOperator(IXLCell cell, IXLWorksheet worksheet);

        #region ctors
        public Excel()
            : this(new string[] { "Лист1" })
        {
        }

        public Excel(string[] sheets)
        {
            _workbook = new XLWorkbook();
            for (int i = 0; i < sheets.Length; i++)
            {
                _workbook.Worksheets.Add(sheets[i]);
            }
        }
        #endregion


        #region IGraphsReport

        public void Close()
        {
            _workbook.Dispose();
        }

        public void FillsTableToBookmark(string bm, DataTable dt, bool del = false, ConditionalFormatting cf = default(ConditionalFormatting))
        {
            throw new NotImplementedException();
        }

        public void FindStringAndAllReplace(string sFind, string sReplace) //ok
        {
            FindCellAndDo(
                sFind,
                (cell, worksheet) => { cell.Value = Regex.Replace(cell.Value.ToString(), @"\b" + sFind + @"\b", sReplace); },
                true);
        }

        public void FindStringAndReplace(string sFind, string sReplace) //ok
        {
            FindCellAndDo(
                sFind,
                (cell, worksheet) => { cell.Value = Regex.Replace(cell.Value.ToString(), @"\b" + sFind + @"\b", sReplace); },
                false);
        }

        public void FindStringAndAllReplaceImage(string sFind, Bitmap image) //ok
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

        public void FindStringAndReplaceImage(string sFind, Bitmap image) //ok
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
            throw new NotImplementedException();
        }

        public void InsertImageToBookmark(string bm, Bitmap image) //ok
        {
            if (_workbook != null)
            {
                IXLCell cell =_workbook.Cell(bm);
                cell.Value = "";
                cell.Worksheet.AddPicture(image).MoveTo(cell);
            }
        }

        public void InsertNewTableToBookmark(string bm, DataTable dt, ConditionalFormatting cf = default(ConditionalFormatting))
        {
            if (_workbook != null)
            {
                IXLCell cell = _workbook.Cell(bm);
                cell.Value = "";
                var range = cell.InsertData(dt.AsEnumerable());
                //range.Con
            }
        }

        public void InsertText(string text)
        {
            throw new NotImplementedException();
        }

        public void InsertTextToBookmark(string bm, string text) //ok
        {
            if (_workbook != null)
            {
                IXLCell cell = _workbook.Cell(bm);
                cell.Value = text;
            }
        }

        public void MergeDocuments(string pathdoc)
        {
            throw new NotImplementedException();
        }

        public void NewDocument()   //???
        {
            throw new NotImplementedException();
        }

        public void NewDocumentTemp(string templatePath)   //??
        {
            throw new NotImplementedException();
        }

        public void OpenDocument(string sPath)
        {
            try
            {
                _workbook = new XLWorkbook(sPath);
                _filePath = sPath;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void Save()
        {
            if (_filePath == null)
            {
                var deal = new FolderBrowserDialog();
                var result = deal.ShowDialog();
                if (result == DialogResult.OK)
                {
                    _workbook.SaveAs(deal.SelectedPath);
                }
            }
            else
            {
                _workbook.SaveAs(_filePath);
            }
        }

        public void SaveAs(string pathToSave)
        {
            _workbook.SaveAs(pathToSave);
            _filePath = pathToSave;
        }

        public void MergeDocuments(IEnumerable<string> pathdoc)
        {
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

        public void Dispose()
        {
            _workbook.Dispose();
        }

    }
}