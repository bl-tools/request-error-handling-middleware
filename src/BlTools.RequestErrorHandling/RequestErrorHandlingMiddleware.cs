using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BlTools.RequestErrorHandling.Models;

namespace BlTools.RequestErrorHandling
{
    internal sealed class RequestErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RequestErrorHandlingOptions _options;
        private readonly ILogger<RequestErrorHandlingMiddleware> _logger;

        public RequestErrorHandlingMiddleware(RequestDelegate next, RequestErrorHandlingOptions options, ILogger<RequestErrorHandlingMiddleware> logger)
        {
            _next = next;
            _options = options;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                Activity.Current.AddTag("RequestMethod", httpContext.Request.Method);
                Activity.Current.AddTag("RequestPath", GetPath(httpContext));

                var resolvedAction = GetResolvedAction(httpContext);
                Activity.Current.AddTag("ResolvedAction", resolvedAction);

                await _next(httpContext);

                Activity.Current.AddTag("StatusCode", httpContext.Response.StatusCode.ToString());
                Activity.Current.AddTag("Elapsed", stopwatch.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture));

                var logLevel = _options.GetLogLevel(httpContext, 0, null);
                if (resolvedAction != null)
                {
                    Activity.Current.AddTag("IsSuccess", "True");
                    LogAsResolvedActionWithSuccessResult(logLevel);
                }
                else
                {
                    Activity.Current.AddTag("IsSuccess", "False");
                    LogAsNotResolvedAction(logLevel);
                }
            }

            catch (Exception ex)
            {
                var isExceptionHandled = await TryHandleExceptionAsync(httpContext, ex, stopwatch.ElapsedMilliseconds);
                if (!isExceptionHandled)
                {
                    Activity.Current.AddTag("StatusCode", ((int)HttpStatusCode.InternalServerError).ToString());
                    var logLevel = _options.GetLogLevel(httpContext, stopwatch.ElapsedMilliseconds, ex);
                    LogAsOkResolvedActionWithFailedResult(logLevel, ex);
                    throw;
                }
            }
        }

        private async Task<bool> TryHandleExceptionAsync(HttpContext httpContext, Exception ex, double elapsedMilliseconds)
        {
            Activity.Current.AddTag("Elapsed", elapsedMilliseconds.ToString(CultureInfo.InvariantCulture));
            Activity.Current.AddTag("IsSuccess", "False");
            Activity.Current.AddTag("ErrorMessage", ex.Message);


            var exceptionType = ex.GetType();
            var exceptionMapping = _options.ExceptionResponseMappingCollection.FirstOrDefault(p => p.ExceptionType == exceptionType);
            exceptionMapping ??= _options.ExceptionResponseMappingCollection.FirstOrDefault(p => exceptionType.IsSubclassOf(p.ExceptionType));
            var isMappingFound = exceptionMapping != null;

            if (isMappingFound)
            {
                httpContext.Response.ContentType = "application/json";
                httpContext.Response.StatusCode = (int)exceptionMapping.StatusCode;
                
                var responsePayload = exceptionMapping.BuildResponsePayload(ex);
                if (responsePayload != null)
                {
                    await httpContext.Response.WriteAsync(responsePayload);
                }

                Activity.Current.AddTag("StatusCode", httpContext.Response.StatusCode.ToString());
                ex = exceptionMapping.IsNeedToLogExceptionStackTrace ? ex : null;
                LogAsOkResolvedActionWithFailedResult(exceptionMapping.LogLevel, ex);
            }

            return isMappingFound;
        }

        private static string GetResolvedAction(HttpContext httpContext)
        {
            string resolvedAction = null;
            var routeData = httpContext.GetRouteData();

            if (routeData?.Values != null)
            {
                routeData.Values.TryGetValue("controller", out var controllerName);
                routeData.Values.TryGetValue("action", out var actionName);

                if (controllerName != null && actionName != null)
                {
                    resolvedAction = $"{controllerName}.{actionName}";
                }
            }

            return resolvedAction;
        }

        private void LogAsResolvedActionWithSuccessResult(LogLevel logLevel, Exception ex = null)
        {
            _logger.Log(logLevel, ex, _options.MessageTemplateForResolvedActionWithSuccessResult);
        }

        private void LogAsOkResolvedActionWithFailedResult(LogLevel logLevel, Exception ex = null)
        {
            _logger.Log(logLevel, ex, _options.MessageTemplateForResolvedActionWithFailedResult);
        }

        private void LogAsNotResolvedAction(LogLevel logLevel, Exception ex = null)
        {
            _logger.Log(logLevel, ex, _options.MessageTemplateForNotResolvedAction);
        }

        private static string GetPath(HttpContext httpContext)
        {
            return httpContext.Features.Get<IHttpRequestFeature>()?.RawTarget ?? httpContext.Request.Path.ToString();
        }
    }
}