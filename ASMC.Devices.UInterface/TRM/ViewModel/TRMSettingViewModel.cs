using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ASMC.Core.ViewModel;
using ASMC.Data.Model;
using ASMC.Devices.OWEN;
using ASMC.Devices.Port;

namespace ASMC.Devices.UInterface.TRM.ViewModel
{
    public class TRMSettingViewModel : SelectionViewModel
    {
        private ComPort.SpeedRate _baudRate;
        public ComPort.SpeedRate BaudSpeedRates
        {
            get => _baudRate;
            set => SetProperty(ref _baudRate,value, nameof(BaudSpeedRates));
        }

        private ComPort.SpeedRate[] _baudRateArr;
        public ComPort.SpeedRate[] BaudSpeedRatesArr
        {
            get => _baudRateArr;
            set => SetProperty(ref _baudRateArr, value, nameof(BaudSpeedRatesArr));
        }

        public TRMSettingViewModel()
        {
            BaudSpeedRatesArr = new[]
            {
                ComPort.SpeedRate.R2400, ComPort.SpeedRate.R4800, ComPort.SpeedRate.R9600, ComPort.SpeedRate.R14400,
                ComPort.SpeedRate.R19200, ComPort.SpeedRate.R28800, ComPort.SpeedRate.R38400, ComPort.SpeedRate.R57600,
                ComPort.SpeedRate.R115200
            };
        }


    }

    public class TRM202DeviceUI : TRM202Device, IControlPannelDevice
    {
        public TRM202DeviceUI()
        {
            this.ViewModel = new TRMSettingViewModel();
            Assembly = Assembly.GetExecutingAssembly();
            DocumentType = "TrmSettingView";
        }

        public string DocumentType { get; }
        public INotifyPropertyChanged ViewModel { get; }
        public Assembly Assembly { get; }
    }
}
