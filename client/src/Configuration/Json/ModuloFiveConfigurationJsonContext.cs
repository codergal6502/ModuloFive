using System.Text.Json.Serialization;

namespace ModuloFivePadClient.Configuration.Json
{
    // See https://github.com/ProgrammerAL/json-serializer-context-registration-generator, which was on page *two* of the Duck Duck go search results! Wild!
    [JsonSourceGenerationOptions(
        UseStringEnumConverter = true,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
        IndentCharacter = ' ',
        IndentSize = 4,
        WriteIndented = true,
        AllowTrailingCommas = true
    )]
    [JsonSerializable(typeof(ModuloFiveConfiguration[]))]
    public partial class ModuloFiveConfigurationJsonContext : JsonSerializerContext
    {

    }
}