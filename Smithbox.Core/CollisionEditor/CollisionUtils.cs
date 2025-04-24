using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.CollisionEditorNS;

public class CollisionUtils
{
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
