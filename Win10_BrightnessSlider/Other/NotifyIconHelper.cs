 

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Reflection;

namespace Win10_BrightnessSlider  //Zhwang.SuperNotifyIcon
{
    public static class NotifyIconHelpers
    {
        public static Rectangle GetNotifyIconRectangle(NotifyIcon notifyIcon, bool returnIfHidden)
        {
            Rectangle? rect;

            //if (SysInfo.IsWindows7OrLater)
                rect = GetNotifyIconRectangleWin7(notifyIcon, returnIfHidden);
            //else rect = GetNotifyIconRectangleLegacy(notifyIcon, returnIfHidden);

            if (rect.HasValue)
                return rect.Value;

            return Rectangle.Empty;
        }

        /// <summary>
        /// Determines whether the specified System.Windows.Forms.NotifyIcon is contained within the Windows 7 notification area fly-out.
        /// Note that this function will return false if the fly-out is closed, or if run on older versions of Windows.
        /// </summary>
        /// <param name="notifyIcon">Notify icon to test.</param>
        /// <returns>True if the notify icon is in the fly-out, false if not.</returns>
        //public static bool IsNotifyIconInFlyOut(NotifyIcon notifyIcon)
        //{
        //    if (!SysInfo.IsWindows7OrLater)
        //        return false;

        //    Rectangle notifyIconRect = GetNotifyIconRectangle(notifyIcon, true);
        //    if (notifyIconRect.IsEmpty)
        //        return false;

        //    return IsRectangleInFlyOut(notifyIconRect);
        //}

        /// <summary>
        /// Determines whether the specified System.Drawing.Rectangle is contained within the Windows 7 notification area fly-out.
        /// Note that this function will return false if the fly-out is closed, or if run on older versions of Windows.
        /// </summary>
        /// <param name="point">System.Drawing.Rectangle to test.</param>
        /// <returns>True if the notify icon is in the fly-out, false if not.</returns>
        //public static bool IsRectangleInFlyOut(Rectangle rectangle)
        //{
        //    if (!SysInfo.IsWindows7OrLater)
        //        return false;

        //    Rectangle taskbarRect = Taskbar.GetTaskbarRectangle();

        //    // Don't use Rectangle.IntersectsWith since we want to check if it's ENTIRELY inside
        //    return (rectangle.Left > taskbarRect.Right || rectangle.Right < taskbarRect.Left
        //         || rectangle.Bottom < taskbarRect.Top || rectangle.Top > taskbarRect.Bottom);
        //}

        /// <summary>
        /// Checks whether a point is within the bounds of the specified notify icon.
        /// </summary>
        /// <param name="point">Point to check.</param>
        /// <param name="notifyicon">Notify icon to check.</param>
        /// <returns>True if the point is contained in the bounds, false otherwise.</returns>
        public static bool IsPointInNotifyIcon(Point point, NotifyIcon notifyicon)
        {
            Rectangle? nirect = NotifyIconHelpers.GetNotifyIconRectangle(notifyicon, true);
            if (nirect == null)
                return false;
            return ((Rectangle)nirect).Contains(point);
        }

        private static bool CanGetNotifyIconIdentifier(NotifyIcon notifyIcon,
            out NativeMethods.NOTIFYICONIDENTIFIER identifier)
        {
            // You can either use uID + hWnd or a GUID, but GUID is new to Win7 and isn't used by NotifyIcon anyway.

            identifier = new NativeMethods.NOTIFYICONIDENTIFIER();

            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
            Type niType = typeof(NotifyIcon);

            FieldInfo idFieldInfo = niType.GetField("id", flags);
            identifier.uID = (uint)(int)idFieldInfo.GetValue(notifyIcon);

            FieldInfo windowFieldInfo = niType.GetField("window", flags);
            NativeWindow nativeWindow = (NativeWindow)windowFieldInfo.GetValue(notifyIcon);
            identifier.hWnd = nativeWindow.Handle;

            if (identifier.hWnd == null || identifier.hWnd == IntPtr.Zero)
                return false;

            identifier.cbSize = (uint)Marshal.SizeOf(identifier);
            return true;
        }

        private static Rectangle? GetNotifyIconRectangleWin7(NotifyIcon notifyIcon, bool returnIfHidden)
        {
            // Get the identifier
            NativeMethods.NOTIFYICONIDENTIFIER identifier;
            if (!CanGetNotifyIconIdentifier(notifyIcon, out identifier))
                return null;

            // And plug it in to get our rectangle!
            var iconLocation = new NativeMethods.RECT();
            int result = NativeMethods.Shell_NotifyIconGetRect(ref identifier, out iconLocation);
            Rectangle rect = iconLocation;

            // 0 means success, 1 means the notify icon is in the fly-out - either is fine
            if ((result == 0 || (result == 1 && returnIfHidden)) && rect.Width > 0 && rect.Height > 0)
                return iconLocation;
            else
                return null;
        }

        //private static Rectangle? GetNotifyIconRectangleLegacy(NotifyIcon notifyIcon, bool returnIfHidden)
        //{
        //    Rectangle? nirect = null;

        //    NativeMethods.NOTIFYICONIDENTIFIER niidentifier;
        //    if (!CanGetNotifyIconIdentifier(notifyIcon, out niidentifier))
        //        return null;

        //    // find the handle of the task bar
        //    IntPtr taskbarparenthandle = NativeMethods.FindWindow("Shell_TrayWnd", null);

        //    if (taskbarparenthandle == IntPtr.Zero)
        //        return null;

        //    // find the handle of the notification area
        //    IntPtr naparenthandle = NativeMethods.FindWindowEx(taskbarparenthandle, IntPtr.Zero, "TrayNotifyWnd", null);

        //    if (naparenthandle == IntPtr.Zero)
        //        return null;

        //    // make a list of toolbars in the notification area (one of them should contain the icon)
        //    List<IntPtr> natoolbarwindows = NativeMethods.GetChildToolbarWindows(naparenthandle);

        //    bool found = false;

        //    for (int i = 0; !found && i < natoolbarwindows.Count; i++)
        //    {
        //        IntPtr natoolbarhandle = natoolbarwindows[i];

        //        // retrieve the number of toolbar buttons (i.e. notify icons)
        //        int buttoncount = NativeMethods.SendMessage(natoolbarhandle, NativeMethods.TB_BUTTONCOUNT, IntPtr.Zero, IntPtr.Zero).ToInt32();

        //        // get notification area's process id
        //        uint naprocessid;
        //        NativeMethods.GetWindowThreadProcessId(natoolbarhandle, out naprocessid);

        //        // get handle to notification area's process
        //        IntPtr naprocesshandle = NativeMethods.OpenProcess(NativeMethods.ProcessAccessFlags.All, false, naprocessid);

        //        if (naprocesshandle == IntPtr.Zero)
        //            return null;

        //        // allocate enough memory within the notification area's process to store the button info we want
        //        IntPtr toolbarmemoryptr = NativeMethods.VirtualAllocEx(naprocesshandle, (IntPtr)null, (uint)Marshal.SizeOf(typeof(NativeMethods.TBBUTTON)), NativeMethods.AllocationType.Commit, NativeMethods.MemoryProtection.ReadWrite);

        //        if (toolbarmemoryptr == IntPtr.Zero)
        //            return null;

        //        try
        //        {
        //            // loop through the toolbar's buttons until we find our notify icon
        //            for (int j = 0; !found && j < buttoncount; j++)
        //            {
        //                int bytesread = -1;

        //                // ask the notification area to give us information about the current button
        //                NativeMethods.SendMessage(natoolbarhandle, NativeMethods.TB_GETBUTTON, new IntPtr(j), toolbarmemoryptr);

        //                // retrieve that information from the notification area's process
        //                NativeMethods.TBBUTTON buttoninfo = new NativeMethods.TBBUTTON();
        //                NativeMethods.ReadProcessMemory(naprocesshandle, toolbarmemoryptr, out buttoninfo, Marshal.SizeOf(buttoninfo), out bytesread);

        //                if (bytesread != Marshal.SizeOf(buttoninfo) || buttoninfo.dwData == IntPtr.Zero)
        //                    return null;

