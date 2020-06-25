using ASMC.Interpreter;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using DevExpress.Data.TreeList;
using DevExpress.Mvvm;

namespace ASMC.Model
{
    public class NodeOperation<T> : ViewModelBase
    {
        private bool? _isCheked;
        private string _text;
        private bool _isExpanded;
        public ObservableCollection<NodeOperation<T>> Children
        {
            get;
        }
        public ObservableCollection<NodeOperation<T>> Parent
        {
            get;
        }
        public NodeOperation()
        {
            Children = new ObservableCollection<NodeOperation<T>>();
            Parent = new ObservableCollection<NodeOperation<T>>();
        }
        public bool? IsChecked
        {
            get => _isCheked;
            set => SetProperty(ref _isCheked, value, nameof(IsChecked));
        }
        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value, nameof(Text));
        }
        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value, nameof(IsExpanded));
        }
        public T Operation { get; set; }
        public bool? Result { get; set; }
    }
}
