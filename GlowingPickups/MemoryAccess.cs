using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using GTA;

namespace GlowingPickups
{
    static internal class MemoryAccess
    {
        public unsafe static byte* FindPattern(string pattern, string mask)
        {
            ProcessModule module = Process.GetCurrentProcess().MainModule;

            ulong address = (ulong)module.BaseAddress.ToInt64();
            ulong endAddress = address + (ulong)module.ModuleMemorySize;

            for (; address < endAddress; address++)
            {
                for (int i = 0; i < pattern.Length; i++)
                {
                    if (mask[i] != '?' && ((byte*)address)[i] != pattern[i])
                    {
                        break;
                    }
                    else if (i + 1 == pattern.Length)
                    {
                        return (byte*)address;
                    }
                }
            }

            return null;
        }

        public static Prop[] GetAllPickupObjects()
        {
            return Array.ConvertAll(PickupObjectPoolTask.GetPickupObjectHandles().ToArray(), handle => new Prop(handle));
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct EntityPool
    {
        [FieldOffset(0x10)] UInt32 num1;
        [FieldOffset(0x20)] UInt32 num2;

        public bool IsFull()
        {
            return num1 - (num2 & 0x3FFFFFFF) <= 256;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct GenericPool
    {
        [FieldOffset(0x00)]
        public long poolStartAddress;
        [FieldOffset(0x08)]
        public byte* byteArray;
        [FieldOffset(0x10)]
        public uint size;
        [FieldOffset(0x14)]
        public uint itemSize;

        public bool IsValid(uint index)
        {
            return index < size && Mask(index) != 0;
        }

        public IntPtr GetAddress(uint index)
        {
            return new IntPtr((Mask(index) & (poolStartAddress + index * itemSize)));
        }

        public long Mask(uint index)
        {
            long num1 = byteArray[index] & 0x80;
            return ~((num1 | -num1) >> 63);
        }
    }

    unsafe public static class PickupObjectPoolTask
    {
        static public IntPtr _AddEntityToPoolFuncAddress;
        static public IntPtr _EntityPoolAddress;
        static public IntPtr _PickupObjectPoolAddress;
        internal delegate int AddEntityToPoolFunc(ulong address); //returns an entity handle
        static internal AddEntityToPoolFunc _addEntToPoolFunc;

        static List<int> _handles = new List<int>();

        static public void Init()
        {
            FindEntityPoolAddress();
            FindPickupPoolAddress();
            FindAddEntityToPoolFuncAddress();
        }

        static public List<Prop> GetPickupObjects()
        {
            FindEntityPoolAddress();
            FindPickupPoolAddress();

            GenericPool* pickupPool = (GenericPool*)(_PickupObjectPoolAddress.ToPointer());
            EntityPool* entitiesPool = (EntityPool*)(_EntityPoolAddress.ToPointer());

            List<Prop> pickupHandles = new List<Prop>();

            for (uint i = 0; i < pickupPool->size; i++)
            {
                if (entitiesPool->IsFull())
                {
                    break;
                }

                if (pickupPool->IsValid(i))
                {

                    ulong address = (ulong)pickupPool->GetAddress(i).ToInt64();

                    if (address != 0)
                    {
                        FindAddEntityToPoolFuncAddress();

                        int handle;
                        handle = _addEntToPoolFunc(address);
                        pickupHandles.Add(new Prop(handle));
                    }
                }
            }
            return pickupHandles;
        }

        static public void FindEntityPoolAddress()
        {
            if (_EntityPoolAddress == IntPtr.Zero)
            {
                var address = MemoryAccess.FindPattern("\x4C\x8B\x0D\x00\x00\x00\x00\x44\x8B\xC1\x49\x8B\x41\x08", "xxx????xxxxxxx");
                _EntityPoolAddress = new IntPtr(*(long*)address);
            }
        }
        static public void FindPickupPoolAddress()
        {
            if (_PickupObjectPoolAddress == IntPtr.Zero)
            {
                var address = MemoryAccess.FindPattern("\x8B\xF0\x48\x8B\x05\x00\x00\x00\x00\xF3\x0F\x59\xF6", "xxxxx????xxxx");
                _PickupObjectPoolAddress = new IntPtr(*(long*)(*(int*)(address + 5) + address + 9));
            }
        }
        static public void FindAddEntityToPoolFuncAddress()
        {
            if (_addEntToPoolFunc == null)
            {
                var address = MemoryAccess.FindPattern("\x48\xF7\xF9\x49\x8B\x48\x08\x48\x63\xD0\xC1\xE0\x08\x0F\xB6\x1C\x11\x03\xD8", "xxxxxxxxxxxxxxxxxxx");
                IntPtr pointer = new IntPtr(address - 0x68);
                _addEntToPoolFunc = (AddEntityToPoolFunc)Marshal.GetDelegateForFunctionPointer(pointer, typeof(AddEntityToPoolFunc));
                _AddEntityToPoolFuncAddress = pointer;
            }
        }
    }
}
