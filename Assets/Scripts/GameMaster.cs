using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameMaster : MonoBehaviour
{
	
	public int intialSnakeSize, initialSnakeSpeed;
	public int gameFieldWidth, gameFieldHeight;
	
	public Snake snake;
	public Joystick joystick;
	public Text scoreText;
	public Text highScoreText;
	public Canvas pauseCanvas;
	public Button resumeButton;
	public Camera gameCamera;
	
	public List<Transform> obstacles;
	public List<Transform> food;
	
	public ObjectPool obstaclesPool;
	public ObjectPool foodPool;
	
	private bool acceleration;
	private int score;
	private int highScore;
	private bool newHighScore;

	private bool isSnakeOffscreen
	{
		get {
			if (snake.transform.position.x > gameFieldWidth - obstaclesPool.objectPrefab.localScale.x ||
			    snake.transform.position.x < obstaclesPool.objectPrefab.localScale.x) return true;
			if (snake.transform.position.z > gameFieldHeight - obstaclesPool.objectPrefab.localScale.z ||
			    snake.transform.position.z < obstaclesPool.objectPrefab.localScale.z) return true;
			return false; 
		}
	}
	
	// Use this for initialization
	void Start ()
	{
		if (PlayerPrefs.HasKey("HighScore"))
		{
			highScore = PlayerPrefs.GetInt("HighScore");
		}
		else
		{
			highScore = 0;
		}

		newHighScore = false;
		
		obstacles = new List<Transform>();
		food = new List<Transform>();
		
		Transform wall = Instantiate(obstaclesPool.objectPrefab, new Vector3(0, 0, gameFieldHeight / 2f), Quaternion.identity);
		wall.localScale = new Vector3(1, 1, gameFieldHeight + 1);
		wall = Instantiate(obstaclesPool.objectPrefab, new Vector3(gameFieldWidth, 0, gameFieldHeight / 2f), Quaternion.identity);
		wall.localScale = new Vector3(1, 1, gameFieldHeight + 1);
		wall = Instantiate(obstaclesPool.objectPrefab, new Vector3(gameFieldWidth / 2f, 0, 0), Quaternion.identity);
		wall.localScale = new Vector3(gameFieldWidth + 1, 1, 1);
		wall = Instantiate(obstaclesPool.objectPrefab, new Vector3(gameFieldWidth / 2f, 0, gameFieldHeight), Quaternion.identity);
		wall.localScale = new Vector3(gameFieldWidth + 1, 1 ,1);
		
		InitializeGame();
	}
	
	// Update is called once per frame
	void Update ()
	{
		snake.Move(joystick.direction, acceleration);
		MoveCamera();
		
		Transform bit = CheckFoodCollisions();
		if(bit) EatFood(bit);
		
		if (isSnakeOffscreen || snake.hasCollidedTail || CheckObstacleCollisions())
		{
			ShowDeathScreen();
		}
		
#if UNITY_EDITOR
		if (Input.GetKey(KeyCode.Space))
		{
			AccelerateButtonDown();
		}
		else
		{
			AccelerateButtonUp();
		}
#endif
	}

	void InitializeGame()
	{
		DespawnAllFood();
		DespawnAllObstacles();
		
		Vector3 initialPosition = new Vector3(gameFieldWidth / 2f, 0f, gameFieldHeight / 2f);
		snake.Initialize(intialSnakeSize, initialPosition, initialSnakeSpeed);
		
		joystick.Initialize();

		score = 0;
		scoreText.text = "Score: " + score;
		
		SpawnFood();
	}

	void MoveCamera()
	{
		Vector3 newPosition = snake.transform.position;
		newPosition.y = 10f;

		if (newPosition.z < 0 + gameCamera.orthographicSize ||
		    newPosition.z > gameFieldHeight - gameCamera.orthographicSize)
		{
			newPosition.z = gameCamera.transform.position.z;
		}

		if (newPosition.x < 0 + gameCamera.aspect * gameCamera.orthographicSize ||
		    newPosition.x > gameFieldWidth - gameCamera.aspect * gameCamera.orthographicSize)
		{
			newPosition.x = gameCamera.transform.position.x;
		}

		gameCamera.transform.position = newPosition;
	}

	void SpawnFood()
	{
		Transform spawn = foodPool.GetObjet();
		spawn.position = FindValidSpawnPosition();
		food.Add(spawn);
	}

	void SpawnObstacle()
	{
		Transform spawn = obstaclesPool.GetObjet();
		spawn.position = FindValidSpawnPosition();
		obstacles.Add(spawn);
	}

	Vector3 FindValidSpawnPosition()
	{
		Vector3 position = new Vector3();
		bool positionIsValid;
		int attempts = 0;
		do
		{
			positionIsValid = true;

			float offset = obstaclesPool.objectPrefab.localScale.magnitude;
			position.x = Random.Range(offset, gameFieldWidth - offset);
			position.z = Random.Range(offset, gameFieldHeight - offset);

			//Must be placed 1/3 screen away from head
			if (Vector3.Distance(snake.transform.position, position) <
			    gameCamera.orthographicSize * gameCamera.aspect / 3f)
			{
				positionIsValid = false;
				continue;
			}

			//Must not spawn on another food
			foreach (Transform bit in food)
			{
				if (Vector3.Distance(position, bit.position) < bit.localScale.magnitude)
				{
					positionIsValid = false;
					break;
				}
			}
			
			//Must not spawn on obstacle
			if(!positionIsValid) continue;

			foreach (Transform obstacle in obstacles)
			{
				if (Vector3.Distance(position, obstacle.position) < obstacle.localScale.magnitude)
				{
					positionIsValid = false;
					break;
				}
			}

			attempts++;
			if (attempts > 100) break;
		} while (!positionIsValid);

		return position;
	}

	void DespawnAllFood()
	{
		foreach (Transform bit in food)
		{
			foodPool.ReturnObject(bit);
		}
		food.Clear();
	}

	void DespawnAllObstacles()
	{
		foreach (Transform obstacle in obstacles)
		{
			obstaclesPool.ReturnObject(obstacle);
		}
		obstacles.Clear();
	}


	/// <summary>
	/// Returns food that snake's head collides with. Returns null if no collision.
	/// </summary>
	/// <returns></returns>
	Transform CheckFoodCollisions()
	{
		foreach (Transform bit in food)
		{
			if (Vector3.Distance(bit.position, snake.transform.position) < snake.transform.localScale.x)
			{
				return bit;
			}
		}
		return null;
	}

	void EatFood(Transform bit)
	{
		if (score < 30 && food.Count-1 < Decimal.Round(score / 10))
		{
			SpawnFood();
		}
		if (score == 30)
		{
			SpawnFood();
			SpawnFood();
		}
		
		bit.position = FindValidSpawnPosition();
		
		score ++;
		scoreText.text = "Score: " + score;
		if (score > highScore)
		{
			highScore = score;
			if (highScore > PlayerPrefs.GetInt("HighScore"))
			{
				PlayerPrefs.SetInt("HighScore", highScore);
				PlayerPrefs.Save();
				newHighScore = true;
			}
		}

		snake.speed += initialSnakeSpeed / 100f;
		snake.AddPart();
		
		SpawnObstacle();
	}

	bool CheckObstacleCollisions()
	{
		foreach (Transform obstacle in obstacles)
		{
			if (Vector3.Distance(obstacle.position, snake.transform.position) < snake.transform.localScale.x)
			{
				return true;
			}
		}
		return false;

	}

	void ShowDeathScreen()
	{
		pauseCanvas.gameObject.SetActive(true);
		resumeButton.gameObject.SetActive(false);
		scoreText.rectTransform.localPosition = new Vector3(0, -220);
		
		if (newHighScore)
		{
			highScoreText.text = "High score: " + highScore + "  NEW HIGH SCORE!";
		}
		else
		{
			highScoreText.text = "High score: " + highScore;
		}

		Time.timeScale = 0;
	}
	
	public void AccelerateButtonDown()
	{
		acceleration = true;
	}

	public void AccelerateButtonUp()
	{
		acceleration = false;
	}

	public void PauseButtonClick()
	{
		resumeButton.gameObject.SetActive(true);
		scoreText.rectTransform.localPosition = new Vector3(0, -220);
		if (newHighScore)
		{
			highScoreText.text = "High score: " + highScore + "  NEW HIGH SCORE!";
		}
		else
		{
			highScoreText.text = "High score: " + highScore;
		}

		Time.timeScale = 0;
	}

	public void ResumeButtonClick()
	{
		scoreText.rectTransform.localPosition = new Vector3(-780, 560);
		Time.timeScale = 1;
	}

	public void RestartButtonClick()
	{
		newHighScore = false;
		scoreText.rectTransform.localPosition = new Vector3(-780, 560);
		Time.timeScale = 1;
		InitializeGame();
	}

	public void ExitButtonClick()
	{
		Application.Quit();
	}

	public void MainMenuButtonClick()
	{
		SceneManager.LoadScene("Menu");
	}
}

