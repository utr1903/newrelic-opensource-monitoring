using Newtonsoft.Json;

namespace dotnet_first.Services.DotnetSecondService.Dtos;

public class CreateValueResponseDto
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("value")]
    public string Value { get; set; }

    [JsonProperty("tag")]
    public string Tag { get; set; }
}

