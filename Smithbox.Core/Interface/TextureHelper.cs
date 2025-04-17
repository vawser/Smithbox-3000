using Hexa.NET.ImGui.Backends.D3D11;
using Silk.NET.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.DDS;

namespace Smithbox.Core.Interface;

public class TextureHelper
{
    private static byte[] GenerateDummyTexture(int width, int height)
    {
        byte[] data = new byte[width * height * 4];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int i = (y * width + x) * 4;
                data[i + 0] = (byte)(x % 256); // R
                data[i + 1] = (byte)(y % 256); // G
                data[i + 2] = 0;               // B
                data[i + 3] = 255;             // A
            }
        return data;
    }
}
