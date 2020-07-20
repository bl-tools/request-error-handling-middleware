using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace BlTools.RequestErrorHandling.Models
{
    public sealed class RequestErrorHandlingOptions
    {
        private const string defaultMessageTemplateForResolvedActionWithSuccessResult = "{ResolvedAction} OK ({RequestPath})";
        private const string defaultMessageTemplateForResolvedActionWithFailedResult = "{ResolvedAction} Fail: {ErrorMessage} ({RequestPath})";
        private const string defaultMessageTemplateForNotResolvedAction = "{RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed} ms";

        private static LogLevel DefaultGetLogLevel(HttpContext context, double _, Exception ex)
        {
            return ex == null && context.Response.StatusCode <= 499 ? LogLevel.Information : LogLevel.Error;
        }

        public Func<HttpContext, double, Exception, LogLevel> GetLogLevel { get; set; }
        public string MessageTemplateForNotResolvedAction { get; set; }
        public string MessageTemplateForResolvedActionWithSuccessResult { get; set; }
        public string MessageTemplateForResolvedActionWithFailedResult { get; set; }

        internal List<IExceptionResponseMapping> ExceptionResponseMappingCollection { get; }

        internal RequestErrorHandlingOptions()
        {
            GetLogLevel = DefaultGetLogLevel;
            MessageTemplateForNotResolvedAction = defaultMessageTemplateForNotResolvedAction;
            MessageTemplateForResolvedActionWithSuccessResult = defaultMessageTemplateForResolvedActionWithSuccessResult;
            MessageTemplateForResolvedActionWithFailedResult = defaultMessageTemplateForResolvedActionWithFailedResult;
            ExceptionResponseMappingCollection = new List<IExceptionResponseMapping>();
        }

        public RequestErrorHandlingOptions AddExceptionResponseMapping<T>(ExceptionResponseMapping<T> mapping)
            where T : Exception
        {
            ExceptionResponseMappingCollection.Add(mapping);
            return this;
        }
    }
}