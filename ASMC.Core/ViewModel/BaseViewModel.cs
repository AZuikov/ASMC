using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using DevExpress.Mvvm;
using NLog;

namespace ASMC.Core.ViewModel
{
    public abstract class BaseViewModel : ViewModelBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region  Fields

        private readonly WeakEvent<EventHandler, EventArgs> _initializing = new WeakEvent<EventHandler, EventArgs>();
        private bool _isInitialized;


        #endregion

        /// <summary>
        /// Происходит при успешной инициализации
        /// модели представления.
        /// </summary>
        public event EventHandler Initialized;

        /// <summary>
        /// Происходит при инициализации
        /// модели представления.
        /// </summary>
        public event EventHandler Initializing
        {
            add => _initializing.Add(value);
            remove => _initializing.Remove(value);
        }


        #region Property

        /// <summary>
        /// Возвращает значение, показывающее, была
        /// ли проведена инициализация модели
        /// представления.
        /// </summary>
        public bool IsInitialized
        {
            get => _isInitialized;
            private set => SetProperty(ref _isInitialized, value);
        }


        #endregion

        #region Methods

        /// <summary>
        /// Выполняет инициализацию
        /// модели представления.
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized)
                return;

            OnInitializing();
            IsInitialized = true;
            OnInitialized();
        }

        /// <summary>
        /// Уведомляет пользователя об исключительной
        /// ситуации в модели представления.
        /// </summary>
        /// <param name = "e">
        /// Исключение, по которому необходимо
        /// вывести подробные сведения.
        /// </param>
        /// <param name = "message">Сообщение о событии.</param>
        /// <param name = "messageService">
        /// Сервис
        /// <see cref = "IMessageBoxService" />,
        /// отвечающий за вывод сообщений.
        /// </param>
        /// <returns>
        /// Возвращает истинно, если сведения по исключению
        /// были переданы в соответствующий сервис; иначе ложно.
        /// </returns>
        /// <remarks>
        /// При вызове метода также производится
        /// логирование данных по исключительной ситуации.
        /// </remarks>
        protected bool Alert(Exception e, string message = null, IMessageBoxService messageService = null)
        {
            var msg = message ?? e.Message;
            if (message == null && e.InnerException != null)
                msg += Environment.NewLine + e.InnerException.Message;

            Logger.Error(e);
            return Alert(msg, true, messageService);
        }

        /// <summary>
        /// Уведомляет о событии в модели представления,
        /// требующем внимания пользователя.
        /// </summary>
        /// <param name = "instruction">
        /// Строка, содержащая текст
        /// события для пользователя.
        /// </param>
        /// <param name = "message">
        /// Строка, содержащая дополнительное
        /// сообщение для пользователя.
        /// </param>
        /// <param name = "criticalState">
        /// Задает состояние
        /// критичного события.
        /// </param>
        /// <param name = "messageService">
        /// Сервис
        /// <see cref = "IMessageBoxService" />,
        /// отвечающий за вывод сообщений.
        /// </param>
        /// <returns>
        /// Возвращает истинно, если сведения по событию
        /// были переданы в соответствующий сервис; иначе ложно.
        /// </returns>
        protected bool Alert(string instruction, string message, bool criticalState = false,
            ITaskMessageService messageService = null)
        {
            var service = messageService ?? GetService<ITaskMessageService>(ServiceSearchMode.PreferLocal);
            if (service == null)
                return false;

            service.InstructionText = instruction;
            service.Show(
                message,
                "Ошибка",
                TaskMessageButton.Ok,
                criticalState ? TaskMessageIcon.Error : TaskMessageIcon.Warning);
            return true;
        }

        /// <summary>
        /// Уведомляет о событии в модели представления,
        /// требующем внимания пользователя.
        /// </summary>
        /// <param name = "message">Сообщение о событии.</param>
        /// <param name = "criticalState">
        /// Задает состояние
        /// критичного события.
        /// </param>
        /// <param name = "messageService">
        /// Сервис
        /// <see cref = "IMessageBoxService" />,
        /// отвечающий за вывод сообщений.
        /// </param>
        /// <returns>
        /// Возвращает истинно, если сведения по событию
        /// были переданы в соответствующий сервис; иначе ложно.
        /// </returns>
        protected bool Alert(string message, bool criticalState = false, IMessageBoxService messageService = null)
        {
            var service = messageService ?? GetService<IMessageBoxService>(ServiceSearchMode.PreferLocal);
            if (service == null)
                return false;

            service.Show(
                message,
                "Ошибка",
                MessageBoxButton.OK,
                criticalState ? MessageBoxImage.Error : MessageBoxImage.Warning);

            return true;
        }

        /// <summary>
        /// Инициирует пользовательский запрос
        /// на подтверждение операции в модели
        /// представления.
        /// </summary>
        /// <param name = "instruction">
        /// Строка, содержащая текст
        /// выполняемой операции для пользователя.
        /// </param>
        /// <param name = "message">
        /// Строка, содержащая дополнительное
        /// сообщение для пользователя.
        /// </param>
        /// <param name = "dontAskThisAgain">Отображение опции Больше не спрашивать.</param>
        /// <param name = "allowsCancel">
        /// Задает состояние
        /// возможности выбора опции Отмена.
        /// </param>
        /// <param name = "criticalState">
        /// Задает состояние
        /// критичного решения.
        /// </param>
        /// <param name = "taskService">
        /// Сервис
        /// <see cref = "ASMC.Core.ITaskMessageService" />, отвечающий за
        /// вывод сообщений.
        /// </param>
        protected bool? Confirm(string instruction, string message, ref bool dontAskThisAgain, bool allowsCancel = true,
            bool criticalState = false, ITaskMessageService taskService = null)
        {
            var service = taskService ?? GetService<ITaskMessageService>(ServiceSearchMode.PreferLocal);
            if (service == null)
                return true;

            if (dontAskThisAgain)
            {
                service.FooterCheckBoxText = "Больше не спрашивать";
                service.FooterCheckBoxChecked = false;
            }

            service.InstructionText = instruction;
            var result = service.Show(
                message,
                "Вопрос",
                allowsCancel
                    ? TaskMessageButton.Yes | TaskMessageButton.No | TaskMessageButton.Cancel
                    : TaskMessageButton.Yes | TaskMessageButton.No,
                criticalState ? TaskMessageIcon.Warning : TaskMessageIcon.None);

            dontAskThisAgain = service.FooterCheckBoxChecked == true;
            switch (result)
            {
                case TaskMessageResult.Yes:
                    return true;
                case TaskMessageResult.No:
                    return false;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Инициирует пользовательский запрос
        /// на подтверждение операции в модели
        /// представления.
        /// </summary>
        /// <param name = "message">
        /// Строка, содержащая текст
        /// выполняемой операции для пользователя.
        /// </param>
        /// <param name = "allowsCancel">
        /// Задает состояние
        /// возможности выбора опции Отмена.
        /// </param>
        /// <param name = "criticalState">
        /// Задает состояние
        /// критичного решения.
        /// </param>
        /// <param name = "messageService">
        /// Сервис
        /// <see cref = "IMessageBoxService" />, отвечающий за
        /// вывод сообщений.
        /// </param>
        protected bool? Confirm(string message, bool allowsCancel = true, bool criticalState = false,
            IMessageBoxService messageService = null)
        {
            var service = messageService ?? GetService<IMessageBoxService>(ServiceSearchMode.PreferLocal);
            if (service == null)
                return true;

            var result = service.Show(
                message,
                "Вопрос",
                allowsCancel ? MessageBoxButton.YesNoCancel : MessageBoxButton.YesNo,
                criticalState ? MessageBoxImage.Warning : MessageBoxImage.Question,
                allowsCancel ? MessageBoxResult.Cancel : MessageBoxResult.No);

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
        /// Инициирует пользовательский запрос
        /// на подтверждение операции в модели
        /// представления.
        /// </summary>
        /// <param name = "instruction">
        /// Строка, содержащая текст
        /// выполняемой операции для пользователя.
        /// </param>
        /// <param name = "message">
        /// Строка, содержащая дополнительное
        /// сообщение для пользователя.
        /// </param>
        /// <param name = "allowsCancel">
        /// Задает состояние
        /// возможности выбора опции Отмена.
        /// </param>
        /// <param name = "criticalState">
        /// Задает состояние
        /// критичного решения.
        /// </param>
        /// <param name = "taskService">
        /// Сервис
        /// <see cref = "ASMC.Core.ITaskMessageService" />, отвечающий за
        /// вывод сообщений.
        /// </param>
        protected bool? Confirm(string instruction, string message, bool allowsCancel = true,
            bool criticalState = false, ITaskMessageService taskService = null)
        {
            var dontAskThisAgain = false;
            return Confirm(instruction, message, ref dontAskThisAgain, allowsCancel, criticalState, taskService);
        }

        /// <summary>
        /// Уведомляет пользователя о событии в
        /// модели представления.
        /// </summary>
        /// <param name = "message">Сообщение о событии.</param>
        /// <param name = "messageService">
        /// Сервис
        /// <see cref = "IMessageBoxService" />,
        /// отвечающий за вывод сообщений.
        /// </param>
        /// <returns>
        /// Возвращает истинно, если сведения по событию
        /// были переданы в соответствующий сервис; иначе ложно.
        /// </returns>
        protected bool Message(string message, IMessageBoxService messageService = null)
        {
            var service = messageService ?? GetService<IMessageBoxService>(ServiceSearchMode.PreferLocal);
            if (service == null)
                return false;

            service.Show(
                message,
                null,
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return true;
        }

        /// <summary>
        /// Уведомляет пользователя о событии в
        /// модели представления.
        /// </summary>
        /// <param name = "instruction">
        /// Строка, содержащая текст
        /// события для пользователя.
        /// </param>
        /// <param name = "message">
        /// Строка, содержащая дополнительное
        /// сообщение для пользователя.
        /// </param>
        /// <param name = "messageService">
        /// Сервис
        /// <see cref = "IMessageBoxService" />,
        /// отвечающий за вывод сообщений.
        /// </param>
        /// <returns>
        /// Возвращает истинно, если сведения по событию
        /// были переданы в соответствующий сервис; иначе ложно.
        /// </returns>
        protected bool Message(string instruction, string message, ITaskMessageService messageService = null)
        {
            var service = messageService ?? GetService<ITaskMessageService>(ServiceSearchMode.PreferLocal);
            if (service == null)
                return false;

            service.InstructionText = instruction;
            service.Show(
                message,
                null,
                TaskMessageButton.Ok,
                TaskMessageIcon.Information);

            return true;
        }

        /// <summary>
        /// Вызывает событие <see cref = "Initialized" />.
        /// </summary>
        protected virtual void OnInitialized()
        {
            Initialized?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Вызывает событие <see cref = "Initializing" />.
        /// </summary>
        protected virtual void OnInitializing()
        {
            _initializing.Raise(this, EventArgs.Empty);
        }

        /// <inheritdoc />
        protected override void OnParentViewModelChanged(object parentViewModel)
        {
            if (parentViewModel is BaseViewModel baseViewModel) baseViewModel.Initializing += ParentInitializing;

            base.OnParentViewModelChanged(parentViewModel);
        }

   

        /// <summary>
        /// Задает значение поля для свойства.
        /// </summary>
        /// <typeparam name = "T">Тип поля.</typeparam>
        /// <param name = "storage">Поле свойства.</param>
        /// <param name = "value">Значение свойства.</param>
        /// <param name = "propertyName">Имя свойства.</param>
        protected new bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            return base.SetProperty(ref storage, value, propertyName);
        }

        /// <summary>
        /// Задает значение поля для свойства.
        /// </summary>
        /// <typeparam name = "T">Тип поля.</typeparam>
        /// <param name = "storage">Поле свойства.</param>
        /// <param name = "value">Значение свойства.</param>
        /// <param name = "changedCallback">Метод, вызываемый при изменении значения.</param>
        /// <param name = "propertyName">Имя свойства.</param>
        protected bool SetProperty<T>(ref T storage, T value, Action changedCallback,
            [CallerMemberName] string propertyName = null)
        {
            return SetProperty(ref storage, value, propertyName, changedCallback);
        }

        private void ParentInitializing(object sender, EventArgs e)
        {
            Initialize();
        }

        #endregion

    }
}