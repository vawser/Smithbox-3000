using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.ECS;

public class MsbData
{
    public Entity Owner { get; set; }

    /// <summary>
    /// A bool to track if this entity has render groups.
    /// </summary>
    public bool HasRenderGroups { get; set; } = true;

    /// <summary>
    /// The render group reference name of this entity.
    /// </summary>
    public uint[] RenderGroupName { get; set; }

    /// <summary>
    /// The drawgroups of this entity.
    /// </summary>
    public uint[] DrawGroups { get; set; }

    /// <summary>
    /// The display groups of this entity.
    /// </summary>
    public uint[] DisplayGroups { get; set; }

    public MsbData(Entity entity)
    {
        Owner = entity;
    }

    /// <summary>
    /// Returns true if owner entity is a Part
    /// </summary>
    public bool IsPart()
    {
        return Owner.Data is MSB1.Part ||
            Owner.Data is MSB2.Part ||
            Owner.Data is MSB3.Part ||
            Owner.Data is MSBB.Part ||
            Owner.Data is MSBD.Part ||
            Owner.Data is MSBS.Part ||
            Owner.Data is MSB_AC6.Part ? true : false;
    }
    
    /// <summary>
     /// Returns true if owner entity is a Region
     /// </summary>
    public bool IsRegion()
    {
        return Owner.Data is MSB1.Region ||
            Owner.Data is MSB2.Region ||
            Owner.Data is MSB3.Region ||
            Owner.Data is MSBB.Region ||
            Owner.Data is MSBD.Region ||
            Owner.Data is MSBE.Region ||
            Owner.Data is MSBS.Region ||
            Owner.Data is MSB_AC6.Region ? true : false;
    }

    /// <summary>
    /// Returns true if this owner is a Event
    /// </summary>
    public bool IsEvent()
    {
        return Owner.Data is MSB1.Event ||
            Owner.Data is MSB2.Event ||
            Owner.Data is MSB3.Event ||
            Owner.Data is MSBB.Event ||
            Owner.Data is MSBD.Event ||
            Owner.Data is MSBE.Event ||
            Owner.Data is MSBS.Event ||
            Owner.Data is MSB_AC6.Event ? true : false;
    }
}
