#define DEBUG
using System;
using System.Collections;
using System.Diagnostics;

public class ImgByName
{
	public static MyHashTable hashImagePath = new MyHashTable();

	public static void SetImage(string name, Image img, sbyte nFrame)
	{
		hashImagePath.put(string.Empty + name, new MainImage(img, nFrame));
	}

	public static MainImage getImagePath(string nameImg, MyHashTable hash)
	{
		MainImage mainImage = (MainImage)hash.get(string.Empty + nameImg);
		if (mainImage == null)
		{
			mainImage = new MainImage();
			MainImage fromRms = getFromRms(nameImg);
			if (fromRms != null)
			{
				mainImage.img = fromRms.img;
				mainImage.nFrame = fromRms.nFrame;
			}
			hash.put(string.Empty + nameImg, mainImage);
		}
		mainImage.count = GameCanvas.timeNow / 1000;
		if (mainImage.img == null)
		{
			mainImage.timeImageNull--;
			if (mainImage.timeImageNull <= 0)
			{
				Service.gI().getImgByName(nameImg);
				mainImage.timeImageNull = 200;
			}
		}
		return mainImage;
	}

	public static MainImage getFromRms(string nameImg)
	{
		string text = "1ImgByName_" + nameImg;
		MainImage result = null;
		sbyte[] array = null;
		array = Rms.loadRMS(text);
		if (array == null)
		{
			return result;
		}
		try
		{
			result = new MainImage();
			result.nFrame = array[0];
			result.img = Image.createImage(array, 1, array.Length - 1);
			if (result.img != null)
			{
				return result;
			}
			return result;
		}
		catch (Exception)
		{
			Debug.WriteLine(text + ">>>>>getFromRms: nulllllllllll 2222");
			return null;
		}
	}

	public static void checkDelHash(MyHashTable hash, int minute, bool isTrue)
	{
		MyVector myVector = new MyVector("checkDelHash");
		if (isTrue)
		{
			hash.clear();
			return;
		}
		IDictionaryEnumerator enumerator = hash.GetEnumerator();
		while (enumerator.MoveNext())
		{
			MainImage mainImage = (MainImage)enumerator.Value;
			if (GameCanvas.timeNow / 1000 - mainImage.count > minute * 60)
			{
				string o = (string)enumerator.Key;
				myVector.addElement(o);
			}
		}
		for (int i = 0; i < myVector.size(); i++)
		{
			hash.remove((string)myVector.elementAt(i));
		}
	}
}
