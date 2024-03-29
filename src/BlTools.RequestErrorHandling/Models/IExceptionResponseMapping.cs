﻿using System;
using System.Net;
using Microsoft.Extensions.Logging;

namespace BlTools.RequestErrorHandling.Models
{
    internal interface IExceptionResponseMapping
    {
        Type ExceptionType { get; }
        HttpStatusCode StatusCode { get; }
        LogLevel LogLevel { get; }
        bool IsNeedToLogExceptionStackTrace { get; }
        bool IsNeedToLogRequestBody { get; }
        bool IsNeedToLogResponseBody { get; }

        string BuildResponsePayload(Exception exception);
    }
}