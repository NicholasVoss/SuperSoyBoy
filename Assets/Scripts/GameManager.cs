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

    public List<PlayerTimeEntry> LoadPreviousTimes()
    {
        try
        {
            var scoresFile = Application.persistentDataPath + "/" + playerName + "_times.dat";
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

    public void SaveTime(decimal time)
    {
        var times = LoadPreviousTimes();
        
        var newTime = new PlayerTimeEntry();
        newTime.entryDate = DateTime.Now;
        newTime.time = time;
        
        var bFormatter = new BinaryFormatter();
        var filePath = Application.persistentDataPath + "/" + playerName + "_times.dat";
        using (var file = File.Open(filePath, FileMode.Create))
        {
            times.Add(newTime);
            bFormatter.Serialize(file, times);
        }
    }

    public void DisplayPreviousTimes()
    {
        var times = LoadPreviousTimes();
        var topThree = times.OrderBy(time => time.time).Take(3);
        
        var timesLabel = GameObject.Find("PreviousTimes").GetComponent<Text>();
        
        timesLabel.text = "BEST TIMES \n";
        foreach (var time in topThree)
        {
            timesLabel.text += time.entryDate.ToShortDateString() + ": " + time.time + "\n";
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadscenemode)
    {
        if (scene.name == "Game")
        {
            DisplayPreviousTimes();
        }
    }

    private void  SetLevelName(string levelFilePath)
    {
        selectedLevel = levelFilePath;
        SceneManager.LoadScene("Game");
    }

    //create buttons for each level
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
}
