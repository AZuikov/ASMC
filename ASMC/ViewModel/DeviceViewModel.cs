using System.ComponentModel;
using System.Linq;
using ASMC.Common.ViewModel;
using NLog;

namespace ASMC.ViewModel
{
    public class DeviceViewModel : BaseViewModel
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields

        private string[] _addresDivece;
        private string _description;
        private bool? _isConnect;
        private string[] _name;
        private string _selectedName;
        private string _stringConnect;

        #endregion

        #region Property

        public string[] AddresDivece
        {
            get => _addresDivece;
            set => SetProperty(ref _addresDivece, value, nameof(AddresDivece), () =>
            {
                if (string.IsNullOrWhiteSpace(StringConnect)) StringConnect = AddresDivece.FirstOrDefault();
            });
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value, nameof(Description));
        }

        public bool IsCanStringConnect { get; set; }

        public bool? IsConnect
        {
            get => _isConnect;
            set => SetProperty(ref _isConnect, value, nameof(IsConnect));
        }

        public string[] Name
        {
            get => _name;
            set => SetProperty(ref _name, value, nameof(Name), () =>
            {
                if (string.IsNullOrWhiteSpace(SelectedName)) SelectedName = Name.FirstOrDefault();
            });
        }

        public string SelectedName
        {
            get => _selectedName;
            set => SetProperty(ref _selectedName, value, nameof(SelectedName));
        }

        public string StringConnect
        {
            get => _stringConnect;
            set => SetProperty(ref _stringConnect, value, nameof(StringConnect));
        }

        #endregion

        public DeviceViewModel()
        {
            PropertyChanged += DeviceViewModel_PropertyChanged;
        }

        #region Methods

        private void DeviceViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(StringConnect)))
                Logger.Info($@"Для устройства {SelectedName} указано подключение: {StringConnect}");
            else if (e.PropertyName.Equals(nameof(SelectedName))) Logger.Info($@"Выбранно устройство: {SelectedName}");
        }

        #endregion

        ~DeviceViewModel()
        {
            PropertyChanged -= DeviceViewModel_PropertyChanged;
        }
    }
}