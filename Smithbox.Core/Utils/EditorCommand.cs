using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.Utils;

/// <summary>
/// Textual commands used to invoke functionality within editors indirectly
/// </summary>
public static class EditorCommand
{
    private static readonly ConcurrentQueue<string[]> QueuedCommands = new();

    public static void AddCommand(string cmd)
    {
        QueuedCommands.Enqueue(cmd.Split(":"));
    }

    public static void AddCommand(IEnumerable<string> cmd)
    {
        QueuedCommands.Enqueue(cmd.ToArray());
    }

    public static string[] GetNextCommand()
    {
        QueuedCommands.TryDequeue(out var cmd);
        return cmd;
    }
}
