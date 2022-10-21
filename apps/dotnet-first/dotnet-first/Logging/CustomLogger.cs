using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace dotnet_first.Logging;

public static class CustomLogger
{
    public static void Run(
        CustomLog customLog
    )
    {
        var log = JsonConvert.SerializeObject(
            customLog,
            new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

        Console.WriteLine(log);
    }
}

