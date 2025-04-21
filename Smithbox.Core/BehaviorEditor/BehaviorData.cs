using Andre.IO.VFS;
using Microsoft.Extensions.Logging;
using Smithbox.Core.Editor;
using Smithbox.Core.FileBrowserNS.Entries;
using Smithbox.Core.ParamEditorNS;
using Smithbox.Core.Resources;
using Smithbox.Core.Utils;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.ComponentModel.Design.ObjectSelectorEditor;

namespace Smithbox.Core.BehaviorEditorNS;

public class BehaviorData
{
    public Project Project;

    public BehaviorBank PrimaryBank;

    public FileDictionary BehaviorFiles;

    public BehaviorData(Project projectOwner)
    {
        Project = projectOwner;

        PrimaryBank = new(this, "Primary");

        BehaviorFiles = new();
        BehaviorFiles.Entries = Project.FileDictionary.Entries.Where(e => e.Extension == "behbnd").ToList();
    }

    /// <summary>
    /// Loads the binder contain the hkx files if it hasn't already
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="filepath"></param>
    /// <param name="targetBank"></param>
    public void LoadBinder(string filename, string filepath, BehaviorBank targetBank)
    {
        if (!PrimaryBank.Binders.ContainsKey(filename))
        {
            try
            {
                var binderData = Project.FS.ReadFileOrThrow(filepath);
                var curBinder = BND4.Read(binderData);

                PrimaryBank.Binders.Add(filename, curBinder);
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Behavior Editor:{targetBank.BankName}] Failed to load {filepath}", LogLevel.Warning);
            }
        }

        PrimaryBank.LoadBehaviorFile(filename);
    }
}
