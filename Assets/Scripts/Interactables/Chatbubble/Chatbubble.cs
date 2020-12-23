using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using Photon.Realtime;

namespace SpaceHub.Conference
{
    public class Chatbubble : MonoBehaviour
    {
        public const string CurrentChatBubbleProperty = "CurrentChatBubble";
        public const string CurrentChatBubblePositionProperty = "CurrentChatBubblePosition";

        public static List<Chatbubble> List = new List<Chatbubble>();

        public Transform[] PlayerPositions;

        public byte InterestGroup;

        public UnityAction PlayersChangedCallback;
        public List<Player> Players { get; private set; } = new List<Player>();

        SphereCollider m_Collider;
        ConferenceInteractable m_Interactable;
        public MeshRenderer DomeOutsideRenderer;

        public AudioSnapshotManager audioSnapshotRef;

        private void Awake()
        {
            m_Interactable = GetComponent<ConferenceInteractable>();
            m_Collider = GetComponent<SphereCollider>();
        }

        private void OnEnable()
        {
            List.Add( this );
        }

        private void OnDisable()
        {
            List.Remove( this );
        }

        public string GetVoiceRoomName()
        {
            return "Chatbubble-" + InterestGroup;
        }

        private void Start()
        {
            if( PlayerLocal.Instance != null )
            {
                PlayerLocal.Instance.Connector.PlayerPropertiesUpdateCallback += OnPlayerPropertiesUpdate;
                PlayerLocal.Instance.Connector.JoinedRoomCallback += OnJoinedRoom;
                PlayerLocal.Instance.Connector.PlayerLeftRoomCallback += OnPlayerLeftRoom;
            }
        }

        public void EnterLocalPlayer()
        {
            PlayerLocalChatbubble.Instance.SetCurrentChatBubble( this );
            BackUI.Instance?.AddBackData( () =>
            {
                PlayerLocalChatbubble.Instance.SetCurrentChatBubble( null );
                Vector3 normal = PlayerLocal.Instance.transform.position - transform.position;
                normal.y = 0f;
                normal.Normalize();
                normal = normal * ( m_Collider.radius + 1f );
                PlayerLocal.Instance.GoToAndLook( transform.position + normal, false, true );

            }, "Exit Chatbubble" );
        }

        public bool IsInside( Vector3 position )
        {
            Vector3 difference = position - transform.position;
            difference.y = 0;


            return difference.magnitude <= m_Collider.radius * Mathf.Max( transform.localScale.x, transform.localScale.y, transform.localScale.z );
        }

        public byte FindClosestFreePlayerPosition( Vector3 currentPosition )
        {
            byte closestPosition = 0;
            float closestDistance = float.MaxValue;

            //Player position byte starts at 1, because 0 gets stripped in custom properties
            for( byte i = 1; i < PlayerPositions.Length + 1; ++i )
            {
                if( IsPlayerPositionFree( i ) == true )
                {
                    float distance = Vector3.Distance( currentPosition, PlayerPositions[ i - 1 ].position );

                    if( distance < closestDistance )
                    {
                        closestPosition = i;
                        closestDistance = distance;
                    }
                }
            }

            return closestPosition;
        }

        public bool IsPlayerPositionFree( byte index )
        {
            for( int i = 0; i < Players.Count; ++i )
            {
                if( Players[ i ].CustomProperties.ContainsKey( Chatbubble.CurrentChatBubblePositionProperty ) )
                {
                    if( (byte)Players[ i ].CustomProperties[ Chatbubble.CurrentChatBubblePositionProperty ] == index )
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public bool HasFreePosition()
        {
            return Players.Count < PlayerPositions.Length;
        }

        public Transform GetTargetTransformForPosition( byte slotPosition )
        {
            int index = slotPosition - 1;
            return PlayerPositions[ index ];
        }

        public Transform GetTargetTransformForPlayer( Player player )
        {
            return GetTargetTransformForPosition( (byte)player.CustomProperties[ Chatbubble.CurrentChatBubblePositionProperty ] );
        }

        public int GetMaximumPlayers()
        {
            return PlayerPositions.Length;
        }

        public List<Player> FindPlayersInside()
        {
            List<Player> playersInside = new List<Player>();

            foreach( var pair in PlayerLocal.Instance.Connector.Network.Client.CurrentRoom.Players )
            {
                if( pair.Value.CustomProperties.ContainsKey( Chatbubble.CurrentChatBubbleProperty ) &&
                    (byte)pair.Value.CustomProperties[ Chatbubble.CurrentChatBubbleProperty ] == InterestGroup )
                {
                    playersInside.Add( pair.Value );
                }
            }

            return playersInside;
        }

        void OnPlayerLeftRoom( Player player )
        {
            if( Players.Contains( player ) )
            {
                Players = FindPlayersInside();
                OnPlayersChanged();
            }
        }

        void OnJoinedRoom()
        {
            Players = FindPlayersInside();
            OnPlayersChanged();
        }

        void OnPlayerPropertiesUpdate( Player player, ExitGames.Client.Photon.Hashtable propertiesThatChanged )
        {
            if( propertiesThatChanged.ContainsKey( Chatbubble.CurrentChatBubbleProperty ) )
            {
                byte newChatBubble = (byte)propertiesThatChanged[ Chatbubble.CurrentChatBubbleProperty ];

                if( Players.Contains( player ) && newChatBubble != InterestGroup )
                {
                    Players = FindPlayersInside();
                    OnPlayersChanged();
                }

                if( Players.Contains( player ) == false && newChatBubble == InterestGroup )
                {
                    Players.Add( player );
                    OnPlayersChanged();
                }
            }
        }

        protected void OnPlayersChanged()
        {
            PlayersChangedCallback?.Invoke();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;

            for( int i = 0; i < PlayerPositions.Length; ++i )
            {
                Gizmos.DrawWireSphere( PlayerPositions[ i ].position, 0.2f );
            }
        }

        public static Chatbubble FindByInterestGroup( byte interestGroup )
        {
            for( int i = 0; i < List.Count; ++i )
            {
                if( List[ i ].InterestGroup == interestGroup )
                {
                    return List[ i ];
                }
            }

            return null;
        }

        public static Chatbubble Find( Vector3 position )
        {
            foreach( var bubble in Chatbubble.List )
            {
                if( bubble.IsInside( position ) )
                {
                    return bubble;
                }
            }

            return null;
        }

        public void OnLocalPlayerJoined()
        {
            m_Interactable.enabled = false;
            m_Collider.enabled = false;
            DomeOutsideRenderer.enabled = false;

            audioSnapshotRef.AudioInside();
        }

        public void OnLocalPlayerLeft()
        {
            m_Interactable.enabled = true;
            m_Collider.enabled = true;
            DomeOutsideRenderer.enabled = true;

            audioSnapshotRef.AudioOutside();
        }
    }
}