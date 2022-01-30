using UnityEngine;

public class BonfireAudioComponent : MonoBehaviour
{
	[Header("Audio Sources")]
	[Tooltip("AudioSource for looping clips.")]
	public AudioSource BonfireLoopAudioSource;

	[Tooltip("AudioSource for one shot clips.")]
	public AudioSource BonfireOneShotAudioSource;

	[Header("Audio Clips")]
	[Tooltip("Audio clip for bonfire ignition.")]
	public AudioClip bonfireIgniteClip;

	[Tooltip("Audio clip for bonfire steady burn.")]
	public AudioClip bonfireSteadyBurnClip;

	/// <summary>
	/// Plays the bonfire's steady burn audio.
	/// </summary>
	public void PlayBonfireLoopAudio()
	{
		StopBonfireLoopAudio();
		BonfireLoopAudioSource.clip = bonfireSteadyBurnClip;
		BonfireLoopAudioSource.loop = true;
		BonfireLoopAudioSource.Play();
	}

	/// <summary>
	/// Plays match audio based on provided string.
	/// </summary>
	public void PlayIgniteClip()
	{
		BonfireOneShotAudioSource.PlayOneShot(bonfireIgniteClip);
	}

	/// <summary>
	/// Stops the bonfire's steady burn audio.
	/// </summary>
	public void StopBonfireLoopAudio()
	{
		BonfireLoopAudioSource.Stop();
	}
}
