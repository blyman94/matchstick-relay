using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// A Singleton used to manage all Canvas/UI elements, such as showing screens,
/// HUD elements, and button click events.
/// </summary>
public class UIManager : Singleton<UIManager>
{
	[Header("Manager References")]
	[Tooltip("TimeManager object to which the HUD display is subscribed.")]
	public TimeManager TimeManager;

	[Header("Menu Navigation")]
	[Tooltip("The first button highlighted by the Gamepad when the game" +
		"over menu is opened.")]
	public GameObject gameOverMenuFirstButton;

	[Tooltip("The first button highlighted by the Gamepad when the" +
		"'how to play' screen closes. Should be the button that originally " +
		"opened the menu.")]
	public GameObject htpClosedButton;

	[Tooltip("The first button highlighted by the Gamepad when the" +
		"'how to play' screen opens. Should be a button on the new screen.")]
	public GameObject htpFirstButton;

	[Tooltip("The first button highlighted by the Gamepad when the main" +
		"menu is loaded.")]
	public GameObject mainMenuFirstButton;

	[Tooltip("The first button highlighted by the Gamepad when the pause" +
		"menu is opened.")]
	public GameObject pauseMenuFirstButton;

	[Tooltip("The first button highlighted by the Gamepad when the settings" +
		"menu is closed. Should be the button that originally opened the " +
		"menu.")]
	public GameObject settingsClosedButton;

	[Tooltip("The first button highlighted by the Gamepad when the settings" +
		"menu is opened. Can be a slider.")]
	public GameObject settingsFirstButton;

	[Tooltip("The first button highlighted by the Gamepad when the win menu" +
		"is opened.")]
	public GameObject winMenuFirstButton;

	[Header("Screen/UI Object References")]
	[Tooltip("Collection of UI Elements associated with the countdown to the" +
		" race.")]
	public GameObject CountdownTextObject;

	[Tooltip("Collection of UI Elements representing the Game Over Menu.")]
	public GameObject GameOverMenuObject;

	[Tooltip("Collection of UI Elements representing the How To Play screen.")]
	public GameObject HowToPlayScreenObject;

	[Tooltip("Collection of UI Elements representing the Player HUD.")]
	public GameObject HUDScreenObject;

	[Tooltip("Collection of UI Elements representing the loading screen.")]
	public GameObject LoadingScreenObject;

	[Tooltip("Collection of UI Elements representing the Main Menu.")]
	public GameObject MainMenuObject;

	[Tooltip("Collection of UI Elements representing the Pause Menu.")]
	public GameObject PauseMenuObject;

	[Tooltip("The object of the TMP component that prompts players to join" +
		"the game.")]
	public GameObject PlayerInputPromptObject;

	[Tooltip("The object of the TMP component that is shown when one player" +
		"has connected to a two player game mode.")]
	public GameObject PlayerOneInputConfrimation;

	[Tooltip("Collection of UI Elements representing the Settings screen.")]
	public GameObject SettingsScreenObject;

	[Tooltip("Collection of UI Elements representing the Win Menu.")]
	public GameObject WinMenuObject;

	[Header("Sliders")]
	[Tooltip("Slider representing the main volume. User can interact with" +
		"this slider to change the main mixer levels to their preferences.")]
	public Slider mainVolSlider;

	[Tooltip("Slider representing the music volume. User can interact with" +
		"this slider to change the main mixer levels to their preferences.")]
	public Slider musicVolSlider;

	[Tooltip("Slider representing the sound effects volume. User can interact " +
		"with this slider to change the main mixer levels to their " +
		"preferences.")]
	public Slider sfxVolSlider;

	[Header("TMP Elements")]
	[Tooltip("The TextMeshPro_Text canvas UI element to be updated " +
		"with the cause of the players loss. Should be located on the" +
		"'Game Over' screen.")]
	public TextMeshProUGUI CauseText;

	[Tooltip("The TextMeshPro_Text canvas UI element to be updated " +
		"with the GameManager's countdown sequence. Shold be on its" +
		"own object.")]
	public TextMeshProUGUI CountdownText;

	[Tooltip("The TextMeshPro_Text canvas UI element to be updated " +
		"with the player's final time of the run. Should be located on" +
		"the 'Win' screen.")]
	public TextMeshProUGUI DetailText;

	[Tooltip("The TextMeshPro_Text canvas UI element to be updated " +
		"with the a random tip during the load screen.")]
	public TextMeshProUGUI GameTipText;

	[Tooltip("The TextMeshPro_Text canvas UI element to shown when loading.")]
	public TextMeshProUGUI LoadingText;

	[Tooltip("The TextMeshPro_Text canvas UI element to be updated " +
		"according to the timer on the Player's HUD.")]
	public TextMeshProUGUI TimeDisplay;

