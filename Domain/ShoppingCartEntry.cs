using System;

namespace Domain
{
    public class ShoppingCartEntry : IComparable<ShoppingCartEntry>
    {

        public string Name { get; private set; }
        public int Amount { get; private set; }
        public decimal PricePerUnit { get; private set; }
        public decimal Price { get; private set; }

        private readonly int _articleNumber;

        public ShoppingCartEntry(string name, int articleNumber, int amount = 0, decimal pricePerUnit = 0.00M)
        {
            Name = name;
            _articleNumber = articleNumber;
            Amount = amount;
            PricePerUnit = pricePerUnit;
            Price = pricePerUnit * amount;
        }

        // Returns the article number
        public int GetArticleNumber()
        {
            return _articleNumber;
        }

        // Increases amount by 1 and recalculates the price
        public void AddOne()
        {
            // Increase amount
            Amount++;

            // Recalculate price
            Price = Amount * PricePerUnit;
        }

        // Reduces amount by 1 and recalculates the price
        public void DeleteOne()
        {
            // Check amount
            if (Amount <= 1)
                throw new InvalidOperationException("The amount must be > 1 to reduce the amount");

            // Reduce amount
            Amount--;
            
            // Recalculate price
            Price = Amount * PricePerUnit;
        }

        // Return 0 for equal, a negative integer if the name of the article comes earlier in the alphabet and visa versa
        public int CompareTo(ShoppingCartEntry other)
        {
            if (_articleNumber == other.GetArticleNumber())     // if the article number is the same the article is the same
                return 0;

            return string.CompareOrdinal(Name, other.Name);     // if not so, sort by the name
        }
    }
}
