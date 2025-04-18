﻿using Smithbox.Core.Editor;
using Smithbox.Core.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Andre.Formats.Param;

namespace Smithbox.Core.Actions;

public class ChangeAliasField : AtomicAction
{
    private readonly AliasEntry AliasEntry;
    private readonly object NewValue;
    private readonly object OldValue;
    private readonly AliasField ChangeType;
    private readonly int TagIndex;

    public ChangeAliasField(AliasEntry curEntry, object oldValue, object newValue, AliasField changeType, int tagIndex = -1)
    {
        AliasEntry = curEntry;
        NewValue = newValue;
        OldValue = oldValue;
        ChangeType = changeType;
        TagIndex = tagIndex;
    }

    public override ActionEvent Execute()
    {
        switch(ChangeType)
        {
            case AliasField.ID:
                AliasEntry.ID = $"{NewValue}";
                break;
            case AliasField.Name:
                AliasEntry.Name = $"{NewValue}";
                break;
            case AliasField.Tags:
                AliasEntry.Tags[TagIndex] = $"{NewValue}";
                break;
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        switch (ChangeType)
        {
            case AliasField.ID:
                AliasEntry.ID = $"{OldValue}";
                break;
            case AliasField.Name:
                AliasEntry.Name = $"{OldValue}";
                break;
            case AliasField.Tags:
                AliasEntry.Tags[TagIndex] = $"{OldValue}";
                break;
        }

        return ActionEvent.NoEvent;
    }

    public enum AliasField
    {
        ID = 0,
        Name = 1,
        Tags = 2
    }
}