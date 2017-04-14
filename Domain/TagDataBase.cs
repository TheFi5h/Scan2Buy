using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.Runtime.InteropServices.WindowsRuntime;
using DataAccess;

namespace Domain
{
    public class TagDataBase : ITagDataBase
    {
        // Variables
        private const string _dataBaseFileName = "../../../SQLiteTagDataBase.db";
        private SQLiteConnection _connection;
        private bool _hasOpenedConnection = false;

        // Methods

        /// <summary>
        /// Sets up a new database file with the link table
        /// </summary>
        public void SetUpDataBase()
        {
            Logger.GetInstance().Log("DB: Initialising Database...");
            if (_hasOpenedConnection)
                return;

            // Create database
            SQLiteConnection.CreateFile(_dataBaseFileName);
            Logger.GetInstance().Log("DB: Created file");

            // Check if the database is already connected
            if (!_hasOpenedConnection)
            {
                // Connect to db
                Connect();
            }

            // Create command
            SQLiteCommand command = new SQLiteCommand(_connection);

            // Specify command
            // Create table "links"
            command.CommandText = "CREATE TABLE IF NOT EXISTS links ("
                                  + "tag_id VARCHAR(30) NOT NULL PRIMARY KEY,"
                                  + "tag_timestamp VARCHAR(50),"
                                  + "tag_data VARCHAR(255),"
                                  + "article_id INTEGER NOT NULL,"
                                  + "article_name VARCHAR(100) NOT NULL,"
                                  + "article_note VARCHAR(255),"
                                  + "article_cost VARCHAR(20) NOT NULL"
                                  + ")";
            /* link
             * string tag_id (PK)
             * string tag_timestamp
             * string tag_data
             * int article_id
             * string article_name
             * string article_note
             * string article_cost
             */

            Logger.GetInstance().Log("DB: Creating table");
            // Execute query
            command.ExecuteNonQuery();
            Logger.GetInstance().Log("DB: Table created");
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
            Logger.GetInstance().Log("DB: Opening connection");

            // Open connection
            _connection.Open();
            Logger.GetInstance().Log("DB: Connection opened");
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

        public bool IsConnected()
        {
            return _hasOpenedConnection;
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
                + articleData.Id + ", " + articleData.Name + ", " + articleData.Note + ", " + articleData.Cost + ")";

            Logger.GetInstance().Log("DB: Creating link");

            return CreateAndExecuteCommand(query);
        }

        public bool DeleteLink(TagData tagData)
        {
            bool returnValue = false;
            if (!_hasOpenedConnection)
                return false;

            // Create query
            string query = $"DELETE FROM links WHERE tag_id={tagData.Id}";

            Logger.GetInstance().Log("DB: Deleting link");

            try
            {
                // Create and execute query
                returnValue = CreateAndExecuteCommand(query);
            }
            catch (Exception e)
            {
                Logger.GetInstance().Log("--Exception caught in TDB: " + e.Message);
            }

            
            return returnValue;
    }

        public bool DeleteLinks(ArticleData articleData)
        {
            bool returnValue = false;
            if (!_hasOpenedConnection)
                return false;

            // Create query
            string query = $"DELETE FROM links WHERE article_id={articleData.Id}";

            // Create and execute query
            try
            {
                returnValue = CreateAndExecuteCommand(query);
            }
            catch (Exception e)
            {
                Logger.GetInstance().Log("--Exception caught in TDB: " + e.Message);
            }
            return returnValue;
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

            SQLiteDataReader reader;

            try
            {
                // Create reader
                reader = command.ExecuteReader();
            }
            catch (Exception e)
            {
                Logger.GetInstance().Log("--Exception caught in TDB: " + e.Message);
                return null;
            }

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
