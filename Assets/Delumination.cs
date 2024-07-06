using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Delumination : MonoBehaviour {
	
    public KMBombModule module;
    public KMBombInfo bombInfo;
    public KMAudio audio;
    public KMRuleSeedable rs;
	
	private bool isSolved;
	private static int moduleCount;
    private int moduleId;



	void Start () {
		
	}
	
	void Awake () {
		moduleId = moduleCount++;
	}
}