        //                // the dwData field contains a pointer to information about the notify icon:
        //                // the handle of the notify icon (an 4/8 bytes) and the id of the notify icon (4 bytes)
        //                IntPtr niinfopointer = buttoninfo.dwData;

        //                // read the notify icon handle
        //                IntPtr nihandlenew;
        //                NativeMethods.ReadProcessMemory(naprocesshandle, niinfopointer, out nihandlenew, Marshal.SizeOf(typeof(IntPtr)), out bytesread);

        //                if (bytesread != Marshal.SizeOf(typeof(IntPtr)))
        //                    return null;

        //                // read the notify icon id
        //                uint niidnew;
        //                NativeMethods.ReadProcessMemory(naprocesshandle, (IntPtr)((int)niinfopointer + (int)Marshal.SizeOf(typeof(IntPtr))), out niidnew, Marshal.SizeOf(typeof(uint)), out bytesread);

        //                if (bytesread != Marshal.SizeOf(typeof(uint)))
        //                    return null;

        //                // if we've found a match
        //                if (nihandlenew == niidentifier.hWnd && niidnew == niidentifier.uID)
        //                {
        //                    // check if the button is hidden: if it is, return the rectangle of the 'show hidden icons' button
        //                    if ((byte)(buttoninfo.fsState & NativeMethods.TBSTATE_HIDDEN) != 0)
        //                    {
        //                        if (returnIfHidden)
        //                            nirect = NotifyArea.GetButtonRectangle();
        //                        else
        //                            return null;
        //                    }
        //                    else
        //                    {
        //                        NativeMethods.RECT result = new NativeMethods.RECT();

        //                        // get the relative rectangle of the toolbar button (notify icon)
        //                        NativeMethods.SendMessage(natoolbarhandle, NativeMethods.TB_GETITEMRECT, new IntPtr(j), toolbarmemoryptr);

        //                        NativeMethods.ReadProcessMemory(naprocesshandle, toolbarmemoryptr, out result, Marshal.SizeOf(result), out bytesread);

        //                        if (bytesread != Marshal.SizeOf(result))
        //                            return null;

        //                        // find where the rectangle lies in relation to the screen
        //                        NativeMethods.MapWindowPoints(natoolbarhandle, (IntPtr)null, ref result, 2);

        //                        nirect = result;
        //                    }

        //                    found = true;
        //                }
        //            }
        //        }
        //        finally
        //        {
        //            // free memory within process
        //            NativeMethods.VirtualFreeEx(naprocesshandle, toolbarmemoryptr, 0, NativeMethods.FreeType.Release);

        //            // close handle to process
        //            NativeMethods.CloseHandle(naprocesshandle);
        //        }
        //    }

        //    return nirect;
        //}
    
    
    }


    public static class UnsafeNativeMethods
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool SetForegroundWindow(HandleRef hWnd);
    }
}



/**
 * Copyright (c) 2010-2011, Richard Z.H. Wang <http://zhwang.me/>
 * Copyright (c) 2010-2011, David Warner <http://quppa.net/>
 * 
 * This library is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 * 
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public License
 * along with this license.  If not, see <http://www.gnu.org/licenses/>.
 */

//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Reflection;
//using System.Runtime.InteropServices;
//using System.Drawing;
//using System.Windows.Forms;

namespace Win10_BrightnessSlider  //Zhwang.SuperNotifyIcon
{
    internal class NativeMethods
    {
        /// <summary>
        /// Retrieves information about the specified button in a toolbar.
        /// </summary>
        public const uint TB_GETBUTTON = 0x417;

        /// <summary>
        /// Retrieves a count of the buttons currently in the toolbar.
        /// </summary>
        public const uint TB_BUTTONCOUNT = 0x418;

        /// <summary>
        /// Retrieves the bounding rectangle of a button in a toolbar.
        /// </summary>
        public const uint TB_GETITEMRECT = 0x41d;

        /// <summary>
        /// Toolbar button state. The button is not visible and cannot receive user input.
        /// </summary>
        public const byte TBSTATE_HIDDEN = 0x08;

        /// <summary>
        /// Returns NULL.
        /// </summary>
        public const uint MONITOR_DEFAULTTONULL = 0;

        /// <summary>
        /// Returns a handle to the primary display monitor.
        /// </summary>
        public const uint MONITOR_DEFAULTTOPRIMARY = 1;

        /// <summary>
        /// Returns a handle to the display monitor that is nearest to the rectangle.
        /// </summary>
        public const uint MONITOR_DEFAULTTONEAREST = 2;

        /// <summary>
        /// Sent to a window after its size has changed.
        /// </summary>
        public const int WM_SIZE = 0x0005;

        /// <summary>
        /// Sent to a window in order to determine what part of the window corresponds to a particular screen coordinate. This can happen, for example, when the cursor moves, when a mouse button is pressed or released, or in response to a call to a function such as WindowFromPoint. If the mouse is not captured, the message is sent to the window beneath the cursor. Otherwise, the message is sent to the window that has captured the mouse.
        /// </summary>
        public const int WM_NCHITTEST = 0x0084;

        /// <summary>
        /// Sent to a window if the mouse causes the cursor to move within a window and mouse input is not captured.
        /// </summary>
        public const int WM_SETCURSOR = 0x0020;

        /// <summary>
        /// Posted when the user presses the left mouse button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
        /// </summary>
        public const int WM_LBUTTONDOWN = 0x0201;

        /// <summary>
        /// Posted when the user presses the right mouse button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
        /// </summary>
        public const int WM_RBUTTONDOWN = 0x0204;

        /// <summary>
        /// Posted when the user presses the middle mouse button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
        /// </summary>
        public const int WM_MBUTTONDOWN = 0x0207;

        /// <summary>
        /// Posted when the user presses the first or second X button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
        /// </summary>
        public const int WM_XBUTTONDOWN = 0x020B;

        /// <summary>
        /// Sent to all top-level windows when Desktop Window Manager (DWM) composition has been enabled or disabled.
        /// </summary>
        public const int WM_DWMCOMPOSITIONCHANGED = 0x031E;

        /// <summary>
        /// In a client area.
        /// </summary>
        public const int HTCLIENT = 0x1;

        /// <summary>
        /// An application-defined callback function used with the EnumWindows or EnumDesktopWindows function. It receives top-level window handles. The WNDENUMPROC type defines a pointer to this callback function. EnumWindowsProc is a placeholder for the application-defined function name.
        /// </summary>
        /// <param name="hWnd">A handle to a top-level window.</param>
        /// <param name="parameter">The application-defined value given in EnumWindows or EnumDesktopWindows.</param>
        /// <returns>To continue enumeration, the callback function must return TRUE; to stop enumeration, it must return FALSE.</returns>
        public delegate bool EnumWindowProc(IntPtr hWnd, IntPtr parameter);

        /// <summary>
        /// Process security and access rights.
        /// </summary>
        [Flags]
        public enum ProcessAccessFlags : uint
        {
            /// <summary>
            /// All possible access rights for a process object.
            /// </summary>
            All = 0x001F0FFF,

            /// <summary>
            /// Required to terminate a process using TerminateProcess.
            /// </summary>
            Terminate = 0x00000001,

            /// <summary>
            /// Required to create a thread.
            /// </summary>
            CreateThread = 0x00000002,

            /// <summary>
            /// Required to perform an operation on the address space of a process (see VirtualProtectEx and WriteProcessMemory).
            /// </summary>
            VMOperation = 0x00000008,

            /// <summary>
            /// Required to read memory in a process using ReadProcessMemory.
            /// </summary>
            VMRead = 0x00000010,

            /// <summary>
            /// Required to write to memory in a process using WriteProcessMemory.
            /// </summary>
            VMWrite = 0x00000020,

            /// <summary>
            /// Required to duplicate a handle using DuplicateHandle.
            /// </summary>
            DupHandle = 0x00000040,

            /// <summary>
            /// Required to set certain information about a process, such as its priority class (see SetPriorityClass).
            /// </summary>
            SetInformation = 0x00000200,

            /// <summary>
            /// Required to retrieve certain information about a process, such as its token, exit code, and priority class (see OpenProcessToken, GetExitCodeProcess, GetPriorityClass, and IsProcessInJob).
            /// </summary>
            QueryInformation = 0x00000400,

