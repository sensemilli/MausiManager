using System;
using System.Windows;

namespace WiCAM.Pn4000.PN3D.BendSimulation.Configuration;

public class ScreenParams
{
	private readonly double _canvasWidth;

	private readonly double _canvasHeight;

	private double _pixelPerMM;

	private double _beamAreaStartX;

	private double _beamAreaStartY;

	private double _drawableAreaWidth;

	private double _drawableAreaHeight;

	private readonly double _minXMM;

	private readonly double _maxXMM;

	private readonly double _minYMM;

	private readonly double _maxYMM;

	public ScreenParams(double canvasWidth, double canvasHeight, double minXMM, double maxXMM, double minYMM, double maxYMM)
	{
		this._canvasWidth = canvasWidth;
		this._canvasHeight = canvasHeight;
		this._minXMM = minXMM;
		this._maxXMM = maxXMM;
		this._minYMM = minYMM;
		this._maxYMM = maxYMM;
		double num = 10.0;
		this._drawableAreaWidth = this._canvasWidth - 2.0 * num;
		this._drawableAreaHeight = this._canvasHeight - 2.0 * num;
		this._pixelPerMM = Math.Min(this._drawableAreaHeight / (maxYMM - minYMM), this._drawableAreaWidth / (maxXMM - minXMM));
		this._beamAreaStartX = (this._canvasWidth - this._pixelPerMM * (maxXMM - minXMM)) / 2.0;
		this._beamAreaStartY = (this._canvasHeight - this._pixelPerMM * (maxYMM - minYMM)) / 2.0;
	}

	public Point GetPointProjection(double x, double y)
	{
		return new Point((x - this._minXMM) * this._pixelPerMM + this._beamAreaStartX, this._beamAreaStartY + (this._maxYMM - this._minYMM - y) * this._pixelPerMM);
	}

	public double MillimeterToPixel(double mm)
	{
		return mm * this._pixelPerMM;
	}

	public double PixelsToMillimeter(double pix)
	{
		return pix / this._pixelPerMM;
	}

	public double GetPixelsPerMM()
	{
		return this._pixelPerMM;
	}
}
