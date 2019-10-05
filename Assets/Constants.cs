using UnityEngine;

/// <summary>
/// A static class filled with values that will never change
/// and/or values used by multiple functions.
/// </summary>
public static class Constants
{
	public static string[] PossibleSpriteShaders =
	{
		"Sprites/Default",
		"Sprites/Mask"
	};
	public static string[] Possible3dShaders =
	{
		//"Standard (Specular setup)"
		"Unlit/Color"
	};
	public static PrimitiveType[] PossiblePrimitiveTypes =
	{
		PrimitiveType.Cube,
		PrimitiveType.Capsule,
		PrimitiveType.Cylinder,
		PrimitiveType.Plane,
		PrimitiveType.Quad,
		PrimitiveType.Sphere
	};
	// how far the jart universe will expand.
	public static int JartCubeSize = 1000;
	public static int TotalJarts = Utils.Randomizer.Next(2, 40);
	public static int JartboardMinSize = 2;
	public static int JartboardMaxSize = Utils.Randomizer.Next(5, 50);
	public static int JartboardMaxScale = Utils.Randomizer.Next(2, 50);
	// the jartlets will be slightly smaller
	public static int JartletMinSize = (int)(JartboardMinSize * 0.8);
	public static int JartletMaxSize = (int)(JartboardMaxSize * 0.8);
	public static int JartletMaxScale = Utils.Randomizer.Next((int)(JartboardMaxScale * 0.25), JartboardMaxScale);
	// pick out the color palette
	public static int ColorPaletteIndex = Utils.Randomizer.Next(0, Colors.PossibleColorPalettes.Count - 1);
}
