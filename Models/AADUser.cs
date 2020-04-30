using System;
using CsvHelper;
using CsvHelper.Configuration.Attributes;

namespace AADAppSample
{
    public class AADUser
    {
        [Index(0)]
        [Name("GroupName")]
        public string groupName { get; set; }
        [Index(1)]
        [Name("UserName")]
        public string userPrincipalName { get; set; }
    }
}