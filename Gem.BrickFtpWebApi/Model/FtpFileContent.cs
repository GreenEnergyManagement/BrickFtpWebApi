namespace Gem.BrickFtpWebApi.Model
{
    // Reverse engineered from JSON HTTP-responds from service using http://json2csharp.com/
    public class FtpFileContent
    {
        public FtpDownloadItem FtpItem { get; private set; }
        public byte[] Content { get; private set; }

        public FtpFileContent(FtpDownloadItem ftpItem, byte[] content)
        {
            FtpItem = ftpItem;
            Content = content;
        }
    }
}