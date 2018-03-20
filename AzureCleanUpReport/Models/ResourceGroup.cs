namespace AzureCleanUpReport.Models
{
    public class ResourceGroup
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Location { get; set; }

        public ResourceGroupProperties Properties { get; set; }
    }
}
