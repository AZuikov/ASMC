using System.Windows.Documents;
using System.Windows.Input;
using DevExpress.Mvvm;
using Palsys.Core.ViewModel;

namespace ASMC.Core.ViewModel
{
    public class ReadAndWriteTextViewModel : BaseViewModel
    {
        public string DocumentHeaderPath { get; set; }
        private bool _radioButtonValue;
        private FlowDocument _userMessage;
        public DateRwt Data { get; private set; }
        public ICommand Send { get; }
        public ICommand RadioCommand { get; private set; }
        public FlowDocument UserMessage
        {
            get => _userMessage;
            set => SetProperty(ref _userMessage, value, nameof(UserMessage));
        }
        public bool RadioButtonValue
        {
            get => _radioButtonValue;
            set => SetProperty(ref _radioButtonValue, value, nameof(RadioButtonValue));
        }

        public ReadAndWriteTextViewModel()
        {
            RadioCommand = new DelegateCommand(() => {Radio(RadioButtonValue);});
        }
        private void Radio(object parametr)
        {

        }

        private void SendMetod()
        {
            Data = new DateRwt
            {
                UserMessage = UserMessage,
                RadioButtonValue = RadioButtonValue
            };
        }
    }

    public class DateRwt
    {
        public FlowDocument UserMessage { get; set; }
        public bool RadioButtonValue { get; set; }
    }
}