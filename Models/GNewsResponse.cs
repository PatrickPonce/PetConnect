namespace PetConnect.Models // O el namespace que estés usando
{
    public class GNewsResponse
    {
        public int TotalArticles { get; set; }
        public List<GNewsArticle> Articles { get; set; }
    }

    public class GNewsArticle
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public string Url { get; set; }
        public string Image { get; set; }
        public GNewsSource Source { get; set; }
    }
    public class GNewsSource
    {
        public string Name { get; set; }
    }
}