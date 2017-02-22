using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    class TagDataBase : ITagDataBase
    {
        private const string _dataBaseFileName = "SQLiteTagDataBase.db";
        private SQLiteConnection _connection;

        public void SetUpDataBase()
        {
            // Connect to db
            Connect();

            // Create command
            SQLiteCommand command = new SQLiteCommand(_connection);

            // Specify command
            // Create table articles
            command.CommandText = "CREATE TABLE IF NOT EXISTS articles ( id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, name VARCHAR(100) NOT NULL, note VARCHAR(255), cost VARCHAR(20) );";
            command.ExecuteNonQuery();

            // Create table tags
            command.CommandText = "CREATE TABLE IF NOT EXISTS tags ( id VARCHAR(255) NOT NULL PRIMARY KEY, timestamp VARCHAR(30) NOT NULL, data VARCHAR(255) );";
            command.ExecuteNonQuery();

            // Freeing the resources
            command.Dispose();
        }

        public void Connect()
        {
            _connection = new SQLiteConnection();
            _connection.ConnectionString = "Data Source=" + _dataBaseFileName;
            _connection.Open();
        }

        public void Disconnect()
        {
            _connection.Close();
            _connection.Dispose();
        }

        public void CreateLink(TagData tagData, ArticleData articleData)
        {
            throw new NotImplementedException();
        }

        public bool DeleteLink(TagData tagData, ArticleData articleData)
        {
            throw new NotImplementedException();
        }

        public ArticleData GetArticleDataByTagData(TagData tagData)
        {
            throw new NotImplementedException();
        }

        public ArticleData GetArticleDataByTagData(int id)
        {
            throw new NotImplementedException();
        }

        public TagData GetTagDataByArticleData(ArticleData articleData)
        {
            throw new NotImplementedException();
        }

        public TagData GetTagDataByArticleData(int id)
        {
            throw new NotImplementedException();
        }
    }
}
