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
    public class GlowingPickups : Script
    {
        Setting settings;
        readonly int _pickupDataOffset;

        public GlowingPickups()
        {
            var xmlPath = Path.ChangeExtension((new Uri(Assembly.GetExecutingAssembly().CodeBase)).LocalPath, "xml");
            var settingLoader = new SettingLoader<Setting>();
            settings = settingLoader.Load(xmlPath) ?? settingLoader.Init(xmlPath);

            unsafe
            {
                var addr = Game.FindPattern("75 37 48 8B 85 ? ? ? ? 40 84 78 0C 75 11");
                if (addr != IntPtr.Zero)
                {
                    _pickupDataOffset = *(int*)(addr + 5);
                }
            }

            if (_pickupDataOffset != 0 && settings != null)
            {
                Tick += OnTick;
                Interval = 0;
            }
            else
            {
                if (_pickupDataOffset == 0)
                {
                    throw new InvalidOperationException("Couldn't find the CPickupData offset of CPickup. Terminating GlowingPickups.");
                }
            }
        }

        private void OnTick(object o, EventArgs e)
        {
            if (settings == null)
            {
                return;
            }

            foreach (var pickup in World.GetAllPickupObjects().Where(x => x.IsVisible))
            {
                unsafe
                {
                    var pos = pickup.Position;
                    var dataAddress = Marshal.ReadIntPtr(pickup.MemoryAddress, _pickupDataOffset);

                    if (dataAddress != IntPtr.Zero)
                    {
                        var red = (int)(BitConverter.ToSingle(
                            BitConverter.GetBytes(Marshal.ReadInt32(dataAddress, 0x5C)), 0) * 255);
                        var green = (int)(BitConverter.ToSingle(
                            BitConverter.GetBytes(Marshal.ReadInt32(dataAddress, 0x60)), 0) * 255);
                        var blue = (int)(BitConverter.ToSingle(
                            BitConverter.GetBytes(Marshal.ReadInt32(dataAddress, 0x64)), 0) * 255);
                        var range = BitConverter.ToSingle(
                            BitConverter.GetBytes(Marshal.ReadInt32(dataAddress, 0x10)), 0) * settings.RangeMultiplier;
                        var intensity = BitConverter.ToSingle(
                            BitConverter.GetBytes(Marshal.ReadInt32(dataAddress, 0x68)), 0) * settings.LightIntensityMultiplier;
                        var darkIntensity = BitConverter.ToSingle(
                            BitConverter.GetBytes(Marshal.ReadInt32(dataAddress, 0x6C)), 0) * settings.ShadowMultiplier;
                        Function.Call(Hash.DRAW_LIGHT_WITH_RANGEEX, pos.X, pos.Y, pos.Z, red,
                        green, blue, range, intensity, darkIntensity);
                    }
                    else
                    {
                        Function.Call(Hash.DRAW_LIGHT_WITH_RANGEEX, pos.X, pos.Y, pos.Z, 255, 57, 0, 5.0f, 30.0f, 10.0f);
                    }
                }
            }
        }
    }
}
