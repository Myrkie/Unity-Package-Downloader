namespace Unity_package_downloader.Json.ProductInfo
{
    [Serializable]
    public class ProductInfoRoot
    {
        public Result result { get; set; }
    }

    [Serializable]
    public class Result
    {
        public Download download { get; set; }
    }

    [Serializable]
    public class Download
    {
        public string filename_safe_category_name { get; set; }
        public string filename_safe_package_name { get; set; }
        public string filename_safe_publisher_name { get; set; }
        public string id { get; set; }
        public string key { get; set; }
        public string url { get; set; }
    }
}