using System;

namespace dotnet_second.Commons.Exceptions;

public class HttpRequestFailedException : Exception
{
    private readonly string _message;

    public HttpRequestFailedException(
        string message
    )
    {
        _message = message;
    }

    public string GetMessage()
        => _message;
}

