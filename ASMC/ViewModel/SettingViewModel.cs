using System.ComponentModel;
using ASMC.Common.ViewModel;
using ASMC.Data.Model;

namespace ASMC.ViewModel
{
    public class SettingViewModel : BaseViewModel
    {
        public delegate void ChangePropery();

        #region Fields

        private string[] _addresDivece;
        private IDeviceUi[] _controlDevices;
        private IDeviceUi[] _testDevices;

        #endregion

        public event ChangePropery Event;

        #region Property

        public string[] AddresDivece
        {
            get => _addresDivece;
            set => SetProperty(ref _addresDivece, value, nameof(AddresDivece), () =>
            {
                foreach (var dev in TestDevice) dev.AddresDivece = AddresDivece;
                foreach (var dev in ControlDevice) dev.AddresDivece = AddresDivece;
            });
        }

        public BindingList<DeviceViewModel> ControlDevice { get; }

        public IDeviceUi[] ControlDevices
        {
            get => _controlDevices;
            set => SetProperty(ref _controlDevices, value, nameof(ControlDevices), ChangedCallback);
        }

        public BindingList<DeviceViewModel> TestDevice { get; }

        public IDeviceUi[] TestDevices
        {
            get => _testDevices;
            set => SetProperty(ref _testDevices, value, nameof(TestDevices), ChangedCallback1);
        }

        #endregion

        public SettingViewModel()
        {
            
            ControlDevice = new BindingList<DeviceViewModel>();
            TestDevice = new BindingList<DeviceViewModel>();
            ControlDevice.ListChanged += ControlDevice_ListChanged;
            TestDevice.ListChanged += ControlDevice_ListChanged;
        }

        #region Methods

        private void ChangedCallback()
        {
            foreach (var device in ControlDevices)
                ControlDevice.Add(new DeviceViewModel
                {
                    Description = device.Description,
                    Devices = device.Devices,
                    AddresDivece = AddresDivece,
                    StringConnect = device.StringConnect,
                    IsCanStringConnect = device.IsCanStringConnect,
                    IsConnect = device.IsConnect
                });
        }

        private void ChangedCallback1()
        {
            foreach (var device in TestDevices)
                TestDevice.Add(new DeviceViewModel
                {
                    Description = device.Description,
                    Devices = device.Devices,
                    AddresDivece = AddresDivece,
                    StringConnect = device.StringConnect,
                    IsCanStringConnect = device.IsCanStringConnect,
                    IsConnect = device.IsConnect
                });
        }

        private void ControlDevice_ListChanged(object sender, ListChangedEventArgs e)
        {
            //if (e.ListChangedType != ListChangedType.ItemChanged) return;
            if(Equals(TestDevices?.Length, TestDevice?.Count))
            for (var i = 0; i < TestDevices?.Length; i++)
            {
                TestDevices[i].StringConnect = TestDevice[i].StringConnect;
                TestDevices[i].SelectedDevice = TestDevice[i].SelectedDevice;
            }
            if (Equals(ControlDevices?.Length, ControlDevice?.Count))
                for (var i = 0; i < ControlDevices?.Length; i++)
            {
                ControlDevices[i].StringConnect = ControlDevice[i].StringConnect;
                ControlDevices[i].SelectedDevice = ControlDevice[i].SelectedDevice;
            }

            Event?.Invoke();
        }

        #endregion
    }
}