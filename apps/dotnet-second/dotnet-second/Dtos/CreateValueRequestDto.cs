using Newtonsoft.Json;

namespace dotnet_second.Dtos;

public class CreateValueRequestDto
{
    [JsonProperty("value")]
    public string? Value { get; set; }

    [JsonProperty("tag")]
    public string? Tag { get; set; }
}

