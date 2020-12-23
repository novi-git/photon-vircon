using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.UI;
using Photon.Realtime;

namespace SpaceHub.Conference
{
    public class CustomizationMenu : MonoBehaviour
    {
        public AvatarCustomization AvatarCustomization;
        public AvatarAnimator AvatarAnimator;
        public AnimationClip CustomizationClip;

        float m_NextCustomizationAnim;

        private IEnumerator Start()
        {
            OnCustomizationChanged();
            yield return new WaitForSeconds( 1f );
            AvatarAnimator?.PlayAnimation( CustomizationData.Instance.GetCustomizationWelcomeAnimations() );
        }

        public void TryPlayCustomizationAnimation()
        {
            if ( Time.realtimeSinceStartup < m_NextCustomizationAnim )
            {
                return;
            }
            m_NextCustomizationAnim = Time.realtimeSinceStartup + 5f;
            AvatarAnimator?.PlayAnimation( CustomizationData.Instance.GetCustomizationAnimations() );
        }

        public void SetSkinColor( Color color )
        {
            CustomizationData.Instance.SkinColor = color;
            OnCustomizationChanged();
        }
        public void SetHairColor( Color color )
        {
            CustomizationData.Instance.HairColor = color;
            OnCustomizationChanged();
        }
        public void SetShirtColor( Color color )
        {
            CustomizationData.Instance.ShirtColor = color;
            OnCustomizationChanged();
        }

        public void SetHairstyleIndex( int index )
        {
            CustomizationData.Instance.HairstyleIndex = (byte)index;
            OnCustomizationChanged();
        }

        public void SetName( string name )
        {
            CustomizationData.Instance.Nickname = name;
            OnCustomizationChanged();
        }
        public void SetCompany( string name )
        {
            CustomizationData.Instance.CompanyName = name;
            OnCustomizationChanged();
        }

        void OnCustomizationChanged()
        {
            CustomizationData.Instance.ApplyCustomization( AvatarCustomization );
            CustomizationData.Instance.SaveToPlayerPrefs();

        }



        public void JoinExpo()
        {
            SceneManager.LoadScene( "Expo" );
            bgm.Instance?.stopBgm();
        }


    }
}
