namespace GorillaInfoWatch.Extensions
{
    public static class MathEx
    {
        public static int Wrap(this int x, int min, int max) // 0, 2
        {
            int range = max - min; // 2
            int result = (x - min) % range; // (2 - 0) % 2 = 0
            if (result < 0)
            {
                result += range;
            }
            return result + min; // 0 + 0 (comments so my ass up at 4:56 AM knows what im doing)
        }
    }
}