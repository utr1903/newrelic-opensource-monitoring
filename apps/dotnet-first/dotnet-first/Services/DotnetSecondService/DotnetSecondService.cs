using System.Diagnostics;
using System.Net;
using System.Text;
using dotnet_first.Commons;
using dotnet_first.Commons.Exceptions;
using dotnet_first.Controllers;
using dotnet_first.Dtos;
using dotnet_first.Logging;
using dotnet_first.Services.DotnetSecondService.Dtos;
using Google.Protobuf;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OpenTelemetry.Resources;

namespace dotnet_first.Services.DotnetSecondService;

public interface IDotnetSecondService
{
    Task<ResponseDto<CreateValueResponseDto>> Run(
        CreateValueRequestDto requestDto
    );
}

public class DotnetSecondService : IDotnetSecondService
{
    private const string DOTNET_SECOND_URI =
        "http://dotnet-second.dotnet.svc.cluster.local:8080/dotnet/second";

    private readonly HttpClient _httpClient;

    private readonly ActivitySource _source;

    public DotnetSecondService(
        IHttpClientFactory factory
    )
    {
        _httpClient = factory.CreateClient();

        _source = new ActivitySource(Constants.OTEL_SERVICE_NAME);
    }

    public async Task<ResponseDto<CreateValueResponseDto>> Run(
        CreateValueRequestDto requestDto
    )
    {
        try
        {
            // Create span
            using var activity = _source.StartActivity($"{nameof(DotnetSecondService)}.{nameof(Run)}");
            
            // Parse request
            var requestDtoAsString = ParseRequestDto(requestDto);

            // Call second dotnet service
            var responseMessage = PerformHttpRequest(requestDtoAsString);

            // Parse response
            return await ParseResponseDto(responseMessage);
        }
        catch (ParsingFailedException e)
        {
            return new ResponseDto<CreateValueResponseDto>
            {
                Message = e.GetMessage(),
                StatusCode = HttpStatusCode.BadRequest,
                Data = null,
            };
        }
        catch (HttpRequestFailedException e)
        {
            return new ResponseDto<CreateValueResponseDto>
            {
                Message = e.GetMessage(),
                StatusCode = HttpStatusCode.InternalServerError,
                Data = null,
            };
        }
    }

    private string ParseRequestDto(
        CreateValueRequestDto requestDto
    )
    {
        // Create span
        using var activity = _source.StartActivity($"{nameof(DotnetSecondService)}.{nameof(ParseRequestDto)}");

        try
        {
            LogParsingRequestDto(activity);

            var requestDtoAsString = JsonConvert.SerializeObject(requestDto);

            LogParsingRequestDtoSuccessful(activity);

            return requestDtoAsString;
        }
        catch (Exception e)
        {
            var message = "Request body is invalid.";
            LogParsingRequestDtoFailed(e, message, activity);
            throw new ParsingFailedException(message);
        }
    }

    private HttpResponseMessage PerformHttpRequest(
        string requestDtoAsString
    )
    {
        // Create span
        using var activity = _source.StartActivity($"{nameof(DotnetSecondService)}.{nameof(PerformHttpRequest)}");

        try
        {
            LogPerformingHttpRequest(activity);

            var stringContent = new StringContent(
            requestDtoAsString,
            Encoding.UTF8,
            "application/json"
        );

            var httpRequest = new HttpRequestMessage(
                HttpMethod.Post,
                DOTNET_SECOND_URI
            )
            {
                Content = stringContent
            };

            var response = _httpClient.Send(httpRequest);

            LogPerformingHttpRequestSuccessful(activity);

            return response;
        }
        catch (Exception e)
        {
            var message = "Performing HTTP request to second Dotnet Service is failed.";
            LogPerformingHttpRequestFailed(e, message, activity);
            throw new HttpRequestFailedException(message);
        }
    }

    private async Task<ResponseDto<CreateValueResponseDto>> ParseResponseDto(
        HttpResponseMessage responseMessage
    )
    {
        // Create span
        using var activity = _source.StartActivity($"{nameof(DotnetSecondService)}.{nameof(ParseResponseDto)}");

        try
        {
            LogParsingResponseDto(activity);

            var responseAsString = await responseMessage.Content.ReadAsStringAsync();

            var response = JsonConvert.DeserializeObject<ResponseDto<CreateValueResponseDto>>(responseAsString);

            LogParsingResponseDtoSuccessful(activity);

            return new ResponseDto<CreateValueResponseDto>
            {
                Message = "Value is created successfully.",
                StatusCode = response.StatusCode,
                Data = response.Data,
            };
        }
        catch (Exception e)
        {
            var message = "Response body could not be parsed.";
            LogParsingResponseDtoFailed(e, message, activity);
            throw new ParsingFailedException(message);
        }
    }

