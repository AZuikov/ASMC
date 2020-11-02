using System.ComponentModel;
using System.Reflection;
using System.Windows;
using ASMC.Common.ViewModel;
using ASMC.Core.UI;
using DevExpress.Mvvm.UI;

namespace ASMC.Common.UI
{
    public class QuestionTextService : SelectionService
    {
        public QuestionTextService()
        {
            Title = "Вопрос";
            ViewLocator = new ViewLocator(Assembly.GetExecutingAssembly());
            DocumentType = "QuestionTextView";
            MaxSize= new Size(1024,768);
        }
        protected override INotifyPropertyChanged CreateViewModel()
        {
            return new QuestionTextViewModel();
        }
    }
}
