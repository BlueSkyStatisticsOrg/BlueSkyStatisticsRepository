using System.Windows;
using System.Windows.Media;

namespace BSky.Controls
{
    public class UIHelper
    {
        public static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            // get parent item      
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            // we’ve reached the end of the tree      
            if (parentObject == null)
                return null;
            // check if the parent matches the type we’re looking for      
            T parent = parentObject as T;
            if (parent != null)
            {
                return parent;
            }
            else
            {
                // use recursion to proceed with next level         
                return FindVisualParent<T>(parentObject);
            }
        }
    }
}
