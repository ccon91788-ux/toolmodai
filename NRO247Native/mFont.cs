using System;
using System.Collections.Generic;
using SkiaSharp;

public class mFont
{
	public const int LEFT = 0;
	public const int RIGHT = 1;
	public const int CENTER = 2;
	public const int RED = 0;
	public const int YELLOW = 1;
	public const int GREEN = 2;
	public const int FATAL = 3;
	public const int MISS = 4;
	public const int ORANGE = 5;
	public const int ADDMONEY = 6;
	public const int MISS_ME = 7;
	public const int FATAL_ME = 8;
	public const int HP = 9;
	public const int MP = 10;

	private int space;
	private Image imgFont;
	private string strFont;
	private int[][] fImages;

	public static int yAddFont;

	// Optimize: Cache character positions for faster lookup
	private Dictionary<char, int> _charIndexCache;

	// Static font instances
	public static mFont tahoma_7b_red;
	public static mFont tahoma_7b_blue;
	public static mFont tahoma_7b_white;
	public static mFont tahoma_7b_yellow;
	public static mFont tahoma_7b_yellowSmall;
	public static mFont tahoma_7b_dark;
	public static mFont tahoma_7b_green2;
	public static mFont tahoma_7b_green;
	public static mFont tahoma_7b_focus;
	public static mFont tahoma_7b_unfocus;
	public static mFont tahoma_7;
	public static mFont tahoma_7_blue1;
	public static mFont tahoma_7_blue1Small;
	public static mFont tahoma_7_green2;
	public static mFont tahoma_7_yellow;
	public static mFont tahoma_7_grey;
	public static mFont tahoma_7_red;
	public static mFont tahoma_7_blue;
	public static mFont tahoma_7_green;
	public static mFont tahoma_7_white;
	public static mFont tahoma_8b;
	public static mFont number_yellow;
	public static mFont number_red;
	public static mFont number_green;
	public static mFont number_gray;
	public static mFont number_orange;
	public static mFont bigNumber_red;
	public static mFont bigNumber_While;
	public static mFont bigNumber_yellow;
	public static mFont bigNumber_green;
	public static mFont bigNumber_orange;
	public static mFont bigNumber_blue;
	public static mFont bigNumber_black;
	public static mFont nameFontRed;
	public static mFont nameFontYellow;
	public static mFont nameFontGreen;
	public static mFont tahoma_7_greySmall;
	public static mFont tahoma_7b_yellowSmall2;
	public static mFont tahoma_7b_green2Small;
	public static mFont tahoma_7_whiteSmall;
	public static mFont tahoma_7b_greenSmall;

	private int height;
	private int wO;
	private string pathImage;

	private static SKPaint measurePaint = new SKPaint
	{
		IsAntialias = true,
		FilterQuality = SKFilterQuality.High,
		Typeface = SKTypeface.FromFamilyName("Arial")
	};

	public mFont(string strFont, string pathImage, string pathData, int space)
	{
		try
		{
			this.strFont = strFont;
			this.space = space;
			this.pathImage = pathImage;
			
			// Optimize: Build character index cache
			_charIndexCache = new Dictionary<char, int>(strFont.Length);
			for (int i = 0; i < strFont.Length; i++)
			{
				_charIndexCache[strFont[i]] = i;
			}

			reloadImage();
			LoadFontData(pathData);
		}
		catch (Exception ex)
		{
			ex.StackTrace.ToString();
		}
	}

	// Optimize: Separate font data loading
	private void LoadFontData(string pathData)
	{
		DataInputStream dataInputStream = null;
		try
		{
			dataInputStream = MyStream.readFile(pathData);
			int count = dataInputStream.readShort();
			fImages = new int[count][];
			
			for (int i = 0; i < count; i++)
			{
				fImages[i] = new int[4];
				fImages[i][0] = dataInputStream.readShort();
				fImages[i][1] = dataInputStream.readShort();
				fImages[i][2] = dataInputStream.readShort();
				fImages[i][3] = dataInputStream.readShort();
				setHeight(fImages[i][3]);
			}
		}
		catch (Exception)
		{
		}
		finally
		{
			try { dataInputStream?.close(); }
			catch { }
		}
	}

