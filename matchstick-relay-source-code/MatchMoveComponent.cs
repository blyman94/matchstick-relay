using System.Collections;
using UnityEngine;

/// <summary>
/// Component that handles the movement of the matches via Unity 3D physics and
/// a rigid body. Allows for jumping, and moving horizontally along the Z axis
/// only while in the air.
/// </summary>
public class MatchMoveComponent : MonoBehaviour
{
	[Tooltip("Index of the player. This game only supports two players, so" +
			"this value should only ever be 0 or 1.")]
	public int playerIndex;

	[Tooltip("Audio component for the match.")]
	public MatchAudioComponent MatchAudioComponent;

	[Header("Physics")]
	[Tooltip("Used in Rigidbody.AddForce method to simulate a jump.")]
	public float JumpForce;

	[Tooltip("Used in Rigidbody.AddForce method to move along Z axis.")]
	public float MoveForce;

	[Tooltip("Rigidbody of the match, used to implement all motion.")]
	public Rigidbody Rb;

	[Header("Ground Check Parameters")]
	[Tooltip("Transform who's position is used to check grounded state.")]
	public Transform GroundCheck;

	[Tooltip("Select all layers to be considered ground.")]
	public LayerMask GroundLayer;

	[Tooltip("A coroutine is used to turn off the ground check momentarily" +
		"on jump. This integer dictates how many frames that ground check " +
		"is disabled for.")]
	public int groundlessFrames = 10;
	
	/// <summary>
	/// Downward force applied to match to achieve variable jump height.
	/// </summary>
	private const float endJumpForce = 2.0f;

	/// <summary>
	/// Radius of the ground check circle that determines whether the match is
	/// grounded.
	/// </summary>
	private const float groundCheckRadius = 0.045f;

	/// <summary>
	/// Accounts for slight downward y velocity on a rigidbody when resting.
	/// </summary>
	private const float yVelocityDeadzone = -0.4f;

	/// <summary>
	/// Grounded state boolean. Component can only jump when grounded.
	/// </summary>
	private bool grounded = true;

	/// <summary>
	/// Stores movement input so that the player can "queue" movement before
	/// jumping.
	/// </summary>
	private Vector3 moveIntent;

	/// <summary>
	/// Checks if the ground check should be disabled, is set to false
	/// immediately after the match jumps.
	/// </summary>
	private bool shouldCheckGround = true;

	public void OnDrawGizmos()
	{
		// used to help visualize ground check in editor.
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(GroundCheck.position, groundCheckRadius);
	}

	private void Update()
	{
		CheckGrounded();
	}

	/// <summary>
	/// Routine to disable ground check after the match jumps. It's a messy
	/// implementation and would be patched in the future.
	/// </summary>
	public IEnumerator DisableGroundCheckRoutine()
	{
		shouldCheckGround = false;
		for (int i = 0; i < groundlessFrames; i++)
		{
			yield return null;
		}
		shouldCheckGround = true;
	}

	/// <summary>
	/// Applies a downward force on the match, used to achieve variable jump
	/// height.
	/// </summary>
	public void EndJump()
	{
		if (Rb.velocity.y > 0)
		{
			Rb.AddForce(Vector3.down * endJumpForce, ForceMode.Impulse);
		}
	}

	/// <summary>
	/// Uses Rigidbody.AddForce to make the match jump based on the JumpForce
	/// parameter.
	/// </summary>
	public void Jump()
	{
		if (grounded)
		{
			StartCoroutine(DisableGroundCheckRoutine());
			Rb.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);
			Rb.AddForce(moveIntent * MoveForce, ForceMode.Impulse);
			grounded = false;
			MatchAudioComponent.PlayMatchOneShotAudio("jump");
		}
	}

	/// <summary>
	/// Updates moveIntent with the input. Then, if not in the grounded state,
	/// uses Rigidbody.AddForce to move the match in the given direction.
	/// </summary>
	/// <param name="movementInput">Movement input from controller.</param>
	public void Move(Vector3 movementInput)
	{
		moveIntent = movementInput;
		// Player cannot move while on the ground!
		if (grounded)
		{
			return;
		}
		else
		{
			Rb.AddForce(moveIntent * MoveForce, ForceMode.Impulse);
			MatchAudioComponent.PlayMatchOneShotAudio("move");
		}
	}

	/// <summary>
	/// Determines if the match is grounded using Physics.CheckSphere, and the 
	/// GroundCheck's position, groundCheckRadius, and GroundLayer as
	/// parameters.
	/// </summary>
	private void CheckGrounded()
	{
		if ((!grounded || Rb.velocity.y < yVelocityDeadzone) && shouldCheckGround)
		{
			grounded = Physics.CheckSphere(GroundCheck.position,
				groundCheckRadius, GroundLayer);
			shouldCheckGround = true;
		}
	}
}
