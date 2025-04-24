using Microsoft.Extensions.Logging;
using Smithbox.Core.Editor;
using Smithbox.Core.Utils;
using SoulsFormats;

namespace Smithbox.Core.CollisionEditorNS;

public class CollisionBank
{
    private CollisionData DataParent;

    public string BankName = "Undefined";

    public Dictionary<string, BinderContents> Binders = new();

    public CollisionBank(CollisionData parent, string bankName)
    {
        DataParent = parent;
        BankName = bankName;
    }

    /// <summary>
    /// Load the external file
    /// </summary>
    public void LoadBinder(string filename, string filepath)
    {
        // Read binder if it hasn't already been loaded
        if (!Binders.ContainsKey(filename))
        {
            try
            {
                var bdtPath = filepath.Replace("hkxbhd", "hkxbdt");

                var bhdData = DataParent.Project.FS.ReadFileOrThrow(filepath);
                var bdtData = DataParent.Project.FS.ReadFileOrThrow(bdtPath);

                var curBinder = BXF4.Read(bhdData, bdtData);

                var newBinderContents = new BinderContents();
                newBinderContents.Name = filename;

                var fileList = new List<BinderFile>();
                foreach (var file in curBinder.Files)
                {
                    fileList.Add(file);
                }

                newBinderContents.Binder = curBinder;
                newBinderContents.Files = fileList;

                Binders.Add(filename, newBinderContents);
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Collision Editor:{BankName}] Failed to load {filepath}", LogLevel.Warning);
            }
        }
    }

    private string SearchFolder = @"";

    /// <summary>
    /// Load the internal file
    /// </summary>
    public void LoadInternalFile()
    {
        var selection = DataParent.Project.CollisionEditor.Selection;

        if (!Binders.ContainsKey(selection._selectedFileName))
            return;

        var binder = Binders[selection._selectedFileName];

        var internalFile = binder.Files.Where(e => e.Name == selection._selectedInternalFileName).FirstOrDefault();

        if (internalFile == null)
            return;

        var compendiumFile = binder.Files.Where(e => e.Name.Contains(".compendium")).FirstOrDefault();

        if (compendiumFile == null)
            return;

        selection.SelectHavokRoot(compendiumFile, internalFile);
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
        var selection = DataParent.Project.CollisionEditor.Selection;

        if (!Binders.ContainsKey(selection._selectedFileName))
            return false;

        var binder = Binders[selection._selectedFileName];

        var internalFile = binder.Files.Where(e => e.Name == selection._selectedInternalFileName).FirstOrDefault();

        if (internalFile == null)
            return false;

        using (MemoryStream memoryStream = new MemoryStream(internalFile.Bytes.ToArray()))
        {
            selection._selectedSerializer.Write(selection._selectedHavokRoot, memoryStream);
            internalFile.Bytes = memoryStream.ToArray();
        }

        return true;
    }
}

public class BinderContents
{
    public string Name { get; set; }
    public BXF4 Binder { get; set; }
    public List<BinderFile> Files { get; set; }
}