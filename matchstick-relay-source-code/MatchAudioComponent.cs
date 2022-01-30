using UnityEngine;

public class MatchAudioComponent : MonoBehaviour
{
	[Header("Audio Clips")]
	[Tooltip("Audio clip for match burning out noise.")]
	public AudioClip matchBurnOutClip;

	[Tooltip("Audio clip for match extinguish by water noise.")]
	public AudioClip matchExtinguishClip;

	[Tooltip("Audio clip for match ignite noise.")]
	public AudioClip matchIgniteClip;

	[Tooltip("Additional Audio clip for match ignite noise.")]
	public AudioClip matchIgniteClip1;

	[Tooltip("Audio clip for match jump noise.")]
	public AudioClip matchJumpClip;

	[Tooltip("Audio clip for match move noise.")]
	public AudioClip matchMoveClip;

	[Tooltip("Audio clip for match burn noise. Only clip played by the loop" +
		"audio source.")]
	public AudioClip matchSteadyBurnClip;

	[Header("Audio Sources")]
	[Tooltip("AudioSource for looping clips.")]
	public AudioSource MatchLoopAudioSource;

	[Tooltip("AudioSource for one shot clips.")]
	public AudioSource MatchOneShotAudioSource;

	[Tooltip("Additional AudioSource for one shot clips.")]
	public AudioSource MatchOneShotAudioSource1;

	/// <summary>
	/// Plays the match's steady burn audio.
	/// </summary>
	public void PlayMatchLoopAudio()
	{
		StopMatchLoopAudio();
		MatchLoopAudioSource.clip = matchSteadyBurnClip;
		MatchLoopAudioSource.loop = true;
		MatchLoopAudioSource.Play();
	}

	/// <summary>
	/// Plays match audio based on provided string.
	/// </summary>
	/// <param name="audioType">String depicting which audio clip to 
	/// play. Accepted values: ignite, jump, move, burnOut, extinguish</param>
	public void PlayMatchOneShotAudio(string audioType)
	{
		switch (audioType)
		{
			case ("ignite"):
				MatchOneShotAudioSource.PlayOneShot(matchIgniteClip);
				MatchOneShotAudioSource1.PlayOneShot(matchIgniteClip1);
				break;
			case ("jump"):
				MatchOneShotAudioSource.PlayOneShot(matchJumpClip);
				break;
			case ("move"):
				MatchOneShotAudioSource.PlayOneShot(matchMoveClip);
				break;
			case ("burnOut"):
				MatchOneShotAudioSource.PlayOneShot(matchBurnOutClip);
				break;
			case ("extinguish"):
				MatchOneShotAudioSource.PlayOneShot(matchExtinguishClip);
				break;
			default:
				Debug.LogError("[MatchAudioComponent.cs] Unrecognized string" +
					"passed to PlayMatchAudio(string audioType) method." +
					"Please see docstring for accepted values.");
				break;
		}
	}
	/// <summary>
	/// Stops the match's steady burn audio.
	/// </summary>
	public void StopMatchLoopAudio()
	{
		MatchLoopAudioSource.Stop();
	}
}
