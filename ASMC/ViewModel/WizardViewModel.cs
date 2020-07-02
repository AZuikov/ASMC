using System;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using ASMC.Core.ViewModel;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using DevExpress.Mvvm;

namespace ASMC.ViewModel
{
    public class WizardViewModel : BaseViewModel
    {
        public enum TabItemControl
        {
            ChoiceSi,
            ChoiceTypeMc,
            Workplace,
            Settings,
            Operations
        }

        private string[] _accessoriesList;
        private DataView _dataOperation;
        private IDevice[] _device;
        private AbstraktOperation.TypeOpeation? _enableOpeation = AbstraktOperation.TypeOpeation.Adjustment;
        private BaseViewModel _regionOperations;
        private TabItemControl _selectedTabItem;
        private IUserItemOperationBase _selectionItemOperation;
        private IProrgam _selectProgram;
        private AbstraktOperation.TypeOpeation _typeOpertion;
        private IUserItemOperationBase[] _userItemOperation;
        private IUserItemOperationBase _CurentItemOperation;
        private ShemeImage _LastShema;

        public WizardViewModel()
        {
            StartCommand = new DelegateCommand(ExecuteMethod);
            NextCommand =
                new DelegateCommand(OnNextCommand,
                    () => typeof(TabItemControl).GetFields().Length - 2 > (int) SelectedTabItem &&
                          SelectProgram != null);
            BackCommand =
                new DelegateCommand(OnBackCommand, () => SelectedTabItem > 0 && SelectProgram != null);
            Prog = new ObservableCollection<IProrgam>();
            LoadPlugins();
            //Prog = new ObservableCollection<IProrgam> {new Device()};
            //Prog[0].AbstraktOperation.IsSpeedWork = false;
            //Prog[0].AbstraktOperation.SelectedTypeOpeation = AbstraktOperation.TypeOpeation.PrimaryVerf;
            //UserItemOperation = Prog[0].AbstraktOperation.SelectedOperation?.UserItemOperation;
            //Device = Prog[0].AbstraktOperation.SelectedOperation?.Device;
            //Prog[0].AbstraktOperation.SelectedOperation?.RefreshDevice();
            //AccessoriesList = Prog[0].AbstraktOperation.SelectedOperation?.Accessories;
            //Prog[0].AbstraktOperation.SelectedOperation?.UserItemOperation[0].StartWork();


            //DataOperation = SelectionItemOperation?.Data.DefaultView;
        }

        private async void ExecuteMethod()
        {
            if (this.SelectionItemOperation==null)
            {
               await  SelectProgram.AbstraktOperation.StartWorkAsync();  
            } 
        }

        public string[] AccessoriesList
        {
            get => _accessoriesList;
            set => SetProperty(ref _accessoriesList, value, nameof(AccessoriesList));
        }

        public ICommand BackCommand { get; }

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

        public AbstraktOperation.TypeOpeation? EnableOpeation
        {
            get => _enableOpeation;
            set => SetProperty(ref _enableOpeation, value, nameof(EnableOpeation), EnableOpeationCallback);
        }


        
        public ICommand StartCommand
        {
            get;
        }
        public ICommand NextCommand { get; }

        /// <summary>
        ///     Позволяет получать коллекцию программ
        /// </summary>
        public ObservableCollection<IProrgam> Prog { get; }


        public BaseViewModel RegionOperations
        {
            get => _regionOperations;
            set => SetProperty(ref _regionOperations, value, nameof(RegionOperations));
        }
        /// <summary>
        /// Позволяет получить или задать выбранную вкладку.
        /// </summary>
        public TabItemControl SelectedTabItem
        {
            get => _selectedTabItem;
            set => SetProperty(ref _selectedTabItem, value, nameof(SelectedTabItem), ChangedCallback);
        }

        private void ChangedCallback()
        {
            if ( SelectedTabItem== TabItemControl.Operations)  
            {

            }
        }

        public IUserItemOperationBase SelectionItemOperation
        {
            get => _selectionItemOperation;
            set => SetProperty(ref _selectionItemOperation, value, nameof(SelectionItemOperation));
        }

        public IProrgam SelectProgram
        {
            get => _selectProgram;
            set => SetProperty(ref _selectProgram, value, nameof(SelectProgram), SelectProgramCallback);
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

        protected override void OnInitialized()
        {
            base.OnInitialized();
            LoadPlugins();
        }
        private void SelectProgramCallback()
        {
            if(SelectProgram != null)
            {
                EnableOpeation = SelectProgram.AbstraktOperation.EnabledOperation;
                SelectProgram.AbstraktOperation.IsSpeedWork = false;
                SelectProgram.AbstraktOperation.SelectedTypeOpeation = TypeOpertion;
                UserItemOperation = SelectProgram.AbstraktOperation.SelectedOperation?.UserItemOperation;
                Device = SelectProgram.AbstraktOperation.SelectedOperation?.Device;
                SelectProgram.AbstraktOperation.SelectedOperation?.RefreshDevice();
                AccessoriesList = SelectProgram.AbstraktOperation.SelectedOperation?.Accessories;
                SelectProgram.AbstraktOperation.ChangeShemaEvent    += AbstraktOperationOnChangeShemaEvent;
            }
        }
        public IUserItemOperationBase CurentItemOperation
        {
            get => _CurentItemOperation;
            set => SetProperty(ref _CurentItemOperation, value, nameof(CurentItemOperation));
        }
        private void AbstraktOperationOnChangeShemaEvent(IUserItemOperationBase sender)
        {
            SelectionItemOperation = sender;
            if (SelectionItemOperation.Sheme?.Number!= LastShema?.Number)
            {
                LastShema = SelectionItemOperation.Sheme;
            }
           
        }

        public ShemeImage LastShema {
            get => _LastShema;
            set => SetProperty(ref _LastShema, value, nameof(LastShema), LastShemaCallback);
        }

        private void LastShemaCallback()
        {
           
        }

        private void EnableOpeationCallback()
        {
            if(EnableOpeation != null) TypeOpertion = EnableOpeation.Value;
          
        }

        private void LoadPlugins()
        {
            var path = $@"{Directory.GetCurrentDirectory()}\Plugins";
            if (!Directory.Exists(path)) return;

            var files = Directory.GetFiles(path);
            foreach (var file in files)
                if (file.EndsWith(".dll"))
                    Assembly.LoadFile(Path.GetFullPath(file));

            if (files.Length <= 0) return;
            var interfaceType = typeof(IProrgam);
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(p => p.GetTypes())
                .Where(p => interfaceType.IsAssignableFrom(p) && p.IsClass).ToArray();
            foreach (var type in types) Prog.Add((IProrgam) Activator.CreateInstance(type));
        }

        private void OnBackCommand()
        {
            SelectedTabItem = SelectedTabItem - 1;
        }

        private void OnNextCommand()
        {
            SelectedTabItem = SelectedTabItem + 1;
        }
    }
}