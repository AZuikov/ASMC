using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using ASMC.Automation.Linear.ViewModel;
using ASMC.Automation.Radio;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using DevExpress.Mvvm;
using DevExpress.Mvvm.UI;
using DevExpress.XtraPrinting.Native;

namespace ASMC.ViewModel
{
    public class WizardViewModel : ViewModelBase
    {
        private ViewModelBase _regionOperations;
        private IUserItemOperationBase[] _userItemOperation;
        private IDevice[] _device;
        private string[] _accessoriesList;
        private DataView _dataOperation;
        private IUserItemOperationBase _selectionItemOperation;
        private AbstraktOperation.TypeOpeation _typeOpertion;

        public ViewModelBase RegionOperations { get => _regionOperations;
            set => SetProperty(ref _regionOperations, value, nameof(RegionOperations));
        }
        public ObservableCollection<string> Header { get; set; } = new ObservableCollection<string>( new[] {"Column1", "Column2", "Column2" });
        public IUserItemOperationBase[] UserItemOperation
        {
            get => _userItemOperation;
            set => SetProperty(ref _userItemOperation, value, nameof(UserItemOperation));
        }
        public IDevice[] Device
        {
            get => _device;
            set => SetProperty(ref _device, value, nameof(Device));
        }
        public string[] AccessoriesList
        {
            get => _accessoriesList;
            set => SetProperty(ref _accessoriesList, value, nameof(AccessoriesList));
        }
        public DataView DataOperation
        {
            get => _dataOperation;
            set => SetProperty(ref _dataOperation, value, nameof(DataOperation));
        }
        public ObservableCollection<IProg> Prog { get; set; }

        public IUserItemOperationBase SelectionItemOperation
        {
            get => _selectionItemOperation;
            set => SetProperty(ref _selectionItemOperation, value, nameof(SelectionItemOperation));
        }
        public AbstraktOperation.TypeOpeation TypeOpertion
        {
            get => _typeOpertion;
            set => SetProperty(ref _typeOpertion, value, nameof(TypeOpertion));
        }
        public WizardViewModel()
        {
           var s =  AP.Math.MathStatistics.Mapping((decimal) 0.6, (decimal) 0.005, (decimal) 0.95, 0, 30);
            Prog = new ObservableCollection<IProg> {new Device()};
            Prog[0].AbstraktOperation.IsSpeedWork = false;
            Prog[0].AbstraktOperation.SelectedTypeOpeation = AbstraktOperation.TypeOpeation.PrimaryVerf;
            UserItemOperation = Prog[0].AbstraktOperation.SelectedOperation?.UserItemOperation;
            Device = Prog[0].AbstraktOperation.SelectedOperation?.Device;
            Prog[0].AbstraktOperation.SelectedOperation?.RefreshDevice();
            AccessoriesList = Prog[0].AbstraktOperation.SelectedOperation?.Accessories;
            Prog[0].AbstraktOperation.SelectedOperation?.UserItemOperation[0].StartWork();
            //DataOperation = SelectionItemOperation?.Data.DefaultView;
        }
    }
}
