using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// Singleton class that handles all 2D audio for menus and gameplay. Designed
/// to be editor friendly so Audio Engineers can work easily within Unity.
/// </summary>
public class AudioManager : Singleton<AudioManager>
{
	[Tooltip("Audio mixer for the player to control with settings.")]
	public AudioMixer MainAudioMixer;

	[Header("Audio Sources")]
	[Tooltip("Audio source for the ambient sounds.")]
	public AudioSource AmbientAudioSource;

	[Tooltip("Audio source responsible for playing countdown sounds.")]
	public AudioSource CountdownAudioSource;

	[Tooltip("Audio source responsible for playing music.")]
	public AudioSource MusicAudioSource;

	[Tooltip("Array of audio sources to play all start signal sounds" +
		"simultaneously. This must be the same length as StartSignalSounds.")]
	public AudioSource[] StartSignalSoundSources;

	[Header("Audio Clips")]
	[Tooltip("Ambient noise played during race sequence.")]
	public AudioClip AmbientClip;

	[Tooltip("Sound to be played when numbers are displayed during race" +
		"countdown.")]
	public AudioClip CountdownClip;

	[Tooltip("Sound to play when player loses.")]
	public AudioClip LossClip;

	[Tooltip("Collection of songs for the game's soundtrack.")]
	public AudioClip[] SoundTrack;

	[Tooltip("Collection of songs to be played simulatneous during the race" +
		"countdown on the 'GO!' signal.")]
	public AudioClip[] StartSignalClips;

	[Tooltip("Sound to play when player wins.")]
	public AudioClip WinClip;

	[Header("Misc.")]
	[Tooltip("Time for music to fade out during scene changes.")]
	public float MusicFadeTime = 2.0f;

	[Tooltip("Music Tracker text object")]
	public Text MusicTracker;

	[Tooltip("Strings to update the music tracker with.")]
	public string[] SoundTrackText;

	private void Start()
	{
		if (StartSignalClips.Length != StartSignalSoundSources.Length)
		{
			Debug.LogError($"[AudioManager.cs] StartSignalSound" +
				$"length ({StartSignalClips.Length}) is not equal to " +
				$"StartSignalSoundSources length " +
				$"({StartSignalSoundSources.Length}). Please ensure there" +
				$"is exactly one sound source per audio clip.");
		}
	}

	/// <summary>
	/// Changes main volume based on slider in game. Special thanks to Bodix on
	/// StackOverflow.
	/// </summary>
	/// <param name="newVolume">New volume on a logarithmic scale.</param>
	public void ChangeMainVolume(float newVolume)
	{
		MainAudioMixer.SetFloat("mainVol", Mathf.Log10(newVolume) * 20);
	}

	/// <summary>
	/// Changes music volume based on slider in game. Special thanks to Bodix 
	/// on StackOverflow.
	/// </summary>
	/// <param name="newVolume">New volume on a logarithmic scale.</param>
	public void ChangeMusicVolume(float newVolume)
	{
		MainAudioMixer.SetFloat("musicVol", Mathf.Log10(newVolume) * 20);
	}

	/// <summary>
	/// Changes SFX volume based on slider in game. Special thanks to Bodix on
	/// StackOverflow.
	/// </summary>
	/// <param name="newVolume">New volume on a logarithmic scale.</param>
	public void ChangeSFXVolume(float newVolume)
	{
		MainAudioMixer.SetFloat("sfxVol", Mathf.Log10(newVolume) * 20);
	}

	/// <summary>
	/// Ends the currently playing song early by fading it out.
	/// </summary>
	public void FadeMusic(bool isIn)
	{
		if (isIn)
		{
			StartCoroutine(FadeInMusicRoutine());
		}
		else
		{
			StartCoroutine(FadeOutMusicRoutine());
		}
	}

	/// <summary>
	/// Plays ambient audio clip through appropraite audio source.
	/// </summary>
	public void PlayAmbientAudio(float fadeTime)
	{
		AmbientAudioSource.loop = true;
		AmbientAudioSource.clip = AmbientClip;
		AmbientAudioSource.Play();
		StartCoroutine(FadeInAmbienceRoutine(fadeTime));
	}