            /// <summary>
            /// Required to wait for the process to terminate using the wait functions.
            /// </summary>
            Synchronize = 0x00100000
        }

        /// <summary>
        /// The type of memory allocation.
        /// </summary>
        [Flags]
        public enum AllocationType
        {
            /// <summary>
            /// Allocates physical storage in memory or in the paging file on disk for the specified reserved memory pages. The function initializes the memory to zero.
            /// To reserve and commit pages in one step, call VirtualAllocEx with MEM_COMMIT | MEM_RESERVE.
            /// The function fails if you attempt to commit a page that has not been reserved. The resulting error code is ERROR_INVALID_ADDRESS.
            /// An attempt to commit a page that is already committed does not cause the function to fail. This means that you can commit pages without first determining the current commitment state of each page.
            /// </summary>
            Commit = 0x1000,

            /// <summary>
            /// Reserves a range of the process's virtual address space without allocating any actual physical storage in memory or in the paging file on disk.
            /// You commit reserved pages by calling VirtualAllocEx again with MEM_COMMIT. To reserve and commit pages in one step, call VirtualAllocEx with MEM_COMMIT |MEM_RESERVE.
            /// Other memory allocation functions, such as malloc and LocalAlloc, cannot use reserved memory until it has been released.
            /// </summary>
            Reserve = 0x2000,

            /// <summary>
            /// Decommits memory.
            /// </summary>
            Decommit = 0x4000,

            /// <summary>
            /// Releases memory.
            /// </summary>
            Release = 0x8000,

            /// <summary>
            /// Indicates that data in the memory range specified by lpAddress and dwSize is no longer of interest. The pages should not be read from or written to the paging file. However, the memory block will be used again later, so it should not be decommitted. This value cannot be used with any other value.
            /// Using this value does not guarantee that the range operated on with MEM_RESET will contain zeroes. If you want the range to contain zeroes, decommit the memory and then recommit it.
            /// When you use MEM_RESET, the VirtualAllocEx function ignores the value of fProtect. However, you must still set fProtect to a valid protection value, such as PAGE_NOACCESS.
            /// VirtualAllocEx returns an error if you use MEM_RESET and the range of memory is mapped to a file. A shared view is only acceptable if it is mapped to a paging file.
            /// </summary>
            Reset = 0x80000,

            /// <summary>
            /// Reserves an address range that can be used to map Address Windowing Extensions (AWE) pages.
            /// This value must be used with MEM_RESERVE and no other values.
            /// </summary>
            Physical = 0x400000,

            /// <summary>
            /// Allocates memory at the highest possible address.
            /// </summary>
            TopDown = 0x100000,

            /// <summary>
            /// Allocates memory using large page support.
            /// The size and alignment must be a multiple of the large-page minimum. To obtain this value, use the GetLargePageMinimum function.
            /// </summary>
            LargePages = 0x20000000
        }

        /// <summary>
        /// The following are the memory-protection options; you must specify one of the following values when allocating or protecting a page in memory. Protection attributes cannot be assigned to a portion of a page; they can only be assigned to a whole page.
        /// </summary>
        [Flags]
        public enum MemoryProtection
        {
            /// <summary>
            /// Enables execute access to the committed region of pages. An attempt to read from or write to the committed region results in an access violation.
            /// </summary>
            Execute = 0x10,

            /// <summary>
            /// Enables execute or read-only access to the committed region of pages. An attempt to write to the committed region results in an access violation.
            /// </summary>
            ExecuteRead = 0x20,

            /// <summary>
            /// Enables execute, read-only, or read/write access to the committed region of pages.
            /// </summary>
            ExecuteReadWrite = 0x40,

            /// <summary>
            /// Enables execute, read-only, or copy-on-write access to a mapped view of a file mapping object. An attempt to write to a committed copy-on-write page results in a public copy of the page being made for the process. The public page is marked as PAGE_EXECUTE_READWRITE, and the change is written to the new page.
            /// </summary>
            ExecuteWriteCopy = 0x80,

            /// <summary>
            /// Disables all access to the committed region of pages. An attempt to read from, write to, or execute the committed region results in an access violation.
            /// </summary>
            NoAccess = 0x01,

            /// <summary>
            /// Enables read-only access to the committed region of pages. An attempt to write to the committed region results in an access violation. If Data Execution Prevention is enabled, an attempt to execute code in the committed region results in an access violation.
            /// </summary>
            ReadOnly = 0x02,

            /// <summary>
            /// Enables read-only or read/write access to the committed region of pages. If Data Execution Prevention is enabled, attempting to execute code in the committed region results in an access violation.
            /// </summary>
            ReadWrite = 0x04,

            /// <summary>
            /// Enables read-only or copy-on-write access to a mapped view of a file mapping object. An attempt to write to a committed copy-on-write page results in a public copy of the page being made for the process. The public page is marked as PAGE_READWRITE, and the change is written to the new page. If Data Execution Prevention is enabled, attempting to execute code in the committed region results in an access violation.
            /// </summary>
            WriteCopy = 0x08,

            /// <summary>
            /// Pages in the region become guard pages. Any attempt to access a guard page causes the system to raise a STATUS_GUARD_PAGE_VIOLATION exception and turn off the guard page status. Guard pages thus act as a one-time access alarm. For more information, see Creating Guard Pages. 
            /// When an access attempt leads the system to turn off guard page status, the underlying page protection takes over.
            /// If a guard page exception occurs during a system service, the service typically returns a failure status indicator.
            /// This value cannot be used with PAGE_NOACCESS.
            /// </summary>
            GuardModifierflag = 0x100,

            /// <summary>
            /// Sets all pages to be non-cachable. Applications should not use this attribute except when explicitly required for a device. Using the interlocked functions with memory that is mapped with SEC_NOCACHE can result in an EXCEPTION_ILLEGAL_INSTRUCTION exception.
            /// The PAGE_NOCACHE flag cannot be used with the PAGE_GUARD, PAGE_NOACCESS, or PAGE_WRITECOMBINE flags.
            /// The PAGE_NOCACHE flag can be used only when allocating public memory with the VirtualAlloc, VirtualAllocEx, or VirtualAllocExNuma functions. To enable non-cached memory access for shared memory, specify the SEC_NOCACHE flag when calling the CreateFileMapping function.
            /// </summary>
            NoCacheModifierflag = 0x200,

            /// <summary>
            /// Sets all pages to be write-combined.
            /// Applications should not use this attribute except when explicitly required for a device. Using the interlocked functions with memory that is mapped as write-combined can result in an EXCEPTION_ILLEGAL_INSTRUCTION exception.
            /// The PAGE_WRITECOMBINE flag cannot be specified with the PAGE_NOACCESS, PAGE_GUARD, and PAGE_NOCACHE flags. 
            /// The PAGE_WRITECOMBINE flag can be used only when allocating public memory with the VirtualAlloc, VirtualAllocEx, or VirtualAllocExNuma functions. To enable write-combined memory access for shared memory, specify the SEC_WRITECOMBINE flag when calling the CreateFileMapping function.
            /// </summary>
            WriteCombineModifierflag = 0x400
        }

        /// <summary>
        /// The type of free operation.
        /// </summary>
        [Flags]
        public enum FreeType
        {
            /// <summary>
            /// Decommits the specified region of committed pages. After the operation, the pages are in the reserved state.
            /// The function does not fail if you attempt to decommit an uncommitted page. This means that you can decommit a range of pages without first determining their current commitment state.
            /// Do not use this value with MEM_RELEASE.
            /// </summary>
            Decommit = 0x4000,

            /// <summary>
            /// Releases the specified region of pages. After the operation, the pages are in the free state.
            /// If you specify this value, dwSize must be 0 (zero), and lpAddress must point to the base address returned by the VirtualAllocEx function when the region is reserved. The function fails if either of these conditions is not met.
            /// If any pages in the region are committed currently, the function first decommits, and then releases them.
            /// The function does not fail if you attempt to release pages that are in different states, some reserved and some committed. This means that you can release a range of pages without first determining the current commitment state.
            /// Do not use this value with MEM_DECOMMIT.
            /// </summary>
            Release = 0x8000,
        }

