using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.ParamEditorNS.Meta;

public class ParamTextureRef
{
    /// <summary>
    /// The lookup process to use.
    /// </summary>
    public string LookupType = "";

    /// <summary>
    /// The name of the texture container.
    /// </summary>
    public string TextureContainer = "";

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
        var refSplit = refString.Split('/');

        LookupType = refSplit[0];

        if (refSplit.Length > 1)
        {
            TextureContainer = refSplit[1];
        }
        if (refSplit.Length > 2)
        {
            TextureFile = refSplit[2];
        }
        if (refSplit.Length > 3)
        {
            TargetField = refSplit[3];
        }

        if (LookupType == "Direct")
        {
            if (refSplit.Length > 4)
            {
                SubTexturePrefix = refSplit[4];
            }
        }
    }

    internal string getStringForm()
    {
        return TextureFile;
    }
}