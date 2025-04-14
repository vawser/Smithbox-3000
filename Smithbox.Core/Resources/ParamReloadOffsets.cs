﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.Resources;

public class GameOffsetResource
{
    public string exeName { get; set; }
    public List<GameOffsetReference> list { get; set; }
}

public class GameOffsetReference
{
    public string exeVersion { get; set; }
    public string paramBaseAob { get; set; }
    public string paramBaseAobRelativeOffset { get; set; }
    public string paramBase { get; set; }
    public string paramInnerPath { get; set; }
    public string paramCountOffset { get; set; }
    public string paramDataOffset { get; set; }
    public string rowPointerOffset { get; set; }
    public string rowHeaderSize { get; set; }

    public List<string> paramOffsets { get; set; }
    public List<string> itemGibOffsets { get; set; }
}

