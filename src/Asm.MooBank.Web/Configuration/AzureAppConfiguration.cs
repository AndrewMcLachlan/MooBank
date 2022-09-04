using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Asm.MooBank.Web.Configuration
{
    public class AzureAppConfiguration
    {
        public string ApplicationId { get; set; }
        public string Audience { get; set; }
        public string ApplicationSecret { get; set; }
        public string RedirectUrl { get; set; }
        public string ADAuthority { get; set; }
        public string TenantId { get; set; }
        public string CallbackPath { get; set; }
    }
}
