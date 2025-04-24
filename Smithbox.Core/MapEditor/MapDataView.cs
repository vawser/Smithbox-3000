using Hexa.NET.ImGui;
using Smithbox.Core.Editor;
using System.Collections;
using System.Reflection;

namespace Smithbox.Core.MapEditorNS;

public class MapDataView
{
    public Project Project;
    public MapEditor Editor;

    public MapDataView(Project curProject, MapEditor editor)
    {
        Project = curProject;
        Editor = editor;
    }

    public void Draw()
    {
        if (Editor.Selection._selectedMap != null)
        {
            if(ImGui.CollapsingHeader("Models"))
            {
                ImGuiListClipper clipper = new ImGuiListClipper();
                clipper.Begin(Editor.Selection._selectedMap.Models.GetEntries().Count);

                while (clipper.Step())
                {
                    for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                    {
                        var curEntry = Editor.Selection._selectedMap.Models.GetEntries()[i];

                        if (ImGui.Selectable($"[{i}]:{curEntry.Name}##modelEntry{i}"))
                        {

                        }
                    }
                }
            }

            if (ImGui.CollapsingHeader("Events"))
            {
                ImGuiListClipper clipper = new ImGuiListClipper();
                clipper.Begin(Editor.Selection._selectedMap.Events.GetEntries().Count);

                while (clipper.Step())
                {
                    for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                    {
                        var curEntry = Editor.Selection._selectedMap.Events.GetEntries()[i];

                        if (ImGui.Selectable($"[{i}]:{curEntry.Name}##eventEntry{i}"))
                        {

                        }
                    }
                }
            }

            if (ImGui.CollapsingHeader("Regions"))
            {
                ImGuiListClipper clipper = new ImGuiListClipper();
                clipper.Begin(Editor.Selection._selectedMap.Regions.GetEntries().Count);

                while (clipper.Step())
                {
                    for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                    {
                        var curEntry = Editor.Selection._selectedMap.Regions.GetEntries()[i];

                        if (ImGui.Selectable($"[{i}]:{curEntry.Name}##regionEntry{i}"))
                        {

                        }
                    }
                }
            }

            if (ImGui.CollapsingHeader("Parts"))
            {
                ImGuiListClipper clipper = new ImGuiListClipper();
                clipper.Begin(Editor.Selection._selectedMap.Parts.GetEntries().Count);

                while (clipper.Step())
                {
                    for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                    {
                        var curEntry = Editor.Selection._selectedMap.Parts.GetEntries()[i];

                        if (ImGui.Selectable($"[{i}]:{curEntry.Name}##partEntry{i}"))
                        {

                        }
                    }
                }
            }
        }
    }
}
