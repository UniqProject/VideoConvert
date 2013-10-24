using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Reflection;

namespace CheckBoxTreeViewLibrary
{
    [StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(CheckBoxTreeViewItem))]
    public class CheckBoxTreeViewItem : TreeViewItem
    {
        public static readonly RoutedEvent CheckedEvent = EventManager.RegisterRoutedEvent("Checked",
            RoutingStrategy.Direct,
            typeof(RoutedEventHandler),
            typeof(CheckBoxTreeViewItem));

        public static readonly RoutedEvent UncheckedEvent = EventManager.RegisterRoutedEvent("Unchecked",
            RoutingStrategy.Direct,
            typeof(RoutedEventHandler),
            typeof(CheckBoxTreeViewItem));

        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked",
                                        typeof(bool),
                                        typeof(CheckBoxTreeViewItem),
                                        new FrameworkPropertyMetadata(false,
                                                                      FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                                                                      CheckedPropertyChanged));

        private static void CheckedPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            CheckBoxTreeViewItem checkBoxTreeViewItem = (CheckBoxTreeViewItem)source;
            if (checkBoxTreeViewItem.IsChecked)
            {
                checkBoxTreeViewItem.OnChecked(new RoutedEventArgs(CheckedEvent, checkBoxTreeViewItem));
            }
            else
            {
                checkBoxTreeViewItem.OnUnchecked(new RoutedEventArgs(UncheckedEvent, checkBoxTreeViewItem));
            }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            PropertyInfo parentTreeViewPi = typeof(TreeViewItem).GetProperty("ParentTreeView", BindingFlags.Instance | BindingFlags.NonPublic);
            CheckBoxTreeView parentCheckBoxTreeView = parentTreeViewPi.GetValue(this, null) as CheckBoxTreeView;
            CheckBoxTreeViewItem checkBoxTreeViewItem = new CheckBoxTreeViewItem();
            if (parentCheckBoxTreeView != null) 
                parentCheckBoxTreeView.OnNewContainer(checkBoxTreeViewItem);
            return checkBoxTreeViewItem;
        }

        [Category("Behavior")]
        public event RoutedEventHandler Checked
        {
            add
            {
                AddHandler(CheckedEvent, value);
            }
            remove
            {
                RemoveHandler(CheckedEvent, value);
            }
        }
        [Category("Behavior")]
        public event RoutedEventHandler Unchecked
        {
            add
            {
                AddHandler(UncheckedEvent, value);
            }
            remove
            {
                RemoveHandler(UncheckedEvent, value);
            }
        }

        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        protected virtual void OnChecked(RoutedEventArgs e)
        {
            RaiseEvent(e);
        }
        protected virtual void OnUnchecked(RoutedEventArgs e)
        {
            RaiseEvent(e);
        }
    }
}
