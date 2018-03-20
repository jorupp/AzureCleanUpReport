namespace AzureCleanUpReport.Models
{
    public class GenericResourceIdentity
    {
        public string PrincipalId { get; set; }

        public string TenantId { get; set; }

        public GenericResourceIdentityType Type { get; set; }
    }
}
