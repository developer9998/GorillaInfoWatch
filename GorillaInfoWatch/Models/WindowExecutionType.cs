namespace GorillaInfoWatch.Models
{
    /// <summary>
    /// An enum representing how a tab is executed
    /// </summary>
    public enum WindowExecutionType
    {
        /// <summary>
        /// The tab will activate and display on the watch
        /// </summary>
        Viewable,

        /// <summary>
        /// The tab will only activate
        /// </summary>
        Callable
    }
}
