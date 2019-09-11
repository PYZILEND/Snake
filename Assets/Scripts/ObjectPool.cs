using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{

	/// <summary>
	/// Prefab of an object, determines what objects are stored in pool
	/// </summary>
	public Transform objectPrefab;
	
	//Queue used to store pooled object
	private Queue<Transform> pool;
	
	void Awake () {
		pool = new Queue<Transform>();
		for (int i = 0; i < 5; i++)
		{
			Transform obj = Instantiate(objectPrefab, transform);
			obj.gameObject.SetActive(false);
			pool.Enqueue(obj);
		}
	}

	/// <summary>
	/// Takes object from pool and activates it
	/// </summary>
	/// <returns></returns>
	public Transform GetObjet()
	{
		if (pool.Count > 0)
		{
			Transform outObject = pool.Dequeue();
			outObject.gameObject.SetActive(true);
			return outObject;
		}

		return Instantiate(objectPrefab, transform);
	}

	/// <summary>
	/// Returns object to pool and deactivates it
	/// </summary>
	/// <param name="returnObject"></param>
	public void ReturnObject(Transform returnObject)
	{
		returnObject.gameObject.SetActive(false);
		pool.Enqueue(returnObject);
	}
}
