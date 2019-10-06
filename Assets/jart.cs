using System;
using System.Collections.Generic;
using UnityEngine;

public class Jart : MonoBehaviour
{
	private static List<GameObject> jartBoards = new List<GameObject>();
	private static List<GameObject> jartlets = new List<GameObject>();
	private static int totalJartletsPerJart;
	public static int ColorPaletteIndex;
	private static int totalJartboards;
	private static int jartboardSize;
	public static int jartletSize;
	// these are declared outside of a function because sometimes i don't want them to change when the function is called.
	private static int jartletWidth;
	private static int jartletHeight;
	private static int jartletDepth;
	private static int jartletPositionX;
	private static int jartletPositionY;
	private static int jartletPositionZ;
	private static Quaternion jartletSkew;
	private static Color32 jartletColor;
	private static PrimitiveType jartletType;

	/**
	 * Creates a sprite. Intended to be used to create Jartlets
	 * and the Jartboard, which is returned by default (using
	 * default parameters). */
	private static Sprite createSprite(Color color, int width = 1, int height = 1, float originX = 0, float originY = 0, int zIndex = -1, bool skew = false)
	{
		GameObject go = new GameObject();
		SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
		Sprite sprite = Sprite.Create(new Texture2D(width, height), new Rect(0, 0, width, height), new Vector2(1, 1));

		// make sure the color is pure by using the mask shader
		sr.material.shader = Shader.Find(Utils.GetRandomArrayItem(Constants.PossibleSpriteShaders));
		// set a color after shader
		sr.material.color = color;
		if (sr.material.shader == Shader.Find("Sprites/Mask"))
		{
			sr.material.color = Color.black; // turn black if it's going to be an alpha mask
		}

		// set the z-index
		sr.sortingOrder = zIndex;

		// make sure masking actually works
		sr.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;

		// tell the sprite renderer about the sprite
		sr.sprite = sprite;

		// set the position of the object
		go.transform.position = new Vector3(originX, originY, Utils.Randomizer.Next(-zIndex, zIndex));
		go.transform.localScale = new Vector3(width, height, 0);
		// skew that renderer all up if we want it skewed
		if (skew)
		{
			sr.transform.rotation = new Quaternion(Utils.Randomizer.Next(-360, 360), Utils.Randomizer.Next(-360, 360), Utils.Randomizer.Next(-360, 360), Utils.Randomizer.Next(-360, 360));
		}

		return sprite;
	}

	/**
	 * Creates a shape. Intended to be used to create Jartlets
	 * and the Jartboard, which is returned by default (using
	 * default parameters). */
	private static GameObject createShape(Color color, PrimitiveType typeOfShape, Quaternion skew, int width = 1, int height = 1, int depth = 1, float originX = 0, float originY = 0, float originZ = 0)
	{
		int shapeRoll = Utils.Randomizer.Next(0, 4);
		GameObject shape = GameObject.CreatePrimitive(typeOfShape);
		Renderer shapeRenderer = shape.GetComponent<Renderer>(); // grab the shape's renderer, don't create a new one

		// make sure the color is pure by using the mask shader
		shapeRenderer.material.shader = Shader.Find(Utils.GetRandomArrayItem(Constants.Possible3dShaders));
		// set a color after shader
		shapeRenderer.material.color = color;

		// set the rendering & shape layers
		//shapeRenderer.sortingOrder = zIndex;

		// make sure masking actually works
		//mr.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;

		// set the position of the object
		shape.transform.position = new Vector3(originX, originY, originZ);
		shape.transform.localScale = new Vector3(width, height, depth);
		shape.transform.rotation = skew;
		return shape;
	}

