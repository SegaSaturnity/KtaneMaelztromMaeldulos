﻿using Wawa.Modules;
using Wawa.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class _breakersScript:ModdedModule{

    public KMRuleSeedable rs;
    public Material[]colors;
    internal bool[,]finalPositions=new bool[3,4];
    private bool[]LR=new bool[]{false, true};
    private char[]letters=new char[]{'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Z'};
    internal bool[]currentBlackPositions=new bool[4]{false,false,false,false};
    internal bool[]currentColorfulPositions=new bool[3]{false,false,false};
    public KMSelectable[]blackBreakers;
    public KMSelectable[]colorfulBreakers;
    public MeshRenderer[]colorfulHandles;
    public TextMesh colorblindText;
    public KMBombInfo bomb;
    public KMColorblindMode colorblind;
    private string serial;
    private int highestDigit;
    private bool allBreakersToRight=false;
    private bool[,,]startingPositions=new bool[4,5,4];
    private char[,,]furtherAdjustments=new char[3,4,4];
    internal bool hasStruck = false;

    void Start(){
        var RND = rs.GetRNG();
        if(RND.Seed == 1){
            startingPositions = new bool[,,]{
                {{true, false, false, false}, {false, false, false, true}, {true, false, true, true}, {true, true, true, false}, {true, false, true, false}},
                {{false, false, false, false}, {true, true, false, false}, {false, true, false, true}, {false, true, true, false}, {false, false, true, false}},
                {{false, true, true, true}, {false, false, true, true}, {true, false, false, true}, {false, true, false, false}, {true, true, false, true}},
                {{true, true, false, true}, {false, true, true, false}, {false, false, true, false}, {true, false, true, true}, {false, true, false, true}}
            };
            furtherAdjustments = new char[,,]{
                {{'E', 'G', '4', '9'}, {'I', 'N', '5', '6'}, {'D', 'T', '2', '3'}, {'C', 'L', '7', '8'}},
                {{'Q', 'U', '2', '8'}, {'P', 'Z', '3', '7'}, {'H', 'V', '4', '5'}, {'B', 'W', '6', '9'}},
                {{'J', 'R', '6', '7'}, {'M', 'X', '2', '9'}, {'K', 'A', '5', '8'}, {'S', 'F', '3', '4'}}
            };
        }else{
            RND.ShuffleFisherYates(letters);
            for(int i = 0; i < 4; i++){
                for(int j = 0; j < 5; j++){
                    for(int k = 0; k < 4; k++){
                        startingPositions[i,j,k]=LR[RND.Next(0,2)];
                    }
                }
            }
            int index = 0;
            for(int i = 0; i < 3; i++){
                for(int j = 0; j < 4; j++){
                    for(int k = 0; k < 2; k++){
                        furtherAdjustments[i,j,k] = letters[index];
                        index++;
                    }
                    for(int k = 2; k < 4; k++){
                        furtherAdjustments[i,j,k] = (char)(RND.Next(0, 10) + 48);
                    }
                }
            }
        }
        foreach(KMSelectable blackB in blackBreakers){
            blackB.Set(onInteract:()=>{
                StartCoroutine(flipBreaker(blackB,Array.IndexOf(blackBreakers,blackB),false));
            });
        }
        foreach(KMSelectable colorfulB in colorfulBreakers){
            colorfulB.Set(onInteract:()=>{
                StartCoroutine(flipBreaker(colorfulB,Array.IndexOf(colorfulBreakers,colorfulB),true));
            });
        }
        serial=bomb.GetSerialNumber();
        highestDigit=highest(Enumerable.Max(bomb.GetSerialNumberNumbers()));
        colorBreakersIn();
	}

	private int highest(int digit){
        if(digit>=0&&digit<=4)
            return 0;
        else if(digit>=5&&digit<=6)
            return 1;
        else if(digit==7)
            return 2;
        else if(digit==8)
            return 3;
        else if(digit==9)
            return 4;
        else throw new ArgumentOutOfRangeException("The int highest(int digit) method was called with a number that was either negative or greater than 10.");
	}

    private IEnumerator flipBreaker(KMSelectable breaker,int index,bool isColorful){
        Shake(breaker,.5f,Sound.BigButtonPress);
        if(allBreakersToRight)
            yield break;
        hasStruck=false;
        if((!isColorful&&!currentBlackPositions[index])||(isColorful&&!currentColorfulPositions[index])){
            for(int i=0;i<10;i++){
                breaker.GetComponent<Transform>().Rotate(0f,0f,-9f);
                yield return null;
            }
            if(isColorful&&!colorfulBreakerCanBeFlipped(index)){
                string ordinal="";
                switch(index){
                    case 0:
                        ordinal="first";
                        break;
                    case 1:
                        ordinal="second";
                        break;
                    case 2:
                        ordinal="third";
                        break;
                }
                string[]positionsWhenStruck=new string[4];
                string[]positionsIntended=new string[4];
                for(int i=0;i<4;i++){
                    positionsWhenStruck[i]=currentBlackPositions[i]?"right":"left";
                    positionsIntended[i]=finalPositions[index,i]?"right":"left";
                }
                Strike("Strike! Flipped the "+ordinal+" breaker when the positions were "+string.Join(", ", positionsWhenStruck)+". The correct positions are "+string.Join(", ", positionsIntended));
                Play(Sound.BigButtonPress);
                for(int i=0;i<10;i++){
                    breaker.GetComponent<Transform>().Rotate(0f,0f,9f);
                    yield return null;
                }
                hasStruck=true;
            }
        }else if((!isColorful&&currentBlackPositions[index])||(isColorful&&currentColorfulPositions[index])){
            for(int i=0;i<10;i++){
                breaker.GetComponent<Transform>().Rotate(0f,0f,9f);
                yield return null;
            }
        }
        if(isColorful&&!hasStruck)
            currentColorfulPositions[index]=!currentColorfulPositions[index];
        else if(!isColorful)
            currentBlackPositions[index]=!currentBlackPositions[index];
        allBreakersToRight=true;
        for(int i=0;i<4;i++){
            if(!currentBlackPositions[i]||(i<3&&!currentColorfulPositions[i])){
                allBreakersToRight=false;
                break;
            }
        }
        if(allBreakersToRight)
            Solve("All breakers have been flipped to the right! The module is now solved!");
    }

    private bool colorfulBreakerCanBeFlipped(int colorfulBreakerPosition){
        for(int i=0;i<4;i++){
            if(finalPositions[colorfulBreakerPosition,i]!=currentBlackPositions[i])
                return false;
        }
        return true;
    }

    private void colorBreakersIn(){
        string[]colorNames=new string[]{"red","blue","green","yellow"};
        char[]colorLetters=new char[]{'R','B','G','Y'};
        int color1=UnityEngine.Random.Range(0,4);
        int color2;
        do{
            color2=UnityEngine.Random.Range(0,4);
        }while(color2==color1);
        int color3;
        do{
            color3=UnityEngine.Random.Range(0,4);
        }while(color3==color1||color3==color2);
        colorfulHandles[0].material=colors[color1];
        colorfulHandles[1].material=colors[color2];
        colorfulHandles[2].material=colors[color3];
        if(colorblind.ColorblindModeActive){
            colorblindText.gameObject.SetActive(true);
            colorblindText.text=""+colorLetters[color1]+colorLetters[color2]+colorLetters[color3];
        }
        Log("The colorful breakers are "+colorNames[color1]+", "+colorNames[color2]+", and "+colorNames[color3]+" from top to bottom.");
        calculateFinalPositions(color1,color2,color3);
    }

    private void calculateFinalPositions(int c1,int c2,int c3){
        for(int i=0;i<4;i++){
            finalPositions[0,i]=startingPositions[c1,highestDigit,i];
            finalPositions[1,i]=startingPositions[c2,highestDigit,i];
            finalPositions[2,i]=startingPositions[c3,highestDigit,i];
        }
        Log("Initial:");
        loggingResult();
        for(int i=0;i<3;i++){
            for(int j=0;j<4;j++){
                for(int k=0;k<4;k++){
                    if(serial.Count(x => x == furtherAdjustments[i,j,k])%2==1)
                        finalPositions[i,j]=!finalPositions[i,j];
                }
            }
        }
        Log("Final:");
        loggingResult();
    }

    private void loggingResult(){
        string result="";
        int v=0;
        foreach(bool pos in finalPositions){
            result+=pos?"R":"L";
            v+=1;
            if(v%4==0){
                Log(result);
                v=0;
                result="";
            }
        }
    }
}
