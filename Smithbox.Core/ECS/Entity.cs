using Smithbox.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.ECS;

/// <summary>
/// This is a wrapper than contains the visual entity (e.g. the object in the viewport) 
/// and its connected logical entity (e.g. the data that belongs to it)
/// </summary>
public class Entity : IDisposable
{
    /// <summary>
    /// Identifier
    /// </summary>
    public Guid Guid { get; set; }

    /// <summary>
    /// The world this entity is parented to.
    /// </summary>
    public World World { get; set; }

    /// <summary>
    /// The entity this entity is parented to.
    /// </summary>
    public Entity Parent { get; set; }

    /// <summary>
    /// The children of this entity
    /// </summary>
    public List<Entity> Children { get; set; }

    /// <summary>
    /// The top-level renderable for this entity (other renderable elements are children to this one).
    /// </summary>
    public Renderable Renderable { get; set; }

    /// <summary>
    /// The backing data for this entity.
    /// </summary>
    public object Data { get; set; }

    /// <summary>
    /// The transform for this entity
    /// </summary>
    public Transform Transform { get; set; } = new Transform();

    /// <summary>
    /// Visibility state
    /// </summary>
    public bool Visible { get; set; }

    /// <summary>
    /// A display name for this entity.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Container for MSB specific properties that an entity may have. Is null for non-map entities.
    /// </summary>
    public MsbData MsbData { get; set; }

    public Entity()
    {
        Guid = GUID.Generate();
    }

    public Entity(World world) 
    {
        Guid = GUID.Generate();
        World = world;
    }

    public Entity(World world, object data)
    {
        Guid = GUID.Generate();
        World = world;
        Data = data;
    }

    public Entity(Entity entity)
    {
        Guid = GUID.Generate();
        World = entity.World;
        Parent = entity.Parent;
    }

    public void OnSelect()
    {

    }

    public void OnDeselect()
    {

    }

    public void Render()
    {

    }

    public void Dispose()
    {
        Renderable.Dispose();
    }
}
