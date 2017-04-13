using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using Domain;

namespace ConfigurationWindow
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<SearchEntry> SearchEntries = new List<SearchEntry>();

        private readonly ITagDataBase tagDb = new TagDataBase();

        private delegate void SetFields(TagData tagData);

        public MainWindow()
        {
            InitializeComponent();

            // Set up connection to database
            tagDb.Connect();

            // Set up reader communicator
            var reader = ReaderCommunicator.GetInstance();
            reader.Connect();
            reader.ActivateScan();
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
            textBoxChipTimestamp.Text = tagData.TimeStamp.ToString(CultureInfo.InvariantCulture);
            textBoxChipData.Text = tagData.Data;
        }

        private void buttonSearch_Click(object sender, RoutedEventArgs e)
        {
            // TODO Search for ArticleName, ArticleNumber and ChipNumber
        }

        private void buttonDeleteLink_Click(object sender, RoutedEventArgs e)
        {
            // TODO Delete link fromdatabase
        }

        private void buttonAddLink_Click(object sender, RoutedEventArgs e)
        {
            // TODO Add link to database
        }
    }

    public class SearchEntry
    {
        public decimal ArticleCost;
        public string ArticleData;
        public string ArticleName;
        public string ArticleNote;
        public string ArticleNumber;
        public DateTime ArticleTimestamp;
        public string TagId;


        private SearchEntry(string articleNumber, string articleName, string articleNote, decimal articleCost,
            string tagId, DateTime tagTimestamp, string tagData)
        {
            ArticleNumber = articleNumber;
            ArticleName = articleName;
            ArticleNote = articleNote;
            ArticleCost = articleCost;
            TagId = tagId;
            ArticleTimestamp = tagTimestamp;
            ArticleData = tagData;
        }
    }
}