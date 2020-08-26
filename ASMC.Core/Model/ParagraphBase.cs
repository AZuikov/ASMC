using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ASMC.Common.Model;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;

namespace ASMC.Core.Model
{
    /// <summary>
    /// Предоставляет базовый клас для пункта операции.
    /// </summary>
    public abstract class ParagraphBase : TreeNode, IUserItemOperationBase
    {
        private bool _isWork;
        private bool? _isGood;

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
                throw new ArgumentException($@"Строка подключения не указана для {currentDevice.UserType}");

            return connect;
        }

        /// <summary>
        /// Проводит инициализацию необходимую для реализации интерфейса <see cref = "IUserItemOperation&lt; T &gt;" />
        /// </summary>
        protected abstract void InitWork();

        private IEnumerable<object> GetProperty()
        {
            var reult = GetType().GetProperties().FirstOrDefault(q => q.Name.Equals(nameof(IUserItemOperation<object>.DataRow)))?.GetValue(this);
            return ((IList) reult)?.Cast<object>();
        }

        #endregion

        /// <summary>
        /// Позволяет получить гуид операции.
        /// </summary>
        public Guid Guid { get; } = Guid.NewGuid();

        /// <inheritdoc />
        public ShemeImage Sheme { get; set; }



        /// <inheritdoc />
        public bool IsWork
        {

            get => _isWork;
            private set=>SetProperty(ref _isWork, value, nameof(IsWork));
        }

        /// <inheritdoc />
        public bool? IsGood
        {
            get => _isWork;
            set => SetProperty(ref _isGood, value, nameof(IsGood));
        }

        /// <inheritdoc />
        public async Task StartSinglWork(CancellationToken token, Guid guid)
        {
            InitWork();
            IsWork = true;
            try
            {
                var res = GetProperty().FirstOrDefault(q => Equals(((IBasicOperation<object>) q).Guid, guid));
                if (res != null)
                {
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
            InitWork();
            IsWork = true;
            IsGood = null;
            var checkResult = new List<Func<bool>>();
            try
            {
                foreach (var row in GetProperty())
                {
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
            }
        }

        /// <inheritdoc />
        public bool IsCheked { get; set; }

        /// <inheritdoc />
        public DataTable Data => FillData();
    }
}