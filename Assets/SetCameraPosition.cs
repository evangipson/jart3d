using UnityEngine;

public class SetCameraPosition : MonoBehaviour
{
	private void centerCameraOnJartboard()
	{
		Camera camera = GetComponent<Camera>();
		//camera.backgroundColor = Color.black;
		//camera.clearFlags = CameraClearFlags.SolidColor;
		camera.transform.position = new Vector3(0f, 0f, 0f);
	}

	// Start is called before the first frame update
	void Start()
	{
		Cursor.visible = false;
		centerCameraOnJartboard();
	}
}
