using Hexa.NET.ImGui;
using Smithbox.Core.Editor;
using Smithbox.Core.Interface;
using Smithbox.Core.ModelEditorNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.MapEditorNS;

public class MapFileList
{
    public Project Project;
    public MapEditor Editor;

    public MapFileList(Project curProject, MapEditor editor)
    {
        Project = curProject;
        Editor = editor;
    }

    public void Draw()
    {
        var entries = Project.MapData.MapFiles.Entries;

        if (entries.Count > 0)
        {
            ImGuiListClipper clipper = new ImGuiListClipper();
            clipper.Begin(entries.Count);

            while (clipper.Step())
            {
                for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    var curEntry = entries[i];

                    var isSelected = Editor.Selection.IsFileSelected(i, curEntry.Filename);

                    var displayName = $"{curEntry.Filename}";

                    if (ImGui.Selectable($"{displayName}##fileEntry{i}", isSelected))
                    {
                        Editor.Selection.SelectFile(i, curEntry.Filename);

                        Project.MapData.PrimaryBank.LoadMap(curEntry.Filename, curEntry.Path);
                    }

                    if (Project.Aliases.MapNames.Any(e => e.ID.ToLower() == curEntry.Filename.ToLower()))
                    {
                        var nameEntry = Project.Aliases.MapNames.Where(e => e.ID.ToLower() == curEntry.Filename.ToLower()).FirstOrDefault();

                        if (nameEntry != null)
                        {
                            UIHelper.DisplayAlias(nameEntry.Name);
                        }
                    }
                }
            }
        }
    }
}
