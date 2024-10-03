// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.BendModel.TextBillboard
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 42974AD7-DECE-42B8-A268-FDDC3C570A12
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using SharpDX.Direct2D1;
using SharpDX.Direct3D11;
using SharpDX.DirectWrite;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System;
using System.Drawing;
using WiCAM.Pn4000.BendModel.Base;

namespace WiCAM.Pn4000.BendModel
{
  public class TextBillboard : Texture2DBillboard
  {
    public TextBillboard(
      SharpDX.Direct3D11.Device device,
      Vector3d position,
      string text,
      TextBillboard.TextBillboardStyle style,
      bool alwaysOnTop = true,
      int sortinglayer = 0,
      bool scaleWithDistance = true,
      bool enabled = true,
      float pixelDensity = 1f)
    {
      this.Position = position;
      this.ScaleWithDistance = scaleWithDistance;
      this.RenderOnTop = alwaysOnTop;
      this.Enabled = enabled;
      this.PixelDensity = pixelDensity;
      this.SortingLayer = sortinglayer;
      SharpDX.DirectWrite.Factory factory1 = new SharpDX.DirectWrite.Factory();
      FontCollection systemFontCollection = factory1.GetSystemFontCollection((RawBool) true);
      string str = style.fontName ?? "";
      int index;
      RawBool familyName = systemFontCollection.FindFamilyName(str, out index);
      if (!(bool) familyName)
      {
        str = SystemFonts.DefaultFont.FontFamily.Name;
        familyName = systemFontCollection.FindFamilyName(str, out index);
      }
      if (!(bool) familyName)
        str = systemFontCollection.GetFontFamily(0).FamilyNames.GetString(0);
      int strokeWidth = 0;
      if (style.roundBorder != null)
        strokeWidth = style.roundBorder.Width;
      TextFormat textFormat = new TextFormat(factory1, str, (float) style.fontSize);
      if (style.textFormat != null)
        textFormat = style.textFormat;
      textFormat.TextAlignment = TextAlignment.Leading;
      textFormat.ParagraphAlignment = ParagraphAlignment.Near;
      TextLayout textLayout1 = new TextLayout(factory1, text, textFormat, 10420f, 10420f);
      this.Extents = new Vector2d((double) textLayout1.Metrics.WidthIncludingTrailingWhitespace + (double) style.padding.left + (double) style.padding.right + (double) (strokeWidth * 2), (double) textLayout1.Metrics.Height + (double) style.padding.top + (double) style.padding.bottom + (double) (strokeWidth * 2));
      if (style.textFormat != null)
      {
        textFormat.TextAlignment = style.textFormat.TextAlignment;
        textFormat.ParagraphAlignment = style.textFormat.ParagraphAlignment;
      }
      else
      {
        textFormat.TextAlignment = TextAlignment.Center;
        textFormat.ParagraphAlignment = ParagraphAlignment.Center;
      }
      TextLayout textLayout2 = new TextLayout(factory1, text, textFormat, (float) this.Extents.X - (float) (strokeWidth * 2) - (float) style.padding.left - (float) style.padding.right, (float) this.Extents.Y - (float) style.padding.top - (float) style.padding.bottom - (float) (strokeWidth * 2));
      Texture2DDescription texture2Ddescription = new Texture2DDescription();
      ref Texture2DDescription local1 = ref texture2Ddescription;
      Vector2d extents = this.Extents;
      int num1 = (int) Math.Ceiling(extents.X);
      local1.Width = num1;
      ref Texture2DDescription local2 = ref texture2Ddescription;
      extents = this.Extents;
      int num2 = (int) Math.Ceiling(extents.Y);
      local2.Height = num2;
      texture2Ddescription.ArraySize = 1;
      texture2Ddescription.MipLevels = 1;
      texture2Ddescription.BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget;
      texture2Ddescription.Usage = ResourceUsage.Default;
      texture2Ddescription.CpuAccessFlags = CpuAccessFlags.None;
      texture2Ddescription.Format = Format.B8G8R8A8_UNorm;
      texture2Ddescription.OptionFlags = ResourceOptionFlags.Shared;
      texture2Ddescription.SampleDescription = new SampleDescription(1, 0);
      Texture2DDescription description = texture2Ddescription;
      this.Texture = new Texture2D(device, description);
      Surface surface = this.Texture.QueryInterface<Surface>();
      SharpDX.Direct2D1.Factory factory2 = new SharpDX.Direct2D1.Factory();
      PixelFormat pixelFormat = new PixelFormat(Format.Unknown, SharpDX.Direct2D1.AlphaMode.Premultiplied);
      RenderTargetProperties targetProperties = new RenderTargetProperties()
      {
        Type = RenderTargetType.Default,
        PixelFormat = pixelFormat,
        DpiX = 0.0f,
        DpiY = 0.0f
      };
      Surface dxgiSurface = surface;
      RenderTargetProperties properties = targetProperties;
      RenderTarget renderTarget = new RenderTarget(factory2, dxgiSurface, properties);
      SolidColorBrush defaultForegroundBrush = new SolidColorBrush(renderTarget, style.textColor);
      SolidColorBrush solidColorBrush1 = new SolidColorBrush(renderTarget, style.bgColor);
      renderTarget.BeginDraw();
      if (style.roundBorder != null)
      {
        renderTarget.Clear(new RawColor4?(new RawColor4(0.0f, 0.0f, 0.0f, 0.0f)));
        SolidColorBrush solidColorBrush2 = new SolidColorBrush(renderTarget, style.roundBorder.Color);
        float num3 = (float) strokeWidth / 2f;
        RawRectangleF rawRectangleF = new RawRectangleF();
        ref RawRectangleF local3 = ref rawRectangleF;
        double left = (double) num3;
        double top = (double) num3;
        extents = this.Extents;
        double right = extents.X - (double) num3;
        extents = this.Extents;
        double bottom = extents.Y - (double) num3;
        local3 = new RawRectangleF((float) left, (float) top, (float) right, (float) bottom);
        RoundedRectangle roundedRect = new RoundedRectangle()
        {
          Rect = rawRectangleF,
          RadiusX = (float) style.roundBorder.Radius,
          RadiusY = (float) style.roundBorder.Radius
        };
        renderTarget.FillRoundedRectangle(roundedRect, (SharpDX.Direct2D1.Brush) solidColorBrush1);
        renderTarget.DrawRoundedRectangle(roundedRect, (SharpDX.Direct2D1.Brush) solidColorBrush2, (float) strokeWidth);
      }
      else
        renderTarget.Clear(new RawColor4?(style.bgColor));
      renderTarget.DrawTextLayout(new RawVector2((float) (strokeWidth + style.padding.left), (float) (strokeWidth + style.padding.top)), textLayout2, (SharpDX.Direct2D1.Brush) defaultForegroundBrush);
      renderTarget.EndDraw();
      renderTarget.Dispose();
    }

    public struct TextBillboardStyle
    {
      public RawColor4 textColor;
      public RawColor4 bgColor;
      public int fontSize;
      public string fontName;
      public TextBillboard.Padding padding;
      public TextFormat textFormat;
      public RoundBorder roundBorder;
    }

    public struct Padding
    {
      public int top;
      public int bottom;
      public int right;
      public int left;
    }
  }
}
