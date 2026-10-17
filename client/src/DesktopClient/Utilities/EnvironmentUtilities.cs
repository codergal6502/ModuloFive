using System;
using System.IO;
using static System.Environment;

namespace ModuloFivePadClient.DesktopClient.Utilities
{
    public static class EnvironmentUtilities
    {
        private const string DIRECTORY_NAME = "ModuloFivePad";
        public static string LogFilePath { get => Path.Combine(EnsureConfigurationDirectory(), "modulo_five_pad_log.txt"); }
        public static string EnsureConfigurationDirectory()
        {
            // See https://developers.redhat.com/blog/2018/11/07/dotnet-special-folder-api-linux#.
            // Use DoNotVerify in case LocalApplicationData doesn’t exist.
            string appData = Path.Combine(Environment.GetFolderPath(SpecialFolder.LocalApplicationData, SpecialFolderOption.DoNotVerify), DIRECTORY_NAME);

            // Ensure the directory and all its parents exist.
            Directory.CreateDirectory(appData);

            return appData;
        }
    }
}
