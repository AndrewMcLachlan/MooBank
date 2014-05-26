using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asm.BankPlus.Security
{
    public static class SecureConfiguration
    {
        public static void EncryptConfig(string sectionName)
        {
            // Open the configuration file and retrieve  
            // the connectionStrings section.
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            ConfigurationSection section = config.GetSection(sectionName) as ConfigurationSection;

            if (!section.SectionInformation.IsProtected)
            {
                // Encrypt the section.
                section.SectionInformation.ProtectSection("DataProtectionConfigurationProvider");
            }
            // Save the current configuration.
            config.Save();
        }
    }
}
