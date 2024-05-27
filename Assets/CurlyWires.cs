using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class CurlyWires : MonoBehaviour {

    public KMBombModule module;
    public KMBombInfo bombInfo;
    public KMAudio audio;
	public KMColorblindMode colorblind;
	
	public KMSelectable[] b_wires;
	public GameObject[] mesh_wires;
	public GameObject[] mesh_cut;
	public TextMesh[] t_wires;
	
	private bool isSolved;
	private bool colorblindModeEnabled;
	private static int moduleCount;
    private int moduleId;
	
	private int[] wire_seq = new int[] {0,0,0};
	
	private string[] table_a = new string[]{ "312", "213", "132", "321", "123", "231", "213", "321", "123" };
	private string[] table_b = new string[]{ "132", "213", "231", "213", "123", "321", "312", "321", "132" };
	
	private int[] table_t = new int[]{ 1, 8, 5, 7, 0, 3, 6, 4, 2, 9, 5, 3, 6, 4, 1, 2, 7, 0, 4, 3, 8, 2, 9, 6, 0, 1, 5, 2, 9, 1, 8, 4, 7, 3, 5, 6, 3, 7, 4, 0, 6, 2, 5, 1, 8 };	 
	
	private int cut_count = 0;
	
	//Red, Blue, Green, White, Black
	private static Color[] wireColours = new Color[]{
        new Color(0.9f, 0f, 0f), 
        new Color(0, 0.4f, 0.85f),      
		new Color(0, 0.9f, 0),    
        new Color(0.95f, 0.95f, 0.95f), 		
		new Color(0.25f, 0.25f, 0.25f),		
    };
	
	void Awake () {
		moduleId = moduleCount++;
		
		colorblindModeEnabled = colorblind.ColorblindModeActive;
	}

	void Start () {
		wire_seq[1] = UnityEngine.Random.Range(2,5);
		wire_seq[2] = UnityEngine.Random.Range(2,5);
		if (UnityEngine.Random.value < 0.67f) wire_seq[1] = 1;
		
		if (wire_seq[1] == 1 && UnityEngine.Random.value <= 0.5f) wire_seq[2] = 1;
		
		//do the shuffle
		for (int t = 0; t < wire_seq.Length; t++ )
        {
            int tmp = wire_seq[t];
            int r = Random.Range(t, wire_seq.Length);
            wire_seq[t] = wire_seq[r];
            wire_seq[r] = tmp;
        }
			
		string s_wires = "";
		string[] colors = new string[]{ "Red ", "Blue ", "Green ", "White ", "Black " };
		string[] colorn = new string[]{ "R", "B", "G", "W", "K" };
		for (int i = 0; i < b_wires.Length; i++){
			int j = i;
			b_wires[j].OnInteract += delegate () { cutPos(j); return false; };
			if (colorblindModeEnabled) t_wires[j].text = colorn[wire_seq[i]].ToString();
			mesh_wires[i].GetComponent<MeshRenderer>().material.color = wireColours[wire_seq[i]];
			mesh_cut[i].GetComponent<MeshRenderer>().material.color = wireColours[wire_seq[i]];
			mesh_cut[i].SetActive(false);
			s_wires += colors[wire_seq[i]];
		}
		
		Debug.LogFormat("[Curly Wires #{0}] Module started.", moduleId);
		Debug.LogFormat("[Curly Wires #{0}] Wire sequence: {1}", moduleId, s_wires);
	}
	
	void cutPos( int pos ){	
		audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.WireSnip, transform);
		b_wires[pos].AddInteractionPunch();
		
		mesh_wires[pos].SetActive(false);
		mesh_cut[pos].SetActive(true);
		
		string[] ord_table;
		ord_table = table_b;
		if(bombInfo.GetSerialNumberLetters().Any(x => x == 'A' || x == 'E' || x == 'I' || x == 'O' || x == 'U')) ord_table = table_a;

		int blues = wire_seq.Count(c => c == 1);
		int redpos = System.Array.IndexOf(wire_seq, 0);
		
		char firstl = bombInfo.GetSerialNumberLetters().First();
		string[] topr = new string[] { "ABC", "DE", "FGH", "IJK", "LMN", "PQR", "STU", "VW", "XZ" };
		int col = 0;
		foreach ( string str in topr ) {
			if (str.Contains(firstl.ToString())) {
				col = System.Array.IndexOf(topr, str);
			}
		}
		//int col = ((int)firstl - 65)/3;
		int row = wire_seq[pos];
		
		bool struck = false;
		if (!bombInfo.GetFormattedTime().Contains(table_t[row * 9 + col].ToString())) {
			Debug.LogFormat("[Curly Wires #{0}] Wire cut at wrong time ({1}), expected {2} any position.", moduleId, bombInfo.GetFormattedTime(), table_t[row * 9 + col].ToString());
			module.HandleStrike();
			struck = true;
		}

		cut_count++;
		if (cut_count == 3) {
			audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
			Debug.LogFormat("[Curly Wires #{0}] Third wire cut, module solved!", moduleId);
			module.HandlePass();
			return;
		}
		
		int pos_to_cut = ord_table[blues * 3 + redpos][cut_count-1] & 0x0f;
		if (pos + 1 != pos_to_cut && !struck) {
			if (cut_count > 1 && pos + 1 != (ord_table[blues * 3 + redpos][2] & 0x0f)) {
				return;
			}
			Debug.LogFormat("[Curly Wires #{0}] Cut the wrong position, expected cut at position {1}, position cut was {2}", moduleId, pos_to_cut, pos+1);
			module.HandleStrike();
			return;
		} 
		
	}
	
}
