using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using ExitGames.Client.Photon;
using Photon.Realtime;

namespace SpaceHub.Conference
{
    public class ModUserList : MonoBehaviour
    {
        [System.Serializable]
        public class Wrapper
        {
            public UserListData[] Items;
            public string[] ValidScopes;
            public string ReceivedScope;
        }

        public enum RoleType
        {
            None,
            LocalModerator,
            Moderator,
            Admin
        }

        [System.Flags]
        public enum FlagType
        {
            None = 0,
            Muted = 1 << 0,
            Blocked = 1 << 1,
            VIP = 1 << 2

            // increase by power of 2 ( bitwise flag )
        }

        [System.Serializable]
        public class RoleData
        {
            public RoleType RoleType;
        }

        [System.Serializable]
        public class ReportData
        {
            public string TimeStamp;
            public string Message;
            public string ReporterName;
            public string ReporterUserId;
        }

        [System.Serializable]
        public class UserListData
        {
            public string Name;
            public string Nickname;
            public string Company;
            public string UserId;
            public string GameId;

            public int Reports;
            public ReportData[] ReportsData;

            public RoleData[] Roles;
            public FlagType Flags;
            public string AppVersion;


            public string GetDisplayName()
            {
                return $"{Nickname} ({Name})";
            }
        }

        public RectTransform Parent;
        public GameObject ItemPrefab;

        public int ItemsPerPage = 10;

        public int PageCount { get; private set; }


        UserListData[] m_Data;
        List<UserListData> m_FilteredData = new List<UserListData>();


        public ModUserListPages PageManager;
        public TMP_InputField NameFilterInput;
        public TMP_Dropdown DropdownRoles;
        public TMP_Dropdown DropdownScope;
        public TMP_Dropdown DropdownFlags;

        public GameObject Spinner;
        private void Awake()
        {
            //SetData( TestData );

            DropdownRoles.ClearOptions();

            SetDropdownOptions<RoleType>( DropdownRoles );
            SetDropdownOptions<FlagType>( DropdownFlags );
        }

        public void SetDropdownOptions<TYPE>( TMP_Dropdown dropdown ) where TYPE : System.Enum
        {
            List<string> RoleOptions = new List<string>( System.Enum.GetNames( typeof( TYPE ) ) );
            dropdown.ClearOptions();
            dropdown.AddOptions( RoleOptions );
        }

        void OnEnable()
        {

            PlayerLocal.Instance.Connector.WebRpcCallback += OnWebRpcResponse;

            if( ConferenceRoomManager.Instance != null )
            {
                RequestData( ConferenceRoomManager.Instance.CurrentRoomName );
            }
        }

        private void OnDisable()
        {

            PlayerLocal.Instance.Connector.WebRpcCallback -= OnWebRpcResponse;
        }

        public void Refresh()
        {
            RequestData( DropdownScope.options[ DropdownScope.value ].text );
        }

        void RequestData( string scope )
        {

            Debug.Log( "request data " + scope );
            // webrtc
            ClearPage();
            PageCount = 0;
            m_FilteredData.Clear();

            PageManager.SetPage( -1 );
            Spinner.SetActive( true );

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add( "Scope", scope );

            PlayerLocal.Instance.Client.OpWebRpc( "userlist", parameters, true );
            //            StartCoroutine( FakeRequestRoutine() );
        }


        public void OnWebRpcResponse( OperationResponse response )
        {
            if( response.CheckIsValidAndReport( "Can't view this Userlist" ) == false ) return;
            if( response.IsResponseToUriPath( "userlist" ) == false ) return;

            string jsonData = response.DebugMessage;
            Debug.Log( "Jsondata: \n" + jsonData );
            SetData( jsonData );
        }

        IEnumerator FakeRequestRoutine()
        {
            yield return new WaitForSeconds( Random.Range( 0.5f, 2f ) );
            SetData( TestData );
        }


        public void SetData( string jsonString )
        {
            Spinner.SetActive( false );
            var wrapper = JsonUtility.FromJson<Wrapper>( jsonString );
            m_Data = wrapper.Items;


            List<string> scopeOptions = new List<string>( wrapper.ValidScopes );

            DropdownScope.ClearOptions();
            DropdownScope.AddOptions( scopeOptions );
            DropdownScope.SetValueWithoutNotify( scopeOptions.IndexOf( wrapper.ReceivedScope ) );

            m_FilteredData.Clear();
            m_FilteredData.AddRange( m_Data );

            ApplyFilter();
        }

