using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AP.Utils.Data;
using AP.Utils.Helps;
using ASMC.Common.ViewModel;
using ASMC.Data.Model;
using ASMC.Devices;
using ASMC.Devices.WithoutInterface.Voltmetr;

namespace B5_71_PRO_Abstract
{
    public class SelectRangeViewModel : FromBaseViewModel
    {
        private string _description="Выбор установленного предела измерения:";
        private List<MeasPoint> _ranges;
        private MeasPoint _SelectRange;

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value, nameof(Description));
        }

        public List<MeasPoint> Ranges
        {
            get => _ranges;
            set => SetProperty(ref _ranges, value, nameof(Ranges));
        }

        public MeasPoint SelectRange
        {
            get => _SelectRange;
            set => SetProperty(ref _SelectRange, value, nameof(SelectRange));
        }
        public SelectRangeViewModel()
        {
            var b357 = new B3_57();
           _ranges = new List<MeasPoint>(b357.Ranges);
           
        }
    }

    


}