	[Tooltip("The TextMeshPro_Text canvas UI element to be updated " +
		"with the a message signaling who wins. Should be located on " +
		"the 'Win' screen.")]
	public TextMeshProUGUI WinText;

	[Header("Misc.")]
	[Tooltip("List of tips to randomly be shown on loading screens.")]
	public string[] Tips;

	/// <summary>
	/// Bool for toggling the GameOver menu.
	/// </summary>
	private bool gameOverMenuShowing = false;

	/// <summary>
	/// Bool for toggling the pause menu.
	/// </summary>
	private bool pauseMenuShowing = false;

	/// <summary>
	/// Bool for toggling the Win menu.
	/// </summary>
	private bool winMenuShowing = false;

	/// <summary>
	/// Allows the GameManager to update the countdown text in accordance with
	/// its countdown sequence.
	/// </summary>
	/// <param name="number">Integer representing the current number
	/// in the countdown.</param>
	public void ChangeCountdownText(int number)
	{
		CountdownText.text = "" + number;
	}

	/// <summary>
	/// Overload that allows the countdown text to be turned to "GO!" at the 
	/// start of the race.
	/// </summary>
	/// <param name="newString">"Go!" string.</param>
	public void ChangeCountdownText(string newString)
	{
		CountdownText.text = newString;
	}

	/// <summary>
	/// Hides the countdown number, called by the GameManager after the 
	/// countdown sequence ends.
	/// </summary>
	public void HideCountDownNumber()
	{
		CountdownTextObject.SetActive(false);
	}

	/// <summary>
	/// Hides the How to Play Screen, returning the player to the main menu.
	/// </summary>
	public void HideHowToPlayScreen()
	{
		HowToPlayScreenObject.SetActive(false);
		EventSystem.current.SetSelectedGameObject(null);
		EventSystem.current.SetSelectedGameObject(htpClosedButton);
	}

	/// <summary>
	/// Hides the HUD screen for the player.
	/// </summary>
	public void HideHudScreen()
	{
		HUDScreenObject.SetActive(false);
	}

	/// <summary>
	/// Hides the loading scene.
	/// </summary>
	public void HideLoadingScreen()
	{
		StartCoroutine(LoadScreenFadeRoutine(false));
	}

	/// <summary>
	/// Hides the line of text that signals to the user that the first player
	/// has joined a two player game mode.
	/// </summary>
	public void HidePlayerInputPrompt()
	{
		PlayerInputPromptObject.SetActive(false);
		PlayerOneInputConfrimation.SetActive(false);
	}

	/// <summary>
	/// Hides the settings Screen, returning the player to the main menu.
	/// </summary>
	public void HideSettingsScreen()
	{
		SettingsScreenObject.SetActive(false);
		EventSystem.current.SetSelectedGameObject(null);
		if (GameManager.GameState == GameState.Paused)
		{
			EventSystem.current.SetSelectedGameObject(pauseMenuFirstButton);
		}
		else
		{
			EventSystem.current.SetSelectedGameObject(settingsClosedButton);
		}
	}

	/// <summary>
	/// Restores the default settings of all volume sliders, which in turn
	/// changes the current volume back to defaults.
	/// </summary>
	public void RestoreSettingDefaults()
	{
		mainVolSlider.value = 1;
		musicVolSlider.value = 1;
		sfxVolSlider.value = 1;
	}

	/// <summary>
	/// Helper function to instruct the event system which button to select
	/// when opening the main menu.
	/// </summary>
	public void SelectStartingMainButton()
	{
		EventSystem.current.SetSelectedGameObject(null);
		EventSystem.current.SetSelectedGameObject(mainMenuFirstButton);
	}

	/// <summary>
	/// Activates the object containing the countdown number for the countdown
	/// sequence.
	/// </summary>
	public void ShowCountDownNumber()
	{
		CountdownTextObject.SetActive(true);
	}

	/// <summary>
	/// Shows the instructional "How to Play" screen from the main menu.
	/// </summary>
	public void ShowHowToPlayScreen()
	{
		HowToPlayScreenObject.SetActive(true);
		EventSystem.current.SetSelectedGameObject(null);
		EventSystem.current.SetSelectedGameObject(htpFirstButton);
	}

	/// <summary>
	/// Shows the HUD screen for the player, which gives them their current
	/// race time.
	/// </summary>
	public void ShowHudScreen()
	{
		HUDScreenObject.SetActive(true);
	}

	/// <summary>
	/// Shows the loading screen with a random tip.
	/// </summary>
	public void ShowLoadingScreen()
	{
		string tip = Tips[Random.Range(0, Tips.Length)];
		GameTipText.text = "Tip: " + tip;
		GameTipText.color = Color.clear;
		LoadingText.color = Color.clear;
		LoadingScreenObject.SetActive(true);
		StartCoroutine(LoadScreenFadeRoutine(true));
	}

