using RS.Commons.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Commons.Helper
{
    public class ConfigHelpler
    {
        public static T GetDefaultConfig<T>(string configKey, T defaultConfig)
        {
            string appPath = FileHelper.WinMapPath($"/Configs/");
            return GetDefaultConfig(appPath, configKey, defaultConfig);
        }

        public static void SaveAppConfigAsync<T>(string saveConfigKey, T saveConfig)
        {
            string appPath = FileHelper.WinMapPath($"/Configs/");
            SaveConfigAsyn(appPath, saveConfigKey, saveConfig);
        }

        public static T GetDefaultConfig<T>(string appPath, string configKey, T defaultConfig)
        {
            string filePath = string.Format("{0}{1}.json", appPath, configKey);
            if (!FileHelper.IsExistFile(filePath))
            {
                return defaultConfig;
            }
            var jsonString = File.ReadAllText(filePath);
            var configResult = jsonString.ToObject<T>();
            if (configResult != null)
            {
                defaultConfig = configResult;
            }
            return defaultConfig;
        }


        private static void SaveConfigAsyn<T>(string appPath, string saveConfigKey, T saveConfig)
        {
            Task.Factory.StartNew(() =>
            {
                SaveConfig(appPath, saveConfigKey, saveConfig);
            });
        }

        private static void SaveConfig<T>(string appPath, string saveConfigKey, T saveConfig)
        {
            FileHelper.CreateDirectory(appPath);
            string jsonString = saveConfig.ToJson();
            string filePath = string.Format("{0}{1}.json", appPath, saveConfigKey);
            FileHelper.SaveStringToFile(jsonString, filePath, FileMode.Create);
        }
    }
}
