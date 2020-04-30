using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CsvHelper;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace AADAppSample
{
    class Program
    {
        static string ChooseString(string string01, string string02)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(string01) == false)
            {
                result = string01;
            }
            else if (string.IsNullOrEmpty(string02) == false)
            {
                result = string02;
            }
            return result;
        }
        static Dictionary<string, string> LoadClientSecretAppSettings()
        {
            Dictionary<string, string> result = null;
            var appConfig = new ConfigurationBuilder()
                    .AddUserSecrets<Program>()
                    .Build();
            string appId = ChooseString(
                Environment.GetEnvironmentVariable("appId"),
                appConfig["appId"]);
            string scopes = ChooseString(
                Environment.GetEnvironmentVariable("scopes"),
                appConfig["scopes"]);
            string tenantId = ChooseString(
                Environment.GetEnvironmentVariable("tenantId"),
                appConfig["tenantId"]);
            string clientSecret = ChooseString(
                Environment.GetEnvironmentVariable("clientSecret"),
                appConfig["clientSecret"]);
            string storageConnectionString = ChooseString(
                Environment.GetEnvironmentVariable("storageConnectionString"),
                appConfig["storageConnectionString"]);
            string storageContainerName = ChooseString(
                Environment.GetEnvironmentVariable("storageContainerName"),
                appConfig["storageContainerName"]);
            string storageBlobName = ChooseString(
                Environment.GetEnvironmentVariable("storageBlobName"),
                appConfig["storageBlobName"]);
            if (string.IsNullOrEmpty(appId) == false &&
                string.IsNullOrEmpty(scopes) == false &&
                string.IsNullOrEmpty(tenantId) == false &&
                string.IsNullOrEmpty(clientSecret) == false &&
                string.IsNullOrEmpty(storageConnectionString) == false &&
                string.IsNullOrEmpty(storageContainerName) == false &&
                string.IsNullOrEmpty(storageBlobName) == false)
            {
                result = new Dictionary<string, string>()
                {
                    {"appId", appId},
                    {"scopes", scopes},
                    {"tenantId", tenantId},
                    {"clientSecret", clientSecret},
                    {"storageConnectionString", storageConnectionString},
                    {"storageContainerName", storageContainerName},
                    {"storageBlobName", storageBlobName}
                };
            } 
            return result;
        }

        static List<AADUser> ListGroups()
        {
            var result = new List<AADUser>();
            var groups = GraphHelper.GetGroupsAsync().Result;

            foreach (var group in groups)
            {
                var directoryObjects = GraphHelper.GetGroupMembersAsync(group.Id).Result;
                foreach (Microsoft.Graph.User directoryObject in directoryObjects)
                {
                    result.Add(new AADUser() {
                        groupName = group.DisplayName,
                        userPrincipalName = directoryObject.UserPrincipalName
                    });
                }
            }
            return result;
        }

        static void Main(string[] args)
        {
            IAuthenticationProvider authProvider = null;

            // Client Secret
            var appConfig = LoadClientSecretAppSettings();
            if (appConfig == null)
            {
                Console.WriteLine("Missing or invalid appsettings.json");
                return;
            }
            var appId = appConfig["appId"];
            var scopesString = appConfig["scopes"];
            var scopes = scopesString.Split(';');
            var tenantId = appConfig["tenantId"];
            var clientSecret = appConfig["clientSecret"];
            var storageConnectionString = appConfig["storageConnectionString"];
            var storageContainerName = appConfig["storageContainerName"];
            var storageBlobName = appConfig["storageBlobName"];

            authProvider = new ClientSecretAuthProvider(appId, scopes, tenantId, clientSecret);
            GraphHelper.Initialize(authProvider);
            while (true)
            {
                try
                {
                    var aadusers = ListGroups();
                    Console.WriteLine($"Get groups and users: {aadusers.Count}");
                    using (var streamWriter = new StreamWriter(@"aadusers.csv"))
                    using (var csvWriter = new CsvWriter(streamWriter, System.Globalization.CultureInfo.CurrentCulture))
                    {
                        csvWriter.Configuration.HasHeaderRecord = true;
                        csvWriter.WriteRecords(aadusers);
                    }

                    BlobServiceClient blobServiceClient = new BlobServiceClient(storageConnectionString);
                    BlobContainerClient containerClient =  blobServiceClient.GetBlobContainerClient(storageContainerName);
                    containerClient.CreateIfNotExists();
                    BlobClient blobClient = containerClient.GetBlobClient(storageBlobName);
                    using (FileStream uploadFileStream = System.IO.File.OpenRead(@"aadusers.csv"))
                    {
                        blobClient.Upload(uploadFileStream, true);
                        Console.WriteLine($"Upload to Blob storage: {blobClient.Uri}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error getting aad users: {ex.Message}");
                }
                System.Threading.Tasks.Task.Delay(1000 * 60 * 30).Wait();
            }
        }
    }
}
