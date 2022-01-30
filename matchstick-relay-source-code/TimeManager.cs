using UnityEngine;

/// <summary>
/// Responsible for timing the player's run when the game is in the Running 
/// state.
/// </summary>
public class TimeManager : MonoBehaviour
{
	/// <summary>
	/// Amount of time passed since beginning of run.
	/// </summary>
	[HideInInspector]
	public int timer = 0;

	/// <summary>
	/// Event to notify subscribers that time has changed. Used mainly to
	/// update HUD timer.
	/// </summary>
	/// <param name="newTime">Current time since beginning of run.</param>
	public delegate void TimeChanged(int newTime);
	public TimeChanged timeChanged;

	private void OnEnable()
	{
		GameManager.stateChanged += ManageTimer;
	}

	private void OnDisable()
	{
		GameManager.stateChanged -= ManageTimer;
	}

	/// <summary>
	/// Update behaviour of the timer according to state. In the Running state,
	/// the timer counts up. In the Paused state, the timer stops counting. In
	/// the Postgame state, the time is recorded into the scoreboard. In the 
	/// Pregame state, the time is reset to 0.
	/// </summary>
	/// <param name="gameState"></param>
	private void ManageTimer(GameState gameState)
	{
		switch (gameState)
		{
			case GameState.Running:
				timer = 0;
				timeChanged?.Invoke(timer);
				InvokeRepeating("TimeGame", 0, 1);
				break;
			case GameState.Paused:
				// Stop timer on pause
				CancelInvoke();
				break;
			case GameState.Postgame:
				CancelInvoke();
				break;
			case GameState.Pregame:
				timer = 0;
				timeChanged?.Invoke(timer);
				break;
		}
	}

	/// <summary>
	/// Incremement timer to reflect time spent on run.
	/// </summary>
	private void TimeGame()
	{
		timer += 1;
		timeChanged?.Invoke(timer);
	}
}
