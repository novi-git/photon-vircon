using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;

namespace SpaceHub.Conference
{

    public class PrivateCallList : MonoBehaviour
    {
        ConferencePrivateCalls PrivateCalls
        {
            get { return PlayerLocal.Instance.PrivateCalls; }
        }
        public int ListLimit = 3;

        public RectTransform OutgoingParent;
        public RectTransform IncommingParent;

        public GameObject OutgoingPrefab;
        public GameObject IncommingPrefab;

        private void OnEnable()
        {
            OnOpen();
        }
        private void OnDisable()
        {
            OnClose();
        }

        void OnOpen()
        {
            if( PlayerLocal.Instance == null )
            {
                return;
            }

            PrivateCalls.IncomingCallsListChangedCallback += OnIncommingCallsChanged;
            PrivateCalls.OutgoingCallsListChangedCallback += OnOutgoingCallsChanged;

            OnIncommingCallsChanged( PrivateCalls.IncomingCallRequests );
            OnOutgoingCallsChanged( PrivateCalls.OutgoingCallRequests );
        }

        void OnClose()
        {
            PrivateCalls.IncomingCallsListChangedCallback -= OnIncommingCallsChanged;
            PrivateCalls.OutgoingCallsListChangedCallback -= OnOutgoingCallsChanged;
        }

        void OnIncommingCallsChanged( List<Player> players )
        {
            CreateList( IncommingParent, IncommingPrefab, players );
        }

        void OnOutgoingCallsChanged( List<Player> players )
        {
            CreateList( OutgoingParent, OutgoingPrefab, players );
        }

        void CreateList( RectTransform parent, GameObject prefab, List<Player> players )
        {
            Helpers.DestroyAllChildren( parent );

            for( int i = 0; i< players.Count && i < ListLimit; ++i )
            {
                var go = Instantiate( prefab, parent );
                var button = go.GetComponent<PrivateCallButton>();
                if ( button )
                {
                    button.Setup( players[ i ] );
                }
            }
        }

    }

}
