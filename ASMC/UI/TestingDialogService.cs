using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ASMC.Core.UI;
using ASMC.ViewModel;
using DevExpress.Mvvm.UI;

namespace ASMC.UI
{
    public class TestingDialogService : FormServiceBase
    {
        public TestingDialogService()
        {
            ViewLocator = new ViewLocator(Assembly.GetExecutingAssembly());
            DocumentType = "TestingDialogView";
            MaxSize = new Size(1024, 768);
        }

        protected override object CreateViewModel()
        {
            return new TestingDialogViewModel();
        }
    }
}
