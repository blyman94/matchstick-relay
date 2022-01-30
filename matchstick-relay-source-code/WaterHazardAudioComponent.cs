using UnityEngine;

/// <summary>
/// Class to handle audio for when the water hazard turns on (warning) and when
/// it is running.
/// </summary>
public class WaterHazardAudioComponent : MonoBehaviour
{
	[Header("Audio Sources")]
	[Tooltip("AudioSource for looping clips.")]
	public AudioSource WaterLoopAudioSource;

	[Tooltip("AudioSource for one shot clips.")]
	public AudioSource WaterOneShotAudioSource;

	[Header("Audio Clips")]
	[Tooltip("Audio clip for water running.")]
	public AudioClip waterRunClip;

	[Tooltip("Audio clip for water starts running.")]
	public AudioClip WaterTurnOnClip;
	
	public void OnEnable()
	{
		MatchBurnComponent.reachedBonfire += StopAllNoise;
	}

	public void OnDisable()
	{
		MatchBurnComponent.reachedBonfire -= StopAllNoise;
	}

	/// <summary>
	/// Plays the water's steady running audio.
	/// </summary>
	public void PlayWaterLoopAudio()
	{
		StopWaterLoopAudio();
		WaterLoopAudioSource.clip = waterRunClip;
		WaterLoopAudioSource.loop = true;
		WaterLoopAudioSource.pitch = Random.Range(0.95f, 1.05f);
		WaterLoopAudioSource.Play();
	}

	/// <summary>
	/// Plays water audio based on boolean for whether the water is turning on
	/// or off.
	/// </summary>
	public void PlayWaterOneShotClip()
	{
		AudioClip clip = WaterTurnOnClip;
		WaterOneShotAudioSource.pitch = Random.Range(0.9f, 1.1f);
		WaterOneShotAudioSource.PlayOneShot(clip);
	}

	/// <summary>
	/// Stops all noise playing from the water hazard.
	/// </summary>
	/// <param name="playerIndex">UNUSED</param>
	public void StopAllNoise(int playerIndex)
	{
		WaterOneShotAudioSource.Stop();
		WaterLoopAudioSource.Stop();
	}

	/// <summary>
	/// Stops the water's looping audio.
	/// </summary>
	public void StopWaterLoopAudio()
	{
		WaterLoopAudioSource.Stop();
	}
}