	public static void init()
	{
		const string chars = " 0123456789+-*='_?.,<>/[]{}!@#$%^&*():aáàảãạâấầẩẫậăắằẳẵặbcdđeéèẻẽẹêếềểễệfghiíìỉĩịjklmnoóòỏõọôốồổỗộơớờởỡợpqrstuúùủũụưứừửữựvxyýỳỷỹỵzwAÁÀẢÃẠĂẮẰẲẴẶÂẤẦẨẪẬBCDĐEÉÈẺẼẸÊẾỀỂỄỆFGHIÍÌỈĨỊJKLMNOÓÒỎÕỌÔỐỒỔỖỘƠỚỜỞỠỢPQRSTUÚÙỦŨỤƯỨỪỬỮỰVXYÝỲỶỸỴZW";
		const string numChars = " 0123456789+-";

		tahoma_7b_red = new mFont(chars, "/myfont/tahoma_7b_red.png", "/myfont/tahoma_7b", 0);
		tahoma_7b_blue = new mFont(chars, "/myfont/tahoma_7b_blue.png", "/myfont/tahoma_7b", 0);
		tahoma_7b_white = new mFont(chars, "/myfont/tahoma_7b_white.png", "/myfont/tahoma_7b", 0);
		tahoma_7b_yellow = new mFont(chars, "/myfont/tahoma_7b_yellow.png", "/myfont/tahoma_7b", 0);
		tahoma_7b_yellowSmall = new mFont(chars, "/myfont/tahoma_7b_yellow.png", "/myfont/tahoma_7b", 0);
		tahoma_7b_dark = new mFont(chars, "/myfont/tahoma_7b_brown.png", "/myfont/tahoma_7b", 0);
		tahoma_7b_green2 = new mFont(chars, "/myfont/tahoma_7b_green2.png", "/myfont/tahoma_7b", 0);
		tahoma_7b_green = new mFont(chars, "/myfont/tahoma_7b_green.png", "/myfont/tahoma_7b", 0);
		tahoma_7b_focus = new mFont(chars, "/myfont/tahoma_7b_focus.png", "/myfont/tahoma_7b", 0);
		tahoma_7b_unfocus = new mFont(chars, "/myfont/tahoma_7b_unfocus.png", "/myfont/tahoma_7b", 0);
		tahoma_7 = new mFont(chars, "/myfont/tahoma_7.png", "/myfont/tahoma_7", 0);
		tahoma_7_blue1 = new mFont(chars, "/myfont/tahoma_7_blue1.png", "/myfont/tahoma_7", 0);
		tahoma_7_green2 = new mFont(chars, "/myfont/tahoma_7_green2.png", "/myfont/tahoma_7", 0);
		tahoma_7_yellow = new mFont(chars, "/myfont/tahoma_7_yellow.png", "/myfont/tahoma_7", 0);
		tahoma_7_grey = new mFont(chars, "/myfont/tahoma_7_grey.png", "/myfont/tahoma_7", 0);
		tahoma_7_red = new mFont(chars, "/myfont/tahoma_7_red.png", "/myfont/tahoma_7", 0);
		tahoma_7_blue = new mFont(chars, "/myfont/tahoma_7_blue.png", "/myfont/tahoma_7", 0);
		tahoma_7_green = new mFont(chars, "/myfont/tahoma_7_green.png", "/myfont/tahoma_7", 0);
		tahoma_7_white = new mFont(chars, "/myfont/tahoma_7_white.png", "/myfont/tahoma_7", 0);
		tahoma_8b = new mFont(chars, "/myfont/tahoma_8b.png", "/myfont/tahoma_8b", -1);
		
		number_yellow = new mFont(numChars, "/myfont/number_yellow.png", "/myfont/number", 0);
		number_red = new mFont(numChars, "/myfont/number_red.png", "/myfont/number", 0);
		number_green = new mFont(numChars, "/myfont/number_green.png", "/myfont/number", 0);
		number_gray = new mFont(numChars, "/myfont/number_gray.png", "/myfont/number", 0);
		number_orange = new mFont(numChars, "/myfont/number_orange.png", "/myfont/number", 0);
		
		bigNumber_red = number_red;
		bigNumber_While = tahoma_7b_white;
		bigNumber_yellow = number_yellow;
		bigNumber_green = number_green;
		bigNumber_orange = number_orange;
		bigNumber_blue = tahoma_7_blue1;
		nameFontRed = tahoma_7_red;
		nameFontYellow = tahoma_7_yellow;
		nameFontGreen = tahoma_7_green;
		tahoma_7_greySmall = tahoma_7_grey;
		tahoma_7b_yellowSmall2 = tahoma_7_yellow;
		tahoma_7b_green2Small = tahoma_7b_green2;
		tahoma_7_whiteSmall = tahoma_7_white;
		tahoma_7b_greenSmall = tahoma_7b_green;
		tahoma_7_blue1Small = tahoma_7_blue1;
		yAddFont = -3;
	}

	public void setHeight(int height)
	{
		this.height = height;
	}

	// Optimize: Use cached index lookup
	private int GetCharIndex(char c)
	{
		if (_charIndexCache.TryGetValue(c, out int index))
		{
			return index;
		}
		return 0;
	}

