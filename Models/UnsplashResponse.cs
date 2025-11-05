namespace PetConnect.Models // O tu namespace
{
    public class UnsplashSearchResponse
    {
        public List<UnsplashPhoto> Results { get; set; }
    }

    public class UnsplashPhoto
    {
        public string Id { get; set; }
        public UnsplashUrls Urls { get; set; }
        public UnsplashUser User { get; set; }
    }

    public class UnsplashUrls
    {
        public string Full { get; set; }
        public string Regular { get; set; }
        public string Small { get; set; }
    }
    
    public class UnsplashUser
    {
        public string Name { get; set; }
    }
}