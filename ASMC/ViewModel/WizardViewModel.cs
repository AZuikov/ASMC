using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using AP.Reports.AutoDocumets;
using ASMC.Common;
using ASMC.Common.ViewModel;
using ASMC.Core;
using ASMC.Core.ViewModel;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using ASMC.UI;
using DevExpress.Mvvm;
using NLog;

namespace ASMC.ViewModel
{
    public class WizardViewModel : BaseViewModel
    {
        /// <summary>
        /// Состояние программы.
        /// </summary>
        public enum StateWork
        {
            Stop,
            Start,
            Pause
        }

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
            Operations,

            /// <summary>
            /// Операции МК.
            /// </summary>
            Documents
        }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region  Fields

        private readonly CancellationTokenSource _isWorkToken = new CancellationTokenSource();

        private string[] _accessoriesList;
        private IUserItemOperationBase _curentItemOperation;
        private DataView _dataOperation;
        private OperationMetrControlBase.TypeOpeation? _enableOpeation;
        private bool _isCheckWork;
        private TabItemControl _selectedTabItem;
        private IUserItemOperationBase _selectionItemOperation;
        private IProgram _selectProgram;

        private SettingViewModel _settingViewModel = new SettingViewModel();
        private StateWork _stateWorkFlag;
        private OperationMetrControlBase.TypeOpeation _typeOpertion;
        private IUserItemOperationBase[] _userItemOperation;

        #endregion

        #region Property

        public string[] AccessoriesList
        {
            get => _accessoriesList;
            set => SetProperty(ref _accessoriesList, value, nameof(AccessoriesList));
        }

        public ICommand CreatDocumetCommandCommand { get; }


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


        public OperationMetrControlBase.TypeOpeation? EnableOpeation
        {
            get => _enableOpeation;
            set => SetProperty(ref _enableOpeation, value, nameof(EnableOpeation), EnableOpeationCallback);
        }

        /// <summary>
        /// Режим проверки(Ускроренные операции)
        /// </summary>
        public bool IsCheckWork
        {
            get => _isCheckWork;
            set => SetProperty(ref _isCheckWork, value, nameof(IsCheckWork), OnIsSpeedWorkCallback);
        }

