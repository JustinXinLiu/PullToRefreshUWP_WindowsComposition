using PullToRefreshXaml.Model;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace PullToRefreshXaml
{
    public sealed partial class MainPageTopDown : Page
    {
        private readonly ObservableCollection<GroupInfoList> _list = Contact.GetContactsGrouped(250);

        public MainPageTopDown()
        {
            InitializeComponent();
            ContactsCVS.Source = _list;
        }

        private async void OnRefreshRequested(object sender, RefreshRequestedEventArgs args)
        {
            using (args.GetDeferral())
            {
                await Task.Delay(3000);
                _list.Insert(0, Contact.GetContactsGrouped(1)[0]);
            }
        }
    }
}