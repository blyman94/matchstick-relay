using UnityEngine;

/// <summary>
/// Behaviour of the moving platforms in the game. Uses a set of points to
/// define a path of travel and continously cycles through this path.
/// </summary>
public class MovingPlatform : MonoBehaviour
{
    [Tooltip("Points among which the platform will travel sequentially.")]
    public Transform[] Points;

    [Header("Physics and Movement")]

    [Tooltip("Feedback amount to dictate accuracy of stop behavior.")]
    public float Gain = 5.0f;

    [Tooltip("Maximum force to apply to get platform to max velocity.")]
    public float MaxForce = 20.0f;

    [Tooltip("Maximum velocity this platform can obtain.")]
    public float MaxVelocity = 3.0f;

    [Tooltip("Rigidbody used for platform motion.")]
	public Rigidbody Rb;

    [Tooltip("Factor to convert distance delta to velocity.")]
    public float ToVelocity = 2.5f;

    /// <summary>
    /// Current distance of the platform from its position to the target
    /// position. Used to determine when the platform should switch target
    /// positions.
    /// </summary>
    private Vector3 currentDistance;

    /// <summary>
    /// Current index of the Points array to which this platform is traveling.
    /// </summary>
    private int currentPoint = 0;

    /// <summary>
    /// Variable to cache the transform of rigidbody being moved. This is used
    /// because the moving platform prefab has an empty parent object that 
    /// controls the logic of the moving platform, and the platform itself is a
    /// child object of the controlling parent.
    /// </summary>
    private Transform rbTransform;

    /// <summary>
    /// Target position of the platform.
    /// </summary>
    private Vector3 targetPosition;

    private void FixedUpdate()
    {
        ApplyMoveForce();
    }

    private void Start()
    {
        targetPosition = Points[currentPoint].position;
        rbTransform = Rb.transform;
    }
    private void Update()
    {
        ChangeTargetPosition();
    }

    /// <summary>
    /// Applies force to the object to move it in 3D. Adds more force depending
    /// on how far the object is from the target position. Allows the object to
    /// stop at the target position using a feedback  control loop.
    /// </summary>
    private void ApplyMoveForce()
    {
        currentDistance = targetPosition - rbTransform.position;
        Vector3 targetVelocity =
            Vector3.ClampMagnitude(ToVelocity * currentDistance, MaxVelocity);
        Vector3 error = targetVelocity - Rb.velocity;
        Vector3 force = Vector3.ClampMagnitude(Gain * error, MaxForce);
        Rb.AddForce(force, ForceMode.Acceleration);
    }

    /// <summary>
    /// Changes the target position of the platform when it has reached its
    /// current destination.
    /// </summary>
    private void ChangeTargetPosition()
    {
        if (currentDistance.magnitude <= 0.05f)
        {
            currentPoint++;
            if (currentPoint > Points.Length - 1)
            {
                currentPoint = 0;
            }
            targetPosition = Points[currentPoint].position;
            currentDistance = targetPosition - rbTransform.position;
        }
    }
}
