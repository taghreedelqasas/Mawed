namespace Maw3ed.BLL.Helpers
{
    public static class ImageUrlHelper
    {
        private const string BaseUrl = "https://mawed.runasp.net";

        public static string? ToFullUrl(string? path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            if (path.StartsWith("http")) return path;
            return $"{BaseUrl}{path}";
        }
    }
}
