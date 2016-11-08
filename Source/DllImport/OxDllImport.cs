
//------------------------------------------------------------------------------
using System.Runtime.InteropServices;

namespace OxLib.DllImport
{
    static public class Win32
    {
        [DllImport("kernel32", ExactSpelling = true)]
        public static extern void SwitchToThread();
    }
}
