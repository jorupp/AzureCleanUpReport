using AzureCleanUpReport.Services;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace AzureCleanUpReport
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var subscriptionId = args[0];
            var authContextUrl = args[1];
            var clientCredential = new ClientCredential(clientId: args[2], clientSecret: args[3]);

            ReportService reportService = new ReportService(authContextUrl, clientCredential);
            reportService.CreateReportAsync(subscriptionId).Wait();
        }
    }
}
