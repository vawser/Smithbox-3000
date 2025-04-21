using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.Utils;

public static class StructExtensions
{
    public static Vector3 XYZ(this Vector4 v)
    {
        return new Vector3(v.X, v.Y, v.Z);
    }

    /// <summary>
    ///     A generic extension method that aids in reflecting
    ///     and retrieving any attribute that is applied to an `Enum`.
    /// </summary>
    public static TAttribute GetAttribute<TAttribute>(this Enum enumValue)
        where TAttribute : Attribute
    {
        return enumValue.GetType()
            .GetMember(enumValue.ToString())
            .First()
            .GetCustomAttribute<TAttribute>();
    }

    public static string GetDisplayName(this Enum enumValue)
    {
        var attribute = enumValue.GetAttribute<DisplayAttribute>();
        if (attribute != null && attribute.Name != null)
        {
            return attribute.Name;
        }

        return $"{enumValue}";
    }
    public static string GetShortName(this Enum enumValue)
    {
        var attribute = enumValue.GetAttribute<DisplayAttribute>();
        if (attribute != null && attribute.ShortName != null)
        {
            return attribute.ShortName;
        }

        return $"{enumValue}";
    }
    public static string GetDescription(this Enum enumValue)
    {
        var attribute = enumValue.GetAttribute<DisplayAttribute>();
        if (attribute != null && attribute.Description != null)
        {
            return attribute.Description;
        }

        return $"{enumValue}";
    }
}
