using Smithbox.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.ECS;

/// <summary>
/// This is a 'renderable' element that is displayed in the viewport
/// </summary>
public class Renderable : IDisposable
{
    /// <summary>
    /// Identifier
    /// </summary>
    public Guid Guid { get; set; }

    /// <summary>
    /// The entity this renderable is parented to.
    /// </summary>
    public Entity Entity { get; set; }

    /// <summary>
    /// The renderable this renderable is parented to.
    /// </summary>
    public Renderable Parent { get; set; }

    /// <summary>
    /// The children of this renderable
    /// </summary>
    public List<Renderable> Children { get; set; }

    /// <summary>
    /// The transform for this renderable
    /// </summary>
    public Transform Transform { get; set; } = new Transform();

    /// <summary>
    /// Visibility state
    /// </summary>
    public bool Visible { get; set; }

    public Renderable(Entity entity)
    {
        Guid = GUID.Generate();
        Entity = entity;
    }

    public Renderable(Renderable parent)
    {
        Guid = GUID.Generate();
        Parent = parent;
    }

    public void Render()
    {

    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
