using System.Collections.Generic;

namespace AzureCleanUpReport.Models
{
    public class ResourceListResult
    {
        public string NextLink { get; set; }

        public GenericResource[] Value { get; set; }
    }
}
