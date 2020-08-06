using ASMC.Common.ViewModel;

namespace ASMC.Core.ViewModel
{
    public class QuestionTextViewModel : FromBaseViewModel
    {
        private string _document;
        private bool _checkBox;

        public QuestionTextViewModel()
        {
            this.AllowSelect = true;
            var a = (document: Document, check: CheckBox);
            Entity = a;
        }

        public string Document
        {
            get => _document;
            set => SetProperty(ref _document, value, nameof(Document));
        }

        public bool CheckBox
        {
            get => _checkBox;
            set => SetProperty(ref _checkBox, value, nameof(CheckBox));
        }

     }
}