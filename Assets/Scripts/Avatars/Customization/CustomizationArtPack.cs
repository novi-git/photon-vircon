using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{
    [CreateAssetMenu]
    public class CustomizationArtPack : ScriptableObject
    {
        [System.Serializable]
        public struct EmoteData
        {
            public AnimationClip Clip;
            public string ButtonText;
            public string ChatSentence;
            public Sprite Icon;
        }

        public bool IsEnabled = true;

        public GameObject BasePrefab;
        public string Prefix;

        public List<CustomizationArtCategory> Categories = new List<CustomizationArtCategory>();
        public List<EmoteData> EmoteAnimations = new List<EmoteData>();
        public List<AnimationClip> CustomizationAnimations = new List<AnimationClip>();
        public List<AnimationClip> CustomizationWelcomeAnimations = new List<AnimationClip>();
    }
}
