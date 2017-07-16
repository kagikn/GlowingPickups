using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlowingPickup
{
    public unsafe sealed class Pattern
    {
        private byte[] bytes;
        private char[] mask;
        private bool isAttempted;
        private IntPtr result;

        public Pattern(byte[] bytes, string mask)
        {
            this.bytes = bytes;
            this.mask = mask.ToCharArray();
        }

        public IntPtr Get(int offset = 0)
        {
            if (!isAttempted)
            {
                result = FindPattern();
            }

            return result;
        }

        public IntPtr FindPattern()
        {
            isAttempted = true;

            MODULEINFO module;

            Win32Native.GetModuleInformation(Win32Native.GetCurrentProcess(), Win32Native.GetModuleHandle(null), out module, (uint)sizeof(MODULEINFO));

            var address = (byte*)module.lpBaseOfDll.ToPointer();

            var address_end = address + module.SizeOfImage;

            for (; address < address_end; address++)
            {
                if (bCompare(address, bytes, mask))
                {
                    return new IntPtr(address);
                }
            }

            return IntPtr.Zero;
        }

        private bool bCompare(byte* pData, byte[] bMask, char[] szMask)
        {
            for (int i = 0; i < Math.Min(bMask.Length, szMask.Length); i++)
            {
                if (szMask[i] != '?' && pData[i] != bMask[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
