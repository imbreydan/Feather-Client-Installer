using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace featherclient {
    internal class Program {
        public static async Task Main(String[] args) {
            Program installer = new Program();
            await installer.run();
        }

        HttpClient httpClient = new HttpClient();
        private static string minecraftDir = $"{Environment.GetEnvironmentVariable("APPDATA")}\\.minecraft";
        private static string librariesDir = $"{minecraftDir}\\libraries\\sapphire\\feather\\1.8.9";
        private static string forgeDir = $"{minecraftDir}\\versions\\1.8.9-forge1.8.9-11.15.1.2318-1.8.9";

        public async Task run() {
            if (Process.GetProcessesByName("javaw").Length > 0) {
                Console.WriteLine("Please close your game and rerun the installer.");
                Thread.Sleep(2500);
                Environment.Exit(1);
            }

            Console.Title = "Feather Client Installer - Sapphire.ac";
            Console.WriteLine("Downloading and installing the Feather Client crack.");

            await download();
            addLauncherProfile();
            Console.WriteLine("Feather Client has been installed and added to your minecraft launcher");
            Thread.Sleep(2500);
        }

        public async Task download() {
            if (!Directory.Exists(librariesDir)) {
                Directory.CreateDirectory(librariesDir);
            }
            if (Directory.Exists(forgeDir + "\\natives")) {
                Directory.Delete(forgeDir, true);
            }
            Directory.CreateDirectory(forgeDir + "\\natives");
            string[] downloads = (await httpClient.GetStringAsync("https://pastebin.com/raw/rhpdUQCC")).Split('\n');

            await downloadFile(new Uri(downloads[0]), $"{minecraftDir}\\versions\\1.8.9-forge1.8.9-11.15.1.2318-1.8.9\\1.8.9-forge1.8.9-11.15.1.2318-1.8.9.json");
            await downloadFile(new Uri(downloads[1]), $"{minecraftDir}\\libraries\\sapphire\\feather\\1.8.9\\feather-1.8.9.jar");
            await downloadFile(new Uri(downloads[2]), $"{forgeDir}\\natives.zip");

            ZipFile.ExtractToDirectory($"{forgeDir}\\natives.zip", $"{forgeDir}\\natives");
            File.Delete(forgeDir + "\\natives.zip");
        }

        public void addLauncherProfile() {
            var launcherProfile = new JObject {
                ["created"] = "1970-01-02T00:00:00.000Z",
                ["icon"] = "Furnace",
                ["lastUsed"] = DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"),
                ["lastVersionId"] = "1.8.9-forge1.8.9-11.15.1.2318-1.8.9",
                ["javaArgs"] = $"-Djava.library.path=\"{forgeDir}\\natives\" -Xmx2G -XX:+UnlockExperimentalVMOptions -XX:+UseG1GC -XX:G1NewSizePercent=20 -XX:G1ReservePercent=20 -XX:MaxGCPauseMillis=50 -XX:G1HeapRegionSize=32M",
                ["name"] = "Feather",
                ["type"] = ""
            };

            StreamReader r = new StreamReader(minecraftDir + "\\launcher_profiles.json");
            string json = r.ReadToEnd();
            JObject jobj = JObject.Parse(json);

            foreach (var item in jobj.Properties()) {
                if (item.Name == "profiles") {
                    JToken profiles = item.Value;
                    profiles["feather"] = launcherProfile;
                }
            }

            r.Close();
            File.WriteAllText(minecraftDir + "\\launcher_profiles.json", jobj.ToString());
        }

        public async Task downloadFile(Uri uri, string outputPath) {
            string fileName = outputPath.Substring(outputPath.LastIndexOf("\\") + 1);
            Console.Write($"Downloading {fileName}... ");
            byte[] fileBytes = await httpClient.GetByteArrayAsync(uri);
            File.WriteAllBytes(outputPath, fileBytes);
            Console.Write($"Complete!\n");
        }

    }
}