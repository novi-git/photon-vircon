using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadWebsite : MonoBehaviour
{
    public string loadWebsiteURL;
    public bool isComputerOn = false;

    private Collider coll;
    
    void Start()
    {
         coll = GetComponent<Collider>(); 
    }

    void StartLoadingWebsite(){
        Application.ExternalEval("window.open('" + loadWebsiteURL + "');");
    }

    void Update()
    {
             if (Input.GetMouseButtonDown(0) && isComputerOn == true)
            {
                
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (coll.Raycast(ray, out hit, 100))
                {
                    Debug.Log("Computer Screen Clicked!"); 
                    StartLoadingWebsite();
                }
            }
    }

    // Start is called before the first frame update
     
}
