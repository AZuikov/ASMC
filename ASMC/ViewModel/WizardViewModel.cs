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
        private IDevice[] _deviceInterface;
        private string[] _accessoriesList;
        private DataView _DataOperation;
        private IUserItemOperationBase _SelectionItemOperation;

        public ViewModelBase RegionOperations { get => _regionOperations;
            set => SetProperty(ref _regionOperations, value, nameof(RegionOperations));
        }
        public ObservableCollection<string> Header { get; set; } = new ObservableCollection<string>( new[] {"Column1", "Column2", "Column2" });
        public IUserItemOperationBase[] UserItemOperation
        {
            get => _userItemOperation;
            set => SetProperty(ref _userItemOperation, value, nameof(UserItemOperation));
        }
        public IDevice[] DeviceInterface
        {
            get => _deviceInterface;
            set => SetProperty(ref _deviceInterface, value, nameof(DeviceInterface));
        }
        public string[] AccessoriesList
        {
            get => _accessoriesList;
            set => SetProperty(ref _accessoriesList, value, nameof(AccessoriesList));
        }
        public DataView DataOperation
        {
            get => _DataOperation;
            set => SetProperty(ref _DataOperation, value, nameof(DataOperation));
        }
        public ObservableCollection<IProg> Prog { get; set; }

        public IUserItemOperationBase SelectionItemOperation
        {
            get => _SelectionItemOperation;
            set => SetProperty(ref _SelectionItemOperation, value, nameof(SelectionItemOperation));
        }
        public WizardViewModel()
        {
            Prog = new ObservableCollection<IProg> {new Devise()};
            UserItemOperation = Prog[0].Operation.UserItemOperationFirsVerf?.UserItemOperation;
            DeviceInterface = Prog[0].Operation.UserItemOperationFirsVerf?.Device;
            Prog[0].Operation.UserItemOperationFirsVerf?.RefreshDevice();
            AccessoriesList = Prog[0].Operation.UserItemOperationFirsVerf?.Accessories;
            //DataOperation = SelectionItemOperation?.Data.DefaultView;
        }
    }
}
