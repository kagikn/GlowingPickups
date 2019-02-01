using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using GTA;
using GTA.Native;
using GTA.Math;
using System.Runtime.InteropServices;

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
            PickupObjectPoolTask.Init();

            _pickupDataOffset = GetPickupDataOffset(Game.Version);

            Tick += OnTick;
            Interval = 0;
        }

        private void OnTick(object o, EventArgs e)
        {
            if (settings == null)
            {
                return;
            }

            var pickupAddresses = PickupObjectPoolTask.GetPickupObjectAddresses();
            foreach (var pickupAddr in pickupAddresses)
            {
                unsafe
                {
                    var isVisible = (Marshal.ReadByte(pickupAddr, 0x2C) & 0x01) == 1;

                    if (!isVisible)
                    {
                        continue;
                    }

                    var pos = *(Vector3*)(pickupAddr + 0x90);
                    var dataAddress = Marshal.ReadIntPtr(pickupAddr, _pickupDataOffset);

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
                        Function.Call(Hash._DRAW_LIGHT_WITH_RANGE_WITH_SHADOW, pos.X, pos.Y, pos.Z, red,
                        green, blue, range, intensity, darkIntensity);
                    }
                    else
                    {
                        Function.Call(Hash._DRAW_LIGHT_WITH_RANGE_WITH_SHADOW, pos.X, pos.Y, pos.Z, 255, 57, 0, 5.0f, 30.0f, 10.0f);
                    }
                }
            }
        }

        //Probably pattern searching is the better way, but I don't know about the pickup data pattern
        private int GetPickupDataOffset(GameVersion version)
        {
            var offset = version >= GameVersion.VER_1_0_944_2_STEAM ? 0x480 : 0x470;
            offset = version >= GameVersion.VER_1_0_1604_0_STEAM ? 0x490 : offset;

            return offset;
        }
    }
}
