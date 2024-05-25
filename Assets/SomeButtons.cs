using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class SomeButtons : MonoBehaviour {

    public KMBombModule module;
    public KMBombInfo bombInfo;
    public KMAudio audio;
	public KMColorblindMode colorblind;
	
	public KMSelectable[] buttons;
	
	private bool isSolved;
	private static int moduleCount;
	private bool colorblindModeEnabled;
    private int moduleId;
	
	//Red, Blue, Green, White, Black
	private static Color[] buttonColours = new Color[]{
        new Color(0.8f, 0.05f, 0.05f), 
        new Color(0, 0.4f, 0.85f),      
		new Color(0, 0.7f, 0),          
        new Color(0.9f, 0.88f, 0.86f),  
        new Color(0.2f, 0.2f, 0.2f)     
    };
	
	private int[] buttonSequence = new int[12]; 
	private int[] answerSequence = new int[]{0,0,0,0,0,0,0,0,0,0,0,0}; 
	

    void Awake() {
		string[] n_colors = new string[] {"R", "B", "G", "W", "K"};
		
        moduleId = moduleCount++;
		colorblindModeEnabled = colorblind.ColorblindModeActive;	
		
		for (int i = 0; i < 12; i++){
			int j = i;
			float rnd = UnityEngine.Random.value;
			if (rnd < 0.12f){
				rnd = 3;
			} else if (rnd < 0.23f) {
				rnd = 4;
			} else if (rnd < 0.45f) {
				rnd = 1;
			} else if (rnd < 0.67f) {
				rnd = 2;
			} else {
				rnd = 0;
			}
			buttonSequence[j] = (int)rnd;
			buttons[j].OnInteract += delegate () { pressPos(j); return false; };
			buttons[j].GetComponent<MeshRenderer>().material.color = buttonColours[buttonSequence[j]];
			buttons[j].transform.GetChild(2).GetComponent<MeshRenderer>().enabled = colorblindModeEnabled;
			buttons[j].transform.GetChild(2).GetComponent<TextMesh>().text = n_colors[buttonSequence[j]];
		}
    }

	void Start () {
		
		if(bombInfo.GetSerialNumberLetters().Any(x => x == 'A' || x == 'E' || x == 'I' || x == 'O' || x == 'U')){
			Debug.LogFormat("[Some Buttons #{0}] Vowel in serial, press 3", moduleId);
			answerSequence[2] = 1;
		}
		if(bombInfo.GetSerialNumberNumbers().Any(x => x > 6)){
			Debug.LogFormat("[Some Buttons #{0}] Digit higher than 6 in serial, press 12", moduleId);
			answerSequence[11] = 1;
		}
		if(bombInfo.IsPortPresent(Port.Parallel)){
			Debug.LogFormat("[Some Buttons #{0}] Parallel port found, 6", moduleId);
			answerSequence[5] = 1;
		}
		if(bombInfo.GetBatteryCount(Battery.AA) + bombInfo.GetBatteryCount(Battery.AAx3) + bombInfo.GetBatteryCount(Battery.AAx4) >= 1){
			Debug.LogFormat("[Some Buttons #{0}] AA found, press 10", moduleId);
			answerSequence[9] = 1;
		}
		if(bombInfo.GetBatteryCount(Battery.D) >= 1){
			Debug.LogFormat("[Some Buttons #{0}] D found, press 5", moduleId);
			answerSequence[4] = 1;
		}
		if(bombInfo.IsPortPresent(Port.PS2)){
			Debug.LogFormat("[Some Buttons #{0}] PS/2 port found, press 8", moduleId);
			answerSequence[7] = 1;
		}
		if(buttonSequence.Count(s => s == 0) >= 4){
			Debug.LogFormat("[Some Buttons #{0}] 4+ red buttons, press 4 and 7", moduleId);
			answerSequence[3] = 1;
			answerSequence[6] = 1;
		}
		if(buttonSequence.Count(s => s == 1) == buttonSequence.Count(s => s == 2)){
			Debug.LogFormat("[Some Buttons #{0}] Same amount green and blue buttons, press 9 and 10", moduleId);
			answerSequence[8] = 1;
			answerSequence[9] = 1;
		}
		if(buttonSequence.Count(s => s == 4) % 2 != 0){
			Debug.LogFormat("[Some Buttons #{0}] Amount black buttons is odd, press 1 and 3", moduleId);
			answerSequence[0] = 1;
			answerSequence[2] = 1;
		}
		if(buttonSequence.Count(s => s == 0) != 1 && buttonSequence.Count(s => s == 1) != 1 && buttonSequence.Count(s => s == 2) != 1 && buttonSequence.Count(s => s == 3) != 1 && buttonSequence.Count(s => s == 4) != 1){
			Debug.LogFormat("[Some Buttons #{0}] No unique colors, press 2 and 11", moduleId);
			answerSequence[1] = 1;
			answerSequence[10] = 1;
		}
		if(buttonSequence.Distinct().ToArray().Length >= 5){
			Debug.LogFormat("[Some Buttons #{0}] 5 different colors present, press 1 and 7", moduleId);
			answerSequence[0] = 1;
			answerSequence[6] = 1;
		}
		
		if(!buttonSequence.Contains(3)){
			Debug.LogFormat("[Some Buttons #{0}] No white buttons, press 6 and 8", moduleId);
			answerSequence[5] = 1;
			answerSequence[7] = 1;
		}
		
	}
	
	void pressPos( int pos ){
		if (isSolved) return;
		
		audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		buttons[pos].AddInteractionPunch();
		if (answerSequence[pos] == 0) {
			Debug.LogFormat("[Some Buttons #{0}] Pressed position {1}, causing a strike.", moduleId, pos+1);
			module.HandleStrike();
		} else {
			answerSequence[pos] = 2;
			if (answerSequence.Count(s => s == 1) == 0){
				Debug.LogFormat("[Some Buttons #{0}] Pressed all valid positions, module solved!", moduleId);
				audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
				isSolved = true;
				module.HandlePass();
			}
		}
	}
}