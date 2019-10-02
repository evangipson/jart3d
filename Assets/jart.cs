using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class jart : MonoBehaviour
{
	// I want to use System Random, not UnityEngine Random
	private System.Random randomizer = new System.Random();
	private Sprite jartboard;
	private Color[] possibleColors = {
		Color.red,
		Color.blue,
		Color.green,
		Color.cyan,
		Color.magenta,
		Color.yellow
	};
	private List<Sprite> jartlets = new List<Sprite>();
	private Sprite sprite;
	private int jartletAmount = 100;
	private int jartboardHeight = 10;
	private int jartboardWidth = 15;

	/**
	 * Creates a sprite. Intended to be used to create Jartlets
	 * and the Jartboard, which is returned by default (using
	 * default parameters). */
	private Sprite createSprite(Color color, string shader, int width = 1, int height = 1, float originX = 0, float originY = 0, int zIndex = -1, bool skew = false)
	{
		GameObject go = new GameObject();
		SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
		sprite = Sprite.Create(new Texture2D(width, height), new Rect(0, 0, width, height), new Vector2(0, 0));

		// set a color
		sr.material.color = color;
		// make sure the color is pure by using the mask shader
		sr.material.shader = Shader.Find(shader);

		// set the z-index
		sr.sortingOrder = zIndex;

		// make sure masking actually works
		sr.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;

		// skew that renderer all up if we want it skewed
		if (skew)
		{
			sr.transform.rotation = new Quaternion(randomizer.Next(-20, 20), randomizer.Next(-20, 20), randomizer.Next(-20, 20), 0);
		}

		// tell the sprite renderer about the sprite
		sr.sprite = sprite;

		// set the position of the object
		go.transform.position = new Vector3(originX / 10, originY / 10, 0);
		go.transform.localScale = new Vector3(width, height, 0);

		return sprite;
	}

	/// <summary>
	/// Will create and place and skew a jarlet.
	/// TODO: implement a small chance to copy previous jartlet.
	/// </summary>
	private void createJartlets()
	{
		for (int i = 0; i < jartletAmount; i++)
		{
			jartlets.Add(createSprite(
				possibleColors[randomizer.Next(0, possibleColors.Length - 1)],
				randomizer.Next(0, 4) >= 1 ? "Sprites/Mask" : "Sprites/Default",
				randomizer.Next(1, jartboardWidth),
				randomizer.Next(1, jartboardHeight),
				randomizer.Next(0, jartboardWidth),
				randomizer.Next(0, jartboardHeight),
				randomizer.Next(0, jartletAmount),
				true
			));
		}
	}

	private void createJartboard()
	{
		jartboard = createSprite(possibleColors[randomizer.Next(0, possibleColors.Length - 1)], "Sprites/Default", jartboardWidth, jartboardHeight);
	}

	public void Start()
	{
		jartletAmount = randomizer.Next(10, 70);
		createJartboard();
		createJartlets();
		// render out newly created jartlets and jartboard
	}
}
