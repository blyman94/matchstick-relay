using UnityEngine;

/// <summary>
/// Component responsible for timing the player on an individual match level,
/// for passing flame from one match to the next, and signaling losses and
/// wins for each player.
/// </summary>
public class MatchBurnComponent : MonoBehaviour
{
	[Tooltip("Rate at which match head shrinks from combustion. " +
		"Realistically a little faster than the matchstick.")]
	public int PlayerIndex = 0;

	[Header("Match Visuals")]
	[Tooltip("Transform of the primitive representing the matchstick.")]
	public Transform Matchstick;
	[Tooltip("Transform of the primitive representing the match's head.")]
	public Transform MatchstickHead;

	[Header("Burn Rates")]
	[Tooltip("Rate at which match head shrinks from combustion. " +
		"Realistically a little faster than the matchstick.")]
	public float HeadBurnRate = 0.05f;
	[Tooltip("Rate at which the matchstick shrinks from combustion.")]
	public float StickBurnRate = 0.1f;

	[Header("Physics")]
	[Tooltip("Transform who's position is used to check grounded state. " +
		"Need a reference to it to move as the match shrinks.")]
	public Transform GroundCheck;
	[Tooltip("BoxCollider to shrink alongside matchstick, to give the" +
		"appearance of a shrinking match.")]
	public BoxCollider MatchstickCollider;

	[Header("Match Status")]
	[Tooltip("Boolean to determine whether the match is the current match" +
		" the player is controlling.")]
	public bool isCurrentMatch;

	[Tooltip("Tracks whether the match has already been controlled by the" +
		" player.")]
	public bool isPreviousMatch;

	[Header("Misc")]
	[Tooltip("Main Camera to be passed along from match to match. This is " +
		"handled by the new Unity Input system in Versus Mode and Solo " +
		"Mode. If the scene is a Coop Mode scene, this can be safely left" +
		"unassigned.")]
	public Transform CamTransform;

	[Tooltip("Audio Component of the match.")]
	public MatchAudioComponent MatchAudioComponent;

	[Tooltip("The VFX Component of the match.")]
	public MatchVFXComponent vfxComponent;

	/// <summary>
	/// Tracks whether the head has fully burned up, signaling the behavior to
	/// move on to burning the matchstick.
	/// </summary>
	private bool headGone = false;

	/// <summary>
	/// Signals that the match has fully burned, ending the player's run.
	/// </summary>
	public delegate void BurnedOut(string cause);
	public static BurnedOut burnedOut;

	/// <summary>
	/// Signals that the match has made contact with another match, and the
	/// flame (and camera, if in Coop mode) should be passed along.
	/// </summary>
	public delegate void FlamePassed(int playerIndex);
	public static FlamePassed flamePassed;

	/// <summary>
	/// Delegate to signal that the player has reached the goal.
	/// </summary>
	public delegate void ReachedBonfire(int playerIndex);
	public static ReachedBonfire reachedBonfire;

	private void OnTriggerEnter(Collider other)
	{
		if (GameManager.GameState == GameState.Running)
		{
			MatchBurnComponent otherBurnComp =
			other.GetComponent<MatchBurnComponent>();
			if (isCurrentMatch)
			{
				if (otherBurnComp != null)
				{
					if (otherBurnComp.CompareTag("MatchP1") ||
						otherBurnComp.CompareTag("MatchP2"))
					{
						PassFlame(otherBurnComp);
					}
				}

				if (other.CompareTag("Bonfire"))
				{
					reachedBonfire?.Invoke(PlayerIndex);
				}

				if (other.CompareTag("Water"))
				{
					vfxComponent.Extinguish(true);
					SignalLossByWater();
				}
			}
		}
	}

	private void Update()
	{
		if (GameManager.GameState == GameState.Running)
		{
			if (!headGone)
			{
				BurnHead();
			}
			else
			{
				BurnMatchstick();
			}
		}
	}

	/// <summary>
	/// Shrinks the scale of the match head to simulate burning.
	/// </summary>
	private void BurnHead()
	{
		MatchstickHead.localScale -= Vector3.one * HeadBurnRate * Time.deltaTime;
		if (MatchstickHead.localScale.x < 0.05f)
		{
			MatchstickHead.gameObject.SetActive(false);
			headGone = true;
		}
	}

	/// <summary>
	/// Shrinks the y scale of the matchstick and its collider to simulate
	/// burning.
	/// </summary>
	private void BurnMatchstick()
	{
		ReduceMatchstickTransformSize();
		ReduceMatchstickColliderSize();
		MoveVFXComponent();
		MoveGroundCheckComponent();
		MoveCameraComponent();
		CheckBurnout();
	}

	/// <summary>
	/// Determines if the match has lost based on the size of its matchstick.
	/// </summary>
	private void CheckBurnout()
	{
		if (Matchstick.localScale.y < 0.1f)
		{
			vfxComponent.Extinguish(false);
			if (isCurrentMatch)
			{
				SignalLossByBurnout();
			}
			enabled = false;
		}
	}

