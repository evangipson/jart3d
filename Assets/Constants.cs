using UnityEngine;

/// <summary>
/// A static class filled with values that will never change
/// and/or values used by multiple functions.
/// </summary>
public static class Constants
{
	// TODO: Fill with nice colors, probably implement pallettes
	public static Color[] PossibleColors = {
		Color.red,
		Color.blue,
		Color.green,
		Color.cyan,
		Color.magenta,
		Color.yellow
	};
	public static string[] PossibleSpriteShaders =
	{
		"Sprites/Default",
		"Sprites/Mask"
	};
	public static string[] Possible3dShaders =
	{
		"Standard (Specular setup)"
	};
	// how far the jart universe will expand.
	public static int JartCubeSize = 1000;
	public static int TotalJarts = Utils.Randomizer.Next(5, 100);
	public static int JartboardMinSize = 2;
	public static int JartboardMaxSize = Utils.Randomizer.Next(10, 100);
	public static int JartboardMaxScale = Utils.Randomizer.Next(2, 10);
	// the jartlets will be slightly smaller
	public static int JartletMinSize = (int)(JartboardMinSize * 0.8);
	public static int JartletMaxSize = (int)(JartboardMaxSize * 0.8);
	public static int JartletMaxScale = Utils.Randomizer.Next(2, 5);
}
