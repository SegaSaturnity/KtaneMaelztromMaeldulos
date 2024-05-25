using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Quadrecta : MonoBehaviour {

    public KMBombModule module;
    public KMBombInfo bombInfo;
    public KMAudio audio;
	public KMColorblindMode colorblind;
	
	public KMSelectable[] buttons;
	public Light light;
	
	private bool isSolved;
	private static int moduleCount;
	private bool colorblindModeEnabled;
    private int moduleId;
	
	//Red, Blue, Green
	private static Color[] lightColours = new Color[]{
        new Color(0.8f, 0.05f, 0.05f), 
        new Color(0, 0.4f, 0.85f),      
		new Color(0, 0.7f, 0),             
    };
	
	private int[] ledColors = new int[4];
	
	private string[][] combinations = new string[][] {
		new string[] { "Okay", "Wait", "What", "When" },
		new string[] { "Okay", "Wait", "Now", "Where" },
		new string[] { "Okay", "What", "Stop", "Where" },
		new string[] { "Okay", "Now", "When", "Stop" },
		new string[] { "Wait", "What", "Now", "Stop" },
		new string[] { "Wait", "When", "Stop", "Where" },
		new string[] { "What", "Now", "When", "Where" },
	};
	
	private string[] words = new string[]{ "Okay", "Wait", "What", "Now", "When", "Stop", "Where" }; 
	
	private int[] table = new int[] {4, 7, 1, 8, 3, 2, 5, 9, 5, 3, 6, 1, 4, 8, 3, 8, 2, 5, 7, 9, 6};
	private int[] solves = new int[] {0,0,0,0};

	
	void Awake () {
		int rand = UnityEngine.Random.Range(0,6);
		moduleId = moduleCount++;
		string[] selected = combinations[rand];
		Debug.LogFormat("[Quadrecta #{0}] Module started. Buttons are: {1}", moduleId, string.Join(", ", selected));
		
		colorblindModeEnabled = colorblind.ColorblindModeActive;
		light.transform.parent.GetChild(1).GetComponent<MeshRenderer>().enabled = colorblindModeEnabled;
		
		//do the shuffle
		for (int t = 0; t < selected.Length; t++ )
        {
            string tmp = selected[t];
            int r = Random.Range(t, selected.Length);
            selected[t] = selected[r];
            selected[r] = tmp;
        }
		
		for (int i = 0; i < buttons.Length; i++){
			int j = i;
			buttons[j].OnInteract += delegate () { pressPos(j); return false; };
			buttons[j].OnInteractEnded += delegate () { releasePos(j, rand); };
			buttons[j].transform.GetChild(2).GetComponent<TextMesh>().text = selected[j].ToUpper();
			
			ledColors[j] = UnityEngine.Random.Range(0,3);
		}
	}
	
	void pressPos( int pos ) {
		StartCoroutine(animationButton(pos, 0.029f, 0.020f));
		audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, transform);
		
		if (ledColors[pos] == -1) return;
		light.color = lightColours[ledColors[pos]];
		light.transform.parent.GetComponent<MeshRenderer>().material.color = lightColours[ledColors[pos]];
		light.enabled = true;
		
		string[] n_colors = new string[] {"R", "B", "G"};
		light.transform.parent.GetChild(1).GetComponent<TextMesh>().text = n_colors[ledColors[pos]];
	}
	
	void releasePos( int pos, int rand ) {
		StartCoroutine(animationButton(pos, 0.020f, 0.029f));
		audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonRelease, transform);
		
		string hword = buttons[pos].transform.GetChild(2).GetComponent<TextMesh>().text;
		string rword = null;
		switch (hword.ToLower()) {
		case "okay":
			if (ledColors[pos] == 0) {
				switch (rand){
				case 0:
				case 2:
					rword = "What";
				break;
				case 1:
				case 3:
					rword = "Now";
				break;
				}					
			} else if (ledColors[pos] == 1) {
				switch (rand){
				case 0:
				case 1:
					rword = "Wait";
				break;
				case 2:
				case 3:
					rword = "Stop";
				break;
				}
			} else {
			switch (rand){
				case 0:
				case 3:
					rword = "When";
				break;
				case 1:
				case 2:
					rword = "Where";
				break;
				}
			}
		break;
		case "wait":
			if (ledColors[pos] == 0) {
				switch (rand){
				case 0:
				case 5:
					rword = "When";
				break;
				case 1:
				case 4:
					rword = "Now";
				break;
				}					
			} else if (ledColors[pos] == 1) {
				switch (rand){
				case 1:
				case 5:
					rword = "Where";
				break;
				case 0:
				case 4:
					rword = "What";
				break;
				}
			} else {
			switch (rand){
				case 4:
				case 5:
					rword = "Stop";
				break;
				case 0:
				case 1:
					rword = "Okay";
				break;
				}
			}
		break;
		case "what":
			if (ledColors[pos] == 0) {
				switch (rand){
				case 0:
				case 6:
					rword = "When";
				break;
				case 2:
				case 4:
					rword = "Stop";
				break;
				}					
			} else if (ledColors[pos] == 1) {
				switch (rand){
				case 0:
				case 2:
					rword = "Okay";
				break;
				case 4:
				case 6:
					rword = "Now";
				break;
				}
			} else {
			switch (rand){
				case 2:
				case 6:
					rword = "Where";
				break;
				case 0:
				case 4:
					rword = "Wait";
				break;
				}
			}
		break;
		case "now":
			if (ledColors[pos] == 0) {
				switch (rand){
				case 3:
				case 4:
					rword = "Stop";
				break;
				case 1:
				case 6:
					rword = "Where";
				break;
				}					
			} else if (ledColors[pos] == 1) {
				switch (rand){
				case 1:
				case 4:
					rword = "Wait";
				break;
				case 3:
				case 6:
					rword = "When";
				break;
				}
			} else {
			switch (rand){
				case 1:
				case 3:
					rword = "Okay";
				break;
				case 4:
				case 6:
					rword = "What";
				break;
				}
			}
		break;
		case "when":
			if (ledColors[pos] == 0) {
				switch (rand){
				case 5:
				case 6:
					rword = "Where";
				break;
				case 0:
				case 3:
					rword = "Okay";
				break;
				}					
			} else if (ledColors[pos] == 1) {
				switch (rand){
				case 0:
				case 6:
					rword = "What";
				break;
				case 3:
				case 5:
					rword = "Stop";
				break;
				}
			} else {
			switch (rand){
				case 0:
				case 5:
					rword = "Wait";
				break;
				case 3:
				case 6:
					rword = "Now";
				break;
				}
			}
		break;
		case "stop":
			if (ledColors[pos] == 0) {
				switch (rand){
				case 2:
				case 3:
					rword = "Okay";
				break;
				case 4:
				case 5:
					rword = "Wait";
				break;
				}					
			} else if (ledColors[pos] == 1) {
				switch (rand){
				case 3:
				case 4:
					rword = "Now";
				break;
				case 2:
				case 5:
					rword = "Where";
				break;
				}
			} else {
			switch (rand){
				case 2:
				case 4:
					rword = "What";
				break;
				case 3:
				case 5:
					rword = "When";
				break;
				}
			}
		break;
		case "where":
			if (ledColors[pos] == 0) {
				switch (rand){
				case 1:
				case 5:
					rword = "Wait";
				break;
				case 2:
				case 6:
					rword = "What";
				break;
				}					
			} else if (ledColors[pos] == 1) {
				switch (rand){
				case 1:
				case 2:
					rword = "Okay";
				break;
				case 5:
				case 6:
					rword = "When";
				break;
				}
			} else {
			switch (rand){
				case 1:
				case 6:
					rword = "Now";
				break;
				case 2:
				case 5:
					rword = "Stop";
				break;
				}
			}
		break;
		}
		
		light.enabled = false;
		light.transform.parent.GetComponent<MeshRenderer>().material.color = new Color(0.45f,0.45f,0.45f);
		
		light.transform.parent.GetChild(1).GetComponent<TextMesh>().text = "";
		
		if (solves[pos] == 1) return;
		
		int index = System.Array.IndexOf(words, rword);
		//i=y*W + x
		int time_check = table[ledColors[pos] * 7 + index];
		string colorn = null;
		switch (ledColors[pos]) {
		case 0:
			colorn = "Red";
		break;
		case 1:
			colorn = "Blue";
		break;
		case 2:
			colorn = "Green";
		break;
		}
		Debug.LogFormat("[Quadrecta #{0}] Button {1} pressed, held word: {2}, related word: {3}, LED color: {4}, expecting a {5} in any position", moduleId, pos, hword, rword, colorn, time_check);
		if (bombInfo.GetFormattedTime().Contains(time_check.ToString())) {
			Debug.LogFormat("[Quadrecta #{0}] Button {1} released at the correct time!", moduleId, pos);
			solves[pos] = 1;
			if (solves.Count(s => s == 0) == 0){
				Debug.LogFormat("[Quadrecta #{0}] Solved all 4 buttons, module solved!", moduleId);
				isSolved = true;
				module.HandlePass();
			}
			audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
			return;
		}
		
		Debug.LogFormat("[Quadrecta #{0}] Released at wrong time ({1}), causing a strike.", moduleId, bombInfo.GetFormattedTime());
		module.HandleStrike();
	}

    private IEnumerator animationButton(int pos,float a, float b)
    {
        var max = 0.05f;
        var cur = 0f;
        while (cur < max)
        {
            buttons[pos].transform.localPosition = new Vector3(buttons[pos].transform.localPosition.x, Easing.InOutQuad(cur, a, b, max), buttons[pos].transform.localPosition.z);
            yield return new WaitForSeconds(.03f);
            cur += Time.deltaTime;
        }
        buttons[pos].transform.localPosition = new Vector3(buttons[pos].transform.localPosition.x, b, buttons[pos].transform.localPosition.z);
    }

}
