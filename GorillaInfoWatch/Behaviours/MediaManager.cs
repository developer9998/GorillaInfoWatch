using BepInEx;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models.Enumerations;
using GorillaInfoWatch.Tools;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace GorillaInfoWatch.Behaviours
{
    public class MediaManager : MonoBehaviour
    {
        public static MediaManager Instance { get; private set; }

        public Dictionary<string, Session> Sessions { get; private set; } = [];
        public string FocussedSession { get; private set; } = null;

        public bool IsCompatible => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || SystemInfo.operatingSystem.ToLower().StartsWith("windows");
        public string ExecutablePath => Path.Combine(Application.streamingAssetsPath, "GorillaInfoWatch", "GorillaInfoMediaProcess.exe");

        public readonly string ExecutableURL = @"https://github.com/developer9998/WindowsMediaController/raw/master/GorillaInfoMediaProcess.exe";

        public ProcessStartInfo consoleStartInfo;

        public Process consoleProcess;

        public event Action<Session> OnSessionFocussed, OnPlaybackStateChanged, OnMediaChanged, OnTimelineChanged;

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

            Application.wantsToQuit += () =>
            {
                if (consoleProcess != null && !consoleProcess.HasExited)
                {
                    try
                    {
                        consoleProcess.StandardInput.WriteLine("quit");
                        consoleProcess.StandardInput.Flush();

                        if (!consoleProcess.WaitForExit(2000))
                        {
                            consoleProcess.Kill();
                            consoleProcess.WaitForExit();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logging.Fatal("Could not kill consoleProcess");
                        Logging.Error(ex);
                    }
                    finally
                    {
                        consoleProcess.Dispose();
                        consoleProcess = null;
                    }
                }

                return true;
            };
        }

        private async void HandleModInitialized()
        {
            await CreateExecutable();

            if (!File.Exists(ExecutablePath))
            {
                Logging.Warning("Executable does not exist");
                Logging.Info(ExecutablePath);
                return;
            }

            consoleStartInfo = new ProcessStartInfo
            {
                FileName = ExecutablePath,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            consoleProcess = new()
            {
                StartInfo = consoleStartInfo,
                EnableRaisingEvents = true
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
            Logging.Info(data);

            JObject obj = JObject.Parse(data);

            string eventName = (string)obj.Property("EventName")?.Value ?? null;
            string sessionId = (string)obj.Property("SessionId")?.Value ?? null;

            ThreadingHelper.Instance.StartSyncInvoke(async () =>
            {
                Session session;

                if (!string.IsNullOrEmpty(eventName))
                {
                    switch (eventName)
                    {
                        case "AddSession":
                            if (string.IsNullOrEmpty(sessionId)) return;

                            if (!Sessions.ContainsKey(sessionId))
                            {
                                session = new Session()
                                {
                                    Id = sessionId
                                };
                                Sessions.Add(sessionId, session);

                                Logging.Message($"Added Session: \"{sessionId}\"");

                                if (string.IsNullOrEmpty(FocussedSession))
                                {
                                    FocussedSession = sessionId;
                                    OnSessionFocussed?.SafeInvoke(session);
                                }
                            }
                            break;

                        case "RemoveSession":
                            if (string.IsNullOrEmpty(sessionId)) return;

                            if (Sessions.ContainsKey(sessionId))
                            {
                                Sessions.Remove(sessionId);

                                Logging.Message($"Removed Session: \"{sessionId}\"");

                                if (Sessions.Count == 0 && FocussedSession != null)
                                {
                                    FocussedSession = null;
                                    OnSessionFocussed?.SafeInvoke(null);
                                }
                            }
                            break;

                        case "SessionFocusChanged":
                            FocussedSession = sessionId;
                            if (sessionId != null) await new WaitUntil(() => Sessions.ContainsKey(sessionId)).AsAwaitable();

                            Logging.Message($"Session Focus Changed: \"{sessionId}\"");

                            OnSessionFocussed?.SafeInvoke((!string.IsNullOrEmpty(sessionId) && Sessions.TryGetValue(sessionId, out session)) ? session : null);
                            break;

                        case "PlaybackStateChanged":
                            if (sessionId == null) return;

                            if (!Sessions.TryGetValue(sessionId, out session))
                            {
                                await new WaitUntil(() => Sessions.ContainsKey(sessionId)).AsAwaitable();
                                session = Sessions[sessionId];
                            }

                            session.PlaybackStatus = (string)obj["PlaybackStatus"];

                            OnPlaybackStateChanged?.SafeInvoke(session);
                            break;

                        case "MediaPropertyChanged":
                            if (sessionId == null) return;

                            if (!Sessions.TryGetValue(sessionId, out session))
                            {
                                await new WaitUntil(() => Sessions.ContainsKey(sessionId)).AsAwaitable();
                                session = Sessions[sessionId];
                            }

                            session.Title = (string)obj["Title"];
                            session.Artist = (string)obj["Artist"];

                            string base64String = (string)obj["Thumbnail"];

                            Texture2D texture = null;

                            if (!string.IsNullOrEmpty(base64String) && !thumbnailCache.TryGetValue(base64String, out texture))
                            {
                                texture = new Texture2D(2, 2)
                                {
                                    filterMode = FilterMode.Point,
                                    wrapMode = TextureWrapMode.Clamp
                                };
                                texture.LoadImage(Convert.FromBase64String(base64String));
                                thumbnailCache.Add(base64String, texture);
                            }

                            session.Thumbnail = texture;

                            OnMediaChanged?.SafeInvoke(session);
                            break;

                        case "TimelinePropertyChanged":
                            if (sessionId == null) return;

                            if (!Sessions.TryGetValue(sessionId, out session))
                            {
                                await new WaitUntil(() => Sessions.ContainsKey(sessionId)).AsAwaitable();
                                session = Sessions[sessionId];
                            }

                            session.Position = (double)obj["Position"];
                            session.StartTime = (double)obj["StartTime"];
                            session.EndTime = (double)obj["EndTime"];

                            OnTimelineChanged?.SafeInvoke(session);
                            break;
                    }
                }
            });
        }

        public async Task CreateExecutable()
        {
            if (!File.Exists(ExecutablePath))
            {
                using UnityWebRequest request = UnityWebRequest.Get(ExecutablePath);

                UnityWebRequestAsyncOperation asyncOperation = request.SendWebRequest();
                await asyncOperation;

                if (request.result == UnityWebRequest.Result.Success)
                {
                    await File.WriteAllTextAsync(ExecutablePath, request.downloadHandler.text);
                }
            }

            await Task.Delay(5000);
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
