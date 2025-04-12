using Smithbox.Core.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.Editor;

public class ActionManager
{
    private readonly List<IActionEventHandler> _eventHandlers = new();
    private readonly Stack<AtomicAction> RedoStack = new();

    private readonly Stack<AtomicAction> UndoStack = new();

    public void AddEventHandler(IActionEventHandler handler)
    {
        _eventHandlers.Add(handler);
    }

    private void NotifyHandlers(ActionEvent evt)
    {
        if (evt == ActionEvent.NoEvent)
        {
            return;
        }

        foreach (IActionEventHandler handler in _eventHandlers)
        {
            handler.OnActionEvent(evt);
        }
    }

    public void ExecuteAction(AtomicAction a)
    {
        NotifyHandlers(a.Execute());
        UndoStack.Push(a);
        RedoStack.Clear();
        //UICache.ClearCaches();
    }

    public void PushSubManager(ActionManager child)
    {
        List<AtomicAction> childList = child.UndoStack.ToList();
        childList.Reverse();
        UndoStack.Push(new CompoundAction(childList));
        RedoStack.Clear();
        //UICache.ClearCaches();
    }

    public AtomicAction PeekUndoAction()
    {
        if (UndoStack.Count() == 0)
        {
            return null;
        }

        return UndoStack.Peek();
    }

    public void UndoAction()
    {
        if (UndoStack.Count() == 0)
        {
            return;
        }

        AtomicAction a = UndoStack.Pop();
        NotifyHandlers(a.Undo());
        RedoStack.Push(a);
        //UICache.ClearCaches();
    }

    public void UndoAllAction()
    {
        if (UndoStack.Count() == 0)
        {
            return;
        }

        while (UndoStack.Count() > 0)
        {
            AtomicAction a = UndoStack.Pop();
            NotifyHandlers(a.Undo());
            RedoStack.Push(a);
            //UICache.ClearCaches();
        }
    }

    public void RedoAction()
    {
        if (RedoStack.Count() == 0)
        {
            return;
        }

        AtomicAction a = RedoStack.Pop();
        NotifyHandlers(a.Execute());
        UndoStack.Push(a);
        //UICache.ClearCaches();
    }

    public bool CanUndo()
    {
        return UndoStack.Count() > 0;
    }

    public bool CanRedo()
    {
        return RedoStack.Count() > 0;
    }

    public void Clear()
    {
        UndoStack.Clear();
        RedoStack.Clear();
    }
}

[Flags]
public enum ActionEvent
{
    NoEvent = 0,

    // An object was added or removed from a scene
    ObjectAddedRemoved = 1
}

/// <summary>
/// Interface for objects that may react to events caused by actions that
/// happen. Useful for invalidating caches that various editors may have.
/// </summary>
public interface IActionEventHandler
{
    public void OnActionEvent(ActionEvent evt);
}