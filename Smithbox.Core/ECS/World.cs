using Smithbox.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.ECS;

/// <summary>
/// This is a wrapper than contains all the entities that belong to a single worldspace
/// e.g. all the entities within a map
/// </summary>
public class World : IDisposable
{
    /// <summary>
    /// Identifier
    /// </summary>
    public Guid Guid { get; set; }

    /// <summary>
    /// The universe this world is parented to.
    /// </summary>
    public Universe Universe { get; set; }

    /// <summary>
    /// The world this world is parented to.
    /// </summary>
    public World Parent { get; set; }

    /// <summary>
    /// The children of this world
    /// </summary>
    public List<World> Children { get; set; }

    /// <summary>
    /// The entities that belong to this world.
    /// </summary>
    public List<Entity> Entities { get; set; }

    /// <summary>
    /// The transform for this world
    /// </summary>
    public Transform Transform { get; set; } = new Transform();

    /// <summary>
    /// Visibility state
    /// </summary>
    public bool Visible { get; set; }

    public World()
    {
        Guid = GUID.Generate();
    }

    public void Render()
    {

    }

    public void Dispose()
    {
        for (int i = 0; i < Entities.Count; i++)
        {
            var curEntry = Entities[i];

            curEntry.Dispose();
        }
    }
}
