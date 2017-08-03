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
        public float ShadowMultiplier { get; set; } = 5f;

        public Setting()
        {
        }
    }
}
