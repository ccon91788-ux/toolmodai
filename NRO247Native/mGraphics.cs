using System;
using SkiaSharp;

public class mGraphics
{
	public static int HCENTER = 1;
	public static int VCENTER = 2;
	public static int LEFT = 4;
	public static int RIGHT = 8;
	public static int TOP = 16;
	public static int BOTTOM = 32;

	private float r, g, b, a;
	public int clipX, clipY, clipW, clipH;
	
	private bool isTranslate = true;
	private int translateX, translateY;

	public const int BASELINE = 64;
	public const int SOLID = 0;
	public const int DOTTED = 1;
	public const int TRANS_MIRROR = 2;
	public const int TRANS_MIRROR_ROT180 = 1;
	public const int TRANS_MIRROR_ROT270 = 4;
	public const int TRANS_MIRROR_ROT90 = 7;
	public const int TRANS_NONE = 0;
	public const int TRANS_ROT180 = 3;
	public const int TRANS_ROT270 = 6;
	public const int TRANS_ROT90 = 5;

	public static int addYWhenOpenKeyBoard;

	private int clipTX, clipTY;
	private SKCanvas canvas;
	private bool _clipSaved = false;
	private SKPaint _paint;
	private SKPaint _bitmapPaint;
	private SKRect _tempRect;
	private SKRect _srcRect;
	private int _cachedArgb = int.MinValue;
	private bool _paintDirty = true;
	private bool _lastStyleWasStroke = false;

	public mGraphics()
	{
		_paint = new SKPaint
		{
			IsAntialias = false,
			FilterQuality = SKFilterQuality.None,
			StrokeWidth = 1f,
			Style = SKPaintStyle.Fill
		};
		
		_bitmapPaint = new SKPaint
		{
			IsAntialias = false,
			FilterQuality = SKFilterQuality.None
		};
	}

	public void SetGraphics(SKCanvas c)
	{
		canvas = c;
	}

	public void translate(int tx, int ty)
	{
		translateX += tx;
		translateY += ty;
		isTranslate = translateX != 0 || translateY != 0;
	}

	public int getTranslateX() => translateX;
	public int getTranslateY() => translateY + addYWhenOpenKeyBoard;

	public void setClip(int x, int y, int w, int h)
	{
		clipTX = translateX;
		clipTY = translateY;
		clipX = x;
		clipY = y;
		clipW = w;
		clipH = h;

		if (isTranslate)
		{
			x += clipTX;
			y += clipTY;
		}

		if (canvas == null) return;

		if (_clipSaved)
		{
			try { canvas.Restore(); }
			catch { }
			_clipSaved = false;
		}

		canvas.Save();
		_clipSaved = true;
		canvas.ClipRect(new SKRect(x, y, x + w, y + h), SKClipOperation.Intersect, antialias: false);
	}

	public int getClipX() => clipX;
	public int getClipY() => clipY;
	public int getClipWidth() => clipW;
	public int getClipHeight() => clipH;

	private void UpdatePaint(bool isStroke)
	{
		int aInt = (int)(a * 255f);
		int rInt = (int)(r * 255f);
		int gInt = (int)(g * 255f);
		int bInt = (int)(b * 255f);
		int argb = (aInt << 24) | (rInt << 16) | (gInt << 8) | bInt;

		if (_paintDirty || _cachedArgb != argb || _lastStyleWasStroke != isStroke)
		{
			_paint.Color = new SKColor((byte)rInt, (byte)gInt, (byte)bInt, (byte)aInt);
			_paint.Style = isStroke ? SKPaintStyle.Stroke : SKPaintStyle.Fill;
			_cachedArgb = argb;
			_paintDirty = false;
			_lastStyleWasStroke = isStroke;
		}
	}

	public void fillRect(int x, int y, int w, int h, int color, int alpha)
	{
		float a2 = (float)alpha / 255f;
		setColor(color, a2);
		fillRect(x, y, w, h);
	}

	public void drawLine(int x1, int y1, int x2, int y2)
	{
		if (canvas == null) return;

		if (isTranslate)
		{
			x1 += translateX;
			y1 += translateY;
			x2 += translateX;
			y2 += translateY;
		}

		UpdatePaint(true);
		canvas.DrawLine(x1, y1, x2, y2, _paint);
	}

	public void drawRect(int x, int y, int w, int h)
	{
		if (canvas == null) return;

		if (isTranslate)
		{
			x += translateX;
			y += translateY;
		}

		UpdatePaint(true);
		canvas.DrawRect(new SKRect(x, y, x + w, y + h), _paint);
	}

	public void drawCircle(int x, int y, int r)
	{
		if (canvas == null) return;

		if (isTranslate)
		{
			x += translateX;
			y += translateY;
		}

		UpdatePaint(true);
		canvas.DrawCircle(x, y, r, _paint);
	}