        /// <summary>
        /// Appbar message value to send.
        /// </summary>
        public enum ABMsg
        {
            /// <summary>
            /// Registers a new appbar and specifies the message identifier that the system should use to send notification messages to the appbar.
            /// </summary>
            ABM_NEW = 0,

            /// <summary>
            /// Unregisters an appbar, removing the bar from the system's internal list.
            /// </summary>
            ABM_REMOVE = 1,

            /// <summary>
            /// Requests a size and screen position for an appbar.
            /// </summary>
            ABM_QUERYPOS = 2,

            /// <summary>
            /// Sets the size and screen position of an appbar.
            /// </summary>
            ABM_SETPOS = 3,

            /// <summary>
            /// Retrieves the autohide and always-on-top states of the Windows taskbar.
            /// </summary>
            ABM_GETSTATE = 4,

            /// <summary>
            /// Retrieves the bounding rectangle of the Windows taskbar.
            /// </summary>
            ABM_GETTASKBARPOS = 5,

            /// <summary>
            /// Notifies the system to activate or deactivate an appbar. The lParam member of the APPBARDATA pointed to by pData is set to TRUE to activate or FALSE to deactivate.
            /// </summary>
            ABM_ACTIVATE = 6,

            /// <summary>
            /// Retrieves the handle to the autohide appbar associated with a particular edge of the screen.
            /// </summary>
            ABM_GETAUTOHIDEBAR = 7,

            /// <summary>
            /// Registers or unregisters an autohide appbar for an edge of the screen.
            /// </summary>
            ABM_SETAUTOHIDEBAR = 8,

            /// <summary>
            /// Notifies the system when an appbar's position has changed.
            /// </summary>
            ABM_WINDOWPOSCHANGED = 9,

            /// <summary>
            /// Windows XP and later: Sets the state of the appbar's autohide and always-on-top attributes.
            /// </summary>
            ABM_SETSTATE = 10
        }

        /// <summary>
        /// A value that specifies an edge of the screen. This member is used when sending the ABM_GETAUTOHIDEBAR, ABM_QUERYPOS, ABM_SETAUTOHIDEBAR, and ABM_SETPOS messages.
        /// </summary>
        public enum ABEdge
        {
            /// <summary>
            /// Left edge of screen.
            /// </summary>
            ABE_LEFT = 0,

            /// <summary>
            /// Top edge of screen.
            /// </summary>
            ABE_TOP = 1,

            /// <summary>
            /// Right edge of screen.
            /// </summary>
            ABE_RIGHT = 2,

            /// <summary>
            /// Bottom edge of screen.
            /// </summary>
            ABE_BOTTOM = 3
        }

        /// <summary>
        /// Autohide and always-on-top states of the Windows taskbar.
        /// </summary>
        public enum ABState
        {
            /// <summary>
            /// Autohide and always-on-top both off.
            /// </summary>
            ABS_MANUAL = 0,

            /// <summary>
            /// Always-on-top on, autohide off.
            /// </summary>
            ABS_AUTOHIDE = 1,

            /// <summary>
            /// Autohide on, always-on-top off.
            /// </summary>
            ABS_ALWAYSONTOP = 2,

            /// <summary>
            /// Autohide and always-on-top both on.
            /// </summary>
            ABS_AUTOHIDEANDONTOP = ABS_AUTOHIDE | ABS_ALWAYSONTOP
        }

        /// <summary>
        /// System metric or configuration setting (see GetSystemMetric).
        /// </summary>
        public enum SystemMetric
        {
            /// <summary>
            /// The default width of an icon, in pixels. The LoadIcon function can load only icons with the dimensions that SM_CXICON and SM_CYICON specifies.
            /// </summary>
            SM_CXICON = 11,

            /// <summary>
            /// The default height of an icon, in pixels. The LoadIcon function can load only icons with the dimensions SM_CXICON and SM_CYICON.
            /// </summary>
            SM_CYICON = 12,

            /// <summary>
            /// The recommended width of a small icon, in pixels. Small icons typically appear in window captions and in small icon view.
            /// </summary>
            SM_CXSMICON = 49,

            /// <summary>
            /// The recommended height of a small icon, in pixels. Small icons typically appear in window captions and in small icon view.
            /// </summary>
            SM_CYSMICON = 50
        }

        /// <summary>
        /// Retrieves the coordinates of a window's client area. The client coordinates specify the upper-left and lower-right corners of the client area. Because client coordinates are relative to the upper-left corner of a window's client area, the coordinates of the upper-left corner are (0,0).
        /// </summary>
        /// <param name="hWnd">A handle to the window whose client coordinates are to be retrieved.</param>
        /// <param name="lpRectangle">A pointer to a RECT structure that receives the client coordinates. The left and top members are zero. The right and bottom members contain the width and height of the window.</param>
        /// <returns>If the function succeeds, the return value is nonzero.
        /// If the function fails, the return value is zero. To get extended error information, call GetLastError.</returns>
        [DllImport("user32.dll")]
        public static extern bool GetClientRectangle(IntPtr hWnd, out RECT lpRectangle);

        /// <summary>
        /// Gets the screen coordinates of the bounding rectangle of a notification icon.
        /// </summary>
        /// <param name="identifier">Pointer to a NOTIFYICONIDENTIFIER structure that identifies the icon.</param>
        /// <param name="iconLocation">Pointer to a RECT structure that, when this function returns successfully, receives the coordinates of the icon.</param>
        /// <returns>If the method succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.</returns>
        [DllImport("Shell32", SetLastError = true)]
        public static extern int Shell_NotifyIconGetRect(ref NOTIFYICONIDENTIFIER identifier, out RECT iconLocation);

        /// <summary>
        /// Retrieves the dimensions of the bounding rectangle of the specified window. The dimensions are given in screen coordinates that are relative to the upper-left corner of the screen.
        /// </summary>
        /// <param name="hWnd">A handle to the window.</param>
        /// <param name="lpRectangle">A pointer to a RECT structure that receives the screen coordinates of the upper-left and lower-right corners of the window.</param>
        /// <returns>If the function succeeds, the return value is nonzero. 
        /// If the function fails, the return value is zero. To get extended error information, call GetLastError.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRectangle);

        /// <summary>
        /// The MapWindowPoints function converts (maps) a set of points from a coordinate space relative to one window to a coordinate space relative to another window.
        /// </summary>
        /// <param name="hWndFrom">A handle to the window from which points are converted. If this parameter is NULL or HWND_DESKTOP, the points are presumed to be in screen coordinates.</param>
        /// <param name="hWndTo">A handle to the window to which points are converted. If this parameter is NULL or HWND_DESKTOP, the points are converted to screen coordinates.</param>
        /// <param name="lpPoints">A pointer to an array of POINT structures that contain the set of points to be converted. The points are in device units. This parameter can also point to a RECT structure, in which case the cPoints parameter should be set to 2.</param>
        /// <param name="cPoints">The number of POINT structures in the array pointed to by the lpPoints parameter.</param>
        /// <returns>
        /// If the function succeeds, the low-order word of the return value is the number of pixels added to the horizontal coordinate of each source point in order to compute the horizontal coordinate of each destination point. (In addition to that, if precisely one of hWndFrom and hWndTo is mirrored, then each resulting horizontal coordinate is multiplied by -1.) The high-order word is the number of pixels added to the vertical coordinate of each source point in order to compute the vertical coordinate of each destination point.
        /// If the function fails, the return value is zero. Call SetLastError prior to calling this method to differentiate an error return value from a legitimate "0" return value.
        /// </returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int MapWindowPoints(IntPtr hWndFrom, IntPtr hWndTo, ref RECT lpPoints, uint cPoints);

