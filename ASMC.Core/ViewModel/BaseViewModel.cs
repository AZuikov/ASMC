using System;
using System.Runtime.CompilerServices;
using DevExpress.Mvvm;

namespace ASMC.Core.ViewModel
{
    public class BaseViewModel : ViewModelBase
    {
        private readonly WeakEvent<EventHandler, EventArgs> _initializing = new WeakEvent<EventHandler, EventArgs>();
        private bool _isInitialized;

        private object _settings;

        /// <summary>
        ///     Возвращает значение, показывающее, была
        ///     ли проведена инициализация модели
        ///     представления.
        /// </summary>
        public bool IsInitialized
        {
            get => _isInitialized;
            private set => SetProperty(ref _isInitialized, value);
        }

        /// <summary>
        ///     Возвращает или задает объект,
        ///     содержащий пользовательские
        ///     параметры модели представления.
        /// </summary>
        public object Settings
        {
            get => _settings;
            set => SetProperty(ref _settings, value, () => OnSettingsChanged(_settings));
        }

        /// <summary>
        ///     Выполняет инициализацию
        ///     модели представления.
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
        ///     Происходит при успешной инициализации
        ///     модели представления.
        /// </summary>
        public event EventHandler Initialized;

        /// <summary>
        ///     Происходит при инициализации
        ///     модели представления.
        /// </summary>
        public event EventHandler Initializing
        {
            add => _initializing.Add(value);
            remove => _initializing.Remove(value);
        }

        /// <summary>
        ///     Происходит при изменении объекта,
        ///     содержащего пользовательские параметры.
        /// </summary>
        public event EventHandler SettingsChanged;

        /// <summary>
        ///     Вызывает событие <see cref="Initialized" />.
        /// </summary>
        protected virtual void OnInitialized()
        {
            Initialized?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     Вызывает событие <see cref="Initializing" />.
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
        ///     Вызывает событие <see cref="SettingsChanged" />.
        /// </summary>
        /// <param name="settings">
        ///     Объект, содержащий
        ///     пользовательские параметры.
        /// </param>
        protected virtual void OnSettingsChanged(object settings)
        {
            SettingsChanged?.Invoke(this, new EventArgs());
        }

        /// <summary>
        ///     Задает значение поля для свойства.
        /// </summary>
        /// <typeparam name="T">Тип поля.</typeparam>
        /// <param name="storage">Поле свойства.</param>
        /// <param name="value">Значение свойства.</param>
        /// <param name="propertyName">Имя свойства.</param>
        protected new bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            return base.SetProperty(ref storage, value, propertyName);
        }

        /// <summary>
        ///     Задает значение поля для свойства.
        /// </summary>
        /// <typeparam name="T">Тип поля.</typeparam>
        /// <param name="storage">Поле свойства.</param>
        /// <param name="value">Значение свойства.</param>
        /// <param name="changedCallback">Метод, вызываемый при изменении значения.</param>
        /// <param name="propertyName">Имя свойства.</param>
        protected bool SetProperty<T>(ref T storage, T value, Action changedCallback,
            [CallerMemberName] string propertyName = null)
        {
            return SetProperty(ref storage, value, propertyName, changedCallback);
        }

        private void ParentInitializing(object sender, EventArgs e)
        {
            Initialize();
        }
    }
}