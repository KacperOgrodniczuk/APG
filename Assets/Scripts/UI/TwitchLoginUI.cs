using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TwitchLoginUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField OauthTokenInput;

    public void OnLoginButtonPressed()
    { 
        TwitchLoginDetails.Username = usernameInput.text;
        TwitchLoginDetails.OAuthToken = OauthTokenInput.text;
    }
}
