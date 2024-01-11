/*
https://learn.microsoft.com/en-us/azure/azure-functions/functions-triggers-bindings?tabs=isolated-process%2Cpython-v2&pivots=programming-language-csharp#bindings-code-examples
https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-cosmosdb-v2-input?tabs=python-v2%2Cisolated-process%2Cnodejs-v4%2Cextensionv4&pivots=programming-language-csharp#example
PowerShell (just in case): Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy Unrestricted
Deploymennt: search for "Publish a project to a new function app in Azure by using advanced options" in https://learn.microsoft.com/en-us/azure/azure-functions/functions-develop-vs-code?tabs=node-v3%2Cpython-v2%2Cisolated-process&pivots=programming-language-csharp
Cosmos DB Conn String: AccountEndpoint=https://af1cosmosdb.documents.azure.com:443/;AccountKey=CiHzD2tXzTF2jJTS8DMguuJc3VaIuURoletSKxkdrB93mDWWwseCs7Xkh9nw8l7olk9pwnzoV0QgACDb0NqigA==
The conn string above will need to be manually entered into local.settings.json (ideally should be excluded from git, it is included for learning/example).
The conn string above will need to be used with `Azure functions: Add New Setting` followed by `Azure Functions: Download Remote Settings`.
Re-Deployment: use `Azure Functions: Redeploy` OR right click the azure function resource then `Deploy to Function App...`.
Function URL: https://af1func.azurewebsites.net/api/HttpTrigger1?code=0rGArQcI2GWcQWvVPQEVyzUfOwS9DP9zlLcEkQDCj3QUAzFuWe7KCw==&name=Peter
Additonal github ref: https://github.com/MicrosoftDocs/azure-docs/tree/main/articles/azure-functions
*/

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
//
using System.Collections.Generic;
using System.Linq;

namespace AF1.Customer
{
    public static class HttpTrigger1
    {
        [FunctionName("HttpTrigger1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req
            , [CosmosDB(databaseName: "Db1", containerName: "Customer", Connection = "CosmosDbConnectionString")]
              IEnumerable<dynamic> inputDocs
            , [CosmosDB(databaseName: "Db1", containerName: "Customer", Connection = "CosmosDbConnectionString")]
              IAsyncCollector<dynamic> outputDocs
            , ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            var customers = inputDocs.Where(x => x.id > 0).ToList();
            foreach (var customer in customers)
            {
                // For simplicity we will treat/use name as city :p
                customer.city = name;
                await outputDocs.AddAsync(customer);
            }

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }
    }
}
