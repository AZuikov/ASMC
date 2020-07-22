using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ASMC.Core.ViewModel;
using ASMC.Data.Model;

namespace ASMC.ViewModel
{
    public class SettingViewModel : BaseViewModel
    {

        private IDevice[] _controlDevices;
        private IDevice[] _testDevices;
        private string[] _addresDivece;
        private DeviceViewModel _controlDeviceVm;

        public IDevice[] ControlDevices
        {
            get => _controlDevices;
            set => SetProperty(ref _controlDevices, value, nameof(ControlDevices), ChangedCallback);
        }

        private void ChangedCallback()
        {
            foreach(var device in ControlDevices)
            {
                ControlDevice.Add(new DeviceViewModel { Description = device.Description, Name = device.Name, AddresDivece = AddresDivece, StringConnect = device.StringConnect});
            }
        }

        public IDevice[] TestDevices
        {
            get => _testDevices;
            set => SetProperty(ref _testDevices, value, nameof(TestDevices), ChangedCallback1);
        }

        public SettingViewModel()
        {
            ControlDevice = new BindingList<DeviceViewModel>();
            TestDevice = new BindingList<DeviceViewModel>();
            ControlDevice.ListChanged += ControlDevice_ListChanged;
            TestDevice.ListChanged += ControlDevice_ListChanged;
        }

        private void ControlDevice_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemChanged)
            {
                for (var i = 0; i < TestDevices.Length; i++)
                {
                    TestDevices[i].StringConnect = TestDevice[i].StringConnect;
                    TestDevices[i].SelectedName = TestDevice[i].SelectedName;
                }
                for(var i = 0; i < ControlDevices.Length; i++)
                {
                    ControlDevices[i].StringConnect = ControlDevice[i].StringConnect;
                    ControlDevices[i].SelectedName = ControlDevice[i].SelectedName;
                }
                Event?.Invoke();
            }

        }
        public BindingList<DeviceViewModel> ControlDevice { get; } 
        public BindingList<DeviceViewModel> TestDevice { get; } 
        private void ChangedCallback1()
        {  
            foreach(var device in TestDevices)
            {
                TestDevice.Add(new DeviceViewModel{Description = device.Description, Name = device.Name, AddresDivece = AddresDivece});
            }
           
        }

        public delegate void ChangePropery();
        public event ChangePropery Event;
        public string[] AddresDivece
        {
            get => _addresDivece;
            set => SetProperty(ref _addresDivece, value, nameof(AddresDivece), () =>
            {
                foreach(var dev in TestDevice)
                {
                    dev.AddresDivece = AddresDivece;
                }
                foreach(var dev in ControlDevice)
                {
                    dev.AddresDivece = AddresDivece;
                }
            });
        }
    }
}
