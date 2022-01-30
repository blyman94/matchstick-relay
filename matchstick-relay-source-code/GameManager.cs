using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Enum to track which game mode the game is in. Helpful when dealing with
/// multiplayer input;
/// </summary>
public enum GameMode { Solo, Coop, Versus, Default }

/// <summary>
/// Enum to track the state of the game. Useful for allowing certain actions
/// at certain points in the game.
/// </summary>
public enum GameState { Pregame, Running, Paused, Postgame, Default }

/// <summary>
/// Singleton class that handles game state, scene transitions, win and loss
/// events, and general game flow.
/// </summary>
public class GameManager : Singleton<GameManager>
{
	/// <summary>
	/// Tracks the game mode of the game. Multiplayer input is an example of a
	/// component that behaves differently based on game mode.
	/// </summary>
	public static GameMode GameMode = GameMode.Default;

	/// <summary>
	/// Tracks how many players have joined the game. This number is compared
	/// to an expected number of players to join a specific game type and 
	/// signals the GameManager to start the game when all players have
	/// joined.
	/// </summary>
	public static float PlayersJoined;

	[Tooltip("UIAudioManager to be controlled by the GameManager.")]
	public AudioManager AudioManager;

	[Tooltip("Time (seconds) it takes for a scene to fade in or out.")]
	[Range(2.0f, 10.0f)]
	public float FadeTime = 2.0f;

	[Tooltip("Time (seconds) it takes for a scene to load. Give plenty of" +
		" time for textures to load and to allow the player to read the" +
		" tips.")]
	[Range(2.0f, 10.0f)]
	public float LoadTime = 2.0f;

	[Tooltip("Black image that covers the whole screen. Will be used " +
		"to fade between scenes.")]
	public Image sceneFader;

	[Tooltip("UIManager to be controlled by the GameManager.")]
	public UIManager UIManager;

	/// <summary>
	/// Tracks the state of the game. Game components behave differently
	/// according to state.
	/// </summary>
	public static GameState gameState = GameState.Default;

	/// <summary>
	/// Delegate to snuff match on restart.
	/// </summary>
	public delegate void SnuffCurrentMatch(bool isWater);
	public static SnuffCurrentMatch snuffMatch;

	/// <summary>
	/// Delegate to update all subscribed game components that the game state
	/// has changed, and provides the state it has changed to.
	/// </summary>
	/// <param name="gameState">Current state of the game.</param>
	public delegate void StateChanged(GameState gameState);
	public static StateChanged stateChanged;

	/// <summary>
	/// Property for the gameState that ensures "StateChanged" is evoked
	/// whenever a new state is set.
	/// </summary>
	public static GameState GameState
	{
		get
		{
			return gameState;
		}
		set
		{
			stateChanged?.Invoke(value);
			gameState = value;
		}
	}

	private void OnDisable()
	{
		MatchBurnComponent.reachedBonfire -= WinGame;
		MatchBurnComponent.burnedOut -= LoseGame;
		MatchControllerComponent.pauseButtonPressed -= TogglePauseState;
		SceneManager.sceneLoaded -= OnSceneLoad;
	}

	private void OnEnable()
	{
		MatchBurnComponent.reachedBonfire += WinGame;
		MatchBurnComponent.burnedOut += LoseGame;
		MatchControllerComponent.pauseButtonPressed += TogglePauseState;
		SceneManager.sceneLoaded += OnSceneLoad;
	}

	private void Start()
	{
		GameMode = GameMode.Default;
		GameState = GameState.Default;
		LoadScene("MainMenu");
	}

