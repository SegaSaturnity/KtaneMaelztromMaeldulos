using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class BallDial : MonoBehaviour {

    public KMBombModule module;
    public KMBombInfo bombInfo;
    public KMAudio audio;
	
	public KMSelectable[] b_directions;
	public KMSelectable b_select;
	public GameObject rot_point;
	
	private bool isSolved;
	private static int moduleCount;
    private int moduleId;
	
	public Material[] faces;
	public MeshRenderer[] b_texts;
	
	private int[] table = new int[]{ 2, 3, 0, 2, 3, 0, 0, 1, 1, 2, 3, 0, 3, 3, 1, 2, 0, 2, 1, 3 };
	
	private bool moving = false;
	private int press_n = 0;
	private int last_press = 0;
	private int clock = 0;
	private int display = -1;
	private int display_type = 0;
	private int spins = 0;
	private int target = 0;

	void Start () {
		
		Debug.LogFormat("[Ball Dial #{0}] Module started.", moduleId);
		StartCoroutine(rotateBall(2));
	}
	
	void Awake () {
		moduleId = moduleCount++;
		
		foreach (KMSelectable d in b_directions) {
			int i = System.Array.IndexOf(b_directions, d);
			d.OnInteract += delegate () { moveDir(i); return false; };
			d.GetComponent<MeshRenderer>().material.color = new Color(0.2f, 0.2f, 0.2f);
		}
		b_select.OnInteract += delegate () { submit(); return false; };
	
	}
	
	void moveDir(int d) {
		string[] s_dirs = new string[] {"up", "right", "down", "left"};
		string[] s_type = new string[] {"arrow", "number", "word", "2 elements", "3 elements"};
		
		if (moving) return;
		Debug.LogFormat("[Ball Dial #{0}] Direction pressed: {1}, display type was: {2}, expected: {3}.", moduleId, s_dirs[mod(d - clock, 4)], s_type[display_type], s_dirs[target]);
		
		b_directions[d].AddInteractionPunch(0.25f);
		if (mod(d - clock, 4) != target && !isSolved) {
			Debug.LogFormat("[Ball Dial #{0}] Strike! Wrong direction press.", moduleId);
			module.HandleStrike();
			
			press_n = 0;
			clock = 0;
		}
		
		if (press_n > 0 && mod(d - clock, 4) == target) {
			press_n--;
		}
		
		last_press = mod(d - clock, 4);
		if (press_n < 1) spins++;
		StartCoroutine(rotateBall(d));
		moving = true;
		
		if (spins > 0 && press_n < 1) target = table[display_type * 4 + last_press];
	}
	
	void submit() {
		if (isSolved) return;
		
		b_select.AddInteractionPunch();
		if (display == -1) {
			Debug.LogFormat("[Ball Dial #{0}] There are no numbers on the ball or you still have to keep spinning it.", moduleId);
		}
		else { 
			Debug.LogFormat("[Ball Dial #{0}] Submitted on number: {1}", moduleId, display);
		}
		if (!displayInSerial()) {
			Debug.LogFormat("[Ball Dial #{0}] Which is not in the serial number, strike!", moduleId);
			module.HandleStrike();
			return;
		}
		Debug.LogFormat("[Ball Dial #{0}] Which is in the serial number, module solved!", moduleId);
		audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
		isSolved = true;
		module.HandlePass();
	}
	
	private IEnumerator rotateBall(int pos)
    {
        float max = 0.4f;
        float cur = 0f;
		Vector2 direction;
		
		switch (pos) {
		case 3:
			direction = new Vector2(360f, 0);
		break;
		case 0:
			direction = new Vector2(0, 360f);
		break;
		case 1:
			direction = new Vector2(-360f, 0);
		break;
		default:
			direction = new Vector2(0, -360f);
		break;
		}
		
		int table_i = UnityEngine.Random.Range(0, 5);
		int[] gen_faces = generateFace(table_i);
		while (displayInSerial() && spins < 2) {
			press_n = 0;
			gen_faces = generateFace(table_i);
			
		}
		logFaces(gen_faces);
		
        while (cur < max / 2) {
            rot_point.transform.localEulerAngles = new Vector3(Easing.InOutQuad(cur, 0, direction.y, max), 0f, Easing.InOutQuad(cur, 0, direction.x, max));
			yield return null;
            cur += Time.deltaTime;
        }
		
		int i = 0;
		foreach ( int f in gen_faces ) {
			b_texts[i].sharedMaterial = faces[f];
			i++;
		}
		
		while (cur < max) {
            rot_point.transform.localEulerAngles = new Vector3(Easing.InOutQuad(cur, 0, direction.y, max), 0f, Easing.InOutQuad(cur, 0, direction.x, max));
			yield return null;
            cur += Time.deltaTime;
        }
			
		moving = false;
		rot_point.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
    }

	void logFaces( int[] ifaces) {
		string[] result = new string[] {"", "", ""};
		
		foreach (int f in ifaces) {
			int i = System.Array.IndexOf(ifaces, f);
			string to_add = "";
			if ( f == 0 || f == 18 || f == 28 || f == 58 || f == 68 ) to_add += "number 0 ";
			if ( f == 1 || f == 19 || f == 29 || f == 59 || f == 69 ) to_add += "number 1 ";
			if ( f == 2 || f == 20 || f == 30 || f == 60 || f == 70 ) to_add += "number 2 ";
			if ( f == 3 || f == 21 || f == 31 || f == 61 || f == 71 ) to_add += "number 3 ";
			if ( f == 4 || f == 22 || f == 32 || f == 62 || f == 72 ) to_add += "number 4 ";
			if ( f == 5 || f == 23 || f == 33 || f == 63 || f == 73 ) to_add += "number 5 ";
			if ( f == 6 || f == 24 || f == 34 || f == 64 || f == 74 ) to_add += "number 6 ";
			if ( f == 7 || f == 25 || f == 35 || f == 65 || f == 75 ) to_add += "number 7 ";
			if ( f == 8 || f == 26 || f == 36 || f == 66 || f == 76 ) to_add += "number 8 ";
			if ( f == 9 || f == 27 || f == 37 || f == 67 || f == 77 ) to_add += "number 9 ";
			if ( f == 10|| f == 38 || f == 42 || f == 78) to_add += "arrow up ";
			if ( f == 11|| f == 39 || f == 43 || f == 79) to_add += "arrow right ";
			if ( f == 12|| f == 40 || f == 44 || f == 80) to_add += "arrow down ";
			if ( f == 13|| f == 41 || f == 45 || f == 81) to_add += "arrow left ";
			if ( f == 14|| f == 46 || f == 50 || f == 54 || f == 82 || f == 86) to_add += "word up ";
			if ( f == 15|| f == 47 || f == 51 || f == 55 || f == 83 || f == 87) to_add += "word right ";
			if ( f == 16|| f == 48 || f == 52 || f == 56 || f == 84 || f == 88) to_add += "word down ";
			if ( f == 17|| f == 49 || f == 53 || f == 57 || f == 85 || f == 89) to_add += "word left ";
			if ( i > 0 && result[i-1] == to_add) continue;
			result[i] = to_add;
		}
		
		string end = "";
		foreach (string s in result) {
			end += s;
		}
		
		Debug.LogFormat("[Ball Dial #{0}] Generated face: {1}", moduleId, end);
	}

	//This has to be a war crime or something.
	//Please simplify this later, me
	private int[] generateFace(int rnd) {
		if (press_n < 1) {
			clock = 0;
			display_type = rnd;
		}
		display = -1;
		
		if (spins+1 % 5 == 0) {
			rnd = -1;
		}
		
		int[] ifaces;
		switch (rnd) {
		case -1:
			display = bombInfo.GetSerialNumberNumbers().First();
			display_type = 1;
			ifaces = new int[] {display, display, display};
			break;
		case 0:
			rnd = UnityEngine.Random.Range(10, 14);
			if (press_n < 1) clock = rnd - 10;
			ifaces = new int[] {rnd, rnd, rnd};
			break;
		case 1:
			rnd = UnityEngine.Random.Range(0, 10);
			if (press_n < 1) {
				display = rnd;
				press_n = rnd;
				if (spins > 0) target = table[display_type * 4 + last_press];
			}
			ifaces = new int[] {rnd, rnd, rnd};
			break;
		case 2:
			rnd = UnityEngine.Random.Range(14, 18);
			if (press_n < 1) last_press = rnd - 14;
			ifaces = new int[] {rnd, rnd, rnd};
			break;
		case 3:
			int num, arr, wor;
			// 2 symbols
			// number arrow
			if (UnityEngine.Random.value > 0.33f) {
				if (UnityEngine.Random.value > 0.5f) {
					num = UnityEngine.Random.Range(18, 28);
					arr = 38;
					if (UnityEngine.Random.value > 0.5f) arr = 40;
					if (press_n < 1) {
						press_n = num - 18;
						if (spins > 0) target = table[display_type * 4 + last_press];
						clock = arr - 38;
						display = num - 18;
					}
				} else {
					num = UnityEngine.Random.Range(28, 38);
					arr = 39;
					if (UnityEngine.Random.value > 0.5f) arr = 41;
					if (press_n < 1) {
						press_n = num - 28;
						if (spins > 0) target = table[display_type * 4 + last_press];
						clock = arr - 38;
						display = num - 28;
					}
				}
				wor = arr;
			//number word
			} else if (UnityEngine.Random.value > 0.66f) {
				num = UnityEngine.Random.Range(28, 38);
				wor = UnityEngine.Random.Range(54, 58);
				if (press_n < 1) {
					press_n = num - 28;
					last_press = wor - 54;
					if (spins > 0) target = table[display_type * 4 + last_press];
				}
				display = num - 28;
				arr = wor;
			//arrow word
			} else {
				if (UnityEngine.Random.value > 0.5f) {
					arr = 38;
					if (UnityEngine.Random.value > 0.5f) arr = 40;
					wor = UnityEngine.Random.Range(46, 50);
					if (press_n < 1) {
						last_press = wor - 46;
						clock = arr - 38;
					}
				} else {
					arr = 39;
					if (UnityEngine.Random.value > 0.5f) arr = 41;
					wor = UnityEngine.Random.Range(50, 54);
					if (press_n < 1) {
						last_press = wor - 50;
						clock = arr - 38;
					}
				}
				num = wor;
			}
			ifaces = new int[] {num, arr, wor};
			break;
		default:
			// 3 symbols
			if (UnityEngine.Random.value > 0.5f) {
				num = UnityEngine.Random.Range(58, 68);
				arr = 80;
				if (UnityEngine.Random.value > 0.5f) arr = 78;
				wor = UnityEngine.Random.Range(82, 86);
				
				if (press_n < 1) {
					press_n = num - 58;
					last_press = wor - 82;
					if (spins > 0) target = table[display_type * 4 + last_press];
					display = num - 58;
					clock = arr - 78;
				}
			} else {
				num = UnityEngine.Random.Range(68, 78);
				arr = 79;
				if (UnityEngine.Random.value > 0.5f) arr = 81;
				wor = UnityEngine.Random.Range(86, 90);
				
				if (press_n < 1) {
					press_n = num - 68;
					last_press = wor - 86;
					if (spins > 0) target = table[display_type * 4 + last_press];
					display = num - 68;
					clock = arr - 78;
				}
			}
			ifaces = new int[] {num, arr, wor};
			break;
		}
		return ifaces;
	}
	
	int mod(int a, int b) {  return ((a %= b) < 0) ? a+b : a;  }
	
	private bool displayInSerial() {
		return bombInfo.GetSerialNumberNumbers().Any(x => x == display);
	}
}
