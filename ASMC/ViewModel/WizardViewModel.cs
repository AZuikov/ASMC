using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using AP.Reports.AutoDocumets;
using AP.Reports.Utils;
using ASMC.Common;
using ASMC.Common.ViewModel;
using ASMC.Core;
using ASMC.Core.Model;
using ASMC.Core.ViewModel;
using ASMC.Data.Model;
using DevExpress.Mvvm;
using DevExpress.Mvvm.Native;
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

        #region Fields

        private string[] _accessoriesList;
        private IUserItemOperationBase _curentItemOperation;
        private DataView _dataOperation;
        private OperationMetrControlBase.TypeOpeation? _enableOpeation;
        private bool _isCheckWork;
        private bool _isManual;

        private CancellationTokenSource _isWorkToken = new CancellationTokenSource();
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

        public bool IsManual
        {
            get => _isManual;
            set => SetProperty(ref _isManual, value, nameof(IsManual),
                               () =>
                               {
                                   if (SelectProgram == null) return;
                                   SelectProgram.Operation.IsManual = IsManual;
                               });
        }

        // ReSharper disable once UnusedMember.Global
        public bool[] ModeWork { get; set; } = {true, false};
        /// <summary>
        /// Предоставляет команжу смены режима работы <see cref="IsManual"/>
        /// </summary>
        public ICommand ChangeModeKeyCommand { get; }
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
            set => SetProperty(ref _stateWorkFlag, value, nameof(StateWorkFlag), () =>
            {
                if (StateWorkFlag == StateWork.Stop) Message("Программа завершена");
            });
        }

        /// <summary>
        /// Позволяет получать или задавать тип выбранной операции МК.
        /// </summary>
        public OperationMetrControlBase.TypeOpeation TypeOpertion
        {
            get => _typeOpertion;
            set => SetProperty(ref _typeOpertion, value, nameof(TypeOpertion), () =>
            {
                ChangeProgram();
                Logger.Info($@"Выбранна операция {TypeOpertion}");
            });
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

            ChangeModeKeyCommand =new DelegateCommand(OnChangeModeKeyCommand);
        }

        private void OnChangeModeKeyCommand()
        {
            IsManual = !IsManual;
        }

        #region Methods

        /// <inheritdoc />
        protected override void OnInitialized()
        {
            base.OnInitialized();
            LoadPlugins();
        }

        private void ChangeProgram()
        {
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

        private void EnableOpeationCallback()
        {
            if (EnableOpeation != null) TypeOpertion = EnableOpeation.Value;
        }

        private void GenerateDocument()
        {
            var filename = SelectProgram.Operation.SelectedOperation.DocumentName + @".dotx";

            var path = $@"{Directory.GetCurrentDirectory()}\Plugins";
            if (!Directory.Exists(path))
                return;
           var document = Directory.GetFiles(path, filename, SearchOption.AllDirectories).FirstOrDefault();
            if (document == null) throw new NullReferenceException($"Шаблон {filename} не найден!");
            var a = new Document.ConditionalFormatting
            {
                Color = Color.IndianRed,
                Condition = Document.ConditionalFormatting.Conditions.Equal,
                Region = Document.ConditionalFormatting.RegionAction.Row,
                Value = "Брак",
                NameColumn = "Результат"
            };
            using (var report = new Word())
            {
                report.OpenDocument(document);
                bool res = true;
                var regInsTextByMark = new Regex(@"(^ITBm\w.+)");
                var regInsTextByReplase = new Regex(@"(^RepT\w.+)");
                var regInTableByMark = new Regex(@"(^ITabBm\w.+)");
                var regFillTableByMark = new Regex(@"(^FillTabBm\w.+)");
                foreach (var uio in SelectProgram.Operation.SelectedOperation.UserItemOperation)
                {
                    var tree = uio as ITreeNode;
                    if (tree == null) continue;
                    if (!FillDoc(tree))
                    {
                        res = false;
                    }
                }

                report.FindStringAndReplace("Result", res?"Пригоден к применению":"Не пригоден к применению");
                path = GetUniqueFileName(".docx");
                report.SaveAs(path);
                Logger.Info($@"Протокол сформирован по пути {path}");

                bool FillDoc(ITreeNode userItem)
                {
                    bool result = true;
                    var n = userItem as IUserItemOperationBase;
                    if (n?.Data != null)
                    {
                        var markName = n.Data.TableName;

                        if (!string.IsNullOrWhiteSpace(markName))
                        {
                            if (regInsTextByMark.IsMatch(markName))
                                report.InsertTextToBookmark(markName, TableToStringConvert(n.Data));
                            else if (regInsTextByReplase.IsMatch(markName))
                                report.FindStringAndReplace(markName, TableToStringConvert(n.Data));
                            else if (regInTableByMark.IsMatch(markName))
                                report.InsertNewTableToBookmark(markName, n.Data, a);
                            else if (regFillTableByMark.IsMatch(markName))
                                report.FillTableToBookmark(n.Data.TableName, n.Data, true, a);
                            else
                                Logger.Error($@"Имя {markName} не распознано");
                        }
                    }

                    if (n.IsGood==false)
                    {
                        result = false;
                    }
                    foreach (var node in userItem.Nodes)
                    {
                        if (n.IsGood == FillDoc(node))
                        {
                            result = false;
                        }
                    }

                    return result;
                }

            }

            Process.Start(path);

            string TableToStringConvert(DataTable dt)
            {
                var dataStr = new StringBuilder();
                foreach (DataRow row in dt.Rows)
                {
                    foreach (var cell in row.ItemArray) dataStr.Append(cell).Append(" ");
                    if (dataStr.Length > 0) dataStr.Remove(dataStr.Length - 1, 1);
                    dataStr.AppendLine();
                }

                return dataStr.ToString().TrimEnd('\n');
            }
        }

        private string GetUniqueFileName(string format)
        {
            var systemFlober = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var asseblyName = Assembly.GetEntryAssembly().GetName().Name;
            var path = Path.Combine(systemFlober, asseblyName);

            var PasteNameToPath = SelectProgram?.Type;
            foreach (var chr in Path.GetInvalidFileNameChars()) PasteNameToPath = PasteNameToPath.Replace(chr, '_');

            var newFileName = path + @"\" + PasteNameToPath + Path.GetRandomFileName() + format;

            try
            {
                Directory.Delete(path, true);
            }
            catch (DirectoryNotFoundException e)
            {
                Logger.Debug(e, $"Дериректория {path} не найдена.");
            }
            catch (IOException e)
            {
                Logger.Debug(e, "Очистить деректорию не получить по причиние, используются файлы.");
            }
            finally
            {
                Directory.CreateDirectory(path);
            }

            return newFileName;
        }

        private void LoadPlugins()
        {
            var interfaceType = typeof(IProgram);
            Type[] types = null;
            try
            {
                types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(p => p.GetTypes())
                                 .Where(p => interfaceType.IsAssignableFrom(p) && p.IsClass && !p.IsAbstract).ToArray();
            }
            catch (Exception e)
            {
                if (e is ReflectionTypeLoadException)
                {
                   (e as ReflectionTypeLoadException).LoaderExceptions.ForEach(q => Logger.Warn(q));
                }
                else
                {
                    Logger.Warn(e);
                }
             
            }

            var servicePack = new ServicePack
            {
                MessageBox = () => GetService<IMessageBoxService>(ServiceSearchMode.PreferLocal),
                ShemForm = () => GetService<ISelectionService>("ImageService"),
                QuestionText = () => GetService<ISelectionService>("QuestionTextService"),
                FreeWindow = ()=> GetService<ISelectionService>("SelectionService")
            };
            if (types == null) return;
            foreach (var type in types)
                try
                {
                    Prog.Add((IProgram) Activator.CreateInstance(type, servicePack));
                    Logger.Debug($@"Загружена сборка {type}");
                }
                catch (InvalidCastException e)
                {
                    Logger.Warn(e, $@"Не соответствует интерфейсу {type}");
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
        }

        private void OnBackCommand()
        {
            SelectedTabItem = SelectedTabItem - 1;
        }

        //todo:Добавить окно формирования документа и сделать асинхронно.
        private void OnCreatDocumetCommand()
        {
            Logger.Info(@"Запущено формирование протокола");

            try
            {
                GenerateDocument();
            }
            catch (Exception e)
            {
                Logger.Error(e, "Протокол не был сформирован");
                Alert(e);
            }
        }

        private void OnIsSpeedWorkCallback()
        {
            SelectProgram.Operation.IsSpeedWork = IsCheckWork;
            Logger.Info($@"Активировани режим ПРОВЕРКИ {IsCheckWork}");
            OnSelectProgramCallback();
        }

        private void OnNextCommand()
        {
            SelectedTabItem = SelectedTabItem + 1;
        }

        private void OnPauseCommand()
        {
            var servise = GetService<ISelectionService>("ShemService");
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

            SettingViewModel.AddresDivece = SelectProgram.Operation.SelectedOperation.AddresDevice;
        }

        private void OnSelectProgramCallback()
        {
            if (SelectProgram == null) return;
            Logger.Info($@"Выбранная операция {SelectProgram}");
            EnableOpeation = SelectProgram.Operation.EnabledOperation;
            foreach (Enum en in Enum.GetValues(typeof(OperationMetrControlBase.TypeOpeation)))
                if (EnableOpeation != null && ((Enum) EnableOpeation).HasFlag(en))
                {
                    TypeOpertion = (OperationMetrControlBase.TypeOpeation) en;
                    break;
                }

            ChangeProgram();
        }

        private async void OnStartCommand()
        {
            StateWorkFlag = StateWork.Start;
            if (_isWorkToken.Token.IsCancellationRequested) _isWorkToken = new CancellationTokenSource();
            if (SelectionItemOperation == null)
            {
                Logger.Info("Программа МК запущена");
                try
                {
                    await SelectProgram.Operation.StartWorkAsync(_isWorkToken);
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                    Alert(e);
                }
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

        #endregion Command
    }
}