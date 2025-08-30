using GorillaInfoWatch.Models.Enumerations;
using GorillaInfoWatch.Tools;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;

namespace GorillaInfoWatch.Utilities
{
    public static class MediaManagerUtility
    {
        public static bool IsCompatible => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || SystemInfo.operatingSystem.ToLower().Contains("windows");
        public static string ExecutablePath => Path.Combine(Application.persistentDataPath, "GorillaInfoMediaManager.exe");

        public static async Task CreateExecutable()
        {
            if (File.Exists(ExecutablePath)) File.Delete(ExecutablePath);

            string resourcePath = "GorillaInfoWatch.Content.GorillaInfoMediaManager.exe";

            using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);
            using FileStream fileStream = new(ExecutablePath, FileMode.Create, FileAccess.Write);
            using MemoryStream memoryStream = new();

            await stream.CopyToAsync(memoryStream);
            await fileStream.WriteAsync(memoryStream.ToArray());
        }

        public static async Task<string> GetData(MediaDataProperty data) => await RunExecutable(data.ToString().Replace(", ", " "));

        public static async Task<string> RunExecutable(string arguments)
        {
            Logging.Message(arguments);
            Logging.Message(ExecutablePath);

            if (!File.Exists(ExecutablePath)) await CreateExecutable();

            ProcessStartInfo processStartInfo = new()
            {
                FileName = ExecutablePath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            using Process process = Process.Start(processStartInfo);

            string output = (await process.StandardOutput.ReadToEndAsync()).Trim();
            return output.Trim();
        }

        public static void SynthesizeKey(MediaKeyCode keyCode) => keybd_event((uint)keyCode, 0, 0, 0);

        // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-keybd_event
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        internal static extern void keybd_event(uint bVk, uint bScan, uint dwFlags, uint dwExtraInfo);
    }
}
