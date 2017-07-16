using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using GTA;

namespace GlowingPickup
{
    static internal class MemoryAccess
    {
        public static Prop[] GetAllPickupObjects()
        {
            return Array.ConvertAll(PickupObjectPoolTask.GetPickupObjectHandles().ToArray(), handle => new Prop(handle));
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct NativeVector3
    {
        [FieldOffset(0x0)]
        public float X;
        [FieldOffset(0x4)]
        public float Y;
        [FieldOffset(0x8)]
        public float Z;
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
            long  num1 = byteArray[index] & 0x80;
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
            //FindAddEntityToPoolFuncAddress();
        }
        static public uint GetPickupCount()
        {
            FindEntityPoolAddress();
            FindPickupPoolAddress();

            GenericPool* pickupPool = (GenericPool*)(_PickupObjectPoolAddress.ToPointer());

            uint count = 0;
            for (uint i = 0; i < pickupPool->size; i++)
            {
                if (pickupPool->IsValid(i))
                {
                    count++;
                }
            }

            return count;
        }

        static public List<int> GetPickupObjectHandles()
        {
            FindEntityPoolAddress();
            FindPickupPoolAddress();

            GenericPool* pickupPool = (GenericPool*)(_PickupObjectPoolAddress.ToPointer());
            EntityPool* entitiesPool = (EntityPool*)(_EntityPoolAddress.ToPointer());

            List<int> pickupsHandle = new List<int>();

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
                        handle = _addEntToPoolFunc(address); //this somehow crashes GTA even if address contains a proper memory address value
                        pickupsHandle.Add(handle);
                    }
                }
            }
            return pickupsHandle;
        }

        static public List<IntPtr> GetPickupObjectAddresses()
        {
            FindEntityPoolAddress();
            FindPickupPoolAddress();

            GenericPool* pickupPool = (GenericPool*)(_PickupObjectPoolAddress.ToPointer());
            EntityPool* entitiesPool = (EntityPool*)(_EntityPoolAddress.ToPointer());

            List<IntPtr> pickupsHandle = new List<IntPtr>();

            for (uint i = 0; i < pickupPool->size; i++)
            {
                if (entitiesPool->IsFull())
                {
                    break;
                }
                
                if (pickupPool->IsValid(i))
                {

                    long address = pickupPool->GetAddress(i).ToInt64();

                    if (address != 0)
                    {
                        pickupsHandle.Add(new IntPtr(address));
                    }
                }
            }
            return pickupsHandle;
        }


        static public void FindEntityPoolAddress()
        {
            if (_EntityPoolAddress == IntPtr.Zero)
            {
                var address = (byte*)new Pattern(new byte[] { 0x4C, 0x8B, 0x0D, 0, 0, 0, 0, 0x44, 0x8B, 0xC1, 0x49, 0x8B, 0x41, 0x08 }, "xxx????xxxxxxx").Get().ToPointer();
                address = (*(int*)(address + 3) + address + 7);
                _EntityPoolAddress = new IntPtr(*(long*)address);
            }
        }
        static public void FindPickupPoolAddress()
        {
            if (_PickupObjectPoolAddress == IntPtr.Zero)
            {
                var address = (byte*)new Pattern(new byte[] { 0x8B, 0xF0, 0x48, 0x8B, 0x05, 0, 0, 0, 0, 0xF3, 0x0F, 0x59, 0xF6 }, "xxxxx????xxxx").Get().ToPointer();
                address = (*(int*)(address + 5) + address + 9);
                _PickupObjectPoolAddress = new IntPtr(*(long*)address);
            }
        }
        static public void FindAddEntityToPoolFuncAddress()
        {
            if (_addEntToPoolFunc == null)
            {
                var bytes = new byte[] { 0x48, 0xF7, 0xF9, 0x49, 0x8B, 0x48, 0x08, 0x48, 0x63, 0xD0, 0xC1, 0xE0, 0x08, 0x0F, 0xB6, 0x1C, 0x11, 0x03, 0xD8 };
                IntPtr pointer = IntPtr.Subtract(new Pattern(bytes, "xxxxxxxxxxxxxxxxxxx").Get(), 0x68);
                _addEntToPoolFunc = (AddEntityToPoolFunc)Marshal.GetDelegateForFunctionPointer(pointer, typeof(AddEntityToPoolFunc));
                _AddEntityToPoolFuncAddress = pointer;
            }
        }
    }

}
