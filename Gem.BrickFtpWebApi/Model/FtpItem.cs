namespace Gem.BrickFtpWebApi.Model
{
    // Reverse engineered from JSON HTTP-responds from service using http://json2csharp.com/
    public class FtpItem
    {
        public int id { get; set; }
        public string path { get; set; }
        public string type { get; set; }
        public int? size { get; set; }
        public string mtime { get; set; }
        public string provided_mtime { get; set; }
        public object crc32 { get; set; }
        public object md5 { get; set; }
        public string permissions { get; set; }
    }
}