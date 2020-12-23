using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{
    [CreateAssetMenu]
    public class CustomizationArtCategory : ScriptableObject
    {
        [System.Serializable]
        public class CustomizationAsset
        {
            public Mesh Mesh;
            public Material Material;
            public GameObject Prefab;
            public Texture2D Texture;

            public CustomizationArtCategory Category { get; set; }

            public GameObject GetPrefab()
            {
                if( Prefab != null )
                {
                    return Prefab;
                }

                return Category?.DefaultPrefab;
            }
        }

        [System.Serializable]
        public struct PreviewOffset
        {
            public Vector3 Offset;
            public Vector3 Scale;
            public Vector3 Rotation;
        }

        public enum CategoryType
        {
            None,
            Face,
            Eyes,
            Mouth,
            Hair,
            UpperBody, 
            LowerBody,
            Hands,
            AttachmentsHead,
            AttachmentsBody,
            Count,
            AttachmentsEyeBrow,
            AttachmentsFacialHair,
        }

        public CategoryType Type;
        public GameObject DefaultPrefab;

        public PreviewOffset PreviewButtonOffset;

        public List<CustomizationAsset> Assets = new List<CustomizationAsset>();
    }
}