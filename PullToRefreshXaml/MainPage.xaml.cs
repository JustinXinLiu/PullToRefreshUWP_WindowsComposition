using PullToRefreshXaml.Extensions;
using PullToRefreshXaml.Model;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using Windows.UI.Composition;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

namespace PullToRefreshXaml
{
    public sealed partial class MainPage : Page
    {
        ScrollViewer _scrollViewer;
        readonly ObservableCollection<GroupInfoList> _list = Contact.GetContactsGrouped(250);

        CompositionPropertySet _scrollerViewerManipulation;
        ExpressionAnimation _rotationAnimation, _opacityAnimation, _offsetAnimation;

        Visual _refreshIconVisual;
        float _refreshIconOffsetY;
        const float REFRESH_ICON_MAX_OFFSET_Y = 12.0f;

        bool _refresh;
        DateTime _pulledDownTime, _restoredTime;

        public MainPage()
        {
            this.InitializeComponent();
            ContactsCVS.Source = _list;

            this.Loaded += (s, e) =>
            {
                _scrollViewer = ListView.GetScrollViewer();
                _scrollViewer.DirectManipulationStarted += OnDirectManipulationStarted;
                _scrollViewer.DirectManipulationCompleted += OnDirectManipulationCompleted;

                // Retrieve the ScrollViewer manipulation and the Compositor.
                _scrollerViewerManipulation = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(_scrollViewer);
                var compositor = _scrollerViewerManipulation.Compositor;

                // At the moment there are three things happening when pulling down the list -
                // 1. The Refresh Icon fades in.
                // 2. The Refresh Icon rotates (400°).
                // 3. The Refresh Icon gets pulled down a bit (REFRESH_ICON_MAX_OFFSET_Y)
                // QUESTION 5
                // Can we also have Geometric Path animation so we can also draw the Refresh Icon along the way?
                //

                // Create a rotation expression animation based on the overpan distance of the ScrollViewer.
                _rotationAnimation = compositor.CreateExpressionAnimation("min(max(0, ScrollManipulation.Translation.Y) * Multiplier, MaxDegree)");
                _rotationAnimation.SetScalarParameter("Multiplier", 10.0f);
                _rotationAnimation.SetScalarParameter("MaxDegree", 400.0f);
                _rotationAnimation.SetReferenceParameter("ScrollManipulation", _scrollerViewerManipulation);

                // Create an opacity expression animation based on the overpan distance of the ScrollViewer.
                _opacityAnimation = compositor.CreateExpressionAnimation("min(max(0, ScrollManipulation.Translation.Y) / Divider, 1)");
                _opacityAnimation.SetScalarParameter("Divider", 30.0f);
                _opacityAnimation.SetReferenceParameter("ScrollManipulation", _scrollerViewerManipulation);

                // Create an offset expression animation based on the overpan distance of the ScrollViewer.
                _offsetAnimation = compositor.CreateExpressionAnimation("(min(max(0, ScrollManipulation.Translation.Y) / Divider, 1)) * MaxOffsetY");
                _offsetAnimation.SetScalarParameter("Divider", 30.0f);
                _offsetAnimation.SetScalarParameter("MaxOffsetY", REFRESH_ICON_MAX_OFFSET_Y);
                _offsetAnimation.SetReferenceParameter("ScrollManipulation", _scrollerViewerManipulation);

                // Get the RefreshIcon's Visual.
                _refreshIconVisual = ElementCompositionPreview.GetElementVisual(RefreshIcon);
                // Set the center point for the rotation animation
                _refreshIconVisual.CenterPoint = new Vector3(Convert.ToSingle(RefreshIcon.ActualWidth / 2), Convert.ToSingle(RefreshIcon.ActualHeight / 2), 0);

                // Kick off the animations.
                _refreshIconVisual.StartAnimation("RotationAngleInDegrees", _rotationAnimation);
                _refreshIconVisual.StartAnimation("Opacity", _opacityAnimation);
                _refreshIconVisual.StartAnimation("Offset.Y", _offsetAnimation);
            };

            this.Unloaded += (s, e) =>
            {
                _scrollViewer.DirectManipulationStarted -= OnDirectManipulationStarted;
                _scrollViewer.DirectManipulationCompleted -= OnDirectManipulationCompleted;
            };
        }

        void OnDirectManipulationStarted(object sender, object e)
        {
            // QUESTION 1
            // I cannot think of a better way to monitor overpan changes, maybe there should be an Animating event?
            //
            Windows.UI.Xaml.Media.CompositionTarget.Rendering += OnCompositionTargetRendering;

            // Initialise the values.
            _refresh = false;
        }

        void OnDirectManipulationCompleted(object sender, object e)
        {
            Windows.UI.Xaml.Media.CompositionTarget.Rendering -= OnCompositionTargetRendering;

            //Debug.WriteLine($"ScrollViewer Rollback animation duration: {(_restoredTime - _pulledDownTime).Milliseconds}");

            // The ScrollViewer's rollback animation is appx. 200ms. So if the duration between the two DateTimes we recorded earlier
            // is greater than 250ms, we should cancel the refresh.
            var cancelled = (_restoredTime - _pulledDownTime) > TimeSpan.FromMilliseconds(250);

            if (_refresh)
            {
                if (cancelled)
                {
                    Debug.WriteLine("Refresh cancelled...");
                }
                else {
                    Debug.WriteLine("Refresh now!!!");
                }
            }
        }

        void OnCompositionTargetRendering(object sender, object e)
        {
            // QUESTION 2
            // What I've noticed is that I have to manually stop and
            // start the animation otherwise the Offset.Y is 0. Why?
            //
            _refreshIconVisual.StopAnimation("Offset.Y");

            // QUESTION 3
            // Why is the Translation always (0,0,0)?
            //
            //Vector3 translation;
            //var translationStatus = _scrollerViewerManipulation.TryGetVector3("Translation", out translation);
            //switch (translationStatus)
            //{
            //    case CompositionGetValueStatus.Succeeded:
            //        Debug.WriteLine($"ScrollViewer's Translation Y: {translation.Y}");
            //        break;
            //    case CompositionGetValueStatus.TypeMismatch:
            //    case CompositionGetValueStatus.NotFound:
            //    default:
            //        break;
            //}

            _refreshIconOffsetY = _refreshIconVisual.Offset.Y;
            //Debug.WriteLine($"RefreshIcon's Offset Y: {_refreshIconOffsetY}");

            // Question 4
            // It's not always the case here as the user can pull it all the way down and then push it back up to
            // CANCEL a refresh!! Though I cannot seem to find an easy way to detect right after the finger is lifted.
            // DirectManipulationCompleted is called too late.
            // What might be really helpful is to have a DirectManipulationDelta event with velocity and other values.
            //
            // At the moment I am calculating the time difference between the list gets pulled all the way down and rolled back up.
            // 
            if (!_refresh)
            {
                _refresh = _refreshIconOffsetY == REFRESH_ICON_MAX_OFFSET_Y;
            }

            if (_refreshIconOffsetY == REFRESH_ICON_MAX_OFFSET_Y)
            {
                _pulledDownTime = DateTime.Now;
                //Debug.WriteLine($"When the list is pulled down: {_pulledDownTime}");
            }

            if (_refresh && _refreshIconOffsetY <= 1)
            {
                _restoredTime = DateTime.Now;
                //Debug.WriteLine($"When the list is back up: {_restoredTime}");
            }

            _refreshIconVisual.StartAnimation("Offset.Y", _offsetAnimation);
        }
    }
}