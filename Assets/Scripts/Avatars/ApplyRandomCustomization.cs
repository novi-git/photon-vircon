using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{

    public class ApplyRandomCustomization : MonoBehaviour
    {
        public Color[] SkinColors;
        public Color[] HairColors;
        public Color[] ShirtColors;
        public int[] HairIndices;

        private void Start()
        {
            /*var avatar = GetComponent<AvatarCustomization>();
            avatar.SkinColor = SkinColors[ Random.Range( 0, SkinColors.Length ) ];
            avatar.HairColor = HairColors[ Random.Range( 0, HairColors.Length ) ];
            avatar.ShirtColor = ShirtColors[ Random.Range( 0, ShirtColors.Length ) ];
            avatar.HairStyleIndex = (byte)HairIndices[ Random.Range( 0, HairIndices.Length ) ];
            avatar.ApplyCustomization();*/
        }
    }
}
