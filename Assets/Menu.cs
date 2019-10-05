using UnityEngine;

public class Menu : MonoBehaviour
{
	public static bool IsPaused = false;
	public GameObject PauseMenuUI;

	public void Resume()
	{
		PauseMenuUI.SetActive(false);
		IsPaused = false;
	}

	public void Pause()
	{
		PauseMenuUI.SetActive(true);
		IsPaused = true;
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
