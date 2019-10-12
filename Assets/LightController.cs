using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightController : MonoBehaviour
{
	Light light;
    // Start is called before the first frame update
    void Start()
    {
		light = gameObject.GetComponent<Light>(); // grab the light from the game
    }

    // Update is called once per frame
    void Update()
    {
		//light.transform.Rotate(0.01f, 0, 0);
    }
}
