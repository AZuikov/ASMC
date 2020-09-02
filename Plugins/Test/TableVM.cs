using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Data;
using ASMC.Common.ViewModel;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;

namespace Plugins.Test
{
    public class TableVm:BaseViewModel
    {
        private Timer timer;
        private BindingList<Cell> _testData = new BindingList<Cell>() { new Cell{Name = "Ячейка 1", ColumnIndex = 0}, new Cell() { Name = "Ячейка 2", ColumnIndex = 1}, new Cell() { Name = "Ячейка 3", ColumnIndex = 0, RowIndex = 2}, new Cell() { Name = "Ячейка 4",ColumnIndex = 1, RowIndex = 2 }, };
        private Cell _selected;

        public TableVm()
        {
            timer = new Timer();
            //timer.Interval = 5000;
            //timer.Start();
            //timer.Elapsed += Timer_Elapsed;
        }
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            TestData[0].Value = double.Parse(TestData[0].Value.ToString()) + 1;
        }
        public BindingList<Cell> TestData
        {
            get => _testData;
            set => SetProperty(ref _testData, value, nameof(TestData));
        }
        public  Cell Selected
        {
            get => _selected;
            set => SetProperty(ref _selected, value, nameof(Selected));
        }
    }

    public class Cell : MeasPoint, ICell
    {
        /// <inheritdoc />
        public string Name { get; set; }

        /// <inheritdoc />
        public object Value { get=> this.NominalVal; 
            set=>this.NominalVal= decimal.Parse(value.ToString()) ; }

        /// <inheritdoc />
        public int RowIndex { get; set; }

        /// <inheritdoc />
        public int ColumnIndex { get; set; }
    }
}
