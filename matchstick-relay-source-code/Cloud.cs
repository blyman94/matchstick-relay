using UnityEngine;

/// <summary>
/// Randomizes the size of the clouds and moves them slowly in the negative z
/// worldspace direction for a little polish.
/// </summary>
public class Cloud : MonoBehaviour
{
	[Tooltip("Array of puff transforms to randomize the size of")]
	public Transform[] puffArray;

	private void Start()
	{
		RandomizeCloudSize();
	}

	private void Update()
	{
		SlowlyMoveCloud();
	}

	/// <summary>
	/// Randomly assings new sizes to each piece of cloud in the puffArray.
	/// </summary>
	private void RandomizeCloudSize()
	{
		for (int i = 0; i < puffArray.Length; i++)
		{
			puffArray[i].localScale = new Vector3(Random.Range(0.7f, 1.3f),
				Random.Range(0.7f, 1.3f), Random.Range(0.7f, 1.3f));
		}
	}

	/// <summary>
	/// Slowly moves the cloud along the world Z axis.
	/// </summary>
	private void SlowlyMoveCloud()
	{
		transform.Translate(new Vector3(0, 0, -0.05f * Time.deltaTime),
					Space.World);
	}
}
