using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Runtime.InteropServices;

namespace WiCAM.Pn4000.JobManager
{


    public class MacroRecorder
    {
        public ObservableCollection<MacroStep> Steps { get; } = new ObservableCollection<MacroStep>();
        private Stopwatch _sw;
        private bool _recording;
        public bool Loop { get; set; }

        // Events for error + status reporting
        public event Action<Exception>? ErrorOccurred;
        public event Action<string>? StatusChanged;

        // PInvoke for SendInput (simple simulation)
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT { public int type; public MOUSEKEYBDHARDWARE mkhi; }
        [StructLayout(LayoutKind.Explicit)]
        private struct MOUSEKEYBDHARDWARE { [FieldOffset(0)] public MOUSEINPUT mi; [FieldOffset(0)] public KEYBDINPUT ki; }
        [StructLayout(LayoutKind.Sequential)] private struct MOUSEINPUT { public int dx; public int dy; public int mouseData; public uint dwFlags; public uint time; public IntPtr dwExtraInfo; }
        [StructLayout(LayoutKind.Sequential)] private struct KEYBDINPUT { public ushort wVk; public ushort wScan; public uint dwFlags; public uint time; public IntPtr dwExtraInfo; }

        private const uint MOUSEEVENTF_MOVE = 0x0001;
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint KEYEVENTF_KEYDOWN = 0x0000;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        private void NotifyError(Exception ex) => ErrorOccurred?.Invoke(ex);
        private void NotifyStatus(string s) => StatusChanged?.Invoke(s);

        public void StartRecording(Window window)
        {
            try
            {
                if (_recording) return;
                Steps.Clear();
                _sw = Stopwatch.StartNew();
                _recording = true;
                window.PreviewMouseMove += Window_PreviewMouseMove;
                window.PreviewMouseDown += Window_PreviewMouseDown;
                window.PreviewMouseUp += Window_PreviewMouseUp;
                window.PreviewKeyDown += Window_PreviewKeyDown;
                window.PreviewKeyUp += Window_PreviewKeyUp;
                NotifyStatus("Macro recording started");
            }
            catch (Exception ex) { NotifyError(ex); }
        }

        public void StopRecording(Window window)
        {
            try
            {
                if (!_recording) return;
                _recording = false;
                window.PreviewMouseMove -= Window_PreviewMouseMove;
                window.PreviewMouseDown -= Window_PreviewMouseDown;
                window.PreviewMouseUp -= Window_PreviewMouseUp;
                window.PreviewKeyDown -= Window_PreviewKeyDown;
                window.PreviewKeyUp -= Window_PreviewKeyUp;
                _sw?.Stop();
                NotifyStatus("Macro recording stopped");
            }
            catch (Exception ex) { NotifyError(ex); }
        }

        private void AddStep(MacroStep step)
        {
            try
            {
                // Basic validation to avoid corrupt steps
                if (step.DelayFromPrevious < TimeSpan.Zero) step.DelayFromPrevious = TimeSpan.Zero;
                if (step.X < 0) step.X = 0;
                if (step.Y < 0) step.Y = 0;

                if (_sw != null)
                {
                    step.DelayFromPrevious = TimeSpan.FromMilliseconds(_sw.ElapsedMilliseconds);
                    _sw.Restart();
                }
                Steps.Add(step);
            }
            catch (Exception ex) { NotifyError(ex); }
        }

