using System;
using System.Runtime.InteropServices;

namespace ASMC.Common
{
    internal class NativeMethods
    {
        private const string User32 = "user32.dll";

        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_DLGMODALFRAME = 0x0001;
        public const int SWP_NOSIZE = 0x0001;
        public const int SWP_NOMOVE = 0x0002;
        public const int SWP_NOZORDER = 0x0004;
        public const int SWP_FRAMECHANGED = 0x0020;
        public const uint WM_SETICON = 0x0080;

        [DllImport(User32)]
        public static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport(User32)]
        public static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        [DllImport(User32)]
        public static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x, int y, int width, int height, uint flags);

        [DllImport(User32)]
        public static extern IntPtr SendMessage(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport(User32)]
        public static extern IntPtr SetWindowsHookEx(HookType idHook, HookProc func, IntPtr instance, int threadId);

        [DllImport(User32)]
        public static extern int UnhookWindowsHookEx(IntPtr hhk);

        [DllImport(User32)]
        public static extern int CallNextHookEx(IntPtr hhk, int code, IntPtr wParam, ref KBDLLHOOKSTRUCT lParam);

        [DllImport(User32)]
        public static extern short GetAsyncKeyState(Keys vKey);

        [DllImport(User32)]
        public static extern short GetKeyState(int keyCode);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        internal delegate int HookProc(int code, IntPtr wParam, ref KBDLLHOOKSTRUCT lParam);

        internal enum HookType
        {
            WH_JOURNALRECORD = 0,
            WH_JOURNALPLAYBACK = 1,
            WH_KEYBOARD = 2,
            WH_GETMESSAGE = 3,
            WH_CALLWNDPROC = 4,
            WH_CBT = 5,
            WH_SYSMSGFILTER = 6,
            WH_MOUSE = 7,
            WH_HARDWARE = 8,
            WH_DEBUG = 9,
            WH_SHELL = 10,
            WH_FOREGROUNDIDLE = 11,
            WH_CALLWNDPROCRET = 12,
            WH_KEYBOARD_LL = 13,
            WH_MOUSE_LL = 14
        }

        internal struct KBDLLHOOKSTRUCT
        {
            public UInt32 vkCode;
            public UInt32 scanCode;
            public UInt32 flags;
            public UInt32 time;
            public IntPtr extraInfo;
        }
    }
}
