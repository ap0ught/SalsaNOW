using System;
using System.IO;

namespace SalsaNOW
{
    /// <summary>
    /// Resolves the root URL for bundled JSON/config hosted alongside the app (no host baked into source).
    /// Set <c>SALSANOW_MANIFEST_BASE</c> or place <c>SalsaNOW.manifest.ini</c> next to the executable with
    /// <c>ManifestBaseUrl=https://your-host</c> (no trailing slash required).
    /// </summary>
    internal static class RemoteManifest
    {
        private const string EnvManifestBase = "SALSANOW_MANIFEST_BASE";
        private const string IniFileName = "SalsaNOW.manifest.ini";
        private static string _baseUrl;

        internal static void ResetForTests() => _baseUrl = null;

        public static void Initialize()
        {
            _baseUrl = Environment.GetEnvironmentVariable(EnvManifestBase)?.Trim().TrimEnd('/');
            if (string.IsNullOrEmpty(_baseUrl))
                _baseUrl = ReadManifestBaseFromIni();

            if (string.IsNullOrEmpty(_baseUrl))
            {
                throw new InvalidOperationException(
                    "Remote manifest host is not configured. Set environment variable " + EnvManifestBase +
                    " to your base URL (example: https://cdn.example.com), or create \"" + IniFileName +
                    "\" in the application folder with a line: ManifestBaseUrl=https://cdn.example.com");
            }
        }

        public static string Url(string relativePath)
        {
            if (string.IsNullOrEmpty(_baseUrl))
                throw new InvalidOperationException("RemoteManifest.Initialize was not called.");
            if (string.IsNullOrEmpty(relativePath))
                throw new ArgumentException("Relative path is required.", nameof(relativePath));

            relativePath = relativePath.TrimStart('/');
            return $"{_baseUrl}/{relativePath}";
        }

        private static string ReadManifestBaseFromIni()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, IniFileName);
            if (!File.Exists(path))
                return null;

            foreach (var raw in File.ReadAllLines(path))
            {
                var line = raw.Trim();
                if (line.Length == 0 || line[0] == '#' || line[0] == ';')
                    continue;

                int eq = line.IndexOf('=');
                if (eq <= 0)
                    continue;

                string key = line.Substring(0, eq).Trim();
                if (!key.Equals("ManifestBaseUrl", StringComparison.OrdinalIgnoreCase))
                    continue;

                return line.Substring(eq + 1).Trim().TrimEnd('/');
            }

            return null;
        }
    }
}
