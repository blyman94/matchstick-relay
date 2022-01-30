using System.Collections;
using UnityEngine;

/// <summary>
/// Allows the camera to listen for match igniting and bonfire igniting events
/// and gives a shake to really sell that explosion.
/// </summary>
public class CameraShakeComponent : MonoBehaviour
{
	[Tooltip("Duration of the camera shake when bonfire lights.")]
	public float shakeDurationBonfire;

	[Tooltip("Duration of the camera shake when match lights.")]
	public float shakeDurationMatch;

	[Tooltip("Magnitude of screen position deviation during shake when" +
		"bonfire lights.")]
	public float shakeIntensityBonfire;

	[Tooltip("Magnitude of screen position deviation during shake when" +
			"match lights.")]
	public float shakeIntensityMatch;

	/// <summary>
	/// Tracks if camera is currently shaking.
	/// </summary>
	private bool isShaking = false;

	/// <summary>
	/// Position of the camera before any shaking.
	/// </summary>
	private Vector3 startPos;

	private void OnDisable()
	{
		GameManager.stateChanged -= CameraShakeMatch;
		MatchBurnComponent.flamePassed -= CameraShakeMatch;
		Bonfire.shakeNow -= CameraShakeBonfire;
		Bonfire.tremble -= Tremble;
	}

	private void OnEnable()
	{
		GameManager.stateChanged += CameraShakeMatch;
		MatchBurnComponent.flamePassed += CameraShakeMatch;
		Bonfire.shakeNow += CameraShakeBonfire;
		Bonfire.tremble += Tremble;
	}

	/// <summary>
	/// Responds to bonfire lighting with bigger shake.
	/// </summary>
	/// <param name="playerIndex">UNUSED</param>
	public void CameraShakeBonfire()
	{
		if (GameManager.GameMode == GameMode.Versus)
		{
			startPos = new Vector3(9, 0.5f, 0);
		}
		else
		{
			startPos = new Vector3(6, 0.5f, 0);
		}
		if (!isShaking)
		{
			StartCoroutine(CameraShakeRoutine(true));
		}
	}

	/// <summary>
	/// Responds to game start and shakes camera during the lighting of the
	/// match.
	/// </summary>
	/// <param name="gameState">Current state of the game. Only responds
	/// if the game state changes to "GameState.Running"</param>
	public void CameraShakeMatch(GameState gameState)
	{
		if (gameState == GameState.Running)
		{
			if (GameManager.GameMode == GameMode.Versus)
			{
				startPos = new Vector3(9, 0.5f, 0);
			}
			else
			{
				startPos = new Vector3(6, 0.5f, 0);
			}
			if (!isShaking)
			{
				StartCoroutine(CameraShakeRoutine(false));
			}
		}
	}

	/// <summary>
	/// Responds to flame passage and shakes camera during the lighting of the
	/// match.
	/// </summary>
	/// <param name="playerIndex">UNUSED</param>
	public void CameraShakeMatch(int playerIndex)
	{
		if (GameManager.GameMode == GameMode.Versus)
		{
			startPos = new Vector3(9, 0.5f, 0);
		}
		else
		{
			startPos = new Vector3(6, 0.5f, 0);
		}
		if (!isShaking)
		{
			StartCoroutine(CameraShakeRoutine(false));
		}
	}

	/// <summary>
	/// Shakes the camera using Random.InsideUnitSphere.
	/// </summary>
	/// <param name="isBonfire">Determines which shake duration and 
	/// intensity to use based on whether the source of the shake is 
	/// a bonfire.</param>
	public IEnumerator CameraShakeRoutine(bool isBonfire)
	{
		isShaking = true;
		float elapsedTime = 0;
		float shakeDuration = isBonfire ? shakeDurationBonfire : shakeDurationMatch;
		float shakeIntensity = isBonfire ? shakeIntensityBonfire : shakeIntensityMatch;
		while (elapsedTime < shakeDuration)
		{
			transform.localPosition = startPos + (Random.insideUnitSphere *
				shakeIntensity);
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		transform.localPosition = startPos;
		isShaking = false;
	}

	/// <summary>
	/// Routine to tremble the camera using Random.insideUnitSphere.
	/// </summary>
	/// <param name="trembleTime">Duration of the Tremble.</param>
	public IEnumerator CameraTrembleRoutine(float trembleTime)
	{
		float elapsedTime = 0;
		while (elapsedTime < (trembleTime - 0.3f))
		{
			transform.localPosition = startPos + (Random.insideUnitSphere *
				shakeIntensityMatch);
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		transform.localPosition = startPos;
	}

	/// <summary>
	/// Slightly shakes the camera, better used for longer periods of shaking.
	/// </summary>
	/// <param name="trembleTime">Duration of the tremble.</param>
	public void Tremble(float trembleTime)
	{
		if (GameManager.GameMode == GameMode.Versus)
		{
			startPos = new Vector3(9, 0.5f, 0);
		}
		else
		{
			startPos = new Vector3(6, 0.5f, 0);
		}
		StartCoroutine(CameraTrembleRoutine(trembleTime));
	}
}
