using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ToggleText : MonoBehaviour
{
    public string buttonNameText = "button";
    public string buttonBackText = "back";
    [SerializeField] private TMP_Text text;

    public void ChangeText(bool toggled)
    {
        text.text = toggled ? buttonBackText : buttonNameText;
    }
}
