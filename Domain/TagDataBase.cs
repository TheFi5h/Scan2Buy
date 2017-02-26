using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    class TagDataBase : ITagDataBase
    {
        // Variables
        private const string _dataBaseFileName = "SQLiteTagDataBase.db";
        private SQLiteConnection _connection;
        private bool _hasOpenedConnection = false;

        // Methods

        /// <summary>
        /// Sets up a new database file with the link table
        /// </summary>
        public void SetUpDataBase()
        {
            // Check if the database is already connected
            if (!_hasOpenedConnection)
            {
                // Connect to db
                Connect();
                _hasOpenedConnection = true;
            }

            // Create command
            SQLiteCommand command = new SQLiteCommand(_connection);

            // Specify command
            // Create table "links"
            command.CommandText = "CREATE TABLE IF NOT EXISTS links ("
                                  + "tag_id VARCHAR(30) NOT NULL PRIMARY KEY,"
                                  + "tag_timestamp VARCHAR(50),"
                                  + "tag_data VARCHAR(255)"
                                  + "article_id INTEGER NOT NULL,"
                                  + "article_name VARCHAR(100) NOT NULL,"
                                  + "article_note VARCHAR(255),"
                                  + "article_cost VARCHAR(20) NOT NULL,"
                                  + ");";
            /* link
             * string tag_id (PK)
             * string tag_timestamp
             * string tag_data
             * int article_id
             * string article_name
             * string article_note
             * string article_cost
             */

            // Execute query
            command.ExecuteNonQuery();
            
            // Freeing the resources
            command.Dispose();
        }

        public void Connect()
        {
            // Check if the connection is already opened
            if (_hasOpenedConnection)
                return;

            // Set up connection
            _connection = new SQLiteConnection();
            _connection.ConnectionString = "Data Source=" + _dataBaseFileName;

            // Open connection
            _connection.Open();
            _hasOpenedConnection = true;
        }

        public void Disconnect()
        {
            // Check if the connection is openend
            if (_hasOpenedConnection == false)
                return;

            // Close connection
            _connection.Close();
            _hasOpenedConnection = false;

            // Free resources
            _connection.Dispose();
        }

        public bool CreateLink(TagData tagData, ArticleData articleData)
        {
            // Check if a conenction is open
            if (!_hasOpenedConnection)
                return false;

            // Create query
            string query =
                "INSERT INTO links (tag_id, tag_timestamp, tag_data, article_id, article_name, article_note, article_cost) VALUES ("
                + tagData.Id + ", " + tagData.TimeStamp.ToString("dd.MM.yyyy HH:mm:ss") + ", " + tagData.Data + ", "
                + articleData.Id + ", " + articleData.Name + ", " + articleData.Note + ", " + articleData.Cost + ");";

            return CreateAndExecuteCommand(query);
        }

        public bool DeleteLink(TagData tagData)
        {
            if (!_hasOpenedConnection)
                return false;

            // Create query
            string query = $"DELETE FROM links WHERE tag_id={tagData.Id}";

            // Create and execute query
            return CreateAndExecuteCommand(query);
        }

        public bool DeleteLinks(ArticleData articleData)
        {
            if (!_hasOpenedConnection)
                return false;

            // Create query
            string query = $"DELETE FROM links WHERE article_id={articleData.Id}";

            // Create and execute query
            return CreateAndExecuteCommand(query);
        }

        public ArticleData GetArticleDataByTagData(TagData tagData)
        {
            return GetArticleDataByTagData(Convert.ToInt32(tagData.Id));
        }

        public ArticleData GetArticleDataByTagData(int id)
        {
            // Check if the connection is open
            if (!_hasOpenedConnection)
                return null;

            // Create query
            string query = $"SELECT article_id, article_name, article_note, article_cost FROM links WHERE tag_id={id}";

            // Create command
            SQLiteCommand command = new SQLiteCommand(query, _connection);

            // Create reader
            SQLiteDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                int resultId = 0;
                decimal resultCost = 0.00M;
                ArticleData result = new ArticleData();

                // Try parse id
                if (!Int32.TryParse(reader[0].ToString(), out resultId))
                {
                    // return if not successful
                    return null;
                }

                // Try parse cost
                if (!Decimal.TryParse(reader[3].ToString(), out resultCost))
                {
                    // return null if not successful
                    return null;
                }

                // Save results in obect
                result.Id = resultId;
                result.Name = reader[1].ToString();
                result.Note = reader[2].ToString();
                result.Cost = resultCost;

                return result;
            }

            return null;
        }

        public List<TagData> GetTagDataByArticleData(ArticleData articleData)
        {
            return GetTagDataByArticleData(articleData.Id);
        }

        public List<TagData> GetTagDataByArticleData(int id)
        {
            List<TagData> resultList = new List<TagData>();

            /* link
             * string tag_id (PK)
             * string tag_timestamp
             * string tag_data
             * int article_id
             * string article_name
             * string article_note
             * string article_cost
             */

            // Check if the connection is open
            if (!_hasOpenedConnection)
                return null;

            // Create query
            string query = $"SELECT tag_id, tag_timestamp, tag_data, FROM links WHERE article_id={id}";

            // Create command
            SQLiteCommand command = new SQLiteCommand(query, _connection);

            // Create reader
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                // Add the result to the list
                resultList.Add(new TagData(
                    reader[0].ToString(),
                    DateTime.ParseExact(reader[1].ToString(), "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                    reader[2].ToString()));
            }

            return resultList;
        }

        private bool CreateAndExecuteCommand(string query)
        {
            if (!_hasOpenedConnection)
                return false;

            // Create command
            SQLiteCommand command = new SQLiteCommand(query, _connection);

            // Execute query
            command.ExecuteNonQuery();

            return true;
        }
    }
}
