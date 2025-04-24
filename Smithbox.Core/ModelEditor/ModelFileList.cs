using Hexa.NET.ImGui;
using Smithbox.Core.Editor;
using Smithbox.Core.Interface;
using Smithbox.Core.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.ModelEditorNS;

public class ModelFileList
{
    public Project Project;
    public ModelEditor Editor;

    public ModelFileList(Project curProject, ModelEditor editor)
    {
        Project = curProject;
        Editor = editor;
    }

    public void Draw()
    {
        DisplayCategory("Characters", Project.ModelData.CharacterPartFiles.Entries);
        DisplayCategory("Objects", Project.ModelData.ObjectPartFiles.Entries);
        DisplayCategory("Equipment", Project.ModelData.EquipPartFiles.Entries);
        DisplayCategory("Map Parts", Project.ModelData.MapPartFiles.Entries);
    }
    public void DisplayCategory(string categoryName, List<FileDictionaryEntry> entries)
    {
        if (entries.Count > 0)
        {
            if (ImGui.CollapsingHeader(categoryName))
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

                        if (ImGui.Selectable($"{displayName}##fileEntry{categoryName}{i}", isSelected))
                        {
                            Editor.Selection.SelectFile(i, curEntry.Filename);

                            Project.ModelData.PrimaryBank.LoadBinder(curEntry.Filename, curEntry.Path);
                        }

                        // Characters
                        if (Project.Aliases.Characters.Any(e => e.ID.ToLower() == curEntry.Filename.ToLower()))
                        {
                            var nameEntry = Project.Aliases.Characters.Where(e => e.ID.ToLower() == curEntry.Filename.ToLower()).FirstOrDefault();

                            if (nameEntry != null)
                            {
                                UIHelper.DisplayAlias(nameEntry.Name);
                            }
                        }

                        // Objects
                        if (Project.Aliases.Assets.Any(e => e.ID.ToLower() == curEntry.Filename.ToLower()))
                        {
                            var nameEntry = Project.Aliases.Assets.Where(e => e.ID.ToLower() == curEntry.Filename.ToLower()).FirstOrDefault();

                            if (nameEntry != null)
                            {
                                UIHelper.DisplayAlias(nameEntry.Name);
                            }
                        }

                        // Parts
                        if (Project.Aliases.Parts.Any(e => e.ID.ToLower() == curEntry.Filename.ToLower()))
                        {
                            var nameEntry = Project.Aliases.Parts.Where(e => e.ID.ToLower() == curEntry.Filename.ToLower()).FirstOrDefault();

                            if (nameEntry != null)
                            {
                                UIHelper.DisplayAlias(nameEntry.Name);
                            }
                        }

                        // Map Parts
                        if (Project.Aliases.MapPieces.Any(e => e.ID.ToLower() == curEntry.Filename.ToLower()))
                        {
                            var nameEntry = Project.Aliases.MapPieces.Where(e => e.ID.ToLower() == curEntry.Filename.ToLower()).FirstOrDefault();

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
}
