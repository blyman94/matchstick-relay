using UnityEngine;

/// <summary>
/// Helper class to add some flare to the main menu.
/// </summary>
public class MainMenuVFX : MonoBehaviour
{
	[Header("VFX")]
	[Tooltip("VFX to be displayed on the 'i' in the main menu.")]
	public GameObject FlameVFX;
	[Tooltip("Initial VFX to be displayed on the 'i' in the main menu.")]
	public GameObject ExplosionVFX;

	[Space]
	[Header("Audio Sorurces")]
	[Tooltip("Loop Audio Source for the main menu audio effects.")]
	public AudioSource LoopAudioSource;
	[Tooltip("One-shot Audio Source for main menu audio effects.")]
	public AudioSource OneShotAudio0;
	[Tooltip("One-shot Audio Source for main menu audio effects.")]
	public AudioSource OneShotAudio1;


	[Space]
	[Header("Audio clips")]
	[Tooltip("Steady burn audio effect slot.")]
	public AudioClip MatchBurnAudio;
	[Tooltip("Audio clip to play on ignite.")]
	public AudioClip MatchIgniteAudio0;
	[Tooltip("Audio clip to play on ignite")]
	public AudioClip MatchIgniteAudio1;


	private void OnEnable()
	{
		GameManager.stateChanged += IgniteUI;
	}

	private void OnDisable()
	{
		GameManager.stateChanged -= IgniteUI;
	}

	private void Start()
	{
		ResetVFX();
	}

	/// <summary>
	/// Ignites initial match when race starts.
	/// </summary>
	/// <param name="gameState">Current game state. Method only ignites
	/// if the incoming gameState is GameState.Running</param>
	private void IgniteUI(GameState gameState)
	{
		if (gameState == GameState.Pregame)
		{
			Ignite();
			PlayAudio();
		}
	}

	/// <summary>
	/// Turns off all VFX.
	/// </summary>
	private void ResetVFX()
	{
		FlameVFX.SetActive(false);
		ExplosionVFX.SetActive(false);
		LoopAudioSource.Stop();
	}

	/// <summary>
	/// Activates the flame VFX.
	/// </summary>
	public void Ignite()
	{
		ExplosionVFX.SetActive(true);
		FlameVFX.SetActive(true);
	}

	/// <summary>
	/// Starts the match burining audio.
	/// </summary>
	public void PlayAudio()
	{
		LoopAudioSource.clip = MatchBurnAudio;
		LoopAudioSource.volume = 1;
		LoopAudioSource.Play();
		OneShotAudio0.PlayOneShot(MatchIgniteAudio0,1);
		OneShotAudio1.PlayOneShot(MatchIgniteAudio1,1);
	}
}
