using SpaceHub.Conference;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmotesToggle : MonoBehaviour
{
    public EmotesUi emotes;

    private void OnEnable() {        
        emotes.ToggleListView();
    }
}
