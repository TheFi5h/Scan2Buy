using System.Collections.Generic;
using System.Linq;

namespace Domain
{
    public class ShoppingCart
    {
        private readonly List<ShoppingCartEntry> _listArticles = new List<ShoppingCartEntry>();
        // same articles are bounded to one entry with an amount > 1
        
        // Using the default Constructor


        // sums up all prices for the items
        public double GetPrice()
        {
            return _listArticles.Sum(item => item.Price);
        }

        // number of entries = number of distinct articles
        public int GetCountDistinctArticles()
        {
            return _listArticles.Count;                     
        }

        // sums up all amounts in the article list
        public int GetCountAllArticles()
        {
            return _listArticles.Sum(item => item.Amount); 
        }

        // add 1 item to the cart (increase amount if article is already inside or 
        public void AddToCart(ShoppingCartEntry newEntry)
        {
            // search in _listArticles, if article is already in the cart
            foreach (var item in _listArticles)
            {
                if (item.GetArticleNumber() == newEntry.GetArticleNumber())
                {
                    item.AddOne();  // if found, add one item (automatically recalculates the price too)
                    return;
                }
            }

            _listArticles.Add(newEntry);
        }

        // reduces the amount of the given item by one or deletes the last remaining one from the cart
        public void TakeOutOfCart(ShoppingCartEntry cartEntry)
        {
            foreach (var item in _listArticles)
            {
                if (item.GetArticleNumber() == cartEntry.GetArticleNumber())     // search for item in list
                {
                    if (item.Amount > 1)
                        item.DeleteOne();       // if there is more than 1, reduce the amount by 1

                    else
                        _listArticles.Remove(item); // if there is only one remove the whole item from the list
                    

                    return;     // we can jump out, because we have already found the item (only distinct items in the cart)
                }
            }
        }



    }
}
