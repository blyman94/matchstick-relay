using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Dynamically assigned when a player is added to a scene. Handles Unity Input
/// system events to controller the current match.
/// </summary>
public class MatchControllerComponent : MonoBehaviour
{
	[Tooltip("Reference to an audio listener of this object. Since Unity" +
				" does not allow multiple audio listeners per scene, this cache " +
		"helps to tell Unity which audio source should be the active one" +
		" in the case of multiplayer split screen.")]
	public AudioListener AudioListener;

	/// <summary>
	/// BurnComponent to control this match's burning
	/// </summary>
	[HideInInspector]
	public MatchBurnComponent burnComponent;

	[Tooltip("The player camera to instantiate alongside a new player, which" +
			"enables spit screen when instantiated by the PlayerInputManager " +
		"object")]
	public Camera PlayerCam;

	[Tooltip("PlayerInput object from Unity's new player input system." +
					"Coupling this object with a camera allows for split screen when " +
		"instatiated via the PlayerInputManager object.")]
	public PlayerInput PlayerInput;

	/// <summary>
	/// MoveComponent to control this match's movement. Located via
	/// "FindGameObjectsWithTag" when a new player enters the game.
	/// </summary>
	private MatchMoveComponent moveComponent;

	/// <summary>
	/// Index of the player. This game only supports two players, so this
	/// value should only ever be 0 or 1.
	/// </summary>
	private int playerIndex;

	/// <summary>
	/// Delegate to signal the GameManager and UIManager that the player has
	/// pressed the pause button.
	/// </summary>
	public delegate void PauseButtonPressed();
	public static PauseButtonPressed pauseButtonPressed;
	
	/// <summary>
	/// Delegate to signal when a player has joined. Helps to disable 
	/// additional audio controllers when multiplayer is introduced.
	/// </summary>
	public delegate void PlayerJoinedGame();
	public static PlayerJoinedGame playerJoinedGame;

	private void OnDisable()
	{
		MatchBurnComponent.flamePassed -= GetCurrentMatch;
	}

	private void OnEnable()
	{
		MatchBurnComponent.flamePassed += GetCurrentMatch;
	}

	/// <summary>
	/// Accepts user input for jumping and calls the Jump method of 
	/// moveComponent. If the input is released, calls the EndJump method of
	/// moveComponent.
	/// </summary>
	/// <param name="context">Input context.</param>
	public void OnJump(InputAction.CallbackContext context)
	{
		if (burnComponent != null && moveComponent != null)
		{
			if (burnComponent.isCurrentMatch &&
			GameManager.GameState == GameState.Running)
			{
				if (context.started)
				{
					moveComponent.Jump();
				}
				else if (context.canceled)
				{
					moveComponent.EndJump();
				}
			}
		}
	}

	/// <summary>
	/// Accepts user input for movement and calls the Move method of
	/// moveComponent.
	/// </summary>
	/// <param name="context">Input context.</param>
	public void OnMovement(InputAction.CallbackContext context)
	{
		if (burnComponent != null && moveComponent != null)
		{
			if (burnComponent.isCurrentMatch &&
			   GameManager.GameState == GameState.Running)
			{
				Vector3 movementInput = new Vector3(0, 0, context.ReadValue<float>());
				moveComponent.Move(movementInput);
			}
		}

	}

	/// <summary>
	/// Invokes the "PauseButtonPressed" delegate when the player presses the
	/// pause button. The GameManager and UIManager are subscribed to this
	/// delegate.
	/// </summary>
	/// <param name="context"></param>
	public void OnPause(InputAction.CallbackContext context)
	{
		/* Work around for new Input System bug where prefab also calls the
		method */
		if (gameObject.scene.IsValid())
		{
			if (context.performed)
			{
				pauseButtonPressed?.Invoke();
			}
		}
	}

	/// <summary>
	/// Finds the move and burn components of the appropriate match when the 
	/// game starts and when matches switch.
	/// </summary>
	private void AssignMatchComponents()
	{
		MatchMoveComponent[] moveComponents
							= FindObjectsOfType<MatchMoveComponent>();

		moveComponent =
			moveComponents.FirstOrDefault(m => m.playerIndex == playerIndex &&
			m.gameObject.GetComponent<MatchBurnComponent>().isCurrentMatch);

		burnComponent =
			moveComponent.gameObject.GetComponent<MatchBurnComponent>();
	}

	private void Awake()
	{
		HandlePlayerJoin();
		if (GameManager.GameMode == GameMode.Versus)
		{
			GetCurrentMatch(playerIndex);
		}
		else
		{
			if (playerIndex == 0)
			{
				GetCurrentMatch(playerIndex);
			}
		}
	}

	/// <summary>
	/// Called when a new player is instantiated, or when a flame is passed on
	/// to a new match. Finds the move component of the next match for the 
	/// player to control and assigns it to the proper input device. It also 
	/// sets up a new camera if the game is not in Coop mode.
	/// </summary>
	/// <param name="matchPlayerIndex">Player index of that match
	/// trying to pass on its camera.</param>
	private void GetCurrentMatch(int matchPlayerIndex)
	{
		if (playerIndex == matchPlayerIndex)
		{
			AssignMatchComponents();

			if (GameManager.GameMode != GameMode.Coop)
			{
				ParentCameraTransform();
				SetCameraTransform();
			}
		}
	}

	/// <summary>
	/// New player input classes are instantiated when the player presses a 
	/// button. Hence, the awake function is run every time a player joins. 
	/// This method helps to sort out audio listeners, as well as alert the 
	/// GameManager to how many players have joined so it can start the game
	/// on time.
	/// </summary>
	private void HandlePlayerJoin()
	{
		playerIndex = PlayerInput.playerIndex;
		GameManager.PlayersJoined++;
		playerJoinedGame?.Invoke();
		if (GameManager.PlayersJoined == 2)
		{
			if (AudioListener != null)
			{
				AudioListener.enabled = false;
			}
		}
	}

	/// <summary>
	/// Parents the player's camera transform to the next match so it will
	/// follow the new match.
	/// </summary>
	private void ParentCameraTransform()
	{
		PlayerCam.transform.SetParent(moveComponent.transform);
		burnComponent.CamTransform = PlayerCam.transform;
	}

	/// <summary>
	/// Sets position and rotation of the new camera upon gaining it as a 
	/// child object.
	/// </summary>
	private void SetCameraTransform()
	{
		PlayerCam.transform.localEulerAngles = new Vector3(0, -90, 0);
		if (GameManager.GameMode == GameMode.Versus)
		{
			PlayerCam.transform.localPosition = new Vector3(9, 0.5f, 0);
		}
		else
		{
			PlayerCam.transform.localPosition = new Vector3(6, 0.5f, 0);
		}
	}
}
