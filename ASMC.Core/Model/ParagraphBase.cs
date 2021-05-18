using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using DevExpress.Mvvm;
using DevExpress.Mvvm.Native;
using FastMember;
using NLog;

namespace ASMC.Core.Model
{
    /// <summary>
    /// Предоставляет базовый класс для пункта операции.
    /// </summary>
    public abstract class ParagraphBase<T> : BindableBase, ITreeNode, IUserItemOperation<T>
    {

        protected const string ConstGood = "Годен";
        protected const string ConstBad = "Брак";
        protected const string ConstNotUsed = "Не выполнено";

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly TreeNode _treeNode;
        private bool _isWork;
        private bool? _isGood;
        private int _count;

        #region Property
        
        /// <summary>
        /// Позволяет задать и получить признак ускоренного выполнения операций.
        /// </summary>
        public bool IsSpeedWork { get; set; }

        /// <summary>
        /// Позволяет получить операции и необходимые настройки для ее выполнения.
        /// </summary>
        public IUserItemOperation UserItemOperation { get; }
        #endregion

        /// <summary>
        /// Инициализирует и задает экземпляр <see cref="ParagraphBase"/> класса.
        /// </summary>
        /// <param name="userItemOperation">The user item operation.</param>
        protected ParagraphBase(IUserItemOperation userItemOperation)
        {
            DataRow = (List<IBasicOperation<T>>)Activator.CreateInstance(typeof(List<IBasicOperation<T>>));
            UserItemOperation = userItemOperation;
            _treeNode = new TreeNode();
        }

        public event EndOperationHandler EndOperationEvent;

        #region Methods

        /// <summary>
        ///  Выполняет экшен и позволяет перехватить исключение указного типа.
        /// </summary>
        /// <param name="action"><see cref="Action"/> который необходимо выполнить</param>
        /// <param name="source"><see cref="CancellationTokenSource"/> используемый для отмены операции с случае ошибки.</param>
        /// <param name="logger">Необязательный параметр, позволяет логировать через логер необходимого класса.</param>
        /// <returns>A TException.</returns>
        protected TException CatchException<TException>(Action action, CancellationTokenSource source, Logger logger = null) where TException : Exception
        {
            if (source.IsCancellationRequested) return null;
            try
            {
                action.Invoke();
            }
            catch (TException e)
            {
                source.Cancel();
                if (logger != null) logger.Error(e);
                else
                {
                    Logger.Error(e);
                }
                return e;
            }

            return null;
        }

        /// <summary>
        /// Выполняет функцию и позволяет перехватить исключение указного типа.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TException"></typeparam>
        /// <param name="func">Выполняемая функция</param>
        /// <param name="source">CancellationTokenSource</param>
        /// <param name="logger">Необязательный параметр, позволяет логировать через логер необходимого класса.</param>
        /// <returns>Возвращает кортеж состоящий из результата и ожидаемого исключения.</returns>
        protected (TResult, TException) CatchException<TException,TResult>(Func<TResult> func, CancellationTokenSource source, Logger logger=null) where TException : Exception
        {
            if (source.IsCancellationRequested) return default;
            try
            {
                return (func.Invoke(), null);
            }
            catch (TException e)
            {
                source.Cancel();
                if (logger!=null) logger.Error(e);
                else
                {
                     Logger.Error(e);
                }
                return (default,e);
            }
        }
        /// <summary>
        /// Предоставляет перечень имен столбцов таблицы для отчетов
        /// </summary>
        protected virtual DataColumn[] GetColumnName()
        {
            var list = new List<DataColumn>();
            var arrNames = GenerateDataColumnTypeObject();
            foreach (var name in arrNames)
            {
                list.Add(new DataColumn(name));
            }
            return list.ToArray();
        }

        

        /// <summary>
        /// предоставляет перечень имен столбцов таблицы для столбцов типа Object.
        /// </summary>
        /// <returns></returns>
        protected virtual string[] GenerateDataColumnTypeObject()
        {
            return new[]{"Результат"};
        }
        /// <summary>
        /// Имя закладки таблички в протоколе.
        /// </summary>
        protected abstract string GetReportTableName();
        /// <summary>
        /// Заполняет свойство <see cref = "Data"/>.
        /// </summary>
        /// <returns></returns>
        protected virtual DataTable FillData()
        {
            var dt = new DataTable { TableName = GetReportTableName() };
            foreach (var cn in GetColumnName())
            {
                if (dt.Columns.IndexOf(cn)!=-1)continue;
                dt.Columns.Add(cn);
            }

            return dt;
        }
        /// <summary>
        /// Выполняет инициализацию устройств.
        /// </summary>
        protected virtual void ConnectionToDevice()
        {
           
        }
        /// <summary>
        /// Связывает строку подключения из графического интерфейса пользователя с выбранным прибором. Работает для эталонных и контролируемых
        /// приборов.
        /// </summary>
        /// <param name = "currentDevice">
        /// Прибор из списка контрольных (эталонов) или контролируемых (поверяемых/проверяемых)
        /// приборов.
        /// </param>
        /// <returns>Возвращает строку подключения для устройства.</returns>
        protected string GetStringConnect(IProtocolStringLine currentDevice)
        {
            var connect = UserItemOperation.ControlDevices
                                           .FirstOrDefault(q => string.Equals(q.SelectedDevice.UserType, currentDevice.UserType,
                                                                              StringComparison
                                                                                 .InvariantCultureIgnoreCase))
                                          ?.StringConnect ??
                          UserItemOperation.TestDevices
                                           .FirstOrDefault(q => string.Equals(q.SelectedDevice.UserType, currentDevice.UserType,
                                                                              StringComparison
                                                                                 .InvariantCultureIgnoreCase))
                                          ?.StringConnect;

            if (!string.IsNullOrEmpty(connect)) return connect;

            Logger.Warn($@"Строка подключения не указана для {currentDevice.UserType}");
            throw new ArgumentException($@"Строка подключения не указана для {currentDevice.UserType}");


        }

