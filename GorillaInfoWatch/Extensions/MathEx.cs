namespace GorillaInfoWatch.Extensions
{
    public static class MathEx
    {
        // https://github.com/KyleTheScientist/Bark/blob/3de171aca033d45f464a5120fb1932c9a0d2a3af/Extensions/MathExtensions.cs#L7
        public static int Wrap(this int x, int min, int max)
        {
            int range = max - min;
            int result = (x - min) % range;
            if (result < 0)
            {
                result += range;
            }
            return result + min;
        }
    }
}