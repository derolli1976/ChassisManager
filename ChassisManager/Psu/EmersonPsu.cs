using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.GFS.WCS.ChassisManager
{
    internal class EmersonPsu : PsuBase
    {
        internal EmersonPsu(byte deviceId)
            : base(deviceId)
        {
        }
    }
}
