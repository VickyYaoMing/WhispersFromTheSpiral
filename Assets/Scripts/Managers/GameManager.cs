using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                var prefab = Resources.Load<GameManager>("GameManager");
                if(prefab != null)
                {
                    instance = Instantiate(prefab);
                }
            }
            return instance;
        }
    }

    public Player Player {  get; set; }
    public InteractionManager InteractionManager { get; set; }
    public SaveSystem SaveSystem { get;  set; }
    public ItemManager ItemManager { get; set; }
    public CheckpointManager CheckpointManager { get; set; }

    private bool isSaving;
    private bool isLoading;
    private bool shouldLoad;

    public bool saveExists;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;

        saveExists = SaveSystem.CheckForSave();

    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            Save();
            //CheckpointManager.CreateNewCheckpoint(new Vector3(-1.07f, 4.39f, -3.92f)); just a debug to test creating new checkpoints
        }
        if (Keyboard.current.fKey.wasPressedThisFrame) 
        {
            //LoadAsync();
            //Load();
        }
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(scene.name == "DemoSceneAct1" && shouldLoad)
        {
            StartCoroutine(Wait());
            shouldLoad = false;
        }
    }

    public void Save()
    {
        SaveSystem.Save();
        Debug.Log("Saved!");
    }

    public void Load()
    {
        SaveSystem.Load();
        Debug.Log("Loaded!");
    }

    public async void SaveAsync()
    {
        isSaving = true;
        await SaveSystem.SaveAsynchronously();
        Debug.Log("Saved!");
        isSaving = false;
    }

    public async void LoadAsync()
    {
        isLoading = true;
        await SaveSystem.LoadAsynchronously();
        Debug.Log("Loaded!");
        isLoading = false;
    }

    public void ShouldLoad(bool should)
    {
        shouldLoad = should;
    }

    public bool DoesSaveExist()
    {
        return SaveSystem.DoesSaveExist();
    }

    public bool IsSaving { get { return isSaving; } }
    public bool IsLoading { get { return isLoading; } }

    IEnumerator Wait()
    {
        float timeElapsed = 0f;

        while (timeElapsed < 1)
        {
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        LoadAsync();
    }

}



//Some kind of way to figure out what the current gamestate is?
public enum GameState
{
    Menu,
    Cutscene,
    Pause,
    Gameplay
}
public enum GameProgression
{
    Intro,
    Act1,
    Act2,
    Act3
}
