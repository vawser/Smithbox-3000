using Hexa.NET.ImGui;
using Smithbox.Core.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.ModelEditorNS;

public class ModelInternalFileList
{
    public Project Project;
    public ModelEditor Editor;

    public ModelInternalFileList(Project curProject, ModelEditor editor)
    {
        Project = curProject;
        Editor = editor;
    }

    public void Draw()
    {
        if (Editor.Selection._selectedFileName == "")
            return;

        if (!Project.ModelData.PrimaryBank.Binders.ContainsKey(Editor.Selection._selectedFileName))
            return;

        var binderFile = Project.ModelData.PrimaryBank.Binders[Editor.Selection._selectedFileName];

        for(int i = 0; i < binderFile.Files.Count; i++)
        {
            var curFile = binderFile.Files[i];

            if(curFile.Name.EndsWith(".flv") || curFile.Name.EndsWith(".flver"))
            {
                var isSelected = Editor.Selection.IsInternalFileSelected(i, curFile.Name);

                var displayName = $@"{Path.GetFileNameWithoutExtension(curFile.Name)}";

                if (ImGui.Selectable($"{displayName}##internalFileEntry{i}", isSelected))
                {
                    Editor.Selection.SelectInternalFile(i, curFile.Name, curFile);
                }
            }
        }
    }
}
