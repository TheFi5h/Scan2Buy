using System;
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

        private readonly ITagDataBase _dataBase = new TagDataBase();

        public MainWindow()
        {
            InitializeComponent();

            // Initialising own procedures

            try
            {
                // Connecting to shopping cart
                Logger.GetInstance().Log("App: Connecting to shopping cart.");
                _shoppingCart = new ShoppingCart();
                Logger.GetInstance().Log("App: Connected to shopping cart.");

                Logger.GetInstance().Log("App: Connecting to database.");
                // Connect to database
                _dataBase.Connect();
                Logger.GetInstance().Log("App: Connected to databse.");
            }
            catch (Exception e)
            {
                Logger.GetInstance().Log("--Exception caught in App: " + e.Message);
            }
            

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

            Logger.GetInstance().Log($"App: {_entries.Count} new Tags scanned.");
            
            // Update grid
            DataGridEntries.Items.Refresh();
        }

        private void buttonExit_Click(object sender, RoutedEventArgs e)
        {
            // Disconnect form objects
            try
            {
                _shoppingCart.Stop();
            }
            catch (Exception ex)
            {
                Logger.GetInstance().Log("Exception caught in App: " + ex.Message);
            }

            Close();
        }

        private void DataGridEntries_OnLoaded(object sender, RoutedEventArgs e)
        {
            var grid = sender as DataGrid;

            if (grid != null)
            {
                // Set items source
                grid.ItemsSource = _entries;
            }
        }

        private void ButtonPay_Click(object sender, RoutedEventArgs e)
        {
            // Remove bought tags from database
            var scannedTags = _shoppingCart.GetScannedTags();

            try
            {
                foreach (var tagId in scannedTags)
                {
                    // Remove the tag from the db
                    _dataBase.DeleteLink(tagId);
                }
            }
            catch (Exception ex)
            {
                Logger.GetInstance().Log("-- Exception caught in App: " + ex.Message);
            }                

            // Clear the shopping cart
            _shoppingCart.Clear();

            // Reset data grid
            DataGridEntries.Items.Clear();

            LabelArticleCountVar.Content = "0";
            LabelPriceVar.Content = "0.00€";

        }
    }
}
