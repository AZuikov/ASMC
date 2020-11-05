using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ASMC.Data.Model.Interface;
using DevExpress.Mvvm.UI;

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
    public interface IDeviceUi
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
        IUserType[] Devices { get; set; }

        /// <summary>
        /// Позволяет задать или получить имя выбранного прибора.
        /// </summary>
        IUserType SelectedDevice { get; set; }

        
        /// <summary>
        /// Позволяет задать или получить строку подключения к прибору.
        /// </summary>
        string StringConnect { get; set; }

        #endregion
    }

    /// <inheritdoc />
    public class Device : IDeviceUi
    {
        /// <inheritdoc />
        public bool IsCanStringConnect { get; set; } = true;

        /// <inheritdoc />
        public string Description { get; set; }

        /// <inheritdoc />
        public IUserType[] Devices { get; set; }

        /// <inheritdoc />
        public IUserType SelectedDevice { get; set; }

        /// <inheritdoc />
        public string StringConnect { get; set; }

        /// <inheritdoc />
        public bool? IsConnect { get; } = true;
    }

    public interface IControlPannelDevice : IUserType
    {
        /// <summary>
        /// Предстваление управления прибором
        /// </summary>
        string DocumentType { get; }
        /// <summary>
        /// ВМ интерфеса управления прибором
        /// </summary>
        INotifyPropertyChanged ViewModel { get; }
        /// <summary>
        /// Указывает где искать представление
        /// </summary>
        Assembly Assembly { get; }

        IUserType Device { get; }

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
        /// Предоставляет результат операции
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        bool? IsGood { get; set; }

        /// <summary>
        /// Флаг указывающий, что данная операция выполняется в текущий момент.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        bool IsWork { get; }

        /// <summary>
        /// Предоставляет наименование операции
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Предоставляет инструкцию для подключения.
        /// </summary>
        ShemeImage Sheme { get; }
        /// <summary>
        /// предоставляет количество операций в пункте
        /// </summary>
        int Count { get; }

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
    /// предоставляет реализицию отборажаемые схемы.
    /// </summary>
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
        /// <summary>
        /// Функция выполняемая для проверки правельности собранной схемы.
        /// </summary>
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

}