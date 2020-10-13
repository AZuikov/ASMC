using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ASMC.Common.ViewModel;
using ASMC.Data.Model;
using ASMC.Devices.WithoutInterface.HourIndicator;

namespace ASMC.Devices.UInterface.AnalogDevice.ViewModel
{
    public class IchBaseSettingUi:IchBase, IControlPannelDevice
    {


        public IchBaseSettingUi()
        {
            ViewModel = new IchSettingViewModel{ IchBaseSettingUi= this };

            DocumentType = "IchSettingView";
            Assembly = Assembly.GetExecutingAssembly();
        }
        /// <inheritdoc />
        public string DocumentType { get; }

        /// <inheritdoc />
        public INotifyPropertyChanged ViewModel { get; }

        /// <inheritdoc />
        public Assembly Assembly { get; }
    }

    public class IchSettingViewModel : ClosableViewModel
    {
        public IchBaseSettingUi IchBaseSettingUi
        {
            get => _ichBaseSettingUi;
            set => SetProperty(ref _ichBaseSettingUi, value, nameof(IchBaseSettingUi),()=> Init());
        }
        private AccuracyClass.Standart[] _availabeAccuracyClass;
        private IchBaseSettingUi _ichBaseSettingUi;
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
