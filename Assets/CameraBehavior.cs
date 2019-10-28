﻿using System.Collections.Generic;
using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
	public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
	public RotationAxes axes = RotationAxes.MouseXAndY;
	public static float sensitivityX = 4F;
	public static float sensitivityY = 4F;

	public float minimumX = -360F;
	public float maximumX = 360F;

	public float minimumY = -60F;
	public float maximumY = 60F;

	float rotationX = 0F;
	float rotationY = 0F;

	private Menu menuComponent;

	private List<float> rotArrayX = new List<float>();
	float rotAverageX = 0F;

	private List<float> rotArrayY = new List<float>();
	float rotAverageY = 0F;

	public float frameCounter = 20;

	Quaternion originalRotation;
	/*
    Below is Writen by Windexglow 11-13-10 and modified by evangipson on 10/27/2019.
	
	Use it, edit it, steal it I don't care.  
    Converted to C# 27-02-13 - no credit wanted.
    Simple flycam I made, since I couldn't find any others made public.  
    Made simple to use (drag and drop, done) for regular keyboard layout  
    wasd : basic movement
    shift : Makes camera accelerate */

	float mainSpeed = 100.0f; //regular speed
	float shiftAdd = 500.0f; //multiplied by how long shift is held.  Basically running
	float maxShift = 1000.0f; //Maximum speed when holdin gshift
	private float totalRun = 1.0f;

	private Vector3 GetBaseInput()
	{ //returns the basic values, if it's 0 than it's not active.
		Vector3 p_Velocity = new Vector3();
		if (Input.GetKey(KeyCode.W))
		{
			p_Velocity += new Vector3(0, 0, 1);
		}
		if (Input.GetKey(KeyCode.S))
		{
			p_Velocity += new Vector3(0, 0, -1);
		}
		if (Input.GetKey(KeyCode.A))
		{
			p_Velocity += new Vector3(-1, 0, 0);
		}
		if (Input.GetKey(KeyCode.D))
		{
			p_Velocity += new Vector3(1, 0, 0);
		}
		return p_Velocity;
	}

	void Update()
	{
		if (menuComponent.isPaused || !menuComponent.gameStarted)
		{
			return;
		}
		if (axes == RotationAxes.MouseXAndY)
		{
			rotAverageY = 0f;
			rotAverageX = 0f;

			rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
			rotationX += Input.GetAxis("Mouse X") * sensitivityX;

			rotArrayY.Add(rotationY);
			rotArrayX.Add(rotationX);

			if (rotArrayY.Count >= frameCounter)
			{
				rotArrayY.RemoveAt(0);
			}
			if (rotArrayX.Count >= frameCounter)
			{
				rotArrayX.RemoveAt(0);
			}

			for (int j = 0; j < rotArrayY.Count; j++)
			{
				rotAverageY += rotArrayY[j];
			}
			for (int i = 0; i < rotArrayX.Count; i++)
			{
				rotAverageX += rotArrayX[i];
			}

			rotAverageY /= rotArrayY.Count;
			rotAverageX /= rotArrayX.Count;

			rotAverageY = ClampAngle(rotAverageY, minimumY, maximumY);
			rotAverageX = ClampAngle(rotAverageX, minimumX, maximumX);

			Quaternion yQuaternion = Quaternion.AngleAxis(rotAverageY, Vector3.left);
			Quaternion xQuaternion = Quaternion.AngleAxis(rotAverageX, Vector3.up);

			transform.localRotation = originalRotation * xQuaternion * yQuaternion;
		}
		else if (axes == RotationAxes.MouseX)
		{
			rotAverageX = 0f;

			rotationX += Input.GetAxis("Mouse X") * sensitivityX;

			rotArrayX.Add(rotationX);

			if (rotArrayX.Count >= frameCounter)
			{
				rotArrayX.RemoveAt(0);
			}
			for (int i = 0; i < rotArrayX.Count; i++)
			{
				rotAverageX += rotArrayX[i];
			}
			rotAverageX /= rotArrayX.Count;

			rotAverageX = ClampAngle(rotAverageX, minimumX, maximumX);

			Quaternion xQuaternion = Quaternion.AngleAxis(rotAverageX, Vector3.up);
			transform.localRotation = originalRotation * xQuaternion;
		}
		else
		{
			rotAverageY = 0f;

			rotationY += Input.GetAxis("Mouse Y") * sensitivityY;

			rotArrayY.Add(rotationY);

			if (rotArrayY.Count >= frameCounter)
			{
				rotArrayY.RemoveAt(0);
			}
			for (int j = 0; j < rotArrayY.Count; j++)
			{
				rotAverageY += rotArrayY[j];
			}
			rotAverageY /= rotArrayY.Count;

			rotAverageY = ClampAngle(rotAverageY, minimumY, maximumY);

			Quaternion yQuaternion = Quaternion.AngleAxis(rotAverageY, Vector3.left);
			transform.localRotation = originalRotation * yQuaternion;
		}
		//Keyboard commands
		Vector3 p = GetBaseInput();
		if (Input.GetKey(KeyCode.LeftShift))
		{
			totalRun += Time.deltaTime;
			p = p * totalRun * shiftAdd;
			p.x = Mathf.Clamp(p.x, -maxShift, maxShift);
			p.y = Mathf.Clamp(p.y, -maxShift, maxShift);
			p.z = Mathf.Clamp(p.z, -maxShift, maxShift);
		}
		else
		{
			totalRun = Mathf.Clamp(totalRun * 0.5f, 1f, 1000f);
			p = p * mainSpeed;
		}
		p = p * Time.deltaTime;
		Vector3 newPosition = transform.position;
		transform.Translate(p);
	}

	void Start()
	{
		menuComponent = GameObject.Find("Menu").GetComponent<Menu>();
		Rigidbody rb = GetComponent<Rigidbody>();
		if (rb)
			rb.freezeRotation = true;
		originalRotation = transform.localRotation;
	}

	public static float ClampAngle(float angle, float min, float max)
	{
		angle = angle % 360;
		if ((angle >= -360F) && (angle <= 360F))
		{
			if (angle < -360F)
			{
				angle += 360F;
			}
			if (angle > 360F)
			{
				angle -= 360F;
			}
		}
		return Mathf.Clamp(angle, min, max);
	}
}