        /// <summary>
        /// Retrieves the identifier of the thread that created the specified window and, optionally, the identifier of the process that created the window.
        /// </summary>
        /// <param name="hWnd">A handle to the window.</param>
        /// <param name="lpdwProcessId">A pointer to a variable that receives the process identifier. If this parameter is not NULL, GetWindowThreadProcessId copies the identifier of the process to the variable; otherwise, it does not.</param>
        /// <returns>The return value is the identifier of the thread that created the window.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        /// <summary>
        /// Opens an existing local process object.
        /// </summary>
        /// <param name="dwDesiredAccess">The access to the process object. This access right is checked against the security descriptor for the process. This parameter can be one or more of the process access rights. If the caller has enabled the SeDebugPrivilege privilege, the requested access is granted regardless of the contents of the security descriptor.</param>
        /// <param name="bInheritHandle">If this value is TRUE, processes created by this process will inherit the handle. Otherwise, the processes do not inherit this handle.</param>
        /// <param name="dwProcessId">The identifier of the local process to be opened. If the specified process is the System Process (0x00000000), the function fails and the last error code is ERROR_INVALID_PARAMETER. If the specified process is the Idle process or one of the CSRSS processes, this function fails and the last error code is ERROR_ACCESS_DENIED because their access restrictions prevent user-level code from opening them.</param>
        /// <returns>If the function succeeds, the return value is an open handle to the specified process.
        /// If the function fails, the return value is NULL. To get extended error information, call GetLastError.</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, uint dwProcessId);

        /// <summary>
        /// Closes an open object handle.
        /// </summary>
        /// <param name="hObject">A valid handle to an open object.</param>
        /// <returns>If the function succeeds, the return value is nonzero.
        /// If the function fails, the return value is zero. To get extended error information, call GetLastError.
        /// If the application is running under a debugger, the function will throw an exception if it receives either a handle value that is not valid or a pseudo-handle value. This can happen if you close a handle twice, or if you call CloseHandle on a handle returned by the FindFirstFile function instead of calling the FindClose function.</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

        /// <summary>
        /// Reserves or commits a region of memory within the virtual address space of a specified process. The function initializes the memory it allocates to zero, unless MEM_RESET is used.
        /// </summary>
        /// <param name="hProcess">The handle to a process. The function allocates memory within the virtual address space of this process.
        /// The handle must have the PROCESS_VM_OPERATION access right. For more information, see Process Security and Access Rights.</param>
        /// <param name="lpAddress">The pointer that specifies a desired starting address for the region of pages that you want to allocate.
        /// If you are reserving memory, the function rounds this address down to the nearest multiple of the allocation granularity.
        /// If you are committing memory that is already reserved, the function rounds this address down to the nearest page boundary. To determine the size of a page and the allocation granularity on the host computer, use the GetSystemInfo function.
        /// If lpAddress is NULL, the function determines where to allocate the region.</param>
        /// <param name="dwSize">The size of the region of memory to allocate, in bytes.
        /// If lpAddress is NULL, the function rounds dwSize up to the next page boundary.
        /// If lpAddress is not NULL, the function allocates all pages that contain one or more bytes in the range from lpAddress to lpAddress+dwSize. This means, for example, that a 2-byte range that straddles a page boundary causes the function to allocate both pages.</param>
        /// <param name="flAllocationType">The type of memory allocation.</param>
        /// <param name="flProtect">The memory protection for the region of pages to be allocated. If the pages are being committed, you can specify any one of the memory protection constants.</param>
        /// <returns>If the function succeeds, the return value is the base address of the allocated region of pages.
        /// If the function fails, the return value is NULL. To get extended error information, call GetLastError.</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, AllocationType flAllocationType, MemoryProtection flProtect);

        /// <summary>
        /// Releases, decommits, or releases and decommits a region of memory within the virtual address space of a specified process.
        /// </summary>
        /// <param name="hProcess">A handle to a process. The function frees memory within the virtual address space of the process. 
        /// The handle must have the PROCESS_VM_OPERATION access right. For more information, see Process Security and Access Rights.</param>
        /// <param name="lpAddress">A pointer to the starting address of the region of memory to be freed.
        /// If the dwFreeType parameter is MEM_RELEASE, lpAddress must be the base address returned by the VirtualAllocEx function when the region is reserved.</param>
        /// <param name="dwSize">The size of the region of memory to free, in bytes.
        /// If the dwFreeType parameter is MEM_RELEASE, dwSize must be 0 (zero). The function frees the entire region that is reserved in the initial allocation call to VirtualAllocEx.
        /// If dwFreeType is MEM_DECOMMIT, the function decommits all memory pages that contain one or more bytes in the range from the lpAddress parameter to (lpAddress+dwSize). This means, for example, that a 2-byte region of memory that straddles a page boundary causes both pages to be decommitted. If lpAddress is the base address returned by VirtualAllocEx and dwSize is 0 (zero), the function decommits the entire region that is allocated by VirtualAllocEx. After that, the entire region is in the reserved state.</param>
        /// <param name="dwFreeType">The type of free operation.</param>
        /// <returns>If the function succeeds, the return value is a nonzero value.
        /// If the function fails, the return value is 0 (zero). To get extended error information, call GetLastError.</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, FreeType dwFreeType);

