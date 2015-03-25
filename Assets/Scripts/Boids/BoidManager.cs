using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoidManager : MonoBehaviour {
	//public List<Boid> boidsl {get;set;}
	public KDTree<float, Boid> boids { get; set; }
	public GameObject boidPrefab;

	public bool randomizePosition = false;
	public int numberOfBoids = 30;
	public float distanceBetweenBoids = 10.0f;
	public float chasePlayerDivisor = 850.0f;

    public static System.Random random = new System.Random();

	public int NumOfBoids
	{
		get { return numberOfBoids; }
		set
		{
			if (value >= 0) { numberOfBoids = value; }
		}
	}

	public Vector3 boundries;
	public Vector3 outOfBoundryPushback;

	// Use this for initialization
	void Start () {        
        //boidsl = new List<Boid> { };
        Time.timeScale = 1.5f;
        
        boids = new KDTree<float, Boid>();

		SpawnBoids(2);
	}
	
	// Update is called once per frame
	void Update () {		
		if (numberOfBoids == 0 && boids.Count == 0) Destroy(gameObject);
		else if (boids.Count < numberOfBoids) { SpawnBoids(1); }
		else if (boids.Count > numberOfBoids) { KillBoids(1); }

		if (boids.Count <= 1) return;

		foreach (Boid b in boids.Root)
		{
			// Create a list which the rules will go into when being applied
			List<Vector3> rules = new List<Vector3> { };

			// Do each rule that the boids must follow
			rules.Add(Rule1(b));
			rules.Add(Rule2(b));
			rules.Add(Rule3(b));
			//rules.Add(Rule4(b));
			rules.Add(BoundPosition(b));

			// Apply the rules to the boid
			b.ApplyRules(CombineRules(rules));

			// Finally, call the update method on the Boids to get them to move
			b.BoidUpdate();
		}

		SortTree();
	}


	private float sortTimer = 0.0f;
	private float maxTimeBeforeSortInSeconds = 0.0f; // 2.5f
	/// <summary>
	/// To save processing, tree is set up to refresh every 2 seconds rather than every frame. This can be done as collisions 
	/// are done on a seperate stage in unity
	/// </summary>
	void SortTree()
	{
		if (sortTimer >= maxTimeBeforeSortInSeconds)
		{
			sortTimer = 0.0f;

			// Update the key
			foreach (TreeNode<float, Boid> b in boids)
			{
				b.Key = Vector3.Distance(transform.position, b.Value.transform.position);
			}

			// Sort the Tree
			boids = boids.Sort();
		}
		else { sortTimer += Time.deltaTime; }
	}


	public void SpawnBoids(int num)
	{
		Vector3 spawnPos;
		if (randomizePosition)
		{
			spawnPos = new Vector3((float)random.Next((int)-boundries.x, (int)boundries.x),
								   (float)random.Next((int)-boundries.y, (int)boundries.y),
								   (float)random.Next((int)-boundries.z, (int)boundries.z));
		}
		else { spawnPos = transform.position; }

		if (spawnPos.x > -0.1f && spawnPos.x < 0.1f)
			spawnPos.x = 0.45f;
		if (spawnPos.y > -0.1f && spawnPos.y < 0.1f)
			spawnPos.y = 0.64f;
		if (spawnPos.z > -0.1f && spawnPos.z < 0.1f)
			spawnPos.z = 0.72f;

		for (int i = 0; i < num; i++)
		{
			GameObject boid = Instantiate(boidPrefab, spawnPos, transform.rotation) as GameObject;
			
			// Set the parent
			//boid.transform.parent = transform;
			
			// Set the manager to be this
			boid.GetComponent<Boid>().manager = this;

			// Lastly, Add the Boid to the list to be tracked
			//boids.Add(boid.GetComponent<Boid>());
			float distanceFromCenter = Vector3.Distance(transform.position, boid.transform.position);
			boids.Add(distanceFromCenter, boid.GetComponent<Boid>());

			Debug.Log("Spawning Boids: " + gameObject.name + ": Number of Boids: " + boids.Count);
		}
	}
	public void KillBoids(int num)
	{
		if (boids.Count == 0 ||
			numberOfBoids == 0) return;

		//Destroy(boids[boids.Count - 1].gameObject);
		//boids.RemoveAt(boids.Count - 1);
		Destroy(boids.Root.Value.gameObject);
		boids.Remove(boids.Root.Key);

		Debug.Log("Killing Boids: " + gameObject.name + ": Number of Boids: " + boids.Count);
	}
	public void KillBoid(Boid _boid)
	{
		//boids.Remove(_boid);
		boids.RemoveValue(_boid);

		//Destroy(_boid.transform.parent.gameObject);
		Destroy(_boid.gameObject);

		--numberOfBoids;

		Debug.Log("Killing Boids: " + _boid.gameObject.name + ": Number of Boids: " + boids.Count);
	}

	Vector3 CombineRules (List<Vector3> rules)
	{
		Vector3 combinedRules = Vector3.zero;
		foreach (Vector3 rule in rules)
		{
			combinedRules = combinedRules + rule;
		}

		//Debug.Log(combinedRules);
		return combinedRules;
	}

	/// <summary>
	/// Boids try to fly towards the centre of mass of neighbouring boids. 
	/// </summary>
	/// <param name="_boid"></param>
	/// <returns></returns>
	Vector3 Rule1(Boid _boid)
	{
		Vector3 pcj = Vector3.zero;

		foreach (Boid b in boids.Root)
		{
			if (b != _boid)
			{
				pcj = pcj + b.transform.position;
			}
		}

		pcj = pcj / (boids.Count - 1);
		return (pcj - _boid.transform.position) / 1000;
	}

	/// <summary>
	/// Boids try to keep a small distance away from other objects (including other boids). 
	/// </summary>
	/// <param name="_boid"></param>
	/// <returns></returns>
	Vector3 Rule2(Boid _boid)
	{
		Vector3 c = Vector3.zero;

		foreach (Boid b in boids.Root)
		{
			if (b != _boid)
			{
				if ((b.transform.position - _boid.transform.position).magnitude < (distanceBetweenBoids + b.distanceFromOtherBoids + _boid.distanceFromOtherBoids))
				{
					c = c - (b.transform.position - _boid.transform.position);
				}
			}
		}

		// Apply a tendency aginst the player at small distances to get them to attempt to avoid Crashing into the player
		//if ((Player.me.transform.position - _boid.transform.position).magnitude < ((distanceBetweenBoids + _boid.distanceFromOtherBoids) * 1.1f))
		{
			//c = c - ((Player.me.transform.position - _boid.transform.position));
		}

		return c;
	}

	/// <summary>
	/// Boids try to match velocity with near boids. 
	/// </summary>
	/// <param name="_boid"></param>
	/// <returns></returns>
	Vector3 Rule3(Boid _boid)
	{
		Vector3 pvj = Vector3.zero;

		foreach (Boid b in boids.Root)
		{
			if (b != _boid)
			{
				pvj = pvj + b.Velocity;
			}
		}

		pvj = pvj / (boids.Count - 1);
		return (pvj - _boid.Velocity) / 8;
	}

	/// <summary>
	/// Applies a tendancy towards the payer
	/// </summary>
	/// <returns></returns>
	Vector3 Rule4(Boid _boid)
	{
		Vector3 chasePlayer = Vector3.zero;

		// If the player is within range of the boids sight, tend towards them
		//if ((Player.me.transform.position - _boid.transform.position).magnitude < _boid.SightRange)
		//{
		//	chasePlayer = Player.me.transform.position - _boid.transform.position;
		//}

		return chasePlayer / chasePlayerDivisor;
	}

	Vector3 BoundPosition(Boid b)
	{
		float xMin = -boundries.x + transform.position.x;
		float xMax = boundries.x + transform.position.x;
		float yMin = -boundries.y + transform.position.y;
		float yMax = boundries.y + transform.position.y;
		float zMin = -boundries.z + transform.position.z;
		float zMax = boundries.z + transform.position.z;

		Vector3 v = Vector3.zero;

		if (b.transform.position.x < xMin)
			v.x = outOfBoundryPushback.x;
		else if (b.transform.position.x > xMax)
			v.x = -outOfBoundryPushback.x;

		if (b.transform.position.y < yMin)
			v.y = outOfBoundryPushback.y;
		else if (b.transform.position.y > yMax)
			v.y = -outOfBoundryPushback.y;

		if (b.transform.position.z < zMin)
			v.z = outOfBoundryPushback.z;
		else if (b.transform.position.z > zMax)
			v.z = -outOfBoundryPushback.z;

		return v;
	}
}
