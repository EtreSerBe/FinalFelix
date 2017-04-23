using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CMicRecorder : MonoBehaviour {


    public float m_fMicSensitivity = 1.0f;
    AudioClip m_pRecordedAudioClip;
    protected bool m_bMicrophoneInitialized = false;

    private void Awake()
    {
        Debug.Log("The number of Microphones detected is: " + Microphone.devices.Length);
        if (Microphone.devices.Length > 0)
        {
            //Then, there's at least one Microphone detected.
            m_pRecordedAudioClip = Microphone.Start(Microphone.devices[0], true, 999, 44100);
            m_bMicrophoneInitialized = true;
        }
    }

    bool CheckRecordedLevel()
    {

        if (m_bMicrophoneInitialized == false)
        {
            Debug.Log("No microphone has been initialized.");
            return false;
        } 

        //get mic volume
        int dec = 128;
        float[] waveData = new float[dec];
        int micPosition = Microphone.GetPosition(null) - (dec + 1); // null means the first microphone
        m_pRecordedAudioClip.GetData(waveData, micPosition);

        // Getting a peak on the last 128 samples
        float levelMax = 0;
        for (int i = 0; i < dec; i++)
        {
            float wavePeak = waveData[i] * waveData[i];
            if (levelMax < wavePeak)
            {
                levelMax = wavePeak;
            }
        }
        float level = Mathf.Sqrt(Mathf.Sqrt(levelMax));
        if (level >= m_fMicSensitivity )
        {
            //Then, this audio must be sent.? Probably.
            Debug.LogWarning("Detected Voice input above the sensitivy value, this audio will be sent over the network.");
            return true;
        }
        //else /*if (level < m_fMicSensitivity)*/  // I don't consider this "if" necessary.
        Debug.Log("This audio doesn't have enough level, it will be rejected.");
        return false;
}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        CheckRecordedLevel();
	}
}
