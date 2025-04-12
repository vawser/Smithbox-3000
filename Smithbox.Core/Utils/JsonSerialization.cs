using Smithbox.Core.Editor;
using Smithbox.Core.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Smithbox.Core.Utils;

[JsonSourceGenerationOptions(WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata, IncludeFields = true)]
[JsonSerializable(typeof(CFG))]
[JsonSerializable(typeof(UI))]
[JsonSerializable(typeof(Project))]
internal partial class SmithboxSerializerContext : JsonSerializerContext
{
}

public class JsonSerialization
{
}
