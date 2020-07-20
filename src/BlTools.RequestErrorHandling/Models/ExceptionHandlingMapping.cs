using System;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace BlTools.RequestErrorHandling.Models
{
    public sealed class ExceptionResponseMapping<T>: IExceptionResponseMapping where T : Exception
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly Func<T, object> _buildResponseObjectFunc;

        public Type ExceptionType => typeof(T);
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
        public LogLevel LogLevel { get; set; } = LogLevel.Information;
        public bool IsNeedToLogExceptionStackTrace { get; set; } = false;

        public ExceptionResponseMapping(Func<T, object> buildResponseObjectFunc)
        {
            _buildResponseObjectFunc = buildResponseObjectFunc;
        }

        public string BuildResponsePayload(Exception exception)
        {
            if (!(exception is T exceptionForProcess))
            {
                return null;
            }
            var responseObject = _buildResponseObjectFunc(exceptionForProcess);

            var responsePayload = JsonSerializer.Serialize(responseObject, _jsonOptions);
            return responsePayload;
        }
    }
}