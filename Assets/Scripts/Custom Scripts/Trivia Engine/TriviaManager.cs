using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TriviaManager : MonoBehaviour
{
    // Start is called before the first frame update
    public TextMeshProUGUI QuestionText;

    public Button[] choices;

    public TriviaObject triviaQuestions;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI itemsNumberText;

    public TextMeshProUGUI endGameScoreText;

    public TextMeshProUGUI timerText;
    public float timeDelay = 15f;

    public Image meter;

    public ColorBlock WrongAnswer;
    public ColorBlock RightAnswer;

    public GameObject EndGamePanel;
    public GameObject GamePanel;
    public GameObject StartGamePanel;

    private ColorBlock standard;

    private int score;
    private int count;
    private int total;
    private System.Random random = new System.Random();
    private bool isGameEnded = false;

    List<Trivia> questions;

    private void Start() {
        EndGamePanel.SetActive(false);
        GamePanel.SetActive(false);
        StartGamePanel.SetActive(true);
    }

    [ContextMenu("Start Game")]
    public void GenerateQuiz() {
        StartGamePanel.SetActive(false);
        GamePanel.SetActive(true);
        standard = choices[0].colors;
        questions = new List<Trivia>();
        questions.Clear();
        questions = ShuffleQuiz();  
        score = 0;
        total = questions.Count;
        count = 1;
        scoreText.text = $"Score: {score}";
        itemsNumberText.text = $"{count} of {total}";
        FormulateQuestion(questions, count);
    }

    private void FormulateQuestion(List<Trivia> questions, int count) {
        
        meter.fillAmount = 0;
        Trivia question = questions[count - 1];
        QuestionText.text = question.Question;
        for(int i = 0; i < question.Options.Length; i++) {
            choices[i].GetComponentInChildren<TextMeshProUGUI>().text = question.Options[i];
            if(question.correctAnswer == i) {
                choices[i].onClick.AddListener(CorrectAnswer);
                choices[i].colors = RightAnswer;
            } else {
                choices[i].onClick.AddListener(IncorrectAnswer);
                choices[i].colors = WrongAnswer;
            }
        }
        StartCoroutine(QuestionTimer());
    }

    private List<Trivia> ShuffleQuiz() {
        if(triviaQuestions == null) {
            Debug.LogError("No Trivia Questions Available");
            return null;
        }
        List<Trivia> triviaShuffle = triviaQuestions.questions.ToList();

        int n = triviaShuffle.Count;
        while (n > 1) {
            n--;
            int k = random.Next(n + 1);
            Trivia value = triviaShuffle[k];
            triviaShuffle[k] = triviaShuffle[n];
            triviaShuffle[n] = value;
        }
        return triviaShuffle;
    }

    void NextQuestion() {
        Debug.Log("I'm being called");
        StopAllCoroutines();
        count++;

        foreach (Button b in choices) {
            b.interactable = true;
            b.onClick.RemoveAllListeners();
            b.colors = standard;
        }

        if (count <= total) {
            itemsNumberText.text = $"{count} of {total}";
            FormulateQuestion(questions, count);
        } else {
            GameEnded();
        }
    }

    void IncorrectAnswer() {              
        StartCoroutine(Display(NextQuestion));
    }

    void CorrectAnswer() {
        score++;
        scoreText.text = $"Score: {score}";
        StartCoroutine(Display(NextQuestion));
    }

    void GameEnded() {
        //Display Score and Game Ended Scene
        EndGamePanel.SetActive(true);
        endGameScoreText.text = score.ToString("00");
        GamePanel.SetActive(false);
    }

    private IEnumerator QuestionTimer() {        
        float tempDelay = 0f;
        while(tempDelay < timeDelay) {
            tempDelay += Time.deltaTime;
            timerText.text = (timeDelay - tempDelay).ToString("00");
            yield return null;
            meter.fillAmount = tempDelay / timeDelay;            
        }
        IncorrectAnswer();
    }

    private IEnumerator Display(System.Action actionToDoPostAnimation) {
        foreach(Button b in choices) {
            b.interactable = false;
        }
        yield return new WaitForSeconds(1f);
        actionToDoPostAnimation.Invoke();
    }
}
