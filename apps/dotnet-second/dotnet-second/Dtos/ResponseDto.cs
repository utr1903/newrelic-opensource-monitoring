using Newtonsoft.Json;
using System.Net;

namespace dotnet_second.Dtos;

public class ResponseDto<T>
{
    [JsonProperty("message")]
    public string? Message { get; set; }

    [JsonProperty("statusCode")]
    public HttpStatusCode StatusCode { get; set; }

    [JsonProperty("data")]
    public T? Data { get; set; }
}

