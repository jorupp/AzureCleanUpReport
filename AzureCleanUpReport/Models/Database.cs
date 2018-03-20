namespace AzureCleanUpReport.Models
{
    public class Database
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Location { get; set; }

        public DatabaseProperties  Properties { get; set; }
    }
}
