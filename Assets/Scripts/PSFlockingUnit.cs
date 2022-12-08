using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Unity3D C# Coding Guidelines: http://wiki.unity3d.com/index.php/Csharp_Coding_Guidelines

namespace PSFlocking 
{
	
	/*! Each Unit that is part of the flock holds this script. It is the base class for custom flocking subclasses. */
	public class PSFlockingUnit : MonoBehaviour 
	{
		
		//! Gets and Sets the GameObject holding a PSUnitManager Component. The setter only works, if the passed GameObject holds a PSUnitManager component.
		public GameObject Manager 
		{	
			get 
			{ 
				return manager;
			}

			set 
			{ 
				PSUnitManager unitManager = value.GetComponent<PSUnitManager>();
				if (unitManager != null) {
					manager = value;
				}
			}
		}
			
		[SerializeField]
		private GameObject manager;
		private Vector3 velocity;
		private Vector3 previousPosition = Vector3.zero;

		// For Developers only: change position of goal randomly
		private float timeUntilNextRandom = 2.0f;
		private float maximumTimeUntilNextRandom = 3.0f;
		private float strengthRandomizer;



		#region MonoBehaviour Subclassing

		/**
		 * @brief Called once from Unity. Do not call manually.
		 */
		protected void Start() 
		{
			// set initial values for velocity
			velocity = new Vector3(Random.Range(0.01f, 0.01f),0f, Random.Range(0.01f, 0.01f));
		}

		/**
		 * @brief Called periodically from Unity. Do not call manually.
		 */
		protected void Update() 
		{

			// early out if no manager is set, or the manager does not have a PSUnitManager script attached to it
			if (this.manager == null || this.manager.GetComponent<PSUnitManager>() == null) 
			{
				return;
			}



			Flock();

			// look to the front
			this.transform.LookAt(this.transform.position + (this.transform.position - this.previousPosition));
			this.previousPosition = this.transform.position;

			// update randomizer strength timer
			this.timeUntilNextRandom -= Time.deltaTime;
			if (this.timeUntilNextRandom < 0.0f) 
			{
				this.MakeNewRandom();
				this.timeUntilNextRandom = Random.Range(1.0f, this.maximumTimeUntilNextRandom);
			}
		}

		#endregion




		#region Flocking Behaviour

		/**
		 * @brief Calculates a vector to align the unit to other units surrounding it.
		 * This function gets all surrounding units that are within the alignmentDistance from PSUnitManager, and also within the viewingAngle from PSUnitManager. 
		 * It returns the average velocity of all those units, altered slightly by strengthRandomizer from PSUnitManager. 
		 * This function can be overridden in a subclass.
		 * @return Vector3 Alignment for the Unit, will be applied as a force on its rigidbody.
		 */ 
		protected virtual Vector3 Align() 
		{
			float alignmentDistance = manager.GetComponent<PSUnitManager>().alignmentDistance;

			Vector3 sum = Vector3.zero;
			int count = 0;
			foreach (GameObject other in manager.GetComponent<PSUnitManager>().units) 
			{
				if (other == this.gameObject) 
				{
					continue;
				}

				// check if other boid is within distance 
				float distance = Vector3.Distance (this.transform.position, other.transform.position);
				if (distance < alignmentDistance) 
				{

					// check if other boid is within viewing angle
					if (this.IsWithinViewingAngle(other)) 
					{
						sum += other.GetComponent<PSFlockingUnit>().velocity;
						count++;
					}
				}
			}

			if (count > 0) 
			{
				sum /= count;
				float strengthMultiplier = this.manager.GetComponent<PSUnitManager>().alignmentStrength + strengthRandomizer;
				strengthMultiplier = Mathf.Max(strengthMultiplier, 0.0f);
				strengthMultiplier = Mathf.Min(strengthMultiplier, 1.0f);
				Vector3 steer = sum * strengthMultiplier;
				return steer;
			}

			return Vector3.zero;
		}

		/**
		 * @brief Calculates a vector to move the unit closer to the flock center.
		 * This function gets all surrounding units that are within the cohesionDistance from PSUnitManager, and also within the viewingAngle from PSUnitManager. 
		 * It returns a vector pointing to the center of all those units.
		 * This function can be overridden in a subclass.
		 * @return Vector3 Cohesion for the Unit, will be applied as a force on its rigidbody.
		 */ 
		protected virtual Vector3 Cohesion() 
		{
			// get the maximum distance other units can be away to be still taken into account for cohesion
			float cohesionDistance = manager.GetComponent<PSUnitManager>().cohesionDistance;

			// prepare variables
			Vector3 sum = Vector3.zero;
			int count = 0;

			// get all boids
			foreach (GameObject other in manager.GetComponent<PSUnitManager>().units) 
			{

				// do not compare this boid to itself
				if (other == this.gameObject) 
				{
					continue;
				}

				// get distance from this boid to other boid
				float distance = Vector3.Distance(this.transform.position, other.transform.position);

				// compare if boid is within distance
				if (distance < cohesionDistance) 
				{
					// check if other boid is within viewing angle
					if (this.IsWithinViewingAngle(other)) 
					{
						sum += other.transform.position;
						count++;
					}
				}
			}

			if (count > 0) 
			{
				sum /= count;
				Vector3 vectorToMiddle = this.VectorToTarget(sum);
				float strengthMultiplier = manager.GetComponent<PSUnitManager>().cohesionStrength + strengthRandomizer;
				strengthMultiplier = Mathf.Max(strengthMultiplier, 0.0f);
				strengthMultiplier = Mathf.Min(strengthMultiplier, 1.0f);
				vectorToMiddle = vectorToMiddle * strengthMultiplier * 2.0f;
				return vectorToMiddle;
			}

			return Vector3.zero;
		}

