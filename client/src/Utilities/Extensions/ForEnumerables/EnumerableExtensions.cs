using Microsoft.Extensions.Logging;

namespace ModuloFivePadClient.Utilities.Extensions.ForEnumerables
{
    public static class EnumerableExtensions
    {
        public static T? CheckAndLogTwoIsTooMany<T>(this IEnumerable<T> tees, string collectionName, ILogger? logger = null)
        {
            var teeList = tees.ToList();
            if (teeList.Count > 1)
            {
                if (null != logger)
                {
                    logger.LogTwoIsTooManyWarning(teeList.Count, collectionName);
                }
            }
            return teeList.FirstOrDefault();
        }
    }
}