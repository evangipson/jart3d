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
	public float release;
	public int waveIndex = 0;
	public float cutoffFrequencyMod;

	private AudioEchoFilter echoFilter;
	private AudioReverbFilter reverbFilter;
	private AudioLowPassFilter lowPassFilter;
	private AudioSource audioSource;

	private void createNewNoteProperties()
	{
		// change around the reverb
		reverbFilter.reverbDelay = Utils.Randomizer.Next(20, 70) * 0.01f;
		reverbFilter.reverbLevel = Utils.Randomizer.Next(1000, 2000);
		cutoffFrequencyMod = Utils.Randomizer.Next(200, 1000);
		// change around the echo
		echoFilter.delay = Utils.Randomizer.Next(0, 1000);
		echoFilter.decayRatio = Utils.Randomizer.Next(0, 10) * 0.1f;
		frequency = Utils.GetRandomArrayItem(MusicPlayer.possibleFrequencies);
		// now the new envelope
		attack = Utils.Randomizer.Next(1, 20) * 0.005f;
		release = Utils.Randomizer.Next(1, 20) * 0.005f;
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
		waveIndex = Utils.Randomizer.Next(0, 4);

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
			playEvanWave(data, channels);
		}
		else
		{
			playSquareWave(data, channels);
		}
	}
}
