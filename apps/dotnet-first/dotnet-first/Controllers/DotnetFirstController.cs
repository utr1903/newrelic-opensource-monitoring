using System.Diagnostics;
using System.Net;
using dotnet_first.Commons;
using dotnet_first.Dtos;
using dotnet_first.Logging;
using dotnet_first.Services.DotnetSecondService;
using dotnet_first.Services.DotnetSecondService.Dtos;
using dotnet_first.Services.ErrorService;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Resources;

namespace dotnet_first.Controllers;

[ApiController]
[Route("dotnet")]
public class DotnetFirstController : ControllerBase
{
    private readonly IDotnetSecondService _dotnetSecondService;
    private readonly IErrorService _errorService;

    private readonly ActivitySource _source;

    public DotnetFirstController(
        IDotnetSecondService dotnetSecondService,
        IErrorService errorService
    )
    {
        _dotnetSecondService = dotnetSecondService;
        _errorService = errorService;

        _source = new ActivitySource(Constants.OTEL_SERVICE_NAME);
    }

    [HttpPost(Name = "second")]
    [Route("second")]
    public async Task<ResponseDto<CreateValueResponseDto>> DotnetSecondMethod(
        [FromBody] CreateValueRequestDto requestDto
    )
    {
        // Create span
        using var activity = _source.StartActivity($"{nameof(DotnetFirstController)}.{nameof(DotnetSecondMethod)}");

        LogFirstDotnetServiceTriggered(activity);

        var response = await _dotnetSecondService.Run(requestDto);

        LogFirstDotnetServiceFinished(activity);

        return response;
    }

    [HttpGet(Name = "error")]
    [Route("error")]
    public ObjectResult ErrorMethod(
        [FromQuery] string errorType = "500"
    )
    {
        // Create span
        using var activity = _source.StartActivity($"{nameof(DotnetFirstController)}.{nameof(ErrorMethod)}");

        LogFirstDotnetServiceTriggered(activity);

        var response = _errorService.Run(errorType);

        LogFirstDotnetServiceFinished(activity);

        return response;
    }

    private void LogFirstDotnetServiceTriggered(
        Activity? activity
    )
    {
        CustomLogger.Run(
            new CustomLog
            {
                ClassName = nameof(DotnetFirstController),
                MethodName = nameof(DotnetSecondMethod),
                LogLevel = CustomLogLevel.INFO,
                Message = $"First Dotnet Service is triggered...",
                TraceId = activity?.TraceId.ToString(),
                SpanId = activity?.SpanId.ToString(),
            });
    }

    private void LogFirstDotnetServiceFinished(
        Activity? activity
    )
    {
        CustomLogger.Run(
            new CustomLog
            {
                ClassName = nameof(DotnetFirstController),
                MethodName = nameof(DotnetSecondMethod),
                LogLevel = CustomLogLevel.INFO,
                Message = $"First Dotnet Service is finished...",
                TraceId = activity?.TraceId.ToString(),
                SpanId = activity?.SpanId.ToString(),
            });
    }
}

