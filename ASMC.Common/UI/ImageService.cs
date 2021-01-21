using System.ComponentModel;
using System.Reflection;
using System.Windows;
using ASMC.Common.ViewModel;
using ASMC.Core.UI;
using DevExpress.Mvvm.UI;

namespace ASMC.Common.UI
{
    public class ImageService : SelectionService
    {
        public ImageService()
        {
            Title = "Схема";
            ViewLocator = new ViewLocator(Assembly.GetExecutingAssembly());
            DocumentType = "ShemView";
            this.SizeToContent = SizeToContent.WidthAndHeight;
            MaxSize= new Size(1024,768);

        }
        protected override INotifyPropertyChanged CreateViewModel()
        {
            return new ShemViewModel();
        }
    }
}
