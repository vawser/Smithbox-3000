using Andre.IO.VFS;
using Smithbox.Core.Editor;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.Utils;

public class VfsUtils
{
    /// <summary>
    /// Gets the VirtualFileSystem we should use for writes.
    /// If no suitable VFS is found, throws InvalidOperationException.
    /// </summary>
    /// <returns></returns>
    public static VirtualFileSystem GetFilesystemForWrite(Project curProject)
    {
        if (curProject.ProjectFS is not EmptyVirtualFileSystem)
            return curProject.ProjectFS;

        if (curProject.VanillaRealFS is not EmptyVirtualFileSystem)
            return curProject.VanillaRealFS;

        throw new InvalidOperationException("No suitable VFS was found for writes");
    }

    public static void WriteWithBackup<T>(Project curProject, string assetPath, T item,
        params object[] writeparms) where T : SoulsFile<T>, new()
    {
        WriteWithBackup(curProject, curProject.FS, curProject.ProjectFS, assetPath, item,
            curProject.ProjectType, writeparms);
    }

    public static void WriteWithBackup<T>(Project curProject, VirtualFileSystem vanillaFs, VirtualFileSystem toFs, string assetPath,
        T item, ProjectType gameType = ProjectType.None, params object[] writeparms) where T : SoulsFile<T>, new()
    {
        try
        {
            // Make a backup of the original file if a mod path doesn't exist
            if (toFs != curProject.ProjectFS && !toFs.FileExists($"{assetPath}.bak") && toFs.FileExists(assetPath))
            {
                toFs.Copy(assetPath, $"{assetPath}.bak");
            }

            if (gameType == ProjectType.DS3 && item is BND4 bndDS3)
            {
                toFs.WriteFile(assetPath + ".temp", SFUtil.EncryptDS3Regulation(bndDS3));
            }
            else if (gameType == ProjectType.ER && item is BND4 bndER)
            {
                toFs.WriteFile(assetPath + ".temp", SFUtil.EncryptERRegulation(bndER));
            }
            else if (gameType == ProjectType.ERN && item is BND4 bndERN)
            {
                toFs.WriteFile(assetPath + ".temp", SFUtil.EncryptNightreignRegulation(bndERN));
            }
            else if (gameType == ProjectType.AC6 && item is BND4 bndAC6)
            {
                toFs.WriteFile(assetPath + ".temp", SFUtil.EncryptAC6Regulation(bndAC6));
            }
            else if (item is BXF3 or BXF4)
            {
                var bhdPath = $@"{(string)writeparms[0]}";
                if (item is BXF3 bxf3)
                {
                    bxf3.Write(out var bhd, out var bdt);
                    toFs.WriteFile(bhdPath + ".temp", bhd);
                    toFs.WriteFile(assetPath + ".temp", bdt);

                    // Ugly but until I rethink the binder API we need to dispose it before touching the existing files
                    bxf3.Dispose();
                }
                else if (item is BXF4 bxf4)
                {
                    bxf4.Write(out var bhd, out var bdt);
                    toFs.WriteFile(bhdPath + ".temp", bhd);
                    toFs.WriteFile(assetPath + ".temp", bdt);

                    // Ugly but until I rethink the binder API we need to dispose it before touching the existing files
                    bxf4.Dispose();
                }

                if (toFs.FileExists(bhdPath))
                {
                    toFs.Copy(bhdPath, bhdPath + ".prev");
                }
                toFs.Move(bhdPath + ".temp", bhdPath);

                return;
            }
            else
            {
                toFs.WriteFile(assetPath + ".temp", item.Write());
            }

            // Ugly but until I rethink the binder API we need to dispose it before touching the existing files
            if (item is IDisposable d)
            {
                d.Dispose();
            }

            if (toFs.FileExists(assetPath))
            {
                toFs.Copy(assetPath, assetPath + ".prev");
            }
            toFs.Move(assetPath + ".temp", assetPath);
        }
        catch (Exception e)
        {
            TaskLogs.AddLog($"[{curProject.ProjectName}] Failed to save: {Path.GetFileName(assetPath)} - {e}");
        }
    }
}
