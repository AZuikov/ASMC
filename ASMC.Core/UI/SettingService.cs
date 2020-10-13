using System.Reflection;
using System.Windows;
using ASMC.Common.UI;
using ASMC.Core.ViewModel;
using DevExpress.Mvvm.UI;

namespace ASMC.Core.UI
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