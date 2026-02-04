using WiCAM.Pn4000.ScreenD3D.Controls;

namespace WiCAM.Pn4000.JobManager
{
    internal interface IScreenD3D11
    {
        Screen3D _Screen3D { get; }

        void Init(Screen3D screen);
    }
}