		/**
		 * @brief Calculates a vector to move the unit awayf rom other units nearby.
		 * This function gets all surrounding units that are within the separationDistance from PSUnitManager, and also within the viewingAngle from PSUnitManager. 
		 * It returns a vector pointing away from units which are close by. The closer a unit is, the stronger it will point away from it (by the power of 3).
		 * This function can be overridden in a subclass.
		 * @return Vector3 Separation for the Unit, will be applied as a force on its rigidbody.
		 */ 
		protected virtual Vector3 Separation() 
		{
			Vector3 force = Vector3.zero;
			foreach (GameObject other in manager.GetComponent<PSUnitManager>().units) 
			{
				if (other == this.gameObject) 
				{
					continue;
				}

				// get the distance to the other object, and the total allowed distance for seperation to work
				float distance = Vector3.Distance(this.transform.position, other.transform.position);
				float separationDistance = this.manager.GetComponent<PSUnitManager>().separationDistance;

				// check if the other unit is within the separation-visibility-distance
				if (distance < separationDistance) 
				{

					// check if the boid is within viewing angle
					if (this.IsWithinViewingAngle(other)) 
					{

						// bring the force in a range of 0..1, depending on distance
						float separationForce = Mathf.Pow(1 - (distance / separationDistance), 3) * 10.0f; // makes it exponentially strong when they get really close together
						Vector3 direction = other.transform.position - this.transform.position;
						direction = direction * (-1);
						direction = direction.normalized;
						direction = direction * separationForce;
						force += direction;
					}
				}
			}

			float strengthMultiplier = this.manager.GetComponent<PSUnitManager>().separationStrength + this.strengthRandomizer;
			strengthMultiplier = Mathf.Max(strengthMultiplier, 0.0f);
			strengthMultiplier = Mathf.Min(strengthMultiplier, 1.0f);
			force = force * strengthMultiplier * 2.0f;
			force = force + force.normalized;

			return force;
		}


		/**
		 * @brief Calculates a vector to move the unit towards a GameObject specified as a goal.
		 * This function gets the goal variable from PSUnitManager and returns a Vector pointing towards it.
		 * If the goal GameObject is not set, the function will return a zero-vector.
		 * This function can be overridden in a subclass.
		 * @return Vector3 Vector towards the goal GameObject of PSUnitManager, or a zero-vector if that variable is not set.
		 */ 
		protected virtual Vector3 SeekGoal() {
			// check if there is a goal 
			if (manager.GetComponent<PSUnitManager>().seekGoal) 
			{
				// get the goal
				GameObject goalGO = manager.GetComponent<PSUnitManager>().goal;
				if (goalGO != null) 
				{
					return this.VectorToTarget(goalGO.transform.position);
				}
			}

			return Vector3.zero;
		}

		private void Flock() 
		{
			velocity = this.GetComponent<Rigidbody>().velocity;
			Vector3 currentForce = Vector3.zero;

			if (manager.GetComponent<PSUnitManager>().obedient) 
			{

				Vector3 ali = this.Align();
				Vector3 coh = this.Cohesion();
				Vector3 separation = this.Separation();
				Vector3 goal = this.SeekGoal();

				// add the differenct forces up and normalize
				currentForce = goal + ali + coh + separation;
				//currentForce = currentForce.normalized;
			}

			if (Random.Range(0, 50) <= 1) 
			{
				if (Random.Range (0, 50) < 1) 
				{
					currentForce = new Vector3(Random.Range (0.01f, 0.1f), Random.Range(0.01f, 0.1f));
				}
			}
				
			this.ApplyForce(currentForce);
		}

		#endregion




		#region Other Functions

		private Vector3 VectorToTarget(Vector3 target) 
		{
			return(target - this.transform.position);
		}

		private void ApplyForce(Vector3 force) 
		{
            // clip the force to maxforce (if bigger than max force)
			if (force.magnitude > manager.GetComponent<PSUnitManager>().maxForce) 
			{
				force = force.normalized;
				force *= manager.GetComponent<PSUnitManager>().maxForce;
			}

            // add the force
            this.GetComponent<Rigidbody>().AddForce(force);


            // now check again if the new force of the object is bigger than the maxVelocity. if so, then clip.
			if (this.GetComponent<Rigidbody>().velocity.magnitude > manager.GetComponent<PSUnitManager>().maxvelocity) 
			{
				this.GetComponent<Rigidbody>().velocity = this.GetComponent<Rigidbody>().velocity.normalized;
				this.GetComponent<Rigidbody>().velocity *= manager.GetComponent<PSUnitManager>().maxvelocity;
			}

            Debug.DrawRay(this.transform.position, force, Color.green, 0.1f);
		}

		private void MakeNewRandom() 
		{
			this.strengthRandomizer = Random.Range(0.0f, manager.GetComponent<PSUnitManager>().randomizerStrength) - (manager.GetComponent<PSUnitManager>().randomizerStrength / 2.0f);
		}

		private bool IsWithinViewingAngle(GameObject other) 
		{
			// check whether that boid is within the viewing angle
			float viewingAngle = manager.GetComponent<PSUnitManager>().viewingAngle;
			float angle = Vector3.Angle (this.transform.forward, this.VectorToTarget(other.transform.position).normalized);
			if (angle < (viewingAngle / 2.0f)) 
			{
				return true;
			} else 
			{
				return false;
			}
		}

		#endregion
	}
}