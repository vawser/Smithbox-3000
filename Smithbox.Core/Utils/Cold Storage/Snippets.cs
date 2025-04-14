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
}
