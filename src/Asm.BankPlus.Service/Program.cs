using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Configuration.Install;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Asm.BankPlus.Security;

namespace Asm.BankPlus.Service
{
    static class Program
    {
        #region Externs
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AllocConsole();
        #endregion

        #region Constants
        private const string ConsoleDisplayName = "BankPlus Service";
        #endregion

        #region Fields
        private static string[] _args;
        private static ILog _log;
        #endregion

        #region Private Methods
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            _args = args;

            if (args != null && (args.Contains("/?") || args.Contains("-?") || args.Contains("-help") || args.Contains("/help")))
            {
                PrintUsage();
            }
            /*else if (args != null && (args.Contains("-interactive") || args.Contains("/interactive")))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new InteractiveForm());
            }*/
            else if (args != null && (args.Contains("-i") || args.Contains("/i")))
            {
                Install(args.Contains("-f") || args.Contains("/f"));
            }
            else if (args != null && (args.Contains("-u") || args.Contains("/u")))
            {
                Uninstall();
            }
            else
            {
                XmlConfigurator.Configure();
                _log = LogManager.GetLogger(typeof(Program));

#if !DEBUG
                try
                {
                    SecureConfiguration.EncryptConfig("appSettings");
                    SecureConfiguration.EncryptConfig("connectionStrings");
                }
                catch (Exception ex)
                {
                    _log.Error("Error encrypting configuration sections", ex);
                    Console.WriteLine(ex.Message);
                }
#endif

                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] 
                { 
                    new BankPlusService() 
                };
                ServiceBase.Run(ServicesToRun);
            }
        }

        private static void PrintUsage()
        {
            //AllocConsole();
            Console.WriteLine("Usage: " + Path.GetFileNameWithoutExtension(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) + " [OPTIONS]");
            Console.WriteLine("Provides scheduling for BankPlus");
            Console.WriteLine();
            //Console.WriteLine("  -interactive                             run as a wnidows application");
            Console.WriteLine("  -i                                       install the service");
            Console.WriteLine("  -f                                       used in conjunction with -i. Forces the install the service by uninstalling it first");
            Console.WriteLine("  -starttype:[automatic|manual|disabled]   set the start type of the service while installing");
            Console.WriteLine("  -servicename:[name]                      sets the service name to something other than the default");
            Console.WriteLine("  -displayname:[name]                      sets the display name to something other than the default. If not included, servicename will be used.");
            Console.WriteLine("  -u                                       uninstall the service");
            Console.WriteLine("  -?, -help                                display this help and exit");
        }
        #endregion

        #region Install Methods
        private static void Install(bool force)
        {
            if (force) Uninstall();

            AssemblyInstaller installer = new AssemblyInstaller(Assembly.GetExecutingAssembly(), new string[] { });
            installer.BeforeInstall += Installer_BeforeInstall;
            installer.UseNewContext = true;
            Dictionary<object, object> state = new Dictionary<object, object>();

            try
            {
                installer.Install(state);
                //installer.Commit(state);
                Console.WriteLine(String.Format("{0} installed.", ConsoleDisplayName));
            }
            catch (Win32Exception wex)
            {
                //installer.Rollback(state);
                if (wex.NativeErrorCode == 1073) // Service already installed
                {
                    Console.WriteLine(String.Format("{0} already installed.", ConsoleDisplayName));
                }
                else
                {
                    throw;
                }
            }
            catch (Exception)
            {
                //installer.Rollback(state);
                throw;
            }
        }

        private static void Installer_BeforeInstall(object sender, InstallEventArgs e)
        {
            string startType = _args.Where(a => a.StartsWith("-starttype:", StringComparison.OrdinalIgnoreCase)).Select(a => a.Substring("-starttype:".Length)).FirstOrDefault();
            string serviceName = _args.Where(a => a.StartsWith("-servicename:", StringComparison.OrdinalIgnoreCase)).Select(a => a.Substring("-servicename:".Length)).FirstOrDefault();
            string displayName = _args.Where(a => a.StartsWith("-displayname:", StringComparison.OrdinalIgnoreCase)).Select(a => a.Substring("-displayname:".Length)).FirstOrDefault();

            ServiceStartMode? serviceStartMode = null;
            ServiceStartMode tempMode;
            if (Enum.TryParse<ServiceStartMode>(startType, true, out tempMode)) serviceStartMode = tempMode;

            AssemblyInstaller installer = sender as AssemblyInstaller;
            ProjectInstaller projectInstaller = null;

            foreach (Installer i in installer.Installers)
            {
                projectInstaller = i as ProjectInstaller;
                if (projectInstaller != null) break;
            }

            if (projectInstaller != null)
            {
                if (serviceStartMode != null)
                {
                    projectInstaller.ServiceInstaller.StartType = serviceStartMode.Value;
                }

                if (!String.IsNullOrWhiteSpace(serviceName))
                {
                    projectInstaller.ServiceInstaller.ServiceName = serviceName;
                    projectInstaller.ServiceInstaller.DisplayName = serviceName;
                }
                if (!String.IsNullOrWhiteSpace(displayName))
                {
                    projectInstaller.ServiceInstaller.DisplayName = displayName;
                }
            }
        }

        private static void Uninstall()
        {
            AssemblyInstaller installer = new AssemblyInstaller(Assembly.GetExecutingAssembly(), new string[] { });
            installer.BeforeUninstall += Installer_BeforeUninstall;
            installer.UseNewContext = true;

            Dictionary<object, object> state = new Dictionary<object, object>();

            try
            {
                installer.Uninstall(state);
                //installer.Commit(state);
                Console.WriteLine(String.Format("{0} uninstalled.", ConsoleDisplayName));
            }
            catch (System.Configuration.Install.InstallException iex)
            {
                Win32Exception wex = iex.InnerException as Win32Exception;
                if (wex != null && wex.NativeErrorCode == 1060) // Service not installed
                {
                    Console.WriteLine(String.Format("{0} not found.", ConsoleDisplayName));
                }
                else
                {
                    //installer.Rollback(state);
                    throw;
                }
            }
            catch (Win32Exception wex)
            {
                if (wex != null && wex.NativeErrorCode == 1060) // Service not installed
                {
                    Console.WriteLine(String.Format("{0} not found.", ConsoleDisplayName));
                }
                else
                {
                    //installer.Rollback(state);
                    throw;
                }
            }
            catch (Exception)
            {
                //installer.Rollback(state);
                throw;
            }
        }

        private static void Installer_BeforeUninstall(object sender, InstallEventArgs e)
        {
            // Make sure we are uninstalling the correct service name.
            string serviceName = _args.Where(a => a.StartsWith("-servicename:", StringComparison.OrdinalIgnoreCase)).Select(a => a.Substring("-servicename:".Length)).FirstOrDefault();

            AssemblyInstaller installer = sender as AssemblyInstaller;
            ProjectInstaller projectInstaller = null;

            foreach (Installer i in installer.Installers)
            {
                projectInstaller = i as ProjectInstaller;
                if (projectInstaller != null) break;
            }

            if (projectInstaller != null)
            {
                if (!String.IsNullOrWhiteSpace(serviceName))
                {
                    projectInstaller.ServiceInstaller.ServiceName = serviceName;
                }
            }
        }
        #endregion
    }
}
