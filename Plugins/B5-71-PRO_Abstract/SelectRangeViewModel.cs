using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Common.ViewModel;
using ASMC.Devices;
using ASMC.Devices.WithoutInterface.Voltmetr;

namespace B5_71_PRO_Abstract
{
    public class SelectRangeViewModel : FromBaseViewModel
    {
        private string _description="dasadsadasdasdasdsdas";
        private ICommand[] _Ranges;
        private ICommand _SelectRange;

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value, nameof(Description));
        }
        public ICommand[] Ranges
        {
            get => _Ranges;
            set => SetProperty(ref _Ranges, value, nameof(Ranges));
        }

        public ICommand SelectRange
        {
            get => _SelectRange;
            set => SetProperty(ref _SelectRange, value, nameof(SelectRange));
        }
        public SelectRangeViewModel()
        {
            var sdas = new B5_57();
            Ranges = sdas.Ranges;
        }
    }
}