	public void fillRect(int x, int y, int w, int h)
	{
		if (canvas == null || w < 0 || h < 0) return;

		if (isTranslate)
		{
			x += translateX;
			y += translateY;
		}

		UpdatePaint(false);
		canvas.DrawRect(new SKRect(x, y, x + w, y + h), _paint);
	}

	public void setColor(int rgb)
	{
		int num = rgb & 0xFF;
		int num2 = (rgb >> 8) & 0xFF;
		int num3 = (rgb >> 16) & 0xFF;
		
		b = (float)num / 255f;
		g = (float)num2 / 255f;
		r = (float)num3 / 255f;
		a = 1f;

		int argb = (255 << 24) | (num3 << 16) | (num2 << 8) | num;
		if (argb != _cachedArgb)
		{
			_paintDirty = true;
		}
	}

	public void setColor(int rgb, float alpha)
	{
		int num = rgb & 0xFF;
		int num2 = (rgb >> 8) & 0xFF;
		int num3 = (rgb >> 16) & 0xFF;
		
		b = (float)num / 255f;
		g = (float)num2 / 255f;
		r = (float)num3 / 255f;
		a = alpha;

		int aInt = (int)(alpha * 255f);
		int argb = (aInt << 24) | (num3 << 16) | (num2 << 8) | num;
		if (argb != _cachedArgb)
		{
			_paintDirty = true;
		}
	}

	public void fillTrans(Image imgTrans, int x, int y, int w, int h)
	{
		setColor(0, 0.5f);
		fillRect(x, y, w, h);
	}

	public void fillArg(int i, int j, int k, int l, int m, int n)
	{
		fillRect(i, j, k, l);
	}

	public void drawRegion(Image arg0, int x0, int y0, int w0, int h0, int arg5, int x, int y, int arg8)
	{
		if (arg0 != null)
		{
			_drawRegion(arg0, x0, y0, w0, h0, arg5, x, y, arg8);
		}
	}

	public void drawRegion(Image arg0, int x0, int y0, int w0, int h0, int arg5, float x, float y, int arg8)
	{
		if (arg0 != null)
		{
			_drawRegion(arg0, x0, y0, w0, h0, arg5, (int)x, (int)y, arg8);
		}
	}

	private void _drawRegion(Image image, int x0, int y0, int w, int h, int transform, int x, int y, int anchor)
	{
		if (image?.bitmap == null) return;

		if (isTranslate)
		{
			x += translateX;
			y += translateY;
		}

		if ((anchor & HCENTER) != 0) x -= w >> 1;
		if ((anchor & VCENTER) != 0) y -= h >> 1;
		if ((anchor & RIGHT) != 0) x -= w;
		if ((anchor & BOTTOM) != 0) y -= h;

		_srcRect.Left = x0;
		_srcRect.Top = y0;
		_srcRect.Right = x0 + w;
		_srcRect.Bottom = y0 + h;

		if (transform == 0)
		{
			_tempRect.Left = x;
			_tempRect.Top = y;
			_tempRect.Right = x + w;
			_tempRect.Bottom = y + h;
			canvas.DrawBitmap(image.bitmap, _srcRect, _tempRect, _bitmapPaint);
			return;
		}

		int saveCount = canvas.Save();
		try
		{
			_tempRect.Left = 0f;
			_tempRect.Top = 0f;
			_tempRect.Right = w;
			_tempRect.Bottom = h;

			switch (transform)
			{
				case 2: // TRANS_MIRROR
					canvas.Translate(x + w, y);
					canvas.Scale(-1f, 1f);
					break;
				case 1: // TRANS_MIRROR_ROT180
					canvas.Translate(x, y + h);
					canvas.Scale(1f, -1f);
					break;
				case 3: // TRANS_ROT180
					canvas.Translate(x + w, y + h);
					canvas.RotateDegrees(180f);
					break;
				case 5: // TRANS_ROT90
					canvas.Translate(x + h, y);
					canvas.RotateDegrees(90f);
					break;
				case 6: // TRANS_ROT270
					canvas.Translate(x, y + w);
					canvas.RotateDegrees(270f);
					break;
				case 7: // TRANS_MIRROR_ROT90
					canvas.Translate(x, y);
					canvas.RotateDegrees(90f);
					canvas.Scale(1f, -1f);
					break;
				case 4: // TRANS_MIRROR_ROT270
					canvas.Translate(x + w, y + h);
					canvas.RotateDegrees(270f);
					canvas.Scale(1f, -1f);
					break;
			}

			canvas.DrawBitmap(image.bitmap, _srcRect, _tempRect, _bitmapPaint);
		}
		finally
		{
			canvas.RestoreToCount(saveCount);
		}
	}

	public void drawImage(Image image, int x, int y, int anchor)
	{
		if (image?.bitmap != null)
		{
			drawRegion(image, 0, 0, image.bitmap.Width, image.bitmap.Height, 0, x, y, anchor);
		}
	}

