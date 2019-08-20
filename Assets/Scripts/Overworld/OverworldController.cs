﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OverworldController : MonoBehaviour
{
    public enum State
    {
        PLAYING = 0,
        MICROGAME_TRANSITION,
        MICROGAME,
        PLAYING_TRANSITION,
        NUM_STATES
    }
    public static OverworldController instance = null;

    [HideInInspector]
    public bool freezeInput {get; set;}

    [HideInInspector]
    public string currentSceneName = "";

    [HideInInspector]    
    public MicrogameController.State lastMicrogameState = MicrogameController.State.NOT_STARTED;

    public GameObject victoryFanfare = null;
    public GameObject lossFanfare = null;

    private State _state = State.PLAYING;
    private float _timeInState = 0.0f;
    private AsyncOperation _asyncMicrogameLoad = null;
    private string _currentMicrogameName = null;
    private GameObject _currentActivatingEnemy = null;
    private GameObject _playerReference = null;
    private GameObject _cameraReference = null;

    // Start is called before the first frame update
    void Start()
    {
        if(!instance)
        {
            instance = this;
            currentSceneName = SceneManager.GetActiveScene().name;
        }

        _playerReference = GameObject.FindGameObjectWithTag("Player");
        Debug.Assert(_playerReference, "The player should be tagged as 'Player'");
        _cameraReference = GameObject.FindGameObjectWithTag("MainCamera");
        Debug.Assert(_playerReference, "Need a Main Camera to reference in Overworld Controller");
    }

    // Update is called once per frame
    void Update()
    {
        _timeInState += Time.deltaTime;
        if(_state == State.MICROGAME_TRANSITION)
        {
            _cameraReference.GetComponent<SimpleBlit>().transitionValue = Mathf.Clamp01(_timeInState);
        }
        else if(_state == State.PLAYING_TRANSITION)
        {
            _cameraReference.GetComponent<SimpleBlit>().transitionValue = Mathf.Clamp01(1.0f - _timeInState);
        }
    }

    void ChangeToState(State newState)
    {
        _state = newState;
        _timeInState = 0.0f;
    }

    public void BeginMicrogame(string microgameName, GameObject activatingEnemy)
    {
        ChangeToState(State.MICROGAME_TRANSITION);
        freezeInput = true;
        StartCoroutine(LoadMicrogameAsync(microgameName));
        StartCoroutine(ExecuteMicrogameScene(microgameName));
        _currentMicrogameName = microgameName;
        _currentActivatingEnemy = activatingEnemy;
    }

    public void EndMicrogame()
    {
        ChangeToState(State.PLAYING_TRANSITION);
        StartCoroutine(UnloadMicrogameAsync(_currentMicrogameName));
        _currentMicrogameName = null;
        _asyncMicrogameLoad = null;

        if(lastMicrogameState == MicrogameController.State.WON)
        {
            Destroy(_currentActivatingEnemy);

            if(victoryFanfare)
            {
                Instantiate(victoryFanfare);
            }
        }
        else
        {
            _currentActivatingEnemy.GetComponent<EnemyScript>().Reset();
            if(lossFanfare)
            {
                Instantiate(lossFanfare);
            }
        }
        _currentActivatingEnemy = null;
    }
        
    IEnumerator ExecuteMicrogameScene(string microgameSceneName)
    {
        yield return new WaitForSeconds(1.0f);
        _asyncMicrogameLoad.allowSceneActivation = true;
        while (!_asyncMicrogameLoad.isDone)
        {
            yield return null;
        }
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(microgameSceneName));
        ChangeToState(State.MICROGAME);
    }

    IEnumerator LoadMicrogameAsync(string microgameSceneName)
    {
        _asyncMicrogameLoad = SceneManager.LoadSceneAsync(microgameSceneName, LoadSceneMode.Additive);
        _asyncMicrogameLoad.allowSceneActivation = false;
        // Wait until the asynchronous scene fully loads
        while (!_asyncMicrogameLoad.isDone)
        {
            yield return null;
        }
    }
    IEnumerator UnloadMicrogameAsync(string microgameSceneName)
    {
        AsyncOperation asyncMicrogameUnload = SceneManager.UnloadSceneAsync(microgameSceneName);
        asyncMicrogameUnload.allowSceneActivation = false;

        float startTime = Time.time;
        // Wait until the asynchronous scene fully unloads
        while (!asyncMicrogameUnload.isDone)
        {
            yield return null;
        }
        
        float elapsedTime = Time.time - startTime;
        float timeToWait = Mathf.Clamp01(1.0f - elapsedTime);

        yield return new WaitForSeconds(timeToWait);
        
        ChangeToState(State.PLAYING);
        freezeInput = false;
    }
}
