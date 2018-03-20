using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AzureCleanUpReport.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace AzureCleanUpReport.Services
{
    public class ReportService
    {
        private string _authContextUrl;
        private ClientCredential _clientCredential;

        public ReportService(string authContextUrl, ClientCredential clientCredential)
        {
            _authContextUrl = authContextUrl;
            _clientCredential = clientCredential;
        }

        public async Task CreateReportAsync(string subscriptionId)
        {
            await this.GetResourceGroupReportDetailsAsync(subscriptionId);
        }

        private async Task GetResourceGroupReportDetailsAsync(string subscriptionId)
        {
            // Get all resource groups
            ICollection<ResourceGroup> resourceGroupListResult = await this.GetResourceGroupsAsync(subscriptionId);

            // Iterate over each and get the resources
            ICollection<GenericResource> resources = new Collection<GenericResource>();
            foreach (var resourceGroup in resourceGroupListResult)
            {
                if (!string.IsNullOrWhiteSpace(resourceGroup?.Name))
                {
                    var currentResources = await this.GetResourcesAsync(subscriptionId, resourceGroup.Name);
                    foreach (var resource in currentResources)
                    {
                        resources.Add(resource);
                    }
                }
            }

            this.CreateResourceReport(resources);
        }

        private async Task<ICollection<ResourceGroup>> GetResourceGroupsAsync(string subscriptionId)
        {
            string uri = $"https://management.azure.com/subscriptions/{subscriptionId}/resourceGroups?$expand=instanceView&api-version=2017-05-10";

            ICollection<ResourceGroup> totalResourceGroups = new Collection<ResourceGroup>();

            string nextLink = uri;
            do
            {
                ResourceGroupListResult resourceGroupListResult = await this.GetWebRequest<ResourceGroupListResult>(nextLink);

                if (resourceGroupListResult != null)
                {
                    foreach (var resource in resourceGroupListResult.Value ?? new ResourceGroup[0])
                    {
                        totalResourceGroups.Add(resource);
                    }

                    nextLink = resourceGroupListResult.NextLink;
                }
            } while (!string.IsNullOrWhiteSpace(nextLink));

            return totalResourceGroups;
        }

        private async Task<ICollection<GenericResource>> GetResourcesAsync(string subscriptionId, string resourceGroupName)
        {
            string uri = $"https://management.azure.com/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/resources?api-version=2017-05-10";

            ICollection<GenericResource> totalResources = new Collection<GenericResource>();

            string nextLink = uri;
            do
            {
                ResourceListResult resourceList = await this.GetWebRequest<ResourceListResult>(nextLink);

                if (resourceList != null)
                {
                    foreach (var resource in resourceList.Value ?? new GenericResource[0])
                    {
                        resource.ResourceGroupName = resourceGroupName;
                        totalResources.Add(resource);
                    }

                    nextLink = resourceList.NextLink;
                }
            } while (!string.IsNullOrWhiteSpace(nextLink));
            
            return totalResources;
        }

        private void CreateResourceReport(ICollection<GenericResource> resources)
        {
            var resultFile = Path.Combine(Path.GetTempPath(), $@"Resources_State_{DateTime.Now:yyyyMMdd}.csv");
            Console.WriteLine(resultFile);

            using (StreamWriter file = File.CreateText(resultFile))
            {
                file.WriteLine("Resource Group Name," +
                               "Resource ID," +
                               "Resource Principal ID," +
                               "Tenant ID," +
                               "Kind," +
                               "Location," +
                               "Managed By," +
                               "Resource Name," +
                               "Plan Name," +
                               "Plan Product," +
                               "Plan Promotion Code," +
                               "Plan Publisher," +
                               "Properties," +
                               "Sku Capacity," +
                               "Sku Family," +
                               "Sku Model," +
                               "Sku Name," +
                               "Sku Size," +
                               "Sku Tier," +
                               "Tags," +
                               "Type");
            }

            string quote(string input)
            {
                return "\"" + input.Replace("\"", "\"\"") + "\"";
            }

            if (resources?.Count > 0)
            {
                using (StreamWriter file = File.AppendText(resultFile))
                {
                    foreach (var resource in resources)
                    {
                        ICollection<string> tags = resource.Tags?.Select(x => $"{x.Key},{x.Value}").ToList() ?? new List<string>();
                        file.WriteLine($"{quote(resource.ResourceGroupName ?? string.Empty)}," +
                                       $"{quote(resource.Id ?? string.Empty)}," +
                                       $"{quote(resource.Identity?.PrincipalId ?? string.Empty)}," +
                                       $"{quote(resource.Identity?.TenantId ?? string.Empty)}," +
                                       $"{quote(resource.Kind ?? string.Empty)}," +
                                       $"{quote(resource.Location ?? string.Empty)}," +
                                       $"{quote(resource.ManagedBy ?? string.Empty)}," +
                                       $"{quote(resource.Name ?? string.Empty)}," +
                                       $"{quote(resource.Plan?.Name ?? string.Empty)}," +
                                       $"{quote(resource.Plan?.Product ?? string.Empty)}," +
                                       $"{quote(resource.Plan?.PromotionCode ?? string.Empty)}," +
                                       $"{quote(resource.Plan?.Publisher ?? string.Empty)}," +
                                       $"{quote(resource.Properties?.ToString() ?? string.Empty)}," +
                                       $"{quote(resource.Sku?.Capacity.ToString() ?? string.Empty)}," +
                                       $"{quote(resource.Sku?.Family ?? string.Empty)}," +
                                       $"{quote(resource.Sku?.Model ?? string.Empty)}," +
                                       $"{quote(resource.Sku?.Name ?? string.Empty)}," +
                                       $"{quote(resource.Sku?.Size ?? string.Empty)}," +
                                       $"{quote(resource.Sku?.Tier ?? string.Empty)}," +
                                       $"{quote(string.Join("/", tags))}," +
                                       $"{quote(resource.Type ?? string.Empty)}"
                                       );
                    }
                }
            }
        }

        private async Task<TEntity> GetWebRequest<TEntity>(string uri)
        {
            TEntity result = default(TEntity);

            // Get token
            string token = await this.GetAccessTokenAsync();

            // Create the request
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + token);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "GET";

            // Get the response
            HttpWebResponse httpResponse = null;
            try
            {
                httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return default(TEntity);
            }

            string rawResult = null;
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                rawResult = streamReader.ReadToEnd();
            }

            result = Newtonsoft.Json.JsonConvert.DeserializeObject<TEntity>(rawResult);

            return result;
        }

        private async Task<string> GetAccessTokenAsync()
        {
            var authenticationContext = new AuthenticationContext(_authContextUrl);
            var result = await authenticationContext.AcquireTokenAsync(resource: "https://management.azure.com/", clientCredential: _clientCredential);

            if (result == null)
            {
                throw new InvalidOperationException("Failed to obtain the JWT token");
            }

            string token = result.AccessToken;
            return token;
        }
    }
}
