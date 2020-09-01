using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Common.ViewModel;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;

namespace Plugins.Test
{
    public class TableVm:BaseViewModel
    {
        private Cell [] _testData = { new Cell{Name = "Ячейка 1"}, new Cell() { Name = "Ячейка 2" }, new Cell() { Name = "Ячейка 3" }, new Cell() { Name = "Ячейка 4" }, };
        private Cell _selected;

        public Cell[] TestData
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
            set=>this.NominalVal= (decimal)value; }
    
    }
}
