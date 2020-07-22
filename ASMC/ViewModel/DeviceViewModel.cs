using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Core.ViewModel;

namespace ASMC.ViewModel
{
    public class DeviceViewModel : BaseViewModel 
    {
        private string _stringConnect;
        private string _selectedName;
        private string[] _name;
        private bool? _isConnect;
        private string _description;
        private string[] _addresDivece;

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value, nameof(Description));
        }
        public bool? IsConnect
        {
            get => _isConnect;
            set => SetProperty(ref _isConnect, value, nameof(IsConnect));
        }
        public string[] AddresDivece
        {
            get => _addresDivece;
            set => SetProperty(ref _addresDivece, value, nameof(AddresDivece), () => { if(string.IsNullOrWhiteSpace(StringConnect)) StringConnect = AddresDivece.FirstOrDefault(); });
        }
        public string[] Name
        {
            get => _name;
            set => SetProperty(ref _name, value, nameof(Name), ()=> { if(string.IsNullOrWhiteSpace(SelectedName))  SelectedName = Name.FirstOrDefault(); });
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
    }
}
