namespace AzureCleanUpReport.Models
{
    public class ResourceGroupListResult
    {
        public ResourceGroup[] Value { get; set; }

        public string NextLink { get; set; }
    }
}
