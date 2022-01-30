using System.Collections;
using UnityEngine;

/// <summary>
/// Handles the VFX Behaviour of the bonfire object during the win sequence.
/// </summary>
public class Bonfire : MonoBehaviour
{
	[Tooltip("Audio component for the bonfire, handling its explosion and" +
				"continuous burn audio.")]
	public BonfireAudioComponent bonfireAudio;

	[Header("VFX")]
	[Tooltip("Particle effect for the initial explosion.")]
	public GameObject ExplosionVFX;

	[Tooltip("Particle effect for the continuous flames.")]
	public GameObject FlamesVFX;

	[Tooltip("Particle effect lighting of flames.")]
	public GameObject LightVFX;

	/// <summary>
	/// Delegate to signal all subscribed cameras to shake during bonfire
	/// explosion.
	/// </summary>
	public delegate void ShakeNow();
	public static ShakeNow shakeNow;

	/// <summary>
	/// Delegate to signal all subscribed cameras to tremble during bonfire
	/// drum crescendo.
	/// </summary>
	public delegate void Tremble(float trembleTime);
	public static Tremble tremble;

	public void OnDisable()
	{
		MatchBurnComponent.reachedBonfire -= Ignite;
	}

	public void OnEnable()
	{
		MatchBurnComponent.reachedBonfire += Ignite;
	}

	private void Start()
	{
		ResetVFX();
	}

	/// <summary>
	/// Activates the flame VFX.
	/// </summary>
	/// <param name="playerIndex">UNUSED</param>
	public void Ignite(int playerIndex)
	{
		StartCoroutine(IgniteBonfireRoutine());
	}

	/// <summary>
	/// Coroutine used to align screen rumble, win noises, and bonfire
	/// explosion into an excellent win sequence.
	/// </summary>
	public IEnumerator IgniteBonfireRoutine()
	{
		tremble?.Invoke(3.2f);
		yield return new WaitForSeconds(3.1f);
		ExplosionVFX.SetActive(true);
		shakeNow?.Invoke();
		bonfireAudio.PlayIgniteClip();
		FlamesVFX.SetActive(true);
		LightVFX.SetActive(true);
		bonfireAudio.PlayBonfireLoopAudio();
	}
	
	/// <summary>
	/// Turns off all VFX.
	/// </summary>
	private void ResetVFX()
	{
		ExplosionVFX.SetActive(false);
		FlamesVFX.SetActive(false);
		LightVFX.SetActive(false);
	}
}
