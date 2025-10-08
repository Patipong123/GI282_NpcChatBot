using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class NpcDialogueResponder : MonoBehaviour
{
    [Header("Refs")]
    public AudioSource audioSource;     
    public InputField inputField;       
    public Text subtitleText;           

    [Header("Rules")]
    public List<ResponseRule> responses = new List<ResponseRule>();
    public AudioClip fallbackClip;      
    [TextArea] public string fallbackSubtitle = "Huh? Can you say that again?";

    [Header("Options")]
    public bool caseInsensitive = true;
    public bool lockWhileSpeaking = true;  

    bool isSpeaking = false;

    [Serializable]
    public class ResponseRule
    {
        
        public string id = "greeting";

        public string[] keywords;

        public bool exactMatch = false;

        public AudioClip clip;

        [TextArea] public string subtitle = "Hello!";

        public int priority = 0;
    }

    void Start()
    {
        if (inputField != null)
        {
            
            inputField.onEndEdit.AddListener(HandleSubmitFromInputField);
        }
    }

    void OnDestroy()
    {
        if (inputField != null)
            inputField.onEndEdit.RemoveListener(HandleSubmitFromInputField);
    }

    
    public void HandleSubmitFromInputField(string userText)
    {
        
        if (!string.IsNullOrWhiteSpace(userText))
        {
            HandleUserText(userText);
        }

        
        if (inputField != null)
        {
            inputField.text = "";
            inputField.ActivateInputField();
        }
    }

    
    public void HandleUserText(string userText)
    {
        if (lockWhileSpeaking && isSpeaking) return;

        string text = userText.Trim();
        if (caseInsensitive) text = text.ToLowerInvariant();

        ResponseRule best = null;
        int bestPriority = int.MinValue;

        foreach (var r in responses)
        {
            if (r == null || r.keywords == null || r.keywords.Length == 0) continue;

            bool matched = false;
            if (r.exactMatch)
            {
                
                string target = caseInsensitive ? string.Join(" ", r.keywords).ToLowerInvariant()
                                                : string.Join(" ", r.keywords);
                matched = (text == target);
            }
            else
            {
                
                foreach (var k in r.keywords)
                {
                    if (string.IsNullOrEmpty(k)) continue;
                    var key = caseInsensitive ? k.ToLowerInvariant() : k;
                    if (text.Contains(key))
                    {
                        matched = true;
                        break;
                    }
                }
            }

            if (matched && r.priority >= bestPriority)
            {
                best = r;
                bestPriority = r.priority;
            }
        }

        
        if (best != null && best.clip != null)
        {
            PlayResponse(best.clip, best.subtitle);
        }
        else if (fallbackClip != null)
        {
            PlayResponse(fallbackClip, fallbackSubtitle);
        }
        else
        {
            
            if (subtitleText) StartCoroutine(ShowSubtitleForDuration(fallbackSubtitle, 2f));
        }
    }

    void PlayResponse(AudioClip clip, string subtitle)
    {
        StopAllCoroutines();
        if (subtitleText) subtitleText.text = "";

        audioSource.Stop();       
        audioSource.clip = clip;
        audioSource.Play();

        
        if (lockWhileSpeaking) isSpeaking = true;

        float dur = clip ? clip.length : 0f;
        StartCoroutine(ShowSubtitleForDuration(subtitle, dur));
        StartCoroutine(UnlockAfter(dur));
    }

    IEnumerator ShowSubtitleForDuration(string text, float seconds)
    {
        if (subtitleText)
        {
            subtitleText.text = text;
            if (seconds > 0f) yield return new WaitForSeconds(seconds);
            subtitleText.text = "";
        }
    }

    IEnumerator UnlockAfter(float seconds)
    {
        if (seconds > 0f) yield return new WaitForSeconds(seconds);
        isSpeaking = false;
    }
}

