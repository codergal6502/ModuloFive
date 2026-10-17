namespace ModuloFivePadClient.Utilities.Extensions.ForStrings
{
    public static class StringExtensions
    {
        public static string LimitWithEllipsis(this string str, int maxLength)
        {
            if (str.Length > maxLength)
            {
                return str.Substring(0, maxLength - 1) + "…";
            }
            else
            {
                return str;
            }

        }
    }
}