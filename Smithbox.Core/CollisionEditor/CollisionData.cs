using HKLib.hk2018;
using Smithbox.Core.BehaviorEditorNS;
using Smithbox.Core.Editor;
using Smithbox.Core.Resources;
using Smithbox.Core.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.CollisionEditorNS;

public class CollisionData
{
    public Project Project;

    public CollisionBank PrimaryBank;

    public FileDictionary CollisionFiles;

    public CollisionData(Project projectOwner)
    {
        Project = projectOwner;

        PrimaryBank = new(this, "Primary");

        CollisionFiles = new();
        CollisionFiles.Entries = Project.FileDictionary.Entries.Where(e => e.Extension == "hkxbhd").ToList();
    }

    // Categories
    private Dictionary<string, Type> HavokCategories_Collision_ER = new Dictionary<string, Type>()
    {

    }; 
    
    public Dictionary<string, List<object>> Categories = new();

    // For getting the type list
    private List<Type> visitedTypes = new();

    public void BuildCategories(hkRootLevelContainer root)
    {
        Categories.Clear();

        var categoryDict = new Dictionary<string, Type>();

        categoryDict = HavokCategories_Collision_ER;

        foreach (var entry in categoryDict)
        {
            var category = entry.Key;
            var havokType = entry.Value;

            var newCategory = new List<object>();
            TraverseObjectTree(root, newCategory, havokType);
            Categories.Add(category, newCategory);
        }

        foreach (var entry in visitedTypes)
        {
            TaskLogs.AddLog($"{entry}");
        }
    }

    private void TraverseObjectTree(object? obj, List<object> entries, Type targetType, HashSet<object>? visited = null)
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

        if (!visitedTypes.Contains(type))
        {
            visitedTypes.Add(type);
        }

        if (obj.GetType() == targetType)
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
}
