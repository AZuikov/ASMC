using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Core.ViewModel;
using DevExpress.Mvvm;

namespace ASMC.ViewModel
{
    public class OperationViewModel : ViewModelBase
    {
        public string Name { get; set; }
        /// <summary>
        /// предоставляет количество операций в пункте
        /// </summary>
        int Count { get; }
    }
}
