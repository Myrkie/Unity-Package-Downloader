namespace Unity_package_downloader.Json;

public class ProductJSON
{
    public class RootObject
    {
        public string createdBy { get; set; }
        public string updatedBy { get; set; }
        public string createdTime { get; set; }
        public string updatedTime { get; set; }
        public string id { get; set; }
        public string packageId { get; set; }
        public string slug { get; set; }
        public int revision { get; set; }
        public string ownerId { get; set; }
        public string ownerType { get; set; }
        public Properties properties { get; set; }
        public string name { get; set; }
        public string displayName { get; set; }
        public string description { get; set; }
        public string[] supportedUnityVersions { get; set; }
        public string[] keyWords { get; set; }
        public Version version { get; set; }
        public ProductReview productReview { get; set; }
        public string originPrice { get; set; }
        public ProductRatings[] productRatings { get; set; }
        public ProductPublisher productPublisher { get; set; }
        public Category category { get; set; }
        public MainImage mainImage { get; set; }
        public MainImageWebp mainImageWebp { get; set; }
        public Images[] images { get; set; }
        public string publisherId { get; set; }
        public string publishNotes { get; set; }
        public string[] requirements { get; set; }
        public string state { get; set; }
        public Localizations localizations { get; set; }
        public Uploads uploads { get; set; }
        public object[] supportLinks { get; set; }
    }

    public class Properties
    {
        public string genesisItemId { get; set; }
        public string firstPublishedDate { get; set; }
        public string launchPromotion { get; set; }
    }

    public class Version
    {
        public string id { get; set; }
        public string name { get; set; }
        public string publishedDate { get; set; }
    }

    public class ProductReview
    {
        public string reviewCount { get; set; }
        public string ratingAverage { get; set; }
        public string ratingCount { get; set; }
        public string hotness { get; set; }
    }

    public class ProductRatings
    {
        public string currency { get; set; }
        public string originalPrice { get; set; }
        public string finalPrice { get; set; }
        public string entitlementType { get; set; }
    }

    public class ProductPublisher
    {
        public string id { get; set; }
        public string name { get; set; }
        public string externalRef { get; set; }
        public string supportUrl { get; set; }
        public string supportEmail { get; set; }
        public string url { get; set; }
        public string gaAccount { get; set; }
        public string gaPrefix { get; set; }
    }

    public class Category
    {
        public string id { get; set; }
        public string name { get; set; }
        public string slug { get; set; }
    }

    public class MainImage
    {
        public string big { get; set; }
        public string icon { get; set; }
        public string icon25 { get; set; }
        public string icon75 { get; set; }
        public string small { get; set; }
        public string url { get; set; }
        public string facebook { get; set; }
        public string small_v2 { get; set; }
        public string big_v2 { get; set; }
    }

    public class MainImageWebp
    {
        public string icon { get; set; }
        public string url { get; set; }
        public string facebook { get; set; }
        public string small_v2 { get; set; }
        public string big_v2 { get; set; }
    }

    public class Images
    {
        public int height { get; set; }
        public int width { get; set; }
        public string imageUrl { get; set; }
        public string webpUrl { get; set; }
        public string thumbnailUrl { get; set; }
        public string type { get; set; }
    }

    public class Localizations
    {
        public Ko_KR ko_KR { get; set; }
        public Zh_CN zh_CN { get; set; }
        public Ja_JP ja_JP { get; set; }
    }

    public class Ko_KR
    {
        public string name { get; set; }
        public string description { get; set; }
        public string publishNotes { get; set; }
    }

    public class Zh_CN
    {
        public string name { get; set; }
        public string description { get; set; }
        public string publishNotes { get; set; }
    }

    public class Ja_JP
    {
        public string name { get; set; }
        public string description { get; set; }
        public string publishNotes { get; set; }
    }

    public class Uploads
    {
        public _018_4_0f1 _018_4_0f1 { get; set; }
    }

    public class _018_4_0f1
    {
        public string assetCount { get; set; }
        public string downloadSize { get; set; }
        public string versionNumber { get; set; }
        public string[] srps { get; set; }
        public object[] dependencies { get; set; }
    }
}