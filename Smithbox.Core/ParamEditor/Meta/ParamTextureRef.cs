using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.ParamEditorNS.Meta;

/// <summary>
/// A texture reference: pointing to a texture file in the File Browser
/// </summary>
public class ParamTextureRef
{
    /// <summary>
    /// The name of the texture file within the texture container.
    /// </summary>
    public List<string> TextureFileNames = new();

    /// <summary>
    /// The param row field that the image index is taken from.
    /// </summary>
    public string TargetField = "";

    /// <summary>
    /// The initial part of the subtexture filename to match with.
    /// </summary>
    public string SubTexturePrefix = "";

    internal ParamTextureRef(ParamMeta curMeta, string refString)
    {
        var refSplit = refString.Split(' ');

        TextureFileNames = GeneratePaths(refSplit[0]);

        if (refSplit.Length > 1)
        {
            TargetField = refSplit[1];
        }
        if (refSplit.Length > 2)
        {
            SubTexturePrefix = refSplit[2];
        }
    }

    internal string getStringForm()
    {
        return TextureFileNames.First();
    }

    internal List<string> GeneratePaths(string input)
    {
        var result = new List<string>();

        // Split the input into base and suffixes
        int baseEnd = input.IndexOf('|');
        if (baseEnd == -1)
            return result; // Invalid input format

        string basePath = input.Substring(0, baseEnd);
        string[] suffixes = input.Substring(baseEnd + 1).Split('|', StringSplitOptions.RemoveEmptyEntries);

        foreach (string suffix in suffixes)
        {
            result.Add($"{basePath}_{suffix}");
        }

        return result;
    }
}