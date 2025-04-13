using Smithbox.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.ECS;

/// <summary>
/// This is a wrapper than contains all the worldspaces
/// </summary>
public class Universe : IDisposable
{
    /// <summary>
    /// Identifier
    /// </summary>
    public Guid Guid { get; set; }

    /// <summary>
    /// The worlds that belong to this universe.
    /// </summary>
    public List<World> Worlds { get; set; }

    /// <summary>
    /// The transform for this universe
    /// </summary>
    public Transform Transform { get; set; } = new Transform();

    /// <summary>
    /// Visibility state
    /// </summary>
    public bool Visible { get; set; }

    public Universe()
    {
        Guid = GUID.Generate();
    }

    public void Render()
    {

    }

    public void Dispose()
    {
        for (int i = 0; i < Worlds.Count; i++)
        {
            var curEntry = Worlds[i];

            curEntry.Dispose();
        }
    }
}
