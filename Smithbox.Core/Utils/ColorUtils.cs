using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.Utils;

public class ColorUtils
{
    public static uint ColorFromVec4(Vector4 color)
    {
        byte r = (byte)(Math.Clamp(color.X, 0f, 1f) * 255);
        byte g = (byte)(Math.Clamp(color.Y, 0f, 1f) * 255);
        byte b = (byte)(Math.Clamp(color.Z, 0f, 1f) * 255);
        byte a = (byte)(Math.Clamp(color.W, 0f, 1f) * 255);

        return (uint)(a << 24 | b << 16 | g << 8 | r);
    }
}