        /// <summary>
        /// Reads data from an area of memory in a specified process. The entire area to be read must be accessible or the operation fails.
        /// </summary>
        /// <param name="hProcess">A handle to the process with memory that is being read. The handle must have PROCESS_VM_READ access to the process.</param>
        /// <param name="lpBaseAddress">A pointer to the base address in the specified process from which to read. Before any data transfer occurs, the system verifies that all data in the base address and memory of the specified size is accessible for read access, and if it is not accessible the function fails.</param>
        /// <param name="lpBuffer">A pointer to a buffer that receives the contents from the address space of the specified process.</param>
        /// <param name="dwSize">The number of bytes to be read from the specified process.</param>
        /// <param name="lpNumberOfBytesRead">A pointer to a variable that receives the number of bytes transferred into the specified buffer. If lpNumberOfBytesRead is NULL, the parameter is ignored.</param>
        /// <returns>If the function succeeds, the return value is nonzero.
        /// If the function fails, the return value is 0 (zero). To get extended error information, call GetLastError.
        /// The function fails if the requested read operation crosses into an area of the process that is inaccessible.</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, out IntPtr lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        /// <summary>
        /// Reads data from an area of memory in a specified process. The entire area to be read must be accessible or the operation fails.
        /// </summary>
        /// <param name="hProcess">A handle to the process with memory that is being read. The handle must have PROCESS_VM_READ access to the process.</param>
        /// <param name="lpBaseAddress">A pointer to the base address in the specified process from which to read. Before any data transfer occurs, the system verifies that all data in the base address and memory of the specified size is accessible for read access, and if it is not accessible the function fails.</param>
        /// <param name="lpBuffer">A pointer to a buffer that receives the contents from the address space of the specified process.</param>
        /// <param name="dwSize">The number of bytes to be read from the specified process.</param>
        /// <param name="lpNumberOfBytesRead">A pointer to a variable that receives the number of bytes transferred into the specified buffer. If lpNumberOfBytesRead is NULL, the parameter is ignored.</param>
        /// <returns>If the function succeeds, the return value is nonzero.
        /// If the function fails, the return value is 0 (zero). To get extended error information, call GetLastError.
        /// The function fails if the requested read operation crosses into an area of the process that is inaccessible.</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, out uint lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        /// <summary>
        /// Reads data from an area of memory in a specified process. The entire area to be read must be accessible or the operation fails.
        /// </summary>
        /// <param name="hProcess">A handle to the process with memory that is being read. The handle must have PROCESS_VM_READ access to the process.</param>
        /// <param name="lpBaseAddress">A pointer to the base address in the specified process from which to read. Before any data transfer occurs, the system verifies that all data in the base address and memory of the specified size is accessible for read access, and if it is not accessible the function fails.</param>
        /// <param name="lpBuffer">A pointer to a buffer that receives the contents from the address space of the specified process.</param>
        /// <param name="dwSize">The number of bytes to be read from the specified process.</param>
        /// <param name="lpNumberOfBytesRead">A pointer to a variable that receives the number of bytes transferred into the specified buffer. If lpNumberOfBytesRead is NULL, the parameter is ignored.</param>
        /// <returns>If the function succeeds, the return value is nonzero.
        /// If the function fails, the return value is 0 (zero). To get extended error information, call GetLastError.
        /// The function fails if the requested read operation crosses into an area of the process that is inaccessible.</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, out TBBUTTON lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        /// <summary>
        /// Reads data from an area of memory in a specified process. The entire area to be read must be accessible or the operation fails.
        /// </summary>
        /// <param name="hProcess">A handle to the process with memory that is being read. The handle must have PROCESS_VM_READ access to the process.</param>
        /// <param name="lpBaseAddress">A pointer to the base address in the specified process from which to read. Before any data transfer occurs, the system verifies that all data in the base address and memory of the specified size is accessible for read access, and if it is not accessible the function fails.</param>
        /// <param name="lpBuffer">A pointer to a buffer that receives the contents from the address space of the specified process.</param>
        /// <param name="dwSize">The number of bytes to be read from the specified process.</param>
        /// <param name="lpNumberOfBytesRead">A pointer to a variable that receives the number of bytes transferred into the specified buffer. If lpNumberOfBytesRead is NULL, the parameter is ignored.</param>
        /// <returns>If the function succeeds, the return value is nonzero.
        /// If the function fails, the return value is 0 (zero). To get extended error information, call GetLastError.
        /// The function fails if the requested read operation crosses into an area of the process that is inaccessible.</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, out RECT lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        /// <summary>
        /// Retrieves the name of the class to which the specified window belongs.
        /// </summary>
        /// <param name="hWnd">A handle to the window and, indirectly, the class to which the window belongs.</param>
        /// <param name="lpClassName">The class name string.</param>
        /// <param name="nMaxCount">The length of the lpClassName buffer, in characters. The buffer must be large enough to include the terminating null character; otherwise, the class name string is truncated to nMaxCount-1 characters.</param>
        /// <returns>If the function succeeds, the return value is the number of characters copied to the buffer, not including the terminating null character.
        /// If the function fails, the return value is zero. To get extended error information, call GetLastError.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        /// <summary>
        /// Sends the specified message to a window or windows. The SendMessage function calls the window procedure for the specified window and does not return until the window procedure has processed the message.
        /// </summary>
        /// <param name="hWnd">A handle to the window whose window procedure will receive the message. If this parameter is HWND_BROADCAST ((HWND)0xffff), the message is sent to all top-level windows in the system, including disabled or invisible unowned windows, overlapped windows, and pop-up windows; but the message is not sent to child windows.
        /// Message sending is subject to UIPI. The thread of a process can send messages only to message queues of threads in processes of lesser or equal integrity level.</param>
        /// <param name="Msg">The message to be sent.</param>
        /// <param name="wParam">Additional message-specific information (wParam).</param>
        /// <param name="lParam">Additional message-specific information (lParam).</param>
        /// <returns>The return value specifies the result of the message processing; it depends on the message sent.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// Sends the specified message to a window or windows. The SendMessage function calls the window procedure for the specified window and does not return until the window procedure has processed the message.
        /// </summary>
        /// <param name="hWnd">A handle to the window whose window procedure will receive the message. If this parameter is HWND_BROADCAST ((HWND)0xffff), the message is sent to all top-level windows in the system, including disabled or invisible unowned windows, overlapped windows, and pop-up windows; but the message is not sent to child windows.
        /// Message sending is subject to UIPI. The thread of a process can send messages only to message queues of threads in processes of lesser or equal integrity level.</param>
        /// <param name="Msg">The message to be sent.</param>
        /// <param name="wParam">Additional message-specific information (wParam).</param>
        /// <param name="lParam">Additional message-specific information (lParam).</param>
        /// <returns>The return value specifies the result of the message processing; it depends on the message sent.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, ref TBBUTTON lParam);

