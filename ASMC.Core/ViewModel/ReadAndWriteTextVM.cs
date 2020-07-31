using System.Windows.Documents;
using ASMC.Common.ViewModel;
using ASMC.Core.UI;
using DevExpress.Mvvm;

namespace ASMC.Core.ViewModel
{
    public class ReadAndWriteTextVM : FromBaseViewModel
    {
        public string DocumentHeaderPath { get; set; }
        private bool _RadioButtonValue;
        private FlowDocument _InputUserText;
        public DateRWT Data { get; private set; }
        public DelegateCommand Send { get; }

        private void SendMetod()
        {
            Data = new DateRWT
            {
                InputUserText = _InputUserText,
                RadioButtonValue = _RadioButtonValue
            };
        }
    }
    public class DateRWT
    {
        public FlowDocument InputUserText { get; set; }
        public bool RadioButtonValue { get; set; }
    }
}
