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
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct EntityPool
    {
        [FieldOffset(0x10)]
        internal uint num1;
        [FieldOffset(0x20)]
        internal uint num2;

        internal bool IsFull()
        {
            return num1 - (num2 & 0x3FFFFFFF) <= 256;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct GenericPool
    {
        [FieldOffset(0x00)]
        public ulong poolStartAddress;
        [FieldOffset(0x08)]
        public IntPtr byteArray;
        [FieldOffset(0x10)]
        public uint size;
        [FieldOffset(0x14)]
        public uint itemSize;

        public bool IsValid(uint index)
        {
            return Mask(index) != 0;
        }

        public ulong GetAddress(uint index)
        {
            return ((Mask(index) & (poolStartAddress + index * itemSize)));
        }

        private ulong Mask(uint index)
        {
            unsafe
            {
                byte* byteArrayPtr = (byte*)byteArray.ToPointer();
                long num1 = byteArrayPtr[index] & 0x80;
                return (ulong)(~((num1 | -num1) >> 63));
            }
        }
    }

    unsafe public static class PickupObjectPoolTask
    {
        //static public IntPtr _AddEntityToPoolFuncAddress;
        static public ulong* _EntityPoolAddress;
        static public ulong* _PickupObjectPoolAddress;
        internal delegate int AddEntityToPoolFunc(ulong address); //returns an entity handle
        static internal AddEntityToPoolFunc _addEntToPoolFunc;

        static List<int> _handles = new List<int>();

        static public void Init()
        {
            FindEntityPoolAddress();
            FindPickupPoolAddress();
            FindAddEntityToPoolFuncAddress();
        }

        //This is unstable and can easily crash the game
        /*static public List<Prop> GetPickupObjects()
        {
            if (*_EntityPoolAddress == 0 || *_PickupObjectPoolAddress == 0)
            {
                return new List<Prop>();
            }

            GenericPool* pickupPool = (GenericPool*)(*_PickupObjectPoolAddress);
            EntityPool* entitiesPool = (EntityPool*)(*_EntityPoolAddress);

            List<Prop> pickupHandles = new List<Prop>();

            for (uint i = 0; i < pickupPool->size; i++)
            {
                if (entitiesPool->IsFull())
                {
                    break;
                }

                if (pickupPool->IsValid(i))
                {

                    ulong address = pickupPool->GetAddress(i);

                    if (address != 0)
                    {
                        int handle = _addEntToPoolFunc(address);
                        pickupHandles.Add(new Prop(handle));
                    }
                }
            }
            return pickupHandles;
        }*/

        static public List<IntPtr> GetPickupObjectAddresses()
        {
            if (*_EntityPoolAddress == 0 || *_PickupObjectPoolAddress == 0)
            {
                return new List<IntPtr>();
            }

            GenericPool* pickupPool = (GenericPool*)(*_PickupObjectPoolAddress);
            EntityPool* entitiesPool = (EntityPool*)(*_EntityPoolAddress);

            List<IntPtr> pickupsAddresses = new List<IntPtr>();

            for (uint i = 0; i < pickupPool->size; i++)
            {
                if (entitiesPool->IsFull())
                {
                    break;
                }

                if (pickupPool->IsValid(i))
                {
                    ulong address = pickupPool->GetAddress(i);

                    if (address != 0)
                    {
                        pickupsAddresses.Add(new IntPtr((long)address));
                    }
                }
            }
            return pickupsAddresses;
        }

        static public void FindEntityPoolAddress()
        {
            var address = MemoryAccess.FindPattern("\x4C\x8B\x0D\x00\x00\x00\x00\x44\x8B\xC1\x49\x8B\x41\x08", "xxx????xxxxxxx");
            _EntityPoolAddress = (ulong*)(*(int*)(address + 3) + address + 7);
        }
        static public void FindPickupPoolAddress()
        {
            var address = MemoryAccess.FindPattern("\x4C\x8B\x05\x00\x00\x00\x00\x40\x8A\xF2\x8B\xE9", "xxx????xxxxx");
            _PickupObjectPoolAddress = (ulong*)(*(int*)(address + 3) + address + 7);
        }
        static public void FindAddEntityToPoolFuncAddress()
        {
            var address = MemoryAccess.FindPattern("\x48\xF7\xF9\x49\x8B\x48\x08\x48\x63\xD0\xC1\xE0\x08\x0F\xB6\x1C\x11\x03\xD8", "xxxxxxxxxxxxxxxxxxx");
            _addEntToPoolFunc = Marshal.GetDelegateForFunctionPointer<AddEntityToPoolFunc>(new IntPtr(address - 0x68));
        }
    }
}
