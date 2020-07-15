using System;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using ASMC.Core;
using ASMC.Core.UI;
using ASMC.Core.ViewModel;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using DevExpress.Mvvm;
using NLog;

namespace ASMC.ViewModel
{
    public class WizardViewModel : BaseViewModel
    {
        /// <summary>
        /// Перечень вкладок.
        /// </summary>
        public enum TabItemControl
        {
            /// <summary>
            /// Выбор программы МК.
            /// </summary>
            ChoiceSi,
            /// <summary>
            /// Выбор вида МК.
            /// </summary>
            ChoiceTypeMc,
            /// <summary>
            /// Подготовка рабочего места.
            /// </summary>
            Workplace,
            /// <summary>
            /// Настройка приборов.
            /// </summary>
            Settings,
            /// <summary>
            /// Операции МК.
            /// </summary>
            Operations
        }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region  Fields

        private string[] _accessoriesList;
        private IUserItemOperationBase _curentItemOperation;
        private DataView _dataOperation;
        private IDevice[] _controlDevices;
        private AbstraktOperation.TypeOpeation? _enableOpeation;
        private bool _isSpeedWork;
        private ShemeImage _lastShema;
        private TabItemControl _selectedTabItem;
        private IUserItemOperationBase _selectionItemOperation;
        private IProgram _selectProgram;
        private AbstraktOperation.TypeOpeation _typeOpertion;
        private IUserItemOperationBase[] _userItemOperation;

        #endregion

        #region Property

        public string[] AccessoriesList
        {
            get => _accessoriesList;
            set => SetProperty(ref _accessoriesList, value, nameof(AccessoriesList));
        }

        public IUserItemOperationBase CurentItemOperation
        {
            get => _curentItemOperation;
            set => SetProperty(ref _curentItemOperation, value, nameof(CurentItemOperation));
        }

        public DataView DataOperation
        {
            get => _dataOperation;
            set => SetProperty(ref _dataOperation, value, nameof(DataOperation));
        }

        public IDevice[] ControlDevices
        {
            get => _controlDevices;
            set => SetProperty(ref _controlDevices, value, nameof(ControlDevices));
        }

        public AbstraktOperation.TypeOpeation? EnableOpeation
        {
            get => _enableOpeation;
            set => SetProperty(ref _enableOpeation, value, nameof(EnableOpeation), EnableOpeationCallback);
        }

        /// <summary>
        /// Режим проверки(Ускроренные операции)
        /// </summary>
        public bool IsSpeedWork
        {
            get => _isSpeedWork;
            set => SetProperty(ref _isSpeedWork, value, nameof(IsSpeedWork), OnIsSpeedWorkCallback);
        }

        private void OnIsSpeedWorkCallback()
        {
            SelectProgram.AbstraktOperation.IsSpeedWork = IsSpeedWork;
            OnSelectProgramCallback();
        }

        /// <summary>
         /// ПОзволяет получать или задавать последнюю отображенную схему.
         /// </summary>
        public ShemeImage LastShema
        {
            get => _lastShema;
            set => SetProperty(ref _lastShema, value, nameof(LastShema), LastShemaCallback);
        }


        /// <summary>
        /// Позволяет получать коллекцию программ
        /// </summary>
        public ObservableCollection<IProgram> Prog { get; } = new ObservableCollection<IProgram>();

        /// <summary>
        /// Позволяет получить или задать выбранную вкладку.
        /// </summary>
        public TabItemControl SelectedTabItem
        {
            get => _selectedTabItem;
            set => SetProperty(ref _selectedTabItem, value, nameof(SelectedTabItem), ChangedCallback);
        }

        public IUserItemOperationBase SelectionItemOperation
        {
            get => _selectionItemOperation;
            set => SetProperty(ref _selectionItemOperation, value, nameof(SelectionItemOperation));
        }

        /// <summary>
        /// Позволяет получать или задавать программу МК.
        /// </summary>
        public IProgram SelectProgram
        {
            get => _selectProgram;
            set => SetProperty(ref _selectProgram, value, nameof(SelectProgram), OnSelectProgramCallback);
        }


        /// <summary>
        /// Позволяет получать или задавать тип выбранной операции МК.
        /// </summary>
        public AbstraktOperation.TypeOpeation TypeOpertion
        {
            get => _typeOpertion;
            set => SetProperty(ref _typeOpertion, value, nameof(TypeOpertion));
        }

