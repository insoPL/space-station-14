using Content.Server.Kitchen.EntitySystems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Server.Medical.Components
{
    [Access(typeof(IvDripSystem)), RegisterComponent]
    public sealed partial class IvDripComponent : Component
    {

    }
}
