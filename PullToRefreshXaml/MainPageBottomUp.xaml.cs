using System;
using PullToRefreshXaml.Model;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace PullToRefreshXaml
{
    public sealed partial class MainPageBottomUp : Page
    {
        private readonly ObservableCollection<GroupInfoList> _list = Contact.GetContactsGrouped(250);

        public MainPageBottomUp()
        {
            InitializeComponent();
            ContactsCVS.Source = _list;

            RefreshCommand = new AsyncDelegateCommand<CancellationToken>(async token =>
            {
                await Task.Delay(2000, token);
                
                _list.Add(Contact.GetContactsGrouped(1)[0]);
            });
        }

        public AsyncDelegateCommand<CancellationToken> RefreshCommand { get; set; }
    }
}