namespace GorillaInfoWatch.Models
{
    public enum DataType
    {
        /// <summary>
        /// The data will be used within the current game
        /// </summary>
        Session,
        /// <summary>
        /// The data will be stored within the user's files, and gathered when the game is initializing
        /// </summary>
        Stored
    }
}
