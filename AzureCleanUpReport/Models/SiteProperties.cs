using System;

namespace AzureCleanUpReport.Models
{
    public class SiteProperties
    {
        public string[] HostNames { get; set; }

        public DateTime? LastModifiedTimeUtc { get; set; }
    }
}
