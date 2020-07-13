using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Core.UI;
using ASMC.ViewModel;

namespace ASMC.UI
{
    public class ShemService : FormServiceBase
    {
        public ShemService()
        {
            Title = "Схема";
        }

        protected override object CreateViewModel()
        {
            return new ShemViewModel();
            //throw new NotImplementedException();
        }
    }
}
