using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
	public static bool IsPaused = false;
	public GameObject PauseMenuUI;
	public Slider CameraSensitivitySlider;

	public void Resume()
	{
		CameraBehavior.camSens = CameraSensitivitySlider.value;
		PauseMenuUI.SetActive(false);
		IsPaused = false;
	}

	public void Pause()
	{
		PauseMenuUI.SetActive(true);
		IsPaused = true;
	}

	public void AdjustCameraSensitivity(float sliderValue)
	{
		CameraBehavior.camSens = CameraSensitivitySlider.value;
	}

	public void NewJart()
	{
		Resume();
		Jart.NewJart();
		SetCameraPosition.CenterCameraOnJartboard();
	}

	public void Quit()
	{
		// save any game data here
		#if UNITY_EDITOR
			// Application.Quit() does not work in the editor so
			// UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
			UnityEditor.EditorApplication.isPlaying = false;
		#else
			Application.Quit();
		#endif
	}

	void Update()
	{
		if(Input.GetKeyDown(KeyCode.Escape))
		{
			if(!IsPaused)
			{
				Pause();
			}
			else
			{
				Resume();
			}
		}
	}
}
