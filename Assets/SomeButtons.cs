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
	public KMRuleSeedable rs;
	
	private enum Port{DVI, Parallel, PS2, RJ45, Serial, StereoRCA, ComponentVideo, CompositeVideo, USB, HDMI, VGA, AC, PCMIA}
	private enum Battery{Unknown=0,Empty=0,D=1,AA=2,AAx3=3,AAx4=4}
	private enum IndicatorLight{unlit, lit}
	private enum Indicator{SND, CLR, CAR, IND, FRQ, SIG, NSA, MSA, TRN, BOB, FRK, NLL}
	
	public KMSelectable[] buttons;
	
	private bool isSolved;
	private static int moduleCount;
	private bool colorblindModeEnabled;
    private int moduleId;
    private string[] colors = new string[]{"red", "blue", "green", "white", "black"};
    private string[]ruleTemplates = new string[12];
    private int[,]generatedNumbers = new int[,]{
        {-1, -1},
        {-1, -1},
        {-1, -1},
        {-1, -1},
        {-1, -1},
        {-1, -1},
        {-1, -1},
        {-1, -1},
        {-1, -1},
        {-1, -1},
        {-1, -1},
        {-1, -1},
        {-1, -1},
        {-1, -1},
        {-1, -1}
    };
    
    private Dictionary<int, List<int>> buttonsToBePressed = new Dictionary<int, List<int>>();
    
    //Debug.LogFormat("[Some Buttons #{0}] Vowel in serial, press 3", moduleId);
	
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
		var RND = rs.GetRNG();
		if(RND.Seed == 1){
		    ruleTemplates = new string[]{"the serial number contains a vowel","the bomb has a lit FRK indicator","the serial number does not contain a vowel","the bomb has at least 1 AA battery","the bomb has at most 1 D battery","the bomb has a PS/2 port","there are at least 4 red buttons","the quantity of blue and green buttons is the same","the quantity of black buttons is odd","there are no buttons of a unique color","there are 5 different colors of buttons","there is at most 1 white button",
		    "the quantity of red and white buttons is different","the quantity of blue buttons is even"};//placeholder rules required for functionality, not actually used in rule seed 1
            buttonsToBePressed.Add(0, new List<int>{2});
            buttonsToBePressed.Add(1, new List<int>{11});
            buttonsToBePressed.Add(2, new List<int>{5});
            buttonsToBePressed.Add(3, new List<int>{9});
            buttonsToBePressed.Add(4, new List<int>{4});
            buttonsToBePressed.Add(5, new List<int>{7});
            buttonsToBePressed.Add(6, new List<int>{3, 6});
            buttonsToBePressed.Add(7, new List<int>{8, 9});
            buttonsToBePressed.Add(8, new List<int>{0, 2});
            buttonsToBePressed.Add(9, new List<int>{1, 10});
            buttonsToBePressed.Add(10, new List<int>{0, 6});
            buttonsToBePressed.Add(11, new List<int>{5, 7});
            generatedNumbers = new int[,]{
                {-1, -1},
                {1, 11},
                {1, -1},
                {1, 2},
                {1, 1},
                {2, -1},
                {4, 0},
                {1, 2},
                {4, -1},
                {-1, -1},
                {-1, -1},
                {0, 3},
                {0, 3},
                {1, -1}
            };
		}else{
		    string[] ruleTemplatesEdgework = {
                                                "the serial number contains a vowel",
                                                "the serial number does not contain a vowel",
                                                "the bomb has PORTS port",
                                                "the bomb has LIGHT IND indicator",
                                                "the bomb has at least _ BATTERY batteries",
                                                "the bomb has at most _ BATTERY batteries"
                                             };
            string[] ruleTemplatesButtons = {
                                    "there are at least ! COLORAMOUNT buttons",
                                    "there are at most ! COLORAMOUNT buttons",
                                    "the quantity of COLOR1 and COLOR2 buttons is the same",
                                    "the quantity of COLOR1 and COLOR2 buttons is different",
                                    "the quantity of COLORPARITY buttons is odd",
                                    "the quantity of COLORPARITY buttons is even",
                                    "there are no buttons of a unique color",
                                    "there are 5 different colors of buttons"
                                     };
		    RND.ShuffleFisherYates(ruleTemplatesEdgework);
		    RND.ShuffleFisherYates(ruleTemplatesButtons);
		    ruleTemplates = ruleTemplatesEdgework.Concat(ruleTemplatesButtons).ToArray();
		    for(int i = 0; i < 14; i++){
		        int repl = -1;
		        int repl2 = -1;
		        if(ruleTemplates[i].Contains("_")){
		            repl = RND.Next(1, 5);
		            generatedNumbers[i,0]=repl;
		            ruleTemplates[i] = ruleTemplates[i].Replace("_", repl.ToString());
		            if(repl == 1){
		                ruleTemplates[i] = ruleTemplates[i].Replace("batteries", "battery");
		            }
		        }
		        if(ruleTemplates[i].Contains("!")){
		            repl = RND.Next(1, 5);
		            generatedNumbers[i,0]=repl;
		            ruleTemplates[i] = ruleTemplates[i].Replace("!", repl.ToString());
		            if(repl == 1){
                        ruleTemplates[i] = ruleTemplates[i].Replace("there are", "there is");
                        ruleTemplates[i] = ruleTemplates[i].Replace("buttons", "button");
		            }
		        }
                if(ruleTemplates[i].Contains("PORTS")){
                    repl = RND.Next(0, 6);
                    generatedNumbers[i,0]=repl;
                    ruleTemplates[i] = ruleTemplates[i].Replace("PORTS", "a(n) "+((Port)repl).ToString());
                }
                if(ruleTemplates[i].Contains("LIGHT")){
                    repl = RND.Next(0, 2);
                    generatedNumbers[i,0]=repl;
                    ruleTemplates[i] = ruleTemplates[i].Replace("LIGHT", "a(n) "+((IndicatorLight)repl).ToString());
                }
                if(ruleTemplates[i].Contains("IND")){
                    repl2 = RND.Next(0, 12);
                    generatedNumbers[i,1]=repl2;
                    ruleTemplates[i] = ruleTemplates[i].Replace("IND", ((Indicator)repl2).ToString());
                }
                if(ruleTemplates[i].Contains("BATTERY")){
                    repl2 = RND.Next(1, 3);
                    generatedNumbers[i,1]=repl2;
                    ruleTemplates[i] = ruleTemplates[i].Replace("BATTERY", ((Battery)repl2).ToString());
                }
                if(ruleTemplates[i].Contains("COLOR1")){
                    repl = RND.Next(0, 5);
                    generatedNumbers[i,0]=repl;
                    ruleTemplates[i] = ruleTemplates[i].Replace("COLOR1", colors[repl]);
                    if(ruleTemplates[i].Contains("COLOR2")){
                        do{
                            repl2 = RND.Next(0, 5);
                        }while(repl2 == repl);
                        generatedNumbers[i,1]=repl2;
                        ruleTemplates[i] = ruleTemplates[i].Replace("COLOR2", colors[repl2]);
                    }
                }
                if(ruleTemplates[i].Contains("COLORAMOUNT")){
                    repl2 = RND.Next(0, 5);
                    generatedNumbers[i,1]=repl2;
                    ruleTemplates[i] = ruleTemplates[i].Replace("COLORAMOUNT", colors[repl2]);
                }
                if(ruleTemplates[i].Contains("COLORPARITY")){
                    repl = RND.Next(0, 5);
                    generatedNumbers[i,0]=repl;
                    ruleTemplates[i] = ruleTemplates[i].Replace("COLORPARITY", colors[repl]);
                }
                int filledInCircle = RND.Next(0, 12);
                if(System.Array.IndexOf(ruleTemplates, "the serial number contains a vowel") < i && ruleTemplates[i] == "the serial number does not contain a vowel"){
                    while(buttonsToBePressed[System.Array.IndexOf(ruleTemplates, "the serial number contains a vowel")][0] == filledInCircle){
                        filledInCircle = RND.Next(0, 12);
                    }
                }
                if(System.Array.IndexOf(ruleTemplates, "the serial number does not contain a vowel") < i && ruleTemplates[i] == "the serial number contains a vowel"){
                    while(buttonsToBePressed[System.Array.IndexOf(ruleTemplates, "the serial number does not contain a vowel")][0] == filledInCircle){
                        filledInCircle = RND.Next(0, 12);
                    }
                }
                List<int> circles = new List<int>();
                circles.Add(filledInCircle);
                if(i > 5){
                    int filledInCircle2;
                    do{
                        filledInCircle2 = RND.Next(0, 12);
                    }while(filledInCircle2 == filledInCircle);
                    circles.Add(filledInCircle2);
                }
                buttonsToBePressed.Add(i, circles);
		    }
		}
		bool serialContainsVowels = bombInfo.GetSerialNumberLetters().Any(x => "AEIOU".Contains(x));
		bool serialContainsNoVowels = !serialContainsVowels;
		bool hasGivenPort = bombInfo.GetPortCount(((Port)generatedNumbers[ruleTemplates.IndexOf(x => x.Contains("port")), 0]).ToString()) > 0;
		bool hasGivenIndicator;
		if((generatedNumbers[ruleTemplates.IndexOf(x => x.Contains("indicator")), 0]) == 0)
		    hasGivenIndicator = bombInfo.IsIndicatorOff(((Indicator)generatedNumbers[ruleTemplates.IndexOf(x => x.Contains("indicator")), 1]).ToString());
		else
		    hasGivenIndicator = bombInfo.IsIndicatorOn(((Indicator)generatedNumbers[ruleTemplates.IndexOf(x => x.Contains("indicator")), 1]).ToString());
		bool hasAtLeastNumberBatteries = bombInfo.GetBatteryCount(generatedNumbers[ruleTemplates.IndexOf(x => x.Contains("the bomb has at least ")), 1]) >= generatedNumbers[ruleTemplates.IndexOf(x => x.Contains("the bomb has at least ")), 0];
		bool hasAtMostNumberBatteries = bombInfo.GetBatteryCount(generatedNumbers[ruleTemplates.IndexOf(x => x.Contains("the bomb has at most ")), 1]) <= generatedNumbers[ruleTemplates.IndexOf(x => x.Contains("the bomb has at most ")), 0];
		bool hasAtLeastNumberColorButtons = buttonSequence.Count(j => j == generatedNumbers[ruleTemplates.IndexOf(x => x.Contains("there are at least ") || x.Contains("there is at least ")), 1]) >= generatedNumbers[ruleTemplates.IndexOf(x => x.Contains("there are at least ") || x.Contains("there is at least ")), 0];
		bool hasAtMostNumberColorButtons = buttonSequence.Count(j => j == generatedNumbers[ruleTemplates.IndexOf(x => x.Contains("there are at most ") || x.Contains("there is at most ")), 1]) <= generatedNumbers[ruleTemplates.IndexOf(x => x.Contains("there are at most ") || x.Contains("there is at most ")), 0];
		bool quantityColoredSame = buttonSequence.Count(j => j == generatedNumbers[ruleTemplates.IndexOf(x => x.Contains(" is the same")), 0]) == buttonSequence.Count(j => j == generatedNumbers[ruleTemplates.IndexOf(x => x.Contains(" is the same")), 1]);
		bool quantityColoredDifferent = buttonSequence.Count(j => j == generatedNumbers[ruleTemplates.IndexOf(x => x.Contains(" is different")), 0]) != buttonSequence.Count(j => j == generatedNumbers[ruleTemplates.IndexOf(x => x.Contains(" is different")), 1]);
		bool quantityOdd = buttonSequence.Count(j => j == generatedNumbers[ruleTemplates.IndexOf(x => x.Contains(" is odd")), 0]) % 2 == 1;
		bool quantityEven = buttonSequence.Count(j => j == generatedNumbers[ruleTemplates.IndexOf(x => x.Contains(" is even")), 0]) % 2 == 0;
		bool noUniqueColors = buttonSequence.Count(x => x == 0) != 1 && buttonSequence.Count(x => x == 1) != 1 && buttonSequence.Count(x => x == 2) != 1 && buttonSequence.Count(x => x == 3) != 1 && buttonSequence.Count(x => x == 4) != 1;
		bool fiveDifferentColors = buttonSequence.Count(x => x == 0) >= 1 && buttonSequence.Count(x => x == 1) >= 1 && buttonSequence.Count(x => x == 2) >= 1 && buttonSequence.Count(x => x == 3) >= 1 && buttonSequence.Count(x => x == 4) >= 1;
		
		bool[]allConditions = new bool[]{serialContainsVowels, serialContainsNoVowels, hasGivenPort, hasGivenIndicator, hasAtLeastNumberBatteries, hasAtMostNumberBatteries, hasAtLeastNumberColorButtons, hasAtMostNumberColorButtons, quantityColoredSame, quantityColoredDifferent, quantityOdd, quantityEven, noUniqueColors, fiveDifferentColors};
		string[]allConditionsWords = new string[]{
		                                             "the serial number contains a vowel",
                                                     "the serial number does not contain a vowel",
                                                     "port",
                                                     "indicator",
                                                     "the bomb has at least ",
                                                     "the bomb has at most ",
                                                     "there are at least ",
                                                     "there are at most ",
                                                     " is the same",
                                                     " is different",
                                                     " is odd",
                                                     " is even",
                                                     "there are no buttons of a unique color",
                                                     "there are 5 different colors of buttons"
                                                 };
		Debug.LogFormat("[Some Buttons #{0}] The buttons are as follows: {1}.", moduleId, string.Join(", ", buttonSequence.Select(x => colors[x]).ToArray()));
		for(int i = 0; i < 14; i++){
		    if(allConditions[i] && ruleTemplates.IndexOf(x => x.Contains(allConditionsWords[i])) < 12){
		        Debug.LogFormat("[Some Buttons #{0}] T{1}, so press the following button(s): {2}.", moduleId, ruleTemplates[ruleTemplates.IndexOf(x => x.Contains(allConditionsWords[i]))].Substring(1), string.Join(", ", buttonsToBePressed[ruleTemplates.IndexOf(x => x.Contains(allConditionsWords[i]))].Select(x => (x+1).ToString()).ToArray()));
		        foreach(int button in buttonsToBePressed[ruleTemplates.IndexOf(x => x.Contains(allConditionsWords[i]))]){
		            answerSequence[button] = 1;
		        }
		    }
		}
		List<string> buttonsFinal = new List<string>();
		for(int i = 0; i < 12; i++){
		    if(answerSequence[i] == 1){
		        buttonsFinal.Add((i+1).ToString());
		    }
		}
		Debug.LogFormat("[Some Buttons #{0}] The following button(s) should be pressed: {1}.", moduleId, string.Join(", ", buttonsFinal.ToArray()));
	}
	
	void pressPos( int pos ){
		audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		buttons[pos].AddInteractionPunch();
		if (isSolved) return;
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
	
	string TwitchHelpMessage = "!{0}, then a sequence of numbers to press the corresponding buttons (buttons labeled in reading order starting from 1). For example, !{0} 1 4 5 8 10 to press buttons 1, 4, 5, 8 and 10.";
	string TwitchManualCode = "https://ktane.timwi.de/HTML/Some%20Buttons.html";

	IEnumerator ProcessTwitchCommand(string command){
	    yield return null;
	    string[]commandParts = command.ToLowerInvariant().Split(' ');
	    int current = -1;
	    foreach(string part in commandParts){
	        if(int.TryParse(part, out current)){
	            if(current < 1 || current > 12){
	                yield return "sendtochaterror {0}, your command contains a number outside the range of 1 to 12.";
	                yield break;
	            }
	        }
	        else{
	            yield return "sendtochaterror {0}, your command must consist entirely of numbers from 1 to 12.";
	            yield break;
	        }
	    }
	    foreach(string part in commandParts)
	        pressPos(int.Parse(part)-1);
	}
	
	void TwitchHandleForcedSolve(){
	    if(isSolved)
	        return;
	    for(int i = 0; i < 12; i++){
	        if(answerSequence[i] == 1)
	            pressPos(i);
	    }
	}
}
