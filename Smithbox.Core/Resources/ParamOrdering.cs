using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.Resources;

/// <summary>
/// For param
/// </summary>
public class ParamOrder
{
    public Dictionary<string, ParamOrderEntry> Entries { get; set; }
}

public class ParamOrderEntry
{
    /// <summary>
    /// Index : Param Name
    /// </summary>
    public Dictionary<int, string> ParamOrder { get; set; }
}


/// <summary>
/// For rows
/// </summary>
public class ParamRowOrder
{
    public Dictionary<string, ParamRowOrderEntry> Entries { get; set; }
}

public class ParamRowOrderEntry
{
    /// <summary>
    /// Display Index : True Index
    /// </summary>
    public Dictionary<int, int> RowOrder { get; set; }
}


/// <summary>
/// For fields
/// </summary>
public class ParamFieldOrder
{
    public Dictionary<string, ParamFieldOrderEntry> Entries { get; set; }
}

public class ParamFieldOrderEntry
{
    /// <summary>
    /// Index : Field Internal Name
    /// </summary>
    public Dictionary<int, string> FieldOrder { get; set; }
}
