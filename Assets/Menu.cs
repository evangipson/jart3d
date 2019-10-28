using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
	public bool isPaused = false;
	public GameObject MainMenuUI;
	public GameObject PauseMenuUI;
	public Slider CameraSensitivitySlider;

	public void Resume()
	{
		CameraBehavior.sensitivityX = CameraSensitivitySlider.value;
		CameraBehavior.sensitivityY = CameraSensitivitySlider.value;
		PauseMenuUI.SetActive(false);
		isPaused = false;
		Cursor.visible = false;
	}

	public void Pause()
	{
		PauseMenuUI.SetActive(true);
		isPaused = true;
		Cursor.visible = true;
	}

	public void AdjustCameraSensitivity(float sliderValue)
	{
		CameraBehavior.sensitivityX = CameraSensitivitySlider.value;
		CameraBehavior.sensitivityY = CameraSensitivitySlider.value;
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

	// will show a "submenu" for the current menu
	// by taking up the full screen in front of it
	// and having a back button to get back to the
	// passed in menu.
	private void showSubMenu(GameObject menu)
	{

	}

	private void showMainMenu()
	{
		 
	}

	private void Start()
	{
		Cursor.visible = false;
	}

	void Update()
	{
		if(Input.GetKeyDown(KeyCode.Escape))
		{
			Jart.ToggleSongQuiet();
			if (!isPaused)
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
