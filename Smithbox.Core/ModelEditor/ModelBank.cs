using Microsoft.Extensions.Logging;
using Smithbox.Core.Editor;
using Smithbox.Core.Utils;
using SoulsFormats;

namespace Smithbox.Core.ModelEditorNS;

public class ModelBank
{
    private ModelData DataParent;

    public string BankName = "Undefined";

    public Dictionary<string, BinderContents> Binders = new();
    public ModelBank(ModelData parent, string bankName)
    {
        DataParent = parent;
        BankName = bankName;
    }

    public void LoadBinder(string filename, string filepath)
    {
        if (!Binders.ContainsKey(filename))
        {
            if (DataParent.Project.ProjectType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.DES)
            {
                try
                {
                    // BXF
                    if (filepath.Contains("bhd"))
                    {
                        var bhdData = DataParent.Project.FS.ReadFileOrThrow(filepath);
                        var bdtData = DataParent.Project.FS.ReadFileOrThrow(filepath.Replace("bhd", "bdt"));
                        var curBinder = BXF3.Read(bhdData, bdtData);

                        var newBinderContents = new BinderContents();
                        newBinderContents.Name = filename;

                        var fileList = new List<BinderFile>();
                        foreach (var file in curBinder.Files)
                        {
                            fileList.Add(file);
                        }

                        newBinderContents.HeaderedBinder_OLD = curBinder;
                        newBinderContents.Files = fileList;

                        Binders.Add(filename, newBinderContents);
                    }

                    // BND
                    if (filepath.Contains("bnd"))
                    {
                        var binderData = DataParent.Project.FS.ReadFileOrThrow(filepath);
                        var curBinder = BND3.Read(binderData);

                        var newBinderContents = new BinderContents();
                        newBinderContents.Name = filename;

                        var fileList = new List<BinderFile>();
                        foreach (var file in curBinder.Files)
                        {
                            fileList.Add(file);
                        }

                        newBinderContents.Binder_OLD = curBinder;
                        newBinderContents.Files = fileList;

                        Binders.Add(filename, newBinderContents);
                    }

                }
                catch (Exception ex)
                {
                    TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Behavior Editor:{BankName}] Failed to load {filepath}", LogLevel.Warning);
                }
            }
            else
            {
                try
                {
                    // BXF
                    if (filepath.Contains("bhd"))
                    {
                        var bhdData = DataParent.Project.FS.ReadFileOrThrow(filepath);
                        var bdtData = DataParent.Project.FS.ReadFileOrThrow(filepath.Replace("bhd", "bdt"));
                        var curBinder = BXF4.Read(bhdData, bdtData);

                        var newBinderContents = new BinderContents();
                        newBinderContents.Name = filename;

                        var fileList = new List<BinderFile>();
                        foreach (var file in curBinder.Files)
                        {
                            fileList.Add(file);
                        }

                        newBinderContents.HeaderedBinder = curBinder;
                        newBinderContents.Files = fileList;

                        Binders.Add(filename, newBinderContents);
                    }

                    // BND
                    if (filepath.Contains("bnd"))
                    {
                        var binderData = DataParent.Project.FS.ReadFileOrThrow(filepath);
                        var curBinder = BND4.Read(binderData);

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

                }
                catch (Exception ex)
                {
                    TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Behavior Editor:{BankName}] Failed to load {filepath}", LogLevel.Warning);
                }
            }
        }
    }

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

public class BinderContents
{
    public string Name { get; set; }

    public BXF3 HeaderedBinder_OLD { get; set; }
    public BND3 Binder_OLD { get; set; }

    public BXF4 HeaderedBinder { get; set; }
    public BND4 Binder { get; set; }

    public List<BinderFile> Files { get; set; }
}