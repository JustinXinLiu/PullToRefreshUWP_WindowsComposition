using System;
using PullToRefreshXaml.Model;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace PullToRefreshXaml
{
    public sealed partial class MainPageTopDown : Page
    {
        private readonly ObservableCollection<GroupInfoList> _list = Contact.GetContactsGrouped(250);

        public MainPageTopDown()
        {
            InitializeComponent();
            ContactsCVS.Source = _list;

            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(1000);
            ListView.ContainerContentChanging += OnListViewContainerContentChanging;
        }

        private async void OnRefreshRequested(object sender, RefreshRequestedEventArgs args)
        {
            using (args.GetDeferral())
            {
                await Task.Delay(2000);
                _list.Insert(0, Contact.GetContactsGrouped(1)[0]);
            }
        }

        private void OnListViewContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.ItemContainer != null && !args.InRecycleQueue && args.Phase == 0)
            {
                var colorAnimation = new ColorAnimationUsingKeyFrames
                {
                    // 'cause the new item comes in with an animation of which duration is about 300s, we add a little delay here to only
                    // animate the color after it appears.
                    BeginTime = TimeSpan.FromMilliseconds(300)
                };
                var keyFrame1 = new LinearColorKeyFrame { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(0)), Value = Colors.White };
                var keyFrame2 = new LinearColorKeyFrame { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(400)), Value = Colors.LightGray };
                var keyFrame3 = new LinearColorKeyFrame { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(1200)), Value = Colors.White };
                colorAnimation.KeyFrames.Add(keyFrame1);
                colorAnimation.KeyFrames.Add(keyFrame2);
                colorAnimation.KeyFrames.Add(keyFrame3);

                ListView.Background = new SolidColorBrush(Colors.Transparent);
                Storyboard.SetTarget(colorAnimation, args.ItemContainer);
                Storyboard.SetTargetProperty(colorAnimation, "(Control.Background).(SolidColorBrush.Color)");

                var storyboard = new Storyboard();
                storyboard.Children.Add(colorAnimation);
                storyboard.Begin();
            }
        }
    }
}