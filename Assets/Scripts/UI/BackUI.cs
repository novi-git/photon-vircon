using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

namespace SpaceHub.Conference
{

    public class BackUI : MonoBehaviour
    {
        public static BackUI Instance;
        public TextMeshProUGUI Text;
        public GameObject Parent;

        struct BackStackData
        {
            public string Key;
            public string Text;
            public UnityAction Action;
        }

        Stack<BackStackData> m_Data = new Stack<BackStackData>();

        private void Awake()
        {
            Instance = this;
            SetVisualsToLastInStack();
        }

        public void AddBackData( UnityAction action, string text = "", string key = "NO_STACK_KEY" )
        {
            while( m_Data.Count > 0 &&
                ( m_Data.Peek().Key == key || m_Data.Peek().Key == "NO_STACK_KEY" )
                )
            {
                m_Data.Pop();
            }

            m_Data.Push( new BackStackData() { Action = action, Text = text, Key = key } );
            SetVisualsToLastInStack();
        }

        public void OnClick()
        {
            if( m_Data.Count == 0 )
            {
                return;
            }

            var data = m_Data.Pop();
            data.Action?.Invoke();

            SetVisualsToLastInStack();
        }

        public void RemoveLast()
        {
            if( m_Data.Count > 0 )
            {
                m_Data.Pop();
            }
            SetVisualsToLastInStack();
        }

        void SetVisualsToLastInStack()
        {
            bool visible = m_Data.Count > 0;
            Parent.SetActive( visible );

            if( visible )
            {
                Text.text = m_Data.Peek().Text;
            }
        }
    }
}