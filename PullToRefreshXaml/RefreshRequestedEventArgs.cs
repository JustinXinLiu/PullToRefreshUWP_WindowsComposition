using Windows.Foundation;

namespace PullToRefreshXaml
{
    public class RefreshRequestedEventArgs
    {
        internal RefreshRequestedEventArgs(DeferralCompletedHandler handler)
        {
            Handler = handler;
        }

        private DeferralCompletedHandler Handler { get; }
        private Deferral Deferral { get; set; }

        public Deferral GetDeferral() => 
            Deferral ?? (Deferral = new Deferral(Handler));
    }
}