	/// <summary>
	/// Counts down the start of the race, so the player is ready when time
	/// actually starts.
	/// </summary>
	public IEnumerator CountdownRoutine()
	{
		// 3...
		UIManager.ChangeCountdownText(3);
		UIManager.ShowCountDownNumber();
		AudioManager.PlayCountDownSound(false);
		yield return new WaitForSeconds(1);

		// 2...
		UIManager.ChangeCountdownText(2);
		AudioManager.PlayCountDownSound(false);
		yield return new WaitForSeconds(1);

		// 1...
		UIManager.ChangeCountdownText(1);
		AudioManager.PlayCountDownSound(false);
		yield return new WaitForSeconds(1);

		// GO!
		UIManager.ChangeCountdownText("GO!");
		AudioManager.PlayCountDownSound(true);
		AudioManager.AmbientAudioSource.volume = 0.6f;
		UIManager.ShowHudScreen();
		GameState = GameState.Running;
		AudioManager.MusicAudioSource.volume = 1;
		AudioManager.PlayMusic();
		yield return new WaitForSeconds(0.5f);
		UIManager.HideCountDownNumber();

	}

	/// <summary>
	/// A method to wrap the SceneManager.LoadScene(str) method that also sets
	/// the GameMode field.
	/// </summary>
	/// <param name="sceneName">Name of the scene to transition to.</param>
	public void LoadScene(string sceneName)
	{
		snuffMatch?.Invoke(false);
		string toScene;
		if (sceneName == "Current")
		{
			// We are restarting
			toScene = SceneManager.GetActiveScene().name;
		}
		else
		{
			toScene = sceneName;
		}

		/* Switch expressions are not supported by the Unity editor. Ignore the
		suggestion in the following line. */
		switch (toScene)
		{
			case ("Solo"):
				GameMode = GameMode.Solo;
				break;
			case ("Coop"):
				GameMode = GameMode.Coop;
				break;
			case ("Versus"):
				GameMode = GameMode.Versus;
				break;
			default:
				GameMode = GameMode.Default;
				break;
		}

		StartCoroutine(TransitionToScene(toScene));
	}

	/// <summary>
	/// Quits out of the application. Called from in game UI.
	/// </summary>
	public void QuitGame()
	{
		Application.Quit();
	}

	/// <summary>
	/// Toggles the GameState to paused when the player presses the pause 
	/// button.
	/// </summary>
	public void TogglePauseState()
	{
		if (GameState == GameState.Paused)
		{
			UIManager.TogglePauseMenu();
			GameState = GameState.Running;
		}
		else
		{
			if (GameState != GameState.Running)
			{
				// Player cannot pause in any state but GameState.Running.
				return;
			}
			else
			{
				UIManager.TogglePauseMenu();
				GameState = GameState.Paused;
			}
		}
	}

