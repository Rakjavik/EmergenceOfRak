using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rak.world
{
    public class Plant : Resource
    {
        public ResourceType GetResourceType()
        {
            return ResourceType.Matter;
        }
    }
}
