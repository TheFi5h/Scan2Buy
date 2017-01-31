using System;

namespace Domain
{
    public class ShoppingCartEntry : IComparable<ShoppingCartEntry>
    {

        public string Name { get; private set; }
        public int Amount { get; private set; }
        public double PricePerUnit { get; private set; }
        public double Price { get; private set; }

        private readonly string _articleNumber;

        public ShoppingCartEntry(string name, string articleNumber, int amount = 0, double pricePerUnit = 0)
        {
            Name = name;
            _articleNumber = articleNumber;

            Amount = amount;
            PricePerUnit = pricePerUnit;
            Price = pricePerUnit * amount;

        }

        public string GetArticleNumber()
        {
            return _articleNumber;
        }

        // increases amount by 1 and recalculates the price
        public void AddOne()
        {
            Amount++;
            Price = Amount * PricePerUnit;
        }

        // reduces amount by 1 and recalculates the price
        public void DeleteOne()
        {
            //TODO if 0
            Amount--;
            Price = Amount * PricePerUnit;
        }

        // return 0 for equal, a negative integer if the name of the article comes earlier in the alphabet and visa versa
        public int CompareTo(ShoppingCartEntry other)
        {
            if (_articleNumber == other.GetArticleNumber())     // if the article number is the same the article is the same
                return 0;

            return string.CompareOrdinal(Name, other.Name);     // if not so, sort by the name
        }
    }
}
