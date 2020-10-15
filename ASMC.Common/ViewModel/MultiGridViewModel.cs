using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Core.ViewModel;

namespace ASMC.Common.ViewModel
{
    public class MultiGridViewModel : FromBaseViewModel
    {

        #region Property

        public ObservableCollection<ITemTable> Content { get; } = new ObservableCollection<ITemTable>();

        #endregion
    }
}
