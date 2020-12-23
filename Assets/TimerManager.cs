using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace SpaceHub.Conference
{
    public class TimerManager : MonoBehaviour
    {
         
        StageHandlerBase m_Connection; 
         
        public GameObject TimerLoginPanel;
        public GameObject TimerControlPanel;


        public TextMeshPro TimerTextMesh; 
        public TMP_InputField PassInput;
        public TMP_InputField TimerControlInputField;
        public TMP_Text TimerControlText;
        public TMP_Text TimerStartButtonText;


        private int totalSeconds = 0;  

        private string timerPass = "hope";
        private string timeText = ""; 

        private bool isTimerLoggedIn = false;  
        private bool isTimerFinished = true; 
        private bool isTimerStarted = false;
        private bool isTimerPause = false;

        private void Awake()
        {
            TimerLoginPanel.SetActive(false);
            TimerControlPanel.SetActive(false);

            m_Connection = transform.parent.GetComponentInChildren<StageHandlerBase>();
            m_Connection.TimerCallback += OnGetTimerUpdate;
        }

        
        private void OnDestroy()
        {
            m_Connection.TimerCallback -= OnGetTimerUpdate;
        }


        // Start is called before the first frame update

        void Start()
        {
            

            int seconds = totalSeconds % 60;
            int minutes = totalSeconds / 60;
            timeText = minutes.ToString("00") + ":" + seconds.ToString("00"); 
            TimerTextMesh.text = timeText;

             //StartCoroutine(Countdown(10));
        }

        public void UpdateTimerHostText(){
             
            int secondsInputText;

            if(TimerControlInputField.text == "")
            { 
                TimerControlInputField.text = "0";
            } 

            secondsInputText =  int.Parse(TimerControlInputField.text);
            int seconds = secondsInputText % 60;
            int minutes = secondsInputText / 60;
            timeText = minutes.ToString("00") + ":" + seconds.ToString("00"); 
            TimerTextMesh.text = timeText; 
            TimerControlText.text = timeText;
        }

        public void ShowLogInTimer(bool show){

            if(isTimerLoggedIn || !isTimerFinished)
                return;

            TimerLoginPanel.SetActive(show);
            PassInput.text = "";
        }
        public void LogInTimer(){
            // Type in HOST password
            TimerLoginPanel.SetActive(false); // Hide

            if(isTimerLoggedIn)
            return;

         //      if(PassInput.text == timerPass){
                //Debug.Log("Password Correct!"); 
                isTimerLoggedIn = true;
                TimerControlPanel.SetActive(true); // Show the Timer Controls  
          //  } else{
                //Debug.Log("Password Incorrect!");
             //   isTimerLoggedIn = false;
           // }
        }

        

        void OnGetTimerUpdate(int timer){
            isTimerFinished = false; // Prevent Other User to Touch the Timer Button! 
            totalSeconds = timer;

            int seconds = timer % 60;
            int minutes = timer / 60;
            timeText = minutes.ToString("00") + ":" + seconds.ToString("00"); 
            TimerTextMesh.text = timeText;
            TimerControlText.text = timeText;

            if(seconds == 0 && minutes == 0){ 
                isTimerFinished = true;
                isTimerStarted = false;
                isTimerPause = false; 
                TimerControlInputField.interactable = true; 
            }

          //  Debug.Log("Is Timer Finished: " + isTimerFinished);
        }

        public void StartTimer(){

            if(TimerControlInputField.text == "" || TimerControlInputField.text == "0")
            return;
            
             if(isTimerStarted){
         
                if(isTimerPause == false){
                    // Pause Timer
                    TimerStartButtonText.text = "RESUME";
                    isTimerPause = true;   
                    StopAllCoroutines(); // Stop Timer Courotine
                    
                }else
                {
                    // Resume Timer
                    TimerStartButtonText.text = "STOP";
                    isTimerPause = false;
                    StartCoroutine(Countdown(totalSeconds));  
                }
            
            }

            // Start timer from the input field
            if(isTimerFinished == true){
                print("Timer Start!");
                isTimerStarted = true;
                TimerStartButtonText.text = "STOP";
                int secondsInputText =  int.Parse(TimerControlInputField.text); 
                TimerControlInputField.interactable = false;
                StartCoroutine(Countdown(secondsInputText)); 
            }

           
            
        }

        public void ResetTimer(){
            // Reset stops the timer and set it to 0 send to ALL
            print("Timer Reset!"); 
            TimerStartButtonText.text = "START"; 
            StopAllCoroutines(); 
            totalSeconds = 0;
            isTimerStarted = false;
            isTimerPause = false; 
            SendTimerUpdate(totalSeconds);
        }

        IEnumerator Countdown (int seconds) {

            isTimerFinished = false;
            totalSeconds = seconds;

            while (totalSeconds > 0) {
                yield return new WaitForSeconds (1);
                totalSeconds--; 
                SendTimerUpdate(totalSeconds);
            }
           // DoStuff ();  
           //  Debug.Log("Is Timer Finished: " + isTimerFinished);
        }

        
        private void SendTimerUpdate (int timer){
             m_Connection.UpdateTimer( timer );
        }
        
    }
}
