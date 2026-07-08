namespace QuickNV.Utils
{
    public class DebugUtils
    {
        public const string DEBUG_FOLDER = "bin/Debug";
        public static bool IsInDebugMode()
        {
            return Directory.Exists(DEBUG_FOLDER);
        }

        public static string GetFolderUnderDebugFolder(string folder)
        {
            return Path.Combine(DEBUG_FOLDER, folder);
        }
    }
}
