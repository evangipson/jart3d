using System.Collections;
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
	public double frequency = 440.0f;
	public float gain = 0;
	public float attack;
	public float sustain;
	public float release;
	public int waveIndex = 0;
	public float cutoffFrequencyMod;

	private AudioEchoFilter echoFilter;
	private AudioReverbFilter reverbFilter;
	private AudioLowPassFilter lowPassFilter;
	private AudioSource audioSource;

	private void createNewNoteProperties()
	{
		float noteTime = Utils.GetRandomArrayItem(MusicPlayer.possibleTimings);
		// change around the reverb
		reverbFilter.reverbDelay = Utils.Randomizer.Next(20, 70) * 0.01f;
		reverbFilter.reverbLevel = Utils.Randomizer.Next(1000, 2000);
		cutoffFrequencyMod = Utils.Randomizer.Next(200, 1000);
		// change around the echo
		echoFilter.delay = Utils.Randomizer.Next(1, 4) * noteTime;
		echoFilter.decayRatio = Utils.Randomizer.Next(0, 10) * 0.1f;
		frequency = Utils.GetRandomArrayItem(MusicPlayer.possibleFrequencies);
		// now the new envelope
		attack = Utils.Randomizer.Next(1, 200) * 0.02f;
		sustain = noteTime; // in ms
		release = Utils.Randomizer.Next(1, 200) * 0.02f;
		// reset gain
		gain = 0;
	}

	/// <summary>
	/// Meant to be called by other classes to trigger a new song.
	/// </summary>
	public void StartNewSong()
	{
		// we currently have 3 waves, so pick one
		// note: random.Next is inclusive lower bound, exclusive high bound
		waveIndex = Utils.Randomizer.Next(0, 5);
		if(waveIndex == 3 || waveIndex == 4)
		{
			audioSource.volume = 0.006f; // noise need to be quiiiiet
		}
		else if(waveIndex == 2)
		{
			audioSource.volume = 0.008f; // sine needs to be a lil less quiiiiet
		}
		else
		{
			audioSource.volume = 0.07f; // everything else is normal volume
		}
		// now set up the timer for the next note
		StartCoroutine(waitAndStartNewNote());
	}

	private IEnumerator waitAndStartNewNote()
	{
		yield return new WaitForSeconds(Utils.Randomizer.Next(50, 5000) / 1000);
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
		if (gain >= MusicPlayer.volume)
		{
			gain = MusicPlayer.volume;
			StartCoroutine(stopEnvelope());
		}
		else
		{
			// apply sustain by waiting to decrease note gain
			yield return new WaitForSeconds(sustain / 1000);
			StartCoroutine(startEnvelope());
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

	void Start()
	{
		// add audio source first because filters depend on it
		audioSource = gameObject.AddComponent<AudioSource>();
		audioSource.volume = Constants.MusicVolume;
		// when you add the oscillator, it will start playing
		echoFilter = gameObject.AddComponent<AudioEchoFilter>();
		reverbFilter = gameObject.AddComponent<AudioReverbFilter>();
		lowPassFilter = gameObject.AddComponent<AudioLowPassFilter>();
		// now that we have all the filters and sources, start playing notes!
		StartNewSong();
	}

	private void Update()
	{
		// don't let the synth get louder than the volume
		if(gain > MusicPlayer.volume)
		{
			gain = MusicPlayer.volume;
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
			data[i] = phase % 3 == 0 ? (float)(gain * (double)Mathf.Cos((float)phase)) : (float)-(gain * (double)Mathf.Atan((float)phase));
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

	private void playPinkNoiseWave(float[] data, int channels)
	{
		cutoffFrequencyMod = 10;
		int offset = 0;
		for (int i = 0; i < data.Length; i++)
		{
			data[i] = (float)((Utils.Randomizer.Next(1, 100) * 0.01f) * 2.0 - 1.0 + offset);
		}
	}

	private void playWhiteNoiseWave(float[] data, int channels)
	{
		int offset = 0;
		for (int i = 0; i < data.Length; i++)
		{
			data[i] = (float)((Utils.Randomizer.Next(1, 100) * 0.01f) * 2.0 - 1.0 + offset);
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
			playTriangleWave(data, channels);
		}
		else if(waveIndex == 1)
		{
			playTriangleWave(data, channels);
		}
		else if(waveIndex == 2)
		{
			playSineWave(data, channels);
		}
		else if (waveIndex == 3)
		{
			if(Utils.Randomizer.Next(0, 10) > 5)
			{
				playWhiteNoiseWave(data, channels);
			}
			else
			{
				playPinkNoiseWave(data, channels);
			}
		}
		else if (waveIndex == 4)
		{
			playSineWave(data, channels);
		}
		else
		{
			playSquareWave(data, channels);
		}
	}
}
