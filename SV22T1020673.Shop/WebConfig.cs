namespace SV22T1020673.Shop
{
    /// <summary>
    /// Các tham số cấu hình chung cho ứng dụng Shop
    /// </summary>
    public static class WebConfig
    {
        /// <summary>
        /// Đường dẫn cơ sở đến máy chủ ảnh (Admin)
        /// </summary>
        public static string ImageServerUrl { get; set; } = "http://localhost:5176";
    }
}
