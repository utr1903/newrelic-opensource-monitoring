using System.Diagnostics;
using System.Net;
using dotnet_second.Commons;
using dotnet_second.Dtos;
using dotnet_second.Logging;
using dotnet_second.Services.DotnetSecondService.Dtos;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Resources;

namespace dotnet_second.Controllers;

[ApiController]
[Route("dotnet")]
public class DotnetSecondController : ControllerBase
{
    private readonly ActivitySource _source;

    public DotnetSecondController()
    {
        _source = new ActivitySource(Constants.OTEL_SERVICE_NAME);
    }

    [HttpPost(Name = "second")]
    [Route("second")]
    public async Task<ResponseDto<CreateValueResponseDto>> DotnetSecondMethod(
        [FromBody] CreateValueRequestDto requestDto
    )
    {
        // Create span
        using var activity = _source.StartActivity($"{nameof(DotnetSecondController)}.{nameof(DotnetSecondMethod)}");

        LogFirstDotnetServiceTriggered(activity);

        var responseDto = new CreateValueResponseDto
        {
            Id = Guid.NewGuid().ToString(),
            Value = requestDto.Value,
            Tag = requestDto.Tag,
        };

        var response = new ResponseDto<CreateValueResponseDto>
        {
            Message = "Value is created successfully.",
            StatusCode = HttpStatusCode.Created,
            Data = responseDto,
        };

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
                ClassName = nameof(DotnetSecondController),
                MethodName = nameof(DotnetSecondMethod),
                LogLevel = CustomLogLevel.INFO,
                Message = $"Second Dotnet Service is triggered...",
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
                ClassName = nameof(DotnetSecondController),
                MethodName = nameof(DotnetSecondMethod),
                LogLevel = CustomLogLevel.INFO,
                Message = $"Second Dotnet Service is finished...",
                TraceId = activity?.TraceId.ToString(),
                SpanId = activity?.SpanId.ToString(),
            });
    }
}

