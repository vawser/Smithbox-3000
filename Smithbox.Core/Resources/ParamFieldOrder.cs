using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.Resources;

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
