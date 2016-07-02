using log4net;
using log4net.Config;
using NetworkManager.DomainContent;
using NetworkManager.Scheduling;
using SQLite;
using System;
using System.IO;
using System.Reflection;
using System.Windows;

namespace NetworkManager {
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application {
        public static readonly Properties config = new Properties(@"config\parameters.properties");

        public static readonly ComputerInfoStore computerInfoStore;
        public static readonly JobStore jobStore;

        public static readonly ILog logger = LogManager.GetLogger(typeof(App));

        /// <summary>
        /// Static constructor. This constructor will any field accessible from all over the 
        /// application (i.e. logger, database access, etc...)
        /// </summary>
        static App() {
            // Prepare the database
            var conn = new SQLiteConnection(config.get("database", "NetworkManager.sqlite"));

            computerInfoStore = new ComputerInfoStore(conn);
            jobStore = new JobStore(conn);

            // Prepare the logger
            configureLogger();
        }

        /// <summary>
        /// Prepare the logger
        /// </summary>
        static private void configureLogger() {
            var configFile = Directory.GetCurrentDirectory() + @"\config\log4net-app.config";

            if (File.Exists(configFile)) {
                XmlConfigurator.Configure(new FileInfo(configFile));
            } else {
                BasicConfigurator.Configure();
                logger.Warn("No log4net configuration file found");
            }
        }
    }
}
