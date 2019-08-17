﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MicrogameController : MonoBehaviour
{
    enum State
    {
        NOT_STARTED = 0,
        PLAYING,
        WON,
        LOST,
        NUM_STATES
    }
    public GameObject microgameSubScene = null;
    public static MicrogameController instance = null;
    private State _microgameState = State.NOT_STARTED;
    public float timeLimitSeconds = 5.0f;
    private float _currentTimerSeconds = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        if(!instance)
        {
            instance = this;
        }
    }

    void OnDestroy()
    {
        Debug.Assert(instance == this, "We somehow made a second instance of the microgame controller? :T");
        instance = null;
    }
    
    // Update is called once per frame
    void Update()
    {
        _currentTimerSeconds += Time.deltaTime;
        if(_currentTimerSeconds > timeLimitSeconds)
        {
            ReturnToOverworld();
        }
    }

    public void ReturnToOverworld()
    {
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("PrototypeOverworld"));
        OverworldController.instance.EndMicrogame();
        Destroy(microgameSubScene);
    }

    public void WinMicrogame()
    {
        if(_microgameState == State.LOST)
        {
            Debug.Log("We've already lost, so we can't win.");
            return;
        }
        _microgameState = State.WON;
        Debug.Log("Large winner!!!");
    }
    public void LoseMicrogame()
    {
        if(_microgameState == State.WON)
        {
            Debug.Log("We've already won, so we can't lose."); //Or can we? To be revisited in case a microgame needs it.
            return;
        }
        _microgameState = State.LOST;
        Debug.Log("Big loser!!!");
    }

    public bool HasWon()
    {
        return _microgameState == State.WON;
    }
    public bool HasLost()
    {
        return _microgameState == State.LOST;
    }

    public bool HasNotYetWon()
    {
        return (_microgameState == State.PLAYING || _microgameState == State.NOT_STARTED);
    }
}