    private void LogParsingRequestDto(
        Activity? activity
    )
    {
        CustomLogger.Run(
            new CustomLog
            {
                ClassName = nameof(DotnetSecondService),
                MethodName = nameof(ParseRequestDto),
                LogLevel = CustomLogLevel.INFO,
                Message = "Parsing request DTO...",
                TraceId = activity?.TraceId.ToString(),
                SpanId = activity?.SpanId.ToString(),
            });
    }

    private void LogParsingRequestDtoSuccessful(
        Activity? activity
    )
    {
        CustomLogger.Run(
            new CustomLog
            {
                ClassName = nameof(DotnetSecondService),
                MethodName = nameof(ParseRequestDto),
                LogLevel = CustomLogLevel.INFO,
                Message = "Parsing request DTO is successful.",
                TraceId = activity?.TraceId.ToString(),
                SpanId = activity?.SpanId.ToString(),
            });
    }

    private void LogParsingRequestDtoFailed(
        Exception e,
        string message,
        Activity? activity
    )
    {
        CustomLogger.Run(
            new CustomLog
            {
                ClassName = nameof(DotnetSecondService),
                MethodName = nameof(ParseRequestDto),
                LogLevel = CustomLogLevel.ERROR,
                Message = message,
                Exception = e.Message,
                StackTrace = e.StackTrace,
                TraceId = activity?.TraceId.ToString(),
                SpanId = activity?.SpanId.ToString(),
            });
    }

    private void LogPerformingHttpRequest(
        Activity? activity
    )
    {
        CustomLogger.Run(
            new CustomLog
            {
                ClassName = nameof(DotnetSecondService),
                MethodName = nameof(PerformHttpRequest),
                LogLevel = CustomLogLevel.INFO,
                Message = $"Performing HTTP request to second Dotnet Service...",
                TraceId = activity?.TraceId.ToString(),
                SpanId = activity?.SpanId.ToString(),
            });
    }

    private void LogPerformingHttpRequestSuccessful(
        Activity? activity
    )
    {
        CustomLogger.Run(
            new CustomLog
            {
                ClassName = nameof(DotnetSecondService),
                MethodName = nameof(PerformHttpRequest),
                LogLevel = CustomLogLevel.INFO,
                Message = $"Performing HTTP request to second Dotnet Service is successful.",
                TraceId = activity?.TraceId.ToString(),
                SpanId = activity?.SpanId.ToString(),
            });
    }

    private void LogPerformingHttpRequestFailed(
        Exception e,
        string message,
        Activity? activity
    )
    {
        CustomLogger.Run(
            new CustomLog
            {
                ClassName = nameof(DotnetSecondService),
                MethodName = nameof(PerformHttpRequest),
                LogLevel = CustomLogLevel.ERROR,
                Message = message,
                Exception = e.Message,
                StackTrace = e.StackTrace,
                TraceId = activity?.TraceId.ToString(),
                SpanId = activity?.SpanId.ToString(),
            });
    }

    private void LogParsingResponseDto(
        Activity? activity
    )
    {
        CustomLogger.Run(
            new CustomLog
            {
                ClassName = nameof(DotnetSecondService),
                MethodName = nameof(ParseResponseDto),
                LogLevel = CustomLogLevel.INFO,
                Message = "Parsing response DTO...",
                TraceId = activity?.TraceId.ToString(),
                SpanId = activity?.SpanId.ToString(),
            });
    }

    private void LogParsingResponseDtoSuccessful(
        Activity? activity
    )
    {
        CustomLogger.Run(
            new CustomLog
            {
                ClassName = nameof(DotnetSecondService),
                MethodName = nameof(ParseResponseDto),
                LogLevel = CustomLogLevel.INFO,
                Message = "Parsing response DTO is successful.",
                TraceId = activity?.TraceId.ToString(),
                SpanId = activity?.SpanId.ToString(),
            });
    }

    private void LogParsingResponseDtoFailed(
        Exception e,
        string message,
        Activity? activity
    )
    {
        CustomLogger.Run(
            new CustomLog
            {
                ClassName = nameof(DotnetSecondService),
                MethodName = nameof(ParseResponseDto),
                LogLevel = CustomLogLevel.ERROR,
                Message = message,
                Exception = e.Message,
                StackTrace = e.StackTrace,
                TraceId = activity?.TraceId.ToString(),
                SpanId = activity?.SpanId.ToString(),
            });
    }
}

