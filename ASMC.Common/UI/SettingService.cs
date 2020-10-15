using System.Reflection;
using System.Windows;
using ASMC.Common.ViewModel;
using ASMC.Core.UI;
using DevExpress.Mvvm.UI;

namespace ASMC.Common.UI
{
    public class SettingService : FormServiceBase
    {
        public SettingService()
        {
            Title = "Схема";
            ViewLocator = new ViewLocator(Assembly.GetExecutingAssembly());
            DocumentType = "ShemView";
            MaxSize= new Size(1024,768);
        }

        protected override object CreateViewModel()
        {
            return new ShemViewModel();
        }
    }
}