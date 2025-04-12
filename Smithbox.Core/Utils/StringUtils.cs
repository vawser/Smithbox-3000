using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.Utils;

public class StringUtils
{
    public static string FlagsEnumToString<T>(T value) where T : struct, Enum
    {
        var vals = Enum.GetValues<T>();
        var names = Enum.GetNames<T>();
        List<string> has = [];
        foreach (var (val, name) in vals.Zip(names))
        {
            if (value.HasFlag(val)) has.Add(name);
        }

        return string.Join(" | ", has);
    }
}
