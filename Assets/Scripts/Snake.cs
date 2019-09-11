using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snake : MonoBehaviour
{
	/// <summary>
	/// Prefab for snake's body segment
	/// </summary>
	public Transform bodyPrefab;
	
	/// <summary>
	/// References to snake's segments
	/// </summary>
	private List<Transform> body;
	
	/// <summary>
	/// Snake's current speed
	/// </summary>
	public float speed;
	
	/// <summary>
	/// Snake's rotation speed
	/// </summary>
	public float rotationSpeed;
	

	void Awake () {
		body = new List<Transform>();
	}
	
	/// <summary>
	/// Moves snake in specified direction. If accelerated, snake's speed doubles
	/// </summary>
	/// <param name="direction"></param>
	/// <param name="accelerated"></param>
	public void Move(Vector2 direction, bool accelerated)
	{
		float step = rotationSpeed * Time.deltaTime;
		Quaternion target = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.y));
		transform.rotation = Quaternion.RotateTowards(transform.rotation, target, step);

		if (accelerated)
		{
			step = speed * 2;
		}
		else
		{
			step = speed;
		}
		transform.position += transform.forward * (step * Time.deltaTime);
		
		MovePart(body[0], transform);
		for (int i = 1; i < body.Count; i++)
		{
			MovePart(body[i], body[i-1]);
		}
	}

	/// <summary>
	/// Moves snake's segment towards other segment
	/// </summary>
	/// <param name="part">Segment to move</param>
	/// <param name="otherPart">Segment to move towards</param>
	void MovePart(Transform part, Transform otherPart)
	{
		float bodyDrag = Vector3.Distance(part.position, otherPart.position) - (part.localScale.x * 0.9f);
	
		part.transform.position = Vector3.MoveTowards(part.transform.position, otherPart.transform.position, bodyDrag);
		//part.transform.position = Vector3.Lerp(part.transform.position, otherPart.transform.position, bodyDrag);
	}
	
	/// <summary>
	/// Returns snake's body to initial size and places snake at initial position
	/// </summary>
	/// <param name="initialSize">Number of segments snake has by default</param>
	/// <param name="initialPosition">Initial position of the snake</param>
	public void Initialize(int initialSize, Vector3 initialPosition, int initialSpeed)
	{
		speed = initialSpeed;
		transform.position = initialPosition;
		transform.rotation = Quaternion.identity;
		foreach (var part in body)
		{
			Destroy(part.gameObject);
		}
		body.Clear();
		
		//Adding first part manually to avoid checking if it's first part in AddPart every time 
		body.Add(Instantiate(bodyPrefab, transform.position + Vector3.back, Quaternion.identity));
		
		for (int i = 1; i < initialSize; i++)
		{
			AddPart();
		}
	}

	/// <summary>
	/// Adds a new segment to snake
	/// </summary>
	public void AddPart()
	{
		body.Add(Instantiate(bodyPrefab, body[body.Count-1].position + Vector3.back, Quaternion.identity));
	}

	/// <summary>
	/// Returns true is snake has collided with it's tail
	/// </summary>
	public bool hasCollidedTail
	{
		get
		{
			//Ignores first body segment
			for(int i = 1; i < body.Count; i++)
			{
				if (Vector3.Distance(transform.position, body[i].position) < transform.localScale.x) return true;
			}

			return false;
		}
	}
}
