using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using ASMC.Core.ViewModel;
using ASMC.Data.Model;
using ASMC.Devices.OWEN;
using ASMC.Devices.Port;
using OwenioNet.Types;

namespace ASMC.Devices.UInterface.TRM.ViewModel
{
    public class TRMSettingViewModel : SelectionViewModel
    {
        #region Fields

        /// <summary>
        /// Адрес прибора.
        /// </summary>
        private int _addresViewView;

        private ComPort.SpeedRate[] _baudRateArrViewView;
        private ComPort.SpeedRate _baudRateViewView;

        private TRM202Device _trm202Device;
        

        private int _maxAddresRangeValueView = 255;

        /// <summary>
        /// Длина сетевого адреса прибора.
        /// </summary>
        private AddressLengthType[] _netAddresLenArrViewView;

        private AddressLengthType _netAddresViewLenView;

        #endregion

        #region Property

        public TRM202Device Trm202Device { get; set; }

        public int AddresView
        {
            get => _addresViewView;
            set => SetProperty(ref _addresViewView, value, nameof(AddresView));
        }

        public ComPort.SpeedRate[] BaudSpeedRatesArrView
        {
            get => _baudRateArrViewView;
            set => SetProperty(ref _baudRateArrViewView, value, nameof(BaudSpeedRatesArrView));
        }

        public ComPort.SpeedRate BaudSpeedRatesView
        {
            get => _baudRateViewView;
            set => SetProperty(ref _baudRateViewView, value, nameof(BaudSpeedRatesView));
        }

        public int MaxAddresRangeValueView
        {
            get => _maxAddresRangeValueView;
            set => SetProperty(ref _maxAddresRangeValueView, value, nameof(MaxAddresRangeValueView)); 
        }

        public AddressLengthType[] NetAddresArrView
        {
            get => _netAddresLenArrViewView;
            set => SetProperty(ref _netAddresLenArrViewView, value, nameof(NetAddresArrView));
        }

        public AddressLengthType NetAddresView
        {
            get => _netAddresViewLenView;
            set => SetProperty(ref _netAddresViewLenView, value, nameof(NetAddresView),
                               () => MaxAddresRangeValueView = _netAddresViewLenView == OwenioNet.Types.AddressLengthType.Bits8 ? 255 : 2047);
        }


        #endregion


        public TRMSettingViewModel()
        {
            BaudSpeedRatesArrView = new[]
            {
                ComPort.SpeedRate.R2400, ComPort.SpeedRate.R4800, ComPort.SpeedRate.R9600, ComPort.SpeedRate.R14400,
                ComPort.SpeedRate.R19200, ComPort.SpeedRate.R28800, ComPort.SpeedRate.R38400, ComPort.SpeedRate.R57600,
                ComPort.SpeedRate.R115200
            };
            _baudRateViewView = Enumerable.FirstOrDefault(BaudSpeedRatesArrView);

            NetAddresArrView = (AddressLengthType[]) Enum.GetValues(typeof(AddressLengthType));
            _netAddresViewLenView = Enumerable.FirstOrDefault(NetAddresArrView);
            _addresViewView = 24;
        }

        #region Methods

        public override void Close()
        {
            Trm202Device.BaudRate = BaudSpeedRatesView;
            Trm202Device.DeviceAddres = AddresView;
            Trm202Device.AddressLength = NetAddresView;
            base.Close();
        }

        protected override bool CanSelect()
        {
            return _addresViewView >= 0 && _addresViewView <= _maxAddresRangeValueView;
        }

        #endregion
    }

    public class TRM202DeviceUI : TRM202Device, IControlPannelDevice
    {
        public TRM202DeviceUI()
        {
            ViewModel = new TRMSettingViewModel();
            Assembly = Assembly.GetExecutingAssembly();
            DocumentType = "TrmSettingView";
            Device = this;
            ((TRMSettingViewModel)ViewModel).Trm202Device = this;
        }

        public string DocumentType { get; }
        public INotifyPropertyChanged ViewModel { get; }
        public Assembly Assembly { get; }
        public IUserType Device { get; }
    }
}