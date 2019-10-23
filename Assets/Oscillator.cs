using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

/// <summary>
/// The class responsible for controlling the music in jart.
/// Huge shout out to https://www.youtube.com/watch?v=GqHFGMy_51c.
/// Expects a monobehavior upon creation to access unity things.
/// </summary>
public class Oscillator : MonoBehaviour
{
	public double increment = 0f;
	public double phase = 0f;
	public double samplingFreq = 48000f;
	public double frequency;
	public float attack;
	public float sustain;
	public float release;
	public int waveIndex = 0;
	public float cutoffFrequencyMod;
	private float maxVolume = 1f;

	private AudioEchoFilter echoFilter;
	private AudioReverbFilter reverbFilter;
	private AudioLowPassFilter lowPassFilter;
	private AudioSource audioSource;

	// define delegate to fill up method list with
	private delegate void PlayWaveformMethod(float[] data, int channels);
	// here's the method list for all waveforms
	private List<PlayWaveformMethod> waveFormMethods = new List<PlayWaveformMethod>();

	private void createNewNoteProperties()
	{
		float noteTime = Utils.GetRandomArrayItem(Jart.possibleTimings);
		// change around the reverb
		reverbFilter.reverbDelay = Utils.Randomizer.Next(20, 70) * 0.01f;
		reverbFilter.reverbLevel = Utils.Randomizer.Next(1000, 2000);
		cutoffFrequencyMod = Utils.Randomizer.Next(20, 1000);
		lowPassFilter.cutoffFrequency = cutoffFrequencyMod;
		lowPassFilter.lowpassResonanceQ = Utils.Randomizer.Next(1, 40) * 0.1f;
		if (waveIndex == 3 || waveIndex == 4)
		{
			lowPassFilter.lowpassResonanceQ = 1;
		}
		// change around the echo
		echoFilter.delay = Utils.Randomizer.Next(1, 4) * noteTime;
		echoFilter.decayRatio = Utils.Randomizer.Next(0, 10) * 0.1f;
		frequency = Utils.GetRandomArrayItem(Jart.possibleFrequencies);
		// now the new envelope
		attack = Utils.Randomizer.Next(1, 50) * 0.01f * maxVolume; // in ms
		sustain = noteTime / 1000; // in ms
		release = Utils.Randomizer.Next(1, 50) * 0.01f * maxVolume; // in ms
	}

	private void adjustWaveVolume()
	{
		if (waveIndex == 1)
		{
			maxVolume = 0.00006f; // evan waves need to be quiet too
		}
		else if (waveIndex == 2)
		{
			maxVolume = 0.0005f; // sine needs to be a lil less quiiiiet
		}
		else if (waveIndex == 3)
		{
			maxVolume = 0.00002f; // white noise need to be waaay quiiiiet
		}
		else if (waveIndex == 4)
		{
			maxVolume = 0.0005f; // pink noise need to be quiiiiet
		}
		else
		{
			maxVolume = 0.005f; // everything else is normal volume
		}
	}

	public void DestroyOscillator()
	{
		Destroy(echoFilter);
		Destroy(reverbFilter);
		Destroy(lowPassFilter);
		Destroy(audioSource);
		Destroy(gameObject);
		Destroy(this);
	}

	/// <summary>
	/// Meant to be called by other classes to trigger a new song.
	/// </summary>
	public void StartNewSong()
	{
		// add audio source first because filters depend on it
		audioSource = gameObject.AddComponent<AudioSource>();
		audioSource.volume = 0;
		audioSource.playOnAwake = false;
		// make sure the audio source is 3d
		audioSource.maxDistance = Constants.JartCubeSize * 0.5f;
		audioSource.rolloffMode = AudioRolloffMode.Linear;
		audioSource.spread = 360;
		audioSource.spatialize = true;
		audioSource.spatialBlend = 1.0f;
		// adjust the audio source volume dependant on which wave we have
		adjustWaveVolume();
		// when you add the oscillator, it will start playing
		echoFilter = gameObject.AddComponent<AudioEchoFilter>();
		reverbFilter = gameObject.AddComponent<AudioReverbFilter>();
		lowPassFilter = gameObject.AddComponent<AudioLowPassFilter>();
		// Put all of our wave form methods in an accessible data structure
		waveFormMethods.Add(playTriangleWave);
		waveFormMethods.Add(playEvanWave);
		waveFormMethods.Add(playSineWave);
		waveFormMethods.Add(playPinkNoiseWave);
		waveFormMethods.Add(playWhiteNoiseWave);
		waveFormMethods.Add(playSquareWave);
		// we currently have multiple possible waves, so pick one
		// note: random.Next is inclusive lower bound, exclusive high bound
		waveIndex = Utils.Randomizer.Next(0, waveFormMethods.Count);
		// now set up the timer for the next note
		StartCoroutine(waitAndStartNewNote());
	}

