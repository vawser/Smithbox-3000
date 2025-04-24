using HKLib.hk2018;
using HKLib.hk2018.hk;
using HKLib.Serialization.hk2018.Binary;
using HKLib.Serialization.hk2018.Binary.Util;
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

    public string CurrentBinderName;
    public HavokBinarySerializer Serializer;
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
            CurrentBinderName = binderName;
            var binder = Binders[binderName];

            foreach(var file in binder.Files)
            {
                if(file.Name.Contains("Behaviors"))
                {
                    if(file.Name.Contains(binderName))
                    {
                        CurrentBinderFile = file;
                        Serializer = new HavokBinarySerializer();
                        using (MemoryStream memoryStream = new MemoryStream(file.Bytes.ToArray()))
                        {
                            CurrentHavokRoot = (hkRootLevelContainer)Serializer.Read(memoryStream);
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
                return SaveBehavior_ER();
            case ProjectType.AC6:
            case ProjectType.ERN:
            default: break;
        }

        return successfulSave;
    }

    public bool SaveBehavior_ER()
    {
        if(Binders.ContainsKey(CurrentBinderName))
        {
            var curBinder = Binders[CurrentBinderName];

            foreach (var file in curBinder.Files)
            {
                if (file.Name.Contains("Behaviors"))
                {
                    if (file.Name.Contains(CurrentBinderName))
                    {
                        using (MemoryStream memoryStream = new MemoryStream(CurrentBinderFile.Bytes.ToArray()))
                        {
                            Serializer.Write(CurrentHavokRoot, memoryStream);
                            file.Bytes = memoryStream.ToArray();
                        }
                    }
                }
            }

            var binderData = curBinder.Write();

            var folder = @$"{DataParent.Project.ProjectPath}\chr\";
            if(!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var outputPath = @$"{folder}\{CurrentBinderName}.behbnd.dcx";

            File.WriteAllBytes(outputPath, binderData);
        }

        return true;
    }
}
