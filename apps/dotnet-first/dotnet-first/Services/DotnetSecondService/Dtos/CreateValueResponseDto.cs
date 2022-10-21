using System;
using Newtonsoft.Json;

namespace dotnet_first.Services.DotnetSecondService.Dtos;

public class CreateValueRequestDto
{
    [JsonProperty("value")]
    public string? Value { get; set; }

    [JsonProperty("tag")]
    public string? Tag { get; set; }
}

