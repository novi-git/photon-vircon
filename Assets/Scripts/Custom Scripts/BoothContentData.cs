using SpaceHub.Conference;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BoothContentData 
{
    public List<BoothDisplayContent> BoothDisplays;
    public ContentLocation Location;
}

[System.Serializable]
public class SlideContentData // Add for presentation display
{
    public List<SlideDisplayContent> Presentations;
    public ContentLocation Location;
} 

[System.Serializable]
public class BoothContentDataList {
    public List<BoothContentData> ContentData = new List<BoothContentData>();
}

[System.Serializable]
public class BoothDisplayContent {
    public BoothDisplay Display;
    public string Name;
    public string ImageURL;
}

[System.Serializable]
public class SlideDisplayContent { // Add for presentation display
    public string Name;
    public string ImageURL;
    public Texture2D SlideTexture;
}

public enum ContentLocation {
    Empty = 0,
    ExpoMain = 1,
    Navigation = 2,
    Cinema = 3,
    Island = 4,
    Booth = 5
}
