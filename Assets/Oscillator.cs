using System.Collections;
using System.Timers;
using UnityEngine;

/// <summary>
/// The class responsible for controlling the music in jart.
/// Huge shout out to https://www.youtube.com/watch?v=GqHFGMy_51c
/// </summary>
public class Oscillator : MonoBehaviour
{
	public double increment = 0f;
	public double phase = 0f;
	public double samplingFreq = 48000f;
	public double frequency = 440.0f;
	public float gain = 0;
	public static float volume = Constants.MusicVolume;
	public float attack;
	public float release;
	private static bool isQuiet = false;
	public int waveIndex = 0;
	public float cutoffFrequencyMod;
	public int scaleTones;
	public float[] possibleFrequencies;
	public int[] scaleIntervals;

	private AudioEchoFilter echoFilter;
	private AudioReverbFilter reverbFilter;
	private AudioLowPassFilter lowPassFilter;

	private int[] buildScaleIntervals()
	{
		scaleTones = Utils.Randomizer.Next(3, 16);
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

	private float[] buildScaleFrequencies()
	{
		scaleIntervals = buildScaleIntervals();
		float[] scaleFrequencies = new float[scaleIntervals.Length];
		for (int i = 0; i < scaleIntervals.Length; i++)
		{
			if (i == 0)
			{
				// set the base tone
				scaleFrequencies[0] = Utils.Randomizer.Next(13000, 52000) * 0.01f;
			}
			else
			{
				// calculate the frequency based on the interval and base note
				// with the small chance to serve up something deliciously fucked up
				scaleFrequencies[i] = scaleFrequencies[0] * Mathf.Pow(1.059463f, scaleIntervals[i]);
			}
		}
		return scaleFrequencies;
	}

	private void createNewNoteProperties()
	{
		// change around the reverb
		reverbFilter.reverbDelay = Utils.Randomizer.Next(0, 250);
		reverbFilter.reverbLevel = Utils.Randomizer.Next(100, 2000);
		cutoffFrequencyMod = Utils.Randomizer.Next(200, 1000);
		// change around the echo
		echoFilter.delay = Utils.Randomizer.Next(0, 1000);
		echoFilter.decayRatio = Utils.Randomizer.Next(0, 50) * 0.01f;
		frequency = Utils.GetRandomArrayItem(possibleFrequencies);
		// now the new envelope
		attack = Utils.Randomizer.Next(1, 50) * 0.0006f;
		release = Utils.Randomizer.Next(1, 50) * 0.0006f;
		// reset gain
		gain = 0;
	}

	/// <summary>
	/// Meant to be called by other classes to trigger a new song.
	/// </summary>
	public void StartNewSong()
	{
		// get a new scale
		possibleFrequencies = buildScaleFrequencies();
		// we currently have 3 waves, so pick one
		// note: random.Next is inclusive lower bound, exclusive high bound
		waveIndex = Utils.Randomizer.Next(0, 4);

		// now set up the timer for the next note
		StartCoroutine(waitAndStartNewNote());
	}

	private IEnumerator waitAndStartNewNote()
	{
		yield return new WaitForSeconds(Utils.Randomizer.Next(1000, 10000) / 1000);
		createNewNoteProperties();

		// now set up the envelope
		StartCoroutine(startEnvelope());
	}

	private IEnumerator stopEnvelope()
	{
		yield return new WaitForSeconds(0.1f);
		gain -= release;
		if(gain <= 0)
		{
			gain = 0;
			// now set up the timer for the next note
			StartCoroutine(waitAndStartNewNote());
		}
		else
		{
			StartCoroutine(stopEnvelope());
		}
	}

	private IEnumerator startEnvelope()
	{
		yield return new WaitForSeconds(0.1f);
		gain += attack;
		if (gain >= volume)
		{
			gain = volume;
			StartCoroutine(stopEnvelope());
		}
		else
		{
			StartCoroutine(startEnvelope());
		}
	}

	void Start()
	{
		echoFilter = FindObjectOfType<AudioEchoFilter>();
		reverbFilter = FindObjectOfType<AudioReverbFilter>();
		lowPassFilter = FindObjectOfType<AudioLowPassFilter>();
		StartNewSong();
	}

	public static void ToggleSongQuiet()
	{
		if(!isQuiet)
		{
			volume = 0.05f;
			isQuiet = true;
		}
		else
		{
			volume = Constants.MusicVolume;
			isQuiet = false;
		}
	}

	private void playTriangleWave(float[] data, int channels)
	{
		// increment the frequency so we know where to move on the x-axis of the waveform
		increment = frequency * 2.0 * Mathf.PI / samplingFreq;
		// iterate through the data to set the position of the phase (or y-axis) of the waveform
		for (int i = 0; i < data.Length; i += channels)
		{
			phase += increment;
			data[i] = (float)(gain * (double)Mathf.PingPong((float)phase, 1.0f));
			// play sound in both speakers if they exist
			if (channels == 2)
			{
				data[i + 1] = data[i];
			}

			if (phase > (2.0 * Mathf.PI))
			{
				phase = 0f;
			}
		}
	}

	private void playEvanWave(float[] data, int channels)
	{
		// increment the frequency so we know where to move on the x-axis of the waveform
		increment = frequency * 2.0 * Mathf.PI / samplingFreq;
		// iterate through the data to set the position of the phase (or y-axis) of the waveform
		for (int i = 0; i < data.Length; i += channels)
		{
			phase += increment;
			data[i] = phase % 3 == 0 ? (float)(gain * (double)Mathf.Atan2((float)phase, 0.5f)) : (float)-(gain * (double)Mathf.Atan((float)phase));
			cutoffFrequencyMod = (float)phase * 100 > 0 ? (float)phase * 100 : 1;
			// play sound in both speakers if they exist
			if (channels == 2)
			{
				data[i + 1] = data[i];
			}

			if (phase > (2.0 * Mathf.PI))
			{
				phase = 0f;
			}
		}
	}

	private void playSineWave(float[] data, int channels)
	{
		// increment the frequency so we know where to move on the x-axis of the waveform
		increment = frequency * 2.0 * Mathf.PI / samplingFreq;
		// iterate through the data to set the position of the phase (or y-axis) of the waveform
		for (int i = 0; i < data.Length; i += channels)
		{
			phase += increment;
			data[i] = Mathf.Sin((float)phase);
			// play sound in both speakers if they exist
			if (channels == 2)
			{
				data[i + 1] = data[i];
			}

			if (phase > (2.0 * Mathf.PI))
			{
				phase = 0f;
			}
		}
	}

	private void playWhiteNoiseWave(float[] data, int channels)
	{
		cutoffFrequencyMod = 20;
		// increment the frequency so we know where to move on the x-axis of the waveform
		increment = Utils.Randomizer.Next(0, 880) * Mathf.PI / samplingFreq;
		// iterate through the data to set the position of the phase (or y-axis) of the waveform
		for (int i = 0; i < data.Length; i += channels)
		{
			phase += increment;
			data[i] = (float)phase * 0.1f;
			// play sound in both speakers if they exist
			if (channels == 2)
			{
				data[i + 1] = data[i];
			}

			if (phase > (2.0 * Mathf.PI))
			{
				phase = 0f;
			}
		}
	}

	private void playSquareWave(float[] data, int channels)
	{
		// increment the frequency so we know where to move on the x-axis of the waveform
		increment = frequency * 2.0 * Mathf.PI / samplingFreq;
		// iterate through the data to set the position of the phase (or y-axis) of the waveform
		for (int i = 0; i < data.Length; i += channels)
		{
			phase += increment;
			if(gain * Mathf.Sin((float)phase) >= 0 * gain)
			{
				data[i] = gain * Mathf.Sin((float)phase);
			}
			else
			{
				data[i] = -gain * 0.6f;
			}
			// play sound in both speakers if they exist
			if (channels == 2)
			{
				data[i + 1] = data[i];
			}

			if (phase > (2.0 * Mathf.PI))
			{
				phase = 0f;
			}
		}
	}

	private void OnAudioFilterRead(float[] data, int channels)
	{
		if(waveIndex == 0)
		{
			playEvanWave(data, channels);
		}
		else if(waveIndex == 1)
		{
			playTriangleWave(data, channels);
		}
		else if(waveIndex == 2)
		{
			playWhiteNoiseWave(data, channels);
		}
		else
		{
			playSquareWave(data, channels);
		}
	}
}
