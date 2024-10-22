﻿using System.Collections.Generic;
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
		//"Skybox/Cubemap",
		"Unlit/Color",
		"Unlit/IntersectionShader",
		"Custom/ToonShader",
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
}
