using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhoneScreen : MonoBehaviour
{
    // Start is called before the first frame update
    private Collider coll;
    private MeshRenderer renderer;

    public List<Color> colorList;
    public List<Texture> textureList;
 
    bool isRaffleStarted = false;

    void Start()
    {
         coll = GetComponent<Collider>();
         renderer = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
             if (Input.GetMouseButtonDown(0) && isRaffleStarted == false)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (coll.Raycast(ray, out hit, 100))
                {
                    Debug.Log("Phone Screen Clicked!");
                    isRaffleStarted = true;
                    StartCoroutine(RaffleSystemCountDown(5, 0.1f)); // Start Raffle System CountDown
                }
            }
    }

    IEnumerator RaffleSystemCountDown(float seconds, float decrement) {
        float counter = seconds;
        while (counter > 0) {
                GenerateRandomColors();
            yield return new WaitForSeconds (decrement);
            counter -= decrement;
        } 
        GenerateRandomTexture();
    }
      
    // Generate random colors
    // Then generate random texture
    void GenerateRandomColors(){
        renderer.material.mainTexture = null; // Remove texture and generate a new color
        var randomIndex = Random.Range(0, colorList.Count);
        renderer.material.color = colorList[randomIndex];
    }

    void GenerateRandomTexture(){
        if(textureList.Count > 0){
            var randomIndex = Random.Range(0, textureList.Count);
            renderer.material.color = Color.white;
            renderer.material.mainTexture = textureList[randomIndex]; 
        } 
        isRaffleStarted = false;
    }
}
