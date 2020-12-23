using ExitGames.Client.Photon;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceHub.Conference
{
    public class SpacesManager : MonoBehaviour
    {
        public static SpacesManager Instance;

        bool m_RegisteredCallback;

        Dictionary<byte, Space> m_Spaces = new Dictionary<byte, Space>();

        private void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            PlayerLocal.Instance.Connector.JoinedRoomCallback += OnJoinedRoom;
            PlayerLocal.Instance.Connector.WebRpcCallback += OnWebRpcResponse;
        }

        public void RegisterSpace( Space space )
        {
            m_Spaces.Add( space.InterestGroup, space );
        }

        public void UnregisterSpace( Space space )
        {
            m_Spaces.Remove( space.InterestGroup );
        }

        void OnJoinedRoom()
        {
            string roomName = PlayerLocal.Instance.Client.CurrentRoom.Name;

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add( "GameId", roomName );

            PlayerLocal.Instance.Client.OpWebRpc( "spaces", parameters, false );
        }

        public void OnWebRpcResponse( OperationResponse response )
        {
            if( response.IsResponseToUriPath( "spaces" ) == false ) return;

            if( response.Parameters.ContainsKey( ParameterCode.WebRpcParameters ) )
            {
                Debug.Log( "Got Spaces response: " + response.OperationCode + " - " + response.ReturnCode );

                foreach( var data in (Dictionary<string,object>)response.Parameters[ ParameterCode.WebRpcParameters ] )
                {
                    if( byte.TryParse( data.Key, out byte interestGroup ) )
                    {
                        if( m_Spaces.ContainsKey( interestGroup ) )
                        {
                            m_Spaces[ interestGroup ].LoadData( (Dictionary<string, object>)data.Value );
                        }
                        else
                        {
                            Debug.LogWarning( "Got a response for Space " + interestGroup + " but no such space exist." );
                        }
                    }
                }
            }
        }
    }
}