namespace ZoomMod;

using System;

public class Config
{
    [Alias(["zoom"])]
    public static float ZoomFactor = 1.5f;

    //[Alias(["minzoom"])]
    //public static int MaxZoomLevel = 5;

    [Alias(["in"])]
    public static bool SmoothIn = false;

    [Alias(["out"])]
    public static bool SmoothOut = false;
}

internal class AliasAttribute : Attribute
{
    public string[] Aliases
    {
        get;
    }
    public AliasAttribute(string[] aliases)
    {
        Aliases = aliases;
    }
}