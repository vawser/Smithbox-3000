using Smithbox.Core.Editor;
using Smithbox.Core.Interface;
using Smithbox.Core.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Smithbox.Core.Utils;

// Common serializer context for JSON generation
[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata, 
    IncludeFields = true)]

// Configuration Data
[JsonSerializable(typeof(CFG))]
[JsonSerializable(typeof(UI))]
[JsonSerializable(typeof(Project))]
[JsonSerializable(typeof(WindowState))]
[JsonSerializable(typeof(ImGuiCFG))]

// Reference Data
[JsonSerializable(typeof(RowNameStore))]
[JsonSerializable(typeof(RowNameParam))]
[JsonSerializable(typeof(RowNameEntry))]

[JsonSerializable(typeof(ParamUpgraderInfo))]
[JsonSerializable(typeof(OldRegulationEntry))]
[JsonSerializable(typeof(UpgraderMassEditEntry))]

[JsonSerializable(typeof(ParamTypeInfo))]
internal partial class SmithboxSerializerContext : JsonSerializerContext
{
}
