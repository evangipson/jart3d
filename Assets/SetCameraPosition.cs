using UnityEngine;

public class SetCameraPosition : MonoBehaviour
{
	public static void CenterCameraOnJartboard()
	{
		Camera camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
		camera.backgroundColor = Utils.GetRandomArrayItem(Colors.PossibleColorPalettes[Jart.ColorPaletteIndex]);
		camera.clearFlags = CameraClearFlags.SolidColor;
		camera.transform.position = new Vector3(Constants.JartCubeSize * 0.5f, Constants.JartCubeSize * 0.5f, Constants.JartCubeSize * 0.5f);
	}

	// Start is called before the first frame update
	void Start()
	{
		CenterCameraOnJartboard();
	}
}
