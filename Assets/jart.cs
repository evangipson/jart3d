using System.Collections.Generic;
using UnityEngine;

public class Jart : MonoBehaviour
{
	// I want to use System Random, not UnityEngine Random
	private System.Random randomizer = new System.Random();
	// TODO: Fill with nice colors, probably implement pallettes
	private Color[] possibleColors = {
		Color.red,
		Color.blue,
		Color.green,
		Color.cyan,
		Color.magenta,
		Color.yellow
	};
	private string[] possibleSpriteShaders =
	{
		"Sprites/Default",
		"Sprites/Mask"
	};
	private string[] possible3dShaders =
	{
		"Standard",
		"Standard (Specular setup)"
	};
	private List<GameObject> jartBoards = new List<GameObject>();
	private List<GameObject> jartlets = new List<GameObject>();
	private int jartboardMinSize = 3;
	private int jartboardMaxSize = 15;
	private int jartCubeSize = 1000; // how far the jart will expand
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
		sr.material.shader = Shader.Find(getRandomArrayItem(possibleSpriteShaders));
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
		go.transform.position = new Vector3(originX, originY, randomizer.Next(-zIndex, zIndex));
		go.transform.localScale = new Vector3(width, height, 0);
		// skew that renderer all up if we want it skewed
		if (skew)
		{
			sr.transform.rotation = new Quaternion(randomizer.Next(-360, 360), randomizer.Next(-360, 360), randomizer.Next(-360, 360), 0);
		}

		return sprite;
	}

	/**
	 * Creates a shape. Intended to be used to create Jartlets
	 * and the Jartboard, which is returned by default (using
	 * default parameters). */
	private GameObject createShape(Color color, int width = 1, int height = 1, int depth = 1, float originX = 0, float originY = 0, float originZ = 0, bool skew = false)
	{
		int shapeRoll = randomizer.Next(0, 3);
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
		shapeRenderer.material.shader = Shader.Find(getRandomArrayItem(possible3dShaders));
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
			shape.transform.rotation = new Quaternion(randomizer.Next(-360, 360), randomizer.Next(-360, 360), randomizer.Next(-360, 360), 0);
		}
		return shape;
	}

	/// <summary>
	/// A generic function that will return a random element
	/// of any array you pass it.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="array"></param>
	/// <returns></returns>
	private T getRandomArrayItem<T>(T[] array)
	{
		return array[randomizer.Next(0, array.Length)];
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
				getRandomArrayItem(possibleColors),
				randomizer.Next(jartboardMinSize, jartboardMaxSize),
				randomizer.Next(jartboardMinSize, jartboardMaxSize),
				randomizer.Next(jartboardMinSize, jartboardMaxSize),
				// place the jartlet _relative_ to the jartboard
				randomizer.Next((int)jartBoards[jartboardIndex].gameObject.transform.position.x - randomizer.Next(jartboardMinSize, jartboardMaxSize), (int)jartBoards[jartboardIndex].gameObject.transform.position.x + randomizer.Next(jartboardMinSize, jartboardMaxSize)),
				randomizer.Next((int)jartBoards[jartboardIndex].gameObject.transform.position.y - randomizer.Next(jartboardMinSize, jartboardMaxSize), (int)jartBoards[jartboardIndex].gameObject.transform.position.y + randomizer.Next(jartboardMinSize, jartboardMaxSize)),
				randomizer.Next((int)jartBoards[jartboardIndex].gameObject.transform.position.z - randomizer.Next(jartboardMinSize, jartboardMaxSize), (int)jartBoards[jartboardIndex].gameObject.transform.position.z + randomizer.Next(jartboardMinSize, jartboardMaxSize)),
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
			getRandomArrayItem(possibleColors),
			randomizer.Next(jartboardMinSize, jartboardMaxSize),
			randomizer.Next(jartboardMinSize, jartboardMaxSize),
			randomizer.Next(jartboardMinSize, jartboardMaxSize),
			// place the jartboard anywhere in the jartcube
			randomizer.Next(-jartCubeSize, jartCubeSize),
			randomizer.Next(-jartCubeSize, jartCubeSize),
			randomizer.Next(-jartCubeSize, jartCubeSize),
			false
		));
	}

	public void Start()
	{
		int jartBoards = 50;
		for (int i = 0; i < jartBoards; i++)
		{
			createJartboard(i);
			createJartlets(randomizer.Next(2, 15), i);
		}
	}
}
