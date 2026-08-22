using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.IO;

public class CoreManager : MonoBehaviour
{
    public static CoreManager Instance;

    public SongData CurrentSongData; 
    public bool IsMvOn = false;      
    public SettingsData CurrentSettings = new SettingsData(); 
    
    public bool IsAutoPlay = false;

    // ⭐ 현재 활성화된 씬의 이름을 추적하여 정확히 지울 수 있도록 함
    private string currentActiveScene = ""; 

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
            LoadSettings(); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadSettings()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "data", "settings.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path, System.Text.Encoding.UTF8);
            CurrentSettings = JsonUtility.FromJson<SettingsData>(json);
            Debug.Log($"[Settings Loaded] Audio: {CurrentSettings.AudioOffset}, Judge: {CurrentSettings.JudgeOffset}, Mode: {CurrentSettings.Mode}");
        }
    }

    void Start()
    {
        string startSceneName = "1_SongSelect"; 
        
        if (CurrentSettings.Mode == 0) startSceneName = "3_Editor";
        else if (CurrentSettings.Mode == 1) startSceneName = "1_SongSelect";
        else if (CurrentSettings.Mode == 2) startSceneName = "2_Game"; 

        currentActiveScene = startSceneName;
        SceneManager.LoadSceneAsync(startSceneName, LoadSceneMode.Additive);
    }

    public void LoadGameScene(SongData songData, bool mvState)
    {
        CurrentSongData = songData;
        IsMvOn = mvState;
        // ⭐ 하드코딩된 언로드 대상을 제거하고 새로 로드할 씬 이름만 넘김
        StartCoroutine(TransitionScene("2_Game"));
    }

    public void LoadSongSelectScene()
    {
        StartCoroutine(TransitionScene("1_SongSelect"));
    }

    public void LoadEditorScene(SongData songData)
    {
        CurrentSongData = songData;
        StartCoroutine(TransitionScene("3_Editor"));
    }

    private IEnumerator TransitionScene(string loadSceneName)
    {
        // 1. 현재 활성화된 씬의 정보를 구조체로 미리 받아둠 (재시작 시 같은 이름의 씬 충돌 방지)
        Scene sceneToUnload = SceneManager.GetSceneByName(currentActiveScene);
        
        // 2. 새로운 씬 로드
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(loadSceneName, LoadSceneMode.Additive);
        while (!loadOp.isDone) yield return null;

        // 3. 이전 씬이 유효하게 열려있다면 정확히 찾아 언로드
        if (sceneToUnload.IsValid() && sceneToUnload.isLoaded)
        {
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(sceneToUnload);
            if (unloadOp != null)
            {
                while (!unloadOp.isDone) yield return null;
            }
        }
        
        // 4. 추적 중인 현재 씬 갱신
        currentActiveScene = loadSceneName;
    }
}