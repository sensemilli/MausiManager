using System;

namespace WiCAM.Pn4000.JobManager.Services
{
    public class Microsoft365Config
    {
        public string ClientId { get; set; } = "IHRE_CLIENT_ID";
        public string TenantId { get; set; } = "IHRE_TENANT_ID";
        public string ClientSecret { get; set; } = "IHR_CLIENT_SECRET";
        
        // Scopes für delegierte Berechtigungen
        public string[] Scopes { get; set; } = new[]
        {
            "Files.Read.All",
            "Files.ReadWrite.All",
            "Sites.Read.All"
        };
    }
}