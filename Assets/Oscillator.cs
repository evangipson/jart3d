using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wave
{
	// define delegate to fill up method list with
	public delegate void PlayWaveformMethod(float[] data, int channels);

	public PlayWaveformMethod WaveMethod;
	public float MaxVolume;
	
	public Wave(PlayWaveformMethod waveMethod, float maxVolume)
	{
		WaveMethod = waveMethod;
		MaxVolume = maxVolume;
	}
}

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
	public float rest;
	public int waveIndex = 0;
	public float cutoffFrequencyMod;
	public float noteTime;

	private AudioEchoFilter echoFilter;
	private AudioReverbFilter reverbFilter;
	private AudioLowPassFilter lowPassFilter;
	private AudioSource audioSource;
	// here's the method list for all waveform methods and related maximum volumes
	private List<Wave> waveForms;

	private void createNewNoteProperties()
	{
		// chance for a note to have the same timing as before
		if (Utils.Randomizer.Next(0, 100) > 70)
		{
			noteTime = Utils.GetRandomArrayItem(Jart.possibleTimings);
		}
		frequency = Utils.GetRandomArrayItem(Jart.possibleFrequencies);
		// now the new envelope
		attack = Utils.Randomizer.Next(1, 50) * 0.01f * waveForms[waveIndex].MaxVolume;
		release = attack + Utils.Randomizer.Next(1, 50) * 0.01f * waveForms[waveIndex].MaxVolume;
		sustain = noteTime / 1000; // in ms
		// if we have a noise wave, make the notes longer in general
		if(waveForms[waveIndex].WaveMethod == playPinkNoiseWave || waveForms[waveIndex].WaveMethod == playWhiteNoiseWave)
		{
			reverbFilter.reverbPreset = AudioReverbPreset.Auditorium;
			echoFilter.enabled = false;
			lowPassFilter.lowpassResonanceQ = 1;
			sustain *= Utils.Randomizer.Next(4, 8);
			attack /= Utils.Randomizer.Next(2, 5);
			release /= Utils.Randomizer.Next(2, 5);
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

	// A(D)SR methods
	private IEnumerator waitAndStartNewNote()
	{
		createNewNoteProperties();
		// small chance to modify our rest value
		if (Utils.Randomizer.Next(0, 100) > 80)
		{
			rest = (Utils.GetRandomArrayItem(Jart.possibleTimings) / 1000) * Utils.Randomizer.Next(0, 4);
		}
		// simulating a "rest" - wait for a sec before starting a new note
		yield return new WaitForSeconds(rest);

		// now set up the envelope
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
		if (audioSource.volume >= waveForms[waveIndex].MaxVolume)
		{
			audioSource.volume = waveForms[waveIndex].MaxVolume;
			// apply sustain by waiting to decrease audiosource volume
			yield return new WaitForSeconds(sustain);
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
			data[i] = (float)(waveForms[waveIndex].MaxVolume * (double)Mathf.PingPong((float)phase, 1.0f));
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
			data[i] = phase % 3 == 0 ? (float)(waveForms[waveIndex].MaxVolume * (double)Mathf.Cos((float)phase)) : (float)-(waveForms[waveIndex].MaxVolume * (double)Mathf.Atan((float)phase));
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

	private void playTriangleSineWave(float[] data, int channels)
	{
		// increment the frequency so we know where to move on the x-axis of the waveform
		increment = frequency * 2.0 * Mathf.PI / samplingFreq;
		// iterate through the data to set the position of the phase (or y-axis) of the waveform
		for (int i = 0; i < data.Length; i += channels)
		{
			phase += increment;
			data[i] = Mathf.Sin((float)phase) * (float)(waveForms[waveIndex].MaxVolume * (double)Mathf.PingPong((float)phase, 1.0f));
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
		cutoffFrequencyMod = 70;
		for (int i = 0; i < data.Length; i += channels)
		{
			data[i] = (float)(Utils.Randomizer.Next(100, 1000) * 0.01f) * 2.0f - 1.0f;
			// play sound in both speakers if they exist
			if (channels == 2)
			{
				data[i + 1] = data[i];
			}
		}
	}

	private void playWhiteNoiseWave(float[] data, int channels)
	{
		for (int i = 0; i < data.Length; i += channels)
		{
			data[i] = (float)(Utils.Randomizer.Next(100, 1000) * 0.01f) * 2.0f - 1.0f;
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
			if (waveForms[waveIndex].MaxVolume * Mathf.Sin((float)phase) >= 0 * waveForms[waveIndex].MaxVolume)
			{
				data[i] = waveForms[waveIndex].MaxVolume * Mathf.Sin((float)phase);
			}
			else
			{
				data[i] = -waveForms[waveIndex].MaxVolume * 0.6f;
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
		// set defaults
		frequency = Utils.GetRandomArrayItem(Jart.possibleFrequencies);
		rest = (Utils.GetRandomArrayItem(Jart.possibleTimings) / 1000) * Utils.Randomizer.Next(0, 4);
		noteTime = Utils.GetRandomArrayItem(Jart.possibleTimings);
		// now set up the waves that we could potentially hear
		waveForms = new List<Wave>();
		// Put all of our wave form methods in an accessible data structure,
		// passed with a maxVolume float
		waveForms.Add(new Wave(playTriangleWave, 0.15f));
		waveForms.Add(new Wave(playSineWave, 0.09f));
		waveForms.Add(new Wave(playTriangleSineWave, 0.15f));
		waveForms.Add(new Wave(playSquareWave, 0.15f));
		waveForms.Add(new Wave(playEvanWave, 0.3f));
		waveForms.Add(new Wave(playPinkNoiseWave, 0.05f));
		waveForms.Add(new Wave(playWhiteNoiseWave, 0.008f));
		// we currently have multiple possible waves, so pick one
		// note: random.Next is inclusive lower bound, exclusive high bound
		waveIndex = Utils.Randomizer.Next(0, waveForms.Count);
		// add audio source first because filters depend on it
		audioSource = gameObject.AddComponent<AudioSource>();
		audioSource.volume = 0;
		audioSource.playOnAwake = false;
		// make sure the audio source is 3d
		audioSource.maxDistance = Constants.JartCubeSize;
		audioSource.rolloffMode = AudioRolloffMode.Linear;
		audioSource.spread = 360;
		audioSource.spatialize = true;
		audioSource.spatialBlend = 1.0f;
		// when you add the oscillator, it will start playing
		echoFilter = gameObject.AddComponent<AudioEchoFilter>();
		reverbFilter = gameObject.AddComponent<AudioReverbFilter>();
		lowPassFilter = gameObject.AddComponent<AudioLowPassFilter>();
		// change around the echo per OSCILLATOR, not per note
		echoFilter.delay = (Utils.Randomizer.Next(25, 100) * 0.01f) * noteTime;
		echoFilter.decayRatio = Utils.Randomizer.Next(5, 9) * 0.1f;
		// ensure we can mess around with the reverb filter,
		// otherwise, all values can't be modified: https://docs.unity3d.com/Manual/class-AudioReverbFilter.html
		reverbFilter.reverbPreset = Utils.Randomizer.Next(0, 100) > 50 ? AudioReverbPreset.Arena : AudioReverbPreset.Mountains;
		// change around the reverb per OSCILLATOR, not per note
		//reverbFilter.diffusion = Utils.Randomizer.Next(20, 90); // echo "density", in percent
		//reverbFilter.density = Utils.Randomizer.Next(20, 90); // modal "density", in percent
		//reverbFilter.dryLevel = -1000; // need to figure out what this actually does
		//reverbFilter.room = -1000; // need to figure out what this actually does
		//reverbFilter.reverbDelay = Utils.Randomizer.Next(10, 30) * 0.01f;
		//reverbFilter.reverbLevel = Utils.Randomizer.Next(0, 2000);
		//reverbFilter.reflectionsLevel = Utils.Randomizer.Next(0, 1000);
		// change around lowpass filter per OSCILLATOR, not per note
		cutoffFrequencyMod = Utils.Randomizer.Next(100, 1500);
		//lowPassFilter.lowpassResonanceQ = Utils.Randomizer.Next(1, 20) * 0.1f;
		// now set up the timer for the next note
		StartCoroutine(waitAndStartNewNote());
	}

	void Update()
	{
		lowPassFilter.cutoffFrequency = cutoffFrequencyMod;
	}

	void OnAudioFilterRead(float[] data, int channels)
	{
		waveForms[waveIndex].WaveMethod(data, channels);
	}
}
