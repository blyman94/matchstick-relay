using UnityEngine;

/// <summary>
/// Helper class to deal with multiple audio listeners during split screen
/// multiplayer.
/// </summary>
public class DisableAudioListener : MonoBehaviour
{
	[Tooltip("This audio Listener will be disabled when the camera switches" +
		"to the player view, to prevent two audio listeners in the same " +
		"scene.")]
	public AudioListener audioListener;

	private void OnEnable()
	{
		MatchControllerComponent.playerJoinedGame += DisableListener;
	}

	private void OnDisable()
	{
		MatchControllerComponent.playerJoinedGame -= DisableListener;
	}

	/// <summary>
	/// Disables the specified audio listener based on game mode and number
	/// of players joined.
	/// </summary>
	public void DisableListener()
	{
		if(GameManager.GameMode != GameMode.Coop)
		{
			if(GameManager.GameMode == GameMode.Versus)
			{
				if(GameManager.PlayersJoined == 1)
				{
					audioListener.enabled = false;
				}
			}
			else
			{
				audioListener.enabled = false;
			}
		}
	}
}
