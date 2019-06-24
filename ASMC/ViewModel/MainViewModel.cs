using ASMC.Core.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace ASMC.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        public MainViewModel()
        {
            
            this.Initialize();
            Alert(new Exception("test"));
        }
    }
}
