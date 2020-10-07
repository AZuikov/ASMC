﻿using System.ComponentModel;
using System.Linq;
using ASMC.Common.ViewModel;
using ASMC.Data.Model;
using DevExpress.Mvvm;
using NLog;

namespace ASMC.ViewModel
{
    public class DeviceViewModel : BindableBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields

        private string[] _addresDivece;
        private string _description;
        private bool? _isConnect;
        private IUserType[] _devices;
        private IUserType _selectedDevice;
        private string _stringConnect;

        #endregion

        #region Property

        public string[] AddresDivece
        {
            get => _addresDivece;
            set => SetProperty(ref _addresDivece, value, nameof(AddresDivece), () =>
            {
                if (string.IsNullOrWhiteSpace(StringConnect)) StringConnect = AddresDivece.FirstOrDefault();
            });
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value, nameof(Description));
        }

        public bool IsCanStringConnect { get; set; }

        public bool? IsConnect
        {
            get => _isConnect;
            set => SetProperty(ref _isConnect, value, nameof(IsConnect));
        }

        public IUserType[] Devices
        {
            get => _devices;
            set => SetProperty(ref _devices, value, nameof(Devices), () =>
            {
                if (SelectedDevice==null) SelectedDevice = Devices.FirstOrDefault();
            });
        }

        public IUserType SelectedDevice
        {
            get => _selectedDevice;
            set => SetProperty(ref _selectedDevice, value, nameof(SelectedDevice));
        }

        public string StringConnect
        {
            get => _stringConnect;
            set => SetProperty(ref _stringConnect, value, nameof(StringConnect), () =>
            {
                var device = SelectedDevice as IDeviceBase;
                if (device == null) return;
                IsConnect = device.IsTestConnect;
            });
        }

        #endregion

        public DeviceViewModel()
        {
            PropertyChanged += DeviceViewModel_PropertyChanged;
        }

        #region Methods

        private void DeviceViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(StringConnect)))
                Logger.Info($@"Для устройства {SelectedDevice} указано подключение: {StringConnect}");
            else if (e.PropertyName.Equals(nameof(SelectedDevice))) Logger.Info($@"Выбранно устройство: {SelectedDevice}");
        }

        #endregion

    }
}