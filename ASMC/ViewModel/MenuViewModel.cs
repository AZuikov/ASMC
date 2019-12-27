using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using DevExpress.Mvvm;
using Palsys.Core.ViewModel;

namespace ASMC.ViewModel
{
    public class MenuViewModel : BaseViewModel
    {
        private Item _selectedItem;

        public ICommand ActivateCommand
        {
            get;
        }

        public ObservableCollection<Item> Items { get; } = new ObservableCollection<Item>();

        public Item SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value, nameof(SelectedItem));
        }

        public MenuViewModel()
        {
            ActivateCommand = new DelegateCommand<Item>(item => item?.Action?.Invoke(), item => item != null);

            Items.CollectionChanged += (sender, args) =>
            {
                if(SelectedItem == null && Items.Any())
                    SelectedItem = Items[0];
            };
        }

        public class Item
        {
            public string Caption
            {
                get;
            }

            public string Description
            {
                get;
            }

            public Action Action
            {
                get;
            }

            public Item(string caption, string description, Action action)
            {
                Caption = caption;
                Description = description;
                Action = action;
            }

            public override string ToString()
            {
                return Caption;
            }
        }
    }
}
