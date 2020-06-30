using System;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using AP.Math;
using ASMC.Core.ViewModel;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;

namespace ASMC.ViewModel
{
    public class WizardViewModel : BaseViewModel
    {
        private string[] _accessoriesList;
        private DataView _dataOperation;
        private IDevice[] _device;
        private BaseViewModel _regionOperations;
        private IUserItemOperationBase _selectionItemOperation;
        private AbstraktOperation.TypeOpeation _typeOpertion;
        private IUserItemOperationBase[] _userItemOperation;
        protected override void OnInitialized()
        {
            base.OnInitialized();
            LoadPlugins();
        }

        public WizardViewModel()
        {
            Prog = new ObservableCollection<IProrgam>();
            LoadPlugins();
            //Prog = new ObservableCollection<IProrgam> {new Device()};
            Prog[0].AbstraktOperation.IsSpeedWork = false;
            Prog[0].AbstraktOperation.SelectedTypeOpeation = AbstraktOperation.TypeOpeation.PrimaryVerf;
            UserItemOperation = Prog[0].AbstraktOperation.SelectedOperation?.UserItemOperation;
            Device = Prog[0].AbstraktOperation.SelectedOperation?.Device;
            Prog[0].AbstraktOperation.SelectedOperation?.RefreshDevice();
            AccessoriesList = Prog[0].AbstraktOperation.SelectedOperation?.Accessories;
            Prog[0].AbstraktOperation.SelectedOperation?.UserItemOperation[0].StartWork();

            
            //DataOperation = SelectionItemOperation?.Data.DefaultView;
        }
        private void LoadPlugins()
        {
           
            var path = $@"{Directory.GetCurrentDirectory()}\Plugins";
            if (!Directory.Exists(path)) return;

            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                if (file.EndsWith(".dll"))
                { 
                    Assembly.LoadFile(Path.GetFullPath(file)); 
                }   
            }
            
            if (files.Length <= 0) return;
            var interfaceType = typeof(IProrgam);
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(p => p.GetTypes())
                .Where(p => interfaceType.IsAssignableFrom(p) && p.IsClass).ToArray();
            foreach (var type in types)
            {
                Prog.Add((IProrgam)Activator.CreateInstance(type));
            }
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

        public IDevice[] Device
        {
            get => _device;
            set => SetProperty(ref _device, value, nameof(Device));
        }

        public ObservableCollection<string> Header { get; set; } =
            new ObservableCollection<string>(new[] {"Column1", "Column2", "Column2"});

        public ObservableCollection<IProrgam> Prog { get; set; }

        public BaseViewModel RegionOperations
        {
            get => _regionOperations;
            set => SetProperty(ref _regionOperations, value, nameof(RegionOperations));
        }

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

        public IUserItemOperationBase[] UserItemOperation
        {
            get => _userItemOperation;
            set => SetProperty(ref _userItemOperation, value, nameof(UserItemOperation));
        }
    }
}