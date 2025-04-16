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
    public string TextureFile = "";

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

        TextureFile = refSplit[0];

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
        return TextureFile;
    }
}