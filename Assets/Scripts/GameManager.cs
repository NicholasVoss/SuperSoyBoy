using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public string playerName;
    public static GameManager instance;
    public GameObject buttonPrefab;
    private string selectedLevel;

    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        DiscoverLevels();
    }

    void Awake()
    {
        //create singleton
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    public void RestartLevel(float delay)
    {
        StartCoroutine(RestartLevelDelay(delay));
    }

    private IEnumerator RestartLevelDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("Game");
    }

    //load times from leaderboard
    public List<PlayerTimeEntry> LoadPreviousTimes()
    {
        try
        {
            var levelName = Path.GetFileName(selectedLevel);
            var scoresFile = Application.persistentDataPath + "/" + playerName + "_" + levelName + "_times.dat";
            using (var stream = File.Open(scoresFile, FileMode.Open))
            {
                var bin = new BinaryFormatter();
                var times = (List<PlayerTimeEntry>)bin.Deserialize(stream);
                return times;
            }
        }

        catch (IOException ex)
        {
            Debug.LogWarning("Couldn't load previous times for: " + playerName + ". Exception: " + ex.Message);
            return new List<PlayerTimeEntry>();
        }
    }

    //save completion time in the leaderboard
    public void SaveTime(decimal time)
    {
        var times = LoadPreviousTimes();
        
        var newTime = new PlayerTimeEntry();
        newTime.entryDate = DateTime.Now;
        newTime.time = time;
        
        var bFormatter = new BinaryFormatter();
        var levelName = Path.GetFileName(selectedLevel);
        var filePath = Application.persistentDataPath + "/" + playerName + "_" + levelName + "_times.dat";
        using (var file = File.Open(filePath, FileMode.Create))
        {
            times.Add(newTime);
            bFormatter.Serialize(file, times);
        }
    }

    //show leaderboard
    public void DisplayPreviousTimes()
    {
        var times = LoadPreviousTimes();
        var levelName = Path.GetFileName(selectedLevel);
        if(levelName != null)
        {
            levelName = levelName.Replace(".json", "");
        }
        var topThree = times.OrderBy(time => time.time).Take(3);
        
        var timesLabel = GameObject.Find("PreviousTimes").GetComponent<Text>();

        timesLabel.text = levelName + "\n";
        timesLabel.text += "BEST TIMES \n";
        foreach (var time in topThree)
        {
            timesLabel.text += time.entryDate.ToShortDateString() + ": " + time.time + "\n";
        }
    }

    //load scene
    private void OnSceneLoaded(Scene scene, LoadSceneMode loadscenemode)
    {
        //load level
        if (!string.IsNullOrEmpty(selectedLevel) && scene.name == "Game")
        {
            Debug.Log("Loading level content for: " + selectedLevel);
            LoadLevelContent();
            DisplayPreviousTimes();
        }
        //load menu
        if(scene.name == "Menu")
        {
            DiscoverLevels();
        }
    }

    private void  SetLevelName(string levelFilePath)
    {
        selectedLevel = levelFilePath;
        SceneManager.LoadScene("Game");
    }

    //create menu buttons for each level
    private void DiscoverLevels()
    {
        var levelPanelRectTransform = GameObject.Find("LevelItemsPanel").GetComponent<RectTransform>();
        var levelFiles = Directory.GetFiles(Application.dataPath, "*.json");

        var yOffset = 0f;
        for (var i = 0; i < levelFiles.Length; i++)
        {
            if (i == 0)
            {
                yOffset = -30f;
            }
            else
            {
                yOffset -= 65f;
            }
            var levelFile = levelFiles[i];
            var levelName = Path.GetFileName(levelFile);

            var levelButtonObj = (GameObject)Instantiate(buttonPrefab, Vector2.zero, Quaternion.identity);

            var levelButtonRectTransform = levelButtonObj.GetComponent<RectTransform>();
            levelButtonRectTransform.SetParent(levelPanelRectTransform, true);

            levelButtonRectTransform.anchoredPosition = new Vector2(212.5f, yOffset);

            var levelButtonText = levelButtonObj.transform.GetChild(0).GetComponent<Text>();
            levelButtonText.text = levelName;

            var levelButton = levelButtonObj.GetComponent<Button>();
            levelButton.onClick.AddListener( delegate { SetLevelName(levelFile); });
            levelPanelRectTransform.sizeDelta = new Vector2(levelPanelRectTransform.sizeDelta.x, 60f * i);
        }
        levelPanelRectTransform.offsetMax = new Vector2(levelPanelRectTransform.offsetMax.x, 0f);
    }

    //load level from JSON
    private void LoadLevelContent()
    {
        var existingLevelRoot = GameObject.Find("Level");
        Destroy(existingLevelRoot);
        var levelRoot = new GameObject("Level");

        var levelFileJsonContent = File.ReadAllText(selectedLevel);
        var levelData = JsonUtility.FromJson<LevelDataRepresentation>(levelFileJsonContent);

        foreach(var li in levelData.levelItems)
        {
            var pieceResource = Resources.Load("Prefabs/" + li.prefabName);
            if(pieceResource == null)
            {
                Debug.LogError("Cannot find resource: " + li.prefabName);
            }

            var piece = (GameObject)Instantiate(pieceResource, li.position, Quaternion.identity);
            var pieceSprite = piece.GetComponent<SpriteRenderer>();
            if (pieceSprite != null)
            {
                pieceSprite.sortingOrder = li.spriteOrder;
                pieceSprite.sortingLayerName = li.spriteLayer;
                pieceSprite.color = li.spriteColor;
            }
            piece.transform.parent = levelRoot.transform;
            piece.transform.position = li.position;
            piece.transform.rotation = Quaternion.Euler(li.rotation.x, li.rotation.y, li.rotation.z);
            piece.transform.localScale = li.scale;
        }
        var SoyBoy = GameObject.Find("SoyBoy");
        SoyBoy.transform.position = levelData.playerStartPosition;
        Camera.main.transform.position = new Vector3(SoyBoy.transform.position.x, SoyBoy.transform.position.y, Camera.main.transform.position.z);

        var camSettings = FindObjectOfType<CameraLerpToTransform>();

        if(camSettings != null)
        {
            camSettings.cameraZDepth = levelData.cameraSettings.cameraZDepth;
            camSettings.camTarget = GameObject.Find(levelData.cameraSettings.cameraTrackTarget).transform;
            camSettings.maxX = levelData.cameraSettings.maxX;
            camSettings.maxY = levelData.cameraSettings.maxY;
            camSettings.minX = levelData.cameraSettings.minX;
            camSettings.minY = levelData.cameraSettings.minY;
            camSettings.trackingSpeed = levelData.cameraSettings.trackingSpeed;
        }
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("Menu");
        }
    }
}
