using Hexa.NET.ImGui;
using HKLib.hk2018.hk;
using Smithbox.Core.Actions;
using Smithbox.Core.Editor;
using Smithbox.Core.Interface.NodeEditor;
using Smithbox.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.BehaviorEditorNS;

public class BehaviorFieldView
{
    public Project Project;
    public BehaviorEditor Editor;

    public bool _showFieldsWindow = false; // Flag to control Fields window visibility
    public NodeRepresentation? _selectedNode = null; // Currently selected node for editing

    public bool DetectShortcuts = false;

    public BehaviorFieldView(Project curProject, BehaviorEditor editor)
    {
        Editor = editor;
        Project = curProject;
    }
    public void Draw()
    {
        DetectShortcuts = ShortcutUtils.UpdateShortcutDetection();

        DisplayHeader();

        // In your main drawing loop or Draw method
        if (_showFieldsWindow && _selectedNode != null)
        {
            ImGui.BeginChild("fieldTableArea");

            var tblFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders;

            if (ImGui.BeginTable($"fieldTable", 2, tblFlags))
            {
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthFixed);

                var nodeType = _selectedNode.Instance.GetType();

                FieldInfo[] array = nodeType.GetFields(BindingFlags.Public | BindingFlags.Instance);
                for (int i = 0; i < array.Length; i++)
                {
                    FieldInfo? field = array[i];

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.Text($"{field.Name}");

                    ImGui.TableSetColumnIndex(1);
                    Editor.FieldInput.DisplayFieldInput(_selectedNode, i, field);
                }

                ImGui.EndTable();
            }

            ImGui.EndChild();
        }
    }

    public void DisplayHeader()
    {

    }
}