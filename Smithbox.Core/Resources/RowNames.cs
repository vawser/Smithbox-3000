﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.Resources;

/// <summary>
/// Full information for row name stripping
/// </summary>
public class RowNameStore
{
    public List<RowNameParam> Params { get; set; }
}

/// <summary>
/// Full information for row name stripping
/// </summary>
public class RowNameParam
{
    /// <summary>
    /// The name of the param
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The row name entries for this param
    /// </summary>
    public List<RowNameEntry> Entries { get; set; }
}

/// <summary>
/// Full information for row name stripping
/// </summary>
public class RowNameEntry
{
    /// <summary>
    /// The index of the row
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// The row ID
    /// </summary>
    public int ID { get; set; }

    /// <summary>
    /// The row name
    /// </summary>
    public string Name { get; set; }
}