	/// <summary>
	/// Coroutine to smoothly fade each scene in and out.
	/// </summary>
	/// <param name="sceneName">Name of the scene to transition to.</param>
	public IEnumerator TransitionToScene(string sceneName)
	{
		EventSystem.current.SetSelectedGameObject(null);

		// Fade out audio
		if(AudioManager.MusicAudioSource.volume > 0)
		{
			AudioManager.FadeMusic(false);
		}
		AudioManager.StopAmbientAudio(FadeTime);

		// Fade out current scene
		sceneFader.gameObject.SetActive(true);
		float elapsedTime = 0.0f;
		while (elapsedTime < FadeTime)
		{
			float alpha = Mathf.Lerp(0, 1, elapsedTime / FadeTime);
			sceneFader.color = new Color(0, 0, 0, alpha);
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		UIManager.HideHudScreen();
		sceneFader.color = new Color(0, 0, 0, 1);

		SceneManager.LoadScene(sceneName);

		// Show Loading Screen
		elapsedTime = 0.0f;
		UIManager.ShowLoadingScreen();
		sceneFader.gameObject.SetActive(false);
		while (elapsedTime < LoadTime)
		{
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		UIManager.HideLoadingScreen();
		yield return new WaitForSeconds(1.0f);
		sceneFader.gameObject.SetActive(true);
		yield return new WaitForSeconds(1.0f);

		// Fade in new scene
		elapsedTime = 0.0f;
		if (sceneName != "MainMenu")
		{
			AudioManager.PlayAmbientAudio(FadeTime);
		}
		while (elapsedTime < FadeTime)
		{
			float alpha = Mathf.Lerp(1, 0, elapsedTime / FadeTime);
			sceneFader.color = new Color(0, 0, 0, alpha);
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		sceneFader.color = new Color(0, 0, 0, 0);
		sceneFader.gameObject.SetActive(false);

		// Wait for players based on which scene was loaded.
		if (sceneName == "Coop" || sceneName == "Versus")
		{
			PlayersJoined = 0;
			StartCoroutine(WaitForPlayersRoutine(2));
		}
		else if (sceneName == "Solo")
		{
			PlayersJoined = 0;
			StartCoroutine(WaitForPlayersRoutine(1));
		}
		else
		{
			// Or just return to main menu.
			AudioManager.MusicAudioSource.volume = 1;
			AudioManager.PlayMusic(0);
			UIManager.SelectStartingMainButton();
			GameState = GameState.Pregame;
		}
	}

	/// <summary>
	/// Starts a coroutine that ends only when the correct number of players 
	/// have joined the game. The game state will not change to "Running" 
	/// until all players have joined.
	/// </summary>
	/// <param name="numPlayers">The number of players expected in this
	/// game type.</param>
	public IEnumerator WaitForPlayersRoutine(int numPlayers)
	{
		UIManager.ShowPlayerInputPrompt();
		PlayerInputManager.instance.EnableJoining();
		bool broken = false;
		while (PlayersJoined < numPlayers)
		{
			float timer = 0.0f;
			while (PlayersJoined == 1)
			{
				if(timer == 0.0f)
				{
					UIManager.ShowPlayerOneConfirmation();
				}
				timer += Time.deltaTime;
				if(timer >= 10)
				{
					broken = true;
					break;
				}
				yield return null;
			}
			if (broken)
			{
				break;
			}
			else
			{
				yield return null;
			}
		}

		UIManager.HidePlayerInputPrompt();
		if (broken)
		{
			LoadScene("MainMenu");
		}
		else
		{
			StartCoroutine(CountdownRoutine());
		}
	}

	/// <summary>
	/// Start the loss sequence.
	/// </summary>
	/// <param name="cause">String describing how the player has lost.</param>
	private void LoseGame(string cause)
	{
		UIManager.HideHudScreen();
		StartCoroutine(LoseGameRoutine(cause));
	}

	/// <summary>
	/// A routine to wait for the player's graphics to repond to a loss, then
	/// to trigger the game over screen.
	/// </summary>
	/// <param name="cause">String describing how the player has lost.</param>
	/// <returns></returns>
	private IEnumerator LoseGameRoutine(string cause)
	{
		GameState = GameState.Postgame;
		AudioManager.PlayLossSound();
		AudioManager.PlayAmbientAudio(FadeTime);
		yield return new WaitForSeconds(2.5f);
		UIManager.ToggleGameOverMenu(cause);
	}

	/// <summary>
	/// Responds to the SceneManager sceneLoaded event. Disables joining on the
	/// player input manager so the player cannot join before the GameManager
	/// expects it to.
	/// </summary>
	/// <param name="scene"></param>
	/// <param name="loadSceneMode"></param>
	private void OnSceneLoad(Scene scene, LoadSceneMode loadSceneMode)
	{
		if (scene.name == "Solo" || scene.name == "Coop" ||
			scene.name == "Versus")
		{
			PlayerInputManager.instance.DisableJoining();
		}
	}

	/// <summary>
	/// Start the win game sequence.
	/// </summary>
	private void WinGame(int playerIndex)
	{
		AudioManager.PlayWinSound();
		StartCoroutine(WinGameRoutine(playerIndex));
	}

	/// <summary>
	/// The win game sequence. Waits a few seconds to let the player admire
	/// the bonfire, then gives them the option to move on from the scene.
	/// </summary>
	/// <param name="playerIndex"></param>
	/// <returns></returns>
	private IEnumerator WinGameRoutine(int playerIndex)
	{
		GameState = GameState.Postgame;
		yield return new WaitForSeconds(3.5f);
		UIManager.HideHudScreen();
		yield return new WaitForSeconds(2.0f);
		UIManager.ToggleWinMenu(playerIndex);
	}
}
