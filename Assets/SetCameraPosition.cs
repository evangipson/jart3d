using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetCameraPosition : MonoBehaviour
{
	private void centerCameraOnJartboard()
	{
		Camera camera = GetComponent<Camera>();
		camera.backgroundColor = Color.white;
		camera.clearFlags = CameraClearFlags.SolidColor;
		camera.transform.position = new Vector3(1.15f, 0.5f, -2.9f);
	}

	// Start is called before the first frame update
	void Start()
	{
		centerCameraOnJartboard();
	}

    // Update is called once per frame
    void Update()
    {
        
    }
}
