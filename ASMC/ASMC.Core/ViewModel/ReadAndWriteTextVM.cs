using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using ASMC.Core.View;
using DevExpress.Mvvm;

namespace ASMC.Core.ViewModel
{
    public class ReadAndWriteTextVM
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
