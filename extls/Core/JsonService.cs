using System.Text.Json;
using static extls.Core.Root;

namespace extls.Core
{
    public static class JsonService
    {
        private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

        static JsonService()
        {
            if (!Directory.Exists(RootPath))
                Directory.CreateDirectory(RootPath);
        }

        public static bool SaveJson<T>(string path, string name, T t) where T : class
        {
            string targetDir = Path.Combine(RootPath, path);
            string fullPath = Path.Combine(targetDir, name);

            try
            {
                if (!Directory.Exists(targetDir))
                    Directory.CreateDirectory(targetDir);

                string json = JsonSerializer.Serialize(t, Options);
                File.WriteAllText(fullPath, json);
                return true;
            }
            catch (Exception ex)
            {
                Print.Error($"Failed to save JSON: {ex.Message}");
                return false;
            }
        }
        public static T LoadJson<T>(string path, string name) where T : class
        {
            string fullPath = Path.Combine(RootPath, path, name);

            if (!File.Exists(fullPath))
                return null!;

            try
            {
                string json = File.ReadAllText(fullPath);
                return JsonSerializer.Deserialize<T>(json)!;
            }
            catch (Exception ex)
            {
                Print.Error($"Failed to load JSON: {ex.Message}");
                return null!;
            }
        }
    }
}
