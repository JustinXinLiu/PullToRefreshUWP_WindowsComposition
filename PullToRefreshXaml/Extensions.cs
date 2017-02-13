using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace PullToRefreshXaml
{
    public static class Extensions
    {
        public static ScrollViewer GetScrollViewer(this DependencyObject element)
        {
            var scrollViewer = element as ScrollViewer;

            if (scrollViewer != null)
            {
                return scrollViewer;
            }

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var child = VisualTreeHelper.GetChild(element, i);

                var result = GetScrollViewer(child);
                if (result == null) continue;

                return result;
            }

            return null;
        }

        public static bool AlmostEqual(this float x, double y, double tolerance = 0.01) => 
            Math.Abs(x - y) < tolerance;
    }
}