	/// <summary>
	/// Shows a line of text prompting the player to press a button to join.
	/// </summary>
	public void ShowPlayerInputPrompt()
	{
		PlayerInputPromptObject.SetActive(true);
	}

	/// <summary>
	/// Shows a line of text to signal to the user that the first player has
	/// joined a two player game mode.
	/// </summary>
	public void ShowPlayerOneConfirmation()
	{
		PlayerOneInputConfrimation.SetActive(true);
	}

	/// <summary>
	/// Shows the settings screen from the main menu.
	/// </summary>
	public void ShowSettingsScreen()
	{
		SettingsScreenObject.SetActive(true);
		EventSystem.current.SetSelectedGameObject(null);
		EventSystem.current.SetSelectedGameObject(settingsFirstButton);
	}

	/// <summary>
	/// Turns the GameOver menu on and off, called by the UI.
	/// </summary>
	/// <param name="cause"></param>
	public void ToggleGameOverMenu(string cause)
	{
		CauseText.text = cause;
		gameOverMenuShowing = !gameOverMenuShowing;
		GameOverMenuObject.SetActive(gameOverMenuShowing);
		if (gameOverMenuShowing)
		{
			EventSystem.current.SetSelectedGameObject(null);
			EventSystem.current.SetSelectedGameObject(gameOverMenuFirstButton);
		}
	}

	/// <summary>
	/// Turns the pause menu on and off, called by the GameManager.
	/// </summary>
	public void TogglePauseMenu()
	{
		pauseMenuShowing = !pauseMenuShowing;
		PauseMenuObject.SetActive(pauseMenuShowing);
		if (pauseMenuShowing)
		{
			EventSystem.current.SetSelectedGameObject(null);
			EventSystem.current.SetSelectedGameObject(pauseMenuFirstButton);
		}
	}

	/// <summary>
	/// Turns the Win menu on and off, called by the UI.
	/// </summary>
	/// <param name="cause"></param>
	public void ToggleWinMenu(int playerIndex)
	{
		switch (GameManager.GameMode)
		{
			case (GameMode.Solo):
				WinText.text = "You Win!";
				DetailText.text = "You Finished in " + TimeDisplay.text +
					" Seconds!";
				break;
			case (GameMode.Coop):
				WinText.text = "You Win!";
				DetailText.text = "Your Team Finished in " +
					TimeDisplay.text + " Seconds!";
				break;
			case (GameMode.Versus):
				WinText.text = "Player " + (playerIndex + 1) + " Wins!";
				DetailText.text = "";
				break;
			default:
				break;
		}
		winMenuShowing = !winMenuShowing;
		WinMenuObject.SetActive(winMenuShowing);
		if (winMenuShowing)
		{
			EventSystem.current.SetSelectedGameObject(null);
			EventSystem.current.SetSelectedGameObject(winMenuFirstButton);
		}
	}

	/// <summary>
	/// Routine to slowly fade the load screen in and out.
	/// </summary>
	/// <param name="isIn">If true, fades scene in. Otherwise, fades 
	/// scene out.</param>
	/// <returns></returns>
	private IEnumerator LoadScreenFadeRoutine(bool isIn)
	{
		float elapsedTime = 0.0f;
		if (isIn)
		{
			while (elapsedTime < 1.0f)
			{
				GameTipText.color = new Color(1, 1, 1, Mathf.Lerp(0, 1, elapsedTime));
				LoadingText.color = new Color(1, 1, 1, Mathf.Lerp(0, 1, elapsedTime));
				elapsedTime += Time.deltaTime;
				yield return null;
			}
		}
		else
		{
			while (elapsedTime < 1.0f)
			{
				GameTipText.color = new Color(1, 1, 1, Mathf.Lerp(1, 0, elapsedTime));
				LoadingText.color = new Color(1, 1, 1, Mathf.Lerp(1, 0, elapsedTime));
				elapsedTime += Time.deltaTime;
				yield return null;
			}
			LoadingScreenObject.SetActive(false);
		}
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoad;
		TimeManager.timeChanged -= UpdateTimeDisplay;
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoad;
		TimeManager.timeChanged += UpdateTimeDisplay;
	}
	/// <summary>
	/// Activates the Main Menu object if the scene loaded is the MainMenu
	/// scene.
	/// </summary>
	/// <param name="scene">Name of the scene loaded</param>
	/// <param name="loadSceneMode">UNUSED</param>
	private void OnSceneLoad(Scene scene, LoadSceneMode loadSceneMode)
	{
		if(scene.name == "MainMenu")
		{
			MainMenuObject.SetActive(true);
		}
		else
		{
			MainMenuObject.SetActive(false);
		}
	}
	/// <summary>
	/// Synchronizes the HUD display to the TimeManager's current timer.
	/// </summary>
	/// <param name="newTime">Current time of the run.</param>
	private void UpdateTimeDisplay(int newTime)
	{
		TimeDisplay.text = newTime.ToString();
	}
}
