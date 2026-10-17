using Microsoft.Extensions.Logging;

namespace ModuloFivePadClient.Utilities
{
    public static class LoggerExtensions
    {

#warning every one of these error logs should be evaluated for an "uh oh!" dialog
        public static void LogNullObjectError(this ILogger logger, string objectName, string className, string functionMame)
        {
            logger.LogNullObject(LogLevel.Error, objectName, className, functionMame);
        }

        public static void LogNullObjectCritical(this ILogger logger, string objectName, string className, string functionMame)
        {
            logger.LogNullObject(LogLevel.Critical, objectName, className, functionMame);
        }

        public static void LogNullObject(this ILogger logger, LogLevel logLevel, string objectName, string className, string functionMame)
        {
            logger.Log(
                logLevel
              , "Object {objectName} was null value in {className}.{functionName}."
              , objectName
              , className
              , functionMame
            );
            return;
        }

        public static void LogNullObjectError(this ILogger logger, string key, string collectionName, string className, string functionMame)
        {
            logger.LogNullObject(LogLevel.Error, collectionName, className, className, functionMame);
        }

        public static void LogNullObjectCritical(this ILogger logger, string key, string collectionName, string className, string functionMame)
        {
            logger.LogNullObject(LogLevel.Critical, collectionName, className, className, functionMame);
        }

        public static void LogNullObject(this ILogger logger, LogLevel logLevel, string key, string collectionName, string className, string functionMame)
        {
            logger.Log(
                logLevel
              , "Key {macroPdsEnum} was not in collection {dictionary} yielded a null value in {className}.{functionName}."
              , key
              , collectionName
              , className
              , functionMame
            );
        }

        public static void LogUnexpectedEventTargetError(this ILogger logger, string eventTargetName, string exepctedObjectName, string className, string functionMame)
        {
            logger.LogUnexpectedEventTarget(LogLevel.Error, eventTargetName, exepctedObjectName, className, functionMame);
        }

        public static void LogUnexpectedEventTargetCritital(this ILogger logger, string eventTargetName, string exepctedObjectName, string className, string functionMame)
        {
            logger.LogUnexpectedEventTarget(LogLevel.Critical, eventTargetName, exepctedObjectName, className, functionMame);
        }
        public static void LogUnexpectedEventTarget(this ILogger logger, LogLevel logLevel, string eventTargetName, string exepctedObjectName, string className, string functionMame)
        {
            logger.Log(
                logLevel
              , "Expected event target {eventTargetName} to be the same object as {exepctedObjectName} but it was a different object in {className}.{functionName}."
              , eventTargetName
              , exepctedObjectName
              , className
              , functionMame
            );
        }

        public static void LogBadIndexError(this ILogger logger, string indexName, int? badIndex, string collectionName, int collectionSize, string className, string functionMame)
        {
            logger.LogBadIndex(LogLevel.Error, indexName, badIndex, collectionName, collectionSize, className, functionMame);
        }

        public static void LogBadIndexCritical(this ILogger logger, string indexName, int? badIndex, string collectionName, int collectionSize, string className, string functionMame)
        {
            logger.LogBadIndex(LogLevel.Critical, indexName, badIndex, collectionName, collectionSize, className, functionMame);
        }

        public static void LogBadIndex(this ILogger logger, LogLevel logLevel, string indexName, int? badIndex, string collectionName, int collectionSize, string className, string functionMame)
        {
            logger.Log(
                logLevel
              , "Got bad index {indexNamee} == {badIndex} into collection {collectionName} with size {collectionSize} but it was a different object in {className}.{functionName}."
              , indexName
              , badIndex?.ToString() ?? "null"
              , collectionName
              , collectionSize
              , className
              , functionMame
            );
        }

        public static void LogTwoIsTooManyWarning(this ILogger logger, int collectionSize, string collectionName)
        {
            logger.LogTwoIsTooMany(LogLevel.Warning, collectionSize, collectionName);
        }

        public static void LogTwoIsTooManyError(this ILogger logger, int collectionSize, string collectionName)
        {
            logger.LogTwoIsTooMany(LogLevel.Error, collectionSize, collectionName);
        }

        public static void LogTwoIsTooMany(this ILogger logger, LogLevel logLevel, int collectionSize, string collectionName)
        {
            logger.Log(
                logLevel
              , "Expected at most one item in {collectionName} but got {count}."
              , collectionName
              , collectionSize
            );
        }
    }
}