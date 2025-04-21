using HKLib.hk2018;
using HKLib.Serialization.hk2018.Binary;
using Smithbox.Core.Editor;
using Smithbox.Core.Utils;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.BehaviorEditorNS;

public class BehaviorBank
{
    private BehaviorData DataParent;

    public string BankName = "Undefined";

    public Dictionary<string, BND4> Binders = new();

    public BinderFile CurrentBinderFile;
    public hkRootLevelContainer CurrentHavokRoot;

    public BehaviorBank(BehaviorData parent, string bankName)
    {
        DataParent = parent;
        BankName = bankName;
    }

    public void LoadBehaviorFile(string binderName)
    {
        if(Binders.ContainsKey(binderName))
        {
            var binder = Binders[binderName];

            foreach(var file in binder.Files)
            {
                if(file.Name.Contains("Behaviors"))
                {
                    if(file.Name.Contains(binderName))
                    {
                        CurrentBinderFile = file;

                        HavokBinarySerializer serializer = new HavokBinarySerializer();
                        using (MemoryStream memoryStream = new MemoryStream(file.Bytes.ToArray()))
                        {
                            CurrentHavokRoot = (hkRootLevelContainer)serializer.Read(memoryStream);
                        }

                        DataParent.BuildCategories(CurrentHavokRoot);
                    }
                }
            }
        }
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
