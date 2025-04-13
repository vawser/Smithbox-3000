using Silk.NET.SDL;

namespace Smithbox_Core
{
    using Silk.NET.SDL;
    using Smithbox.Core.Interface;
    using Smithbox.Core.Utils;
    using System;
    using System.Text.Json;

    public class ClosingEventArgs : EventArgs
    {
        public bool Handled { get; set; }
    }

    public class ClosedEventArgs : EventArgs
    {
    }

    public unsafe class CoreWindow : IDisposable
    {
        protected static readonly Sdl sdl = App.sdl;
        private Window* window;
        private uint id;
        private int width;
        private int height;
        private bool disposedValue;

        private WindowState WindowState;

        public CoreWindow()
        {
            WindowState = LoadWindowState();

            WindowFlags flags = WindowFlags.Resizable | WindowFlags.Hidden | WindowFlags.AllowHighdpi;

            switch (App.Backend)
            {
                case Backend.OpenGL:
                    flags |= WindowFlags.Opengl;
                    break;

                case Backend.Vulkan:
                    flags |= WindowFlags.Vulkan;
                    break;
            }

            window = sdl.CreateWindow("", WindowState.X, WindowState.Y, WindowState.Width, WindowState.Height, (uint)flags);
            id = sdl.GetWindowID(window);
        }

        public uint Id => id;

        public int Width => width;

        public int Height => height;

        public Window* SDLWindow => window;

        public event EventHandler<ResizedEventArgs>? Resized;

        public event EventHandler<ClosingEventArgs>? Closing;

        public event EventHandler<ClosedEventArgs>? Closed;

        public void Show()
        {
            sdl.ShowWindow(window);
        }

        internal void Destroy()
        {
            if (window != null)
            {
                sdl.DestroyWindow(window);
                window = null;
                id = 0;
            }
        }

        internal void ProcessWindowEvent(WindowEvent windowEvent)
        {
            switch ((WindowEventID)windowEvent.Event)
            {
                case WindowEventID.Resized:
                    var oldWidth = this.width;
                    var oldHeight = this.height;
                    int width = windowEvent.Data1;
                    int height = windowEvent.Data2;
                    var resizedEventArgs = new ResizedEventArgs(width, height, oldWidth, oldHeight);
                    this.width = width;
                    this.height = height;
                    OnResized(resizedEventArgs);
                    if (!resizedEventArgs.Handled)
                    {
                        Resized?.Invoke(this, resizedEventArgs);
                    }
                    else
                    {
                        sdl.SetWindowSize(window, oldWidth, oldHeight);
                        this.width = oldWidth;
                        this.height = oldHeight;
                    }

                    break;

                case WindowEventID.Close:
                    SaveWindowState(sdl, window);
                    ClosingEventArgs eventArgs = new();
                    Closing?.Invoke(this, eventArgs);
                    if (eventArgs.Handled)
                    {
                        return;
                    }
                    Dispose();
                    Closed?.Invoke(this, new());
                    break;
            }
        }

        protected virtual void OnResized(ResizedEventArgs resizedEventArgs)
        {
        }

        public virtual void InitGraphics()
        {
        }

        public virtual void Render()
        {
            DPI.UpdateDpi(window);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Destroy();

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private unsafe void SaveWindowState(Sdl sdl, Window* window)
        {
            int width, height, x, y;

            sdl.GetWindowSize(window, &width, &height);
            sdl.GetWindowPosition(window, &x, &y);

            WindowState state = new();

            state.Width = width;
            state.Height = height;
            state.X = x;
            state.Y = y;
            state.IsMaximized = (sdl.GetWindowFlags(window) & (uint)WindowFlags.WindowMaximized) != 0;

            string json = JsonSerializer.Serialize(state, SmithboxSerializerContext.Default.WindowState);

            File.WriteAllText(@$"{Consts.ConfigurationFolder}\Program.json", json);
        }

        private WindowState LoadWindowState()
        {
            var windowState = new WindowState();
            var configPath = @$"{AppContext.BaseDirectory}\{Consts.ConfigurationFolder}\Program.json";

            if (File.Exists(configPath))
            {
                try
                {
                    string json = File.ReadAllText(configPath);
                    windowState = JsonSerializer.Deserialize(json, SmithboxSerializerContext.Default.WindowState);
                }
                catch
                {
                    windowState = new WindowState();
                    windowState.Width = 800;
                    windowState.Height = 600;
                    windowState.X = Sdl.WindowposCentered;
                    windowState.Y = Sdl.WindowposCentered;
                    windowState.IsMaximized = false;
                }
            }

            return windowState;
        }
    }
}

public class WindowState
{
    public int Width { get; set; } = 800;
    public int Height { get; set; } = 600;
    public int X { get; set; } = Sdl.WindowposCentered;
    public int Y { get; set; } = Sdl.WindowposCentered;
    public bool IsMaximized { get; set; } = false;
}