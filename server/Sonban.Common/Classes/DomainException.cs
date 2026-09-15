using System;
using System.Net;

namespace Sonban.Common.Classes;

public class DomainException : Exception
{
    public HttpStatusCode ErrorCode { get; }

    public DomainException(HttpStatusCode errorCode, string errorMessage) : base(errorMessage)
    {
        ErrorCode = errorCode;
    }
}
