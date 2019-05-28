using System;
using Microsoft.Office.Interop.Excel;
using Range = Microsoft.Office.Interop.Excel.Range;
using Workbook = Microsoft.Office.Interop.Excel.Workbook;
using Worksheet = Microsoft.Office.Interop.Excel.Worksheet;

namespace AP.Reports.MSInterop
{
    public class ExcelInterop:IDisposable
    {
        #region Filed
        private Application _application;
        private Workbook _workbook;
        private Workbooks _workbooks;
        private Worksheet _worksheet;
        private Range _range;
        #endregion

        #region Property
        /// <summary>
        /// Количество объедененных столбцов
        /// </summary>
        public int MergedColumns { get; set; } = 1;
        /// <summary>
        /// Координата выделенной ячейки(левый верхний угол)
        /// </summary>
        public Point Point { get; set; }

        /// <summary>
        /// Экземпляр приложения экселя
        /// </summary>
        public Application Application { set => _application = value;
            get
            {
                if (_application==null)
                {
                    _application = new Application();
                    return _application;
                }
                else
                {
                    return _application;
                }
            }
        }
        /// <summary>
        /// Позволяет здавать и получать книгу экселя
        /// </summary>
        public Workbook Workbook
        {
            get
            {
                return _workbook ?? (_workbook = Workbooks.Count == 0 ? Workbooks.Add() : Application.ActiveWorkbook);
            }
            set
            {
                if (_application.Workbooks!=null)
                {
                    foreach (var workbook in _application.Workbooks)
                    {
                        if (Equals(workbook, value))
                        {
                            _workbook = value;
                            break;
                        }
                    }
                }
                if (_workbook!= value)
                {
                    if (_application.Workbooks != null) _workbook = _application.Workbooks.Add(value);
                }
            }
        }
        /// <summary>
        /// Позволяет получаnь коллекцию книг
        /// </summary>
        public Workbooks Workbooks
        {
            get
            {
               return Application.Workbooks;
            }
        }
        /// <summary>
        /// Лист в книге
        /// </summary>
        public Worksheet Worksheet {
            get
            {
                new NotImplementedException();
                return _worksheet; 
            }
            set
            {
                new NotImplementedException();
            }
        }
        /// <summary>
        /// Диапазон
        /// </summary>
        public Range Range
        {
            get
            {
                new NotImplementedException();
                return _range;
            }
            set
            {
                new NotImplementedException();
            }
        }
        
        #endregion
        public void Dispose()
        {
            _application.Quit();
        }
    }
}
