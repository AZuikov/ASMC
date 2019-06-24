using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AP.Utils.Data;
using ASMC.Core.Interface;
using ASMC.Core.UI;
using DevExpress.Mvvm;
using NLog;

namespace ASMC.Core.ViewModel
{
    /// <summary>
    /// Базовая модель представления.
    /// </summary>
    public abstract class BaseViewModel : ViewModelBase
    {
        #region Fields

        private bool _isInitialized;
        private bool _isBusy;
        private IDataProvider _dataProvider;

        private readonly object _syncRoot = new object();

        private readonly WeakEvent<EventHandler, EventArgs> _dataProviderChanged =
            new WeakEvent<EventHandler, EventArgs>();

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        #endregion

        /// <summary>
        /// Происходит при инициализации
        /// модели представления.
        /// </summary>
        public event EventHandler Initialized;

        /// <summary>
        /// Происходит при изменении
        /// доступа к источнику данных.
        /// </summary>
        public event EventHandler DataProviderChanged
        {
            add => _dataProviderChanged.Add(value);
            remove => _dataProviderChanged.Remove(value);
        }

        /// <summary>
        /// Возвращает значение, показывающее, была
        /// ли проведена инициализация модели
        /// представления.
        /// </summary>
        public bool IsInitialized
        {
            get => _isInitialized;
            private set => SetProperty(ref _isInitialized, value, nameof(IsInitialized));
        }

