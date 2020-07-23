using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Drawing;
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
using AP.Reports.AutoDocumets;

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
            Operations,
            /// <summary>
            /// Операции МК.
            /// </summary>
            Documents
        }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region  Fields

        private string[] _accessoriesList;
        private IUserItemOperationBase _curentItemOperation;
        private DataView _dataOperation;
        private OperationBase.TypeOpeation? _enableOpeation;
        private bool _isSpeedWork;
        private ShemeImage _lastShema;
        private TabItemControl _selectedTabItem;
        private IUserItemOperationBase _selectionItemOperation;
        private IProgram _selectProgram;
        private OperationBase.TypeOpeation _typeOpertion;
        private IUserItemOperationBase[] _userItemOperation;

        private SettingViewModel _settingViewModel = new SettingViewModel();

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

       public SettingViewModel SettingViewModel
       {
           get => _settingViewModel;
           set => SetProperty(ref _settingViewModel, value, nameof(SettingViewModel));
       }

     

        public OperationBase.TypeOpeation? EnableOpeation
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
            SelectProgram.Operation.IsSpeedWork = IsSpeedWork;
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


        /// <summary>
        /// Позволяет получать или задавать тип выбранной операции МК.
        /// </summary>
        public OperationBase.TypeOpeation TypeOpertion
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
            CreatDocumetCommandCommand =
                new DelegateCommand(OnCreatDocumetCommand);
        }

        public ICommand CreatDocumetCommandCommand { get; }

        private void OnCreatDocumetCommand()
        {
            Word report = new Word();
            report.OpenDocument(@"D:\Б5-71_1.docx");
            var a = new Document.ConditionalFormatting{ Color =  Color.IndianRed, Condition = Document.ConditionalFormatting.Conditions.Equal, Region = Document.ConditionalFormatting.RegionAction.Row , Value = "Брак", NameColumn = "Результат"};

            foreach (var uio in SelectProgram.Operation.SelectedOperation.UserItemOperation)
            {       
                report.FillTableToBookmark(uio.Data.TableName, uio.Data, false, a); 
            }
            
            var path = GetUniqueFileName(DateTime.Now.ToShortDateString(), ".docx");
            report.SaveAs(path);
            report.Close();
            System.Diagnostics.Process.Start(path);
        }
        private static string GetUniqueFileName(string name, string format)
        {

            var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\MaterialPass";

            var newFileName = path + @"\" + name + format;
            if(!Directory.Exists(path))
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\MaterialPass");
            else
            {
                Directory.Delete(path, true);
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\MaterialPass");
            }
            var tempFiles = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
            var i = 0;
            while(true)
            {
                var fileExist = false;
                foreach(var tempFile in tempFiles)
                    if(tempFile == newFileName)
                        fileExist = true;
                if(fileExist == false)
                    break;
                newFileName = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\" + name + "_" + i + format;
                i++;
            }
            return newFileName;
        }

        private void OnRefreshCommand()
        {
            SelectProgram.Operation.SelectedOperation.RefreshDevice();
            SettingViewModel.AddresDivece = SelectProgram.Operation.SelectedOperation.AddresDivece;
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
                    .Where(p => interfaceType.IsAssignableFrom(p) && p.IsClass && !p.IsAbstract).ToArray();
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

        private void OnStartCommand()
        {
#pragma warning disable 4014
            if (SelectionItemOperation == null)   SelectProgram.Operation.StartWorkAsync(new CancellationTokenSource());
#pragma warning restore 4014
        }

        private void OnSelectProgramCallback()
        {
            if (SelectProgram == null) return;
            EnableOpeation = SelectProgram.Operation.EnabledOperation;
            foreach (Enum en in Enum.GetValues(typeof(OperationBase.TypeOpeation)))
                if (EnableOpeation != null && ((Enum) EnableOpeation).HasFlag(en))
                {
                    TypeOpertion = (OperationBase.TypeOpeation) en;
                    break;
                }

            SelectProgram.Operation.SelectedTypeOpeation = TypeOpertion;
            UserItemOperation = SelectProgram.Operation?.SelectedOperation?.UserItemOperation;
            if (SettingViewModel!=null)
            {
                SettingViewModel.Event -= SettingViewModel_Event;
            }
            SettingViewModel = new SettingViewModel
            {
                ControlDevices = SelectProgram.Operation.SelectedOperation?.ControlDevices,
                TestDevices = SelectProgram.Operation.SelectedOperation?.TestDevices
            };
            SettingViewModel.Event += SettingViewModel_Event;
            AccessoriesList = SelectProgram.Operation.SelectedOperation?.Accessories;
            SelectProgram.Operation.ChangeShemaEvent += AbstraktOperationOnChangeShemaEvent;
        }

        private void SettingViewModel_Event()
        {  
            SelectProgram.Operation.SelectedOperation.ControlDevices = SettingViewModel.ControlDevices;
            SelectProgram.Operation.SelectedOperation.TestDevices = SettingViewModel.TestDevices;
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