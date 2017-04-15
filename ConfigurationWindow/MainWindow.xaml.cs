using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using DataAccess;
using Domain;

namespace ConfigurationWindow
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<SearchEntry> SearchEntries = new List<SearchEntry>();

        private readonly ITagDataBase _tagDb = new TagDataBase();

        private IReaderCommunicator _reader = null;

        private delegate void SetFields(TagData tagData);

        public MainWindow()
        {
            InitializeComponent();

            Logger.GetInstance().Log("CW: Connecting to database.");

            // Set up connection to database
            _tagDb.Connect();

            Logger.GetInstance().Log("CW: Connected to database.");

            // Set items source for grid
            dataGridSearch.ItemsSource = SearchEntries;

            // Set up reader communicator
            var reader = ReaderCommunicator.GetInstance();
            Logger.GetInstance().Log("CW: Connecting Reader.");
            reader.Connect();
            Logger.GetInstance().Log("CW: Connected Reader.");
            Logger.GetInstance().Log("CW: Activating scanning.");
            reader.ActivateScan();
            Logger.GetInstance().Log("CW: Scanning activated.");
            reader.NewTagScanned += ReaderOnNewTagScanned;
        }

        private void ReaderOnNewTagScanned(TagData tagData)
        {
            // Tell UI-Thread to fill fields
            textBoxArticleName.Dispatcher.BeginInvoke(new SetFields(FieldSetter), tagData);
        }

        public void FieldSetter(TagData tagData)
        {
            // Set the fields
            textBoxSearch.Text = tagData.Id;
            textBoxChipNumber.Text = tagData.Id;
            textBoxChipTimestamp.Text = tagData.TimeStamp.ToString("dd.MM.yyyy HH:mm:ss");
            textBoxChipData.Text = tagData.Data;
        }

        private void buttonSearch_Click(object sender, RoutedEventArgs e)
        {
            bool tagsFound = false;
            bool articleFound = false;

            bool textParsable;
            int parsedNumber = 0;

            // Check for db conntection
            if (!_tagDb.IsConnected())
                return;

            // Clear previous search entries
            SearchEntries.Clear();

            // Check if field is set
            if (textBoxSearch.Text == "") return;

            // Try parse given number
            textParsable = int.TryParse(textBoxSearch.Text, out parsedNumber);

            // Search by chip number
            ArticleData articleFromDb = _tagDb.GetArticleDataByTagData(textBoxSearch.Text);

            // Check return value
            if (articleFromDb != null)
            {
                articleFound = true;

                SearchEntries.Add(new SearchEntry(articleFromDb.Id.ToString(), articleFromDb.Name, articleFromDb.Note, articleFromDb.Cost,
                    "", DateTime.Now, ""));
                Logger.GetInstance().Log($"CW: Article received: {articleFromDb.Id}, {articleFromDb.Name}, {articleFromDb.Cost}, {articleFromDb.Note}");
            }

            if (textParsable)
            {
                // Search by article number
                List<TagData> tagsFromDb = _tagDb.GetTagDataByArticleData(parsedNumber);

                // Check return value
                if (tagsFromDb.Count >= 1)
                {
                    tagsFound = true;

                    foreach (var td in tagsFromDb)
                    {
                        SearchEntries.Add(new SearchEntry("", "", "", 0.00M, td.Id, td.TimeStamp, td.Data));
                        Logger.GetInstance().Log($"CW: Tag received: {td.Id}, {td.TimeStamp}, {td.Data}");
                    }
                }
            }
            

            //Update data grid
            dataGridSearch.Items.Refresh();

            // Set status label
            if (articleFound)
            {
                if (tagsFound)
                {
                    labelStatus.Content = "Status: Artikel und Tags gefunden.";
                }
                else
                {
                    labelStatus.Content = "Status: Artikel gefunden.";
                }
            }
            else
            {
                if (tagsFound)
                {
                    labelStatus.Content = "Status: Tags gefunden.";
                }
                else
                {
                    labelStatus.Content = "Status: Suche ergab keine Ergebnisse.";
                }
            }
        }

        private void buttonDeleteLink_Click(object sender, RoutedEventArgs e)
        {
            // Check for db connection
            if (!_tagDb.IsConnected())
                return;

            Logger.GetInstance().Log("CW: Deleting link.");

            // Check if field is set
            if (textBoxChipNumber.Text != "")
            {
                // Delete the entry with the given chip number
                if (_tagDb.DeleteLink(textBoxChipNumber.Text))
                {
                    // Tag could be deleted
                    labelStatus.Content = "Status: Link erfolgreich gelöscht.";
                    Logger.GetInstance().Log("CW: Deleted link.");
                }
                else
                {
                    labelStatus.Content = "Status: Link konnte nicht gelöscht werden";
                    Logger.GetInstance().Log("CW: Could not delete link.");
                }
            }
        }

        private void buttonAddLink_Click(object sender, RoutedEventArgs e)
        {
            int parsedArticleNumber;
            bool couldParseArticleNumber;
            decimal parsedArticlePrice;
            bool couldParseArticlePrice;

            try
            {
                couldParseArticleNumber = int.TryParse(textBoxArticleNumber.Text, out parsedArticleNumber);

                couldParseArticlePrice = decimal.TryParse(textBoxArticlePrice.Text, out parsedArticlePrice);

                if (couldParseArticlePrice == false || couldParseArticleNumber == false)
                {
                    labelStatus.Content = "Status: Bitte gültige Werte eingeben!";
                    return;
                }

                TagData tagData = new TagData(textBoxChipNumber.Text, DateTime.Now, textBoxChipData.Text);
                ArticleData articleData = new ArticleData();

                articleData.Id = parsedArticleNumber;
                articleData.Name = textBoxArticleName.Text;
                articleData.Cost = parsedArticlePrice;
                articleData.Note = textBoxArticleNote.Text;

                if (_tagDb.IsConnected())
                {
                    if (_tagDb.CreateLink(tagData, articleData))
                    {
                        labelStatus.Content = "Status: Link erfolgreich hinzugefügt.";
                    }
                    else
                    {
                        labelStatus.Content = "Status: Link konnte nicht hinzugefügt werden!";
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.GetInstance().Log("--Exception caught in CW: " + exception.Message);
            }
            
        }

        // Event called when closing the window
        private void ConfigurationWindow_OnClosing(object sender, CancelEventArgs e)
        {
            // Clear up objects
            // Disconnect tag database
            if(_tagDb.IsConnected())
                _tagDb.Disconnect();

            //Disconnect reader
            _reader?.Disconnect();
        }
    }

    public class SearchEntry
    {
        public string ArticleNumber { get; set; }
        public string ArticleName { get; set; }
        public decimal ArticleCost { get; set; }
        public string ArticleNote { get; set; }
        public string TagId { get; set; }
        public DateTime TagTimestamp { get; set; }
        public string TagData { get; set; }

        public SearchEntry(string articleNumber, string articleName, string articleNote, decimal articleCost,
            string tagId, DateTime tagTimestamp, string tagData)
        {
            ArticleNumber = articleNumber;
            ArticleName = articleName;
            ArticleNote = articleNote;
            ArticleCost = articleCost;
            TagId = tagId;
            TagTimestamp = tagTimestamp;
            TagData = tagData;
        }
    }
}