using System.Collections.Generic;
using UnityEngine;

public class Jart : MonoBehaviour
{
	private static List<GameObject> jartBoards = new List<GameObject>();
	private static List<GameObject> jartlets = new List<GameObject>();
	private static int totalJartletsPerJart = Utils.Randomizer.Next(1, 25);// pick out the color palette
	public static int ColorPaletteIndex = Utils.Randomizer.Next(0, Colors.PossibleColorPalettes.Count - 1);
	private static int totalJarts = Utils.Randomizer.Next(2, 40);
	// set bounds for jartboards, the base of jarts
	private static int jartboardMinSize = 2;
	private static int jartboardMaxSize = Utils.Randomizer.Next(5, 50);
	private static int jartboardMaxScale = Utils.Randomizer.Next(2, 50);
	// the jartlets will be slightly smaller
	public static int jartletMinSize = (int)(jartboardMinSize * 0.8);
	public static int jartletMaxSize = (int)(jartboardMaxSize * 0.8);
	public static int jartletMaxScale = Utils.Randomizer.Next((int)(jartboardMaxScale * 0.25), jartboardMaxScale);

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
	private static GameObject createShape(Color color, int width = 1, int height = 1, int depth = 1, float originX = 0, float originY = 0, float originZ = 0, bool skew = false)
	{
		int shapeRoll = Utils.Randomizer.Next(0, 4);
		GameObject shape = GameObject.CreatePrimitive(Utils.GetRandomArrayItem(Constants.PossiblePrimitiveTypes));
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
		// skew that renderer all up if we want it skewed
		if (skew)
		{
			shape.transform.rotation = new Quaternion(Utils.Randomizer.Next(-360, 360), Utils.Randomizer.Next(-360, 360), 0, 0);
		}
		return shape;
	}

	/// <summary>
	/// Will create and place and skew a jarlet.
	/// TODO: implement a small chance to copy previous jartlet.
	/// </summary>
	private static void createJartlets(int jartletAmount, int jartboardIndex)
	{
		int renderingLayer = jartBoards[jartboardIndex].GetComponent<Renderer>().sortingOrder; // will help define z-axis
		for (int i = 0; i < jartletAmount; i++)
		{
			jartlets.Add(createShape(
				Utils.GetRandomArrayItem(Colors.PossibleColorPalettes[ColorPaletteIndex]),
				// change the length, width, and depth based off of how big the cube is.
				Utils.Randomizer.Next(jartletMinSize, jartletMaxSize) * Utils.Randomizer.Next(1, jartletMaxScale),
				Utils.Randomizer.Next(jartletMinSize, jartletMaxSize) * Utils.Randomizer.Next(1, jartletMaxScale),
				Utils.Randomizer.Next(jartletMinSize, jartletMaxSize) * Utils.Randomizer.Next(1, jartletMaxScale),
				// place the jartlet _relative_ to the jartboard
				Utils.Randomizer.Next((int)jartBoards[jartboardIndex].gameObject.transform.position.x - (int)(jartBoards[jartboardIndex].gameObject.transform.localScale.x * 0.5), (int)jartBoards[jartboardIndex].gameObject.transform.position.x + (int)(jartBoards[jartboardIndex].gameObject.transform.localScale.x * 0.5)),
				Utils.Randomizer.Next((int)jartBoards[jartboardIndex].gameObject.transform.position.y - (int)(jartBoards[jartboardIndex].gameObject.transform.localScale.y * 0.5), (int)jartBoards[jartboardIndex].gameObject.transform.position.y + (int)(jartBoards[jartboardIndex].gameObject.transform.localScale.y * 0.5)),
				Utils.Randomizer.Next((int)jartBoards[jartboardIndex].gameObject.transform.position.z - (int)(jartBoards[jartboardIndex].gameObject.transform.localScale.z * 0.5), (int)jartBoards[jartboardIndex].gameObject.transform.position.z + (int)(jartBoards[jartboardIndex].gameObject.transform.localScale.z * 0.5)),
				true
			));
		}
	}

	/// <summary>
	/// Creates a jartboard and gets the next jartletAmount.
	/// </summary>
	private static void createJartboard(int jartboardIndex)
	{
		jartBoards.Add(createShape(
			Utils.GetRandomArrayItem(Colors.PossibleColorPalettes[ColorPaletteIndex]),
			Utils.Randomizer.Next(jartboardMinSize, jartboardMaxSize) * Utils.Randomizer.Next(1, jartboardMaxScale),
			Utils.Randomizer.Next(jartboardMinSize, jartboardMaxSize) * Utils.Randomizer.Next(1, jartboardMaxScale),
			Utils.Randomizer.Next(jartboardMinSize, jartboardMaxSize) * Utils.Randomizer.Next(1, jartboardMaxScale),
			// place the jartboard anywhere in the jartcube
			Utils.Randomizer.Next(-Constants.JartCubeSize, Constants.JartCubeSize),
			Utils.Randomizer.Next(-Constants.JartCubeSize, Constants.JartCubeSize),
			Utils.Randomizer.Next(-Constants.JartCubeSize, Constants.JartCubeSize),
			false
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
		// reset all the jart variables
		totalJartletsPerJart = Utils.Randomizer.Next(1, 25);
		totalJarts = Utils.Randomizer.Next(2, 40);
		jartboardMinSize = 2;
		jartboardMaxSize = Utils.Randomizer.Next(5, 50);
		jartboardMaxScale = Utils.Randomizer.Next(2, 50);
		jartletMinSize = (int)(jartboardMinSize * 0.8);
		jartletMaxSize = (int)(jartboardMaxSize * 0.8);
		jartletMaxScale = Utils.Randomizer.Next((int)(jartboardMaxScale * 0.25), jartboardMaxScale);
	}

	public static void NewJart()
	{
		clearJart();
		for (int i = 0; i < totalJarts; i++)
		{
			createJartboard(i);
			createJartlets(totalJartletsPerJart, i);
		}
	}

	public void Start()
	{
		NewJart();
	}
}
