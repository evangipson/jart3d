using System.Collections.Generic;
using UnityEngine;

public class Jart : MonoBehaviour
{
	private static List<GameObject> jartBoards = new List<GameObject>();
	private static List<GameObject> jartlets = new List<GameObject>();
	private static int totalJartletsPerJart;
	private static int minJartlets = 4;
	private static int maxJartlets = 70;
	public static int ColorPaletteIndex;
	private static int totalJartboards;
	private static int jartboardSize;
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
	public static string worldShaderName;

	private Menu menuComponent;

	// music "brain" functions
	public static void ToggleSongQuiet()
	{
		if (!isQuiet)
		{
			AudioListener.volume *= 0.5f;
			isQuiet = true;
		}
		else
		{
			AudioListener.volume *= 2f;
			isQuiet = false;
		}
	}

	private static int[] buildScaleIntervals()
	{
		List<List<int>> potentialScaleIntervals = new List<List<int>>();
		// set up all potential scales
		potentialScaleIntervals.Add(new List<int> { 0, 2, 4, 5, 7, 9, 11 }); // major
		potentialScaleIntervals.Add(new List<int> { 0, 2, 3, 5, 7, 8, 10 }); // natural minor
		potentialScaleIntervals.Add(new List<int> { 0, 2, 3, 5, 7, 8, 11 }); // harmonic minor
		potentialScaleIntervals.Add(new List<int> { 0, 2, 3, 5, 7, 9, 11 }); // melodic minor
		potentialScaleIntervals.Add(new List<int> { 0, 2, 4, 7, 9 }); // pentatonic
		potentialScaleIntervals.Add(new List<int> { 0, 2, 5, 10 }); // custom evan ambient
		potentialScaleIntervals.Add(new List<int> { 0, 2, 4, 5, 7, 9, 10 }); // mixolydian
		potentialScaleIntervals.Add(new List<int> { 0, 2, 4, 6, 7, 9, 11 }); // lydian
		potentialScaleIntervals.Add(new List<int> { 0, 3, 5, 6, 7, 10 }); // blues
		potentialScaleIntervals.Add(new List<int> { 0, 3, 4, 7, 8, 11 }); // hexatonic
		List<int> localScaleIntervals = new List<int>() { 0 };
		if (Utils.Randomizer.Next(0, 100) > 66)
		{
			int scaleTones = Utils.Randomizer.Next(4, 16);
			// generate a new unique scale
			// note: i = 1 to begin loop here because we set first interval above
			// in the localScaleIntervals initialization
			for (int i = 1; i < scaleTones; i++)
			{
				// how many half steps between each interval?
				localScaleIntervals.Add(localScaleIntervals[i - 1] + Utils.Randomizer.Next(1, 3));
			}
		}
		else
		{
			// get a scale from the presets
			localScaleIntervals = potentialScaleIntervals.ToArray()[Utils.Randomizer.Next(0, potentialScaleIntervals.Count)];
		}
		return localScaleIntervals.ToArray();
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
		// which shader are we going to use this time?
		//worldShaderName = Constants.Possible3dShaders[Utils.Randomizer.Next(0, Constants.Possible3dShaders.Length)];
		shapeRenderer.material.shader = Shader.Find(worldShaderName);

		// set a color after shader
		shapeRenderer.material.color = color;
		// set the position of the object
		shape.transform.position = new Vector3(originX, originY, originZ);
		shape.transform.localRotation = skew;

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
		int degreeShift = Utils.Randomizer.Next(-20, 20);
		Vector3 baseJartletSize = new Vector3(
			Utils.Randomizer.Next((int)(jartBoards[jartboardIndex].gameObject.transform.localScale.x * 0.09), (int)(jartBoards[jartboardIndex].gameObject.transform.localScale.x * 0.5)),
			Utils.Randomizer.Next((int)(jartBoards[jartboardIndex].gameObject.transform.localScale.y * 0.09), (int)(jartBoards[jartboardIndex].gameObject.transform.localScale.y * 0.5)),
			Utils.Randomizer.Next((int)(jartBoards[jartboardIndex].gameObject.transform.localScale.z * 0.09), (int)(jartBoards[jartboardIndex].gameObject.transform.localScale.z * 0.5))
		);
		Vector3 positionShift = new Vector3(
			Utils.Randomizer.Next(-(int)baseJartletSize.x, (int)baseJartletSize.x),
			Utils.Randomizer.Next(-(int)baseJartletSize.y, (int)baseJartletSize.y),
			Utils.Randomizer.Next(-(int)baseJartletSize.z, (int)baseJartletSize.z)
		);
		// first the jartlet's parameters will be either modified or made up,
		// then the jartlet will be created and added to the jartlets list.
		for (int i = 0; i < jartletAmount; i++)
		{
			// chance to repeat the jartlet with a slight shift, without changing width/height/depth.
			// note: we have to have made a jartlet before repeating one.
			if (i > 0 && Utils.Randomizer.Next(0, 100) > 15)
			{
				// get bigger or smaller
				if (Utils.Randomizer.Next(0, 10) > 5)
				{
					jartletWidth += Utils.Randomizer.Next((int)(baseJartletSize.x * -0.25), (int)(baseJartletSize.x * 0.25));
					jartletHeight += Utils.Randomizer.Next((int)(baseJartletSize.y * -0.25), (int)(baseJartletSize.y * 0.25));
					jartletDepth += Utils.Randomizer.Next((int)(baseJartletSize.z * -0.25), (int)(baseJartletSize.z * 0.25));
				}
				// modify position
				if (Utils.Randomizer.Next(0, 10) > 3)
				{
					jartletPositionX += (int)positionShift.x;
				}
				if (Utils.Randomizer.Next(0, 10) > 3)
				{
					jartletPositionY += (int)positionShift.y;
				}
				if (Utils.Randomizer.Next(0, 10) > 3)
				{
					jartletPositionZ += (int)positionShift.z;
				}
				// modify rotation
				if (Utils.Randomizer.Next(0, 10) > 3)
				{
					jartletSkew.x += degreeShift;
				}
				if (Utils.Randomizer.Next(0, 10) > 3)
				{
					jartletSkew.y += degreeShift;
				}
				if (Utils.Randomizer.Next(0, 10) > 3)
				{
					jartletSkew.z += degreeShift;
				}
			}
			// generate new parameters for a unique jartlet
			else
			{
				// generate fresh jartlet parameters
				jartletColor = Utils.GetRandomArrayItem(Colors.PossibleColorPalettes[ColorPaletteIndex]);
				jartletType = Utils.GetRandomArrayItem(Constants.PossiblePrimitiveTypes);
				jartletWidth = Utils.Randomizer.Next((int)(baseJartletSize.x * 0.25), (int)baseJartletSize.x);
				jartletHeight = Utils.Randomizer.Next((int)(baseJartletSize.y * 0.25), (int)baseJartletSize.y);
				jartletDepth = Utils.Randomizer.Next((int)(baseJartletSize.z * 0.25), (int)baseJartletSize.z);
				jartletPositionX = Utils.Randomizer.Next((int)jartBoards[jartboardIndex].transform.position.x - (int)jartBoards[jartboardIndex].transform.localScale.x / 2, (int)jartBoards[jartboardIndex].transform.position.x + (int)jartBoards[jartboardIndex].transform.localScale.x / 2);
				jartletPositionY = Utils.Randomizer.Next((int)jartBoards[jartboardIndex].transform.position.y - (int)jartBoards[jartboardIndex].transform.localScale.y / 2, (int)jartBoards[jartboardIndex].transform.position.y + (int)jartBoards[jartboardIndex].transform.localScale.y / 2);
				jartletPositionZ = Utils.Randomizer.Next((int)jartBoards[jartboardIndex].transform.position.z - (int)jartBoards[jartboardIndex].transform.localScale.z / 2, (int)jartBoards[jartboardIndex].transform.position.z + (int)jartBoards[jartboardIndex].transform.localScale.z / 2);
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
		totalJartletsPerJart = Utils.Randomizer.Next(minJartlets, maxJartlets);
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
		// which shader are we going to use this time?
		worldShaderName = Constants.Possible3dShaders[Utils.Randomizer.Next(0, Constants.Possible3dShaders.Length)];
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

	public static void StartMainMenuMusic()
	{
		clearJart();
		// get a new scale & timings
		possibleFrequencies = buildScaleFrequencies();
		possibleTimings = buildNoteTimings();
		int mainMenuOscillatorCount = Utils.Randomizer.Next(2, 5);
		for(int i = 0; i < mainMenuOscillatorCount; i++)
		{
			// when you add the oscillator, it will start playing
			Oscillator newOsc = new GameObject("Oscillator").AddComponent<Oscillator>();
			// attach oscillators to main menu
			newOsc.transform.parent = GameObject.Find("Menu").GetComponent<Menu>().transform;
			oscList.Add(newOsc);
		}
	}

	public void Start()
	{
		menuComponent = GameObject.Find("Menu").GetComponent<Menu>();
	}

	public void Update()
	{
		// make sure the menu isn't up
		if (!menuComponent.isPaused && menuComponent.gameStarted)
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
					totalJartletsPerJart = Utils.Randomizer.Next(minJartlets, maxJartlets);
					// Add jartlets to the last jartboard created
					createJartlets(totalJartletsPerJart, jartBoards.Count - 1);
				}
			}
		}
	}
}