	// A(D)SR methods
	private IEnumerator waitAndStartNewNote()
	{
		yield return new WaitForSeconds(Utils.GetRandomArrayItem(Jart.possibleTimings) / 1000);
		createNewNoteProperties();

		// now set up the envelope
		audioSource.volume = 0;
		StartCoroutine(startEnvelope());
	}

	private IEnumerator stopEnvelope()
	{
		audioSource.volume -= release;
		if(audioSource.volume <= 0)
		{
			audioSource.volume = 0;
			// now set up the timer for the next note
			StartCoroutine(waitAndStartNewNote());
		}
		else
		{
			yield return new WaitForSeconds(0.01f);
			StartCoroutine(stopEnvelope());
		}
	}

	private IEnumerator startEnvelope()
	{
		audioSource.volume += attack;
		if (audioSource.volume >= maxVolume)
		{
			audioSource.volume = maxVolume;
			// apply sustain by waiting to decrease audiosource volume
			yield return new WaitForSeconds(Utils.Randomizer.Next(1, 5) * sustain);
			StartCoroutine(stopEnvelope());
		}
		else
		{
			yield return new WaitForSeconds(0.01f);
			StartCoroutine(startEnvelope());
		}
	}

	// Wave Methods
	// note: when adding one, don't forget to put it in waveFormMethods,
	// inside the Start() function.
	private void playTriangleWave(float[] data, int channels)
	{
		// increment the frequency so we know where to move on the x-axis of the waveform
		increment = frequency * 2.0 * Mathf.PI / samplingFreq;
		// iterate through the data to set the position of the phase (or y-axis) of the waveform
		for (int i = 0; i < data.Length; i += channels)
		{
			phase += increment;
			data[i] = (float)(Constants.MusicVolume * (double)Mathf.PingPong((float)phase, 1.0f));
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
			data[i] = phase % 3 == 0 ? (float)(Constants.MusicVolume * (double)Mathf.Cos((float)phase)) : (float)-(Constants.MusicVolume * (double)Mathf.Atan((float)phase));
			cutoffFrequencyMod = Utils.Randomizer.Next(150, 1000);
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

	private void playPinkNoiseWave(float[] data, int channels)
	{
		frequency = Utils.Randomizer.Next(100, 500);
		cutoffFrequencyMod = 10;
		for (int i = 0; i < data.Length; i += channels)
		{
			data[i] = (float)((Utils.Randomizer.Next(1, 1000) * 0.01f) * 2.0 - 1.0);
			// play sound in both speakers if they exist
			if (channels == 2)
			{
				data[i + 1] = data[i];
			}
		}
	}

	private void playWhiteNoiseWave(float[] data, int channels)
	{
		frequency = Utils.Randomizer.Next(100, 500);
		for (int i = 0; i < data.Length; i += channels)
		{
			data[i] = (float)((Utils.Randomizer.Next(1, 1000) * 0.01f) * 2.0 - 1.0);
			// play sound in both speakers if they exist
			if (channels == 2)
			{
				data[i + 1] = data[i];
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
			if (Constants.MusicVolume * Mathf.Sin((float)phase) >= 0 * Constants.MusicVolume)
			{
				data[i] = Constants.MusicVolume * Mathf.Sin((float)phase);
			}
			else
			{
				data[i] = -Constants.MusicVolume * 0.6f;
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

	// unity methods derived from MonoBehaviour
	void Start()
	{
		// call the same function other classes will call to start the oscillator!
		StartNewSong();
	}

	private void Update()
	{
		lowPassFilter.cutoffFrequency = cutoffFrequencyMod;
		if (audioSource.volume > maxVolume)
		{
			audioSource.volume = maxVolume;
		}
	}

	private void OnAudioFilterRead(float[] data, int channels)
	{
		waveFormMethods[waveIndex](data, channels);
	}
}
