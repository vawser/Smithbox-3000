using Smithbox.Core.Editor;
using Smithbox.Core.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Smithbox.Core.Utils.Cold_Storage;

internal class Snippets
{
    //private void RegenerateRowNames(string folder, string outputName)
    //{
    //    if (Project.ParamData.Initialized)
    //    {
    //        // Import old row names
    //        var oldRowNameFolder = $@"{AppContext.BaseDirectory}\Assets\PARAM\{LocatorUtils.GetGameDirectory(Project)}\{folder}";

    //        if (!Directory.Exists(oldRowNameFolder))
    //            return;

    //        var files = Directory.EnumerateFiles(oldRowNameFolder).ToList().ToDictionary(e => Path.GetFileNameWithoutExtension(e));

    //        var primaryBank = Project.ParamData.PrimaryBank;
    //        foreach (var entry in primaryBank.Params)
    //        {
    //            if (files.ContainsKey(entry.Key))
    //            {
    //                var nameFilePath = files[entry.Key];

    //                var dictionary = new Dictionary<int, string>();

    //                foreach (var line in File.ReadLines(nameFilePath))
    //                {
    //                    int firstSpaceIndex = line.IndexOf(' ');
    //                    if (firstSpaceIndex > 0)
    //                    {
    //                        string keyPart = line.Substring(0, firstSpaceIndex);
    //                        string valuePart = line.Substring(firstSpaceIndex + 1);

    //                        if (int.TryParse(keyPart, out int key))
    //                        {
    //                            dictionary[key] = valuePart;
    //                        }
    //                    }
    //                }

    //                foreach (var row in entry.Value.Rows)
    //                {
    //                    if (dictionary.ContainsKey(row.ID))
    //                    {
    //                        row.Name = dictionary[row.ID];
    //                    }
    //                }
    //            }
    //        }

    //        var rowNameStore = new RowNameStore();
    //        rowNameStore.Params = new();

    //        // Export new json names
    //        foreach (var entry in primaryBank.Params)
    //        {
    //            var curRowNameParam = new RowNameParam();
    //            curRowNameParam.Name = entry.Key;
    //            curRowNameParam.Entries = new();

    //            for (int i = 0; i < entry.Value.Rows.Count; i++)
    //            {
    //                var row = entry.Value.Rows[i];

    //                var curRowNameEntry = new RowNameEntry();
    //                curRowNameEntry.Index = i;
    //                curRowNameEntry.Name = row.Name;
    //                curRowNameEntry.ID = row.ID;

    //                curRowNameParam.Entries.Add(curRowNameEntry);
    //            }

    //            rowNameStore.Params.Add(curRowNameParam);
    //        }

    //        var path = @$"{AppContext.BaseDirectory}\.smithbox\{outputName}.json";

    //        var json = JsonSerializer.Serialize(rowNameStore, SmithboxSerializerContext.Default.RowNameStore);

    //        File.WriteAllText(path, json);
    //    }
    //}

    //private void DisplayEditor()
    //{
    //    ImGui.Begin($"Pad##pad{ID}");

    //    if (ImGui.Button("Convert"))
    //    {
    //        AliasPerGame(ProjectType.AC6);
    //        AliasPerGame(ProjectType.ER);
    //        AliasPerGame(ProjectType.SDT);
    //        AliasPerGame(ProjectType.DS3);
    //        AliasPerGame(ProjectType.BB);
    //        AliasPerGame(ProjectType.DS2S);
    //        AliasPerGame(ProjectType.DS2);
    //        AliasPerGame(ProjectType.DS1R);
    //        AliasPerGame(ProjectType.DS1);
    //        AliasPerGame(ProjectType.DES);
    //        AliasPerGame(ProjectType.ERN);
    //    }

    //    ImGui.End();
    //}

    //private void AliasPerGame(ProjectType projectType)
    //{
    //    var sourcePath = @$"{AppContext.BaseDirectory}\Assets\Aliases\";

