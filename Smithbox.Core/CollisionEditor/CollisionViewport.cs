using Hexa.NET.ImGui;
using Smithbox.Core.Editor;
using Smithbox.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.CollisionEditorNS;

public class CollisionViewport
{
    public Project Project;
    public CollisionEditor Editor;

    public bool DetectShortcuts = false;

    public CollisionViewport(Project curProject, CollisionEditor editor)
    {
        Editor = editor;
        Project = curProject;
    }
    public void Draw()
    {
        DetectShortcuts = ShortcutUtils.UpdateShortcutDetection();

        // TODO
    }
}
