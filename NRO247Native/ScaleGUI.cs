using System;

public class ScaleGUI
{
    public static float WIDTH;
    public static float HEIGHT;

    public static void initScaleGUI(int width, int height)
    {
        WIDTH = width;
        HEIGHT = height;
        Console.WriteLine($"Init Scale GUI: Screen.w={WIDTH} Screen.h={HEIGHT}");
    }
}