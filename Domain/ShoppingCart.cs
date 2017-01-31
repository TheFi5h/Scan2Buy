﻿using System;
using System.Collections.Generic;
using System.Linq;
using DataAccess;

namespace Domain
{
    public class ShoppingCart 
    {
        // The list of items in the shoppingCart. Only distinct entries with an amount
        private readonly List<ShoppingCartEntry> _listArticles = new List<ShoppingCartEntry>();
        private readonly IReaderCommunicator _reader = ReaderCommunicator.GetInstance();

        public delegate void NewEntryEventHandler(NewEntryEventArgs e);
        public event NewEntryEventHandler OnEntryChanged;

        // Using the default Constructor
        public ShoppingCart()
        {
            // TODO start database for tag/article data
            
            _reader.Connect();
            _reader.NewTagScanned += HandleNewTag;
        }

        ~ShoppingCart()
        {
            _reader.Disconnect();
        }

        // Starts the reader to search for tags
        public void Start()
        {
            _reader.ActivateScan();
        }

        public void Stop()
        {
            _reader.DeactivateScan();
        }

        public void HandleNewTag(TagData tagData)
        {
            // Search for item with id in db
            // TODO
            // i.e. db.getEntry(tagData.Id);

            string name = "artikel";
            int articleNumber = Convert.ToInt32(tagData.Id);
            int amount = 1;
            double pricePerUnit = 3.98f;

           AddToCart(new ShoppingCartEntry(name, articleNumber, amount, pricePerUnit));     // add scanned item to cart
        }

        // Sums up all prices for the items
        public double GetPrice()
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
        public IReadOnlyCollection<ShoppingCartEntry> GetEntries()
        {
            return _listArticles.AsReadOnly();
        }

        // Add 1 item to the cart (increase amount if article is already inside or 
        private void AddToCart(ShoppingCartEntry newEntry)
        {
            // Search in _listArticles, if article is already in the cart
            foreach (var item in _listArticles)
            {
                if (item.GetArticleNumber() == newEntry.GetArticleNumber())
                {
                    item.AddOne();  // If found, add one item (automatically recalculates the price too)
                    OnEntryChanged(new NewEntryEventArgs(NewEntryEventArgs.ShoppingCartAction.Add, newEntry));  // Trigger event
                    Logger.GetInstance().Log("Item added to ShoppingCart");
                    return;
                }
            }

            _listArticles.Add(newEntry);
            OnEntryChanged(new NewEntryEventArgs(NewEntryEventArgs.ShoppingCartAction.Add, newEntry));  // Trigger event
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
            //TODO specify what the subscriber eventually needs
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
}