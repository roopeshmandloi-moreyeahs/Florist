namespace SP_SGmiddleware.Models
{
    public class VersionURL
    {
        public string Id { get; set; }
        public string URL { get; set; }
        public VersionURL(string Id, string URL)
        {
            this.Id = Id;
            this.URL = URL;
        }
    }
}
