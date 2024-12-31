namespace JWT.Core.Model
{
    /// <summary>
    /// Bu class appsettigs.json'a göre oluşturuldu.
    /// options pattern'e göre program.cs'den configure ettik.
    /// IOptions<CustomTokenOptionCustomTokenOption> options ile oluşturup options.Value ile getirilir.
    /// </summary>
    public class CustomTokenOption
    {
        public List<string> Audience { get; set; }
        public string Issuer { get; set; }
        public int AccessTokenExpiration { get; set; }
        public int RefreshTokenExpiration { get; set; }
        public string SecurityKey { get; set; }
    }
}
