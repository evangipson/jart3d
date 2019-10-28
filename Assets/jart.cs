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
	private static List<Oscillator> oscList = new List<Oscillator>();
	public static int scaleTones;
	public static float[] possibleFrequencies;
	public static int[] scaleIntervals;
	public static float[] possibleTimings;
	private static bool isQuiet = false;

	private Menu menuComponent;

	public static void ToggleSongQuiet()
	{
		if (!isQuiet)
		{
			AudioListener.volume = 0.5f;
			isQuiet = true;
		}
		else
		{
			AudioListener.volume = 1.0f;
			isQuiet = false;
		}
	}

	private static int[] buildScaleIntervals()
	{
		scaleTones = Utils.Randomizer.Next(5, 16);
		int[] localScaleIntervals = new int[scaleTones];
		for (int i = 0; i < scaleTones; i++)
		{
			if (i == 0)
			{
				localScaleIntervals[0] = 0;
			}
			else
			{
				// how many half steps between each interval?
				localScaleIntervals[i] = localScaleIntervals[i - 1] + Utils.Randomizer.Next(1, 3);
			}
		}
		return localScaleIntervals;
	}

	public static float[] buildScaleFrequencies()
	{
		scaleIntervals = buildScaleIntervals();
		float[] scaleFrequencies = new float[scaleIntervals.Length];
		for (int i = 0; i < scaleIntervals.Length; i++)
		{
			if (i == 0)
			{
				// set the base tone
				scaleFrequencies[0] = 1.059463f * Utils.Randomizer.Next(200, 300);
			}
			else
			{
				// calculate the frequency based on the interval and base note
				scaleFrequencies[i] = scaleFrequencies[0] * Mathf.Pow(1.059463f, scaleIntervals[i]);
			}
		}
		return scaleFrequencies;
	}

	public static float[] buildNoteTimings()
	{
		float[] localNoteTimes = new float[6];
		for (int i = 0; i < localNoteTimes.Length; i++)
		{
			// quarter note at 60bpm is 1 second
			if (i == 0)
			{
				localNoteTimes[i] = Utils.Randomizer.Next(50, 500); // 1st note timing is a sixteenth note, or 1/4th of 1 second at 60bpm
			}
			else
			{
				localNoteTimes[i] = localNoteTimes[i - 1] * 1.33f;
			}
		}
		return localNoteTimes;
	}

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
			sr.transform.rotation = new Quaternion(Utils.Randomizer.Next(-360, 360), Utils.Randomizer.Next(-360, 360), Utils.Randomizer.Next(-360, 360), Utils.Randomizer.Next(-360, 360));
		}

		return sprite;
	}

	/**
	 * Creates a shape. Intended to be used to create Jartlets
	 * and the Jartboard, which is returned by default (using
	 * default parameters). */
	private static GameObject createShape(Color32 color, PrimitiveType typeOfShape, Quaternion skew, int width = 1, int height = 1, int depth = 1, float originX = 0, float originY = 0, float originZ = 0)
	{
		int shapeRoll = Utils.Randomizer.Next(0, 4);
		GameObject shape;

		// if the type is a quad or plane, we need to do special stuff to make it "3d"
		if (typeOfShape == PrimitiveType.Quad || typeOfShape == PrimitiveType.Plane)
		{
			// fake the planes by making cubes that are slim
			shape = GameObject.CreatePrimitive(PrimitiveType.Cube);
			shape.transform.localScale = new Vector3(width, height, 2);
		}
		else
		{
			shape = GameObject.CreatePrimitive(typeOfShape);
			shape.transform.localScale = new Vector3(width, height, depth);
		}
		// grab the shape's renderer, don't create a new one
		Renderer shapeRenderer = shape.GetComponent<Renderer>();
		// make sure the color is pure by using the mask shader
		shapeRenderer.material.shader = Shader.Find(Utils.GetRandomArrayItem(Constants.Possible3dShaders));
		// set a color after shader
		shapeRenderer.material.color = color;
		// set the position of the object
		shape.transform.position = new Vector3(originX, originY, originZ);
		shape.transform.rotation = skew;

		// set the rendering & shape layers
		//shapeRenderer.sortingOrder = zIndex;

		// make sure masking actually works
		//mr.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;

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
			if (i > 0 && Utils.Randomizer.Next(0, 100) > 40)
			{
				// get bigger or smaller
				if (Utils.Randomizer.Next(0, 10) > 7)
				{
					jartletSize = Utils.Randomizer.Next((int)(jartletSize * 1.0), (int)(jartletSize * 1.3));
				}
				else
				{
					jartletSize = Utils.Randomizer.Next((int)(jartletSize * 0.66), (int)(jartletSize * 1.0));
				}
				// now calculate size
				jartletWidth = Utils.Randomizer.Next((int)(jartletSize * 0.25), jartletSize);
				jartletHeight = Utils.Randomizer.Next((int)(jartletSize * 0.25), jartletSize);
				jartletDepth = Utils.Randomizer.Next((int)(jartletSize * 0.25), jartletSize);
				// first modify position
				if (Utils.Randomizer.Next(0, 10) > 3)
				{
					jartletPositionX += Utils.Randomizer.Next(0, (int)(jartletWidth * 0.75));
				}
				if (Utils.Randomizer.Next(0, 10) > 3)
				{
					jartletPositionY += Utils.Randomizer.Next(0, (int)(jartletHeight * 0.75));
				}
				if (Utils.Randomizer.Next(0, 10) > 3)
				{
					jartletPositionZ += Utils.Randomizer.Next(0, (int)(jartletDepth * 0.75));
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
				jartletPositionZ = Utils.Randomizer.Next((int)jartBoards[jartboardIndex].gameObject.transform.position.z - (int)(jartBoards[jartboardIndex].gameObject.transform.localScale.z * 0.5), (int)jartBoards[jartboardIndex].gameObject.transform.position.z + (int)(jartBoards[jartboardIndex].gameObject.transform.localScale.z * 0.5));
				//jartletPositionZ = (int)jartBoards[jartboardIndex].gameObject.transform.position.z;
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
	/// Will set an oscillators position similar to the passed in jartboard.
	/// </summary>
	/// <param name="jartboard"></param>
	private static void tieOscillatorToJartboard(GameObject jartboard) {
		// when you add the oscillator, it will start playing
		Oscillator newOsc = new GameObject("Oscillator").AddComponent<Oscillator>();
		newOsc.transform.parent = jartboard.transform; // force this new gameobject to be a child
		newOsc.transform.position = jartboard.transform.position; // stick the oscillator on the jartboard
		oscList.Add(newOsc);
	}

	/// <summary>
	/// Creates a jartboard and gets the next jartletAmount, and
	/// places the new jartboard at the passed in position.
	/// </summary>
	private static void createJartboard(Vector3 position)
	{
		// how big is this jartboard?
		jartboardSize = Utils.Randomizer.Next((int)(Constants.JartCubeSize * 0.1), (int)(Constants.JartCubeSize * 0.5));
		// how many jartlets should we have?
		totalJartletsPerJart = Utils.Randomizer.Next(3, 40);
		// how many oscillators per jartboard?
		//int oscillatorsPerJartboard = Utils.Randomizer.Next(1, 2);
		int oscillatorsPerJartboard = 1;
		// get a new scale & timings
		possibleFrequencies = buildScaleFrequencies();
		possibleTimings = buildNoteTimings();
		// now add the jartboard
		jartBoards.Add(createShape(
			Utils.GetRandomArrayItem(Colors.PossibleColorPalettes[ColorPaletteIndex]),
			Utils.GetRandomArrayItem(Constants.PossiblePrimitiveTypes),
			Quaternion.Euler(Utils.Randomizer.Next(0, 360), Utils.Randomizer.Next(0, 360), Utils.Randomizer.Next(0, 360)),
			Utils.Randomizer.Next((int)(jartboardSize * 0.5), jartboardSize),
			Utils.Randomizer.Next((int)(jartboardSize * 0.5), jartboardSize),
			Utils.Randomizer.Next((int)(jartboardSize * 0.5), jartboardSize),
			position.x,
			position.y,
			position.z
		));
		// and then attach the oscillator(s) to it
		for(int i = 0; i < oscillatorsPerJartboard; i++)
		{
			tieOscillatorToJartboard(jartBoards[jartBoards.Count - 1]);
		}
	}

	public static void clearJart()
	{
		// clear out old oscillators
		for (int i = 0; i < oscList.Count; i++)
		{
			oscList[i].DestroyOscillator();
		}
		oscList.Clear();
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
		totalJartboards = Utils.Randomizer.Next(1, 20);
		//totalJartboards = 0;
	}

	public static void NewJart()
	{
		clearJart();
		Vector3 jartboardPosition = new Vector3();
		for (int i = 0; i < totalJartboards; i++)
		{
			// place the jartboard anywhere in the jartcube
			jartboardPosition.x = Utils.Randomizer.Next(-Constants.JartCubeSize, Constants.JartCubeSize);
			jartboardPosition.y = Utils.Randomizer.Next(-Constants.JartCubeSize, Constants.JartCubeSize);
			jartboardPosition.z = Utils.Randomizer.Next(-Constants.JartCubeSize, Constants.JartCubeSize);
			createJartboard(jartboardPosition);
			createJartlets(totalJartletsPerJart, i);
		}
	}

	public void Start()
	{
		menuComponent = GameObject.Find("Menu").GetComponent<Menu>();
		NewJart();
	}

	public void Update()
	{
		// make sure the menu isn't up
		if (!menuComponent.isPaused)
		{
			// the user has clicked and let up
			// note: this is less specific, so it must come later
			if (Input.GetMouseButtonDown(0))
			{
				//This gets the Main Camera from the Scene
				Camera mainCamera = Camera.main;
				Vector3 jartboardPosition = new Vector3();
				jartboardPosition = Camera.main.transform.position + Camera.main.transform.forward * Utils.Randomizer.Next(jartboardSize, jartboardSize * 4);
				createJartboard(jartboardPosition);
				createJartlets(totalJartletsPerJart, jartBoards.Count - 1);
			}
			// the user has right clicked
			if (Input.GetMouseButtonDown(1))
			{
				if (jartBoards.Count > 0)
				{
					// how many jartlets should we have?
					totalJartletsPerJart = Utils.Randomizer.Next(3, 40);
					// Add jartlets to the last jartboard created
					createJartlets(totalJartletsPerJart, jartBoards.Count - 1);
				}
			}
		}
	}
}