        /// <summary>
        /// Позволяет получать или задавать перечень операций МК.
        /// </summary>
        public IUserItemOperationBase[] UserItemOperation
        {
            get => _userItemOperation;
            set => SetProperty(ref _userItemOperation, value, nameof(UserItemOperation));
        }

        #endregion

        public WizardViewModel()
        {
            StartCommand = new DelegateCommand(OnStartCommand);
            NextCommand =
                new DelegateCommand(OnNextCommand,
                    () => typeof(TabItemControl).GetFields().Length - 2 > (int) SelectedTabItem &&
                          SelectProgram != null);
            BackCommand =
                new DelegateCommand(OnBackCommand, () => SelectedTabItem > 0 && SelectProgram != null);
            RefreshCommand =
                new DelegateCommand(OnRefreshCommand);
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

        private void OnRefreshCommand()
        {
           SelectProgram.AbstraktOperation.SelectedOperation.RefreshDevice();
        }

        #region Methods

        protected override void OnInitialized()
        {
            base.OnInitialized();
            LoadPlugins();
        }

        private void AbstraktOperationOnChangeShemaEvent(IUserItemOperationBase sender)
        {
            SelectionItemOperation = sender;
            if (SelectionItemOperation.Sheme?.Number != LastShema?.Number) LastShema = SelectionItemOperation.Sheme;
        }

        private void ChangedCallback()
        {
            if (SelectedTabItem == TabItemControl.Operations)
            {
            }
        }

        private void EnableOpeationCallback()
        {
            if (EnableOpeation != null) TypeOpertion = EnableOpeation.Value;
        }

        private void LastShemaCallback()
        {
            var message = GetService<IMessageBoxService>(ServiceSearchMode.PreferLocal);
            //message.Show("32321", "fdsfsf", MessageBoxButton.OK, MessageBoxImage.None);
            // var service = GetService<IFormService>("ShemService");
            //service?.Show();
        }

        private void LoadPlugins()
        {
            var path = $@"{Directory.GetCurrentDirectory()}\Plugins";
            if (!Directory.Exists(path)) return;

            var files = Directory.GetFiles(path);
            try
            {
                foreach (var file in files)
                    if (file.EndsWith(".dll"))
                        Assembly.LoadFile(Path.GetFullPath(file));
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            if (files.Length <= 0) return;
            var interfaceType = typeof(IProgram);
            Type[] types = null;
            try
            {
                types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(p => p.GetTypes())
                    .Where(p => interfaceType.IsAssignableFrom(p) && p.IsClass).ToArray();
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }


            if (types == null) return;
            foreach (var type in types)
                Prog.Add((IProgram) Activator.CreateInstance(type));
            foreach (var program in Prog)
            {
                program.TaskMessageService = GetService<IMessageBoxService>(ServiceSearchMode.PreferLocal);
            }
        }

        private void OnBackCommand()
        {
            SelectedTabItem = SelectedTabItem - 1;
        }

        private void OnNextCommand()
        {
            SelectedTabItem = SelectedTabItem + 1;
        }

        private async void OnStartCommand()
        {
            if (SelectionItemOperation == null)  SelectProgram.AbstraktOperation.StartWorkAsync(new CancellationTokenSource());
        }

        private void OnSelectProgramCallback()
        {
            if (SelectProgram == null) return;
            EnableOpeation = SelectProgram.AbstraktOperation.EnabledOperation;
            foreach (Enum en in Enum.GetValues(typeof(AbstraktOperation.TypeOpeation)))
                if (EnableOpeation != null && ((Enum) EnableOpeation).HasFlag(en))
                {
                    TypeOpertion = (AbstraktOperation.TypeOpeation) en;
                    break;
                }

            SelectProgram.AbstraktOperation.SelectedTypeOpeation = TypeOpertion;
            UserItemOperation = SelectProgram.AbstraktOperation?.SelectedOperation?.UserItemOperation;
            ControlDevices = SelectProgram.AbstraktOperation.SelectedOperation?.ControlDevices;
            SelectProgram.AbstraktOperation.SelectedOperation?.RefreshDevice();
            AccessoriesList = SelectProgram.AbstraktOperation.SelectedOperation?.Accessories;
            SelectProgram.AbstraktOperation.ChangeShemaEvent += AbstraktOperationOnChangeShemaEvent;
        }

        #endregion

        #region Command
        public ICommand RefreshCommand
        {
            get;
        }
        public ICommand BackCommand { get; }

        /// <summary>
        /// Комманда запуска режима МК.
        /// </summary>
        public ICommand StartCommand { get; }

        public ICommand NextCommand { get; }

        #endregion
    }
}