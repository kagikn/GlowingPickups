using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlowingPickup
{
    public class Setting
    {
        public float RangeMultiplier { get; set; } = 1f;
        public float IntensityMultiplier { get; set; } = 3f;
        public float DarkIntensityMultiplier { get; set; } = 3f;

        public Setting()
        {
        }
    }
}
