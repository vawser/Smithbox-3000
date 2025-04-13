using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.ECS;

public class Transform
{
    public Matrix4x4 Matrix { get; set; }

    public Transform()
    {
        Matrix = new Matrix4x4();
    }
}
