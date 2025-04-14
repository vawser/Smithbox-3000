﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.Resources;

public class ParamCommutativeResource
{
    public List<ParamCommutativeEntry> Groups { get; set; }
}

public class ParamCommutativeEntry
{
    public string Name { get; set; }
    public List<string> Params { get; set; }
}
