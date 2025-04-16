using Andre.Formats;
using Smithbox.Core.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Andre.Formats.Param;

namespace Smithbox.Core.ParamEditorNS;

public class ParamFieldDecorator
{
    private Project Project;
    private ParamEditor Editor;

    public ParamFieldDecorator(Project curProject, ParamEditor editor)
    {
        Project = curProject;
        Editor = editor;
    }

    public void DisplayFieldInfo(string imguiID, Param curParam, Row curRow, Column curField, object curValue, ParamFieldMeta fieldMeta, bool isReadOnly = false)
    {
        // ParamRef

        // ParamTextRef

        // ParamfileRef

        // ParamTextureRef

        // ParamEnum

        // Project Enum

        // ParamCalcCorrectDef

        // ParamSoulCostDef

        // Virtual Ref

        // Alias: Particle

        // Alias: Sound

        // Alias: Event Flag

        // Alias: Cutscene

        // Alias: Movie
    }
}
