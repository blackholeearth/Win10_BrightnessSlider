using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gma.System.MouseKeyHook;
using System.Windows.Forms;

namespace Win10_BrightnessSlider.Other
{
    /// <summary>
    /// global mouse key hook - GlobalMouseKeyHook example
    /// </summary>
    public class GlobalMouseKeyHook_example
    {
        private IKeyboardMouseEvents m_GlobalHook;
        public void Subscribe()
        {
            // Note: for the application hook, use the Hook.AppEvents() instead
            m_GlobalHook = Hook.GlobalEvents();

            m_GlobalHook.MouseDownExt += GlobalHookMouseDownExt;
            //m_GlobalHook.KeyPress += GlobalHookKeyPress;
        }
        private void GlobalHookKeyPress(object sender, KeyPressEventArgs e)
        {
            Console.WriteLine("KeyPress: \t{0}", e.KeyChar);
        }
        private void GlobalHookMouseDownExt(object sender, MouseEventExtArgs e)
        {
            Console.WriteLine("MouseDown: \t{0}; \t System Timestamp: \t{1}", e.Button, e.Timestamp);

            // uncommenting the following line will suppress the middle mouse button click
            // if (e.Buttons == MouseButtons.Middle) { e.Handled = true; }

        }

        public void Unsubscribe()
        {
            m_GlobalHook.MouseDownExt -= GlobalHookMouseDownExt;
            //m_GlobalHook.KeyPress -= GlobalHookKeyPress;

            //It is recommened to dispose it
            m_GlobalHook.Dispose();
        }
    }




}
