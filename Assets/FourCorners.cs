using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FourCorners : MonoBehaviour {

    public KMBombModule module;
    public KMBombInfo bombInfo;
    public KMAudio audio;
	public KMColorblindMode colorblind;
	
	public KMSelectable button;
	public Light[] lights;
	
	private bool isSolved;
	private bool colorblindModeEnabled;
	private static int moduleCount;
    private int moduleId;
	
	private bool lightsOn = false;
	private int[] actives;
	private Color[] colors;
	private int iwin;
	
	//Red, Blue, Green, White, Grey
	private static Color[] lightColours = new Color[]{
        new Color(1f, 0f, 0f, 0.3f), 
        new Color(0, 0.4f, 0.85f, 0.8f),      
		new Color(0, 1f, 0f, 0.3f),    
        new Color(0.95f, 0.95f, 0.95f, 0.55f), 		
		new Color(0.4f, 0.4f, 0.4f),		
    };
	
	private string[] table = new string[]{ "01", "22", "11", "00", "12", "00", "02", "11", "22", "11", "01", "12", "02", "12", "00", "22", "00", "02", "22", "01", "11", "01", "12", "02" };
	
	private string[] l_col = new string[]{ "01", "03", "02", "13", "12", "23" };
	
	void Awake () {
		moduleId = moduleCount++;
		
		colorblindModeEnabled = colorblind.ColorblindModeActive;
		
		button.OnInteract += delegate () { pressButton(); return false; };
		button.OnInteractEnded += delegate () { releaseButton(); };
	}
	
	void Start () {
		float scalar = transform.lossyScale.x;
		foreach (Light l in lights) {
			l.range *= scalar;
			l.transform.parent.GetChild(1).GetComponent<MeshRenderer>().enabled = colorblindModeEnabled;
		}
		
		Debug.LogFormat("[Four Corners #{0}] Module started.", moduleId);
	}
	
	void pressButton() {
		audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, transform);
		button.AddInteractionPunch();
		actives = new int[] {0, 0};
		while (actives[1] == actives[0]) {
			actives[0] = UnityEngine.Random.Range(0, 4) + 1;
			actives[1] = UnityEngine.Random.Range(0, 4) + 1;
		}
		foreach (int i in actives) {
			lights[i].enabled = true;
			int rnd = UnityEngine.Random.Range(0, 3);
			lights[i].color = lightColours[rnd];
			lights[i].transform.parent.GetComponent<MeshRenderer>().material.color = lightColours[rnd];
		}
		lights[0].enabled = true;
		int b_rnd = UnityEngine.Random.Range(0, 4);
		lights[0].color = lightColours[b_rnd];
		button.GetComponent<MeshRenderer>().material.color = lightColours[b_rnd];
		
		lightsOn = true;
		StartCoroutine(Blink());
		
		//Colorblind
		string[] n_colors = new string[] {"R", "B", "G", "W"};
		button.transform.GetChild(1).GetComponent<TextMesh>().text = n_colors[b_rnd];
	}
	
	void releaseButton() {
		audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonRelease, transform);
		lightsOn = false;
		
		Color button_color = button.GetComponent<MeshRenderer>().material.color;
		
		for (int i = 0; i < 5; i++) {
			if (iwin < i || i == 0) lights[i].enabled = false;
			lights[i].color = lightColours[3];
			lights[i].transform.parent.GetComponent<MeshRenderer>().material.color = lightColours[3];
			lights[i].transform.parent.GetChild(1).GetComponent<TextMesh>().text = "";
		}
		
		StopAllCoroutines();
		
		if (isSolved) return;
		//logging
		string[] n_colors = new string[] {"Red", "Blue", "Green", "White"};
		string[] n_pos = new string[] {"NW", "NE", "SE", "SW"};
		string n_actives = n_pos[actives[0]-1] + ", " + n_pos[actives[1]-1];
		Debug.LogFormat("[Four Corners #{0}] Active lights: {1}.", moduleId, n_actives);
		
		
		//check solve
		string l_coli = "";
		int[] col_check = new int[2];
		System.Array.Sort(actives);
		int j = 0;
		foreach (int i in actives) {
			l_coli += (i - 1).ToString();
			col_check[j] = System.Array.IndexOf(lightColours, colors[j]);
			j++;
		}
		System.Array.Sort(col_check);
		string s_check = "";
		foreach (int i in col_check) {
			s_check += i.ToString();
		}
		int icol = System.Array.IndexOf(l_col, l_coli);
		int irow = System.Array.IndexOf(lightColours, button_color);
		Debug.LogFormat("[Four Corners #{0}] Button color: {1}.", moduleId, n_colors[irow]);
		string col_table = table[icol * 4 + irow];
		Debug.LogFormat("[Four Corners #{0}] Expected colors: {1}, {2}.", moduleId, n_colors[col_table[0] & 0x0f], n_colors[col_table[1] & 0x0f]);
		if (s_check != col_table) {
			Debug.LogFormat("[Four Corners #{0}] Released at wrong time ({1}, {2}), causing a strike.", moduleId, n_colors[s_check[0] & 0x0f], n_colors[s_check[1] & 0x0f]);
			module.HandleStrike();
			return;
		}
		iwin++;
		lights[iwin].enabled = true;
		audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
		Debug.LogFormat("[Four Corners #{0}] Released at the right colors!", moduleId);
		if (iwin > 3) {
			Debug.LogFormat("[Four Corners #{0}] Pressed right 4 times, module solved!", moduleId);
			isSolved = true;
			module.HandlePass();
		}
	}
	
	private IEnumerator Blink()
    {
		colors = new Color[]{lights[actives[0]].color, lights[actives[1]].color};
		string[] n_colors = new string[] {"R", "B", "G", "W"};
		
		int max = UnityEngine.Random.Range(4, 6);
		int z = 0;
        while (lightsOn)
        {
			if (z % max == 0 && z != 0) {
				Color button_color = button.GetComponent<MeshRenderer>().material.color;
				
				string l_coli = "";
				System.Array.Sort(actives);
				int j = 0;
				foreach (int i in actives) {
					l_coli += (i - 1).ToString();
					j++;
				}
				int icol = System.Array.IndexOf(l_col, l_coli);
				int irow = System.Array.IndexOf(lightColours, button_color);
				string col_table = table[icol * 4 + irow];
				
				colors = new Color[]{lightColours[col_table[0] & 0x0f], lightColours[col_table[1] & 0x0f]};
			}
			
			lights[actives[0]].color = colors[0];
			lights[actives[1]].color = colors[1];
			
			lights[actives[0]].transform.parent.GetComponent<MeshRenderer>().material.color = colors[0];
			lights[actives[1]].transform.parent.GetComponent<MeshRenderer>().material.color = colors[1];
				
			int icol1 = System.Array.IndexOf(lightColours, colors[0]);
			int icol2 = System.Array.IndexOf(lightColours, colors[1]);
			lights[actives[0]].transform.parent.GetChild(1).GetComponent<TextMesh>().text = n_colors[icol1];
			lights[actives[1]].transform.parent.GetChild(1).GetComponent<TextMesh>().text = n_colors[icol2];

            yield return new WaitForSeconds(2f);
			colors = new Color[]{lightColours[UnityEngine.Random.Range(0, 3)], lightColours[UnityEngine.Random.Range(0, 3)]};
			
			z++;
        }
		
		
    }
	
}