        public ICommand PauseCommand { get; }


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
            set => SetProperty(ref _selectedTabItem, value, nameof(SelectedTabItem), TabItemChanged);
        }

        /// <summary>
        /// Позволяет задать или получить
        /// </summary>
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

        public SettingViewModel SettingViewModel
        {
            get => _settingViewModel;
            set => SetProperty(ref _settingViewModel, value, nameof(SettingViewModel));
        }

        public StateWork StateWorkFlag
        {
            get => _stateWorkFlag;
            set => SetProperty(ref _stateWorkFlag, value, nameof(StateWorkFlag));
        }


        /// <summary>
        /// Позволяет получать или задавать тип выбранной операции МК.
        /// </summary>
        public OperationMetrControlBase.TypeOpeation TypeOpertion
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
            StartCommand = new DelegateCommand(OnStartCommand, () => StateWorkFlag != StateWork.Start);
            NextCommand =
                new DelegateCommand(OnNextCommand,
                                    () => typeof(TabItemControl).GetFields().Length - 2 > (int) SelectedTabItem &&
                                          SelectProgram != null);
            BackCommand =
                new DelegateCommand(OnBackCommand, () => SelectedTabItem > 0 && SelectProgram != null);
            StopCommand = new DelegateCommand(OnStopCommand,
                                              () => !_isWorkToken.IsCancellationRequested &&
                                                    StateWorkFlag != StateWork.Stop);
            RefreshCommand =
                new DelegateCommand(OnRefreshCommand);
            CreatDocumetCommandCommand =
                new DelegateCommand(OnCreatDocumetCommand);
            PauseCommand = new DelegateCommand(OnPauseCommand);
        }

        #region Methods

        private static string GetUniqueFileName(string name, string format)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\MaterialPass";

            var newFileName = path + @"\" + name + format;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                                          @"\MaterialPass");
            }
            else
            {
                Directory.Delete(path, true);
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                                          @"\MaterialPass");
            }

            var tempFiles =
                Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
            var i = 0;
            while (true)
            {
                var fileExist = false;
                foreach (var tempFile in tempFiles)
                    if (tempFile == newFileName)
                        fileExist = true;
                if (fileExist == false)
                    break;
                newFileName = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\" + name +
                              "_" + i + format;
                i++;
            }

            return newFileName;
        }

        /// <inheritdoc />
        protected override void OnInitialized()
        {
            base.OnInitialized();
            LoadPlugins();
        }

        private void EnableOpeationCallback()
        {
            if (EnableOpeation != null) TypeOpertion = EnableOpeation.Value;
        }


        private void LoadPlugins()
        {
            //var path = $@"{Directory.GetCurrentDirectory()}\Plugins";
            //if (!Directory.Exists(path)) return;

            //var files = Directory.GetFiles(path);
            //try
            //{
            //    foreach (var file in files)
            //        if (file.EndsWith(".dll"))
            //            Assembly.LoadFile(Path.GetFullPath(file));
            //}
            //catch (Exception e)
            //{
            //    Logger.Error(e);
            //}

            //if (files.Length <= 0) return;
            var interfaceType = typeof(IProgram);
            Type[] types = null;
            try
            {
                types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(p => p.GetTypes())
                                 .Where(p => interfaceType.IsAssignableFrom(p) && p.IsClass && !p.IsAbstract).ToArray();
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            var servicePack = new ServicePack
            {
                MessageBox = GetService<IMessageBoxService>(ServiceSearchMode.PreferLocal),
                ShemForm = GetService<IFormService>("ImageService"),
                TestingDialog= GetService<IFormService>("TestingDialogService")
            };
            if (types == null) return;
            foreach (var type in types)
            {
                Prog.Add((IProgram)Activator.CreateInstance(type, servicePack));
            }
               
        }

        private void OnBackCommand()
        {
            SelectedTabItem = SelectedTabItem - 1;
        }

        private void OnCreatDocumetCommand()
        {
            var report = new Word();
            report.OpenDocument(@"D:\Б5-71_1.docx");
            var a = new Document.ConditionalFormatting
            {
                Color = Color.IndianRed,
                Condition = Document.ConditionalFormatting.Conditions.Equal,
                Region = Document.ConditionalFormatting.RegionAction.Row,
                Value = "Брак",
                NameColumn = "Результат"
            };

            foreach (var uio in SelectProgram.Operation.SelectedOperation.UserItemOperation)
                report.FillTableToBookmark(uio.Data.TableName, uio.Data, false, a);

            var path = GetUniqueFileName(DateTime.Now.ToShortDateString(), ".docx");
            report.SaveAs(path);
            report.Close();
            Process.Start(path);
        }

        private void OnIsSpeedWorkCallback()
        {
            SelectProgram.Operation.IsSpeedWork = IsCheckWork;
            OnSelectProgramCallback();
        }

        private void OnNextCommand()
        {
            SelectedTabItem = SelectedTabItem + 1;
        }

        private void OnPauseCommand()
        {
            var servise = GetService<IFormService>("ShemService");
            servise.Show();
            throw new NotImplementedException();
        }

        private async void OnRefreshCommand()
        {
            try
            {
                await Task.Factory.StartNew(SelectProgram.Operation.SelectedOperation.RefreshDevice);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                Alert(e);
            }

            SettingViewModel.AddresDivece = SelectProgram.Operation.SelectedOperation.AddresDivece;
        }

        private void OnSelectProgramCallback()
        {
            if (SelectProgram == null) return;
            EnableOpeation = SelectProgram.Operation.EnabledOperation;
            foreach (Enum en in Enum.GetValues(typeof(OperationMetrControlBase.TypeOpeation)))
                if (EnableOpeation != null && ((Enum) EnableOpeation).HasFlag(en))
                {
                    TypeOpertion = (OperationMetrControlBase.TypeOpeation) en;
                    break;
                }

            SelectProgram.Operation.SelectedTypeOpeation = TypeOpertion;
            UserItemOperation = SelectProgram.Operation?.SelectedOperation?.UserItemOperation;
            if (SettingViewModel != null) SettingViewModel.Event -= SettingViewModel_Event;
            SettingViewModel = new SettingViewModel
            {
                ControlDevices = SelectProgram.Operation.SelectedOperation?.ControlDevices,
                TestDevices = SelectProgram.Operation.SelectedOperation?.TestDevices
            };
            SettingViewModel.Event += SettingViewModel_Event;
            AccessoriesList = SelectProgram.Operation.SelectedOperation?.Accessories;
        }

        private async void OnStartCommand()
        {
            StateWorkFlag = StateWork.Start;
            if (SelectionItemOperation == null)
            {
                await SelectProgram.Operation.StartWorkAsync(_isWorkToken);
            }
            StateWorkFlag = StateWork.Stop;
        }

        private void OnStopCommand()
        {
            StateWorkFlag = StateWork.Stop;
            _isWorkToken.Cancel();
        }

        private void SettingViewModel_Event()
        {
            SelectProgram.Operation.SelectedOperation.ControlDevices = SettingViewModel.ControlDevices;
            SelectProgram.Operation.SelectedOperation.TestDevices = SettingViewModel.TestDevices;
        }


        private void TabItemChanged()
        {
            if (SelectedTabItem == TabItemControl.Settings) OnRefreshCommand();
        }

        #endregion

        #region Command

        public ICommand StopCommand { get; }

        public ICommand RefreshCommand { get; }

        public ICommand BackCommand { get; }

        /// <summary>
        /// Комманда запуска режима МК.
        /// </summary>
        public ICommand StartCommand { get; }

        public ICommand NextCommand { get; }

        #endregion
    }
}