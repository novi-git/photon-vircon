using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="TriviaQuestions", menuName = "Trivia/Questions")]
public class TriviaObject : ScriptableObject
{
    public Trivia[] questions;
}
