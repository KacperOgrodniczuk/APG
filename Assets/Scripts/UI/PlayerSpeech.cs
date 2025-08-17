using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerSpeech : MonoBehaviour
{
    private TextMeshProUGUI speech;

    private void Start()
    {
        speech = GetComponent<TextMeshProUGUI>();
    }

    public void SetText(string text)
    {
        speech.text = text;
    }

    public void Flip()
    {
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}
