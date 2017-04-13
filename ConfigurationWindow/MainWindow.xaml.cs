using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Domain;
using DataAccess;

namespace ConfigurationWindow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // TODO Initialize DB
           Domain.ITagDataBase

            // TODO Initialize Reader -> on scan -> add data from chip to fields
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
}
