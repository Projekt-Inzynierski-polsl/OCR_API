namespace OCR_API
{
    public static class AuthenticationSettings
    {
        public static string JwtKey { get; set; }
        public static int JwtExpireDays { get; set; }
        public static string JwtIssuer { get; set; }
        public static string EncryptionKey { get; set; }
    }
}
