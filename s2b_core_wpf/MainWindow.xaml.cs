using System;
using System.Collections.Generic;
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
        public List<DataGridEntry> Entries = new List<DataGridEntry>();
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

                // Close window
                Close();
            }

            DataGridEntries.ItemsSource = Entries;

            /*
            DataGridEntries.AutoGenerateColumns = true;         // automatically binds all public properties of shopping cart entry to one column each
            DataGridEntries.IsReadOnly = true;                  // So that the user cant move items 
            DataGridEntries.CanUserAddRows = false;
            DataGridEntries.CanUserDeleteRows = false;
            DataGridEntries.CanUserReorderColumns = false;
            DataGridEntries.CanUserResizeColumns = false;
            DataGridEntries.CanUserResizeRows = false;
            DataGridEntries.CanUserSortColumns = false;
            DataGridEntries.HeadersVisibility = DataGridHeadersVisibility.All;
            */
            _shoppingCart.OnEntryChanged += ShoppingCartGuard;  // Subscribe to OnEntryChangedEvent
            _shoppingCart.Start();                              // activate Scanning for targets
        }

        public async void ShoppingCartGuard(ShoppingCart.NewEntryEventArgs e)
        {
            try
            {
                Logger.GetInstance().Log("SCG: triggered"); // DEBUG

                // Get Scanned entries
                Entries.Clear();

                Logger.GetInstance().Log("SCG: List cleared"); // DEBUG

                foreach (var entry in _shoppingCart.GetEntries())
                {
                    Entries.Add(entry);
                }

                Logger.GetInstance().Log("SCG: Entries added");

                // Call a delegate in this thread to set the values of the controls accordingly
                await LabelArticleCountVar.Dispatcher.BeginInvoke(new SetFields(SetControls), e);
            }
            catch (Exception ex)
            {
                Logger.GetInstance().Log("--Exception caught in App: " + ex.Message);
            }
            
        }

        private void SetControls(ShoppingCart.NewEntryEventArgs e)
        {
            try
            {
                // Update labels
                Logger.GetInstance().Log("SCG: Set Controls started"); // DEBUG

                LabelArticleCountVar.Content = _shoppingCart.GetCountAllArticles();
                LabelPriceVar.Content = _shoppingCart.GetPrice() + "€";

                Logger.GetInstance().Log($"App: {Entries.Count} entries in List scanned.");

                // Update grid
                DataGridEntries.Items.Refresh();

                Logger.GetInstance().Log($"App: {Entries.Count} entries in data grid.");
            }
            catch (Exception ex)
            {
                Logger.GetInstance().Log("--Exception caugth in SetControls: " + ex.Message);
            }
            
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
            Entries.Clear();
            DataGridEntries.Items.Refresh();

            LabelArticleCountVar.Content = "0";
            LabelPriceVar.Content = "0.00€";

        }
    }
}
