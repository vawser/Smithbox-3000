﻿namespace Smithbox.Core.Interface.Input.Events
{
    using Smithbox.Core.Interface.Input;

    public class MouseButtonEventArgs : EventArgs
    {
        public MouseButton Button { get; internal set; }

        public MouseButtonState State { get; internal set; }

        public int Clicks { get; internal set; }
    }
}