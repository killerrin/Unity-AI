using UnityEngine;
using System.Collections;

[RequireComponent (typeof(CharacterController))]
public class Boid : MonoBehaviour {
	public BoidManager manager { get; set; }

	public CharacterController characterController;

	public float SightRange = 100.0f;
	public float distanceFromOtherBoids;

	public float Speed;
	public float RotateSpeed;
	public float VelocityLimit;
	public Vector3 Velocity { get; set; }

	// Use this for initialization
	void Start () {
		Velocity = Vector3.zero;

		characterController = GetComponent<CharacterController>();
	}
	
	// Update is called once per frame
	public void BoidUpdate () {
        //Velocity = new Vector3(Velocity.x, Velocity.y, 0.0f);

		characterController.Move(Velocity * Time.deltaTime);
        if (Velocity != Vector3.zero) {
            Quaternion rotation = Quaternion.RotateTowards(transform.rotation,
                                                           Quaternion.LookRotation(new Vector3(Velocity.x, Velocity.y, Velocity.z)),
                                                           Time.deltaTime * RotateSpeed);
            transform.rotation = rotation;
        }
	}

	public void ApplyRules(Vector3 combinedRules)
	{
		Velocity = Velocity + combinedRules;
		//b.Velocity *= (Time.deltaTime * b.Speed);

		// Limit the velocity to the boids
		LimitVelocity();
	}

	public void LimitVelocity()
	{
		if (Velocity.magnitude > VelocityLimit)
		{
			Velocity = (Velocity / Velocity.magnitude) * VelocityLimit;
		}
	}
}
