using System.Collections.Generic;

namespace GorillaInfoWatch.Models
{
    public abstract class ScreenContent
    {
        /// <summary>
        /// All lines at a specified page
        /// </summary>
        public abstract IEnumerable<ScreenLine> GetPageLines(int page);

        // Given title at a specified page
        public abstract string GetPageTitle(int page);

        /// <summary>
        /// Total page count
        /// </summary>
        public abstract int GetPageCount();
    }
}