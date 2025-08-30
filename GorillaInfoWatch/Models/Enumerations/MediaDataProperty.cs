using System;

namespace GorillaInfoWatch.Models.Enumerations
{
    [Flags]
    public enum MediaDataProperty : ushort
    {
        AlbumArtist = 1 << 0,
        AlbumTitle = 1 << 1,
        AlbumTrackCount = 1 << 2,
        Artist = 1 << 3,
        Genres = 1 << 4,
        Title = 1 << 5,
        TrackNumber = 1 << 6,
        Thumbnail = 1 << 7,
        Timeline = 1 << 8,
        Status = 1 << 9,
        SessionId = 1 << 10
    }
}