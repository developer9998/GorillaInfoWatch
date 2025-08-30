using BepInEx;
using GorillaInfoWatch.Models.Enumerations;
using GorillaInfoWatch.Tools;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;

namespace GorillaInfoWatch.Behaviours
{
    public class MediaManager : MonoBehaviour
    {
        public static MediaManager Instance { get; private set; }

        public static Dictionary<string, Session> Sessions { get; private set; } = [];

        public static string FocussedSession { get; private set; } = null;

        public static bool IsCompatible => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || SystemInfo.operatingSystem.ToLower().StartsWith("windows");
        public static string ExecutablePath => Path.Combine(Application.persistentDataPath, "Sample_CMD.exe");

        public const string ResourcePath = "GorillaInfoWatch.Content.Sample.CMD.exe";

        public ProcessStartInfo consoleStartInfo;

        public Process consoleProcess;

        private bool isProcessKilled;

        private readonly Dictionary<string, Texture2D> thumbnailCache = [];

        public void Awake()
        {
            if (!IsCompatible || (Instance != null && Instance != this))
            {
                Logging.Info($"MediaManager.IsCompatible : {IsCompatible}");
                Destroy(this);
                return;
            }

            Instance = this;

            Main.OnInitialized += HandleModInitialized;
            Application.quitting += HandleApplicationQuitting;
            Application.wantsToQuit += HandleApplicationQuitAttempt;
        }

        private void HandleModInitialized()
        {
            // TODO: Fix CreateExecutable method leading to error
            // As of now, CreateExecutable results in the executable being inaccessible (UnauthorizedAccessException) 

            consoleStartInfo = new ProcessStartInfo
            {
                FileName = ExecutablePath,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            consoleProcess = new()
            {
                StartInfo = consoleStartInfo
            };

            consoleProcess.OutputDataReceived += (sender, args) =>
            {
                if (string.IsNullOrEmpty(args.Data)) return;
                OnDataReceived(args.Data);
            };

            consoleProcess.Start();

            consoleProcess.BeginOutputReadLine();
        }

        public void OnDataReceived(string data)
        {
            // This method is not ran on the Unity thread, please do so when performing any Unity methods via ThreadingHelper

            JObject obj = JObject.Parse(data);

            string eventName = (string)obj.Property("EventName", StringComparison.Ordinal)?.Value ?? null;
            string sessionId = (string)obj.Property("SessionId", StringComparison.Ordinal)?.Value ?? null;

            Session session;

            if (!string.IsNullOrEmpty(eventName))
            {
                Logging.Message(eventName);

                switch (eventName)
                {
                    case "AddSession":
                        if (string.IsNullOrEmpty(sessionId)) return;

                        if (!Sessions.ContainsKey(sessionId)) Sessions.Add(sessionId, new Session()
                        {
                            Id = sessionId
                        });

                        break;
                    case "RemoveSession":
                        if (string.IsNullOrEmpty(sessionId)) return;

                        if (Sessions.ContainsKey(sessionId)) Sessions.Remove(sessionId);

                        break;
                    case "SessionFocusChanged":
                        FocussedSession = sessionId;
                        break;
                    case "PlaybackStateChanged":
                        if (Sessions.TryGetValue(sessionId, out session))
                        {
                            session.PlaybackStatus = (string)obj["PlaybackStatus"];
                        }
                        break;
                    case "MediaPropertyChanged":
                        if (Sessions.TryGetValue(sessionId, out session))
                        {
                            session.Title = (string)obj["Title"];
                            session.Artist = (string)obj["Artist"];

                            string base64String = (string)obj["Thumbnail"];

                            ThreadingHelper.Instance.StartSyncInvoke(() =>
                            {
                                Texture2D texture = null;

                                if (!string.IsNullOrEmpty(base64String))
                                {
                                    if (!thumbnailCache.TryGetValue(base64String, out texture))
                                    {
                                        texture = new Texture2D(2, 2)
                                        {
                                            filterMode = FilterMode.Point,
                                            wrapMode = TextureWrapMode.Clamp
                                        };
                                        texture.LoadImage(Convert.FromBase64String(base64String));
                                    }
                                }

                                session.Thumbnail = texture;
                            });
                        }
                        break;
                    case "TimelinePropertyChanged":
                        if (Sessions.TryGetValue(sessionId, out session))
                        {
                            session.Position = (double)obj["Position"];
                            session.StartTime = (double)obj["StartTime"];
                            session.EndTime = (double)obj["EndTime"];
                        }
                        break;
                }
            }
        }

        public void HandleApplicationQuitting()
        {
            if (!isProcessKilled)
            {
                if (consoleProcess != null)
                {
                    consoleProcess.Kill();
                    consoleProcess.WaitForExit();
                    consoleProcess.Dispose();
                    consoleProcess = null;
                }
                isProcessKilled = true;
                Application.Quit();
            }
        }

        public bool HandleApplicationQuitAttempt() => isProcessKilled;

        public async Task CreateExecutable()
        {
            if (File.Exists(ExecutablePath)) File.Delete(ExecutablePath);

            using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourcePath);
            using FileStream fileStream = new(ExecutablePath, FileMode.Create, FileAccess.Write);
            using MemoryStream memoryStream = new();

            await stream.CopyToAsync(memoryStream);
            await fileStream.WriteAsync(memoryStream.ToArray());
        }

        public void PushKey(MediaKeyCode keyCode)
        {
            ThreadingHelper.Instance.StartAsyncInvoke(() =>
            {
                keybd_event((uint)keyCode, 0, 0, 0);
                return null;
            });
        }

        // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-keybd_event
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        internal static extern void keybd_event(uint bVk, uint bScan, uint dwFlags, uint dwExtraInfo);

        public class Session
        {
            public string Id;

            public string Title;

            public string Artist;

            public string[] Genres;

            public int TrackNumber;

            public string AlbumTitle;

            public string AlbumArtist;

            public int AlbumTrackCount;

            public double StartTime;

            public double EndTime;

            public double Position;

            public string PlaybackStatus;

            public Texture2D Thumbnail;
        }
    }
}
