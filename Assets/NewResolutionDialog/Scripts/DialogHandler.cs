using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogHandler : MonoBehaviour
{
    public string SceneName;
    public GameObject Dialog;

    // Start is called before the first frame update
    void Start()
    {
#if !UNITY_STANDALONE
        GotoNextScene();
#else
        Dialog.SetActive( true );
#endif
    }

    public void GotoNextScene()
    {
        //SceneManager.LoadScene( SceneName );
    }
}
