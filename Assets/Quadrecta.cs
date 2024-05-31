using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Quadrecta : MonoBehaviour {

    public KMBombModule module;
    public KMBombInfo bombInfo;
    public KMAudio audio;
	public KMColorblindMode colorblind;
	public KMRuleSeedable rs;
	
	public KMSelectable[] buttons;
	public Light light;
	
	private bool isSolved;
	private static int moduleCount;
	private bool colorblindModeEnabled;
    private int moduleId;
    private string[] selected;
	
	//Red, Blue, Green
	private static Color[] lightColours = new Color[]{
        new Color(0.8f, 0.05f, 0.05f), 
        new Color(0, 0.4f, 0.85f),      
		new Color(0, 0.7f, 0),             
    };
	
	private int[] ledColors = new int[4];
	
	private string[][] combinations;
	
	private string[] words = new string[]{ "Okay", "Wait", "What", "Now", "When", "Stop", "Where", "Again", "Blank", "Repeat" }; 
	
	private int[] table = new int[21];
	private string[,,] relatedWords = new string[7,3,2];
	private int[] solves = new int[] {0,0,0,0};
	int[][]indices = new int[][]{
                new int[]{2,3,5,1,4,6},
                new int[]{3,4,6,2,5,0},
                new int[]{4,5,0,3,6,1},
                new int[]{5,6,1,4,0,2},
                new int[]{6,0,2,5,1,3},
                new int[]{0,1,3,6,2,4},
                new int[]{1,2,4,0,3,5}
	};

	
	void Awake () {
	    var RND = rs.GetRNG();
	    RND.ShuffleFisherYates(words);
	    for(int i = 0; i < 7; i++){
	        relatedWords[i,0,0]=words[indices[i][0]];
	        relatedWords[i,0,1]=words[indices[i][1]];
	        relatedWords[i,1,0]=words[indices[i][2]];
	        relatedWords[i,1,1]=words[indices[i][3]];
	        relatedWords[i,2,0]=words[indices[i][4]];
	        relatedWords[i,2,1]=words[indices[i][5]];
	    }
	    for(int i = 0; i < 21; i++){
	        table[i] = RND.Next(0, 10);
	    }
	    combinations = new string[][]{
	        new string[]{words[0], words[1], words[2], words[4]},
	        new string[]{words[1], words[0], words[3], words[6]},
	        new string[]{words[0], words[2], words[5], words[6]},
	        new string[]{words[0], words[3], words[4], words[5]},
	        new string[]{words[1], words[2], words[3], words[5]},
	        new string[]{words[1], words[4], words[5], words[6]},
	        new string[]{words[2], words[3], words[4], words[6]}
	    };
		int rand = UnityEngine.Random.Range(0,6);
		moduleId = moduleCount++;
		selected = combinations[rand];
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
		string[]rwords = new string[2];
		List<int> index = new List<int>();
		List<int> time_checks = new List<int>();
	    if(selected.Contains(relatedWords[words.Select(x => x.ToUpper()).IndexOf(x => x == hword),ledColors[pos],0])){
	        rwords[0] = relatedWords[words.Select(x => x.ToUpper()).IndexOf(x => x == hword),ledColors[pos],0];
	        index.Add(System.Array.IndexOf(words, rwords[0]));
	        time_checks.Add(table[ledColors[pos] * 7 + index.Last()]);
	    }
	    if(selected.Contains(relatedWords[words.Select(x => x.ToUpper()).IndexOf(x => x == hword),ledColors[pos],1])){
	        rwords[1] = relatedWords[words.Select(x => x.ToUpper()).IndexOf(x => x == hword),ledColors[pos],1];
	        index.Add(System.Array.IndexOf(words, rwords[1]));
	        time_checks.Add(table[ledColors[pos] * 7 + index.Last()]);
	    }
	    if(index.Count == 0){
	        rwords[0] = relatedWords[words.Select(x => x.ToUpper()).IndexOf(x => x == hword),ledColors[pos],0];
	        index.Add(System.Array.IndexOf(words, rwords[0]));
	        time_checks.Add(table[ledColors[pos] * 7 + index.Last()]);
	    }
		
		light.enabled = false;
		light.transform.parent.GetComponent<MeshRenderer>().material.color = new Color(0.45f,0.45f,0.45f);
		
		light.transform.parent.GetChild(1).GetComponent<TextMesh>().text = "";
		
		if (solves[pos] == 1) return;
		//i=y*W + x
		//System.Array.IndexOf(words, rwords[0])
		//table[ledColors[pos] * 7 + index]
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
		Debug.LogFormat("[Quadrecta #{0}] Button {1} pressed, held word: {2}, related word(s): {3}, LED color: {4}, expecting any of {5} in any position", moduleId, pos, hword, string.Join(", ", rwords.ToArray()), colorn, string.Join(", ", time_checks.Select(x => x.ToString()).ToArray()));
		if (bombInfo.GetFormattedTime().ToCharArray().Where(x => x != ':' && x != '.').Any(time_checks.Select(x => (char)(x + 48)).Contains)) {
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

    string TwitchHelpMessage = "!{0} hold OKAY to hold the button labeled OKAY, !{0} release 4 to release when the timer has a 4 in any position.";
    string TwitchManualCode = "https://ktane.timwi.de/HTML/Quadrecta.html";
    private KMSelectable heldButton = null;
    IEnumerator ProcessTwitchCommand(string command)
    {
        yield return null;
        string[]commandParts=command.ToLowerInvariant().Split(' ');
	    if(commandParts.Length < 2){
	        yield return "sendtochaterror {0}, too few parameters.";
	        yield break;
	    }
        if(commandParts.Length > 2){
            yield return "sendtochaterror {0}, Too many parameters.";
            yield break;
        }
        switch(commandParts[0]){
            case "hold":
                if(selected.Select(x => x.ToUpperInvariant()).Contains(commandParts[1].ToUpperInvariant())){
                    heldButton = buttons[selected.Select(x => x.ToUpperInvariant()).IndexOf(x => x == commandParts[1].ToUpperInvariant())];
                    yield return heldButton;
                }else{
                    yield return "sendtochaterror {0}, that button is not on the module.";
                }
                break;
            case "release":
                int number;
                if(heldButton == null){
                    yield return "sendtochaterror {0}, no button is being held right now.";
                }
                if(int.TryParse(commandParts[1], out number)){
                    yield return new WaitUntil(() => bombInfo.GetFormattedTime().ToCharArray().Where(x => x != ':' && x != '.').Contains((char)(number+48)));
                    yield return heldButton;
                    heldButton = null;
                }else{
                    yield return "sendtochaterror {0}, that is not a number.";
                }
                break;
            default:
                yield return "sendtochaterror {0}, your command must begin with either hold or release.";
                break;
        }
    }
    IEnumerator TwitchHandleForcedSolve(){
        yield return null;
        if(isSolved)
            yield break;
        for (int i = 0; i < buttons.Length; i++){
			pressPos(i);
		    string[]rwords = new string[2];
		    List<int> index = new List<int>();
		    List<int> time_checks = new List<int>();
		    string hword = buttons[i].transform.GetChild(2).GetComponent<TextMesh>().text;
	        if(selected.Contains(relatedWords[words.Select(x => x.ToUpper()).IndexOf(x => x == hword),ledColors[i],0])){
	            rwords[0] = relatedWords[words.Select(x => x.ToUpper()).IndexOf(x => x == hword),ledColors[i],0];
	            index.Add(System.Array.IndexOf(words, rwords[0]));
	            time_checks.Add(table[ledColors[i] * 7 + index.Last()]);
	        }
	        if(selected.Contains(relatedWords[words.Select(x => x.ToUpper()).IndexOf(x => x == hword),ledColors[i],1])){
	            rwords[1] = relatedWords[words.Select(x => x.ToUpper()).IndexOf(x => x == hword),ledColors[i],1];
	            index.Add(System.Array.IndexOf(words, rwords[1]));
	            time_checks.Add(table[ledColors[i] * 7 + index.Last()]);
	        }
	        if(index.Count == 0){
	            rwords[0] = relatedWords[words.Select(x => x.ToUpper()).IndexOf(x => x == hword),ledColors[i],0];
	            index.Add(System.Array.IndexOf(words, rwords[0]));
	            time_checks.Add(table[ledColors[i] * 7 + index.Last()]);
	        }
			yield return new WaitUntil(() => bombInfo.GetFormattedTime().ToCharArray().Where(x => x != ':' && x != '.').Any(time_checks.Select(x => (char)(x + 48)).Contains));
			releasePos(i, 0);
		}
    }
}
