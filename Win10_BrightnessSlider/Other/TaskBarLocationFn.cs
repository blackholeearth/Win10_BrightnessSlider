using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Win10_BrightnessSlider
{
    public static class TaskBarLocationFn
    {
        //GuiBehaviour
        public enum TaskBarLocation { TOP, BOTTOM, LEFT, RIGHT }
        public static TaskBarLocation GetTaskBarLocation()
        {
            TaskBarLocation taskBarLocation = TaskBarLocation.BOTTOM;
            bool taskBarOnTopOrBottom = (Screen.PrimaryScreen.WorkingArea.Width == Screen.PrimaryScreen.Bounds.Width);
            if (taskBarOnTopOrBottom)
            {
                if (Screen.PrimaryScreen.WorkingArea.Top > 0)
                    taskBarLocation = TaskBarLocation.TOP;
                else
                    taskBarLocation = TaskBarLocation.BOTTOM;
            }
            else
            {
                if (Screen.PrimaryScreen.WorkingArea.Left > 0)
                {
                    taskBarLocation = TaskBarLocation.LEFT;
                }
                else
                {
                    taskBarLocation = TaskBarLocation.RIGHT;
                }
            }
            return taskBarLocation;
        }

        /// <summary>
        /// doesnt work well with autohide
        /// considers  RTL , PrimaryScreen ,
        /// </summary>
        /// <returns></returns>
        public static Point calc_FormLoc_NearToTaskbarDate(Size formSize)
        {
            Point pvis = new Point(0, 0);
            Rectangle scrWA = GetScreen_WorkingArea();

            var taskbarloc = GetTaskBarLocation();

            //tr eng: false
            //arabic: true
            var rtl = System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft;
            //rtl = true;//testitngggg
            if (!rtl)
            {
                if (taskbarloc == TaskBarLocation.TOP)
                    pvis = new Point(scrWA.Width - formSize.Width, scrWA.Top);
                else if (taskbarloc == TaskBarLocation.LEFT)
                    pvis = new Point(scrWA.Left, scrWA.Height - formSize.Height);
                else if (taskbarloc == TaskBarLocation.BOTTOM)
                    pvis = new Point(scrWA.Width - formSize.Width, scrWA.Height - formSize.Height);
                else if (taskbarloc == TaskBarLocation.RIGHT)
                    pvis = new Point(scrWA.Width - formSize.Width, scrWA.Height - formSize.Height);

            }
            else
            {
                if (taskbarloc == TaskBarLocation.TOP)
                    pvis = new Point(0, scrWA.Top);
                else if (taskbarloc == TaskBarLocation.LEFT)
                    pvis = new Point(scrWA.Left, 0);
                else if (taskbarloc == TaskBarLocation.BOTTOM)
                    pvis = new Point(0, scrWA.Height - formSize.Height);
                else if (taskbarloc == TaskBarLocation.RIGHT)
                    pvis = new Point(scrWA.Right - formSize.Width, scrWA.Top);
            }

            return pvis;
        }

        public static Rectangle offset_FormLoc_for_RoundCorners( Rectangle formRect, bool is_RoundCorners )
        {
            if (!is_RoundCorners)
                return formRect;

            var taskbarloc = GetTaskBarLocation();
            var scrWA = GetScreen_WorkingArea();

            var padSize = 16;
            var inflateVal= (int) -padSize / 2;

            scrWA.Inflate(inflateVal, inflateVal);
            //var formRect = new Rectangle(pMouseLoc, formSize);
            var clampedRect = formRect.Clamp(scrWA);
           
            return clampedRect;
        }

        public static Rectangle offset_FormLoc_for_RoundCorners_CalculateAutoHide(Rectangle formRect, bool is_RoundCorners)
        {
            if (!is_RoundCorners)
                return formRect;

            var taskbarloc = GetTaskBarLocation();
            var scrWA = GetScreen_WA_CalculateAutoHide();

            var padSize = 16;
            var inflateVal = (int)-padSize / 2;

            scrWA.Inflate(inflateVal, inflateVal);
            //var formRect = new Rectangle(pMouseLoc, formSize);
            var clampedRect = formRect.Clamp(scrWA);

            return clampedRect;
        }


        /// <summary>
        ///  by Cursor.Position 
        /// </summary>
        private static Screen GetScreen()
        {
            var scr = Screen.PrimaryScreen;
            if (true)
                scr = Screen.FromPoint(Cursor.Position);
            return scr;
        }

        private static Rectangle GetScreen_WorkingArea()
        {
            var scr = Screen.PrimaryScreen;
            if (true)
                scr = Screen.FromPoint(Cursor.Position);
            return scr.WorkingArea;
        }

        /// <summary>
        /// by Cursor.Position  ,,,,,for AutoHide only bottom , right is calculated.
        /// </summary>
        /// <returns></returns>
        private static Rectangle GetScreen_WA_CalculateAutoHide()
        {
            var scrWA = GetScreen_WorkingArea();

            //var taskbarVis = IsTaskbarVisible();
            return Calculate_AutoHide(scrWA);

        }

        /// <summary>
        /// only bottom , right is calculated.
        /// </summary>
        /// <returns></returns>
        private static Rectangle Calculate_AutoHide(this Rectangle scrWA)
        {
            int TaskBar_H = 0;
            int TaskBar_W = 0;
            try
            {
                // kısa kenar, aradığımız eksendir . yani o eksen, eni oluyor. 
                var taskBarRect = TaskBarUtil.GetTaskbarPosition();

                if (taskBarRect.Width > taskBarRect.Height)
                    TaskBar_H = (int)(taskBarRect.Height * 1);
                else
                    TaskBar_W = (int)(taskBarRect.Width * 1);
            }
            catch (Exception ex) 
            { RamLogger.Log("TaskBarUtil.GetTaskbarPosition(): Ex:" + ex); }

            scrWA.Width -= TaskBar_W;
            scrWA.Height -= TaskBar_H;

            return scrWA;
        }


        /// <summary>
        /// user will click icon in the taskbar.
        /// </summary>
        /// <param name="formSize"></param>
        public static Point calc_FormLoc_NearToTaskbarDate_viaMouseLocation(Size formSize )
        {
            var pMouseLoc = Cursor.Position;
            pMouseLoc = PushPointToNearestCornerOfRect(pMouseLoc, GetScreen().Bounds);

            //var taskbarVis = IsTaskbarVisible();
            //var scrWA = GetScreen().WorkingArea;
            var scrWA = GetScreen_WA_CalculateAutoHide();

            var formRect = new Rectangle(pMouseLoc, formSize);
            var clampedRect = formRect.Clamp(scrWA);

            return clampedRect.Location;
        }


        /// <summary>
        /// used for   ploc : curser.pos ,  rect : scr.primaryScreen.Bounds
        /// </summary>
        /// <param name="pLoc"></param>
        /// <param name="rect"></param>
        /// <returns></returns>
        private static Point PushPointToNearestCornerOfRect(Point pLoc, Rectangle rect)
        {
            var psbWidthCenter = rect.Width * 0.5;
            var psbHeightCenter = rect.Height * 0.5;

            bool CloserToLeft = (pLoc.X < psbWidthCenter);
            bool CloserToTop = (pLoc.Y < psbHeightCenter);

            //push mouseLoc to Closest corner
            pLoc.X = CloserToLeft ? rect.X : rect.Width;
            pLoc.Y = CloserToTop ? rect.Y : rect.Height;

            return pLoc;
        }

        /// <summary>
        /// clamp via pushing rectange back inside larger rectangle
        /// </summary>
        /// <param name="smaller"></param>
        /// <param name="larger"></param>
        /// <returns></returns>
        public static Rectangle Clamp(this Rectangle smaller, Rectangle larger)
        {
            Rectangle ret = new Rectangle();
            ret.Width = smaller.Width;
            ret.Height = smaller.Height;
            ret.X = smaller.X;
            ret.Y = smaller.Y;

            if (smaller.X + smaller.Width > larger.X + larger.Width)
            {
                ret.X = larger.Width - smaller.Width;
            }
            if (smaller.X < larger.X)
            {
                ret.X = larger.X;
            }

            if (smaller.Y + smaller.Height > larger.Y + larger.Height)
            {
                ret.Y = larger.Height - smaller.Height;
            }
            if (smaller.Y < larger.Y)
            {
                ret.Y = larger.Y;
            }

            return ret;
        }

        public static bool IsTaskbarVisible()
        {
            var scrPS = Screen.PrimaryScreen;
            var taskbar_Hidden =
            scrPS.Bounds.Height == scrPS.WorkingArea.Height
            && scrPS.Bounds.Width == scrPS.WorkingArea.Width;

            // Console.WriteLine(scrPS.Bounds.Height +",(h),"+ scrPS.WorkingArea.Height);
            // Console.WriteLine(scrPS.Bounds.Width + ",(w)," + scrPS.WorkingArea.Width);

            return !taskbar_Hidden;
        }




        /// <summary>
        /// screen is found by cursor.Position
        /// </summary>
        /// <returns></returns>
        public static Rectangle Clamp_Into_ScreenWorkingArea(Rectangle rectx)
        {
            //var taskbarVis = IsTaskbarVisible();
            var scrWA = GetScreen_WorkingArea();
            var clampedRect = rectx.Clamp(scrWA);
            return clampedRect;
        }
        public static Rectangle ClampPoint_Into_ScreenWorkingArea(Point location)
        {
            //var taskbarVis = IsTaskbarVisible();
            var scrWA = GetScreen_WorkingArea();
            var clampedRect = new Rectangle(location, new Size(0, 0)).Clamp(scrWA);
            return clampedRect;
        }


    }
}