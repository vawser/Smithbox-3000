using Hexa.NET.ImGui;
using Org.BouncyCastle.Utilities;
using Smithbox.Core.BehaviorEditorNS;
using Smithbox.Core.Editor;
using Smithbox.Core.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.CollisionEditorNS;

public class CollisionTreeView
{
    public Project Project;
    public CollisionEditor Editor;

    public bool DetectShortcuts = false;

    public CollisionTreeView(Project curProject, CollisionEditor editor)
    {
        Editor = editor;
        Project = curProject;
    }
    public void Draw()
    {
        DetectShortcuts = ShortcutUtils.UpdateShortcutDetection();

        ImGui.BeginChild("collisionTreeArea");

        DrawObjectTree($"Root##root", Editor.Selection._selectedHavokRoot);

        ImGui.EndChild();
    }

    public void DrawObjectTree(string label, object? obj, HashSet<object>? visited = null)
    {
        if (obj == null)
        {
            ImGui.Text($"{label}: null");
            return;
        }

        visited ??= new HashSet<object>();
        if (!visited.Add(obj))
        {
            ImGui.Text($"{label}: (circular reference)");
            return;
        }

        Type type = obj.GetType();
        bool isLeaf = type.IsPrimitive || type == typeof(string) || type.IsEnum;

        if (isLeaf)
        {
            ImGui.Text($"{label}: {obj}");
        }
        else if (obj is IList list)
        {
            if (ImGui.TreeNodeEx($"{label} ({type.Name}) [{list.Count}]"))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    DrawObjectTree($"[{i}]", list[i], visited);
                }
                ImGui.TreePop();
            }
        }
        else
        {
            // Here we check if the tree node is clicked
            if (ImGui.TreeNodeEx($"{label} ({type.Name})"))
            {
                // When clicked, set the selected root to this object
                if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                {
                    Editor.Selection.SelectedObject = obj;
                    Editor.NodeView._selectedRoot = obj;
                    Editor.NodeView._needsRebuild = true;
                }

                // Traverse and display properties/fields
                foreach (var prop in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    object? value = prop.GetValue(obj);
                    DrawObjectTree(prop.Name, value, visited);
                }
                ImGui.TreePop();
            }
        }

        visited.Remove(obj);
    }
}