	/// <summary>
	/// Moves camera in response to the shrinking of the stick, the give the
	/// illusion it is stationary.
	/// </summary>
	private void MoveCameraComponent()
	{
		CamTransform.position = new Vector3(CamTransform.position.x,
					CamTransform.position.y + (StickBurnRate * 0.25f * Time.deltaTime),
					CamTransform.position.z);
	}

	/// <summary>
	/// Moves the ground check component in response to the shrinking of the 
	/// stick, to give the illusion that it is stationary.
	/// </summary>
	private void MoveGroundCheckComponent()
	{
		GroundCheck.position = new Vector3(GroundCheck.position.x,
					GroundCheck.position.y + (StickBurnRate * 0.5f * Time.deltaTime),
					GroundCheck.position.z);
	}

	/// <summary>
	/// Moves the vfx component in response to the shrinking of the stick,
	/// to give the illusion that it is stationary.
	/// </summary>
	private void MoveVFXComponent()
	{
		Vector3 newVFXPosition = new Vector3(vfxComponent.VFXSlot.position.x,
					vfxComponent.VFXSlot.position.y - (StickBurnRate * 0.5f * Time.deltaTime),
					vfxComponent.VFXSlot.position.z);
		vfxComponent.UpdateVFXPosition(newVFXPosition);
	}

	/// <summary>
	/// Transfers player control, burn behaviour and the camera (if in
	/// Coop Mode) to the next match.
	/// </summary>
	/// <param name="other">The collider of the next match to be lit.</param>
	private void PassFlame(MatchBurnComponent otherBurnComp)
	{
		if (!otherBurnComp.isPreviousMatch)
		{
			MatchAudioComponent.StopMatchLoopAudio();

			// Update next match's state
			otherBurnComp.isCurrentMatch = true;
			otherBurnComp.HeadBurnRate = HeadBurnRate;
			otherBurnComp.StickBurnRate = StickBurnRate;

			// Update this match's state
			isPreviousMatch = true;
			gameObject.layer = 11;
			isCurrentMatch = false;

			int nextPlayerIndex;
			if (GameManager.GameMode == GameMode.Coop)
			{
				nextPlayerIndex = PlayerIndex == 1 ? 0 : 1;
				Vector3 camLocalPos = CamTransform.localPosition;
				CamTransform.SetParent(otherBurnComp.transform);
				otherBurnComp.CamTransform = CamTransform;
				CamTransform.localPosition = camLocalPos;
			}
			else
			{
				nextPlayerIndex = PlayerIndex;
			}
			otherBurnComp.vfxComponent.Ignite();
			flamePassed?.Invoke(nextPlayerIndex);
		}
	}

	/// <summary>
	/// Reduces the collider size of the matchstick to match the transform size
	/// as the match shrinks due to burning.
	/// </summary>
	private void ReduceMatchstickColliderSize()
	{
		Vector3 newColliderSize = new Vector3(MatchstickCollider.size.x,
					MatchstickCollider.size.y - StickBurnRate * Time.deltaTime,
					MatchstickCollider.size.z);
		MatchstickCollider.size = newColliderSize;
	}

	/// <summary>
	/// Reduces the size of the transform representing the match as the match
	/// shrinks due to burning.
	/// </summary>
	private void ReduceMatchstickTransformSize()
	{
		Vector3 newMatchstickScale = new Vector3(Matchstick.localScale.x,
					Matchstick.localScale.y - StickBurnRate * Time.deltaTime,
					Matchstick.localScale.z);
		Matchstick.localScale = newMatchstickScale;
	}

	/// <summary>
	/// Signals to GameManager that a player has lost the game by way of 
	/// burning out.
	/// </summary>
	private void SignalLossByBurnout()
	{
		string cause;
		switch (GameManager.GameMode)
		{
			case GameMode.Solo:
				cause = "You burned out!";
				break;
			case GameMode.Coop:
				cause = "Player " + (PlayerIndex + 1) + " burned out!";
				break;
			case GameMode.Versus:
				int winner = PlayerIndex == 0 ? 2 : 1;
				cause = "Player " + winner + " wins! (Player " +
					(PlayerIndex + 1) + " burned out)";
				break;
			default:
				cause = "";
				break;
		}
		burnedOut?.Invoke(cause);
	}

	/// <summary>
	/// Signals to the GameManager that a player has lost the game by way of
	/// water hazard.
	/// </summary>
	private void SignalLossByWater()
	{
		string cause;
		switch (GameManager.GameMode)
		{
			case GameMode.Solo:
				cause = "You were doused!";
				break;
			case GameMode.Coop:
				cause = "Player " + (PlayerIndex + 1) + " was doused!";
				break;
			case GameMode.Versus:
				int winner = PlayerIndex == 0 ? 2 : 1;
				cause = "Player " + winner + " wins! (Player " +
					(PlayerIndex + 1) + " was doused)";
				break;
			default:
				cause = "";
				break;
		}
		burnedOut?.Invoke(cause);
	}
}
