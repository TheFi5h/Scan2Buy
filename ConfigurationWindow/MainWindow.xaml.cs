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

            // Set up reader communicator
            /*
            var reader = ReaderCommunicator.GetInstance();
            Logger.GetInstance().Log("CW: Connecting Reader.");
            reader.Connect();
            Logger.GetInstance().Log("CW: Connected Reader.");
            Logger.GetInstance().Log("CW: Activating scanning.");
            reader.ActivateScan();
            Logger.GetInstance().Log("CW: Scanning activated.");
            reader.NewTagScanned += ReaderOnNewTagScanned;
            */
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
            textBoxChipTimestamp.Text = tagData.TimeStamp.ToString(CultureInfo.InvariantCulture);
            textBoxChipData.Text = tagData.Data;
        }

        private void buttonSearch_Click(object sender, RoutedEventArgs e)
        {
            bool tagsFound = false;
            bool articleFound = false;

            // Check for db conntection
            if (!_tagDb.IsConnected())
                return;

            // Clear previous search entries
            SearchEntries.Clear();

            // Check which fields are set
            if (textBoxSearch.Text != "")
            {
                int parsedNumber = 0;

                // Try parse given number
                if (!Int32.TryParse(textBoxSearch.Text, out parsedNumber))
                {
                    labelStatus.Content = "Status: Bitte gültige Nummer eingeben!";
                    return;
                }

                // Search by chip number
                ArticleData articleFromDb = _tagDb.GetArticleDataByTagData(parsedNumber);

                // Check return value
                if (articleFromDb != null)
                {
                    articleFound = true;

                    SearchEntries.Add(new SearchEntry(articleFromDb.Id.ToString(), articleFromDb.Name, articleFromDb.Note, articleFromDb.Cost,
                        "", DateTime.Now, ""));
                }

                // Search by article number
                List<TagData> tagsFromDb = _tagDb.GetTagDataByArticleData(parsedNumber);

                // Check return value
                if (tagsFromDb.Count >= 1)
                {
                    tagsFound = true;

                    foreach (var td in tagsFromDb)
                    {
                        SearchEntries.Add(new SearchEntry("", "", "", 0.00M, td.Id, td.TimeStamp, td.Data));
                    }
                }

                //Update data grid
                dataGridSearch.ItemsSource = SearchEntries;

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

                        //DEBUG
                        Logger.GetInstance().Log($"CW: Article received: {articleFromDb.Id}, {articleFromDb.Name}, {articleFromDb.Cost}, {articleFromDb.Note}");
                    }
                }
                else
                {
                    if (tagsFound)
                    {
                        labelStatus.Content = "Status: Tags gefunden.";

                        //DEBUG
                        foreach (var tag in tagsFromDb)
                        {
                            Logger.GetInstance().Log($"CW: Tag received: {tag.Id}, {tag.TimeStamp}, {tag.Data}");
                        }
                    }
                    else
                    {
                        labelStatus.Content = "Status: Suche ergab keine Ergebnisse.";
                    }
                }

            }
        }

        private void buttonDeleteLink_Click(object sender, RoutedEventArgs e)
        {
            int parsedTagId = 0;

            // Check for db connection
            if (!_tagDb.IsConnected())
                return;

            // Try parsing the text to an int
            if (!Int32.TryParse(textBoxChipNumber.Text, out parsedTagId))
            {
                // If parsing not successful
                labelStatus.Content = "Status: Bitte gültige Id eingeben!";
                return;
            }
                

            // Check if field is set
            if (textBoxChipNumber.Text != "")
            {
                // Delete the entry with the given chip number
                if (_tagDb.DeleteLink(parsedTagId))
                {
                    // Tag could be deleted
                    labelStatus.Content = "Status: Link erfolgreich gelöscht.";
                }
                else
                {
                    labelStatus.Content = "Status: Link konnte nicht gelöscht werden";
                }
            }
        }

        private void buttonAddLink_Click(object sender, RoutedEventArgs e)
        {
            TagData tagData = new TagData(textBoxChipNumber.Text, DateTime.Now, textBoxChipData.Text);
            ArticleData articleData = new ArticleData();
            articleData.Id = Convert.ToInt32(textBoxArticleNumber.Text);
            articleData.Name = textBoxArticleName.Text;
            articleData.Cost = Convert.ToDecimal(textBoxArticlePrice.Text);
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

        // Event called when closing the window
        private void ConfigurationWindow_OnClosing(object sender, CancelEventArgs e)
        {
            // Clear up objects
            if(_tagDb.IsConnected())
                _tagDb.Disconnect();

            
            reader.Disconnect();
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