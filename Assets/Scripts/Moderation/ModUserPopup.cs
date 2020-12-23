using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ExitGames.Client.Photon;
using Photon.Realtime;


namespace SpaceHub.Conference
{
    public class ModUserPopup : PopupBase
    {
        public static ModUserPopup Instance;

        public TextMeshProUGUI Name;
        public TextMeshProUGUI Company;


        public RectTransform ReportListParent;
        public GameObject ReportsPrefab;

        public TextMeshProUGUI FullReportMessageText;
        public TextMeshProUGUI FullReportReporterText;
        public TextMeshProUGUI FullReportTimestampText;

        ModUserList.UserListData m_Data;

        private void Awake()
        {
            Instance = this;
            Close();
        }

        private void Start()
        {
            OpenCallback += OnOpen;
            CloseCallback += OnClose;
        }

        void OnOpen()
        {
            PlayerLocal.Instance.Connector.WebRpcCallback -= OnWebRpcResponse;
            PlayerLocal.Instance.Connector.WebRpcCallback += OnWebRpcResponse;
        }
        void OnClose()
        {
            PlayerLocal.Instance.Connector.WebRpcCallback -= OnWebRpcResponse;
        }

        public void Show( ModUserList.UserListData data )
        {
            SetData( data );
            RequestSingleUser( data.UserId );
        }

        public void Show( string userId )
        {
            DoOpen();
            RequestSingleUser( userId );
        }

        public void Close()
        {
            DoClose();
        }

        void RequestSingleUser( string userId )
        {
            Debug.Log( "Requesting user id :" + userId );
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add( "TargetUserId", userId );

            PlayerLocal.Instance.Client.OpWebRpc( "userdetails", parameters, true );
        }

        public void OnWebRpcResponse( OperationResponse response )
        {
            if( response.CheckIsValidAndReport( "Can't get user details." ) == false ) return;
            if( response.IsResponseToUriPath( "userdetails" ) == false ) return;

            string jsonData = response.DebugMessage;
            Debug.Log( "Jsondata: \n" + jsonData );
            SetData( jsonData );
        }

        void SetData( string jsonData )
        {
            var data = JsonUtility.FromJson<ModUserList.UserListData>( jsonData );
            if( data == null )
            {
                Debug.LogError( "Could not get user data from server." );
                return;
            }
            SetData( data );
        }

        void SetData( ModUserList.UserListData data )
        {
            m_Data = data;
            Name.text = data.GetDisplayName();
            Company.text = data.Company;

            /*
            data.ReportsData = new ModUserList.ReportData[]
            {
               new ModUserList.ReportData()
               {
                   Message = " testmessage!",
                   ReporterName = "reporter name ",
                   TimeStamp = "timestamp"
               }
               ,
               new ModUserList.ReportData()
               {
                   Message = " testmessagasd fasdf asdf e!",
                   ReporterName = "reporter nameasf asdf  ",
                   TimeStamp = "timestamp asdf"
               }
            };*/

            Helpers.DestroyAllChildren( ReportListParent );
            if( data.ReportsData != null )
            {
                foreach( var report in data.ReportsData )
                {
                    var go = Instantiate( ReportsPrefab, ReportListParent );

                    var texts = go.GetComponentsInChildren<TextMeshProUGUI>();
                    texts[ 0 ].text = System.DateTime.Parse( report.TimeStamp ).ToString( "g" );
                    texts[ 1 ].text = report.Message;
                    texts[ 2 ].text = report.ReporterName;

                    var reportItem = report;

                    var button = go.GetComponent<Button>();
                    button.onClick.AddListener( () => { ShowFullReport( reportItem ); } );
                }

            }

            if( data.ReportsData != null && data.ReportsData.Length > 0 )
            {
                ShowFullReport( data.ReportsData[ 0 ] );
            }
            else
            {
                HideFullReport();
            }

            DoOpen();
        }

        void HideFullReport()
        {
            FullReportMessageText.text = "No Reports";
            FullReportReporterText.text = "";
            FullReportTimestampText.text = "";
        }

        void ShowFullReport( ModUserList.ReportData report )
        {
            FullReportMessageText.text = report.Message;
            FullReportReporterText.text = report.ReporterName;
            FullReportTimestampText.text = System.DateTime.Parse( report.TimeStamp ).ToString( "g" );
        }

        string GetChatId()
        {
            return Groups.Helper.GetAuthName( m_Data.UserId, m_Data.Nickname );
        }

        public void TeleportToUser()
        {
            SignalManager.Instance.SendRequestUserPositionAndTeleport( GetChatId() );
        }

        public void TeleportUserToMe()
        {
            SignalManager.Instance.SendPullUserToMyPosition( GetChatId() );
        }

        public void BanUser()
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add( "TargetUserId", m_Data.UserId );

            PlayerLocal.Instance.Client.OpWebRpc( "userban", parameters, true );

            SignalManager.Instance.SendPrivateSignal( SignalManager.SignalType.Kick, GetChatId(), null );
        }

    }
}
