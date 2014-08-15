using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TerminalPowerTools
{
    static class NativeMethods
    {
        [DllImport("user32.dll")]
        static extern short VkKeyScan(char ch);

        public static Key ResolveKey(char charToResolve)
        {
            return KeyInterop.KeyFromVirtualKey(VkKeyScan(charToResolve));
        }
    }
}
