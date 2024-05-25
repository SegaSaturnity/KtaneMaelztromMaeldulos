using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class QnA : MonoBehaviour {

    public KMBombModule module;
    public KMBombInfo bomb;
    public KMAudio audio;
	
	private bool isSolved;
	private static int moduleCount;
    private int moduleId;
	
	private string[] questions = new string[] {"WHAT", "WHEN", "WHERE", "WHO", "HOW", "WHY"};
	private string[] answers = new string[] {"THAT", "THEN", "THERE", "THEM", "THEY", "THIS", "THESE", "THOSE"};
	private int[] table = new int[] {6, 4, 2, 3, 1, 7, 5, 6, 3, 0, 4, 2, 1, 0, 5, 7, 3, 6, 4, 1, 7, 2, 0, 5, 7, 5, 0, 1, 6, 3, 3, 2, 4, 6, 7, 1, 2, 7, 1, 4, 5, 0, 0, 3, 6, 5, 2, 4};
	
	private string[] c_questions;
	private string[] c_answers;
	private string previous;
	
	public KMSelectable[] b_answers;
	public TextMesh display_text;

	void Start () {
		moduleId = moduleCount++;
		Debug.LogFormat("[Q&A #{0}] Module started.", moduleId);
		
		c_answers = (string[])answers.Clone();
		c_questions = (string[])questions.Clone();
		
		Shuffle(questions);
		Shuffle(answers);
		
		Debug.LogFormat("[Q&A #{0}] Starting board: {1}", moduleId, string.Join(", ", answers));
		Debug.LogFormat("[Q&A #{0}] First press should be {1}", moduleId, answers[7]);
		
		foreach (KMSelectable button in b_answers){
			int i = System.Array.IndexOf(b_answers, button);
			button.transform.GetChild(0).GetComponent<TextMesh>().text = answers[i];
			button.OnInteract += delegate () { pressAnswer(i); return false; };
		}
		
	}

	private void pressAnswer ( int pos ) {
		if (isSolved) return;
		
		b_answers[pos].AddInteractionPunch();
		string ans_press = b_answers[pos].transform.GetChild(0).GetComponent<TextMesh>().text;
		
		if (display_text.text == "" && pos != 7) {
			Debug.LogFormat("[Q&A #{0}] First press should be bottom right. Strike.", moduleId);
			module.HandleStrike();
			return;
		}	
		else if (display_text.text != "") {
			int y = System.Array.IndexOf(c_answers, previous);
			int x = System.Array.IndexOf(c_questions, display_text.text);
			int z = System.Array.IndexOf(c_answers, ans_press);
			
			if (table[y * 6 + x] != z) {
				Debug.LogFormat("[Q&A #{0}] Pressed {1}, expected {2} on question {3}", moduleId, ans_press, c_answers[table[y * 6 + x]], display_text.text);
				module.HandleStrike();
				return;
			}
			Debug.LogFormat("[Q&A #{0}] Pressed {1}, on question {2}, correct.", moduleId, ans_press, display_text.text);
		}
		
		audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
		if (questions.Length == 0) {
			Debug.LogFormat("[Q&A #{0}] Answered all the questions, module solved!", moduleId);
			isSolved = true;
			module.HandlePass();
			return;
		}
		display_text.text = questions[0];
		questions = questions.Skip(1).ToArray();
		previous = ans_press;
	}

	private void Shuffle( string[] arr ) {
		for (int t = 0; t < arr.Length; t++ )
        {
            string tmp = arr[t];
            int r = Random.Range(t, arr.Length);
            arr[t] = arr[r];
            arr[r] = tmp;
        }
	}
}