	public void drawString(mGraphics g, string st, int x, int y, int align)
	{
		if (string.IsNullOrEmpty(st)) return;

		int length = st.Length;
		int startX = align switch
		{
            LEFT => x,
			RIGHT => x - getWidth(st),
			_ => x - (getWidth(st) >> 1)
		};

		int currentX = startX;
		
		for (int i = 0; i < length; i++)
		{
			int charIndex = GetCharIndex(st[i]);
			if (charIndex >= 0 && charIndex < fImages.Length)
			{
				int[] charData = fImages[charIndex];
				int srcX = charData[0];
				int srcY = charData[1];
				int w = charData[2];
				int h = charData[3];

				if (srcY + h > imgFont.bitmap.Height)
				{
					srcY -= imgFont.bitmap.Height;
					srcX = imgFont.bitmap.Width >> 1;
				}

				g.drawRegion(imgFont, srcX, srcY, w, h, 0, currentX, y, 20);
				currentX += w + space;
			}
		}
	}

	public void drawStringBorder(mGraphics g, string st, int x, int y, int align)
	{
		drawString(g, st, x, y, align);
	}

	public void drawStringBorder(mGraphics g, string st, int x, int y, int align, mFont font2)
	{
		drawString(g, st, x, y, align, font2);
	}

	public void drawString(mGraphics g, string st, int x, int y, int align, mFont font)
	{
		if (string.IsNullOrEmpty(st)) return;

		int length = st.Length;
		int startX = align switch
		{
			LEFT => x,
			RIGHT => x - getWidth(st),
			_ => x - (getWidth(st) >> 1)
		};

		int currentX = startX;
		
		for (int i = 0; i < length; i++)
		{
			int charIndex = GetCharIndex(st[i]);
			if (charIndex >= 0 && charIndex < fImages.Length)
			{
				int[] charData = fImages[charIndex];
				int srcX = charData[0];
				int srcY = charData[1];
				int w = charData[2];
				int h = charData[3];

				if (srcY + h > imgFont.bitmap.Height)
				{
					srcY -= imgFont.bitmap.Height;
					srcX = imgFont.bitmap.Width >> 1;
				}

				if (!GameCanvas.lowGraphic && font != null)
				{
					g.drawRegion(font.imgFont, srcX, srcY, w, h, 0, currentX + 1, y, 20);
					g.drawRegion(font.imgFont, srcX, srcY, w, h, 0, currentX, y + 1, 20);
				}
				g.drawRegion(imgFont, srcX, srcY, w, h, 0, currentX, y, 20);
				currentX += w + space;
			}
		}
	}

	public MyVector splitFontVector(string src, int lineWidth)
	{
		MyVector myVector = new MyVector();
		if (string.IsNullOrEmpty(src)) return myVector;

		string text = string.Empty;
		for (int i = 0; i < src.Length; i++)
		{
			char ch = src[i];
			
			if (ch == '\n' || ch == '\b')
			{
				myVector.addElement(text);
				text = string.Empty;
				continue;
			}

			text += ch;
			
			if (getWidth(text) > lineWidth)
			{
				int num = text.Length - 1;
				while (num >= 0 && text[num] != ' ')
				{
					num--;
				}
				
				if (num < 0)
				{
					num = text.Length - 1;
				}
				
				myVector.addElement(text.Substring(0, num));
				i = i - (text.Length - num) + 1;
				text = string.Empty;
			}

			if (i == src.Length - 1 && !string.IsNullOrWhiteSpace(text))
			{
				myVector.addElement(text);
			}
		}
		
		return myVector;
	}

	public string[] splitFontArray(string src, int lineWidth)
	{
		MyVector myVector = splitFontVector(src, lineWidth);
		string[] array = new string[myVector.size()];
		for (int i = 0; i < myVector.size(); i++)
		{
			array[i] = (string)myVector.elementAt(i);
		}
		return array;
	}

	public int getWidth(string s)
	{
		if (string.IsNullOrEmpty(s)) return 0;

		int width = 0;
		for (int i = 0; i < s.Length; i++)
		{
			int charIndex = GetCharIndex(s[i]);
			if (charIndex >= 0 && charIndex < fImages.Length)
			{
				width += fImages[charIndex][2] + space;
			}
		}
		return width;
	}

	public int getWidthExactOf(string s)
	{
		try
		{
			return (int)measurePaint.MeasureText(s);
		}
		catch (Exception ex)
		{
			Cout.LogError("GET WIDTH OF " + s + " FAIL.\n" + ex.Message);
			return getWidthNotExactOf(s);
		}
	}

	public int getWidthNotExactOf(string s)
	{
		return s.Length * wO;
	}

	public int getHeight() => height;

	public void reloadImage()
	{
		imgFont = GameCanvas.loadImage(pathImage);
	}
}