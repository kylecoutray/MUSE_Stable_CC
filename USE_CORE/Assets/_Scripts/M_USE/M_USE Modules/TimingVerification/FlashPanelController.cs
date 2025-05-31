﻿/*
MIT License

Copyright (c) 2023 Multitask - Unified - Suite -for-Expts

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files(the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/



using UnityEngine;
using UnityEngine.UI;
using System;


public class FlashPanelController : MonoBehaviour
{
	private Image panelImageL;
	private Image panelImageR;

	//public static bool runPattern;
	private int[] leftSequence;
	private int[] rightSequence;

	private int leftSequenceCount = 0;
	private int rightSequenceCount = 0;

	public float leftLuminanceFactor = 0;
	public float rightLuminanceFactor = 0;

	private int leftSegmentLength = 1;
	private int rightSegmentLength = 3;

	private int frameCounter = 0;
    public bool runPattern; 

	// Use this for initialization
	void Start()
	{
		FindPanels();
		leftSequence = MakeSequence(leftSegmentLength);
		rightSequence = MakeSequence(rightSegmentLength);
	}

	public void FindPanels()
	{
		if(panelImageL == null)
		{
			panelImageL = GameObject.Find("FlashPanelL").GetComponent<Image>();
			panelImageR = GameObject.Find("FlashPanelR").GetComponent<Image>();
		}
    }

	private void Update()
	{
		if (runPattern)
			RunPattern();
	}

	public void TurnOffFlashPanels()
    {
		if(panelImageL.gameObject != null)
			panelImageL.gameObject.SetActive(false);
		if(panelImageR.gameObject != null)
			panelImageR.gameObject.SetActive(false);
    }

	public void ReverseFlipBothSquares(){
		if (Time.frameCount % 1 == 0)
		{
			rightLuminanceFactor = leftLuminanceFactor;
			leftLuminanceFactor = Math.Abs(leftLuminanceFactor - 1);
			SetSquareColours(leftLuminanceFactor, rightLuminanceFactor);
		}
	}

	public void FlipBothSquares()
	{
		if (Time.frameCount % 1 == 0)
		{
			leftLuminanceFactor = Math.Abs(leftLuminanceFactor - 1);
			rightLuminanceFactor = leftLuminanceFactor;
			SetSquareColours(leftLuminanceFactor, rightLuminanceFactor);
		}
	}

	public void RunPattern()
	{
		if (Time.frameCount % 1 == 0)
		{
			//the mod should be left to 1 unless debugging, it indicates how many frames a single element of the sequences should last.
			//so this will usually be 1, but if you want to see each black/white shift you might change it to 20, or even 60 (in which
			//case a single element will last an entire second at 60FPS.
			leftLuminanceFactor = leftSequence[leftSequenceCount];
			rightLuminanceFactor = rightSequence[rightSequenceCount];
			rightSequenceCount++;
			leftSequenceCount++;
			if (leftSequenceCount == leftSequence.Length)
			{
				leftSequenceCount = 0;
			}

			if (rightSequenceCount == rightSequence.Length)
			{
				rightSequenceCount = 0;
			}

			//leftLuminanceFactor = 1f;
			//rightLuminanceFactor = 1f;
			//rightLuminanceFactor =  Math.Abs (leftLuminanceFactor - 1);
			SetSquareColours(leftLuminanceFactor, rightLuminanceFactor);
		}
	}


    public void SetSquareColours(float leftLum, float rightLum)
    {
		leftLum = Mathf.Clamp(leftLum, 0.2f, 0.8f);
		rightLum = Mathf.Clamp(rightLum, 0.2f, 0.8f);

		Vector4 leftColour = new Vector4(leftLum, leftLum, leftLum, 1);
        Vector4 rightColour = new Vector4(rightLum, rightLum, rightLum, 1);

        panelImageL.color = leftColour;
        panelImageR.color = rightColour;
    }


    public void CalibratePhotodiodes() {
		leftLuminanceFactor = Mathf.Abs (leftLuminanceFactor - 1);
		Vector4 squareColour = new Vector4 (leftLuminanceFactor * 1, leftLuminanceFactor * 1, leftLuminanceFactor * 1, 1);
		panelImageL.color = squareColour;
		panelImageR.color = squareColour;
		//			calibratePhotodiodes = false;
		//			OptiExperimentController.newSession = true;
		//			runPattern = true;
		//			OptiUDPController.SendString ("ARDUINO CMD:CAF " + 10000 * freerunningCalbirationDuration);
	}


	int[] MakeSequence(int segmentLength){
		//Create a [2^N, N] binary table, where N = the length of each binary number
		//(and 2^N is the number of possible numbers composed of that length)
		//then we create a N x 2^N sequence, which is just each of the numbers in order.
		//So, e.g., if we want numbers of length 2, the sequence is 00011011 (0,1,2,3 in decimal)

		string seqString = "";
		for (int segCount = 0; segCount < (int)Math.Pow (2, segmentLength); segCount++) {
			seqString = seqString + ToBin (segCount, segmentLength);
		}

		int[] sequence = new int[seqString.Length];
		for (int eleCount = 0; eleCount < seqString.Length; eleCount++) {
			sequence [eleCount] = Convert.ToInt32(Char.ToString(seqString [eleCount]));
		}
		return sequence;
	}

	string ToBin(int value, int len) {
		string thisString = (len > 1 ? ToBin(value >> 1, len - 1) : null) + "01"[value & 1];
		return thisString;
	}
}
