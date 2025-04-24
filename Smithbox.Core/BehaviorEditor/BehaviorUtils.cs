﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.BehaviorEditorNS;

public class BehaviorUtils
{
    /// <summary>
    /// Get the 'title' for the internal file entries, since the normal filename isn't descriptive
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetInternalFileTitle(string path)
    {
        var filename = Path.GetFileNameWithoutExtension(path);
        var name = $"Information: {filename}";

        if(path.Contains("Character"))
        {
            name = $"Character: {filename}";
        }

        if (path.Contains("Behavior"))
        {
            name = $"Behavior: {filename}";
        }

        return name;
    }

    /// <summary>
    /// Determines the category type for the internal file
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static HavokCategoryType GetInternalFileCategoryType(string path)
    {
        if (path.Contains("Character"))
        {
            return HavokCategoryType.Character;
        }

        if (path.Contains("Behavior"))
        {
            return HavokCategoryType.Behavior;
        }

        return HavokCategoryType.Information;
    }

    public static string GetObjectFieldValue(object curEntry, string fieldName)
    {
        var objType = curEntry.GetType();
        var strValue = $"{objType}";
        FieldInfo nameField = objType.GetField(fieldName);
        if (nameField != null)
        {
            strValue = $"{(string)nameField.GetValue(curEntry)}";
        }

        return strValue;
    }
}

/// <summary>
/// Determines which categories to use.
/// </summary>
public enum HavokCategoryType
{
    None,
    Information,
    Character,
    Behavior
}
