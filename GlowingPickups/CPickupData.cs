using System;
using System.IO;
using System.Reflection;
using GTA;
using GTA.Native;
using GTA.Math;
using System.Runtime.InteropServices;
using System.Linq;

namespace GlowingPickups
{
    [StructLayout(LayoutKind.Explicit)]
    public struct CPickupData
    {
        [FieldOffset(0x8)]
        public uint NameHash;
        [FieldOffset(0x40)]
        public uint ModelHash;
        [FieldOffset(0x10)]
        public float GlowRange;
        [FieldOffset(0x54)]
        public float Scale;
        [FieldOffset(0x5C)]
        public float GlowRed;
        [FieldOffset(0x60)]
        public float GlowGreen;
        [FieldOffset(0x64)]
        public float GlowBlue;
        [FieldOffset(0x68)]
        public float GlowIntensity; // for SP
        [FieldOffset(0x6C)]
        public float DarkGlowIntensity; // for SP
    }
}
