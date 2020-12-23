using SpaceHub.Conference;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.Networking; 

public class ContentManager : MonoBehaviour {
    public ContentLocation Location = ContentLocation.Empty;
    public string fileName = "";

    public List<BoothDisplayContent> BoothDisplays = new List<BoothDisplayContent>();

    private const string  CMSurl = "https://vircondemo.taktylstudios.com/dev/CMS/JSON/";
 
    [HideInInspector]
    public const string FileLocation = "/JSON/";

    private void Awake() {
        SceneManager.sceneLoaded += LoadDataScene;
    }

    private void Start(){
        /* for(int x = 0; x < BoothDisplays.Count; x++ ){
            Debug.Log(BoothDisplays[x].Display.transform.name + " : "); 
        } */
         LoadData();
         
    }

    private void LoadDataScene(Scene scene, LoadSceneMode mode) {
       
    }

    private void OnDisable() {
        //SaveData();        
    }

    [ContextMenu("Save Data")]
    private void SaveData() {
        BoothContentData data = new BoothContentData { BoothDisplays = BoothDisplays, Location = Location };
        string content = JsonUtility.ToJson(data);
        string path = Application.streamingAssetsPath + FileLocation + Location.ToString() + ".json";
        System.IO.File.WriteAllText(path, content);
    }

    private void LoadData() {
        if(BoothDisplays.Count == 0) {
            return;
        } 
        /* 
        if(isOnline){ 
        }else{  
        }*/

  /* #if UNITY_EDITOR
        string path = Application.streamingAssetsPath + FileLocation + Location.ToString() + ".json";
        Debug.Log("Local Path: " + path);
        BoothContentData data = JsonUtility.FromJson<BoothContentData>(File.ReadAllText(path));
        Debug.Log("Data: " + File.ReadAllText(path));
        //Debug.Log("DATA: " +JsonUtility.FromJson<BoothContentData>(File.ReadAllText(path))); 
        //BoothDisplays = data.BoothDisplays; 
        LoadData(data);
    #endif  */

    //#if UNITY_WEBGL || UNITY_EDITOR  || UNITY_ANDROID
        StartCoroutine(Download());
   // #endif
    }

    IEnumerator Download() {
       // string path = Application.absoluteURL + "/StreamingAssets/" + FileLocation + Location.ToString() + ".json";
       //Debug.Log("Start Download!");

        //string path = Path.Combine(CMSurl, $"{Location.ToString()}.json"); 
        string path;
        if(fileName == ""){
            path = Path.Combine(CMSurl, $"{Location.ToString()}.json");
        }else{
            path = Path.Combine(CMSurl, fileName);
        }

        BoothContentData data = null;
        UnityWebRequest www = UnityWebRequest.Get(path);
        yield return www.SendWebRequest();
       // Debug.Log("Download Path: " + path);
        if (www.isNetworkError || www.isHttpError) {
            Debug.Log(www.error);
        } else {
            data = JsonUtility.FromJson<BoothContentData>(www.downloadHandler.text);
           // Debug.Log("Data: " + www.downloadHandler.text);
           // Debug.Log("Downloaded Data: " + data);
        } 

        LoadData(data);

    }

    private void LoadData(BoothContentData data) {
        if (data != null) {
            Debug.Log("Success getting Data");

            if (data.BoothDisplays != null) { 
               // Debug.Log("Booth Displays Count: " +  BoothDisplays.Count);
               // Debug.Log("JSON Booth Display is Not Empty: " + data.BoothDisplays.Count); 

                for (int i = 0; i < data.BoothDisplays.Count; i++) {
                
                if(BoothDisplays[i].Display == null) // if Booth Display is null continue
                    continue;

                BoothDisplays[i].ImageURL = data.BoothDisplays[i].ImageURL;
                BoothDisplays[i].Display.ImageUri = BoothDisplays[i].ImageURL;

              // Debug.Log(BoothDisplays[i].Display.transform.name + " : " + data.BoothDisplays[i].ImageURL);
            
                BoothDisplays[i].Display.SetImageUrl(BoothDisplays[i].ImageURL);
                } // end of for loop


            }  
            
        } else {
            Debug.LogErrorFormat("Scene: {0}, Error loading CMS", SceneManager.GetActiveScene().name);
        }
    }
}

