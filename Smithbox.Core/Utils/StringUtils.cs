using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
    public static unsafe byte* StringToUtf8(string str)
    {
        if (str == null)
            return null;

        // Encode to UTF-8 with null terminator
        byte[] utf8Bytes = Encoding.UTF8.GetBytes(str + '\0');

        // Allocate unmanaged memory
        IntPtr unmanagedPtr = Marshal.AllocHGlobal(utf8Bytes.Length);

        // Copy bytes to unmanaged memory
        Marshal.Copy(utf8Bytes, 0, unmanagedPtr, utf8Bytes.Length);

        return (byte*)unmanagedPtr.ToPointer();
    }

    public static unsafe void FreeUtf8(byte* ptr)
    {
        if (ptr != null)
        {
            Marshal.FreeHGlobal((IntPtr)ptr);
        }
    }
    public static string TruncateWithEllipsis(string input, int maxLength = 80)
    {
        if (string.IsNullOrEmpty(input) || input.Length <= maxLength)
            return input;

        int cutoff = input.LastIndexOf(' ', maxLength);
        if (cutoff <= 0)
            cutoff = maxLength;

        return input.Substring(0, cutoff).TrimEnd() + "...";
    }
}
