using Hexa.NET.ImGui;
using Smithbox.Core.ParamEditorNS;
using Smithbox.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Smithbox.Core.Editor;

/// <summary>
/// This is the project-level handler, includes all data and editors for the project
/// </summary>
public class Project
{
    public Guid ProjectGUID;
    public string ProjectName;
    public string ProjectPath;
    public string DataPath;
    public ProjectType ProjectType;

    public Project() { }

    public Project(Guid newGuid, string projectName, string projectPath, string dataPath, ProjectType projectType)
    {
        ProjectGUID = newGuid;
        ProjectName = projectName;
        ProjectPath = projectPath;
        DataPath = dataPath;
        ProjectType = projectType;
    }

    /// <summary>
    /// If true, the data elements (i.e. Aliases and Editors) for this project have been initialized.
    /// </summary>
    [JsonIgnore]
    private bool Initialized = false;

    /// <summary>
    /// If true, this project is the currently selected project.
    /// </summary>
    [JsonIgnore]
    public bool IsSelected = false;

    [JsonIgnore]
    public ParamEditor PrimaryParamEditor;

    [JsonIgnore]
    public ParamEditor SecondaryParamEditor;

    [JsonIgnore]
    public ParamData ParamData;

    public void Initialize()
    {
        // Data is initialized separately to the editors themselves
        ParamData = new(this);

        PrimaryParamEditor = new ParamEditor(0, this);
        SecondaryParamEditor = new ParamEditor(1, this);

        Initialized = true;
    }

    public void Draw()
    {
        if (!Initialized)
        {
            Initialize();
        }

        ParamData.Update();

        // Param Editors
        if (CFG.Current.DisplayPrimaryParamEditor)
        {
            PrimaryParamEditor.Draw();
        }

        if (CFG.Current.DisplaySecondaryParamEditor)
        {
            SecondaryParamEditor.Draw();
        }
    }

    public void Save()
    {
        var folder = FolderUtils.GetProjectFolder();
        var file = Path.Combine(folder, $"{ProjectGUID}.json");

        var json = JsonSerializer.Serialize(this, SmithboxSerializerContext.Default.Project);

        File.WriteAllText(file, json);
    }
}