        /// <summary>
        /// Retrieves a handle to a window whose class name and window name match the specified strings. The function searches child windows, beginning with the one following the specified child window. This function does not perform a case-sensitive search.
        /// </summary>
        /// <param name="hwndParent">A handle to the parent window whose child windows are to be searched.
        /// If hwndParent is NULL, the function uses the desktop window as the parent window. The function searches among windows that are child windows of the desktop. 
        /// If hwndParent is HWND_MESSAGE, the function searches all message-only windows.</param>
        /// <param name="hwndChildAfter">A handle to a child window. The search begins with the next child window in the Z order. The child window must be a direct child window of hwndParent, not just a descendant window. 
        /// If hwndChildAfter is NULL, the search begins with the first child window of hwndParent. 
        /// Note that if both hwndParent and hwndChildAfter are NULL, the function searches all top-level and message-only windows.</param>
        /// <param name="lpszClass">The class name or a class atom created by a previous call to the RegisterClass or RegisterClassEx function. The atom must be placed in the low-order word of lpszClass; the high-order word must be zero.
        /// If lpszClass is a string, it specifies the window class name. The class name can be any name registered with RegisterClass or RegisterClassEx, or any of the predefined control-class names, or it can be MAKEINTATOM(0x8000). In this latter case, 0x8000 is the atom for a menu class. For more information, see the Remarks section of this topic.</param>
        /// <param name="lpszWindow">The window name (the window's title). If this parameter is NULL, all window names match.</param>
        /// <returns>If the function succeeds, the return value is a handle to the window that has the specified class and window names.
        /// If the function fails, the return value is NULL. To get extended error information, call GetLastError.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        /// <summary>
        /// Retrieves a handle to the top-level window whose class name and window name match the specified strings. This function does not search child windows. This function does not perform a case-sensitive search.
        /// </summary>
        /// <param name="lpClassName">The class name or a class atom created by a previous call to the RegisterClass or RegisterClassEx function. The atom must be in the low-order word of lpClassName; the high-order word must be zero. 
        /// If lpClassName points to a string, it specifies the window class name. The class name can be any name registered with RegisterClass or RegisterClassEx, or any of the predefined control-class names.
        /// If lpClassName is NULL, it finds any window whose title matches the lpWindowName parameter.</param>
        /// <param name="lpWindowName">The window name (the window's title). If this parameter is NULL, all window names match.</param>
        /// <returns>If the function succeeds, the return value is a handle to the window that has the specified class name and window name.
        /// If the function fails, the return value is NULL. To get extended error information, call GetLastError.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        /// <summary>
        /// Retrieves the length, in characters, of the specified window's title bar text (if the window has a title bar). If the specified window is a control, the function retrieves the length of the text within the control. However, GetWindowTextLength cannot retrieve the length of the text of an edit control in another application.
        /// </summary>
        /// <param name="hWnd">A handle to the window or control.</param>
        /// <returns>If the function succeeds, the return value is the length, in characters, of the text. Under certain conditions, this value may actually be greater than the length of the text. For more information, see the following Remarks section.
        /// If the window has no text, the return value is zero. To get extended error information, call GetLastError.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        /// <summary>
        /// Enumerates the child windows that belong to the specified parent window by passing the handle to each child window, in turn, to an application-defined callback function. EnumChildWindows continues until the last child window is enumerated or the callback function returns FALSE.
        /// </summary>
        /// <param name="hWndParent">A handle to the parent window whose child windows are to be enumerated. If this parameter is NULL, this function is equivalent to EnumWindows.</param>
        /// <param name="lpEnumFunc">A pointer to an application-defined callback function. For more information, see EnumChildProc.</param>
        /// <param name="lParam">An application-defined value to be passed to the callback function.</param>
        /// <returns>The return value is not used.</returns>
        [DllImport("user32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumChildWindows(IntPtr hWndParent, EnumWindowProc lpEnumFunc, IntPtr lParam);

        /// <summary>
        /// Returns a list of handles for windows of the class 'ToolbarWindow32'.
        /// </summary>
        /// <param name="parent">The parent window whose children should be searched.</param>
        /// <returns>A list of handles for windows of the class 'ToolbarWindow32'</returns>
        public static List<IntPtr> GetChildToolbarWindows(IntPtr parent)
        {
            List<IntPtr> result = new List<IntPtr>();
            GCHandle listHandle = GCHandle.Alloc(result);

            try
            {
                EnumWindowProc childProc = new EnumWindowProc(EnumToolbarWindow);
                EnumChildWindows(parent, childProc, GCHandle.ToIntPtr(listHandle));
            }
            finally
            {
                if (listHandle.IsAllocated)
                    listHandle.Free();
            }

            return result;
        }

        /// <summary>
        /// Enumerates windows with the class name 'ToolbarWindow32'.
        /// </summary>
        /// <param name="handle">A handle to a top-level window.</param>
        /// <param name="pointer">The application-defined value given in EnumWindows or EnumDesktopWindows.</param>
        /// <returns>To continue enumeration, the callback function must return TRUE; to stop enumeration, it must return FALSE.</returns>
        public static bool EnumToolbarWindow(IntPtr handle, IntPtr pointer)
        {
            GCHandle gch = GCHandle.FromIntPtr(pointer);

            List<IntPtr> list = gch.Target as List<IntPtr>;

            if (list == null)
                throw new InvalidCastException("GCHandle Target could not be cast as List<IntPtr>");

            StringBuilder classname = new StringBuilder(128);

            GetClassName(handle, classname, classname.Capacity);

            if (classname.ToString() == "ToolbarWindow32")
                list.Add(handle);

            return true;
        }

        /// <summary>
        /// Returns a list of handles for windows of the class 'ToolbarWindow32'.
        /// </summary>
        /// <param name="parent">The handle of the parent window whose children should be searched.</param>
        /// <returns>A list of handles for windows of the class 'ToolbarWindow32'.</returns>
        public static List<IntPtr> GetChildButtonWindows(IntPtr parent)
        {
            List<IntPtr> result = new List<IntPtr>();
            GCHandle listHandle = GCHandle.Alloc(result);

            try
            {
                EnumWindowProc childProc = new EnumWindowProc(EnumButtonWindow);
                EnumChildWindows(parent, childProc, GCHandle.ToIntPtr(listHandle));
            }
            finally
            {
                if (listHandle.IsAllocated)
                    listHandle.Free();
            }

            return result;
        }

        /// <summary>
        /// Enumerates windows with the class name 'Button' and a blank window name.
        /// </summary>
        /// <param name="handle">A handle to a top-level window.</param>
        /// <param name="pointer">The application-defined value given in EnumWindows or EnumDesktopWindows.</param>
        /// <returns>To continue enumeration, the callback function must return TRUE; to stop enumeration, it must return FALSE.</returns>
        public static bool EnumButtonWindow(IntPtr handle, IntPtr pointer)
        {
            GCHandle gch = GCHandle.FromIntPtr(pointer);

            List<IntPtr> list = gch.Target as List<IntPtr>;

            if (list == null)
                throw new InvalidCastException("GCHandle Target could not be cast as List<IntPtr>");

            StringBuilder classname = new StringBuilder(128);

            GetClassName(handle, classname, classname.Capacity);

            if (classname.ToString() == "Button" && GetWindowTextLength(handle) == 0)
                list.Add(handle);

            return true;
        }

        /// <summary>
        /// Retrieves the cursor's position, in screen coordinates.
        /// </summary>
        /// <param name="lpPoint">A pointer to a POINT structure that receives the screen coordinates of the cursor.</param>
        /// <returns>Returns nonzero if successful or zero otherwise. To get extended error information, call GetLastError.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(out POINT lpPoint);

        /// <summary>
        /// Sends an appbar message to the system.
        /// </summary>
        /// <param name="dwMessage">Appbar message value to send. This parameter can be one of the following values.</param>
        /// <param name="pData">The address of an APPBARDATA structure. The content of the structure on entry and on exit depends on the value set in the dwMessage parameter.</param>
        /// <returns>This function returns a message-dependent value. For more information, see the Windows SDK documentation for the specific appbar message sent. Links to those documents are given in the See Also section.</returns>
        [DllImport("shell32.dll", SetLastError = true)]
        public static extern IntPtr SHAppBarMessage(ABMsg dwMessage, ref APPBARDATA pData);

        /// <summary>
        /// Retrieves a handle to the display monitor that has the largest area of intersection with a specified rectangle.
        /// </summary>
        /// <param name="lprc">A pointer to a RECT structure that specifies the rectangle of interest in virtual-screen coordinates.</param>
        /// <param name="dwFlags">Determines the function's return value if the rectangle does not intersect any display monitor.</param>
        /// <returns>If the rectangle intersects one or more display monitor rectangles, the return value is an HMONITOR handle to the display monitor that has the largest area of intersection with the rectangle.
        /// If the rectangle does not intersect a display monitor, the return value depends on the value of dwFlags.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr MonitorFromRect(ref RECT lprc, uint dwFlags);

        /// <summary>
        /// Retrieves information about a display monitor.
        /// </summary>
        /// <param name="hMonitor">A handle to the display monitor of interest.</param>
        /// <param name="lpmi">A pointer to a MONITORINFO or MONITORINFOEX structure that receives information about the specified display monitor.</param>
        /// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        /// <summary>
        /// Retrieves a handle to the foreground window (the window with which the user is currently working). The system assigns a slightly higher priority to the thread that creates the foreground window than it does to other threads.
        /// </summary>
        /// <returns>The return value is a handle to the foreground window. The foreground window can be NULL in certain circumstances, such as when a window is losing activation.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetForegroundWindow();

        /// <summary>
        /// Extends the window frame behind the client area.
        /// </summary>
        /// <param name="hWnd">The handle to the window in which the frame will be extended into the client area.</param>
        /// <param name="pMarInset">A pointer to a MARGINS structure that describes the margins to use when extending the frame into the client area.</param>
        /// <returns>If the method succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.</returns>
        [DllImport("DwmApi.dll", SetLastError = true)]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

        /// <summary>
        /// Calls the default window procedure to provide default processing for any window messages that an application does not process. This function ensures that every message is processed. DefWindowProc is called with the same parameters received by the window procedure.
        /// </summary>
        /// <param name="hWnd">A handle to the window procedure that received the message.</param>
        /// <param name="Msg">The message.</param>
        /// <param name="wParam">Additional message information. The content of this parameter depends on the value of the Msg parameter (wParam).</param>
        /// <param name="lParam">Additional message information. The content of this parameter depends on the value of the Msg parameter (lParam).</param>
        /// <returns>The return value is the result of the message processing and depends on the message.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// Checks whether the cursor is over the current window's client area.
        /// </summary>
        /// <param name="hWnd">Handle of the window to check.</param>
        /// <param name="wParam">Additional message information. The content of this parameter depends on the value of the Msg parameter (wParam).</param>
        /// <param name="lParam">Additional message information. The content of this parameter depends on the value of the Msg parameter (lParam).</param>
        /// <returns>True if the cursor is over the client area, false if not.</returns>
        public static bool IsOverClientArea(IntPtr hWnd, IntPtr wParam, IntPtr lParam)
        {
            IntPtr uHitTest = DefWindowProc(hWnd, WM_NCHITTEST, wParam, lParam);
            if (uHitTest.ToInt32() == HTCLIENT) // check if we're over the client area
                return true;
            return false;
        }

        /// <summary>
        /// Retrieves the specified system metric or system configuration setting.
        /// Note that all dimensions retrieved by GetSystemMetrics are in pixels.
        /// </summary>
        /// <param name="smIndex">The system metric or configuration setting to be retrieved. Note that all SM_CX* values are widths and all SM_CY* values are heights. Also note that all settings designed to return Boolean data represent TRUE as any nonzero value, and FALSE as a zero value.</param>
        /// <returns>If the function succeeds, the return value is the requested system metric or configuration setting.
        /// If the function fails, the return value is 0. GetLastError does not provide extended error information.</returns>
        [DllImport("user32.dll", SetLastError = false)]
        public static extern int GetSystemMetrics(SystemMetric smIndex);

        /// <summary>
        /// Obtains a value that indicates whether Desktop Window Manager (DWM) composition is enabled. Applications can listen for composition state changes by handling the WM_DWMCOMPOSITIONCHANGED notification.
        /// </summary>
        /// <param name="enabled">A pointer to a value that, when this function returns successfully, receives TRUE if DWM composition is enabled; otherwise, FALSE.</param>
        /// <returns>If the method succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.</returns>
        [DllImport("dwmapi.dll")]
        public static extern int DwmIsCompositionEnabled(out bool enabled);

        /// <summary>
        /// Contains information used by Shell_NotifyIconGetRectangle to identify the icon for which to retrieve the bounding rectangle.
        /// </summary>
        /// <remarks>
        /// The icon can be identified to Shell_NotifyIconGetRectangle through this structure in two ways: 
        ///    guidItem alone (recommended)
        ///    hWnd plus uID
        /// If guidItem is used, hWnd and uID are ignored.
        /// </remarks>
        public struct NOTIFYICONIDENTIFIER
        {
            /// <summary>
            /// Size of this structure, in bytes. 
            /// </summary>
            public uint cbSize;

            /// <summary>
            /// A handle to the parent window used by the notification's callback function. For more information, see the hWnd member of the NOTIFYICONDATA structure.
            /// </summary>
            public IntPtr hWnd;

            /// <summary>
            /// The application-defined identifier of the notification icon. Multiple icons can be associated with a single hWnd, each with their own uID.
            /// </summary>
            public uint uID;

            /// <summary>
            /// A registered GUID that identifies the icon.
            /// </summary>
            public Guid guidItem;
        }

        /// <summary>
        /// Defines the coordinates of the upper-left and lower-right corners of a rectangle.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            /// <summary>
            /// The x-coordinate of the upper-left corner of the rectangle.
            /// </summary>
            public int left;

            /// <summary>
            /// The y-coordinate of the upper-left corner of the rectangle.
            /// </summary>
            public int top;

            /// <summary>
            /// The x-coordinate of the lower-right corner of the rectangle.
            /// </summary>
            public int right;

            /// <summary>
            /// The y-coordinate of the lower-right corner of the rectangle.
            /// </summary>
            public int bottom;

            /// <summary>
            /// Returns whether the rectangle does not have a zero width or height.
            /// </summary>
            /// <returns>Whether the rectangle does not have a zero width or height.</returns>
            public bool HasSize()
            {
                return right - left > 0 && bottom - top > 0;
            }

            /// <summary>
            /// Converts a RECT structure to an equivalent System.Windows.Rectangle structure. Returns a 0-width rectangle if the calculated width or height is negative.
            /// </summary>
            /// <param name="rect">The RECT to convert.</param>
            /// <returns>The equivalent System.Windows.Rectangle.</returns>
            public static implicit operator Rectangle(RECT rect)
            {
                // return a 0-width rectangle if the width or height is negative
                if (rect.right - rect.left < 0 || rect.bottom - rect.top < 0)
                    return new Rectangle(rect.left, rect.top, 0, 0);
                return new Rectangle(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
            }

            /// <summary>
            /// Converts a RECT structure equivalent to the specified System.Windows.Rectangle. Double precision is lost.
            /// </summary>
            /// <param name="rect">The System.Windows.Rectangle to convert.</param>
            /// <returns>The equivalent RECT structure.</returns>
            public static implicit operator RECT(Rectangle rect)
            {
                return new RECT()
                {
                    left = (int)rect.Left,
                    top = (int)rect.Top,
                    right = (int)rect.Right,
                    bottom = (int)rect.Bottom
                };
            }
        }

        /// <summary>
        /// The POINT structure defines the x- and y- coordinates of a point.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            /// <summary>
            /// The x-coordinate of the point.
            /// </summary>
            public int x;

            /// <summary>
            /// The y-coordinate of the point.
            /// </summary>
            public int y;

            /// <summary>
            /// Converts a POINT to a System.Windows.Point.
            /// </summary>
            /// <param name="point">The POINT structure to convert.</param>
            /// <returns>The equivalent System.Windows.Point.</returns>
            public static implicit operator Point(POINT point)
            {
                return new Point(point.x, point.y);
            }
        }

        /// <summary>
        /// Contains information about a button in a toolbar.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct TBBUTTON
        {
            /// <summary>
            /// Zero-based index of the button image. Set this member to I_IMAGECALLBACK, and the toolbar will send the TBN_GETDISPINFO notification code to retrieve the image index when it is needed.
            /// </summary>
            public int iBitmap;

            /// <summary>
            /// Command identifier associated with the button. This identifier is used in a WM_COMMAND message when the button is chosen.
            /// </summary>
            public int idCommand;

            /// <summary>
            /// Contains the fsState and fsStyle fields (bytes) and padding (2 bytes on 32-bit systems, 6 bytes on 64-bit systems).
            /// Access the fields fsState and fsStyle to retrieve this information.
            /// </summary>
            public IntPtr fsStateStylePadding;

            /// <summary>
            /// Application-defined value.
            /// </summary>
            public IntPtr dwData;

            /// <summary>
            /// Zero-based index of the button string, or a pointer to a string buffer that contains text for the button.
            /// </summary>
            public IntPtr iString;

            /// <summary>
            /// Gets button state flags. This member can be a combination of the values listed in Toolbar Button States.
            /// </summary>
            public byte fsState
            {
                get
                {
                    if (IntPtr.Size == 8)
                        return BitConverter.GetBytes(this.fsStateStylePadding.ToInt64())[0];
                    else
                        return BitConverter.GetBytes(this.fsStateStylePadding.ToInt32())[0];
                }
            }

            /// <summary>
            /// Gets button style. This member can be a combination of the button style values listed in Toolbar Control and Button Styles.
            /// </summary>
            public byte fsStyle
            {
                get
                {
                    if (IntPtr.Size == 8)
                        return BitConverter.GetBytes(this.fsStateStylePadding.ToInt64())[1];
                    else
                        return BitConverter.GetBytes(this.fsStateStylePadding.ToInt32())[1];
                }
            }
        }

        /// <summary>
        /// Contains information about a system appbar message. This structure is used with the SHAppBarMessage function.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct APPBARDATA
        {
            /// <summary>
            /// The size of the structure, in bytes.
            /// </summary>
            public uint cbSize;

            /// <summary>
            /// The handle to the appbar window.
            /// </summary>
            public IntPtr hWnd;

            /// <summary>
            /// An application-defined message identifier. The application uses the specified identifier for notification messages that it sends to the appbar identified by the hWnd member. This member is used when sending the ABM_NEW message.
            /// </summary>
            public uint uCallbackMessage;

            /// <summary>
            /// A value that specifies an edge of the screen. This member is used when sending the ABM_GETAUTOHIDEBAR, ABM_QUERYPOS, ABM_SETAUTOHIDEBAR, and ABM_SETPOS messages. This member can be one of the following values.
            /// </summary>
            public ABEdge uEdge;

            /// <summary>
            /// A RECT structure to contain the bounding rectangle, in screen coordinates, of an appbar or the Windows taskbar. This member is used when sending the ABM_GETTASKBARPOS, ABM_QUERYPOS, and ABM_SETPOS messages.
            /// </summary>
            public RECT rc;

            /// <summary>
            /// A message-dependent value. This member is used with the ABM_SETAUTOHIDEBAR and ABM_SETSTATE messages.
            /// </summary>
            public IntPtr lParam;
        }

        /// <summary>
        /// Contains information about a display monitor.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct MONITORINFO
        {
            /// <summary>
            /// The size of the structure, in bytes.
            /// </summary>
            public uint cbSize;

            /// <summary>
            /// A RECT structure that specifies the display monitor rectangle, expressed in virtual-screen coordinates. Note that if the monitor is not the primary display monitor, some of the rectangle's coordinates may be negative values.
            /// </summary>
            public RECT rcMonitor;

            /// <summary>
            /// A RECT structure that specifies the work area rectangle of the display monitor, expressed in virtual-screen coordinates. Note that if the monitor is not the primary display monitor, some of the rectangle's coordinates may be negative values.
            /// </summary>
            public RECT rcWork;

            /// <summary>
            /// A set of flags that represent attributes of the display monitor.
            /// </summary>
            public uint dwFlags;
        }

        /// <summary>
        /// Returned by the GetThemeMargins function to define the margins of windows that have visual styles applied.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct MARGINS
        {
            /// <summary>
            /// Width of the left border that retains its size.
            /// </summary>
            public int cxLeftWidth;

            /// <summary>
            /// Width of the right border that retains its size.
            /// </summary>
            public int cxRightWidth;

            /// <summary>
            /// Height of the top border that retains its size.
            /// </summary>
            public int cyTopHeight;

            /// <summary>
            /// Height of the bottom border that retains its size.
            /// </summary>
            public int cyBottomHeight;
        }
    }
}