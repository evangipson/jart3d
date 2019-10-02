using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class jart : MonoBehaviour
{
	public int jartletAmount = 10;
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
	private int jartboardHeight = 10;
	private int jartboardWidth = 10;

	/**
	 * Creates a sprite. Intended to be used to create Jartlets
	 * and the Jartboard, which is returned by default (using
	 * default parameters). */
	private Sprite createSprite(Color color, int width = 1, int height = 1, float originX = 0, float originY = 0)
	{
		GameObject go = new GameObject();
		SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
		sprite = Sprite.Create(new Texture2D(width, height), new Rect(0, 0, width, height), new Vector2(0, 0), 100.0f);
		
		// set a color
		sr.material.color = color;
		sr.sprite = sprite;

		// set the position of the object
		go.transform.position = new Vector3(originX / 10, originY / 10, 0);
		go.transform.localScale = new Vector3(width - originX, height - originY, 0);

		return sprite;
	}

	private void createJartlets()
	{
		for (int i = 0; i < jartletAmount; i++)
		{
			jartlets.Add(createSprite(
				possibleColors[randomizer.Next(0, possibleColors.Length - 1)],
				randomizer.Next(1, jartboardWidth),
				randomizer.Next(1, jartboardHeight),
				randomizer.Next(0, jartboardWidth),
				randomizer.Next(0, jartboardHeight)
			));
		}
	}

	private void createJartboard()
	{
		jartboard = createSprite(Color.white, jartboardWidth, jartboardHeight);
	}

	public void Start()
	{
		createJartboard();
		createJartlets();
		// render out newly created jartlets and jartboard
	}
}
