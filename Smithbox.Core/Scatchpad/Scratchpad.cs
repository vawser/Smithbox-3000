using Hexa.NET.ImGui;
using Smithbox.Core.Editor;
using Smithbox.Core.Interface;
using Smithbox.Core.Resources;
using Smithbox.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Smithbox.Core.Scatchpad;

/// <summary>
/// This is a blank editor used for personal stuff, should not be included in release builds
/// </summary>
public class Scratchpad
{
    private Project Project;

    // Defined here so we can remove NoMove when setting up the imgui.ini
    private ImGuiWindowFlags MainWindowFlags = ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoMove;
    private ImGuiWindowFlags SubWindowFlags = ImGuiWindowFlags.NoMove;

    public int ID = 0;

    public ActionManager ActionManager;

    public Scratchpad(int id, Project projectOwner)
    {
        Project = projectOwner;
        ID = id;

        ActionManager = new();
    }

    public void Draw(string[] cmd)
    {
        ImGui.Begin($"Scratchpad##Scratchpad{ID}", MainWindowFlags);

        uint dockspaceID = ImGui.GetID($"ScratchpadDockspace{ID}");
        ImGui.DockSpace(dockspaceID, Vector2.Zero, ImGuiDockNodeFlags.PassthruCentralNode);

        Menubar();

        // Add all relevant data bank checks here since scatchpad may access any of them
        if (Project.ParamData.Initialized)
        {
            if (Project.IsSelected)
            {
                DisplayEditor();
            }
            else
            {
                ImGui.Text("You have not selected a project yet.");
            }
        }

        ImGui.End();
    }

    private unsafe void Menubar()
    {
        if (ImGui.BeginMenuBar())
        {
            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("Save"))
                {
                    Save();
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Edit"))
            {

                if(ImGui.Selectable("Color Picker"))
                {
                    ColorPicker.Show();
                }

                ImGui.EndMenu();
            }

            ImGui.EndMenuBar();
        }
    }

    private void DisplayEditor()
    {
        ImGui.Begin($"Pad##pad{ID}");

        if (ImGui.Button("Convert"))
        {
            AliasPerGame(ProjectType.AC6);
        }

        ImGui.End();
    }

    private void AliasPerGame(ProjectType projectType)
    {
        var sourcePath = @$"{AppContext.BaseDirectory}\Assets\Aliases\";

        var store = new AliasStore();
        store.Assets = new();
        BuildAliasList(@$"{sourcePath}\Assets\{LocatorUtils.GetGameDirectory(projectType)}\Asset.json", store.Assets);

        store.Characters = new();
        BuildAliasList(@$"{sourcePath}\Characters\{LocatorUtils.GetGameDirectory(projectType)}\Character.json", store.Characters);

        store.Cutscenes = new();
        BuildAliasList(@$"{sourcePath}\Cutscenes\{LocatorUtils.GetGameDirectory(projectType)}\Cutscene.json", store.Cutscenes);

        store.EventFlags = new();
        BuildAliasList(@$"{sourcePath}\Flags\{LocatorUtils.GetGameDirectory(projectType)}\EventFlag.json", store.EventFlags);

        store.Gparams = new();
        BuildAliasList(@$"{sourcePath}\Gparams\{LocatorUtils.GetGameDirectory(projectType)}\Gparams.json", store.Gparams);

        store.MapPieces = new();
        BuildAliasList(@$"{sourcePath}\MapPieces\{LocatorUtils.GetGameDirectory(projectType)}\MapPiece.json", store.MapPieces);

        store.MapNames = new();
        BuildAliasList(@$"{sourcePath}\Maps\{LocatorUtils.GetGameDirectory(projectType)}\Maps.json", store.MapNames);

        store.Movies = new();
        BuildAliasList(@$"{sourcePath}\Movies\{LocatorUtils.GetGameDirectory(projectType)}\Movie.json", store.Movies);

        store.Particles = new();
        BuildAliasList(@$"{sourcePath}\Particles\{LocatorUtils.GetGameDirectory(projectType)}\Fxr.json", store.Particles);

        store.Parts = new();
        BuildAliasList(@$"{sourcePath}\Parts\{LocatorUtils.GetGameDirectory(projectType)}\Part.json", store.Parts);

        store.Sounds = new();
        BuildAliasList(@$"{sourcePath}\Sounds\{LocatorUtils.GetGameDirectory(projectType)}\Sound.json", store.Sounds);

        store.TalkScripts = new();
        BuildAliasList(@$"{sourcePath}\Talks\{LocatorUtils.GetGameDirectory(projectType)}\Talk.json", store.TalkScripts);

        store.TimeActs = new();
        BuildAliasList(@$"{sourcePath}\TimeActs\{LocatorUtils.GetGameDirectory(projectType)}\TimeActs.json", store.TimeActs);

        var outputPath = $@"C:\Users\benja\Programming\C#\Smithbox-3000\Smithbox.Core\Assets\Aliases\{LocatorUtils.GetGameDirectory(projectType)}";

        if(!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        outputPath = outputPath + @$"\Aliases.json";

        var json = JsonSerializer.Serialize(store, SmithboxSerializerContext.Default.AliasStore);

        File.WriteAllText(outputPath, json);
    }

    private void BuildAliasList(string sourcePath, List<AliasEntry> newList)
    {
        try
        {
            var source = JsonSerializer.Deserialize(sourcePath, SmithboxSerializerContext.Default.AliasResource);
            if (source != null)
            {
                foreach (var entry in source.list)
                {
                    var newEntry = new AliasEntry();

                    newEntry.ID = entry.id;
                    newEntry.Name = entry.name;
                    newEntry.Tags = entry.tags;

                    newList.Add(newEntry);
                }
            }
            else
            {
                newList = new();
            }
        }
        catch (Exception ex)
        {
            newList = new();
        }
    }

    private async void Save()
    {
        /*
        Task<bool> saveTask = Project.ParamData.PrimaryBank.Save();
        bool saveTaskFinished = await saveTask;

        if (saveTaskFinished)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Saved primary param bank.");
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to save primary param bank.");
        }
        */
    }
}
