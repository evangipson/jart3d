using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetCameraPosition : MonoBehaviour
{
	private void centerCameraOnJartboard()
	{
		Camera camera = GetComponent<Camera>();
		camera.transform.position = new Vector3(0.5f, 0.5f, -1);
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
