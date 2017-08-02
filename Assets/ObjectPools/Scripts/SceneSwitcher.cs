using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
	public void SwitchScene()
	{
		int nextScene = (SceneManager.GetActiveScene().buildIndex + 1)
			% SceneManager.sceneCountInBuildSettings;
		SceneManager.LoadScene(nextScene);
	}
}
