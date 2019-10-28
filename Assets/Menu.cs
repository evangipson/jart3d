using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
	public bool isPaused = false;
	public bool gameStarted = false;
	public GameObject MainMenuUI;
	public GameObject PauseMenuUI;
	public Slider CameraSensitivitySlider;
	private List<Oscillator> oscList = new List<Oscillator>();

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

	public void ViewJart()
	{
		Jart.NewJart();
		gameStarted = true;
		MainMenuUI.SetActive(false);
		Cursor.visible = false;
		Resume();
		SetCameraPosition.CenterCameraOnJartboard();
	}

	public void CreateSandbox()
	{
		Jart.clearJart();
		gameStarted = true;
		MainMenuUI.SetActive(false);
		Cursor.visible = false;
		Resume();
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

	private void Start()
	{
		gameStarted = false;
		MainMenuUI.SetActive(true);
		// note: this main menu music will be stopped by
		// the creation of a new jart, because when an old
		// jart gets cleaned up, so do all oscillators.
		Jart.StartMainMenuMusic();
	}

	void Update()
	{
		// esc key to show pause menu, if we've started the game
		if(gameStarted && Input.GetKeyDown(KeyCode.Escape))
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
