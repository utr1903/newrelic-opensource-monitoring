using System;
namespace dotnet_second.Commons.Exceptions;

public class ParsingFailedException : Exception
{
    private readonly string _message;

    public ParsingFailedException(
        string message
    )
    {
        _message = message;
    }

    public string GetMessage()
        => _message;
}

