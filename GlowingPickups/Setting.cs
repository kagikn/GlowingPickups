using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlowingPickups
{
    public class Setting
    {
        public float RangeMultiplier { get; set; } = 1f;
        public float LightIntensityMultiplier { get; set; } = 5f;
        public float FalloffExponent { get; set; } = 2.5f;

        public Setting()
        {
        }
    }
}
