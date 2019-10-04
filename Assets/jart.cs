using System.Collections.Generic;
using UnityEngine;

public class Jart : MonoBehaviour
{
	private List<GameObject> jartBoards = new List<GameObject>();
	private List<GameObject> jartlets = new List<GameObject>();
	/**
	 * Creates a sprite. Intended to be used to create Jartlets
	 * and the Jartboard, which is returned by default (using
	 * default parameters). */
	private Sprite createSprite(Color color, int width = 1, int height = 1, float originX = 0, float originY = 0, int zIndex = -1, bool skew = false)
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
			sr.transform.rotation = new Quaternion(Utils.Randomizer.Next(-360, 360), Utils.Randomizer.Next(-360, 360), Utils.Randomizer.Next(-360, 360), 0);
		}

		return sprite;
	}

	/**
	 * Creates a shape. Intended to be used to create Jartlets
	 * and the Jartboard, which is returned by default (using
	 * default parameters). */
	private GameObject createShape(Color color, int width = 1, int height = 1, int depth = 1, float originX = 0, float originY = 0, float originZ = 0, bool skew = false)
	{
		int shapeRoll = Utils.Randomizer.Next(0, 4);
		GameObject shape = GameObject.CreatePrimitive(PrimitiveType.Cube);
		// shape can be capsule or sphere randomly
		if (shapeRoll > 1)
		{
			shape = GameObject.CreatePrimitive(PrimitiveType.Capsule);
		}
		else if(shapeRoll > 2)
		{
			shape = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		}
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
			shape.transform.rotation = new Quaternion(Utils.Randomizer.Next(-360, 360), Utils.Randomizer.Next(-360, 360), Utils.Randomizer.Next(-360, 360), 0);
		}
		return shape;
	}

	/// <summary>
	/// Will create and place and skew a jarlet.
	/// TODO: implement a small chance to copy previous jartlet.
	/// </summary>
	private void createJartlets(int jartletAmount, int jartboardIndex)
	{
		int renderingLayer = jartBoards[jartboardIndex].GetComponent<Renderer>().sortingOrder; // will help define z-axis
		for (int i = 0; i < jartletAmount; i++)
		{
			jartlets.Add(createShape(
				Utils.GetRandomArrayItem(Constants.PossibleColors),
				// change the length, width, and depth based off of how big the cube is.
				Utils.Randomizer.Next(Constants.JartletMinSize, Constants.JartletMaxSize) * Utils.Randomizer.Next(1, Constants.JartletMaxScale),
				Utils.Randomizer.Next(Constants.JartletMinSize, Constants.JartletMaxSize) * Utils.Randomizer.Next(1, Constants.JartletMaxScale),
				Utils.Randomizer.Next(Constants.JartletMinSize, Constants.JartletMaxSize) * Utils.Randomizer.Next(1, Constants.JartletMaxScale),
				// place the jartlet _relative_ to the jartboard
				Utils.Randomizer.Next((int)jartBoards[jartboardIndex].gameObject.transform.position.x - Utils.Randomizer.Next(Constants.JartboardMinSize, Constants.JartboardMaxSize), (int)jartBoards[jartboardIndex].gameObject.transform.position.x + Utils.Randomizer.Next(Constants.JartboardMinSize, Constants.JartboardMaxSize)),
				Utils.Randomizer.Next((int)jartBoards[jartboardIndex].gameObject.transform.position.y - Utils.Randomizer.Next(Constants.JartboardMinSize, Constants.JartboardMaxSize), (int)jartBoards[jartboardIndex].gameObject.transform.position.y + Utils.Randomizer.Next(Constants.JartboardMinSize, Constants.JartboardMaxSize)),
				Utils.Randomizer.Next((int)jartBoards[jartboardIndex].gameObject.transform.position.z - Utils.Randomizer.Next(Constants.JartboardMinSize, Constants.JartboardMaxSize), (int)jartBoards[jartboardIndex].gameObject.transform.position.z + Utils.Randomizer.Next(Constants.JartboardMinSize, Constants.JartboardMaxSize)),
				true
			));
		}
	}

	/// <summary>
	/// Creates a jartboard and gets the next jartletAmount.
	/// </summary>
	private void createJartboard(int jartboardIndex)
	{
		jartBoards.Add(createShape(
			Utils.GetRandomArrayItem(Constants.PossibleColors),
			Utils.Randomizer.Next(Constants.JartboardMinSize, Constants.JartboardMaxSize) * Utils.Randomizer.Next(1, Constants.JartboardMaxScale),
			Utils.Randomizer.Next(Constants.JartboardMinSize, Constants.JartboardMaxSize) * Utils.Randomizer.Next(1, Constants.JartboardMaxScale),
			Utils.Randomizer.Next(Constants.JartboardMinSize, Constants.JartboardMaxSize) * Utils.Randomizer.Next(1, Constants.JartboardMaxScale),
			// place the jartboard anywhere in the jartcube
			Utils.Randomizer.Next(-Constants.JartCubeSize, Constants.JartCubeSize),
			Utils.Randomizer.Next(-Constants.JartCubeSize, Constants.JartCubeSize),
			Utils.Randomizer.Next(-Constants.JartCubeSize, Constants.JartCubeSize),
			false
		));
	}

	public void Start()
	{
		int totalJartletsPerJart = Utils.Randomizer.Next(1, 50);
		for (int i = 0; i < Constants.TotalJarts; i++)
		{
			totalJartletsPerJart = Utils.Randomizer.Next(1, 50);
			createJartboard(i);
			createJartlets(totalJartletsPerJart, i);
		}
	}
}
