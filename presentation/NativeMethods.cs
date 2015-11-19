using System;
using System.Runtime.InteropServices;

namespace Presentation
{
    public static class NativeMethods
    {
        public delegate bool wglSwapIntervalEXT(int interval);

        public static wglSwapIntervalEXT wglSwapInterval;
    }
}
