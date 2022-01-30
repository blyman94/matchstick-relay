using System.Collections;
using UnityEngine;

/// <summary>
/// Behavior for the water hazard obstacle in the level. Water hazards dump 
/// water at specified intervals and immediately put out any match under it.
/// </summary>
public class WaterHazard : MonoBehaviour
{

	[Tooltip("Audio component for the water hazard. Handles its turning on, " +
		"turning off, and stream audio.")]
	public WaterHazardAudioComponent WaterHazardAudioComponent;

	[Header("Behaviour Times")]
	[Tooltip("How long after the start method to begin dumping water.")]
	public float StartTime;

	[Tooltip("How long the water dumps for before turning off.")]
	public float HoseTime;

	[Tooltip("How long to wait between water dumps.")]
	public float RestTime;

	[Header("Particle Systems")]
	[Tooltip("Reference to the particle system in the prefab that " +
		"represents small droplet particles resulting from splash.")]
	public ParticleSystem DropsParticleSystem;

	[Tooltip("Reference to the particle system in the prefab that Distorts" +
		" the ground to create a ripple effect.")]
	public ParticleSystem RipplesParticleSystem;

	[Tooltip("Reference to the particle system in the prefab that " +
		"respresents the impact of the water on the ground.")]
	public ParticleSystem SplashParticleSystem;

	[Tooltip("Reference to the particle system in the prefab that" +
		" respresents the shower of water.")]
	public ParticleSystem StreamParticleSystem;

	[Header("Misc.")]
	[Tooltip("Collider to trigger lose conditions for matches.")]
	public BoxCollider hazardCollider;

	[Tooltip("Cylinder primitive to represent the water area.")]
	public Transform Water;

	/// <summary>
	/// Distance water must travel to reach the ground.
	/// </summary>
	private readonly float maxWaterHeight = 1.5f;

	/// <summary>
	/// How quickly the water falls from pipe to ground.
	/// </summary>
	private readonly float waterFallRate = 2.0f;

	/// <summary>
	/// Cache hose time to a WaitForSeconds.
	/// </summary>
	private WaitForSeconds hoseDelay;

	/// <summary>
	/// Boolean to determine if the water should wait for StartTime before
	/// dumping.
	/// </summary>
	private bool isFirstTime = true;

	/// <summary>
	/// Cache rest time to a WaitForSeconds.
	/// </summary>
	private WaitForSeconds restDelay;

	/// <summary>
	/// Cache start time to a WaitForSeconds.
	/// </summary>
	private WaitForSeconds startDelay;

	/// <summary>
	/// Starting local position of the water transform.
	/// </summary>
	private Vector3 startPos;

	/// <summary>
	/// Starting local size of the water transform.
	/// </summary>
	private Vector3 startScale;

	public void OnDisable()
	{
		MatchBurnComponent.reachedBonfire -= StopAllRoutines;
	}

	public void OnEnable()
	{
		MatchBurnComponent.reachedBonfire += StopAllRoutines;
	}

	private void Start()
	{
		AssignDurationToParticleSystems();

		InitializeWaterHazardCollider();

		CacheWaitForSecondsObjects();

		StartCoroutine(WaterHazardRoutine());
	}

	/// <summary>
	/// Public function to allow for stopping of coroutines by responding to a
	/// delegate with a semantic parameter for unrelated methods.
	/// </summary>
	/// <param name="playerIndex">UNUSED</param>
	public void StopAllRoutines(int playerIndex)
	{
		StopAllCoroutines();
	}

	/// <summary>
	/// Changes the duration of each particle system to match the behavior
	/// of the hazard trigger. Offsets were fine tuned to fit the visual and
	/// cannot be changed in the Editor by design.
	/// </summary>
	private void AssignDurationToParticleSystems()
	{
		ParticleSystem.MainModule streamMain = StreamParticleSystem.main;
		streamMain.duration = HoseTime + 1.5f;

		ParticleSystem.MainModule splashMain = SplashParticleSystem.main;
		splashMain.duration = HoseTime + 0.2f;

		ParticleSystem.MainModule dropsMain = DropsParticleSystem.main;
		dropsMain.duration = HoseTime + 0.2f;

		ParticleSystem.MainModule ripplesMain = RipplesParticleSystem.main;
		ripplesMain.duration = HoseTime + 0.2f;
	}

	/// <summary>
	/// Stores public times as WaitForSeconds objects to be used in the main
	/// coroutine.
	/// </summary>
	private void CacheWaitForSecondsObjects()
	{
		startDelay = new WaitForSeconds(StartTime);
		restDelay = new WaitForSeconds(RestTime);
		hoseDelay = new WaitForSeconds(HoseTime);
	}

	/// <summary>
	/// Sets the position and scale of the water hazard collider, and turns it
	/// off.
	/// </summary>
	private void InitializeWaterHazardCollider()
	{
		hazardCollider.enabled = false;
		startPos = new Vector3(0, 1.653f, 0);
		startScale = new Vector3(0.3f, 0.1f, 0.3f);
	}

	/// <summary>
	/// Routine called to periodically turn on and off water hazard based on 
	/// variables.
	/// </summary>
	private IEnumerator WaterHazardRoutine()
	{
		if (isFirstTime)
		{
			isFirstTime = false;
			yield return startDelay;
		}

		hazardCollider.enabled = true;
		WaterHazardAudioComponent.PlayWaterOneShotClip();
		yield return new WaitForSeconds(1.0f);
		WaterHazardAudioComponent.PlayWaterLoopAudio();
		StreamParticleSystem.Play();

		while(Water.localScale.y < maxWaterHeight)
		{
			Water.localScale = new Vector3(Water.localScale.x,
				Water.localScale.y + waterFallRate * Time.deltaTime,
				Water.localScale.z);
			Water.localPosition = new Vector3(Water.localPosition.x,
				Water.localPosition.y - waterFallRate * Time.deltaTime,
				Water.localPosition.z);
			yield return null;
		}
		SplashParticleSystem.Play();
		yield return hoseDelay;

		while (Water.localScale.y > 0.9f)
		{
			Water.localScale = new Vector3(Water.localScale.x,
				Water.localScale.y - waterFallRate * Time.deltaTime,
				Water.localScale.z);
			Water.localPosition = new Vector3(Water.localPosition.x,
				Water.localPosition.y - waterFallRate * Time.deltaTime,
				Water.localPosition.z);
			yield return null;
		}

		hazardCollider.enabled = false;
		WaterHazardAudioComponent.StopWaterLoopAudio();
		while (Water.localScale.y > 0.1f)
		{
			Water.localScale = new Vector3(Water.localScale.x,
				Water.localScale.y - waterFallRate * Time.deltaTime,
				Water.localScale.z);
			Water.localPosition = new Vector3(Water.localPosition.x,
				Water.localPosition.y - waterFallRate * Time.deltaTime,
				Water.localPosition.z);
			yield return null;
		}

		Water.localPosition = startPos;
		Water.localScale = startScale;

		yield return restDelay;

		StartCoroutine(WaterHazardRoutine());
	}
}
