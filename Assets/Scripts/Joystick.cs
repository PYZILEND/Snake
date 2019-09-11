using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

public class Joystick : MonoBehaviour, IDragHandler
{
	/// <summary>
	/// Represents steer direction in range of [-1; 1]
	/// </summary>
	public Vector2 direction;
	
	//Represents joystick's knob
	private Transform joystick;
	
	void Awake () {
		joystick = transform.GetChild(0);
		direction = Vector2.up;
	}

	public void OnDrag(PointerEventData data)
	{
		joystick.position = data.position;
		joystick.localPosition = Vector2.ClampMagnitude(joystick.localPosition * 100, 50);
		direction = joystick.localPosition.normalized;
	}

	public void Initialize()
	{
		joystick.localPosition = new Vector2(0, 0);
		direction = Vector2.up;
	}
}
