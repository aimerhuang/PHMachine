using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Interop;

namespace Freedom.ZHPHMachine
{
    /// <summary>
    /// 置顶帮助类
    /// </summary>
    public class TopMostTool
    {
        public static int SW_SHOW = 5;
        public static int SW_NORMAL = 1;
        public static int SW_MAX = 3;
        public static int SW_HIDE = 0;
        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);    //窗体置顶
        public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);    //取消窗体置顶
        public const uint SWP_NOMOVE = 0x0002;    //不调整窗体位置
        public const uint SWP_NOSIZE = 0x0001;    //不调整窗体大小
        public bool isFirst = true;

        private static Window _fullWindow;
        private static WindowState _windowState;
        private static WindowStyle _windowStyle;
        private static bool _windowTopMost;
        private static ResizeMode _windowResizeMode;
        private static Rect _windowRect;


        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", EntryPoint = "ShowWindow")]
        public static extern bool ShowWindow(System.IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        /// <summary>
        /// 查找子窗口
        /// </summary>
        /// <param name="hwndParent"></param>
        /// <param name="hwndChildAfter"></param>
        /// <param name="lpClassName"></param>
        /// <param name="lpWindowName"></param>
        /// <returns></returns>
        [DllImport("User32.dll", EntryPoint = "FindWindowEx")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpClassName, string lpWindowName);

        /// <summary>
        /// 窗体置顶，可以根据需要传入不同的值(需要置顶的窗体的名字Title)
        /// </summary>
        public static void SetTopWindow()
        {
            IntPtr frm = FindWindow(null, "MainWindow");    // 程序中需要置顶的窗体的名字
            if (frm != IntPtr.Zero)
            {
                SetWindowPos(frm, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);

                var child = FindWindowEx(frm, IntPtr.Zero, null, "MainWindow");
            }

        }

        public static void SetTopmost(IntPtr handle)
        {
            SetWindowPos(handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
        }


        /// <summary>
        /// 进入全屏    
        /// </summary>
        /// <param name="window"></param>
        public static void GoFullscreen(Window window)
        {
            //已经是全屏 
            //if (window.Topmost && window.WindowState == WindowState.Maximized) return;

            //存储窗体信息
            _windowState = window.WindowState;
            _windowStyle = window.WindowStyle;
            _windowTopMost = window.Topmost;
            _windowResizeMode = window.ResizeMode;
            _windowRect.X = window.Left;
            _windowRect.Y = window.Top;
            _windowRect.Width = window.Width;
            _windowRect.Height = window.Height;


            //变成无边窗体 
            window.WindowState = WindowState.Normal;//假如已经是Maximized，就不能进入全屏，所以这里先调整状态 
            window.WindowStyle = WindowStyle.None;
            window.ResizeMode = ResizeMode.NoResize;
            window.Topmost = true;//最大化后总是在最上面 

            //获取窗口句柄 
            var handle = new WindowInteropHelper(window).Handle;
            //获取当前显示器屏幕
            Screen screen = Screen.FromHandle(handle);

            //调整窗口最大化,全屏的关键代码就是下面3句 
            window.MaxWidth = screen.Bounds.Width;
            window.MaxHeight = screen.Bounds.Height;
            window.WindowState = WindowState.Maximized;

            //解决切换应用程序的问题
            window.Activated += new EventHandler(window_Activated);
            window.Deactivated += new EventHandler(window_Deactivated);
            //记住成功最大化的窗体 
            _fullWindow = window;
        }

        static void window_Deactivated(object sender, EventArgs e)
        {
            var window = sender as Window;
            window.Topmost = false;
        }
        static void window_Activated(object sender, EventArgs e)
        {
            var window = sender as Window;
            window.Topmost = true;
        }
    }
}
