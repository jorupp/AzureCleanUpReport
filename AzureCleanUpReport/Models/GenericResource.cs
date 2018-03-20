using System.Collections.Generic;

namespace AzureCleanUpReport.Models
{
    public class GenericResource
    {
        public string ResourceGroupName { get; set; }

        public string Id { get; set; }

        public GenericResourceIdentity Identity { get; set; }

        public string Kind { get; set; }

        public string Location { get; set; }

        public string ManagedBy { get; set; }

        public string Name { get; set; }

        public GenericResourcePlan Plan { get; set; }

        public object Properties { get; set; }

        public GenericResourceSku Sku { get; set; }

        public IDictionary<string,string> Tags { get; set; }

        public string Type { get; set; }
    }
}
