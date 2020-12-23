using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace SpaceHub.Conference
{
    public class ChatbubblePlayerCountDisplay : MonoBehaviour
    {
        Chatbubble m_Chatbubble;
        TextMeshPro m_Text;

        // Start is called before the first frame update
        void Awake()
        {
            m_Text = GetComponent<TextMeshPro>();

            m_Chatbubble = GetComponentInParent<Chatbubble>();
            m_Chatbubble.PlayersChangedCallback += OnPlayersChanged;
        }

        void OnPlayersChanged()
        {
            m_Text.text = m_Chatbubble.Players.Count + " / " + m_Chatbubble.GetMaximumPlayers();
        }
    }
}