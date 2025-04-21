using Hexa.NET.ImGui;
using Smithbox.Core.Editor;
using Smithbox.Core.Interface.NodeEditor;
using System;
using System.Collections.Generic;
using System.Linq;
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

    public BehaviorFieldView(Project curProject, BehaviorEditor editor)
    {
        Editor = editor;
        Project = curProject;
    }
    public void Draw()
    {
        // In your main drawing loop or Draw method
        if (_showFieldsWindow && _selectedNode != null)
        {
            // Show editable fields for the selected node
            Type nodeType = _selectedNode.Instance.GetType();

            foreach (var field in nodeType.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                object? value = field.GetValue(_selectedNode.Instance);
                string fieldName = field.Name;

                // Edit the field based on its type (here we support int, string, float as an example)
                if (value is int intValue)
                {
                    int newValue = intValue;
                    if (ImGui.InputInt(fieldName, ref newValue))
                    {
                        field.SetValue(_selectedNode.Instance, newValue);
                    }
                }
                else if (value is float floatValue)
                {
                    float newValue = floatValue;
                    if (ImGui.InputFloat(fieldName, ref newValue))
                    {
                        field.SetValue(_selectedNode.Instance, newValue);
                    }
                }
                else if (value is string stringValue)
                {
                    string newValue = stringValue;
                    if (ImGui.InputText(fieldName, ref newValue, 255))
                    {
                        field.SetValue(_selectedNode.Instance, newValue);
                    }
                }
                // Add more field types as needed (e.g., bool, enum, etc.)
            }
        }
    }
}