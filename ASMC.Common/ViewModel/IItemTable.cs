using System.ComponentModel;
using ASMC.Data.Model.Interface;

namespace ASMC.Common.ViewModel
{
    public interface IItemTable:INotifyPropertyChanged
    {
        string Header { get; set; }
        BindingList<ICell> Cells { get; }
        ICell Selected { get; set; }
    }
}