using System;
using dotnet_first.Dtos;
using dotnet_first.Services.DotnetSecondService.Dtos;
using System.Net;
using System.Diagnostics;
using dotnet_first.Logging;
using OpenTelemetry.Resources;
using dotnet_first.Commons;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_first.Services.ErrorService;

public interface IErrorService
{
    ObjectResult Run(
        string errorType
    );
}

public class ErrorService : IErrorService
{
    private readonly ActivitySource _source;

    public ErrorService()
    {
        _source = new ActivitySource(Constants.OTEL_SERVICE_NAME);
    }

    public ObjectResult Run(
        string errorType
    )
    {
        // Create span
        using var activity = _source.StartActivity($"{nameof(ErrorService)}.{nameof(Run)}");

        switch (errorType)
        {
            case "wait":
                return Wait(activity);
            case "cpu":
                return Cpu(activity);
            case "mem":
                return Mem(activity);
            default:
                return InternalServerError(activity);
        }
    }

    public ObjectResult Wait(
        Activity activity
    )
    {
        var message = "Remove Thread.Sleep() in the next commit.";

        Thread.Sleep(3000);
        CustomLogger.Run(
            new CustomLog
            {
                ClassName = nameof(ErrorService),
                MethodName = nameof(Wait),
                LogLevel = CustomLogLevel.INFO,
                Message = message,
                TraceId = activity?.TraceId.ToString(),
                SpanId = activity?.SpanId.ToString(),
            });

        var result = new ObjectResult(new ResponseDto<CreateValueResponseDto>
        {
            Message = message,
            StatusCode = HttpStatusCode.OK,
            Data = null,
        });
        result.StatusCode = (int)HttpStatusCode.OK;
        return result;
    }

    public ObjectResult Cpu(
        Activity activity
    )
    {
        var message = "Looping over 200000000 elements asynchronously...";

        Task.Run(() =>
        {
            var total = 0.0;
            for (var counter = 0; counter < 200000000; ++counter)
            {
                var temp = Math.Pow(counter, 5) / Math.Sqrt(counter) + Math.Sqrt(counter) +
                    Math.Pow(counter, 3);
                total += temp;
            }
        });

        CustomLogger.Run(
            new CustomLog
            {
                ClassName = nameof(ErrorService),
                MethodName = nameof(Cpu),
                LogLevel = CustomLogLevel.INFO,
                Message = message,
                TraceId = activity?.TraceId.ToString(),
                SpanId = activity?.SpanId.ToString(),
            });

        var result = new ObjectResult(new ResponseDto<CreateValueResponseDto>
        {
            Message = message,
            StatusCode = HttpStatusCode.OK,
            Data = null,
        });
        result.StatusCode = (int)HttpStatusCode.OK;
        return result;
    }

    public ObjectResult Mem(
        Activity activity
    )
    {
        var message = "Starting Google Chrome...";

        Task.Run(() =>
        {
            var array = new double[10000000000];
            for (var counter = 0; counter < array.Length; ++counter)
            {
                array[counter] = Double.MaxValue;
            }
        });

        CustomLogger.Run(
            new CustomLog
            {
                ClassName = nameof(ErrorService),
                MethodName = nameof(Mem),
                LogLevel = CustomLogLevel.INFO,
                Message = message,
                TraceId = activity?.TraceId.ToString(),
                SpanId = activity?.SpanId.ToString(),
            });

        var result = new ObjectResult(new ResponseDto<CreateValueResponseDto>
        {
            Message = message,
            StatusCode = HttpStatusCode.OK,
            Data = null,
        });
        result.StatusCode = (int)HttpStatusCode.OK;
        return result;
    }

    public ObjectResult InternalServerError(
        Activity activity
    )
    {
        var message = "Task failed magnificently.";

        CustomLogger.Run(
            new CustomLog
            {
                ClassName = nameof(ErrorService),
                MethodName = nameof(InternalServerError),
                LogLevel = CustomLogLevel.ERROR,
                Message = message,
                TraceId = activity?.TraceId.ToString(),
                SpanId = activity?.SpanId.ToString(),
            });

        var result = new ObjectResult(new ResponseDto<CreateValueResponseDto>
        {
            Message = message,
            StatusCode = HttpStatusCode.InternalServerError,
            Data = null,
        });
        result.StatusCode = (int)HttpStatusCode.InternalServerError;
        return result;
    }
}

