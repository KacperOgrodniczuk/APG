using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockTimer : MonoBehaviour
{
    public Slider slider;
    public Image Fill;
    public Image background;

    public void SetMaxTime(float maxTime)
    {
        slider.maxValue = maxTime;
        slider.value = 0;
    }

    public void SetTime(float time)
    {
        slider.value = time;
    }

    public void Flip() //flip the blockbar when appropriate so that it doesn't seem to flip when the character is facing left
    {
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}
