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

                if (TargetColumns != null)
                {
                    for(int i = 0; i < TargetColumns.Count; i++)
                    {
                        var curColumn = TargetColumns[i];
                        var curFieldMeta = CurrentFieldMetas[i];

                        var displayName = TargetRow[curColumn].Def.InternalName;
                        if (CFG.Current.DisplayCommunityFieldNames)
                        {
                            displayName = curFieldMeta.AltName;
                        }

                        var iconValue = TargetRow[curColumn].Value;
                        ImGui.Text($"{displayName}: {iconValue}");
                    }
                }

                ImGui.End();
            }
        }
    }

    private Row TargetRow;
    private bool DisplayIcon = false;

    private List<ParamFieldMeta> CurrentFieldMetas;
    private List<Column> TargetColumns;
    private List<byte[]> TargetTextureDatas;

    public void UpdateIconPreview(Row curRow)
    {
        var curParam = Editor.Selection._selectedParam;

        ParamMeta paramMeta = null;

        TargetRow = curRow;
        DisplayIcon = false;

        TargetColumns = new();
        CurrentFieldMetas = new();

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
                var tempFieldMeta = paramMeta.GetField(curColumn.Def);

                if (tempFieldMeta != null)
                {
                    if (tempFieldMeta.TextureRef != null)
                    {
                        DisplayIcon = true;
                        TargetColumns.Add(curColumn);
                        CurrentFieldMetas.Add(tempFieldMeta);

                        // Build ImGui.Texture here, and then store it, then in Draw() render it

                        //var textureRef = tempFieldMeta.TextureRef;
                        //var textureData = Project.FS.GetFile(textureRef.TextureFile).GetData().ToArray();
                    }
                }
            }
        }
    }
}
