/*
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



using EyeTrackerData_Namespace;
using System.Collections.Generic;
using System.Linq;
using Tobii.Research;
using UnityEngine;

namespace Tobii.Research.Unity.CodeExamples
{
    // the events in the SDK are called on a thread internal to the SDK. That thread can not safely set values
    // that are to be read on the main thread. The simplest way to make it safe is to enqueue the date, and dequeue it
    // on the main thread, e.g. via Update() in a MonoBehaviour.
    public class TobiiGazeDataSubscription : MonoBehaviour
    {
        public IEyeTracker _eyeTracker;
        private Queue<GazeDataEventArgs> _queue = new Queue<GazeDataEventArgs>();
        /*public USE_ExperimentTemplate_Data.GazeData GazeData;
        public TobiiEyeTrackerController TobiiEyeTrackerController;*/

        void Awake()
        {/*
            _eyeTracker = trackers.FirstOrDefault(s => (s.DeviceCapabilities & Capabilities.HasGazeData) != 0);
            if (_eyeTracker == null)
            {
                Debug.Log("No screen based eye tracker detected!");
            }
            else
            {
                Debug.Log("Selected eye tracker with serial number {0}" + _eyeTracker.SerialNumber);
            }*/
        }

        void Update()
        {
            PumpGazeData();
        }

        /*void OnEnable()
        {
            if (_eyeTracker != null)
            {
                Debug.Log("Calling OnEnable with eyetracker: " + _eyeTracker.DeviceName);
                _eyeTracker.GazeDataReceived += EnqueueEyeData;
            }
        }*/

        void OnDisable()
        {
            if (_eyeTracker != null)
            {
                _eyeTracker.GazeDataReceived -= EnqueueEyeData;
            }
        }

        void OnDestroy()
        {
            EyeTrackingOperations.Terminate();
        }
        void OnApplicationQuit()
        {
            EyeTrackingOperations.Terminate();
        }

        // This method will be called on a thread belonging to the SDK, and can not safely change values
        // that will be read from the main thread.
        public void EnqueueEyeData(object sender, GazeDataEventArgs e)
        {
            lock (_queue)
            {
                _queue.Enqueue(e);
            }
        }

        private GazeDataEventArgs GetNextGazeData()
        {
            lock (_queue)
            {
                return _queue.Count > 0 ? _queue.Dequeue() : null;
            }
        }

        public void PumpGazeData()
        {
            var next = GetNextGazeData();
            while (next != null)
            {
                HandleGazeData(next);
                next = GetNextGazeData();
            }
        }

        // This method will be called on the main Unity thread
        private void HandleGazeData(GazeDataEventArgs e)
        {
            TobiiGazeSample mostRecentGazeSample = new TobiiGazeSample();
            // Left Eye Data
            mostRecentGazeSample.leftPupilValidity = e.LeftEye.Pupil.Validity.ToString();
            mostRecentGazeSample.leftGazeOriginValidity = e.LeftEye.GazeOrigin.Validity.ToString();
            mostRecentGazeSample.leftGazePointValidity = e.LeftEye.GazePoint.Validity.ToString();
            mostRecentGazeSample.leftGazePointOnDisplayArea = e.LeftEye.GazePoint.PositionOnDisplayArea.ToVector2();
            mostRecentGazeSample.leftGazeOriginInUserCoordinateSystem = e.LeftEye.GazeOrigin.PositionInUserCoordinates.ToVector3();
            mostRecentGazeSample.leftGazePointInUserCoordinateSystem = e.LeftEye.GazePoint.PositionInUserCoordinates.ToVector3();
            mostRecentGazeSample.leftGazeOriginInTrackboxCoordinateSystem = e.LeftEye.GazeOrigin.PositionInTrackBoxCoordinates.ToVector3();
            mostRecentGazeSample.leftPupilDiameter = e.LeftEye.Pupil.PupilDiameter;

            // Right Eye Data
            mostRecentGazeSample.rightPupilValidity = e.RightEye.Pupil.Validity.ToString();
            mostRecentGazeSample.rightGazeOriginValidity = e.RightEye.GazeOrigin.Validity.ToString();
            mostRecentGazeSample.rightGazePointValidity = e.RightEye.GazePoint.Validity.ToString();
            mostRecentGazeSample.rightGazePointOnDisplayArea = e.RightEye.GazePoint.PositionOnDisplayArea.ToVector2();
            mostRecentGazeSample.rightGazeOriginInUserCoordinateSystem = e.RightEye.GazeOrigin.PositionInUserCoordinates.ToVector3();
            mostRecentGazeSample.rightGazePointInUserCoordinateSystem = e.RightEye.GazePoint.PositionInUserCoordinates.ToVector3();
            mostRecentGazeSample.rightGazeOriginInTrackboxCoordinateSystem = e.RightEye.GazeOrigin.PositionInTrackBoxCoordinates.ToVector3();
            mostRecentGazeSample.rightPupilDiameter = e.RightEye.Pupil.PupilDiameter;

            mostRecentGazeSample.systemTimeStamp = e.SystemTimeStamp;

            Session.TobiiEyeTrackerController.mostRecentGazeSample = mostRecentGazeSample;

            if (Session.GazeData != null)
                StartCoroutine(Session.GazeData.AppendDataToBuffer());
        }
    }
}
