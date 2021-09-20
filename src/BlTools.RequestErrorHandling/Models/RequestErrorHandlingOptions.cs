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

        private static bool DefaultCheckRequestBodyShouldBeLogged(HttpContext _)
        {
            return false;
        }

        private static bool DefaultCheckResponseBodyShouldBeLogged(HttpContext _)
        {
            return false;
        }

        public Func<HttpContext, double, Exception, LogLevel> GetLogLevel { get; set; }


        internal Func<HttpContext, bool> CheckRequestBodyShouldBeLogged { get; set; }
        internal Func<HttpContext, bool> CheckResponseBodyShouldBeLogged { get; set; }


        public string MessageTemplateForNotResolvedAction { get; set; }
        public string MessageTemplateForResolvedActionWithSuccessResult { get; set; }
        public string MessageTemplateForResolvedActionWithFailedResult { get; set; }

        internal bool IsAdditionalLoggingOptionsEnabled { get; private set; }

        internal List<IExceptionResponseMapping> ExceptionResponseMappingCollection { get; }

        internal RequestErrorHandlingOptions()
        {
            GetLogLevel = DefaultGetLogLevel;
            CheckRequestBodyShouldBeLogged = DefaultCheckRequestBodyShouldBeLogged;
            CheckResponseBodyShouldBeLogged = DefaultCheckResponseBodyShouldBeLogged;
            MessageTemplateForNotResolvedAction = defaultMessageTemplateForNotResolvedAction;
            MessageTemplateForResolvedActionWithSuccessResult = defaultMessageTemplateForResolvedActionWithSuccessResult;
            MessageTemplateForResolvedActionWithFailedResult = defaultMessageTemplateForResolvedActionWithFailedResult;
            ExceptionResponseMappingCollection = new List<IExceptionResponseMapping>();
        }

        public RequestErrorHandlingOptions AddExceptionResponseMapping<T>(ExceptionResponseMapping<T> mapping)
            where T : Exception
        {
            ExceptionResponseMappingCollection.Add(mapping);
            if (mapping.IsNeedToLogRequestBody)
            {
                IsAdditionalLoggingOptionsEnabled = true;
            }
            return this;
        }

        public void EnableAdditionalLoggingOptions(Func<HttpContext, bool> checkRequestBodyShouldBeLogged,
                                                   Func<HttpContext, bool> checkResponseBodyShouldBeLogged)
        {
            if (checkRequestBodyShouldBeLogged != null)
            {
                IsAdditionalLoggingOptionsEnabled = true;
                CheckRequestBodyShouldBeLogged = checkRequestBodyShouldBeLogged;
            }

            if (checkResponseBodyShouldBeLogged != null)
            {
                IsAdditionalLoggingOptionsEnabled = true;
                CheckResponseBodyShouldBeLogged = checkResponseBodyShouldBeLogged;
            }
        }
    }
}