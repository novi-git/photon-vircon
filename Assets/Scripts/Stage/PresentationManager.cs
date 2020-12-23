using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using UnityEngine.UI;
using UnityEngine.Networking;
using SpaceHub.Conference;  
using UnityEngine.SceneManagement;
using System.IO;  
using TMPro;
namespace SpaceHub.Conference
{

    public class PresentationManager : MonoBehaviour
    {

        public Material ScreenMaterial;
        public MeshRenderer PresentationRenderer;
        public MeshRenderer TeleprompterRenderer;
        public Texture2D EmptyTexture;

        public Transform SlidesParent;
        public GameObject SlideButtonPrefab;
        public TextMeshProUGUI  SlidesNumberText;
        public Slider SliderLoadingBar;
        public GameObject PreloaderCanvas;

        StageHandlerBase m_Connection;

        //public string DefaultSlide;
        public string Location = "Presentation";

        private const string  CMSurl = "https://vircondemo.taktylstudios.com/dev/CMS/JSON/";

        private int slideIndex = 0;
        private int slideDownloadIndex = 0;
        private int maxSlideCount = 0;

        public List<SlideDisplayContent> SlideDisplays = new List<SlideDisplayContent>(); 
        private void Awake()
        {
            m_Connection = transform.parent.GetComponentInChildren<StageHandlerBase>();
            /*ScreenMaterial.SetTexture( "_BaseMap", EmptyTexture );

            if( string.IsNullOrEmpty( DefaultSlide ) == false )
            {
                DownloadTexture( DefaultSlide );
            } */
            m_Connection.PresentationCallback += OnGetPresentation;
        }

        private void Start(){ 
            PreloaderCanvas.SetActive(true);
            SliderLoadingBar.value = 0;
            StartCoroutine(Download()); // JSON Files
        } 
          
        IEnumerator Download() {
        // string path = Application.absoluteURL + "/StreamingAssets/" + FileLocation + Location.ToString() + ".json";
        Debug.Log("Start Presentation Download!"); 
        string path = Path.Combine(CMSurl, $"{Location.ToString()}.json");
        SlideContentData data = null;
        UnityWebRequest www = UnityWebRequest.Get(path);
        yield return www.SendWebRequest();
        Debug.Log("Download Path: " + path);
        if (www.isNetworkError || www.isHttpError) {
            Debug.Log(www.error);
        } else {
            data = JsonUtility.FromJson<SlideContentData>(www.downloadHandler.text);
            //Debug.Log("Data: " + www.downloadHandler.text);
            //Debug.Log("Downloaded Data: " + data);
        }  
             LoadData(data); 
            
        } // End of download

        private void LoadData(SlideContentData data) {
            if (data != null) {
                Debug.Log("Success getting Data");

                if (data.Presentations != null) {   
                    Debug.Log("JSON Slide Display is Not Empty: " + data.Presentations.Count);  
                        maxSlideCount = data.Presentations.Count;
                        for (int i = 0; i < data.Presentations.Count; i++) { 

                            SlideDisplays.Add(data.Presentations[i]);     
                           // Debug.Log("Name: " + data.Presentations[i].Name + " Slides URL:" + i + " : " + data.Presentations[i].ImageURL);
                      
                        } // end of for loop   

                     

                        StartCoroutine(DownloadInSync()); // Download Images in Sync
                    }

            } else {
                Debug.LogErrorFormat("Scene: {0}, Error loading CMS", SceneManager.GetActiveScene().name);
            }
        }

        IEnumerator DownloadInSync() {
 

            Debug.Log("Start Downloading Slides");

            for ( int i = 0; i < SlideDisplays.Count; i++) { 
                yield return StartCoroutine( Helpers.DownloadTextureRoutine( SlideDisplays[i].ImageURL, OnDownloadedTextureSave)); // Download and Save Texture to List 
                Debug.Log("Slide " + i); 
                
                // Set the default first slide texture
                if(i == 0 && SlideDisplays[i].SlideTexture != null){
                    ScreenMaterial.SetTexture( "_BaseMap", SlideDisplays[i].SlideTexture );
                }

                   if(SlidesParent != null)
                        {
                             GameObject slideButtonGO = Instantiate(SlideButtonPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                                slideButtonGO.transform.SetParent(SlidesParent, false);
                                SlideButton slideButton = slideButtonGO.GetComponent<SlideButton>(); 
                                slideButton.Initiate(this, i, SlideDisplays[i].Name, SlideDisplays[i].ImageURL, SlideDisplays[i].SlideTexture); 
                        }
               
                slideDownloadIndex++;
                Preloading(); // Update loading bar
            }

            Debug.Log("Done Downloading Slides!");
            PreloaderCanvas.SetActive(false); 
            
        } 

        private void Preloading(){
            float percentage = (float)slideDownloadIndex / (float)maxSlideCount;
            Debug.Log("Slides Percentage: " + percentage);
            SlidesNumberText.text = slideDownloadIndex + " / " + maxSlideCount;
            SliderLoadingBar.value = percentage; 
        }

        private void OnDestroy()
        {
            m_Connection.PresentationCallback -= OnGetPresentation;
        }

        void OnGetPresentation( int uri )
        {
            Debug.Log("Get Slide Index: " + uri);
            slideIndex = uri;
            //DownloadTexture( uri );
            ScreenMaterial.SetTexture( "_BaseMap", SlideDisplays[uri].SlideTexture );
            PresentationRenderer.sharedMaterial = ScreenMaterial;

            if(TeleprompterRenderer != null)
                TeleprompterRenderer.sharedMaterial = ScreenMaterial; 
        }

        void DownloadTexture( string uri )
        {
           // StopAllCoroutines();
            StartCoroutine( Helpers.DownloadTextureRoutine( uri, OnDownloadedTexture ) ); 
            Debug.Log("Download Slide Presentation: " + uri);
        }

        public void SetSlideIndex(int index){
            slideIndex = index; 
            // Set All Slides Color 
            //SendSlideUri(SlideDisplays[slideIndex].ImageURL); 
            SendSlideIndex (slideIndex);
        }
        public void SendSlideIndex (int index){
             m_Connection.SendPresentationUri( index );
        }
        public void SendSlideUri( string uri ) // calling from UI Buttons
        {
           // m_Connection.SendPresentationUri( uri );
        }

        public void NextSlide(){
            if(slideIndex < SlideDisplays.Count - 1){ 
                slideIndex++; 
            } else{
                slideIndex = 0;
            }

            SetSlideIndex(slideIndex); 
        }

        public void PreviousSlide(){ 
            if(slideIndex > 0){ 
                slideIndex--; 
            } else{
                slideIndex = SlideDisplays.Count - 1;
            }

             SetSlideIndex(slideIndex);
        }
 
        void OnDownloadedTexture( Texture2D tex )
        {
            ScreenMaterial.SetTexture( "_BaseMap", tex );
            PresentationRenderer.sharedMaterial = ScreenMaterial;

            if(TeleprompterRenderer != null)
                TeleprompterRenderer.sharedMaterial = ScreenMaterial;   
        }

        void OnDownloadedTextureSave( Texture2D tex){
            //Debug.Log("On Downloaded: " + uri);
            SlideDisplays[slideDownloadIndex].SlideTexture = tex;
            //SlideTextures.Add(tex);
        }

    }
}
