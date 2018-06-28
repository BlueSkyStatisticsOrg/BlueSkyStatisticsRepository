using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using BSky.Statistics.Common;
using System.Collections;

namespace BlueSky.UserControls
{
    /// <summary>
    /// Interaction logic for VariableList.xaml
    /// </summary>
    public partial class VariableList : ListBox
    {
        public VariableList()
        {
            InitializeComponent();
        }


        private void ListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataSourceVariable data = GetDataFromListBox(this, e.GetPosition(this)) as DataSourceVariable;
            if (data != null)
            {
                DragDropEffects effs = DragDrop.DoDragDrop(this, data, DragDropEffects.Move);
                if (effs == DragDropEffects.Move)
                {
                    IList lst = this.ItemsSource as IList;
                    lst.Remove(data);
                }

            }
        }

        private static object GetDataFromListBox(ListBox source, Point point)
        {
            UIElement element = source.InputHitTest(point) as UIElement;
            if (element != null)
            {
                object data = DependencyProperty.UnsetValue;
                while (data == DependencyProperty.UnsetValue)
                {
                    data = source.ItemContainerGenerator.ItemFromContainer(element);

                    if (data == DependencyProperty.UnsetValue)
                    {
                        element = VisualTreeHelper.GetParent(element) as UIElement;
                    }

                    if (element == source)
                    {
                        return null;
                    }
                }

                if (data != DependencyProperty.UnsetValue)
                {
                    return data;
                }
            }

            return null;
        }
        private void ListBox_Drop(object sender, DragEventArgs e)
        {
                DataSourceVariable d = e.Data.GetData(typeof(DataSourceVariable)) as DataSourceVariable;
                IList list = this.ItemsSource as IList;
                if(d != null)
                    list.Add(d);
        }

        private void ListBox_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = null != e.Data.GetData(typeof(DataSourceVariable)) ? DragDropEffects.Move : DragDropEffects.None;
        }
    }
}
