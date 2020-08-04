using System.Reflection;
using System.Windows;
using ASMC.Common.UI;
using ASMC.Core.ViewModel;
using DevExpress.Mvvm.UI;

namespace ASMC.Core.UI
{
    public class QuestionTextService : FormServiceBase
    {
        public QuestionTextService()
        {
            Title = "Вопрос";
            ViewLocator = new ViewLocator(Assembly.GetExecutingAssembly());
            DocumentType = "QuestionTextView";
            MaxSize= new Size(1024,768);
        }

        protected override object CreateViewModel()
        {
            return new QuestionTextViewModel();
        }
    }
}
