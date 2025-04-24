using Hexa.NET.ImGui;
using Smithbox.Core.BehaviorEditorNS;
using Smithbox.Core.Editor;
using Smithbox.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.CollisionEditorNS;

public class CollisionFieldView
{
    public Project Project;
    public CollisionEditor Editor;

    public bool DetectShortcuts = false;

    public CollisionFieldView(Project curProject, CollisionEditor editor)
    {
        Editor = editor;
        Project = curProject;
    }
    public void Draw()
    {
        DetectShortcuts = ShortcutUtils.UpdateShortcutDetection();

        // In your main drawing loop or Draw method
        if (Editor.Selection.SelectedObject != null)
        {
            ImGui.BeginChild("fieldTableArea");

            var tblFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders;

            if (ImGui.BeginTable($"fieldTable", 2, tblFlags))
            {
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthFixed);

                var objType = Editor.Selection.SelectedObject.GetType();

                FieldInfo[] array = objType.GetFields(BindingFlags.Public | BindingFlags.Instance);
                for (int i = 0; i < array.Length; i++)
                {
                    FieldInfo? field = array[i];

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.Text($"{field.Name}");

                    ImGui.TableSetColumnIndex(1);
                    Editor.FieldInput.DisplayFieldInput(Editor.Selection.SelectedObject, i, field);
                }

                ImGui.EndTable();
            }

            ImGui.EndChild();
        }
    }
}
