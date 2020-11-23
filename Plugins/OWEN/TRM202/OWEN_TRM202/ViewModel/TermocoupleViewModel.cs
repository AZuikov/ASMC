using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Common.ViewModel;
using ASMC.Core.ViewModel;

namespace OWEN_TRM202
{
   public class TermocoupleViewModel: SelectionViewModel
    {
        //private TableViewModel _content;
        public ObservableCollection<IItemTable> Content { get; } = new ObservableCollection<IItemTable>();
       

        protected override bool CanSelect()
        {
            if (Content.Count <= 1) return false;
            return Content.All(q => q.Cells.All(p => !string.IsNullOrWhiteSpace(p?.Value?.ToString())));
        }
    }
}
