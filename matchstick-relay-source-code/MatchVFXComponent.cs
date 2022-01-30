using UnityEngine;

/// <summary>
/// Controls the VFX of the match (Lit when current match, smoke when burnt
/// out, and none when idle). Also controls the location of the VFX as the fire
/// burns.
/// </summary>
public class MatchVFXComponent : MonoBehaviour
{
	[Header("Match Components")]
	[Tooltip("Audio component for the match.")]
	public MatchAudioComponent MatchAudioComponent;

	[Tooltip("The match's burn component.")]
	public MatchBurnComponent BurnComponent;

	[Header("VFX")]
	[Tooltip("VFX to be displayed when match is lit.")]
	public GameObject FlameVFX;

	[Tooltip("VFX to be displayed when match has burned out.")]
	public GameObject SmokeVFX;

	[Tooltip("VFX to be displayed when match first ignites.")]
	public GameObject ExplosionVFX;

	[Tooltip("Light emitted by the match.")]
	public GameObject LightVFX;

	[Header("Misc.")]
	[Tooltip("Parent transform to both VFX.")]
	public Transform VFXSlot;

	private void OnEnable()
	{
		GameManager.stateChanged += IgniteInitialMatch;
		GameManager.snuffMatch += Extinguish;
	}

	private void OnDisable()
	{
		GameManager.stateChanged -= IgniteInitialMatch;
		GameManager.snuffMatch -= Extinguish;
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
	private void IgniteInitialMatch(GameState gameState)
	{
		if(gameState == GameState.Running)
		{
			if (BurnComponent.isCurrentMatch)
			{
				Ignite();
			}
		}
	}

	/// <summary>
	/// Turns off all VFX.
	/// </summary>
	private void ResetVFX()
	{
		FlameVFX.SetActive(false);
		SmokeVFX.SetActive(false);
		ExplosionVFX.SetActive(false);
		LightVFX.SetActive(false);
		MatchAudioComponent.StopMatchLoopAudio();
	}

	/// <summary>
	/// Activates the flame VFX.
	/// </summary>
	public void Ignite()
	{
		MatchAudioComponent.PlayMatchOneShotAudio("ignite");
		MatchAudioComponent.PlayMatchLoopAudio();
		ExplosionVFX.SetActive(true);
		FlameVFX.SetActive(true);
		LightVFX.SetActive(true);
	}

	/// <summary>
	/// Deactivates the flame VFX and activates the smoke VFX.
	/// </summary>
	public void Extinguish(bool isWater)
	{
		if(BurnComponent.isCurrentMatch || BurnComponent.isPreviousMatch)
		{
			MatchAudioComponent.StopMatchLoopAudio();
			if (isWater)
			{
				MatchAudioComponent.PlayMatchOneShotAudio("extinguish");
			}
			else
			{
				MatchAudioComponent.PlayMatchOneShotAudio("burnOut");
			}
			FlameVFX.SetActive(false);
			LightVFX.SetActive(false);
			SmokeVFX.SetActive(true);
		}
	}

	/// <summary>
	/// Changes position of the VFX slot. Useful for updating its position when
	/// the match shrinks.
	/// </summary>
	/// <param name="newPosition">New position of the VFX slot.</param>
	public void UpdateVFXPosition(Vector3 newPosition)
	{
		VFXSlot.position = newPosition;
	}
}