    //    var store = new AliasStore();
    //    store.Assets = new();
    //    BuildAliasList(@$"{sourcePath}\Assets\{LocatorUtils.GetGameDirectory(projectType)}\Asset.json", store.Assets);

    //    store.Characters = new();
    //    BuildAliasList(@$"{sourcePath}\Characters\{LocatorUtils.GetGameDirectory(projectType)}\Character.json", store.Characters);

    //    store.Cutscenes = new();
    //    BuildAliasList(@$"{sourcePath}\Cutscenes\{LocatorUtils.GetGameDirectory(projectType)}\Cutscene.json", store.Cutscenes);

    //    store.EventFlags = new();
    //    BuildAliasList(@$"{sourcePath}\Flags\{LocatorUtils.GetGameDirectory(projectType)}\EventFlag.json", store.EventFlags);

    //    store.Gparams = new();
    //    BuildAliasList(@$"{sourcePath}\Gparams\{LocatorUtils.GetGameDirectory(projectType)}\Gparams.json", store.Gparams);

    //    store.MapPieces = new();
    //    BuildAliasList(@$"{sourcePath}\MapPieces\{LocatorUtils.GetGameDirectory(projectType)}\MapPiece.json", store.MapPieces);

    //    store.MapNames = new();
    //    BuildAliasList(@$"{sourcePath}\Maps\{LocatorUtils.GetGameDirectory(projectType)}\Maps.json", store.MapNames);

    //    store.Movies = new();
    //    BuildAliasList(@$"{sourcePath}\Movies\{LocatorUtils.GetGameDirectory(projectType)}\Movie.json", store.Movies);

    //    store.Particles = new();
    //    BuildAliasList(@$"{sourcePath}\Particles\{LocatorUtils.GetGameDirectory(projectType)}\Fxr.json", store.Particles);

    //    store.Parts = new();
    //    BuildAliasList(@$"{sourcePath}\Parts\{LocatorUtils.GetGameDirectory(projectType)}\Part.json", store.Parts);

    //    store.Sounds = new();
    //    BuildAliasList(@$"{sourcePath}\Sounds\{LocatorUtils.GetGameDirectory(projectType)}\Sound.json", store.Sounds);

    //    store.TalkScripts = new();
    //    BuildAliasList(@$"{sourcePath}\Talks\{LocatorUtils.GetGameDirectory(projectType)}\Talk.json", store.TalkScripts);

    //    store.TimeActs = new();
    //    BuildAliasList(@$"{sourcePath}\TimeActs\{LocatorUtils.GetGameDirectory(projectType)}\TimeActs.json", store.TimeActs);

    //    var outputPath = $@"C:\Users\benja\Programming\C#\Smithbox-3000\Smithbox.Core\Assets\Aliases\{LocatorUtils.GetGameDirectory(projectType)}";

    //    if (!Directory.Exists(outputPath))
    //    {
    //        Directory.CreateDirectory(outputPath);
    //    }

    //    outputPath = outputPath + @$"\Aliases.json";

    //    var json = JsonSerializer.Serialize(store, SmithboxSerializerContext.Default.AliasStore);

    //    File.WriteAllText(outputPath, json);
    //}

    //private void BuildAliasList(string sourcePath, List<AliasEntry> newList)
    //{
    //    if (!File.Exists(sourcePath))
    //        return;

    //    var fileData = File.ReadAllText(sourcePath);

    //    try
    //    {
    //        var source = JsonSerializer.Deserialize(fileData, SmithboxSerializerContext.Default.AliasResource);
    //        if (source != null)
    //        {
    //            foreach (var entry in source.list)
    //            {
    //                var newEntry = new AliasEntry();

    //                newEntry.ID = entry.id;
    //                newEntry.Name = entry.name;
    //                newEntry.Tags = entry.tags;

    //                newList.Add(newEntry);
    //            }
    //        }
    //        else
    //        {
    //            newList = new();
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        newList = new();
    //    }
    //}

