namespace AzureCleanUpReport.Models
{
    public class VirtualMachine
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public VirtualMachineStatus[] Statuses { get; set; }
    }
}
