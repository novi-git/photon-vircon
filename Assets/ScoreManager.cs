using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 
using System;
using Photon.Realtime;

namespace SpaceHub.Conference
{
    public enum TeamColor {
        RED,
        BLUE,
        GREEN,
        YELLOW
    }

    [System.Serializable]
    public class TeamBuildingColor
    {  
        public TeamColor teamColor; 
        public TextMeshPro ScoreTextMesh;
        public int score;
    }

    public class ScoreManager : MonoBehaviour, IInRoomCallbacks
    {
        StageHandlerBase m_Connection; 
        private int maxScore = 20; 
        public TeamBuildingColor[] TeamBuildingList;    

        private void Awake()
        {
            m_Connection = transform.parent.GetComponentInChildren<StageHandlerBase>(); 
        }

       IEnumerator Start()
        {
            
            m_Connection.ScoreCallback += OnGetScoreBoard; 

            if( PlayerLocal.Instance.Connector == null )
            {
                yield break;
            }

            PlayerLocal.Instance.Client.AddCallbackTarget( this ); 

        }

        
        protected void OnDestroy()
        {
            
            m_Connection.ScoreCallback -= OnGetScoreBoard; 

            if( PlayerLocal.Instance.Connector != null &&
                PlayerLocal.Instance.Connector.Network != null &&
                PlayerLocal.Instance.Connector.Network.Client != null )
            {
                PlayerLocal.Instance.Connector.Network.Client.RemoveCallbackTarget( this );
            }


        }  

        void OnGetScoreBoard (ExitGames.Client.Photon.Hashtable data){
            // Get then update their UI's 
            //Debug.Log(data);
            TeamColor color = (TeamColor)data[ "TeamColor" ];
            int score = (int)data[ "TeamScore" ]; 

            for(int i = 0; i < TeamBuildingList.Length; i++){

                var TeamBuilding = TeamBuildingList[i];

                if(TeamBuilding.teamColor == color){
                        TeamBuilding.score = score; // Update Score
                        TeamBuilding.ScoreTextMesh.text = score.ToString("00"); // Update UI
                }
            }

        }

        void SendScoreBoard(TeamColor color, int score){

            var data = new ExitGames.Client.Photon.Hashtable(); 
            data.Add( "TeamColor", color );
            data.Add( "TeamScore", score );

            //Debug.Log("Send ScoreBoard from ScoreManager!");
            m_Connection.SendScoreBoard( data );

        }

        public void ResetScore(string colorName){
            Enum.TryParse(colorName, out TeamColor teamColor); // parse the string parameter from the button

            for(int i = 0; i < TeamBuildingList.Length; i++){ 
                var TeamBuilding = TeamBuildingList[i]; 

                if(TeamBuilding.teamColor == teamColor){ 
                        TeamBuilding.score = 0; 
                        SendScoreBoard(teamColor, TeamBuilding.score); // Send to the Server! 
                }
            }
        }

        public void IncreaseScore(string colorName){ 

            Enum.TryParse(colorName, out TeamColor teamColor); // parse the string parameter from the button

            for(int i = 0; i < TeamBuildingList.Length; i++){ 
                var TeamBuilding = TeamBuildingList[i];

                if(TeamBuilding.teamColor == teamColor){

                    if(TeamBuilding.score < maxScore)
                        TeamBuilding.score += 1; // Update Score 
                    else
                        TeamBuilding.score = 0;

                    SendScoreBoard(teamColor, TeamBuilding.score); // Send to the Server!
                       // TeamBuilding.ScoreTextMesh.text = TeamBuilding.score.ToString("00"); // Update UI
                }
            }

        }

        public void DecreaseScore(string colorName){ 

            Enum.TryParse(colorName, out TeamColor teamColor);

            for(int i = 0; i < TeamBuildingList.Length; i++){ 
                var TeamBuilding = TeamBuildingList[i];

                if(TeamBuilding.teamColor == teamColor){

                        if(TeamBuilding.score > 0)
                            TeamBuilding.score -= 1; // Update Score
                        else
                            TeamBuilding.score = maxScore;

                        SendScoreBoard(teamColor, TeamBuilding.score); // Send to the Server!
                      //  TeamBuilding.ScoreTextMesh.text = TeamBuilding.score.ToString("00"); // Update UI
                }
            }

        }

        private void UpdateScoreBoard(){
            
            for(int i = 0; i < TeamBuildingList.Length; i++){ 
                var TeamBuilding = TeamBuildingList[i]; 
                SendScoreBoard(TeamBuilding.teamColor, TeamBuilding.score); // Send to the Server!
                //  TeamBuilding.ScoreTextMesh.text = TeamBuilding.score.ToString("00"); // Update UI 
            }
        }

         

         public void OnPlayerEnteredRoom( Player newPlayer )
        { 
            // If you are the host UpdateScoreBoard to other player!
            UpdateScoreBoard();
 
            Debug.Log("Update ScoreBoard to: " + newPlayer.NickName);
            
        }

        public void OnMasterClientSwitched( Player newMasterClient ) { }
        public void OnPlayerLeftRoom( Player otherPlayer ) { }
        public void OnPlayerPropertiesUpdate( Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps ) { }
        public void OnRoomPropertiesUpdate( ExitGames.Client.Photon.Hashtable propertiesThatChanged ) { }
    }
}
