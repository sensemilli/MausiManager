using System;

namespace WiCAM.Pn4000.JobManager
{
    public enum MacroEventType { MouseMove, MouseDown, MouseUp, KeyDown, KeyUp }

    public class MacroStep
    {
        public MacroEventType EventType { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int MouseButton { get; set; } // 0=left
        public int VirtualKey { get; set; } // for keys
        public TimeSpan DelayFromPrevious { get; set; }
        public override string ToString() => $"{EventType} ({X},{Y}) vk={VirtualKey} dt={DelayFromPrevious.TotalMilliseconds}ms";
    }
}