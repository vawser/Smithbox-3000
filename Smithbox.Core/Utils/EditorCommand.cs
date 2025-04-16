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
    private static readonly ConcurrentQueue<Command> QueuedCommands = new();

    public static void AddCommand(Command cmd)
    {
        QueuedCommands.Enqueue(cmd);
    }

    public static void AddCommand(IEnumerable<Command> cmd)
    {
        foreach (var c in cmd)
        {
            QueuedCommands.Enqueue(c);
        }
    }

    public static Command GetNextCommand()
    {
        QueuedCommands.TryDequeue(out var cmd);
        return cmd;
    }
}

public class Command
{
    public EditorTarget Editor { get; set; }
    public string[] Instructions { get; set; }
}

public enum EditorTarget
{
    ParamEditor,
    ModelEditor
}

