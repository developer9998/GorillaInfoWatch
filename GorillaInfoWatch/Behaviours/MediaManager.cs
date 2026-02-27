using BepInEx;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models.Interfaces;
using GorillaInfoWatch.Models.MediaControl;
using GorillaInfoWatch.Tools;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;

namespace GorillaInfoWatch.Behaviours;

public class MediaManager : MonoBehaviour, IInitializeCallback
{
    public static MediaManager Instance { get; private set; }
    public Dictionary<string, Session> Sessions { get; private set; } = [];
    public string FocussedSession { get; private set; } = null;
    public string ExecutablePath => Path.Combine(Application.streamingAssetsPath, "GorillaInfoWatch", "GorillaInfoMediaProcess.exe");

    public event Action<Session> OnSessionFocussed, OnPlaybackStateChanged, OnMediaChanged, OnTimelineChanged;

    private ProcessStartInfo _startInfo;

    private Process _process;

    private readonly Dictionary<string, Texture2D> _thumbnailCache = [];

    public void Awake()
    {
        bool hasCompatibility = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || SystemInfo.operatingSystem.ToLower().StartsWith("windows");

        if (!hasCompatibility)
        {
            Logging.Warning("MediaManager is incompatible (not on a Windows operating system)");
            Destroy(this);
            return;
        }

        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        Application.wantsToQuit += HandleGameQuit;
    }

    public async void Initialize()
    {
        await CreateExecutable();

        if (!File.Exists(ExecutablePath))
        {
            Logging.Warning("Executable does not exist");
            Logging.Info(ExecutablePath);
            return;
        }

        _startInfo = new ProcessStartInfo
        {
            FileName = ExecutablePath,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        _process = new()
        {
            StartInfo = _startInfo,
            EnableRaisingEvents = true
        };

        _process.OutputDataReceived += (sender, args) =>
        {
            if (string.IsNullOrEmpty(args.Data)) return;
            OnDataReceived(args.Data);
        };

        _process.Start();

        _process.BeginOutputReadLine();
    }

    public bool HandleGameQuit()
    {
        QuitExecutable();
        return true;
    }

    public void OnDataReceived(string data)
    {
        // Logging.Info(data);

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

                        try
                        {
                            if (!string.IsNullOrEmpty(base64String) && !_thumbnailCache.TryGetValue(base64String, out texture))
                            {
                                texture = new(2, 2)
                                {
                                    filterMode = FilterMode.Point,
                                    wrapMode = TextureWrapMode.Clamp
                                };
                                texture.LoadImage(Convert.FromBase64String(base64String));
                                _thumbnailCache.TryAdd(base64String, texture);
                            }
                        }
                        catch
                        {

                        }

                        texture ??= Texture2D.whiteTexture;

                        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), Vector2.zero);

                        session.Thumbnail = sprite;

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
        string directoryName = Path.GetDirectoryName(ExecutablePath);

        if (!Directory.Exists(directoryName) || !File.Exists(ExecutablePath))
        {
            // NOTE: Unity's web request functionality doesn't work as intended when downloading and writing this file, please avoid it

            using HttpClient client = new();
            using Stream stream = await client.GetStreamAsync(Constants.URL_MediaProcess);

            if (!Directory.Exists(directoryName)) Directory.CreateDirectory(directoryName);
            using FileStream fileStream = new(ExecutablePath, FileMode.OpenOrCreate);
            await stream.CopyToAsync(fileStream);
        }

        await Task.Delay(5000);
    }

    public void QuitExecutable()
    {
        if (_process != null && !_process.HasExited)
        {
            try
            {
                _process.StandardInput.WriteLine("quit");
                _process.StandardInput.Flush();

                if (!_process.WaitForExit(2500))
                {
                    _process.Kill();
                    _process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Logging.Fatal("Could not kill consoleProcess");
                Logging.Error(ex);
            }
            finally
            {
                _process.Dispose();
                _process = null;
            }
        }
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

        public Sprite Thumbnail;
    }
}
