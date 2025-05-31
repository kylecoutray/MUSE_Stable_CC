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



using UnityEngine;


public class FullScreenController : MonoBehaviour
{
    private bool IsFullScreen;
    public event System.Action<bool> FullScreenChangedEvent;
    private readonly int TargetWidth = 1920;
    private readonly int TargetHeight = 1080;

    void Start()
    {
        IsFullScreen = Screen.fullScreen;
        Screen.SetResolution(TargetWidth, TargetHeight, IsFullScreen);
    }

    void Update()
    {
        if (IsFullScreen != Screen.fullScreen)
        {
            IsFullScreen = Screen.fullScreen;
            Screen.SetResolution(TargetWidth, TargetHeight, IsFullScreen);
            OnFullScreenChanged(IsFullScreen);
        }
    }

    protected virtual void OnFullScreenChanged(bool isFullScreen)
    {
        FullScreenChangedEvent?.Invoke(IsFullScreen);
    }

    public void SubscribeToFullScreenChanged(System.Action<bool> delegateMethod)
    {
        FullScreenChangedEvent += delegateMethod;
    }

    public void UnsubscribeToFullScreenChanged(System.Action<bool> delegateMethod)
    {
        FullScreenChangedEvent -= delegateMethod;
    }

}

