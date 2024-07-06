using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mixometer : MonoBehaviour {
	
    public KMBombModule module;
    public KMBombInfo bombInfo;
    public KMAudio audio;
    public KMRuleSeedable rs;
	
	private bool isSolved;
	private static int moduleCount;
    private int moduleId;
	
	private bool fakeStrike = true;
	
	public GameObject[] dials;
	public KMSelectable[] buttons;

	private int[][] i_buttons = new int[5][]  
	{
		new int[] {0, 1, 2, 3},
		new int[] {0, 1, 2, 3},
		new int[] {0, 1, 2, 3},
		new int[] {0, 1, 2, 3},
		new int[] {0, 1, 2, 3}
	};
	
	private int[] s_pos = new int[5];
	
	private int[][] b_pos = new int[5][]
	{
		new int[] {3, 7, 6, 4, 3, 2, 8, 1},
		new int[] {5, 9, 2, 1, 7, 4, 0, 3},
		new int[] {4, 1, 8, 0, 6, 3, 9, 2},
		new int[] {7, 3, 5, 8, 0, 9, 6, 4},
		new int[] {1, 0, 9, 6, 2, 5, 7, 8}
	};
	
	private string ans = "nnnn";
	private string sub = "0000";

	void Start () {
		
		var RND = rs.GetRNG();
		if(RND.Seed != 1){
			for(var x = 0; x < 5; x++){
				for(var y = 0; y < 8; y++){
					b_pos[x][y] = RND.Next(0,10);
				}
			}
		}
		
		shuffle(i_buttons[1]);
		System.Array.Resize(ref i_buttons[1], i_buttons[1].Length-1);
		i_buttons[2] = i_buttons[1];
		shuffle(i_buttons[2]);
		System.Array.Resize(ref i_buttons[2], i_buttons[2].Length-1);
		i_buttons[0] = i_buttons[2];
		shuffle(i_buttons[0]);
		System.Array.Resize(ref i_buttons[0], i_buttons[0].Length-1);
		
		System.Array.Resize(ref i_buttons[4], i_buttons[4].Length-4);
		
		foreach (int[] a in i_buttons) {
			shuffle(a);
		};
		
		for (int t = 0; t < i_buttons.Length; t++ )	{
			int[] tmp = i_buttons[t];
			int r = Random.Range(t, i_buttons.Length);
			i_buttons[t] = i_buttons[r];
			i_buttons[r] = tmp;
		}
		
		int j = 0;
		foreach (KMSelectable button in buttons) {
			int dir = UnityEngine.Random.Range(0, 7);
			s_pos[j] = dir;
			button.transform.Find("Sticker").transform.Rotate(0, 45f * dir, 0);
			if (i_buttons[j].Length != 0) {
				int a = i_buttons[j].Length - 1;
				ans = ans.Remove(a, 1).Insert(a, b_pos[j][dir].ToString());
			}
			
			j++;
			string[] dirs = {"North", "Northeast", "East", "Southeast", "South", "Southwest", "West", "Northwest"};
			if (i_buttons[j-1].Length == 0) {
				Debug.LogFormat("[Mixometer #{0}] Button {1} is the report button. Arrow is pointing {2}.", moduleId, j, dirs[dir]);
				continue;
			}
			Debug.LogFormat("[Mixometer #{0}] Button {1} rotates {2} dials. Arrow is pointing {3}.", moduleId, j, i_buttons[j-1].Length.ToString(), dirs[dir]);
			
			for (int x = 0; x < UnityEngine.Random.Range(0, 7); x++) {
				rotateDials(i_buttons[j-1]);
			}
		}
		
		buttons[0].OnInteract += delegate () { pressRotate(0); return false; };
		buttons[1].OnInteract += delegate () { pressRotate(1); return false; };
		buttons[2].OnInteract += delegate () { pressRotate(2); return false; };
		buttons[3].OnInteract += delegate () { pressRotate(3); return false; };
		buttons[4].OnInteract += delegate () { pressRotate(4); return false; };
		
		Debug.LogFormat("[Mixometer #{0}] The correct answer is {1}.", moduleId, ans);
		
	}
	
	void Awake () {
		moduleId = moduleCount++;
	}
	
	void pressRotate (int pos) {
		if (i_buttons[pos].Length != 0) audio.PlaySoundAtTransform("Dial", transform);
		buttons[pos].AddInteractionPunch(0.25f);
		rotateDials(i_buttons[pos]);
	}
	
	void rotateDials (int[] a_dials) {
		if (a_dials.Length == 0) {
			if (isSolved) return;
			if (sub == ans) {
				Debug.LogFormat("[Mixometer #{0}] Number is correct, module is solved!", moduleId);
				audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
				module.HandlePass();
				isSolved = true;
				return;
			}
			if (fakeStrike) {
				Debug.LogFormat("[Mixometer #{0}] Found the report button. Fake strike.", moduleId);
				audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.Strike, transform);
				fakeStrike = false;
				return;
			}
			Debug.LogFormat("[Mixometer #{0}] Number reported was {1}, expected {2}, strike.", moduleId, sub, ans);
			module.HandleStrike();
			return;
		}
		
		foreach ( int d in a_dials ) {
			StartCoroutine(rotateDial(d));
		}
	}
	
	private IEnumerator rotateDial(int pos) {
		int n = (int.Parse(sub[pos].ToString()) + 1) % 10;
		sub = sub.Remove(pos, 1).Insert(pos, n.ToString());
		
		yield return new WaitForSeconds(0.02f * Random.Range(1, 5));
		for (float i = 0f; i < 6f; i ++){
			dials[pos].transform.Rotate(new Vector3(0f, 6f, 0f));
			yield return null;
		}
		yield return new WaitForSeconds(0.1f);
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
}
