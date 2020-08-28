using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using DevExpress.Mvvm;
using DevExpress.Mvvm.Native;
using NLog;

namespace ASMC.Core.Model
{
    /// <summary>
    /// Предоставляет базовый клас для пункта операции.
    /// </summary>
    public abstract class ParagraphBase : ViewModelBase, IUserItemOperationBase, ITreeNode
    {
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
        protected IUserItemOperation UserItemOperation { get; }

        #endregion

        protected ParagraphBase(IUserItemOperation userItemOperation)
        {
            UserItemOperation = userItemOperation;
            _treeNode = new TreeNode();
        }

        #region Methods

        /// <summary>
        /// Заполняет свойство <see cref = "Data" />.
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
        protected string GetStringConnect(IDevice currentDevice)
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
            {
                Logger.Warn($@"Строка подключения не указана для {currentDevice.UserType}");
                throw new ArgumentException($@"Строка подключения не указана для {currentDevice.UserType}");
            }
         

            return connect;
        }

        /// <summary>
        /// Проводит инициализацию необходимую для реализации интерфейса <see cref = "IUserItemOperation&lt; T &gt;" />
        /// </summary>
        protected abstract void InitWork();

        private object[] GetProperty()
        {
            var reult = GetType().GetProperties().FirstOrDefault(q => q.Name.Equals(nameof(IUserItemOperation<object>.DataRow)))?.GetValue(this);
            return ((IList) reult)?.Cast<object>().ToArray();
        }

        #endregion

        /// <summary>
        /// Позволяет получить гуид операции.
        /// </summary>
        public Guid Guid { get; } = Guid.NewGuid();

        /// <inheritdoc />
        public ShemeImage Sheme { get; set; }

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
            private set=>SetProperty(ref _isWork, value, nameof(IsWork));
        }

        /// <inheritdoc />
        public string Name
        { get=> _treeNode.Name;
            set => _treeNode.Name = value;
        }

        /// <inheritdoc />
        public TreeNode FirstNode
        {
            get => _treeNode.FirstNode;
        }

        /// <inheritdoc />
        public TreeNode LastNode
        {
            get => _treeNode.LastNode;
        }

        /// <inheritdoc />
        public TreeNode Parent { get => _treeNode.Parent; }

        /// <inheritdoc />
        public CollectionNode Nodes { get => _treeNode.Nodes; }

        /// <inheritdoc />
        public bool? IsGood
        {
            get => _isGood;
            set => SetProperty(ref _isGood, value, nameof(IsGood));
        }

        /// <inheritdoc />
        public async Task StartSinglWork(CancellationToken token, Guid guid)
        {
            Logger.Info($@"Начато выполнение пункта {Name}");
            InitWork();
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
        public async Task StartWork(CancellationToken token)
        {
            Logger.Info($@"Выполняется пункт {Name}");
            InitWork();
            IsWork = true;
            IsGood = null;
            var checkResult = new List<Func<bool>>();
            try
            {
                var array = GetProperty().ToArray();
                Count = array.Length;

                Logger.Debug($@"Всего строк в оперции {Count}");
                foreach (var row in array)
                {
                   
                    Logger.Debug($@"Выполняется строка №{Array.IndexOf(array, row)}");
                    var metod = row.GetType().GetMethods().FirstOrDefault(q => q.Name.Equals("WorkAsync"));
                    if (metod != null) await (Task) metod.Invoke(row, new object[] {token});
                    checkResult.Add((Func<bool>) row.GetType().GetProperty(nameof(IBasicOperation<object>.IsGood))
                                                    .GetValue(row));
                }
            }
            finally
            {
               IsGood= checkResult.All(q => q != null && q.Invoke());
                IsWork = false;
                Logger.Info($@"Пункт выполнился с результатом {IsGood}");
            }
        }

        /// <inheritdoc />
        public bool IsCheked { get; set; }

        /// <inheritdoc />
        public DataTable Data => FillData();
    }
}