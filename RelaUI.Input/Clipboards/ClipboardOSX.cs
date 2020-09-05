using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace RelaUI.Input.Clipboards
{
    public class ClipboardOsx : IClipboard
    {
        private IntPtr NSString = objc_getClass("NSString");
        private IntPtr NSPasteboard = objc_getClass("NSPasteboard");
        private IntPtr NSStringPboardType;
        private IntPtr UtfTextType;
        private IntPtr GeneralPasteboard;

        public ClipboardOsx()
        {
            UtfTextType = objc_msgSend(objc_msgSend(NSString, sel_registerName("alloc")), sel_registerName("initWithUTF8String:"), "public.utf8-plain-text");
            NSStringPboardType = objc_msgSend(objc_msgSend(NSString, sel_registerName("alloc")), sel_registerName("initWithUTF8String:"), "NSStringPboardType");

            GeneralPasteboard = objc_msgSend(NSPasteboard, sel_registerName("generalPasteboard"));
        }

        public string GetClipboard()
        {
            var ptr = objc_msgSend(GeneralPasteboard, sel_registerName("stringForType:"), NSStringPboardType);
            var charArray = objc_msgSend(ptr, sel_registerName("UTF8String"));
            return Marshal.PtrToStringAnsi(charArray);
        }

        public void SetClipboard(string s)
        {
            IntPtr str = IntPtr.Zero;
            try
            {
                str = objc_msgSend(objc_msgSend(NSString, sel_registerName("alloc")), sel_registerName("initWithUTF8String:"), s);
                objc_msgSend(GeneralPasteboard, sel_registerName("clearContents"));
                objc_msgSend(GeneralPasteboard, sel_registerName("setString:forType:"), str, UtfTextType);
            }
            finally
            {
                if (str != IntPtr.Zero)
                {
                    objc_msgSend(str, sel_registerName("release"));
                }
            }
        }

        [DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
        private static extern IntPtr objc_getClass(string className);

        [DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
        private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);

        [DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
        private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, string arg1);

        [DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
        private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg1);

        [DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
        private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg1, IntPtr arg2);

        [DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
        private static extern IntPtr sel_registerName(string selectorName);
    }
}