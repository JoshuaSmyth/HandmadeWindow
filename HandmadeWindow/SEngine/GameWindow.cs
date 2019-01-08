using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Win32Hello.SEngine;
using static Win32Hello.SEngine.Win32;

namespace HandmadeWindow.SEngine
{
    public struct OffscreenBuffer
    {
        public BITMAPINFO Info;
        public int Width;
        public int Height;
        public int bpp;
        public int pitch;
        public byte[] Memory;   // should this really be an Int32?

        public void Clear()
        {
            // TODO Pass in a colour
            // Note: Buffer.BlockCopy may be faster?
            for(int y = 0; y < Memory.Length; y++)
            {
                Memory[y] = 0;
            }
        }
    };

    struct WindowDimensions
    {
        public int Width;
        public int Height;
    }

    public class GameWindow
    {
        static string AppName = "Default Game Window";
        static string ClassName = "GameWindowClass";
        static IntPtr hWnd;

        static Int32 WindowWidth = 800;
        static Int32 WindowHeight = 600;

        static bool IsRunning = true;
        static OffscreenBuffer GlobalOffscreenBuffer;

        int xOffset = 0;
        int yOffset = 0;

        public void Init(String title, Int32 width, Int32 height)
        {
            AppName = title;
            WindowWidth = width;
            WindowHeight = height;
        }

        public virtual void Update()
        {
            // Noop
        }

        public virtual void Render(OffscreenBuffer buffer)
        {
            // Noop
        }

        public void Show()
        {
            if(RegisterClass() == 0)
            {
                return;
            }

            if(Create() == 0)
            {
                return;
            }

            GlobalOffscreenBuffer = CreateBackBuffer(WindowWidth, WindowHeight);
            var sw = Stopwatch.StartNew();
            var frameCount = 0;
            var fps = 0.0f;
            Win32.MSG Msg = new Win32.MSG();
            while(IsRunning)
            {
                // TODO Frame Rate Limiting
                while(Win32.PeekMessage(out Msg, hWnd, 0, 0, 1))
                {
                    if(Msg.message == WM_QUIT)
                    {
                        IsRunning = false;
                    }

                    // Update Messages
                    {
                        Win32.TranslateMessage(ref Msg);
                        Win32.DispatchMessage(ref Msg);
                    }
                }

                // Render
                {
                    Update();
                    Render(GlobalOffscreenBuffer);
                    
                    // Paint
                    IntPtr hDC = Win32.GetDC(hWnd);
                    var dimensions = GetWindowDimensions(hWnd);

                    Win32.StretchDIBits(hDC, 0, 0, WindowWidth, WindowHeight, 0, 0, dimensions.Width, GlobalOffscreenBuffer.Height, GlobalOffscreenBuffer.Memory, ref GlobalOffscreenBuffer.Info, 0, (uint)TernaryRasterOperations.SRCCOPY);
                    ReleaseDC(hWnd, hDC);
                }

                frameCount++;
                fps = frameCount / (sw.ElapsedMilliseconds * 0.001f);

                // TODO Do some sleeping to maintain fps limits
            }
        }

        private static OffscreenBuffer CreateBackBuffer(int width, int height)
        {
            var buffer = new OffscreenBuffer();

            buffer.Info.Init();     // Populates BiSize
            buffer.Info.biWidth = width;
            buffer.Info.biHeight = -height;    // TopDown
            buffer.Info.biPlanes = 1;
            buffer.Info.biBitCount = 32;
            buffer.Info.biCompression = BitmapCompressionMode.BI_RGB;
            buffer.Info.biXPelsPerMeter = 0;
            buffer.Info.biYPelsPerMeter = 0;
            buffer.Info.biClrUsed = 0;
            buffer.Info.biClrImportant = 0;

            buffer.Width = width;
            buffer.Height = height;
            buffer.bpp = 4;
            buffer.pitch = buffer.Width * buffer.bpp;

            buffer.Memory = new byte[buffer.Width * buffer.Height * buffer.bpp];

            return buffer;
        }


        private static WindowDimensions GetWindowDimensions(IntPtr window)
        {
            WindowDimensions result;

            RECT clientRect;
            Win32.GetClientRect(window, out clientRect);

            result.Width = clientRect.right - clientRect.left;
            result.Height = clientRect.bottom - clientRect.top;

            return result;
        }

        private static int RegisterClass()
        {
            Win32.WNDCLASSEX wcex = new Win32.WNDCLASSEX
            {
                style = Win32.ClassStyles.DoubleClicks
            };

            wcex.cbSize = (uint)Marshal.SizeOf(wcex);
            wcex.lpfnWndProc = WndProc;
            wcex.cbClsExtra = 0;
            wcex.cbWndExtra = 0;
            wcex.hIcon = Win32.LoadIcon(IntPtr.Zero, (IntPtr)Win32.IDI_APPLICATION);
            wcex.hCursor = Win32.LoadCursor(IntPtr.Zero, (int)Win32.IDC_ARROW);
            wcex.hIconSm = IntPtr.Zero;
            wcex.hbrBackground = (IntPtr)(Win32.COLOR_WINDOW + 1);
            wcex.lpszMenuName = null;
            wcex.lpszClassName = ClassName;

            if(Win32.RegisterClassEx(ref wcex) == 0)
            {
                Win32.MessageBox(IntPtr.Zero, "RegisterClassEx failed", AppName, (int)(Win32.MB_OK | Win32.MB_ICONEXCLAMATION | Win32.MB_SETFOREGROUND));
                return 0;
            }
            return 1;
        }

        private static int Create()
        {
            hWnd = Win32.CreateWindowEx(0, ClassName, AppName, WS_OVERLAPPED | WS_SYSMENU | Win32.WS_VISIBLE | Win32.WS_MINIMIZEBOX, 250, 250, WindowWidth, WindowHeight, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            if(hWnd != IntPtr.Zero)
            {
                return 1;
            }

            Win32.MessageBox(IntPtr.Zero, "CreateWindow failed", AppName, (int)(Win32.MB_OK | Win32.MB_ICONEXCLAMATION | Win32.MB_SETFOREGROUND));
            return 0;
        }

        private static IntPtr WndProc(IntPtr hWnd, uint message, IntPtr wParam, IntPtr lParam)
        {
            switch(message)
            {
                case Win32.WM_SIZE:
                    {
                        return IntPtr.Zero;
                    }
                case Win32.WM_PAINT:
                    {
                        Win32.PAINTSTRUCT ps = new Win32.PAINTSTRUCT();

                        IntPtr hDC = Win32.BeginPaint(hWnd, out ps);
                        var dimensions = GetWindowDimensions(hWnd);
                        Win32.StretchDIBits(hDC, 0, 0, WindowWidth, WindowHeight, 0, 0, dimensions.Width, GlobalOffscreenBuffer.Height, GlobalOffscreenBuffer.Memory, ref GlobalOffscreenBuffer.Info, 0, (uint)TernaryRasterOperations.SRCCOPY);
                        Win32.EndPaint(hWnd, ref ps);

                        return IntPtr.Zero;
                    }
                case Win32.WM_DESTROY:
                    {
                        IsRunning = false;
                        Win32.PostQuitMessage(0);
                        return IntPtr.Zero;
                    }
                default:
                    {
                        return Win32.DefWindowProc(hWnd, message, wParam, lParam);
                    }
            }
        }
    }
}