        /// <summary>
        /// Возвращает истинно, если выполняется
        /// операция с данными; иначе ложно
        /// </summary>
        public bool IsBusy
        {
            get => _isBusy;
            protected set
            {
                lock (_syncRoot)
                {
                    if (SetProperty(ref _isBusy, value, nameof(IsBusy)))
                        CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        /// <summary>
        /// Возвращает или задает интерфейс
        /// доступа к источнику данных.
        /// </summary>
        public IDataProvider DataProvider
        {
            get => _dataProvider;
            set => SetProperty(ref _dataProvider, value, nameof(DataProvider),
                () => OnDataProviderChanged(_dataProvider));
        }

        #region Methods

        /// <summary>
        /// Выполняет инициализацию
        /// модели представления.
        /// </summary>
        public void Initialize()
        {
            Logger.Debug("dsadad");
            if (_isInitialized)
                return;
            IsInitialized = true;
            OnInitialized();
        }

        /// <summary>
        /// Уведомляет пользователя об исключительной
        /// ситуации в модели представления.
        /// </summary>
        /// <param name="e">Исключение, по которому необходимо
        /// вывести подробные сведения.</param>
        /// <param name="messageService">Сервис
        /// <see cref="IMessageBoxService"/>,
        /// отвечающий за вывод сообщений.</param>
        /// <returns>Возвращает истинно, если сведения по исключению
        /// были переданы в соответствующий сервис; иначе ложно.</returns>
        protected bool Alert(Exception e, IMessageBoxService messageService = null)
        {
            var service = messageService ?? GetService<IMessageBoxService>(ServiceSearchMode.PreferLocal);
            if (service == null)
                return false;

            var metod = e.TargetSite.Name;
            var _class = e.TargetSite.DeclaringType?.Assembly.FullName;
            var msg = e.Message;
            if (e.InnerException != null)
                msg += Environment.NewLine + e.InnerException.Message;
            msg += Environment.NewLine + $"Сборка: {_class}. Метод: {metod}";
            Logger.Error(e);
            service.Show(
                msg,
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return true;
        }
        
        /// <summary>
        /// Инициализирует сервис и отображает окно
        /// </summary>
        /// <param name="view">Принимает наименования представления View(Page)</param>
        /// <param name="content">Принимает объект контента ViewModel</param>
        /// <param name="key">Принимает имя формы, по умолчению имеет значение NULL</param>
        /// <param name="windowService">Сервис <see cref="IWindowService"/>, отвечающий за формирование окна.</param>
        protected bool FormShow(string view, object content, string key = null, IWindowService windowService = null)
        {
            var service = windowService ?? GetService<IWindowService>(key, ServiceSearchMode.PreferLocal);
            if (service == null)
                return false;
            service.Show(view, null, this);
            return true;
        }
        /// <summary>
        /// Закрывает окно 
        /// </summary>
        /// <param name="key">Принимает имя формы, по умолчению имеет значение NULL</param>  
        /// <param name="windowService">Сервис <see cref="IWindowService"/>, отвечающий за формирование окна.</param>
        protected bool FormClosed(string key = null, IWindowService windowService = null)
        {
            var service = windowService ?? GetService<IWindowService>(key, ServiceSearchMode.PreferLocal);
            if(service == null)
                return false;
            if(service.IsWindowAlive)
            {
                service.Close();
            }
            return true;
        }

        /// <summary>
        /// Инициирует пользовательский запрос
        /// на подтверждение операции в модели
        /// представления.
        /// </summary>
        /// <param name="message">Строка, содержащая текст
        ///     выполняемой операции для пользователя.</param>
        /// <param name="defaultResult">Задает кнопку на которую по умолчанию устанавливается фокус</param>
        /// <param name="criticalState">Задает состояние
        ///     критичного решения.</param>
        /// <param name="allowsCancel">Задает состояние
        ///     возможности выбора опции Отмена.</param>
        /// <param name="messageService">Сервис
        ///     <see cref="IMessageBoxService"/>, отвечающий за
        ///     вывод сообщений.</param>
        protected bool? Confirm(string message, bool criticalState = false,
            MessageBoxResult defaultResult = MessageBoxResult.No, bool allowsCancel = false,
            IMessageBoxService messageService = null)
        {
            var service = messageService ?? GetService<IMessageBoxService>(ServiceSearchMode.PreferLocal);
            if (service == null)
                return true;

            var result = service.Show(
                message,
                "Вопрос",
                allowsCancel ? MessageBoxButton.YesNoCancel : MessageBoxButton.YesNo,
                criticalState ? MessageBoxImage.Warning : MessageBoxImage.Asterisk, defaultResult);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    return true;
                case MessageBoxResult.No:
                    return false;
                default:
                    return null;
            }
        }
        /// <summary>
        /// Инициирует пользовательское предупреждение
        /// </summary>
        /// <param name="message">Строка, содержащая текст
        ///     предупреждения для пользователя.</param>
        /// <param name="messageService">Сервис
        ///     <see cref="IMessageBoxService"/>, отвечающий за
        ///     вывод сообщений.</param>
        /// <returns></returns>
        protected bool Warning(string message, IMessageBoxService messageService = null)
        {
            var service = messageService ?? GetService<IMessageBoxService>(ServiceSearchMode.PreferLocal);
            if(service == null)
                return false; 
            service.Show(message,"Предупреждение.", MessageBoxButton.OK, MessageBoxImage.Warning);
            return true;
        }
        /// <summary>
        /// Инициирует запуск задачи
        /// и запускает отображение выполнения процеса 
        /// представления.е
        /// </summary>
        /// <param name="message">Строка, содержащая текст сообщения.</param>
        /// <param name="caption"></param>
        /// <param name="taskToRun"></param>
        /// <param name="tokenSource">Задает состояние
        /// возможности выбора источника Отмена.</param>
        /// <param name="messageService">>Сервис
        /// <see cref="IProgressService"/>, отвечающий за
        /// отображение длительного процесса.</param> 
        /// <returns>Возвращает истинно, если задача была завершена  иначе ложно.</returns>
        protected bool StartTaskAndShowProgressService(string message, string caption, Task taskToRun,
            CancellationTokenSource tokenSource = null, IProgressService messageService = null)
        {
            var service = messageService ?? GetService<IProgressService>(ServiceSearchMode.PreferLocal);
            if (service == null)
                return false;
            service.Show(message, caption, taskToRun, tokenSource);
            return true;
        }

        /// <summary>
        /// Вызывает событие <see cref="Initialized"/>.
        /// </summary>
        protected virtual void OnInitialized()
        {
            Initialized?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Вызывает событие <see cref="DataProviderChanged"/>.
        /// </summary>
        /// <param name="dataProvider">Интерфейс доступа к
        /// источнику данных.</param>
        protected virtual void OnDataProviderChanged(IDataProvider dataProvider)
        {
            _dataProviderChanged.Raise(this, EventArgs.Empty);
        }

        /// <inheritdoc />
        protected override void OnParentViewModelChanged(object parentViewModel)
        {
            if (parentViewModel is BaseViewModel baseViewModel)
            {
                UpdateDataProvider(baseViewModel);
                baseViewModel.DataProviderChanged += ParentDataProviderChanged;
            }

            base.OnParentViewModelChanged(parentViewModel);
        }

        private void ParentDataProviderChanged(object sender, EventArgs e)
        {
            UpdateDataProvider(sender as BaseViewModel);
        }

        private void UpdateDataProvider(BaseViewModel baseViewModel)
        {
            if (baseViewModel != null)
                DataProvider = (IDataProvider) baseViewModel.DataProvider?.Clone();
        }

        #endregion
    }
}