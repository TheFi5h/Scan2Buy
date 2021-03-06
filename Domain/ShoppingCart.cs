﻿using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using DataAccess;

namespace Domain
{
    public class ShoppingCart 
    {
        // The list of items in the shoppingCart. Only distinct entries with an amount
        private readonly List<ShoppingCartEntry> _listArticles = new List<ShoppingCartEntry>();
        private readonly IReaderCommunicator _reader = ReaderCommunicator.GetInstance();
        private readonly ITagDataBase _db = new TagDataBase();
        private readonly List<string> _tagsInCart = new List<string>();

        public delegate void NewEntryEventHandler(NewEntryEventArgs e);
        public event NewEntryEventHandler OnEntryChanged;

        // Using the default Constructor
        public ShoppingCart()
        {
            try
            {
                Logger.GetInstance().Log("SC: Connecting to database.");
                // Connect to database
                _db.Connect();
                Logger.GetInstance().Log("SC: Connected to database.");
                Logger.GetInstance().Log("SC: Connecting to reader.");
                // Connect to RFID-Reader
                _reader.Connect();
                Logger.GetInstance().Log("SC: Connected to reader.");
            }
            catch (Exception e)
            {
                Logger.GetInstance().Log("--Exception caught in SC: " + e.Message);
            }
            // Setup event
            _reader.NewTagScanned += HandleNewTag;
        }

        ~ShoppingCart()
        {
            // Disconnect reader
            _reader.Disconnect();
        }

        // Starts the reader to search for tags
        public void Start()
        {
            Logger.GetInstance().Log("SC: Activating scanning.");
            // Activate the scanner
            _reader.ActivateScan();
            Logger.GetInstance().Log("SC: Scanning activated.");
        }

        public void Stop()
        {
            // Stop the scanner
            _reader.DeactivateScan();
        }

        public void HandleNewTag(TagData tagData)
        {
            try
            {
                // Only add if not already in list of scanned tags
                if (_tagsInCart.Contains(tagData.Id))
                    return;

                // Add it to the list of scanned tags
                _tagsInCart.Add(tagData.Id);

                // Search for item with id in db
                ArticleData newArticle = _db.GetArticleDataByTagData(tagData);

                // Check null
                if (newArticle == null)
                    return;

                AddToCart(new ShoppingCartEntry(newArticle.Name, newArticle.Id, 1,
                    newArticle.Cost)); // add scanned item to cart
            }
            catch (Exception e)
            {
                Logger.GetInstance().Log("--Exception caught in SC: " + e.Message);
            }
        }

        // Sums up all prices for the items
        public decimal GetPrice()
        {
            return _listArticles.Sum(item => item.Price);
        }

        // Number of entries = number of distinct articles
        public int GetCountDistinctArticles()
        {
            return _listArticles.Count;                     
        }

        // Sums up all amounts in the article list
        public int GetCountAllArticles()
        {
            return _listArticles.Sum(item => item.Amount); 
        }

        /// <summary>
        /// Gets the entries in the shopping cart as a readonly list
        /// </summary>
        /// <returns>A list of all current ShoppingCartEntries</returns>
        public List<DataGridEntry> GetEntries()
        {
            List<DataGridEntry> entries = new List<DataGridEntry>();

            foreach (var entry in _listArticles)
            {
                entries.Add(new DataGridEntry(entry.Name, entry.Amount, entry.PricePerUnit, entry.Price));
            }

            return entries;
        }

        public List<string> GetScannedTags()
        {
            // Return copy of scanned tags
            return new List<string>(_tagsInCart);
        }

        public void Clear()
        {
            // Clear the lists
            _listArticles.Clear();
            _tagsInCart.Clear();
        }

        // Add 1 item to the cart (increase amount if article is already inside or 
        private void AddToCart(ShoppingCartEntry newEntry)
        {
            try
            {
                // Search in _listArticles, if article is already in the cart
                foreach (var item in _listArticles)
                {
                    if (item.GetArticleNumber() == newEntry.GetArticleNumber())
                    {
                        item.AddOne(); // If found, add one item (automatically recalculates the price too)
                        OnEntryChanged(
                            new NewEntryEventArgs(NewEntryEventArgs.ShoppingCartAction.Add, newEntry)); // Trigger event
                        Logger.GetInstance().Log($"Amount changed for '{item.GetArticleNumber()}'");
                        return;
                    }
                }

                _listArticles.Add(newEntry);
                OnEntryChanged(new NewEntryEventArgs(NewEntryEventArgs.ShoppingCartAction.Add,
                    newEntry)); // Trigger event
            }
            catch (Exception e)
            {
                Logger.GetInstance().Log("--Exception caught in SC: " + e.Message);
            }
        }

        // Reduces the amount of the given item by one or deletes the last remaining one from the cart
        private void TakeOutOfCart(ShoppingCartEntry cartEntry)
        {
            foreach (var item in _listArticles)
            {
                if (item.GetArticleNumber() != cartEntry.GetArticleNumber()) continue;      // If the item doesn't match -> continue searching

                if (item.Amount > 1)
                {
                    item.DeleteOne(); // If there is more than 1, reduce the amount by 1
                    OnEntryChanged(new NewEntryEventArgs(NewEntryEventArgs.ShoppingCartAction.Remove, cartEntry));      // Trigger event
                    Logger.GetInstance().Log("Item removed from ShoppingCart");
                }

                else
                {
                    _listArticles.Remove(item); // If there is only one remove the whole item from the list
                    OnEntryChanged(new NewEntryEventArgs(NewEntryEventArgs.ShoppingCartAction.Remove, cartEntry));          // Trigger event
                    Logger.GetInstance().Log("Item removed from ShoppingCart");
                }


                return;     // We can jump out, because we have already found the item (only distinct items in the cart)
            }
        }


        public class NewEntryEventArgs : EventArgs
        {
            public enum ShoppingCartAction
            {
                Add,
                Remove
            }

            public ShoppingCartAction Action { get; set; }
            public ShoppingCartEntry Entry { get; set; }

            // Constructor of the Event Args
            public NewEntryEventArgs(ShoppingCartAction shoppingCartAction, ShoppingCartEntry entry)
            {
                Action = shoppingCartAction;
                Entry = entry;
            }
        }
    }

    public class DataGridEntry
    {
        public string Name { get; set; }
        public int Amount { get; set; }
        public decimal PricePerUnit { get; set; }
        public decimal Price { get; set; }

        public DataGridEntry(string name, int amount, decimal pricePerUnit, decimal price)
        {
            Name = name;
            Amount = amount;
            PricePerUnit = pricePerUnit;
            Price = price;
        }
    }
}
