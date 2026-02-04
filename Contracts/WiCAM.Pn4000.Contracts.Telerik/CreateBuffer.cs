namespace WiCAM.Pn4000.Contracts.Telerik;

public delegate void CreateBuffer(int pixelWidth, int pixelHeight, out nint buffer, out int bufferSize, out int stride);
