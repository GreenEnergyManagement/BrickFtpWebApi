namespace Gem.BrickFtpWebApi.Model
{
    // Reverse engineered from JSON HTTP-responds from service using http://json2csharp.com/
    public class UploadItem
    {
        public string @ref { get; set; }
        public string path { get; set; }
        public string action { get; set; }
        public bool ask_about_overwrites { get; set; }
        public string http_method { get; set; }
        public string upload_uri { get; set; }
        public string expires { get; set; }
        public int partsize { get; set; }
        public int part_number { get; set; }
        public int available_parts { get; set; }
        public Send send { get; set; }
        public Headers headers { get; set; }
        public Parameters parameters { get; set; }
    }
}