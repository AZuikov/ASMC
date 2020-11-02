using System.ComponentModel;
using System.Reflection;
using System.Windows;
using ASMC.Common.ViewModel;
using ASMC.Core.UI;
using DevExpress.Mvvm.UI;

namespace ASMC.Common.UI
{
    public class SettingService : SelectionService
    {
        public SettingService()
        {
            Title = "Схема";
            ViewLocator = new ViewLocator(Assembly.GetExecutingAssembly());
            DocumentType = "ShemView";
            MaxSize= new Size(1024,768);
        }

        /// <inheritdoc />
        protected override INotifyPropertyChanged CreateViewModel()
        {
            return new ShemViewModel();
        }
    }
}