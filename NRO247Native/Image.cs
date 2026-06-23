#define DEBUG
using System;
using System.Diagnostics;
using System.IO;
using SkiaSharp;
using LoadAssets;

public class Image : IDisposable
{
	private const int INTERVAL = 5;
	private const int MAXTIME = 500;

	public SKBitmap bitmap;
	public int w;
	public int h;
	public static Image imgTemp;
	public static string filenametemp;
	public static byte[] datatemp;
	public static Image imgSrcTemp;
	public static int xtemp;
	public static int ytemp;
	public static int wtemp;
	public static int htemp;
	public static int transformtemp;
	public static int status;

	private bool _disposed = false;

	public Image()
	{
		bitmap = new SKBitmap(1, 1, SKColorType.Rgba8888, SKAlphaType.Premul);
		w = 1;
		h = 1;
	}

	public Image(SKBitmap bmp)
	{
		bitmap = bmp ?? new SKBitmap(1, 1, SKColorType.Rgba8888, SKAlphaType.Premul);
		w = bitmap?.Width ?? 0;
		h = bitmap?.Height ?? 0;
	}

	public static Image createImage(string filename)
	{
		return __createImage(filename);
	}

	public static Image createImage(byte[] imageData)
	{
		return __createImage(imageData);
	}

	public static Image createImage(int width, int height)
	{
		return __createImage(width, height);
	}

	public static Image createImage(sbyte[] imageData, int offset, int length)
	{
		if (imageData == null || offset < 0 || length < 0 || offset + length > imageData.Length)
		{
			return null;
		}
		byte[] array = new byte[length];
		Buffer.BlockCopy(imageData, offset, array, 0, length);
		
		return createImage(array);
	}

	public static byte convertSbyteToByte(sbyte var)
	{
		return (byte)(var & 0xFF);
	}

	public static Image createRGBImage(int[] rgb, int w, int h, bool bl)
	{
		if (w <= 0 || h <= 0 || rgb == null)
		{
			return null;
		}

		Image image = createImage(w, h);
		if (image?.bitmap == null)
		{
			return null;
		}
		int len = Math.Min(rgb.Length, w * h);
		IntPtr pixelsAddr = image.bitmap.GetPixels();
		
		unsafe
		{
			uint* ptr = (uint*)pixelsAddr.ToPointer();
			for (int i = 0; i < len; i++)
			{
				ptr[i] = (uint)ConvertRGBToColor(rgb[i]);
			}
		}

		return image;
	}

	private static int ConvertRGBToColor(int rgb)
	{
		int b = rgb & 0xFF;
		int g = (rgb >> 8) & 0xFF;
		int r = (rgb >> 16) & 0xFF;
		return (int)(0xFF000000 | ((uint)r << 16) | ((uint)g << 8) | (uint)b);
	}

	public static SKColor setColorFromRGB(int rgb)
	{
		int b = rgb & 0xFF;
		int g = (rgb >> 8) & 0xFF;
		int r = (rgb >> 16) & 0xFF;
		return new SKColor((byte)r, (byte)g, (byte)b, 0xFF);
	}


    private static Image __createImage(string filename)
    {
        try
        {
            if (string.IsNullOrEmpty(filename))
                return null;

            // Nếu không có đuôi -> tự thêm .png
            string relativePath = Path.HasExtension(filename)
                ? filename
                : filename + ".png";

            // Load từ ResourceCache
            SKBitmap bmp = AssetBundle.GetBitmap(relativePath);
            if (bmp == null)
            {
                Debug.WriteLine("Resource not found: " + relativePath);
                return null;
            }

            return new Image
            {
                bitmap = bmp,
                w = bmp.Width,
                h = bmp.Height
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Load image error: " + ex.Message);
            return null;
        }
    }

	private static Image __createImage(byte[] imageData)
	{
		if (imageData == null || imageData.Length == 0)
		{
			try
			{
				Cout.LogError("Create Image from byte array fail");
			}
			catch { }
			return null;
		}

		Image image = new Image();
		try
		{
			SKBitmap bmp = SKBitmap.Decode(imageData);
			if (bmp == null)
			{
				try
				{
					Cout.LogError("Failed to decode image from byte array");
				}
				catch { }
				return image;
			}

			image.bitmap = bmp;
			image.w = bmp.Width;
			image.h = bmp.Height;
			return image;
		}
		catch (Exception ex)
		{
			try
			{
				Cout.LogError("CREATE IMAGE FROM ARRAY FAIL: " + ex.Message);
			}
			catch { }
			return image;
		}
	}

	private static Image __createImage(int width, int height)
	{
		Image image = new Image();
		if (width <= 0) width = 1;
		if (height <= 0) height = 1;

		image.bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
		image.w = width;
		image.h = height;
		image.bitmap.Erase(SKColors.Transparent);
		return image;
	}

	public int getWidth() => w;
	public int getHeight() => h;
	public int getRealImageWidth() => w;
	public int getRealImageHeight() => h;

    public void getRGB(ref int[] data, int x1, int x2, int x, int y, int w, int h)
    {
        if (bitmap == null) return;

        if (data == null || data.Length < w * h)
            data = new int[w * h];

        int bmpWidth = bitmap.Width;
        int bmpHeight = bitmap.Height;
        IntPtr pixelsAddr = bitmap.GetPixels();

        unsafe
        {
            uint* pixels = (uint*)pixelsAddr.ToPointer();
            int index = 0;

            for (int j = 0; j < h; j++)
            {
                int py = y + j;

                // FIX: thiếu || trong biểu thức
                if (py < 0 || py >= bmpHeight)
                {
                    for (int i = 0; i < w; i++)
                        data[index++] = 0;
                    continue;
                }

                int rowStart = py * bmpWidth;

                for (int i = 0; i < w; i++)
                {
                    int px = x + i;

                    if (px >= 0 && px < bmpWidth)
                        data[index++] = (int)pixels[rowStart + px];
                    else
                        data[index++] = 0;
                }
            }
        }
    }

    public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			if (disposing)
			{
				bitmap = null;
			}
			_disposed = true;
		}
	}

	~Image()
	{
		Dispose(false);
	}
}