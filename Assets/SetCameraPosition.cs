using UnityEngine;

public class SetCameraPosition : MonoBehaviour
{
	public static void CenterCameraOnJartboard()
	{
		Camera camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
		camera.backgroundColor = Utils.GetRandomArrayItem(Colors.PossibleColorPalettes[Jart.ColorPaletteIndex]);
		camera.clearFlags = CameraClearFlags.SolidColor;
		camera.transform.position = new Vector3(0f, 0f, 0f);
	}

	// Start is called before the first frame update
	void Start()
	{
		CenterCameraOnJartboard();
	}
}
