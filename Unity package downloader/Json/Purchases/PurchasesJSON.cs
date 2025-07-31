namespace Unity_package_downloader.Json.Purchases
{
    [Serializable]
    public class PurchaseRoot
    {
        public Results[] results { get; set; }
        public int total { get; set; }
        public Category[] category { get; set; }
        public PublisherSuggest[] publisherSuggest { get; set; }
    }

    [Serializable]
    public class Results
    {
        public string id { get; set; }
        public object orderId { get; set; }
        public string grantTime { get; set; }
        public int packageId { get; set; }
        public object[] tagging { get; set; }
        public string displayName { get; set; }
        public bool isPublisherAsset { get; set; }
        public bool isHidden { get; set; }
    }

    [Serializable]
    public class Category
    {
        public string name { get; set; }
        public int count { get; set; }
    }

    [Serializable]
    public class PublisherSuggest
    {
        public string name { get; set; }
        public int count { get; set; }
    }
}
