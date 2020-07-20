using System;
using BlTools.RequestErrorHandling.Models;
using Microsoft.AspNetCore.Builder;

namespace BlTools.RequestErrorHandling
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseRequestErrorHandling(
            this IApplicationBuilder app,
            Action<RequestErrorHandlingOptions> configureOptions = null)
        {
            var requestLoggingOptions = new RequestErrorHandlingOptions();

            configureOptions?.Invoke(requestLoggingOptions);

            if (requestLoggingOptions.MessageTemplateForNotResolvedAction == null)
                throw new ArgumentException("MessageTemplateForNotResolvedAction cannot be null.");
            if (requestLoggingOptions.MessageTemplateForResolvedActionWithSuccessResult == null)
                throw new ArgumentException("MessageTemplateForResolvedActionWithSuccessResult cannot be null.");
            if (requestLoggingOptions.MessageTemplateForResolvedActionWithFailedResult == null)
                throw new ArgumentException("MessageTemplateForResolvedActionWithFailedResult cannot be null.");
            if (requestLoggingOptions.GetLogLevel == null)
                throw new ArgumentException("GetLogLevel cannot be null.");
            return app.UseMiddleware<RequestErrorHandlingMiddleware>(requestLoggingOptions);
        }
    }
}