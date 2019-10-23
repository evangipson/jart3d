using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
	private List<Oscillator> oscList = new List<Oscillator>();
	public int scaleTones;
	public static float[] possibleFrequencies;
	public static int[] scaleIntervals;
	public static float[] possibleTimings;
	private static bool isQuiet = false;
	public int numberOfOscillators;

	public static void ToggleSongQuiet()
	{
		if (!isQuiet)
		{
			Constants.MusicVolume = 0.5f;
			isQuiet = true;
		}
		else
		{
			Constants.MusicVolume = 1.0f;
			isQuiet = false;
		}
	}

	private int[] buildScaleIntervals()
	{
		scaleTones = Utils.Randomizer.Next(5, 16);
		int[] localScaleIntervals = new int[scaleTones];
		for (int i = 0; i < scaleTones; i++)
		{
			if (i == 0)
			{
				localScaleIntervals[0] = 1;
			}
			else
			{
				// how many half steps between each interval?
				localScaleIntervals[i] = localScaleIntervals[i - 1] + Utils.Randomizer.Next(1, 4);
			}
		}
		return localScaleIntervals;
	}

	public float[] buildScaleFrequencies()
	{
		scaleIntervals = buildScaleIntervals();
		float[] scaleFrequencies = new float[scaleIntervals.Length];
		for (int i = 0; i < scaleIntervals.Length; i++)
		{
			if (i == 0)
			{
				// set the base tone
				scaleFrequencies[0] = Utils.Randomizer.Next(8000, 20000) * 0.01f;
			}
			else
			{
				// calculate the frequency based on the interval and base note
				scaleFrequencies[i] = scaleFrequencies[0] * Mathf.Pow(1.059463f, scaleIntervals[i]);
			}
		}
		return scaleFrequencies;
	}

	public float[] buildNoteTimings()
	{
		float[] localNoteTimes = new float[6];
		for (int i = 0; i < localNoteTimes.Length - 1; i++)
		{
			// quarter note at 60bpm is 1 second
			if (i == 0)
			{
				localNoteTimes[i] = Utils.Randomizer.Next(250, 500); // 1st note timing is a sixteenth note, or 1/4th of 1 second at 60bpm
			}
			else
			{
				localNoteTimes[i] = localNoteTimes[i - 1] * 2;
			}
		}
		return localNoteTimes;
	}

	public void DestroyMusicPlayer()
	{
		Destroy(gameObject);
		Destroy(this);
	}

	// Start is called before the first frame update
	void Start()
	{
		// clear out old oscillators
		for (int i = 0; i < oscList.Count; i++)
		{
			oscList[i].DestroyOscillator();
		}
		oscList.Clear();
		// get a new scale & timings
		possibleFrequencies = buildScaleFrequencies();
		possibleTimings = buildNoteTimings();
		// generate new oscillators
		numberOfOscillators = Utils.Randomizer.Next(2, 6);
		for (int i = 0; i < numberOfOscillators; i++)
		{
			// when you add the oscillator, it will start playing
			Oscillator newOsc = new GameObject("Oscillator").AddComponent<Oscillator>();
			newOsc.transform.parent = transform; // force this new gameobject to be a child
			oscList.Add(newOsc);
		}
	}

    // Update is called once per frame
    void Update()
    {
        
    }
}
