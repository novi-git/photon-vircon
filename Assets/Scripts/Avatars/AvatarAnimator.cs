using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{


    [RequireComponent( typeof( Animator ) )]
    public class AvatarAnimator : MonoBehaviour
    {
        Animator m_Animator;
        AnimatorOverrideController m_OverrideController;
        AvatarCustomization m_Customizer;

        List<CustomizationArtPack.EmoteData> Clips
        {
            get { return CustomizationData.Instance.GetAllEmotes(); }
        }

        int m_EmoteId;
        int m_LoopEmoteId;
        int m_TalkingId;

        bool m_IsTalking;

        float m_TalkingTarget = 0f;
        float m_TalkingLerpSpeed = 1f;
        float m_TalkingBlend;

        float m_LastTimeXRUpdateReceived;

        private void Awake()
        {
            m_Customizer = GetComponent<AvatarCustomization>();
            m_Animator = GetComponent<Animator>();
            m_EmoteId = Animator.StringToHash( "emote" );
            m_LoopEmoteId = Animator.StringToHash( "loopingEmote" );
            m_TalkingId = Animator.StringToHash( "talking" );

            m_OverrideController = new AnimatorOverrideController( m_Animator.runtimeAnimatorController );
            m_Animator.runtimeAnimatorController = m_OverrideController;
        }

        public void SetTalking( bool value )
        {
            m_IsTalking = value;
            m_TalkingTarget = value ? 1.5f : -1f;
            m_TalkingLerpSpeed = value ? 3f : 1f;
        }

        public void OnReceivedXRUpdate()
        {
            m_LastTimeXRUpdateReceived = Time.realtimeSinceStartup;
        }

        private void Update()
        {
            bool enableAnimator = m_LastTimeXRUpdateReceived + 2f < Time.realtimeSinceStartup;

            m_Animator.enabled = enableAnimator;
            if ( enableAnimator == false )
            {
                return;
            }

            if ( m_Customizer.IsTalking != m_IsTalking )
            {
                SetTalking( m_Customizer.IsTalking );
            }

            m_TalkingBlend = Mathf.MoveTowards( m_TalkingBlend, m_TalkingTarget, Time.deltaTime * m_TalkingLerpSpeed );
            m_Animator.SetFloat( m_TalkingId, m_TalkingBlend );

#if UNITY_EDITOR
            if( PlayerLocal.IsPlayerTyping() )
            {
                return;
            }

            PlayDebugAnim( KeyCode.Alpha1 );
            PlayDebugAnim( KeyCode.Alpha2 );
            PlayDebugAnim( KeyCode.Alpha3 );
            PlayDebugAnim( KeyCode.Alpha4 );
            PlayDebugAnim( KeyCode.Alpha5 );
            PlayDebugAnim( KeyCode.Alpha6 );
            PlayDebugAnim( KeyCode.Alpha7 );
            PlayDebugAnim( KeyCode.Alpha8 );
            PlayDebugAnim( KeyCode.Alpha9 );
#endif
        }

#if UNITY_EDITOR
        void PlayDebugAnim( KeyCode code )
        {
            if( Input.GetKeyDown( code ) )
            {
                byte id = (byte)( code - KeyCode.Alpha1 );
                PlayAnimation( id );

            }
        }
#endif
        public void PlayAnimation( List<AnimationClip> clips )
        {
            PlayAnimation( clips[ Random.Range( 0, clips.Count ) ] );
        }

        public void PlayAnimation( AnimationClip clip )
        {
            m_OverrideController[ "AvatarEmoteEmpty" ] = clip;
            m_Animator.SetTrigger( m_EmoteId );
        }



        public void PlayAnimation( byte animId )
        {
            int index = animId % Clips.Count;
            PlayAnimation( Clips[ index ].Clip );
        }
    }
}