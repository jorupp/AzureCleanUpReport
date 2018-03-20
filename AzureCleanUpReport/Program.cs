using AzureCleanUpReport.Services;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;

namespace AzureCleanUpReport
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length < 4)
            {
                Console.WriteLine("Args: <Azure subscriptionId> <auth url> <clientId> <clientSecret>");
                return;
            }
            var subscriptionId = args[0];
            var authContextUrl = args[1];
            var clientCredential = new ClientCredential(clientId: args[2], clientSecret: args[3]);

            ReportService reportService = new ReportService(authContextUrl, clientCredential);
            reportService.CreateReportAsync(subscriptionId).Wait();
        }
    }
}
