using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{


	public Text highScore;
	// Use this for initialization
	void Start () {
		if (PlayerPrefs.HasKey("HighScore"))
		{
			highScore.text = "High score: " + PlayerPrefs.GetInt("HighScore");
		}
		else
		{
			highScore.text = "High score: 0";
		}
	}

	public void StartButtonClick()
	{
		Time.timeScale = 1;
		SceneManager.LoadScene("Game", LoadSceneMode.Single);
	}

	public void DeleteHighScoreButtonClick()
	{
		PlayerPrefs.SetInt("HighScore", 0);
		PlayerPrefs.Save();
		highScore.text = "High score: 0";
	}

	public void ExitButtonClick()
	{
		Application.Quit();
	}
}