	/// <summary>
	/// Will create and place and skew a jarlet.
	/// TODO: implement a small chance to copy previous jartlet.
	/// </summary>
	private static void createJartlets(int jartletAmount, int jartboardIndex)
	{
		// first the jartlet's parameters will be either modified or made up,
		// then the jartlet will be created and added to the jartlets list.
		for (int i = 0; i < jartletAmount; i++)
		{
			// chance to repeat the jartlet with a slight shift, without changing width/height/depth.
			// note: we have to have made a jartlet before repeating one.
			if (i > 0 && Utils.Randomizer.Next(0, 100) > 12)
			{
				// first modify position
				if (Utils.Randomizer.Next(0, 10) > 3)
				{
					jartletPositionX += Utils.Randomizer.Next(0, (int)(jartletWidth * 0.5));
				}
				if (Utils.Randomizer.Next(0, 10) > 3)
				{
					jartletPositionY += Utils.Randomizer.Next(0, (int)(jartletHeight * 0.5));
				}
				if (Utils.Randomizer.Next(0, 10) > 3)
				{
					jartletPositionZ += Utils.Randomizer.Next(0, (int)(jartletDepth * 0.5));
				}
				// now rotation
				if (Utils.Randomizer.Next(0, 10) > 7)
				{
					jartletSkew.x += Utils.Randomizer.Next(-10, 10);
				}
				if (Utils.Randomizer.Next(0, 10) > 7)
				{
					jartletSkew.y += Utils.Randomizer.Next(-10, 10);
				}
				if (Utils.Randomizer.Next(0, 10) > 7)
				{
					jartletSkew.z += Utils.Randomizer.Next(-10, 10);
				}
				// get bigger or smaller
				if (Utils.Randomizer.Next(0, 10) > 7)
				{
					jartletSize = Utils.Randomizer.Next((int)(jartletSize * 1.1), (int)(jartletSize * 1.3));
				}
				else
				{
					jartletSize = Utils.Randomizer.Next((int)(jartletSize * 0.66), (int)(jartletSize * 0.8));
				}
			}
			// generate new parameters for a unique jartlet
			else
			{
				jartletSize = Utils.Randomizer.Next((int)(jartBoards[jartboardIndex].gameObject.transform.localScale.x * 0.075), (int)(jartBoards[jartboardIndex].gameObject.transform.localScale.x * 0.5));
				jartletColor = Utils.GetRandomArrayItem(Colors.PossibleColorPalettes[ColorPaletteIndex]);
				jartletType = Utils.GetRandomArrayItem(Constants.PossiblePrimitiveTypes);
				jartletWidth = Utils.Randomizer.Next((int)(jartletSize * 0.25), jartletSize);
				jartletHeight = Utils.Randomizer.Next((int)(jartletSize * 0.25), jartletSize);
				jartletDepth = Utils.Randomizer.Next((int)(jartletSize * 0.25), jartletSize);
				jartletPositionX = Utils.Randomizer.Next((int)jartBoards[jartboardIndex].gameObject.transform.position.x - (int)(jartBoards[jartboardIndex].gameObject.transform.localScale.x * 0.5), (int)jartBoards[jartboardIndex].gameObject.transform.position.x + (int)(jartBoards[jartboardIndex].gameObject.transform.localScale.x * 0.5));
				jartletPositionY = Utils.Randomizer.Next((int)jartBoards[jartboardIndex].gameObject.transform.position.y - (int)(jartBoards[jartboardIndex].gameObject.transform.localScale.y * 0.5), (int)jartBoards[jartboardIndex].gameObject.transform.position.y + (int)(jartBoards[jartboardIndex].gameObject.transform.localScale.y * 0.5));
				//jartletPositionZ = Utils.Randomizer.Next((int)jartBoards[jartboardIndex].gameObject.transform.position.z - (int)(jartBoards[jartboardIndex].gameObject.transform.localScale.z * 0.5), (int)jartBoards[jartboardIndex].gameObject.transform.position.z + (int)(jartBoards[jartboardIndex].gameObject.transform.localScale.z * 0.5));
				jartletPositionZ = (int)jartBoards[jartboardIndex].gameObject.transform.position.z;
				jartletSkew = Quaternion.Euler(Utils.Randomizer.Next(0, 360), Utils.Randomizer.Next(0, 360), Utils.Randomizer.Next(0, 360));
			}
			jartlets.Add(createShape(
				jartletColor,
				jartletType,
				jartletSkew,
				jartletWidth,
				jartletHeight,
				jartletDepth,
				jartletPositionX,
				jartletPositionY,
				jartletPositionZ
			));
		}
	}

	/// <summary>
	/// Creates a jartboard and gets the next jartletAmount.
	/// </summary>
	private static void createJartboard()
	{
		jartBoards.Add(createShape(
			Utils.GetRandomArrayItem(Colors.PossibleColorPalettes[ColorPaletteIndex]),
			Utils.GetRandomArrayItem(Constants.PossiblePrimitiveTypes),
			Quaternion.Euler(Utils.Randomizer.Next(0, 360), Utils.Randomizer.Next(0, 360), Utils.Randomizer.Next(0, 360)),
			Utils.Randomizer.Next((int)(jartboardSize * 0.5), jartboardSize),
			Utils.Randomizer.Next((int)(jartboardSize * 0.5), jartboardSize),
			Utils.Randomizer.Next((int)(jartboardSize * 0.5), jartboardSize),
			// place the jartboard anywhere in the jartcube
			Utils.Randomizer.Next(-Constants.JartCubeSize, Constants.JartCubeSize),
			Utils.Randomizer.Next(-Constants.JartCubeSize, Constants.JartCubeSize),
			Utils.Randomizer.Next(-Constants.JartCubeSize, Constants.JartCubeSize)
		));
	}

	public static void clearJart()
	{
		// remove jartboards and jartlets from unity
		jartBoards.ForEach(delegate(GameObject jartBoard){
			Destroy(jartBoard);
		});
		jartlets.ForEach(delegate (GameObject jartlets) {
			Destroy(jartlets);
		});
		// empty out the lists containing all the jarts
		jartBoards.Clear();
		jartlets.Clear();
		// pick out the color palette
		ColorPaletteIndex = Utils.Randomizer.Next(0, Colors.PossibleColorPalettes.Count - 1);
		// define how many jartboards this jart will have
		totalJartboards = Utils.Randomizer.Next(1, 5);
	}

	public static void NewJart()
	{
		clearJart();
		for (int i = 0; i < totalJartboards; i++)
		{
			totalJartletsPerJart = Utils.Randomizer.Next(3, 40);
			jartboardSize = Utils.Randomizer.Next((int)(Constants.JartCubeSize * 0.1), (int)(Constants.JartCubeSize * 0.5));
			createJartboard();
			createJartlets(totalJartletsPerJart, i);
		}
	}

	public void Start()
	{
		NewJart();
	}
}
