namespace Domain
{
    class ArticleData      // represents the data about one distinct article
    { 
        public int Id { get; set; }         // Acts as an id of the article
        public string Name { get; set; }        // The Name of the article
        public string Note { get; set; }        // A note to the article
        public decimal Cost { get; set; }        // The cost of 1 single article
    }
}
