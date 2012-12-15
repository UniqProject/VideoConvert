using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace CheckBoxTreeViewLibrary
{
    [StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(CheckBoxTreeViewItem))]
    public class CheckBoxTreeView : TreeView
    {
        public static DependencyProperty CheckedItemsProperty =
            DependencyProperty.Register("CheckedItems",
                                        typeof(IList),
                                        typeof(CheckBoxTreeView));

        private readonly RoutedEventHandler _checkedEventHandler;
        private readonly RoutedEventHandler _uncheckedEventHandler;

        public CheckBoxTreeView()
        {
            _checkedEventHandler = CheckBoxTreeViewItemChecked;
            _uncheckedEventHandler = CheckBoxTreeViewItemUnchecked;

            DependencyPropertyDescriptor dpd =
                DependencyPropertyDescriptor.FromProperty(ItemsSourceProperty, typeof(CheckBoxTreeView));
            if (dpd != null)
            {
                dpd.AddValueChanged(this, ItemsSourceChanged);
            }
        }
        void ItemsSourceChanged(object sender, EventArgs e)
        {
            Type type = ItemsSource.GetType();
            if (ItemsSource is IList)
            {
                Type listType = typeof(ObservableCollection<>).MakeGenericType(type.GetGenericArguments()[0]);
                CheckedItems = (IList)Activator.CreateInstance(listType);
            }
        }

        internal void OnNewContainer(CheckBoxTreeViewItem newContainer)
        {
            newContainer.Checked -= _checkedEventHandler;
            newContainer.Unchecked -= _uncheckedEventHandler;
            newContainer.Checked += _checkedEventHandler;
            newContainer.Unchecked += _uncheckedEventHandler;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            CheckBoxTreeViewItem checkBoxTreeViewItem = new CheckBoxTreeViewItem();
            OnNewContainer(checkBoxTreeViewItem);
            return checkBoxTreeViewItem;
        }

        void CheckBoxTreeViewItemChecked(object sender, RoutedEventArgs e)
        {
            CheckBoxTreeViewItem checkBoxTreeViewItem = sender as CheckBoxTreeViewItem;
            Action action = () =>
            {
                if (checkBoxTreeViewItem == null) return;

                var checkedItem = checkBoxTreeViewItem.Header;
                if (!checkedItem.GetType().Name.Equals("NamedObject"))
                    CheckedItems.Add(checkedItem);
            };
            Dispatcher.BeginInvoke(action, DispatcherPriority.ContextIdle);
        }

        void CheckBoxTreeViewItemUnchecked(object sender, RoutedEventArgs e)
        {
            CheckBoxTreeViewItem checkBoxTreeViewItem = sender as CheckBoxTreeViewItem;
            
            Action action = () =>
            {
                if (checkBoxTreeViewItem == null) return;

                var uncheckedItem = checkBoxTreeViewItem.Header;
                if (!uncheckedItem.GetType().Name.Equals("NamedObject"))
                    CheckedItems.Remove(uncheckedItem);
            };
            Dispatcher.BeginInvoke(action, DispatcherPriority.ContextIdle);
            
        }

        public IList CheckedItems
        {
            get { return (IList)GetValue(CheckedItemsProperty); }
            set { SetValue(CheckedItemsProperty, value); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
// ReSharper disable UnusedMember.Local
        private void OnPropertyChanged(string propertyName)
// ReSharper restore UnusedMember.Local
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
