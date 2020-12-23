using UnityEngine;
using System.Collections;

public class Spinner : MonoBehaviour 
{
    RectTransform m_Transform;
    [SerializeField] float Speed = 1f;

    private void Awake()
    {
        m_Transform = GetComponent<RectTransform>();
    }

    void Update() 
    {
        m_Transform.localRotation = Quaternion.Euler( 0, 0, -Time.realtimeSinceStartup * 360f * Speed );
    }
}
