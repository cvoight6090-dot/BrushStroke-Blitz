using UnityEngine;

// Places the six colors on a wheel and measures how far apart two colors are
public static class ColorWheel
{
    static int Position(ElementColor color)
    {
        switch (color)
        {
        case ElementColor.Red: return 0;
        case ElementColor.Orange: return 1;
        case ElementColor.Yellow: return 2;
        case ElementColor.Green: return 3;
        case ElementColor.Blue: return 4;
        default: return 5; //purple
        }
    }

    public static int Distance(ElementColor a, ElementColor b)
    {
        int gap = Mathf.Abs(Position(a) - Position(b));
        return Mathf.Min(gap, 6 - gap);
    }


}
