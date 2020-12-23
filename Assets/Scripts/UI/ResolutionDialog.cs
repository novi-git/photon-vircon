using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SpaceHub.Conference
{
    public class ResolutionDialog : MonoBehaviour
    {
        public string NextSceneName;
        public GameObject DialogObject;
        public TextMeshProUGUI DebugText;
        public Button[] QualityButtons;
        public Color SelectedColor;
        public Color NormalColor;

        public UiFunnelManager Funnel;

        // Start is called before the first frame update
        void Start()
        {
            
#if UNITY_ANDROID
            Debug.Log("Android Resolution Dialog");
            //GotoNextScene();
#else
            if( DialogObject != null )
            {
                DialogObject.SetActive( true );
            }
            Application.targetFrameRate = 60;
#endif
            
            UpdateDebugText();
            UpdateQualityButtons();
            SetQualityLevel(0);
#if UNITY_WEBGL
            Debug.Log("WebGL Resolution Dialog");
            Funnel = GetComponentInParent<UiFunnelManager>();
            
#endif
        }

        private void Update()
        {
            if( Screen.width < 640 || Screen.height < 480 )
            {
                Screen.SetResolution( 640, 480, false );
            }

            UpdateDebugText();
        }

        void UpdateQualityButtons()
        {
            for( int i = 0; i < QualityButtons.Length; ++ i)
            {
                var colors = QualityButtons[ i ].colors;
                colors.normalColor = QualitySettings.GetQualityLevel() == i ? SelectedColor : NormalColor;
                colors.selectedColor = QualitySettings.GetQualityLevel() == i ? SelectedColor : NormalColor;
                QualityButtons[ i ].colors = colors;
            }
        }
        void UpdateDebugText()
        {
            if( DebugText == null )
            {
                return;
            }

            StringBuilder str = new StringBuilder();
            str.AppendLine( "Debug Information" );
            str.AppendLine();

            str.AppendLine( "Screen Resolution" );
            str.AppendLine( $"{Screen.width} x {Screen.height}" );
            str.AppendLine();

            str.AppendLine( "Rendering Resolution" );
            str.AppendLine( $"{Display.main.renderingWidth} x {Display.main.renderingHeight}" );
            str.AppendLine();

            str.AppendLine( "System Resolution" );
            str.AppendLine( $"{Display.main.systemWidth} x {Display.main.systemHeight}" );
            str.AppendLine();

            DebugText.text = str.ToString();
        }

        public void GotoNextScene()
        {
            Debug.Log("Load Scene " + NextSceneName);
            SceneManager.LoadScene( NextSceneName );
        }

        public void SetQualityLevel( int level )
        {
            QualitySettings.SetQualityLevel( level, true );

#if UNITY_ANDROID
            Application.targetFrameRate = 72;
#else
            Application.targetFrameRate = 60;
#endif
            UpdateQualityButtons();
        }

        public void SetResolution( float percentage )
        {
            int width = (int)( Display.main.systemWidth * percentage );
            int height = (int)( Display.main.systemHeight * percentage );
            FullScreenMode fullScreenMode = FullScreenMode.Windowed;

            if( percentage == 1f )
            {
                fullScreenMode = FullScreenMode.ExclusiveFullScreen;
            }

            Screen.SetResolution( width, height, fullScreenMode );
        }
    }
}