        private void Window_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!_recording) return;
            try
            {
                var p = e.GetPosition(null);
                AddStep(new MacroStep { EventType = MacroEventType.MouseMove, X = (int)p.X, Y = (int)p.Y });
            }
            catch (Exception ex) { NotifyError(ex); }
        }

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!_recording) return;
            try
            {
                var p = e.GetPosition(null);
                AddStep(new MacroStep { EventType = MacroEventType.MouseDown, X = (int)p.X, Y = (int)p.Y, MouseButton = (int)e.ChangedButton });
            }
            catch (Exception ex) { NotifyError(ex); }
        }

        private void Window_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_recording) return;
            try
            {
                var p = e.GetPosition(null);
                AddStep(new MacroStep { EventType = MacroEventType.MouseUp, X = (int)p.X, Y = (int)p.Y, MouseButton = (int)e.ChangedButton });
            }
            catch (Exception ex) { NotifyError(ex); }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!_recording) return;
            try { AddStep(new MacroStep { EventType = MacroEventType.KeyDown, VirtualKey = KeyInterop.VirtualKeyFromKey(e.Key) }); }
            catch (Exception ex) { NotifyError(ex); }
        }

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (!_recording) return;
            try { AddStep(new MacroStep { EventType = MacroEventType.KeyUp, VirtualKey = KeyInterop.VirtualKeyFromKey(e.Key) }); }
            catch (Exception ex) { NotifyError(ex); }
        }

        public async Task PlayAsync(CancellationToken token)
        {
            if (Steps.Count == 0) { NotifyStatus("No macro steps to play"); return; }

            try
            {
                do
                {
                    foreach (var s in Steps)
                    {
                        token.ThrowIfCancellationRequested();
                        if (s.DelayFromPrevious.TotalMilliseconds > 0)
                            await Task.Delay(s.DelayFromPrevious, token);

                        try
                        {
                            SimulateStep(s);
                        }
                        catch (OperationCanceledException) { throw; }
                        catch (Exception ex)
                        {
                            // per-step errors: notify and continue or decide to stop
                            NotifyError(new InvalidOperationException($"Error simulating step {s}: {ex.Message}", ex));
                        }
                    }
                } while (Loop && !token.IsCancellationRequested);
                NotifyStatus("Playback finished");
            }
            catch (OperationCanceledException)
            {
                NotifyStatus("Playback cancelled");
            }
            catch (Exception ex)
            {
                NotifyError(ex);
            }
        }

        private void SimulateStep(MacroStep s)
        {
            // Isolated try/catch removed here; callers handle per-step errors
            if (s.EventType == MacroEventType.MouseMove)
            {
                System.Windows.Forms.Cursor.Position = new System.Drawing.Point(s.X, s.Y);
            }
            else if (s.EventType == MacroEventType.MouseDown)
            {
                if (s.MouseButton == 0) DoMouse(MOUSEEVENTF_LEFTDOWN);
            }
            else if (s.EventType == MacroEventType.MouseUp)
            {
                if (s.MouseButton == 0) DoMouse(MOUSEEVENTF_LEFTUP);
            }
            else if (s.EventType == MacroEventType.KeyDown)
            {
                DoKey((ushort)s.VirtualKey, KEYEVENTF_KEYDOWN);
            }
            else if (s.EventType == MacroEventType.KeyUp)
            {
                DoKey((ushort)s.VirtualKey, KEYEVENTF_KEYUP);
            }
        }

        private void DoMouse(uint flags)
        {
            var input = new INPUT
            {
                type = 0,
                mkhi = new MOUSEKEYBDHARDWARE { mi = new MOUSEINPUT { dx = 0, dy = 0, mouseData = 0, dwFlags = flags, time = 0, dwExtraInfo = IntPtr.Zero } }
            };
            uint sent = SendInput(1, new[] { input }, Marshal.SizeOf(typeof(INPUT)));
            if (sent == 0)
            {
                int err = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"SendInput (mouse) failed: {err}");
            }
        }

        private void DoKey(ushort vk, uint flags)
        {
            var input = new INPUT
            {
                type = 1,
                mkhi = new MOUSEKEYBDHARDWARE { ki = new KEYBDINPUT { wVk = vk, wScan = 0, dwFlags = flags, time = 0, dwExtraInfo = IntPtr.Zero } }
            };
            uint sent = SendInput(1, new[] { input }, Marshal.SizeOf(typeof(INPUT)));
            if (sent == 0)
            {
                int err = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"SendInput (key) failed: {err}");
            }
        }
    }
}