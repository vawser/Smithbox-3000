using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.ParamEditorNS;

public class ParamActions
{
    private ParamEditor Editor;

    public ParamActions(ParamEditor curEditor)
    {
        Editor = curEditor;
    }

    public void Draw()
    {

    }

    public void DuplicateRow()
    {

    }

    public void DeleteRow()
    {

    }

    public void CopyRow()
    {
        /*
        if (_activeView._selection.RowSelectionExists())
        {
            CopySelectionToClipboard();
        }
        */
    }

    public void PasteRow()
    {
        /*
        if (ParamBank.ClipboardRows.Any())
        {
            EditorCommandQueue.AddCommand(@"param/menu/ctrlVPopup");
        }
        */
    }

    public void GoToRow()
    {
        /*
        if (_activeView._selection.RowSelectionExists())
        {
            GotoSelectedRow = true;
        }
        */
    }

    public bool IsCommutativeParam()
    {
        return true;
    }

    public void DisplayCommutativeDuplicateMenu()
    {

    }

    public void ImportDefaultParamRowNames(ParamBank targetBank)
    {

    }
}
