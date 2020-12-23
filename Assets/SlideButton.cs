using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using SpaceHub.Conference;  
using UnityEngine.UI;

public class SlideButton : MonoBehaviour
{
    private PresentationManager pManager;
    private int index;
    private string name;
    private string imageURL;
    private Texture2D texture;
    public Image image;


    private void Start(){
      //  image = transform.GetChild(0).GetComponent<Image>();
    }
    
    public void SelectSlide(){

       // pManager.SendSlideUri(imageURL);
        pManager.SetSlideIndex(index);

    }

    public void Initiate(PresentationManager _pManager, int _index, string _name, string _imageURL, Texture2D _texture){
        pManager = _pManager;
        index = _index;
        name = _name;
        imageURL = _imageURL;
        texture = _texture;
        //image.material.mainTexture = _texture;
        image.material.SetTexture( "_MainTex", texture );
    }
}
