using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using DataAccess;
using Domain;

namespace s2b_core_wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>// Trigger event
    public partial class MainWindow : Window
    {
        private readonly ShoppingCart _shoppingCart;
        private ReadOnlyCollection<ShoppingCartEntry> _entries;
        private delegate void SetFields(ShoppingCart.NewEntryEventArgs e);


        public MainWindow()
        {
            InitializeComponent();

            // Initialising own procedures

            _shoppingCart = new ShoppingCart();                  // create new shoppingCart


            DataGridEntries.AutoGenerateColumns = true;         // automatically binds all public properties of shopping cart entry to one column each
            DataGridEntries.IsReadOnly = true;                  // So that the user cant move items 
            DataGridEntries.CanUserAddRows = false;
            DataGridEntries.CanUserDeleteRows = false;
            DataGridEntries.CanUserReorderColumns = false;
            DataGridEntries.CanUserResizeColumns = false;
            DataGridEntries.CanUserResizeRows = false;
            DataGridEntries.CanUserSortColumns = false;
            DataGridEntries.HeadersVisibility = DataGridHeadersVisibility.All;
            _shoppingCart.OnEntryChanged += ShoppingCartGuard;  // Subscribe to OnEntryChangedEvent
            _shoppingCart.Start();                              // activate Scanning for targets
        }

        public void ShoppingCartGuard(ShoppingCart.NewEntryEventArgs e)
        {
            // Call a delegate in this thread to set the values of the controls accordingly
            LabelArticleCountVar.Dispatcher.BeginInvoke(new SetFields(SetControls), e);
        }

        private void SetControls(ShoppingCart.NewEntryEventArgs e)
        {
            // Update labels
            LabelArticleCountVar.Content = _shoppingCart.GetCountAllArticles();
            LabelPriceVar.Content = _shoppingCart.GetPrice() + "€";

            // Update table
            _entries = (ReadOnlyCollection<ShoppingCartEntry>)_shoppingCart.GetEntries();   // readonly list of all entries in the cart

            // Update grid
            Logger.GetInstance().Log($"App: {_entries.Count} new Tags scanned.");
            // TODO needed?
            DataGridEntries.ItemsSource = _entries;
        }

        private void buttonExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DataGridEntries_OnLoaded(object sender, RoutedEventArgs e)
        {
            // Get items (probably still none)
            var items = _shoppingCart.GetEntries();

            var grid = sender as DataGrid;

            if (grid != null)
            {
                // Set items source
                grid.ItemsSource = items;
            }
        }
    }
}