        public void ShowPage( int pageIndex )
        {
            var page = m_FilteredData.Skip( ItemsPerPage * pageIndex ).Take( ItemsPerPage );

            ClearPage();

            foreach( var item in page )
            {
                var go = Instantiate( ItemPrefab, Parent );
                var script = go.GetComponent<ModUserListItem>();
                script.SetData( item );
            }

            PageManager.SetPage( pageIndex );
        }

        public void ResetFilter()
        {
            NameFilterInput.SetTextWithoutNotify( "" );
            //DropdownScope.SetValueWithoutNotify( 0 );
            DropdownRoles.SetValueWithoutNotify( 0 );
            DropdownFlags.SetValueWithoutNotify( 0 );

            ApplyFilter();
        }

        public void ApplyFilter()
        {
            m_FilteredData.Clear();
            m_FilteredData.AddRange( m_Data );
            if( string.IsNullOrEmpty( NameFilterInput.text ) == false )
            {
                string nameFilter = NameFilterInput.text.ToLowerInvariant();
                m_FilteredData = m_FilteredData.Where( item => item.Name.ToLowerInvariant().Contains( nameFilter ) || item.Company.ToLowerInvariant().Contains( nameFilter ) ).ToList();
            }

            if( DropdownRoles.value != 0 )
            {
                var roleType = (RoleType)DropdownRoles.value;
                m_FilteredData = m_FilteredData.Where( item => item.Roles != null && item.Roles.Length > 0 && item.Roles.FirstOrDefault( role => role.RoleType == roleType ) != null ).ToList();
            }


            //if( DropdownScope.value != 0 )
            {
                var room = DropdownScope.options[ DropdownScope.value ].text;
                if( room != "Global" )
                {
                    m_FilteredData = m_FilteredData.Where( item => item.GameId == room ).ToList();
                }
            }

            if( DropdownFlags.value != 0 )
            {
                var flag = (FlagType)DropdownFlags.value;
                m_FilteredData = m_FilteredData.Where( item => item.Flags.HasFlag( flag ) ).ToList();
            }

            OnListChanged();
        }

        void OnListChanged()
        {
            PageCount = Mathf.FloorToInt( m_FilteredData.Count / ItemsPerPage ) + 1;
            ShowPage( 0 );
        }

        void ClearPage()
        {
            for( int i = 3; i < Parent.childCount; ++i )
            {
                Destroy( Parent.GetChild( i ).gameObject );
            }
        }

