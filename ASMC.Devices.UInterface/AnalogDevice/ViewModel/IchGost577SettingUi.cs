﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ASMC.Common.ViewModel;
using ASMC.Core.ViewModel;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.WithoutInterface.HourIndicator;
using ASMC.Devices.WithoutInterface.HourIndicator.IchGost577;

namespace ASMC.Devices.UInterface.AnalogDevice.ViewModel
{
    public class IchGost577SettingUi<T> : IControlPannelDevice where T : IchGost577,new()
    {
        public IUserType Device { get; set; }

        public IchGost577SettingUi()
        {
            Device = new T();
            ViewModel = new IchSettingViewModel<T> { IchBaseSettingUi = Device as T};
            Assembly = Assembly.GetExecutingAssembly();
            DocumentType = "IchSettingView";
        }
        /// <inheritdoc />
        public string DocumentType { get; }

        /// <inheritdoc />
        public INotifyPropertyChanged ViewModel { get; }

        /// <inheritdoc />
        public Assembly Assembly { get; }

        /// <inheritdoc />
        public string UserType => Device?.UserType;


    }
    public class IchSettingViewModel<T> : SelectionViewModel where T : IchGost577, IUserType, new()
    {
        /// <inheritdoc />
        protected override bool CanSelect()
        {
            return true;
        }

        public T IchBaseSettingUi
        {
            get => _ichBaseSettingUi;
            set => SetProperty(ref _ichBaseSettingUi, value, nameof(IchBaseSettingUi),()=> Init());
        }
        private AccuracyClass.Standart[] _availabeAccuracyClass;
        private T _ichBaseSettingUi;
        private AccuracyClass.Standart _currentAccuracyClass;

        public AccuracyClass.Standart[] AvailabeAccuracyClass
        {
            get => _availabeAccuracyClass;
            set => SetProperty(ref _availabeAccuracyClass, value, nameof(AvailabeAccuracyClass));
        }

        public AccuracyClass.Standart CurrentAccuracyClass
        {
            get => _currentAccuracyClass;
            set => SetProperty(ref _currentAccuracyClass, value, nameof(CurrentAccuracyClass), ()=> IchBaseSettingUi.CurrentAccuracyClass=value);
        }
        private void Init()
        {
            AvailabeAccuracyClass = IchBaseSettingUi.AvailabeAccuracyClass;
        }

    }
}
