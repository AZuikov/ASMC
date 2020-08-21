using ASMC.Common.ViewModel;
using ASMC.Data.Model.Interface;
using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ASMC.Data.Model
{
    public interface IUserItemOperation<T> : IUserItemOperationBase
    {
        #region Property

        List<IBasicOperation<T>> DataRow { get; set; }

        #endregion
    }

    /// <summary>
    /// Интерфейст описывающий подключаемые настройки утроств, необходимыех для выполнения операций.
    /// </summary>
    public interface IDevice
    {
        #region Property

        /// <summary>
        /// Позволяет получить описание устройства.
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Позволяет получить или задать признак возможности выбора строки подключения.
        /// </summary>
        bool IsCanStringConnect { get; set; }

        /// <summary>
        /// Позволяет получить статус подключения устройства.
        /// </summary>
        bool? IsConnect { get; }

        /// <summary>
        /// позволяет поучать или задавать перечень взаимозаменяемых устройств.
        /// </summary>
        string[] Name { get; set; }

        /// <summary>
        /// Позволяет задать или получить имя выбранного прибора.
        /// </summary>
        string SelectedName { get; set; }

        /// <summary>
        /// Позволяет задать или получить строку подключения к прибору.
        /// </summary>
        string StringConnect { get; set; }

        #endregion
    }

    /// <inheritdoc />
    public class Device : IDevice
    {
        /// <inheritdoc />
        public bool IsCanStringConnect { get; set; } = true;

        /// <inheritdoc />
        public string Description { get; set; }

        /// <inheritdoc />
        public string[] Name { get; set; }

        /// <inheritdoc />
        public string SelectedName { get; set; }

        /// <inheritdoc />
        public string StringConnect { get; set; }

        /// <inheritdoc />
        public bool? IsConnect { get; } = true;
    }

    /// <summary>
    /// Предоставляет интерфес к циклу операций метрологического контроля.
    /// Например: поверка, калибровка.
    /// </summary>
    public interface IUserItemOperation
    {
        #region Property

        /// <summary>
        /// Возвращает перечень необходимого оборудования.
        /// </summary>
        string[] Accessories { get; }

        /// <summary>
        /// Возвращает все доступные подключения
        /// </summary>
        string[] AddresDevice { get; set; }

        /// <summary>
        /// Возвращает перечень устройст используемых для МК подключаемых устройств.
        /// </summary>
        IDevice[] ControlDevices { get; set; }

        /// <summary>
        /// Имя документа который будет формироватся без формата файла.
        /// </summary>
        string DocumentName { get; }

        ServicePack ServicePack { get; }

        /// <summary>
        /// Возвращает перечень устройст подвергаемых МК.
        /// </summary>
        IDevice[] TestDevices { get; set; }

        /// <summary>
        /// Возвращает перечень операций
        /// </summary>
        IUserItemOperationBase[] UserItemOperation { get; }

        #endregion

        #region Methods

        void FindDivice();

        /// <summary>
        /// Выполняет обнавление списка устройств.
        /// </summary>
        void RefreshDevice();

        #endregion
    }

    /// <inheritdoc />
    public abstract class Operation : IUserItemOperation
    {
        protected Operation(ServicePack servicePack)
        {
            ServicePack = servicePack;
        }

        /// <inheritdoc />
        public IDevice[] TestDevices { get; set; }

        /// <inheritdoc />
        public IUserItemOperationBase[] UserItemOperation { get; set; }

        /// <inheritdoc />
        public string DocumentName { get; protected set; }

        /// <inheritdoc />
        public string[] Accessories { get; protected set; }

        /// <inheritdoc />
        public string[] AddresDevice { get; set; }

        /// <inheritdoc />
        public IDevice[] ControlDevices { get; set; }

        /// <summary>
        /// Проверяет всели эталоны подключены
        /// </summary>
        public abstract void RefreshDevice();

        /// <inheritdoc />
        public abstract void FindDivice();

        public ServicePack ServicePack { get; set; }
    }

    /// <summary>
    /// Содержит доступныйе виды Метрологического контроля.
    /// </summary>
    public class OperationMetrControlBase
    {
        /// <summary>
        /// Содержит перечесления типов операции.
        /// </summary>
        [Flags]
        public enum TypeOpeation
        {
            PeriodicVerf = 1,
            PrimaryVerf = 2,
            Calibration = 4,
            Adjustment = 8
        }
        /// <summary>
        /// Признак автоматического прохода всех операций.
        /// </summary>
        public bool IsManual { get; set; }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Property

        /// <summary>
        /// Предоставляет доступные операции.
        /// </summary>
        public TypeOpeation? EnabledOperation
        {
            get
            {
                TypeOpeation? res = null;
                if (UserItemOperationPrimaryVerf != null || SpeedUserItemOperationPrimaryVerf != null)
                    res = TypeOpeation.PrimaryVerf;
                if (UserItemOperationPeriodicVerf != null || SpeedUserItemOperationPeriodicVerf != null)
                    if (res != null)
                        res = res | TypeOpeation.PeriodicVerf;
                    else
                        res = TypeOpeation.PeriodicVerf;
                if (UserItemOperationCalibration != null || SpeedUserItemOperationCalibration != null)
                    if (res != null)
                        res |= TypeOpeation.Calibration;
                    else
                        res = TypeOpeation.Calibration;
                if (UserItemOperationAdjustment != null)
                    if (res != null)
                        res |= TypeOpeation.Adjustment;
                    else
                        res = TypeOpeation.Adjustment;
                return res;
            }
        }

        /// <summary>
        /// Позволяет задать или получить признак определяющий ускоренную работу(ПРОВЕРКА).
        /// </summary>
        public bool IsSpeedWork { get; set; }

        /// <summary>
        /// Позволяет получить выбранную операцию.
        /// </summary>
        public IUserItemOperation SelectedOperation
        {
            get
            {
                IUserItemOperation res = null;
                if (SelectedTypeOpeation.HasFlag(TypeOpeation.PrimaryVerf))
                    res = IsSpeedWork ? SpeedUserItemOperationPrimaryVerf : UserItemOperationPrimaryVerf;
                else if (SelectedTypeOpeation.HasFlag(TypeOpeation.PeriodicVerf))
                    res = IsSpeedWork ? SpeedUserItemOperationPeriodicVerf : UserItemOperationPeriodicVerf;
                else if (SelectedTypeOpeation.HasFlag(TypeOpeation.Calibration))
                    res = IsSpeedWork ? SpeedUserItemOperationCalibration : UserItemOperationCalibration;
                else if (SelectedTypeOpeation.HasFlag(TypeOpeation.Adjustment))
                    res = SelectedTypeOpeation.HasFlag(TypeOpeation.Adjustment) ? UserItemOperationAdjustment : null;

                //if (res?.UserItemOperation != null)
                //    foreach (var t in res.UserItemOperation)
                //        t.MessageBoxService = TaskMessageService;
                return res;
            }
        }

        /// <summary>
        /// Позволяет задать или получить тип операции.
        /// </summary>
        public TypeOpeation SelectedTypeOpeation { get; set; }

        protected ShemeImage LastShem { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Запускает все операции асинхронно
        /// </summary>
        /// <returns></returns>
        public async Task StartWorkAsync(CancellationTokenSource source)
        {
            foreach (var lonleyTreeNode in SelectedOperation.UserItemOperation)
                clrNode(lonleyTreeNode, source);
        }

        private async void clrNode(IUserItemOperationBase OperationsArr, CancellationTokenSource source)
        {
            try
            {
                if ( OperationsArr.IsCheked|| !IsManual)
                {
                    ShowShem(OperationsArr.Sheme);
                    await OperationsArr.StartWork(source.Token);
                }
            }
            catch (Exception e)
            {
                source.Cancel();
                source.Token.ThrowIfCancellationRequested();
                Logger.Error(e);
            }

            Logger.Debug(OperationsArr.ToString);

            var tree = (TreeNode) OperationsArr;

            if (tree.Nodes.Count != 0)
                for (var i = 0; i < tree.Nodes.Count; i++)
                    clrNode((IUserItemOperationBase) tree.Nodes[i], source);
        }

        private async void ShowShem(ShemeImage sheme)
        {
            if (sheme == null || LastShem?.Number == sheme.Number) return;
            LastShem = sheme;
            var ser = SelectedOperation.ServicePack.ShemForm;
            if (ser == null)
            {
                Logger.Error("Сервис не найден");
                throw new NullReferenceException("Сервис не найден");
            }

            ser.Entity = sheme;
            do
            {
                ser.Show();
            } while (!await sheme.ChekShem());
        }

        #endregion

        #region Operations

        /// <summary>
        /// Позволяет  задать или получить операции ускоренной калибровки.
        /// </summary>
        protected IUserItemOperation SpeedUserItemOperationCalibration { get; set; }

        /// <summary>
        /// Позволяет  задать или получить операции ускоренной переодической поверки.
        /// </summary>
        protected IUserItemOperation SpeedUserItemOperationPeriodicVerf { get; set; }

        /// <summary>
        /// Позволяет  задать или получить операции ускоренной первичной поверки.
        /// </summary>
        protected IUserItemOperation SpeedUserItemOperationPrimaryVerf { get; set; }

        /// <summary>
        /// Позволяет  задать или получить операции регрулировки
        /// </summary>
        protected IUserItemOperation UserItemOperationAdjustment { get; set; }

        /// <summary>
        /// Позволяет  задать или получить операции калибровки.
        /// </summary>
        protected IUserItemOperation UserItemOperationCalibration { get; set; }

        /// <summary>
        /// Позволяет  задать или получить операции переодической поверки.
        /// </summary>
        protected IUserItemOperation UserItemOperationPeriodicVerf { get; set; }

        /// <summary>
        /// Позволяет  задать или получить операции первичной поверки.
        /// </summary>
        protected IUserItemOperation UserItemOperationPrimaryVerf { get; set; }

        #endregion Operations
    }

    /// <summary>
    /// Предоставляет интерфес пункта(параграфа) операции
    /// </summary>
    public interface IUserItemOperationBase
    {
        #region Property

        /// <summary>
        /// Предоставляет данные для отображения операций.
        /// </summary>
        DataTable Data { get; }

        /// <summary>
        /// Предоставляет гуид операции.
        /// </summary>
        Guid Guid { get; }

        /// <summary>
        /// Флаг необходимости выполнения данной операции
        /// </summary>
        bool IsCheked { get; set; }
        /// <summary>
        /// Флаг указывающий, что данная операция выполняется в текущий момент.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        bool IsWork { get; }

        /// <summary>
        /// Предоставляет результат операции
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        bool? IsGood { get; set; }

        /// <summary>
        /// Предоставляет наименование операции
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Предоставляет инструкцию для подключения.
        /// </summary>
        ShemeImage Sheme { get; }

        TransactionDetails TransactionDetails { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Запускает выполнение операций с указаном Гуидом.
        /// </summary>
        Task StartSinglWork(CancellationToken token, Guid guid);

        /// <summary>
        /// Запускает выполнение всех операций.
        /// </summary>
        Task StartWork(CancellationToken token);

        #endregion
    }

    /// <summary>
    ///  Предоставляет базовый клас для пункта операции.
    /// </summary>
    public abstract class ParagraphBase : TreeNode, IUserItemOperationBase
    {
        #region Property

        /// <summary>
        /// Позволяет задать и получить признак ускоренного выполнения операций.
        /// </summary>
        public bool IsSpeedWork { get; set; }

        /// <summary>
        /// Позволяет получить операции и необходимые настройки для ее выполнения.
        /// </summary>
        protected IUserItemOperation UserItemOperation { get; }

        #endregion

        protected ParagraphBase(IUserItemOperation userItemOperation)
        {
            UserItemOperation = userItemOperation;
        }

        #region Methods
        /// <summary>
        /// Заполняет свойство <see cref="Data"/>.
        /// </summary>
        /// <returns></returns>
        protected abstract DataTable FillData();

        /// <summary>
        /// Связывает строку подключения из интрефеса пользователя с выбранным прибором. Работает для контрольных и контролируемых
        /// приборов.
        /// </summary>
        /// <param name = "currentDevice">
        /// Прибор из списка контрольных (эталонов) или контролируемых (поверяемых/проверяемых)
        /// приборов.
        /// </param>
        /// <returns>Возвращает строку подключения для устройства.</returns>
        protected string GetStringConnect(Devices.IDevice currentDevice)
        {
            var connect = UserItemOperation.ControlDevices
                                           .FirstOrDefault(q => string.Equals(q.SelectedName, currentDevice.UserType,
                                                                              StringComparison
                                                                                 .InvariantCultureIgnoreCase))
                                          ?.StringConnect ??
                          UserItemOperation.TestDevices
                                           .FirstOrDefault(q => string.Equals(q.SelectedName, currentDevice.UserType,
                                                                              StringComparison
                                                                                 .InvariantCultureIgnoreCase))
                                          ?.StringConnect;

            if (string.IsNullOrEmpty(connect))
                throw new ArgumentException($@"Строка подключения не указана для {currentDevice.UserType}");

            return connect;
        }
        /// <summary>
        /// Проводит инициализацию необходимую для реализации интерфейса <see cref="IUserItemOperation<T>"/> 
        /// </summary>
        protected abstract void InitWork();

        #endregion

        /// <summary>
        /// Позволяет получить гуид операции.
        /// </summary>
        public Guid Guid { get; } = Guid.NewGuid();

        /// <inheritdoc />
        public ShemeImage Sheme { get; set; }

        public TransactionDetails TransactionDetails { get; protected set; }

        /// <inheritdoc />
        public bool IsWork { get; }

        /// <inheritdoc />
        public bool? IsGood { get; set; }

        /// <inheritdoc />
        public async Task StartSinglWork(CancellationToken token, Guid guid)
        {
            InitWork();
            var res= GetProperty().FirstOrDefault(q => Equals(((IBasicOperation<object>) q).Guid, guid));
           var metod = res.GetType().GetMethods().FirstOrDefault(q => q.Name.Equals("WorkAsync"));
            await (Task)metod.Invoke(res, new Object[] { token });
        }

        private IEnumerable<object> GetProperty()
        {
            var findname = typeof(IUserItemOperation<object>).GetProperties().FirstOrDefault()?.Name;
            var reult = this.GetType().GetProperties().FirstOrDefault(q => q.Name.Equals(findname)).GetValue(this);
            return ((IList)reult).Cast<object>();

        }

        /// <inheritdoc />
        public async Task StartWork(CancellationToken token)
        {
            InitWork();
            foreach (var row in GetProperty())
            {
             var metod=   row.GetType().GetMethods().FirstOrDefault(q => q.Name.Equals("WorkAsync"));
            await (Task) metod.Invoke(row, new Object []{token});
            }


           
        }

        /// <inheritdoc />
        public bool IsCheked { get; set; }

        /// <inheritdoc />
        public DataTable Data => FillData();
    }

    public class ShemeImage
    {
        #region Fields

        private Func<Task<bool>> _chekShem;
        private string _extendedDescription;

        private string _fileName;
        private string _fileNameDescription;

        #endregion

        #region Property

        /// <summary>
        /// Позволяет получать или задавать имя сборки-папки где и искать файл
        /// </summary>
        public string AssemblyLocalName { get; set; }

        public Func<Task<bool>> ChekShem
        {
            get
            {
                if (_chekShem == null)
                {
#pragma warning disable 1998
                    return async () => true;
#pragma warning restore 1998
                }

                ;
                return _chekShem;
            }
            set => _chekShem = value;
        }

        /// <summary>
        /// Позволяет получать или задавать описание(заголовок) схемы
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Позволяет задать или получить разширенное описание.
        /// </summary>
        public string ExtendedDescription
        {
            get => _extendedDescription;
            set
            {
                if (!string.IsNullOrWhiteSpace(value)) FileNameDescription = null;
                _extendedDescription = value;
            }
        }

        /// <summary>
        /// Позволяет получать или задавать имя файла.
        /// </summary>
        public string FileName
        {
            get => _fileName;
            set
            {
                var format = Path.GetExtension(value);
                if ("".Equals(format) && !string.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException(@"Раcширение файла не обнаружино");
                _fileName = value;
            }
        }

        /// <summary>
        /// Позволяет получать или задавать имя файла раcширеного описания.
        /// </summary>
        public string FileNameDescription
        {
            get => _fileNameDescription;
            set
            {
                var format = Path.GetExtension(value);
                if ("".Equals(format) && !string.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException(@"Раcширение файла не обнаружино");

                if (!string.IsNullOrWhiteSpace(value) && format != null &&
                    !format.Equals(".rtf", StringComparison.CurrentCultureIgnoreCase))
                    throw new ArgumentNullException(@"Расширение файла не RTF");
                ExtendedDescription = null;
                _fileNameDescription = value;
            }
        }

        /// <summary>
        /// Позволяет получать или задавать номер схемы. Используется для контроля отображения схем.
        /// </summary>
        public int Number { get; set; }

        #endregion
    }

    public class TransactionDetails : BaseViewModel
    {
        public delegate void CountUpdateHandler();

        #region Fields

        private int _count;
        private int _countReady;

        #endregion

        public event CountUpdateHandler Notify;

        #region Property

        public int Count
        {
            get => _count;
            internal set => SetProperty(ref _count, value, nameof(Count), ChangedCallback);
        }

        public int CountReady
        {
            get => _countReady;
            internal set => SetProperty(ref _countReady, value, nameof(CountReady), ChangedCallback);
        }

        #endregion

        #region Methods

        private void ChangedCallback()
        {
            Notify?.Invoke();
        }

        #endregion
    }
}