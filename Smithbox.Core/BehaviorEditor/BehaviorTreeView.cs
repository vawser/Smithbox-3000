using Hexa.NET.ImGui;
using HKLib.hk2018;
using Smithbox.Core.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.BehaviorEditorNS;

public class BehaviorTreeView
{
    public Project Project;
    public BehaviorEditor Editor;

    public BehaviorTreeView(Project curProject, BehaviorEditor editor)
    {
        Editor = editor;
        Project = curProject;
    }

    public void Draw()
    {
        var havokRoot = Project.BehaviorData.PrimaryBank.CurrentHavokRoot;

        if (havokRoot != null)
        {
            BuildCmsgList(havokRoot);
            //DrawObjectTree($"Root", havokRoot);

            for (int i = 0; i < CmsgEntries.Count; i++)
            {
                var curEntry = CmsgEntries[i];

                DrawObjectTree($"Root##root{i}", curEntry);
            }
        }
    }

    private List<object> CmsgEntries;

    private bool SetupCmsgList = false;

    public void BuildCmsgList(hkRootLevelContainer root)
    {
        if (SetupCmsgList)
            return;

        CmsgEntries = new();

        TraverseObjectTree(root, CmsgEntries, typeof(CustomManualSelectorGenerator));

        SetupCmsgList = true;
    }

    public void TraverseObjectTree(object? obj, List<object> entries, Type targetType, HashSet<object>? visited = null)
    {
        if (obj == null)
        {
            return;
        }

        visited ??= new HashSet<object>();
        if (!visited.Add(obj))
        {
            return;
        }

        Type type = obj.GetType();
        bool isLeaf = type.IsPrimitive || type == typeof(string) || type.IsEnum;

        if(obj.GetType() == targetType)
        {
            entries.Add(obj);
        }

        if (obj is IList list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                TraverseObjectTree(list[i], entries, targetType, visited);
            }
        }
        else
        {
            foreach (var prop in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                object? value = prop.GetValue(obj);
                TraverseObjectTree(value, entries, targetType, visited);
            }
        }

        visited.Remove(obj);
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