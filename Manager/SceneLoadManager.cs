using System;
using System.Collections;
using System.Threading.Tasks;
using Currency.Core.Run;
using MicroWar.Multiplayer;
using RuntimeBuildscenes;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class SceneLoadManager : MonoSingle<SceneLoadManager>
{
    private int _sceneIndex;
    private string _sceneName;
    private UnityAction<SceneLoadState> _sceneLoadCallback;

    private bool _isLoading = false;
    public SceneIndex LastSceneIndex { get; private set; }
    private NetworkSceneManager _networkSceneManager;

    private void Start()
    {
        _networkSceneManager = NetworkManager.Singleton.SceneManager;
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
    }

    public void AddSceneLoadCallback(UnityAction<SceneLoadState> sceneLoadCallback)
    {
        _sceneLoadCallback += sceneLoadCallback;
    }
    
    public void RemoveSceneLoadCallback(UnityAction<SceneLoadState> sceneLoadCallback)
    {
        _sceneLoadCallback -= sceneLoadCallback;
    }

    public void OnReloadScene()
    {
        LastSceneIndex = (SceneIndex)GetActiveSceneIndex();
        SceneManager.LoadScene(_sceneName);
    }
    
    public void OnLoadScene(SceneIndex sceneIndex, bool anyway = false)
    {
        _sceneIndex = (int)sceneIndex;
        LastSceneIndex = (SceneIndex)GetActiveSceneIndex();
        if (!GetActiveSceneIndex().Equals(_sceneIndex) || anyway)
        {
            _sceneName = GetSceneName(_sceneIndex);
            SceneManager.LoadScene(_sceneName);
        }
    }

    public void OnLoadMultiScene(SceneIndex sceneIndex)
    {
        _sceneIndex = (int)sceneIndex;
        LastSceneIndex = (SceneIndex)GetActiveSceneIndex();
        string sceneName = sceneIndex.ToString();
        if (sceneIndex < SceneIndex.LEVEL_001)
            sceneName = $"{_sceneIndex + 1}.{sceneName}";
        SceneManager.LoadScene(_sceneName);
    }

    public void OnLoadSceneAsync(SceneIndex sceneIndex, bool useLoadUI = false)
    {
        if (_isLoading)
            return;
        _isLoading = true;
        _sceneIndex = (int)sceneIndex;
        LastSceneIndex = (SceneIndex)GetActiveSceneIndex();
        if (!GetActiveSceneIndex().Equals(_sceneIndex))
        {
            _sceneName = GetSceneName(_sceneIndex);
            StopAllCoroutines();
            if (useLoadUI)
                StartCoroutine(LoadSceneAsyncWithUI());
            else
                StartCoroutine(nameof(LoadSceneAsync));
        }
        else
            _isLoading = false;
    }
    
    private void OnServerStarted()
    {

    }

    public string GetActiveSceneName()
    {
        return GetSceneName(GetActiveSceneIndex());
    }
    
    public int GetActiveSceneIndex()
    {
        return SceneManager.GetActiveScene().buildIndex;
    }

    public string GetSceneName(int sceneIndex)
    {
        var buildSceneRecords = BuildScenes.Records;
        var scene = buildSceneRecords[sceneIndex];
        return scene.Name;
    }

    IEnumerator LoadSceneAsync()
    {
        _sceneLoadCallback?.Invoke(SceneLoadState.Before);
        
        XRUIInputModule inputModule = FindObjectOfType<XRUIInputModule>();
        inputModule.enableXRInput = false;
        inputModule.enableMouseInput = false;
        float speed = 2f;
        // UIManager.Instance.ShowUI("LoadingUI", true);
        // LoadingUI loadingUI = UIManager.Instance.GetUIComponent<LoadingUI>();
        // loadingUI.ShowBackground(true, speed);
        yield return new WaitForSeconds(1 / speed);
        //todo:scene start

        _sceneLoadCallback?.Invoke(SceneLoadState.Start);
        
        // BaseScene baseScene = FindObjectOfType<BaseScene>();
        // if (baseScene)
        //     baseScene.OnUnloadScene();
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(_sceneName);
        asyncOperation.allowSceneActivation = false;
        while (asyncOperation.progress < 0.9f)
            yield return new WaitForEndOfFrame();
        asyncOperation.allowSceneActivation = true;
        
        _sceneLoadCallback?.Invoke(SceneLoadState.End);
        
        yield return new WaitUntil(() => GetActiveSceneName().Equals(_sceneName));
        _isLoading = false;
        // UIManager.Instance.ShowUI("LoadingUI", false);
        
        _sceneLoadCallback?.Invoke(SceneLoadState.Final);
    }

    IEnumerator LoadSceneAsyncWithUI()
    {
        _sceneLoadCallback?.Invoke(SceneLoadState.Before);
        
        float speed = 1;
        // UIManager.Instance.ShowUI("LoadingUI", true);
        // LoadingUI loadingUI = UIManager.Instance.GetUIComponent<LoadingUI>();
        // loadingUI.ShowBackground(true, speed);
        yield return new WaitForSeconds(1);
        
        _sceneLoadCallback?.Invoke(SceneLoadState.Start);
        
        Camera.main.cullingMask = LayerMask.NameToLayer("UI");
        // loadingUI.ShowLoadingContent(true, speed, "同步数据...");
        yield return new WaitForSeconds(1);
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(_sceneName);
        asyncOperation.allowSceneActivation = false;
        int process = 0;
        int showProcess = 0;
        while (asyncOperation.progress < 0.9f)
        {
            process = (int)(asyncOperation.progress * 100);
            while (showProcess < process)
            {
                ++showProcess;
                // loadingUI.UploadProcess(showProcess);
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForEndOfFrame();
        }
        process = 100;
        while (showProcess < process)
        {
            ++showProcess;
            // loadingUI.UploadProcess(showProcess);
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForSeconds(0.5f);
        // loadingUI.ShowLoadingContent(true, speed * 100, "载入中...");
        showProcess = 0;
        while (showProcess < process)
        {
            ++showProcess;
            // loadingUI.UploadProcess(showProcess);
            yield return new WaitForEndOfFrame();
        }
        // loadingUI.ShowLoadingContent(false, speed);
        // loadingUI.ShowBackground(false, speed);
        // UIManager.Instance.ShowUI("LoadingUI", false);
        yield return new WaitForSeconds(0.3f);
        asyncOperation.allowSceneActivation = true;
       
        _sceneLoadCallback?.Invoke(SceneLoadState.End);
        
        yield return new WaitUntil(() => GetActiveSceneName().Equals(_sceneName));
        _isLoading = false;

        _sceneLoadCallback?.Invoke(SceneLoadState.Final);
    }
    
    public enum  SceneLoadState
    {
        Before,
        Start,
        End,
        Final
    }
    public enum SceneIndex
    {
        Entry = 0,
        Login,
        Lobby,
        MultiplayerLobby,
        MRScene,
        
        LEVEL_001 = 7,
        LEVEL_002,
        LEVEL_003,
        
        LEVEL_M001 = 17
    }
}