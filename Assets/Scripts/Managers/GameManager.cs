using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    public static GameManager Instance
    {
        get
        {
#if UNITY_EDITOR
            if(!Application.isPlaying)
            {
                return null;
            }

            if(instance == null)
            {
                Instantiate(Resources.Load<GameManager>("GameManager"));
            }
#endif
            return instance;
        }
    }

    public Player Player {  get; set; }
    public SaveSystem SaveSystem { get;  set; }

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
        }
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            SaveSystem.Save();
            Debug.Log("Saved!");
        }
        if (Keyboard.current.fKey.wasPressedThisFrame) 
        {
            SaveSystem.Load();
            Debug.Log("Loaded!");
        }

    }
}

public enum GameProgression
{
    Intro,
    Act1,
    Act2,
    Act3
}
