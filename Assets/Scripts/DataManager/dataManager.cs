using UnityEngine;
using System.Linq;
using Ink.Parsed;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class dataManager : MonoBehaviour
{
    [Header("Debug")]

    [SerializeField] bool turnOffPersistence = false;

    [Header("File Storage Config")]

    [SerializeField] string fileName;

    private gameData gameData;
    private List<IPersistData> persistDataObj;
    public static dataManager instance {  get; private set; }

    private fileDataHandler dataHandler;

    private void Awake()
    {

        if (instance != null)
        {
            Destroy(gameObject);
        }
        
        instance = this;

        DontDestroyOnLoad(this.gameObject);
        this.dataHandler = new fileDataHandler(Application.persistentDataPath, fileName);
    }

    public void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    public void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        this.persistDataObj = findAllDataobj();
        LoadGame();
    }

    public void OnSceneUnloaded(Scene scene)
    {
        SaveGame();
    }

    public void NewGame()
    {
        this.gameData = new gameData();
    }

    public void SaveGame()
    {
        // pass data to scripts to be updated
        foreach (IPersistData data in persistDataObj)
        {
            data.SaveData(ref gameData);
        }
        // svae data to a file

        dataHandler.Save(gameData);
    }

    public void LoadGame()
    {
        this.gameData = dataHandler.Load();
       
        if (gameData == null || turnOffPersistence)
        {
            NewGame();
        }

        foreach (IPersistData data in persistDataObj)
        {
            data.LoadData(gameData);
        }
        // push saved data to scripts
    }

    public void OnApplicationQuit()
    {
        
        SaveGame();
    }

    private List<IPersistData> findAllDataobj()
    {
        IEnumerable<IPersistData> dataPersisobj = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<IPersistData>();

        return new List<IPersistData>(dataPersisobj);
    }
}