    //private void GenerateFileDictionary(string filepath)
    //{
    //    var writePath = $"{AppContext.BaseDirectory}/{Path.GetFileName(filepath)}.json";

    //    var curDictionary = new FileDictionary();
    //    curDictionary.Entries = new();

    //    var file = File.ReadAllText(filepath);
    //    var contents = file.Split("\n");

    //    var currentArchive = "";

    //    foreach (var line in contents)
    //    {
    //        if (line == "" || line == " ")
    //            continue;

    //        if (line.StartsWith("#"))
    //        {
    //            currentArchive = line.Replace("#", "");
    //        }
    //        else
    //        {
    //            var newEntry = new FileDictionaryEntry();
    //            newEntry.Archive = currentArchive.Replace("\r", "");
    //            newEntry.Path = line.Replace("\r", "");

    //            if (newEntry.Path != "")
    //            {
    //                newEntry.Folder = Path.GetDirectoryName(newEntry.Path).Replace('\\', '/'); ;
    //                if (line.Contains(".dcx"))
    //                {
    //                    var extension = Path.GetExtension(Path.GetFileNameWithoutExtension(newEntry.Path));
    //                    var fileName = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(line));
    //                    newEntry.Filename = Path.GetFileName(fileName);

    //                    if (extension != "")
    //                        newEntry.Extension = Path.GetFileName(extension).Substring(1, extension.Length - 1);
    //                    else
    //                        newEntry.Extension = "";
    //                }
    //                else
    //                {
    //                    var extension = Path.GetExtension(newEntry.Path);
    //                    var fileName = Path.GetFileNameWithoutExtension(line);
    //                    newEntry.Filename = Path.GetFileName(fileName);

    //                    if (extension != "")
    //                        newEntry.Extension = Path.GetFileName(extension).Substring(1, extension.Length - 1);
    //                    else
    //                        newEntry.Extension = "";
    //                }

    //                curDictionary.Entries.Add(newEntry);
    //            }
    //        }
    //    }

    //    var json = JsonSerializer.Serialize(curDictionary, SmithboxSerializerContext.Default.FileDictionary);

    //    File.WriteAllText(writePath, json);
    //}

    //public void FileDirectoryPrint()
    //{
    //    if (ImGui.MenuItem("DEBUG"))
    //    {
    //        string rootDirectory = SelectedProject.DataPath;

    //        var entries = new List<FileDictionaryEntry>();
    //        var files = Directory.GetFiles(rootDirectory, "*", SearchOption.AllDirectories);

    //        foreach (var file in files)
    //        {
    //            string relativePath = Path.GetRelativePath(rootDirectory, file);
    //            string archive = rootDirectory;
    //            string folder = Path.GetDirectoryName(relativePath)?.Replace("\\", "/") ?? "";
    //            string fileName = Path.GetFileNameWithoutExtension(file);
    //            string extension = Path.GetExtension(file).TrimStart('.').ToLower();

    //            // Handle .dcx case
    //            if (extension == "dcx")
    //            {
    //                var baseName = Path.GetFileNameWithoutExtension(file); // strip .dcx
    //                string basePath = Path.Combine(Path.GetDirectoryName(file), baseName);
    //                extension = Path.GetExtension(basePath).TrimStart('.').ToLower();
    //                fileName = Path.GetFileNameWithoutExtension(basePath);
    //            }

    //            entries.Add(new FileDictionaryEntry
    //            {
    //                Archive = "None",
    //                Path = $@"/{relativePath.Replace("\\", "/")}",
    //                Folder = $@"/{folder}",
    //                Filename = $@"{fileName}",
    //                Extension = extension
    //            });
    //        }

    //        string json = JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true });
    //        File.WriteAllText(@$"{AppContext.BaseDirectory}\file_dictionary.json", json);

    //        Console.WriteLine("File dictionary written to file_dictionary.json");
    //    }
    //}
}