	public void drawImage(Image image, int x, int y)
	{
		if (image?.bitmap != null)
		{
			drawRegion(image, 0, 0, image.bitmap.Width, image.bitmap.Height, 0, x, y, TOP | LEFT);
		}
	}

	public void drawImage(Image image, float x, float y, int anchor)
	{
		if (image?.bitmap != null)
		{
			drawRegion(image, 0, 0, image.bitmap.Width, image.bitmap.Height, 0, (int)x, (int)y, anchor);
		}
	}

	public void fillRoundRect(int x, int y, int width, int height, int arcWidth, int arcHeight)
	{
		if (canvas == null) return;

		if (isTranslate)
		{
			x += translateX;
			y += translateY;
		}

		UpdatePaint(false);
		float rx = (float)arcWidth / 2f;
		float ry = (float)arcHeight / 2f;
		canvas.DrawRoundRect(new SKRect(x, y, x + width, y + height), rx, ry, _paint);
	}

	public void reset()
	{
		isTranslate = false;
		translateX = translateY = 0;

		if (_clipSaved && canvas != null)
		{
			try { canvas.Restore(); }
			catch { }
			_clipSaved = false;
		}
	}

	public static int getImageWidth(Image image) => image?.bitmap?.Width ?? 0;
	public static int getImageHeight(Image image) => image?.bitmap?.Height ?? 0;
	public static bool isNotTranColor(SKColor color) => color.Alpha != 0;

	public static Image blend(Image img0, float level, int rgb)
	{
		if (img0?.bitmap == null) return null;

		int num = rgb & 0xFF;
		int num2 = (rgb >> 8) & 0xFF;
		int num3 = (rgb >> 16) & 0xFF;
		
		float bBlend = (float)num / 255f;
		float gBlend = (float)num2 / 255f;
		float rBlend = (float)num3 / 255f;

		SKBitmap srcBmp = img0.bitmap;
		int w = srcBmp.Width;
		int h = srcBmp.Height;
		
		Image result = Image.createImage(w, h);
		SKBitmap dstBmp = result.bitmap;

		IntPtr srcPtr = srcBmp.GetPixels();
		IntPtr dstPtr = dstBmp.GetPixels();

		unsafe
		{
			uint* src = (uint*)srcPtr.ToPointer();
			uint* dst = (uint*)dstPtr.ToPointer();
			int len = w * h;

			for (int i = 0; i < len; i++)
			{
				uint pixel = src[i];
				byte a = (byte)(pixel >> 24);
				
				if (a == 0)
				{
					dst[i] = 0;
					continue;
				}

				byte r = (byte)(pixel >> 16);
				byte g = (byte)(pixel >> 8);
				byte b = (byte)pixel;

				float fr = r / 255f;
				float fg = g / 255f;
				float fb = b / 255f;

				float nr = Math.Max(0f, Math.Min(1f, (rBlend - fr) * level + fr));
				float ng = Math.Max(0f, Math.Min(1f, (gBlend - fg) * level + fg));
				float nb = Math.Max(0f, Math.Min(1f, (bBlend - fb) * level + fb));

				dst[i] = ((uint)a << 24) | ((uint)(nr * 255f) << 16) | ((uint)(ng * 255f) << 8) | (uint)(nb * 255f);
			}
		}

		return result;
	}

	public void drawImageFog(Image image, int x, int y, int anchor)
	{
		if (image?.bitmap != null)
		{
			drawRegion(image, 0, 0, image.bitmap.Width, image.bitmap.Height, 0, x, y, anchor);
		}
	}

	public static SKColor setColorObj(int rgb)
	{
		int num = rgb & 0xFF;
		int num2 = (rgb >> 8) & 0xFF;
		int num3 = (rgb >> 16) & 0xFF;
		return new SKColor((byte)num3, (byte)num2, (byte)num, 0xFF);
	}

	public static int blendColor(float level, int color, int colorBlend)
	{
		SKColor c1 = setColorObj(color);
		SKColor c2 = setColorObj(colorBlend);

		float rr = Math.Max(0f, Math.Min(255f, (c2.Red - c1.Red) * level + c1.Red));
		float gg = Math.Max(0f, Math.Min(255f, (c2.Green - c1.Green) * level + c1.Green));
		float bb = Math.Max(0f, Math.Min(255f, (c2.Blue - c1.Blue) * level + c1.Blue));

		return ((int)rr << 16) | ((int)gg << 8) | (int)bb;
	}

	public static int getIntByColor(SKColor cl)
	{
		return (cl.Red << 16) | (cl.Green << 8) | cl.Blue;
	}

	public static int getRealImageWidth(Image img) => img?.w ?? 0;
	public static int getRealImageHeight(Image img) => img?.h ?? 0;

	public void DisposeResources()
	{
		_paint?.Dispose();
		_bitmapPaint?.Dispose();
		_paint = null;
		_bitmapPaint = null;
	}
}