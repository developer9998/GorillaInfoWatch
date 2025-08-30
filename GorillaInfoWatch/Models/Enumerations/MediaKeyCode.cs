namespace GorillaInfoWatch.Models.Enumerations
{
    // https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
    public enum MediaKeyCode : uint
    {
        MuteVolume = 0xAD,
        DecreaseVolume = 0xAE,
        IncreaseVolume = 0xAF,
        NextTrack = 0xB0,
        PreviousTrack = 0xB1,
        Stop = 0xB2,
        PlayPause = 0xB3
    }
}