	/// <summary>
	/// Plays CountdownSound if it is not the start sound, then plays all
	/// StartSignalSounds at once.
	/// </summary>
	/// <param name="isStartSound"></param>
	public void PlayCountDownSound(bool isStartSound)
	{
		if (!isStartSound)
		{
			CountdownAudioSource.PlayOneShot(CountdownClip);
		}
		else
		{
			for (int i = 0; i < StartSignalSoundSources.Length; i++)
			{
				StartSignalSoundSources[i].PlayOneShot(StartSignalClips[i]);
			}
		}
	}

	/// <summary>
	/// Uses the music audio source to play the specified sound for player
	/// loss.
	/// </summary>
	public void PlayLossSound()
	{
		if (MusicAudioSource.isPlaying)
		{
			MusicAudioSource.Stop();
		}
		MusicAudioSource.PlayOneShot(LossClip);
	}

	/// <summary>
	/// Plays the given music track on loop.
	/// </summary>
	/// <param name="musicTrack">Track number of music to be played.</param>
	public void PlayMusic(int musicTrack)
	{
		MusicAudioSource.clip = SoundTrack[musicTrack];
		MusicTracker.text = SoundTrackText[musicTrack];
		MusicAudioSource.Play();
	}

	/// <summary>
	/// Plays random song off soundtrack (ignores element 0 - main menu song);
	/// </summary>
	public void PlayMusic()
	{
		int musicTrack = Random.Range(1, SoundTrack.Length);
		MusicAudioSource.clip = SoundTrack[musicTrack];
		MusicTracker.text = SoundTrackText[musicTrack];
		MusicAudioSource.Play();
	}

	/// <summary>
	/// Uses the music audio source to play the specified sound for player
	/// win.
	/// </summary>
	public void PlayWinSound()
	{
		if (MusicAudioSource.isPlaying)
		{
			MusicAudioSource.Stop();
		}
		MusicAudioSource.PlayOneShot(WinClip);
	}

	/// <summary>
	/// Stops ambient audio clip through appropriate audio source.
	/// </summary>
	public void StopAmbientAudio(float fadeTime)
	{
		StartCoroutine(FadeOutAmbienceRoutine(fadeTime));
	}

	/// <summary>
	/// Routine to slowly fade In ambience based on fadeTime argument.
	/// </summary>
	private IEnumerator FadeInAmbienceRoutine(float fadeTime)
	{
		float elapsedTime = 0.0f;
		while (elapsedTime < fadeTime)
		{
			AmbientAudioSource.volume = Mathf.Lerp(0.0f, 1.0f,
				elapsedTime / fadeTime);
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		AmbientAudioSource.volume = 1;
	}

	/// <summary>
	/// Routine to slowly fade in music based on MusicFadeTime variable.
	/// </summary>
	private IEnumerator FadeInMusicRoutine()
	{
		float elapsedTime = 0.0f;
		while (elapsedTime < MusicFadeTime)
		{
			MusicAudioSource.volume = Mathf.Lerp(0.0f, 1.0f,
				elapsedTime / MusicFadeTime);
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		MusicAudioSource.volume = 1;
	}

	/// <summary>
	/// Routine to slowly fade out ambience based on fadeTime argument.
	/// </summary>
	private IEnumerator FadeOutAmbienceRoutine(float fadeTime)
	{
		float elapsedTime = 0.0f;
		while (elapsedTime < fadeTime)
		{
			AmbientAudioSource.volume = Mathf.Lerp(1.0f, 0.0f,
				elapsedTime / fadeTime);
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		AmbientAudioSource.volume = 0;
		AmbientAudioSource.Stop();
	}

	/// <summary>
	/// Routine to slowly fade out music based on MusicFadeTime variable.
	/// </summary>
	private IEnumerator FadeOutMusicRoutine()
	{
		float elapsedTime = 0.0f;
		while (elapsedTime < MusicFadeTime)
		{
			MusicAudioSource.volume = Mathf.Lerp(1.0f, 0.0f,
				elapsedTime / MusicFadeTime);
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		MusicAudioSource.volume = 0;
	}
}
