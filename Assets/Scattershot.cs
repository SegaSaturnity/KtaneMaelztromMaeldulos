using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class Scattershot : MonoBehaviour {
	
	public KMBombModule module;
    public KMBombInfo bombInfo;
    public KMAudio audio;
	public KMColorblindMode colorblind;
	public KMRuleSeedable rs;
	
	private bool isSolved;
	private bool colorblindModeEnabled;
	private static int moduleCount;
    private int moduleId;
	
	public KMSelectable[] b_buttons;
	public GameObject[] o_buttons;
	
	public Sprite[] icons;
	public Sprite[] s_shadows;
	public Sprite[] c_shadows;
	public Sprite[] t_shadows;
	
	private string key;
	private string ans = "0000";
	private int start = 0;
	
	private string[] column = new string[]{"11", "13", "03", "31", "32"};
	private string[] row = new string[]{" 12", " 01", " 02", " 22", " 21"};
	private Dictionary<string, Sprite[]> shadows;
	
	private Dictionary<string, string[]> table = new Dictionary<string, string[]>() {};
	
	//Shape, Color, Bright, Text
	//Square, Circle, Triangle
	//Red, Blue, White
	//Light, Dark
	//U, Y
	private string[] buttons_ar = new string[]{ "1111", "1112", "1121", "1122", "1211", "1212",
												"1221", "1222", "1311", "1312", "1321", "1322",
												"2111", "2112", "2122", "2121", "2211", "2212",
												"2221", "2222", "2311", "2312", "2321", "2322",
												"3111", "3112", "3122", "3121", "3211", "3212",
												"3221", "3222", "3311", "3312", "3321", "3322" };	 
										 
	//Red, Blue, White
	private static Color[] buttonColours = new Color[]{
        new Color(191f/255f, 0f, 0, 1f), 
        new Color(0, 96f/255f, 191/255f, 1f),       
        new Color(170f/255f, 170f/255f, 170f/255f), 
        new Color(1f, 85f/255f, 113f/255f, 1f), 
        new Color(64f/255f, 159f/255f, 1, 1f),    		
        new Color(191f/255f, 191f/255f, 191f/255f), 
    };

	void Awake () {
		shadows = new Dictionary<string, Sprite[]>(){
			{ "Square", s_shadows },
			{ "Circle", c_shadows },
			{ "Triangle", t_shadows }
		};
	
		Clear();
		int i = 0;
		foreach ( KMSelectable button in b_buttons ) {
			int j = i;
			button.OnInteract += delegate () { Select(j); return false; };	
			i++;
		}
		
		var RND = rs.GetRNG();
		if(RND.Seed != 1){
		    column = RND.ShuffleFisherYates(column);
			row = RND.ShuffleFisherYates(row);
		}
		table = new Dictionary<string, string[]>(){
			{ "01", new string[] {column[0] + row[4], column[1] + row[2]} },
			{ "02", new string[] {column[2] + row[0], column[3] + row[1]} },
			{ "03", new string[] {column[1] + row[1], column[4] + row[3]} },
			{ "11", new string[] {column[2] + row[3], column[3] + row[0]} },
			{ "12", new string[] {column[0] + row[3], column[4] + row[4]} },
			{ "13", new string[] {column[3] + row[4], column[4] + row[2], column[2] + row[1]} },
			{ "21", new string[] {column[0] + row[2], column[3] + row[3], column[4] + row[1]} },
			{ "22", new string[] {column[0] + row[0], column[1] + row[4], column[2] + row[2]} },
			{ "31", new string[] {column[0] + row[1], column[1] + row[3], column[4] + row[0]} },
			{ "32", new string[] {column[1] + row[0], column[2] + row[4], column[3] + row[2]} }
		};
		
		moduleId = moduleCount++;
		colorblindModeEnabled = colorblind.ColorblindModeActive;
	}

	
	void Start () {
		float scalar = transform.lossyScale.x;
		for (var n = 0; n < o_buttons.Length; n++)
		{
			o_buttons[n].transform.Find("Point light").GetComponent<Light>().range *= scalar;
		}
		
		//do the shuffle
		for (int t = 0; t < buttons_ar.Length; t++ )
        {
            string tmp = buttons_ar[t];
            int r = Random.Range(t, buttons_ar.Length);
            buttons_ar[t] = buttons_ar[r];
            buttons_ar[r] = tmp;
        }
		
		key = buttons_ar[Random.Range(0, buttons_ar.Length)];
		start = Random.Range(0, 4);
		
		Debug.LogFormat("[Scattershot #{0}] Module starting, key button is{1}.", moduleId, TranslateAll(key));
		
		int i = 0;
		foreach (string n in buttons_ar)
		{
			int sel = 0;
			GameObject obj = null;
			Color col = new Color(0, 0, 0);
			
			if ( n[0].ToString() == "1") obj = o_buttons[i].transform.Find("Square").gameObject;
			else if ( n[0].ToString() == "2") obj = o_buttons[i].transform.Find("Circle").gameObject;
			else obj = o_buttons[i].transform.Find("Triangle").gameObject;
			
			obj.SetActive(true);
			
			int ncol = int.Parse(n[1].ToString()) - 1;
			col = buttonColours[ncol];
			
			obj.GetComponent<MeshRenderer>().material.color = col;
			o_buttons[i].transform.Find("Point light").GetComponent<Light>().color = buttonColours[(3 + int.Parse(n[1].ToString()) - 1)];
			o_buttons[i].transform.Find("ColorBlind").GetComponent<TextMesh>().text = new string[] {"R", "B", "W"} [ncol];
			o_buttons[i].transform.Find("ColorBlind").gameObject.SetActive(colorblindModeEnabled);
			
			if ( n[2].ToString() == "2") sel+=3;
			sel+=int.Parse(n[0].ToString())-1;
			
			if ( n[3].ToString() == "2") sel+=6; 
			
			obj.transform.Find("Canvas/Image").GetComponent<UnityEngine.UI.Image>().sprite = icons[sel];
			i++;
		}
	}

	void Select(int pos) {
		Clear();
		
		if (isSolved) return;
		audio.PlaySoundAtTransform("Press", transform);
		b_buttons[pos].AddInteractionPunch(0.25f);
		
		int i = 0;
		if (buttons_ar[pos] == key) {
			Debug.LogFormat("[Scattershot #{0}] Pressed the right button, horray! Solved!", moduleId);
			module.HandlePass();
			isSolved = true;
			return;
		}
		
		Debug.LogFormat("[Scattershot #{0}] Pressed{1}, looking for{2}.", moduleId, TranslateAll(buttons_ar[pos]), TranslateAll(ans));
		foreach (char c in buttons_ar[pos]) {
			int j = i;
			if (ans[j] != c && ans[j] != "0"[0]) {
				Debug.LogFormat("[Scattershot #{0}] Strike!", moduleId);
				ans = "0000";
				module.HandleStrike();
				break;
			}
			i++;
		}
		
		if (ans[start] == "0"[0]) {
			ans = ans.Remove(start, 1).Insert(start, key[start].ToString());
		}
		
		
		i = 0;
		string tcheck = start.ToString() + ans[start].ToString();
		string[] to_blink = table[tcheck][Random.Range(0, table[tcheck].Length)].Split(" "[0]);
		Debug.LogFormat("[Scattershot #{0}] Lit up: all {1} and all {2}, key shown: {3}.", moduleId, TranslateID(to_blink[0]), TranslateID(to_blink[1]), TranslateID(tcheck));
		foreach ( KMSelectable button in b_buttons ) {
			int j = i;
			//check for blink
			foreach (string seq in to_blink) {
				if (buttons_ar[j][int.Parse(seq[0].ToString())].ToString() == seq[1].ToString()) {
					Color col = buttonColours[(3 + int.Parse(buttons_ar[j][1].ToString()) - 1)];
					foreach (string shape in new string[] {"Triangle", "Circle", "Square"}) {
						o_buttons[j].transform.Find(shape).GetComponent<MeshRenderer>().material.color = col;
						o_buttons[j].transform.Find(shape + "/Canvas/Shadow").GetComponent<UnityEngine.UI.Image>().sprite = shadows[shape][1];
					}
					o_buttons[j].transform.Find("Point light").GetComponent<Light>().enabled = true;
				}
			}
			i++;
		}
		start = (start + 1) % 4;
	}

	void Clear() {
		int i = 0;
		foreach ( KMSelectable button in b_buttons ) {
			int j = i;
			Color col = buttonColours[(int.Parse(buttons_ar[j][1].ToString()) - 1)];
			foreach (string shape in new string[] {"Triangle", "Circle", "Square"}) {
				o_buttons[j].transform.Find(shape).GetComponent<MeshRenderer>().material.color = col;
				o_buttons[j].transform.Find(shape + "/Canvas/Shadow").GetComponent<UnityEngine.UI.Image>().sprite = shadows[shape][0];
			}
			o_buttons[j].transform.Find("Point light").GetComponent<Light>().enabled = false;
			i++;
		}
    }
	
	string TranslateAll(string id){
		string result = "";
		switch(id[2].ToString())
		{
			case "0":
				result+=" any";
				break;
			case "1":
				result+=" light";
				break;
			case "2":
				result+=" dark";
				break;
		}
		switch(id[1].ToString())
		{
			case "0":
				result+=" any";
				break;
			case "1":
				result+=" red";
				break;
			case "2":
				result+=" blue";
				break;
			case "3":
				result+=" white";
				break;
		}
		switch(id[0].ToString())
		{
			case "0":
				result+=" any";
				break;
			case "1":
				result+=" square";
				break;
			case "2":
				result+=" circle";
				break;
			case "3":
				result+=" triangle";
				break;
		}
		switch(id[3].ToString())
		{
			case "0":
				result+=" any";
				break;
			case "1":
				result+=" U";
				break;
			case "2":
				result+=" Y";
				break;
		}
		return result;
	}

	string TranslateID(string id){
		if (id[0].ToString() == "0")
		{
			switch(id[1].ToString())
			{
				case "1":
					return "square";
				case "2":
					return "circle";
				case "3":
					return "triangle";
			}
		}
		else if (id[0].ToString() == "1")
		{
			switch(id[1].ToString())
			{
				case "1":
					return "red";
				case "2":
					return "blue";
				case "3":
					return "white";
			}
		}
		else if (id[0].ToString() == "2")
		{
			switch(id[1].ToString())
			{
				case "1":
					return "light";
				case "2":
					return "dark";
			}
		}
		else
		{
			switch(id[1].ToString())
			{
				case "1":
					return "U";
				case "2":
					return "Y";
			}
		}
		return null;
	}

    string TwitchHelpMessage = "!{0}, then a letter and number (no space in between) to press the button at that position. Letter represents column, number represents row. For example, !{0} A6 to press the bottom-right button.";
    string TwitchManualCode = "https://ktane.timwi.de/HTML/Scattershot.html";

    IEnumerator ProcessTwitchCommand(string command){
        yield return null;
        if(command.Length < 2)
            yield return "sendtochaterror {0}, too few parameters.";
        else if(command.Length > 2)
            yield return "sendtochaterror {0}, too many parameters.";
        else if(!"ABCDEF".Contains(command[0]) || !"123456".Contains(command[1]))
            yield return "sendtochaterror {0}, invalid coordinate.";
        else
            Select("123456".IndexOf(command[1]) * 6 + "ABCDEF".IndexOf(command[0]));
    }

    void TwitchHandleForcedSolve(){
        if(isSolved)
            return;
        Select(System.Array.IndexOf(buttons_ar, key));
    }

}

