using Hexa.NET.ImGui;
using Smithbox.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Smithbox.Core.Editor;

[JsonSerializable(typeof(Project))]
internal partial class ProjectSerializerContext : JsonSerializerContext
{
}

/// <summary>
/// This is the project-level handler, includes all data and editors for the project
/// </summary>
public class Project
{
    private Guid ProjectGUID;

    [JsonIgnore]
    private string _projectName = "Test";

    [JsonIgnore]
    private string _projectPath = "";

    public Project()
    {
    }
    public Project(Guid newGuid)
    {
        ProjectGUID = newGuid;
    }
    public void Draw()
    {
        ImGui.Begin($"{_projectName}##ProjectDock_{ProjectGUID}");



        ImGui.End();
    }

    // Save the <name>.json to the .smithbox folder
    public void Save()
    {

    }
}
