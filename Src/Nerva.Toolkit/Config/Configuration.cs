using System.IO;
using AngryWasp.Helpers;
using AngryWasp.Logger;
using AngryWasp.Serializer;
using Nerva.Toolkit.Helpers;

namespace Nerva.Toolkit.Config
{	
	public class Configuration
	{
        #region Configuration properties

        public string ToolsPath { get; set; } = "./";

        public bool CheckForUpdateOnStartup { get; set; } = true;

        public bool LogRpcTraffic { get; set; } = false;

        public string WalletAddress { get; set; } = Constants.DEV_WALLET_ADDRESS;

        [SerializerExclude]
        public bool NewDaemonOnStartup { get; set; } = true;
        
        public Daemon Daemon { get; set; } = new Daemon();

        public Wallet Wallet { get; set; } = new Wallet();

        #endregion

        #region Integration code

        private static string loadedConfigFile;
        private static Configuration instance;

        public static string LoadedConfigFile
        {
            get { return loadedConfigFile; }
        }

        public static Configuration Instance
        {
            get { return instance; }
        }

        public static void Load(string file)
        {
            loadedConfigFile = file;

            if (!File.Exists(loadedConfigFile))
            {
                instance = new Configuration();
            
                //TODO: Set default values for a new config file
                
                Log.Instance.Write("Configuration created at '{0}'", loadedConfigFile);
                new ObjectSerializer().Serialize(instance, loadedConfigFile);
            }
            else
            {
                Log.Instance.Write("Configuration loaded from '{0}'", loadedConfigFile);
                var os = new ObjectSerializer();
                instance = os.Deserialize<Configuration>(XHelper.LoadDocument(loadedConfigFile));
            }
        }

        public static void Save()
        {
            new ObjectSerializer().Serialize(instance, loadedConfigFile);
            Log.Instance.Write("Configuration saved to '{0}'", loadedConfigFile);
        }

        #endregion
    }
}