using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class Delumination : MonoBehaviour {
	
    public KMBombModule module;
    public KMBombInfo bombInfo;
    public KMAudio audio;
	public KMColorblindMode colorblind;
    public KMRuleSeedable rs;
	
	private bool isSolved;
	private bool colorblindModeEnabled;
	private static int moduleCount;
    private int moduleId;
	
	public KMSelectable[] switches;
	public GameObject[] lights;
	
	private int[] dirs = new int[] {1, 1, 1, 1};
	private int[] s_int = new int[4];
	private int[] l_int = new int[4];
	private bool flipping = false;
	private int black_l = 4;
	
	private string[] table = { "042135", "154203", "204513", "310254", "105342", "432510", "304251", "235401", "520431", "341205", "245103", "051423", "354210", "512034", "145023", "453102", "410253", "043512", "530142", "203145" };
	private string answer_u;
	private string answer_d;
	
	public Texture2D[] lightColoursText;
	public Texture2D[] lightColoursTextOff;
	
	private bool turn_off = false;

	//Red, Blue, Green, White, Black, Yellow
	private static Color[] switchColours = new Color[]{
        new Color(0.80f, 0.24f, 0.24f), 
        new Color(0.18f, 0.54f, 0.90f),      
		new Color(0.21f, 0.70f, 0.35f),    
		new Color(0.90f, 0.90f, 0.90f),	
        new Color(0.075f, 0.075f, 0.075f), 	
		new Color(0.80f, 0.80f, 0.24f),		
    };

	void Start () {
		string[] color_names = new string[] {"Red", "Blue", "Green", "White", "Black", "Yellow"};
		int[] order_extended = {5, 4, 2, 1, 3};
		//Gen switch and light colors
		s_int = GenCols();
		string colord = "";
		
		float scalar = transform.lossyScale.x;
		for (var n = 0; n < lights.Length; n++)	{
			lights[n].transform.Find("Point light").GetComponent<Light>().range *= scalar;
		}
		
		var RND = rs.GetRNG();
		if(RND.Seed != 1){
			string[] color_order = {"0", "1", "2", "3", "4", "5"};
		
			RND.ShuffleFisherYates(order_extended);
			
		    for(var i = 0; i < 5; i++){
                for(var j = 0; j < 4; j++){
					RND.ShuffleFisherYates(color_order);
                    table[i*4 + j] = System.String.Join("", color_order);
                }
			}
		}
		
		foreach (KMSelectable s in switches) {
			int i = System.Array.IndexOf(switches, s);
			s.GetComponent<MeshRenderer>().material.color = switchColours[s_int[i]];
			s.OnInteract += delegate () { pressFlip(i); return false; };
			colord += color_names[s_int[i]];
			string cb_text = color_names[s_int[i]][0].ToString();
			if (s_int[i] == 4) cb_text = "K";
			if (colorblindModeEnabled) s.transform.parent.Find("ColorText").GetComponent<TextMesh>().text = cb_text;
			if ( i < 3 ) colord += ", ";
		}
		Debug.LogFormat("[Delumination #{0}] Switches are: {1}", moduleId, colord);
		
		l_int = GenCols();
		colord = "";
		
		foreach (GameObject l in lights) {
			int i = System.Array.IndexOf(lights, l);
			l.transform.Find("Light Text").GetComponent<MeshRenderer>().material.mainTexture = lightColoursTextOff[l_int[i]];
			l.transform.Find("Point light").GetComponent<Light>().color = switchColours[l_int[i]];
			string cb_text = color_names[l_int[i]][0].ToString();
			if (l_int[i] == 4) {
				black_l = 0;
				int loop = 0;
				while (l_int.Contains(black_l)) {
					black_l = Random.Range(0, 5);
					l.transform.Find("Light Text").GetComponent<MeshRenderer>().material.mainTexture = lightColoursTextOff[black_l];
					loop++;
					if (loop > 99) {
						black_l = 4;
						break;
					}
				}
				cb_text = color_names[black_l][0].ToString();
				if (black_l == 4) cb_text = "K";
			}
			if (colorblindModeEnabled) l.transform.Find("ColorText").GetComponent<TextMesh>().text = cb_text;
			colord += color_names[l_int[i]];
			if ( i < 3 ) colord += ", ";
		}
		Debug.LogFormat("[Delumination #{0}] Lights are: {1}", moduleId, colord);
		
		int row = bombInfo.GetSerialNumberNumbers().Max();
		if (RedNextTo("switch", order_extended[1])) {
			Debug.LogFormat("[Delumination #{0}] {1} switch is next to red, row is the lowest serial number.", moduleId, color_names[order_extended[1]]);
			row = bombInfo.GetSerialNumberNumbers().Min();
		}
		
		if (row < 4) row = 5;
		else if (row < 7) row = 6;
		row -= 5;
		
		int col = System.Array.IndexOf(s_int, 0);
		if (RedNextTo("switch", order_extended[0])) {
			Debug.LogFormat("[Delumination #{0}] {1} switch is next to red, position of {1} is the column.", moduleId, color_names[order_extended[0]]);
			col = System.Array.IndexOf(s_int, order_extended[0]);
		}
		if (s_int.Contains(order_extended[2])) {
			Debug.LogFormat("[Delumination #{0}] {1} switch is present, switches are right to left.", moduleId, color_names[order_extended[2]]);
			col = (col - 3) * -1;
		}
		answer_u = table[ row * 4 + col ];
		for (int i = 0; i < 6; i++) {
			if ( System.Array.IndexOf(s_int, i) == -1 ) answer_u = answer_u.Replace(i.ToString(), string.Empty);
		}
		
		row = bombInfo.GetSerialNumberNumbers().Max();
		if (RedNextTo("light", order_extended[1])) {
			Debug.LogFormat("[Delumination #{0}] {1} light is next to red, row is the lowest serial number.", moduleId, color_names[order_extended[1]]);
			row = bombInfo.GetSerialNumberNumbers().Min();
		}
		
		if (row < 3) row = 5;
		else if (row < 7) row = 6;
		row -= 5;
		
		col = System.Array.IndexOf(l_int, 0);
		if (RedNextTo("light", order_extended[0])) {
			Debug.LogFormat("[Delumination #{0}] {1} light is next to red, position of {1} is the column.", moduleId, color_names[order_extended[0]]);
			col = System.Array.IndexOf(l_int, 5);
		}
		if (l_int.Contains(order_extended[2])) {
			Debug.LogFormat("[Delumination #{0}] {1} light is present, lights are right to left.", moduleId, color_names[order_extended[2]]);
			col = (col - 3) * -1;
		}
		answer_d = table[ row * 4 + col ];
		for (int i = 0; i < 6; i++) {
			if ( System.Array.IndexOf(l_int, i) == -1 || i == 4 ) answer_d = answer_d.Replace(i.ToString(), string.Empty);
		}

		if (!bombInfo.GetSerialNumberLetters().Any(x => "AEIOU".Contains(x))) {
			Debug.LogFormat("[Delumination #{0}] No vowels, time to reverse the sequence.", moduleId);
			answer_u = new string(answer_u.Reverse().ToArray());
			answer_d = new string(answer_d.Reverse().ToArray());
		}
		if (!s_int.Contains(order_extended[3])) {
			Debug.LogFormat("[Delumination #{0}] No {1} switches, Red is on last.", moduleId, color_names[order_extended[3]]);
			answer_u = answer_u.Replace("0", string.Empty);
			answer_u += "0";
		}
		if (!l_int.Contains(order_extended[3])) {
			Debug.LogFormat("[Delumination #{0}] No {1} lights, Red is off first.", moduleId, color_names[order_extended[3]]);
			answer_d = answer_d.Replace("0", string.Empty);
			answer_d = "0" + answer_d;
		}
		
		string debug_a = "";
		foreach (char c in answer_u) {
			debug_a += color_names[(c - '0')] + " ";
		}
		Debug.LogFormat("[Delumination #{0}] Sequence to turn on is: {1}", moduleId, debug_a);
		
		debug_a = "";
		foreach (char c in answer_d) {
			debug_a += color_names[(c - '0')] + " ";
		}
		Debug.LogFormat("[Delumination #{0}] Sequence to turn off is: {1}", moduleId, debug_a);
	}
	
	int[] GenCols () {
		int[] cols = {1,2,3,4,5};
		shuffle(cols);
		System.Array.Resize<int>(ref cols, cols.Length - 1);
		cols[0] = 0;
		shuffle(cols);
		return cols;
	}
	
	void Awake () {
		moduleId = moduleCount++;
		colorblindModeEnabled = colorblind.ColorblindModeActive;
	}
	
	void pressFlip(int pos) {
		if (flipping || isSolved) return;
		Debug.LogFormat("[Delumination #{0}] Pressed position {1}.", moduleId, pos+1);
		if (l_int[pos] == 4 && dirs[pos] == -1) {
			Debug.LogFormat("[Delumination #{0}] Black should not be turned off. Strike.", moduleId);
			module.HandleStrike();                                 
			return;                                                
		}                                                          
		if (!turn_off && dirs[pos] == -1) {                        
			Debug.LogFormat("[Delumination #{0}] Tried to turn off before turning all on. Strike.", moduleId);
			module.HandleStrike();                                 
			return;                                                
		}                                                          
		if (turn_off && dirs[pos] == 1) {                          
			Debug.LogFormat("[Delumination #{0}] Tried to turn on before turning all off. Strike.", moduleId);
			module.HandleStrike();
			return;
		}
		
		if (!turn_off) {
			if (s_int[pos].ToString() != answer_u[dirs.Count(c => c == -1)].ToString()) {
				module.HandleStrike();
				Debug.LogFormat("[Delumination #{0}] Wrong position, strike.", moduleId);
				return;
			}
		} else {
			if (l_int[pos].ToString() != answer_d[dirs.Count(c => c == 1)].ToString()) {
				module.HandleStrike();
				Debug.LogFormat("[Delumination #{0}] Wrong position, strike.", moduleId);
				return;
			}
		}
		switches[pos].AddInteractionPunch(0.5f);
		audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.Switch, transform);
		StartCoroutine(flipSwitch(pos));
	}
	
	private IEnumerator flipSwitch(int pos) {
		flipping = true;
		for (float i = 0f; i < 5f; i++){
			switches[pos].transform.Rotate(new Vector3(0f, 0f, 6f * dirs[pos]));
			yield return null;
		}
		dirs[pos] *= -1;
		if (dirs[pos] < 1) {
			lights[pos].transform.Find("Light Text").GetComponent<MeshRenderer>().material.mainTexture = lightColoursText[l_int[pos]];
			if (l_int[pos] != 4) lights[pos].transform.Find("Point light").GetComponent<Light>().enabled = true;
			else lights[pos].transform.Find("Light Text").GetComponent<MeshRenderer>().material.mainTexture = lightColoursTextOff[black_l];
		} else {
			lights[pos].transform.Find("Light Text").GetComponent<MeshRenderer>().material.mainTexture = lightColoursTextOff[l_int[pos]];
			lights[pos].transform.Find("Point light").GetComponent<Light>().enabled = false;
		}
		
		int check = dirs.Count(c => c == -1);
		if (check == 4) {
			turn_off = true;
			Debug.LogFormat("[Delumination #{0}] All lights switched on.", moduleId);
		}
		
		check = dirs.Count(c => c == 1);
		if (l_int.Contains(4)) {
			check += 1;
		}
		if (check == 4 && turn_off) {
			Debug.LogFormat("[Delumination #{0}] All lights got turned on and off! You did it!", moduleId);
			audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
			module.HandlePass();
			isSolved = true;
		}
		flipping = false;
    }
	
	//do the shuffle
	private void shuffle(int[] arr) {		
		for (int t = 0; t < arr.Length; t++ )
		{
			int tmp = arr[t];
			int r = Random.Range(t, arr.Length);
			arr[t] = arr[r];
			arr[r] = tmp;
		}
	}
	
	private bool RedNextTo(string type, int col) {
		int lpos = System.Array.IndexOf(l_int, 0);
		int spos = System.Array.IndexOf(s_int, 0);
		
		if (type == "switch") {
			if (s_int[Mathf.Clamp(spos - 1, 0, 3)] == col || s_int[Mathf.Clamp(spos + 1, 0, 3)] == col) return true;
		}
		else if (type == "light") {
			if (l_int[Mathf.Clamp(lpos - 1, 0, 3)] == col || l_int[Mathf.Clamp(lpos + 1, 0, 3)] == col) return true;
		}

		return false;
	}
	
    string TwitchHelpMessage = "!{0}, then a sequence of numbers 1-4 (such as !{0} 12343241) to flip the corresponding switches (command treats order as left to right).";
	string TwitchManualCode = "https://ktane.timwi.de/HTML/Delumination.html";
	
	IEnumerator ProcessTwitchCommand(string command){
	    yield return null;
	    if(command.Contains(' ')){
	        yield return "sendtochaterror {0}, too many parameters.";
	        yield break;
	    }
        if(!command.All(c => c >= '1' && c <= '4')){
            yield return "sendtochaterror {0}, your command must consist solely of numbers 1 to 4.";
            yield break;
        }
	    foreach(char c in command){
	        switch(c){
	            case '1':
	                pressFlip(0);
	                yield return new WaitWhile(() => flipping);
	                break;
	            case '2':
	                pressFlip(1);
	                yield return new WaitWhile(() => flipping);
	                break;
	            case '3':
	                pressFlip(2);
	                yield return new WaitWhile(() => flipping);
	                break;
	            case '4':
	                pressFlip(3);
	                yield return new WaitWhile(() => flipping);
	                break;
	            default:
                    yield return "sendtochaterror {0}, your command must consist solely of numbers 1 to 4.";
                    yield break;
	        }
	    }
	}
	
	IEnumerator TwitchHandleForcedSolve(){
        yield return null;
        if(isSolved)
            yield break;
        if(!turn_off){
            foreach(char c in answer_u){
                if(dirs[s_int.Select(x => (char)(x + '0')).IndexOf(x => x == c)] == 1){
                    pressFlip(s_int.Select(x => (char)(x + '0')).IndexOf(x => x == c));
                    yield return new WaitWhile(() => flipping);
                }
            }
        }
        if(turn_off){
            foreach(char c in answer_d){
                if(dirs[l_int.Select(x => (char)(x + '0')).IndexOf(x => x == c)] == -1){
                    pressFlip(l_int.Select(x => (char)(x + '0')).IndexOf(x => x == c));
                    yield return new WaitWhile(() => flipping);
                }
            }
        }
    }
}