        #region testdata
        string TestData = "{\"ValidScopes\":[\"Global\",\"Room1\",\"Room2\"],\"ReceivedScope\":\"Room1\",\"Items\":[{\"Name\":\"Amy Terrell\",\"Company\":\"Animalia\",\"UserId\":\"19f4774f-a4de-4404-bd15-4237ae5b1693\",\"GameId\":\"Chatbubble\",\"Reports\":0,\"Roles\":[{\"id\":0,\"RoleType\":2}],\"AppVersion\":\"officia\",\"Flags\":7},{\"Name\":\"Hogan Foreman\",\"Company\":\"Comstruct\",\"UserId\":\"c22eeed3-8907-4ba1-9ea6-8a0701dac7f1\",\"GameId\":\"Room1\",\"Reports\":5,\"Roles\":[{\"id\":0,\"RoleType\":3}],\"AppVersion\":\"tempor\",\"Flags\":0},{\"Name\":\"Mitchell Ellison\",\"Company\":\"Aquasure\",\"UserId\":\"42dac633-2ce5-4147-9ece-da17e580d282\",\"GameId\":\"Room1\",\"Reports\":9,\"Roles\":[{\"id\":0,\"RoleType\":3}],\"AppVersion\":\"do\",\"Flags\":1},{\"Name\":\"Doyle Schwartz\",\"Company\":\"Kiosk\",\"UserId\":\"8c95af4c-69c1-4bdb-a88b-727cfc624f0f\",\"GameId\":\"Room2\",\"Reports\":2,\"Roles\":[{\"id\":0,\"RoleType\":3},{\"id\":1,\"RoleType\":1}],\"AppVersion\":\"culpa\",\"Flags\":7},{\"Name\":\"Rebecca Craig\",\"Company\":\"Xsports\",\"UserId\":\"3557b438-6dae-4e1d-8d3d-fe274bf16c37\",\"GameId\":\"Room1\",\"Reports\":6,\"Roles\":[{\"id\":0,\"RoleType\":3},{\"id\":1,\"RoleType\":3}],\"AppVersion\":\"dolor\",\"Flags\":1},{\"Name\":\"Johnson Hopper\",\"Company\":\"Webiotic\",\"UserId\":\"c153a5ce-319f-413c-b86a-e2a71bd3beb4\",\"GameId\":\"Room1\",\"Reports\":5,\"Roles\":[{\"id\":0,\"RoleType\":1}],\"AppVersion\":\"pariatur\",\"Flags\":5},{\"Name\":\"Vickie Greene\",\"Company\":\"Tetak\",\"UserId\":\"b7be4f5c-c27c-4a57-b68c-bd9ce2b77615\",\"GameId\":\"Room1\",\"Reports\":1,\"Roles\":[],\"AppVersion\":\"irure\",\"Flags\":2},{\"Name\":\"Kidd Mcfarland\",\"Company\":\"Pharmacon\",\"UserId\":\"7a3fd78a-f644-4b54-811b-90ee5492cb78\",\"GameId\":\"Room1\",\"Reports\":3,\"Roles\":[{\"id\":0,\"RoleType\":1},{\"id\":1,\"RoleType\":3}],\"AppVersion\":\"ea\",\"Flags\":0},{\"Name\":\"Alford Santiago\",\"Company\":\"Columella\",\"UserId\":\"b2bb1995-e1a8-43bf-9dd6-d3ce6f990614\",\"GameId\":\"Room2\",\"Reports\":1,\"Roles\":[{\"id\":0,\"RoleType\":3}],\"AppVersion\":\"labore\",\"Flags\":5},{\"Name\":\"Ina Lee\",\"Company\":\"Isotronic\",\"UserId\":\"174b9510-d919-4b8b-b529-db2d65aa19a6\",\"GameId\":\"Room2\",\"Reports\":6,\"Roles\":[{\"id\":0,\"RoleType\":2}],\"AppVersion\":\"culpa\",\"Flags\":4},{\"Name\":\"Church Salas\",\"Company\":\"Hairport\",\"UserId\":\"f9abab17-eee3-4579-b5b3-cea4047deb0f\",\"GameId\":\"Room1\",\"Reports\":4,\"Roles\":[{\"id\":0,\"RoleType\":1},{\"id\":1,\"RoleType\":2}],\"AppVersion\":\"amet\",\"Flags\":2},{\"Name\":\"Alyce Daugherty\",\"Company\":\"Vixo\",\"UserId\":\"f0a45a9d-5643-418d-92db-8154d81b12c6\",\"GameId\":\"Room2\",\"Reports\":7,\"Roles\":[{\"id\":0,\"RoleType\":3}],\"AppVersion\":\"ipsum\",\"Flags\":1},{\"Name\":\"Rivas Clay\",\"Company\":\"Geoforma\",\"UserId\":\"a472765d-e394-44c3-b2bb-4ed588745f1a\",\"GameId\":\"Chatbubble\",\"Reports\":10,\"Roles\":[{\"id\":0,\"RoleType\":3},{\"id\":1,\"RoleType\":3}],\"AppVersion\":\"commodo\",\"Flags\":4},{\"Name\":\"Deidre Young\",\"Company\":\"Glasstep\",\"UserId\":\"cb2004f3-7a2d-4c1e-ad0f-1acb26470f2d\",\"GameId\":\"Room1\",\"Reports\":4,\"Roles\":[{\"id\":0,\"RoleType\":1},{\"id\":1,\"RoleType\":2}],\"AppVersion\":\"est\",\"Flags\":4},{\"Name\":\"Maynard Lynch\",\"Company\":\"Cincyr\",\"UserId\":\"54362704-579b-47f3-a71d-aa479329b895\",\"GameId\":\"Room1\",\"Reports\":3,\"Roles\":[{\"id\":0,\"RoleType\":3},{\"id\":1,\"RoleType\":2}],\"AppVersion\":\"et\",\"Flags\":6},{\"Name\":\"Erna Berry\",\"Company\":\"Anarco\",\"UserId\":\"2823ebde-fb48-4e7b-bb2c-b2b6080bd007\",\"GameId\":\"Room1\",\"Reports\":5,\"Roles\":[{\"id\":0,\"RoleType\":1},{\"id\":1,\"RoleType\":1}],\"AppVersion\":\"qui\",\"Flags\":1},{\"Name\":\"Holmes Peters\",\"Company\":\"Pigzart\",\"UserId\":\"8c1eaa46-a424-4e39-af11-6758baca5c60\",\"GameId\":\"Chatbubble\",\"Reports\":10,\"Roles\":[{\"id\":0,\"RoleType\":3},{\"id\":1,\"RoleType\":3}],\"AppVersion\":\"exercitation\",\"Flags\":2},{\"Name\":\"Ilene Mcintyre\",\"Company\":\"Uniworld\",\"UserId\":\"c28078aa-4efe-4a90-9aad-9ec7665e7ffb\",\"GameId\":\"Chatbubble\",\"Reports\":0,\"Roles\":[{\"id\":0,\"RoleType\":1}],\"AppVersion\":\"fugiat\",\"Flags\":6},{\"Name\":\"Marilyn Wall\",\"Company\":\"Delphide\",\"UserId\":\"a2615564-b69b-4dd7-8888-449d5f37d472\",\"GameId\":\"Chatbubble\",\"Reports\":7,\"Roles\":[{\"id\":0,\"RoleType\":1},{\"id\":1,\"RoleType\":2}],\"AppVersion\":\"cupidatat\",\"Flags\":6},{\"Name\":\"Willa Whitaker\",\"Company\":\"Optyk\",\"UserId\":\"18aeaa42-5e16-49c9-be6d-ab246e440ad0\",\"GameId\":\"Room2\",\"Reports\":10,\"Roles\":[{\"id\":0,\"RoleType\":3}],\"AppVersion\":\"id\",\"Flags\":5},{\"Name\":\"Shannon Frederick\",\"Company\":\"Trasola\",\"UserId\":\"f11f23c0-577b-4453-bf2a-a87dcdbfc46d\",\"GameId\":\"Room2\",\"Reports\":0,\"Roles\":[{\"id\":0,\"RoleType\":3}],\"AppVersion\":\"veniam\",\"Flags\":7},{\"Name\":\"Bernice Rivera\",\"Company\":\"Cognicode\",\"UserId\":\"74853143-ee8a-4cb1-9a1e-a953789a2bcc\",\"GameId\":\"Chatbubble\",\"Reports\":6,\"Roles\":[{\"id\":0,\"RoleType\":2}],\"AppVersion\":\"occaecat\",\"Flags\":6},{\"Name\":\"Patrick Solis\",\"Company\":\"Zomboid\",\"UserId\":\"1c097ebb-6775-40c9-a9ba-fe211710b2d3\",\"GameId\":\"Room2\",\"Reports\":1,\"Roles\":[{\"id\":0,\"RoleType\":1}],\"AppVersion\":\"et\",\"Flags\":2},{\"Name\":\"Anderson Simmons\",\"Company\":\"Comverges\",\"UserId\":\"df2c9c5e-e41f-4bec-a8c9-82e240722de9\",\"GameId\":\"Room2\",\"Reports\":2,\"Roles\":[],\"AppVersion\":\"nisi\",\"Flags\":5},{\"Name\":\"Hays Knowles\",\"Company\":\"Furnigeer\",\"UserId\":\"34594924-a658-4e1a-86b4-3e854aa350c2\",\"GameId\":\"Room1\",\"Reports\":4,\"Roles\":[{\"id\":0,\"RoleType\":1}],\"AppVersion\":\"minim\",\"Flags\":3},{\"Name\":\"Barnes Clayton\",\"Company\":\"Quilch\",\"UserId\":\"62be9c8e-a71d-46d7-87ba-1f63e8bf515c\",\"GameId\":\"Chatbubble\",\"Reports\":4,\"Roles\":[{\"id\":0,\"RoleType\":2},{\"id\":1,\"RoleType\":2}],\"AppVersion\":\"deserunt\",\"Flags\":0},{\"Name\":\"Tracie Tucker\",\"Company\":\"Eweville\",\"UserId\":\"846f6269-2307-45e9-9ae8-dfe5663d1fc0\",\"GameId\":\"Room1\",\"Reports\":4,\"Roles\":[{\"id\":0,\"RoleType\":3},{\"id\":1,\"RoleType\":1}],\"AppVersion\":\"minim\",\"Flags\":3},{\"Name\":\"Ingram Carney\",\"Company\":\"Hotcakes\",\"UserId\":\"081df2f0-93d8-465b-9b67-2243c016eb53\",\"GameId\":\"Room2\",\"Reports\":9,\"Roles\":[{\"id\":0,\"RoleType\":1},{\"id\":1,\"RoleType\":3}],\"AppVersion\":\"adipisicing\",\"Flags\":4},{\"Name\":\"Alma Wolfe\",\"Company\":\"Eclipsent\",\"UserId\":\"88f12cc4-709e-439d-a2da-0502a9f24154\",\"GameId\":\"Chatbubble\",\"Reports\":7,\"Roles\":[],\"AppVersion\":\"ipsum\",\"Flags\":7},{\"Name\":\"Bridget Guthrie\",\"Company\":\"Tourmania\",\"UserId\":\"b0d7e075-3d4f-4214-9a01-6980f87be549\",\"GameId\":\"Room2\",\"Reports\":1,\"Roles\":[{\"id\":0,\"RoleType\":3}],\"AppVersion\":\"quis\",\"Flags\":2},{\"Name\":\"Julie Ramsey\",\"Company\":\"Zentury\",\"UserId\":\"7fd73d82-bb13-42f3-9245-a891b9dfd8f9\",\"GameId\":\"Chatbubble\",\"Reports\":10,\"Roles\":[{\"id\":0,\"RoleType\":1}],\"AppVersion\":\"id\",\"Flags\":2},{\"Name\":\"Cobb Bright\",\"Company\":\"Apexia\",\"UserId\":\"381e5faa-157e-4881-a49c-3d373f32bf67\",\"GameId\":\"Room1\",\"Reports\":6,\"Roles\":[{\"id\":0,\"RoleType\":2}],\"AppVersion\":\"sunt\",\"Flags\":2},{\"Name\":\"Bertha Medina\",\"Company\":\"Geekology\",\"UserId\":\"11849b5f-ca22-4b51-9661-8c196cf28a66\",\"GameId\":\"Room1\",\"Reports\":4,\"Roles\":[{\"id\":0,\"RoleType\":2},{\"id\":1,\"RoleType\":2}],\"AppVersion\":\"est\",\"Flags\":6},{\"Name\":\"Stone Vincent\",\"Company\":\"Accusage\",\"UserId\":\"104ef410-a1d1-4faa-9ae3-de24318ca6b0\",\"GameId\":\"Room2\",\"Reports\":4,\"Roles\":[{\"id\":0,\"RoleType\":1},{\"id\":1,\"RoleType\":1}],\"AppVersion\":\"nostrud\",\"Flags\":1},{\"Name\":\"Rios Harris\",\"Company\":\"Gleamink\",\"UserId\":\"6f4d051c-e50a-4796-8175-ba6dade21585\",\"GameId\":\"Room2\",\"Reports\":9,\"Roles\":[],\"AppVersion\":\"irure\",\"Flags\":3},{\"Name\":\"Riddle Barlow\",\"Company\":\"Buzzworks\",\"UserId\":\"55b3df07-ff08-455d-b166-ac1eb43ebfc4\",\"GameId\":\"Room2\",\"Reports\":1,\"Roles\":[{\"id\":0,\"RoleType\":3}],\"AppVersion\":\"cillum\",\"Flags\":3},{\"Name\":\"Petra Waters\",\"Company\":\"Pearlesex\",\"UserId\":\"cec3d9a5-3758-4682-893f-73ccba0e985a\",\"GameId\":\"Room2\",\"Reports\":3,\"Roles\":[],\"AppVersion\":\"ut\",\"Flags\":6},{\"Name\":\"Deana Kennedy\",\"Company\":\"Eyewax\",\"UserId\":\"11e37992-f776-4ef4-a779-77a8a8dae9ae\",\"GameId\":\"Room2\",\"Reports\":9,\"Roles\":[{\"id\":0,\"RoleType\":2},{\"id\":1,\"RoleType\":2}],\"AppVersion\":\"fugiat\",\"Flags\":6},{\"Name\":\"Lawson Williamson\",\"Company\":\"Chillium\",\"UserId\":\"a1d90c4e-cf40-4c34-8f27-2b7808d980d0\",\"GameId\":\"Room1\",\"Reports\":5,\"Roles\":[{\"id\":0,\"RoleType\":2},{\"id\":1,\"RoleType\":1}],\"AppVersion\":\"mollit\",\"Flags\":2},{\"Name\":\"Duncan Mcneil\",\"Company\":\"Kyaguru\",\"UserId\":\"45517ba2-04b8-449c-abfa-5c94740d940f\",\"GameId\":\"Chatbubble\",\"Reports\":1,\"Roles\":[{\"id\":0,\"RoleType\":2}],\"AppVersion\":\"non\",\"Flags\":7},{\"Name\":\"Elaine Hurley\",\"Company\":\"Calcu\",\"UserId\":\"0fc8f9c4-efe3-4ded-8db2-2f9212b852ed\",\"GameId\":\"Room2\",\"Reports\":7,\"Roles\":[{\"id\":0,\"RoleType\":3},{\"id\":1,\"RoleType\":3}],\"AppVersion\":\"quis\",\"Flags\":2},{\"Name\":\"Conner Mercado\",\"Company\":\"Namegen\",\"UserId\":\"fe59cfd6-1123-4925-8c15-c94dc58201d4\",\"GameId\":\"Room2\",\"Reports\":3,\"Roles\":[],\"AppVersion\":\"proident\",\"Flags\":7},{\"Name\":\"Campbell Nolan\",\"Company\":\"Inrt\",\"UserId\":\"f5273b6c-b2e7-4317-befe-dd78af4f6baf\",\"GameId\":\"Room2\",\"Reports\":5,\"Roles\":[{\"id\":0,\"RoleType\":1},{\"id\":1,\"RoleType\":1}],\"AppVersion\":\"dolor\",\"Flags\":2},{\"Name\":\"Clayton Morton\",\"Company\":\"Lotron\",\"UserId\":\"335eca95-e959-49b4-bce3-4d594c8ed78b\",\"GameId\":\"Chatbubble\",\"Reports\":9,\"Roles\":[{\"id\":0,\"RoleType\":1}],\"AppVersion\":\"non\",\"Flags\":6},{\"Name\":\"Kari Torres\",\"Company\":\"Terrago\",\"UserId\":\"d7cc0355-ee87-415f-abc9-94e48d131b9b\",\"GameId\":\"Room1\",\"Reports\":9,\"Roles\":[{\"id\":0,\"RoleType\":3}],\"AppVersion\":\"pariatur\",\"Flags\":1},{\"Name\":\"Arnold Sparks\",\"Company\":\"Concility\",\"UserId\":\"59cd3b89-5845-4377-a2c1-9dbc5193a5ba\",\"GameId\":\"Chatbubble\",\"Reports\":7,\"Roles\":[],\"AppVersion\":\"ad\",\"Flags\":5},{\"Name\":\"Janelle West\",\"Company\":\"Tersanki\",\"UserId\":\"0956e07c-a393-4ded-bfd5-b6ab6641cf5a\",\"GameId\":\"Chatbubble\",\"Reports\":9,\"Roles\":[],\"AppVersion\":\"aliqua\",\"Flags\":0},{\"Name\":\"Tanisha Griffith\",\"Company\":\"Cyclonica\",\"UserId\":\"cc8caafc-e159-4f7b-b9ec-3408e651d4fd\",\"GameId\":\"Room2\",\"Reports\":0,\"Roles\":[{\"id\":0,\"RoleType\":1},{\"id\":1,\"RoleType\":1}],\"AppVersion\":\"consequat\",\"Flags\":0},{\"Name\":\"Jeanette Johnston\",\"Company\":\"Duflex\",\"UserId\":\"65ac271f-dbce-4b9c-9411-e3297bd6edbf\",\"GameId\":\"Room1\",\"Reports\":0,\"Roles\":[{\"id\":0,\"RoleType\":3}],\"AppVersion\":\"mollit\",\"Flags\":3},{\"Name\":\"Kathie Rogers\",\"Company\":\"Nitracyr\",\"UserId\":\"0c60ff40-2f70-41a6-9f18-762344fa7785\",\"GameId\":\"Room2\",\"Reports\":0,\"Roles\":[{\"id\":0,\"RoleType\":3}],\"AppVersion\":\"sint\",\"Flags\":3},{\"Name\":\"Hopper Robles\",\"Company\":\"Kage\",\"UserId\":\"6e030c35-d3ed-41a7-b434-8e159ddf2f44\",\"GameId\":\"Chatbubble\",\"Reports\":4,\"Roles\":[],\"AppVersion\":\"incididunt\",\"Flags\":1},{\"Name\":\"Mccormick Singleton\",\"Company\":\"Songbird\",\"UserId\":\"080f40c4-8734-4c72-97dd-c83db777ed9b\",\"GameId\":\"Chatbubble\",\"Reports\":0,\"Roles\":[{\"id\":0,\"RoleType\":1},{\"id\":1,\"RoleType\":2}],\"AppVersion\":\"occaecat\",\"Flags\":3}]}";
        #endregion
    }
}