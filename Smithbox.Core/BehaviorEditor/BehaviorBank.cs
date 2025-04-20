using Smithbox.Core.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.BehaviorEditorNS;

public class BehaviorBank
{
    private BehaviorData DataParent;

    private string BankName = "Undefined";

    public bool IsBankLoaded { get; private set; }

    public Dictionary<string, byte[]> BehaviorFiles = new();

    public BehaviorBank(BehaviorData parent, string bankName)
    {
        DataParent = parent;
        BankName = bankName;
    }

    /// <summary>
    /// Save task for this bank
    /// </summary>
    public async Task<bool> Save()
    {
        await Task.Delay(1000);

        var successfulSave = false;

        switch (DataParent.Project.ProjectType)
        {
            case ProjectType.DES:
            case ProjectType.DS1:
            case ProjectType.DS1R:
            case ProjectType.DS2:
            case ProjectType.DS2S:
            case ProjectType.DS3:
            case ProjectType.BB:
            case ProjectType.SDT:
            case ProjectType.ER:
            case ProjectType.AC6:
            case ProjectType.ERN:
            default: break;
        }

        return successfulSave;
    }
}