        /// <summary>
        /// ППозволяет получить устройство.
        /// </summary>
        /// <typeparam name="TDevice"></typeparam>
        /// <returns></returns>
        protected object GetSelectedDevice<TDevice>()
        {
            var device =  UserItemOperation.TestDevices.FirstOrDefault(q => q.SelectedDevice is TDevice) ??
                          UserItemOperation.ControlDevices.FirstOrDefault(q => q.SelectedDevice is TDevice);

            return device?.SelectedDevice;
        }
 

        private IEnumerable<object> GetProperty()
        {
            var reult = GetType().GetProperties().FirstOrDefault(q => q.Name.Equals(nameof(IUserItemOperation<object>.DataRow)))?.GetValue(this);
            return ((IList) reult)?.Cast<object>();
        }

        #endregion

        /// <summary>
        /// Позволяет получить Guid операции.
        /// </summary>
        public Guid Guid { get; } = Guid.NewGuid();

        /// <inheritdoc />
        public SchemeImage Sheme { get; set; }

        /// <inheritdoc />
        public int Count
        {
            get => _count;
            protected set => SetProperty(ref _count, value, nameof(Count));
        }


        /// <inheritdoc />
        public bool IsWork
        {
            get => _isWork;
            set=>SetProperty(ref _isWork, value, nameof(IsWork));
        }

        

        /// <inheritdoc />
        public string Name
        { get=> _treeNode.Name;
            set => _treeNode.Name = value;
        }

        /// <inheritdoc />
        public ITreeNode FirstNode => _treeNode.FirstNode;

        /// <inheritdoc />
        public ITreeNode LastNode => _treeNode.LastNode;

        /// <inheritdoc />
        public ITreeNode Parent { get => _treeNode.Parent;
            set => _treeNode.Parent = value;
        }

        /// <inheritdoc />
        public CollectionNode Nodes => _treeNode.Nodes;

        /// <inheritdoc />
        public bool? IsGood
        {
            get => _isGood;
            set => SetProperty(ref _isGood, value, nameof(IsGood));
        }

        /// <inheritdoc />
        public async Task StartSinglWorkAsync(CancellationTokenSource token, Guid guid)
        {
            Logger.Info($@"Начато выполнение пункта {Name}");
            InitWork(token);
            IsWork = true;
            try
            {
                var res = GetProperty().FirstOrDefault(q => Equals(((IBasicOperation<object>) q).Guid, guid));
                if (res != null)
                {
                    Count = 1;
                    var metod = res.GetType().GetMethods()
                                   .FirstOrDefault(q => q.Name.Equals(nameof(IBasicOperation<object>.WorkAsync)));
                    if (metod != null) await (Task) metod.Invoke(res, new object[] {token});

                }
            }
            finally
            {
                IsWork = false;
            }
        }

        


        /// <inheritdoc />
        public  async Task StartWork(CancellationTokenSource token)
        {
            Logger.Info($@"Выполняется пункт {Name}");
            InitWork(token);
            if (Parent != null) Parent.IsWork = true;
            IsWork = true;
            IsGood = null;
            var checkResult = new List<Func<bool>>();
            try
            {
                var array = GetProperty().ToArray();
                Count = array.Length;

                Logger.Debug($@"Всего строк в операции {Count}");
                foreach (var row in array)
                {
                   
                    Logger.Debug($@"Выполняется строка №{Array.IndexOf(array, row)}");

                    var metod = row.GetType().GetMethod(nameof(IBasicOperation<object>.WorkAsync));
                    if (metod != null) await (Task)metod.Invoke(row, new object[] { token });
                    checkResult.Add((Func<bool>) row.GetType().GetProperty(nameof(IBasicOperation<object>.IsGood))
                                                    .GetValue(row));
                    EndOperationEvent.Invoke(row);
                }

            }
            finally
            {
               IsGood= checkResult.All(q => q != null && q.Invoke());
               if (Parent != null)
               {
                   if (IsGood != true)
                   {
                       Parent.IsGood = false;
                   }
                   Parent.IsWork = false;
               }
                
               IsWork = false;
               Logger.Info($@"Пункт выполнился с результатом {IsGood}");
            }
        }

        /// <summary>
        /// Проводит инициализацию необходимую для реализации интерфейса <see cref = "IUserItemOperation&lt; T &gt;" />
        /// </summary>
        /// <param name="token"></param>
        /// <param name="token1"></param>
        protected virtual void InitWork(CancellationTokenSource token)
        {
            DataRow?.Clear();
            ConnectionToDevice();
        }
       

        /// <inheritdoc />
        public bool IsCheked { get; set; }

      

        /// <inheritdoc />
        public DataTable Data => FillData();

        /// <inheritdoc />
        public T[,] TestMeasPoints { get; set; }

        /// <inheritdoc />
        public List<IBasicOperation<T>> DataRow { get; set; }
    }

   
}