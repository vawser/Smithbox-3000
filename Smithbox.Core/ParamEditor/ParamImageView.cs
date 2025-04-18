using Hexa.NET.ImGui;
using Smithbox.Core.Editor;
using Smithbox.Core.ParamEditorNS.Meta;
using Smithbox.Core.Utils;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Andre.Formats.Param;

namespace Smithbox.Core.ParamEditorNS;

public class ParamImageView
{
    public Project Project;
    public ParamEditor Editor;

    private int ID;

    // Defined here so we can remove NoMove when setting up the imgui.ini
    private ImGuiWindowFlags MainWindowFlags = ImGuiWindowFlags.MenuBar; //| ImGuiWindowFlags.NoMove;
    private ImGuiWindowFlags SubWindowFlags = ImGuiWindowFlags.NoMove;

    public bool DetectShortcuts = false;

    public ParamImageView(Project curProject, ParamEditor editor)
    {
        ID = editor.ID;
        Project = curProject;
        Editor = editor;
    }

    public void Draw()
    {
        if (CFG.Current.DisplayParamImageView)
        {
            if (TargetRow == null)
                return;

            if (DisplayIcon)
            {
                ImGui.Begin($"Image Preview##ImagePreview{ID}", MainWindowFlags);

                if (ImGui.IsWindowFocused())
                {
                    DetectShortcuts = true;
                }

                foreach(var entry in CurrentTextureReferences)
                {
                    var curColumn = entry.Key;
                    var curFieldMeta = entry.Value.FieldMeta;

                    var displayName = TargetRow[curColumn].Def.InternalName;
                    if (CFG.Current.DisplayCommunityFieldNames)
                    {
                        displayName = curFieldMeta.AltName;
                    }

                    var iconValue = TargetRow[curColumn].Value;
                    ImGui.Text($"{displayName}: {iconValue}");
                }

                ImGui.End();
            }
        }
    }

    private Row TargetRow;
    private bool DisplayIcon = false;

    private Dictionary<Column, TextureReference> CurrentTextureReferences;

    public void UpdateIconPreview(Row curRow)
    {
        var curParam = Editor.Selection._selectedParam;

        ParamMeta paramMeta = null;

        TargetRow = curRow;
        DisplayIcon = false;

        CurrentTextureReferences = new();

        if (curParam.AppliedParamdef != null)
        {
            paramMeta = Project.ParamData.GetParamMeta(curParam.AppliedParamdef);
        }

        // Go over each field, and if there is a field meta that points to TextureRef,
        // store the meta and the column for display in the window
        foreach (var curColumn in curRow.Columns)
        {
            if (paramMeta != null)
            {
                var tMeta = paramMeta.GetField(curColumn.Def);

                if (tMeta != null)
                {
                    if (tMeta.TextureRef != null)
                    {
                        DisplayIcon = true;
                        var newTextureReference = new TextureReference();

                        newTextureReference.FieldMeta = tMeta;
                        newTextureReference.TargetField = tMeta.TextureRef.TargetField;
                        newTextureReference.SubTexturePrefix = tMeta.TextureRef.SubTexturePrefix;
                        newTextureReference.TextureFilePaths = tMeta.TextureRef.TextureFileNames;

                        newTextureReference.Textures = new();

                        foreach (var entry in tMeta.TextureRef.TextureFileNames)
                        {
                            var textureData = Project.FS.GetFile(entry).GetData().ToArray();
                            newTextureReference.Textures.Add(textureData);
                        }

                        CurrentTextureReferences.Add(curColumn, newTextureReference);
                    }
                }
            }
        }
    }
}

public class TextureReference
{
    public string TargetField { get; set; }
    public string SubTexturePrefix { get; set; }
    public ParamFieldMeta FieldMeta { get; set; }

    public List<string> TextureFilePaths { get; set; }
    public List<byte[]> Textures { get; set; }
}