using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.LowLevel;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public enum TouchProcessType
{
    ViaOldInputTouch,
    ViaNewInputEnhancedTouch,
    ViaNewInputEventTrace
}

public class TouchProcessData
{
    public long beginPhaseTimeMS;
    public long endPhaseTimeMS;

    public long beginPhaseCount;
    public long endPhaseCount;
}

public class TouchPerformanceTests : MonoBehaviour
{
    readonly int kCount = Enum.GetValues(typeof(TouchProcessType)).Length;
    TouchProcessData[] m_Data;

    AndroidJavaObject m_JavaClass;
    bool m_UsingNewInputSystem;
    bool m_UsingOldInputSystem;
    string m_Error = string.Empty;

    public static GUIStyle boldStyle;
    public static GUIStyle errorStyle;

    void Start()
    {
        m_Data = new TouchProcessData[kCount];
        for (int i = 0; i < kCount; i++)
            m_Data[i] = new TouchProcessData();

        m_JavaClass = new AndroidJavaObject("com.unity3d.player.ExtendedUnityPlayer");
        if (m_JavaClass == null)
            throw new Exception("Failed to find com.unity3d.player.ExtendedUnityPlayer");
        m_UsingNewInputSystem = false;
        m_UsingOldInputSystem = false;
#if ENABLE_INPUT_SYSTEM
        m_UsingNewInputSystem = true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER 
        m_UsingOldInputSystem = true;
#endif

        if (m_UsingNewInputSystem)
        {
            EnhancedTouchSupport.Enable();
            InputSystem.onEvent += InputSystem_onEvent;
        }
    }

    private TouchProcessData GetData(TouchProcessType type)
    {
        return m_Data[(int)type];
    }

    private unsafe void InputSystem_onEvent(UnityEngine.InputSystem.LowLevel.InputEventPtr eventPtr, InputDevice device)
    {
        var touchScreenDevice = device as Touchscreen;
        if (touchScreenDevice == null)
            return;

        if (!eventPtr.IsA<StateEvent>())
            return;
        var stateEvent = StateEvent.From(eventPtr);
        if (stateEvent->stateFormat != TouchState.Format)
            return;
        var touchState = (TouchState*)stateEvent->state;

        var data = GetData(TouchProcessType.ViaNewInputEventTrace);

        if (touchState->phase == UnityEngine.InputSystem.TouchPhase.Began)
        {
            data.beginPhaseTimeMS = GetTimeMS();
            data.endPhaseTimeMS = -1;
            data.beginPhaseCount++;
        }

        if (touchState->phase == UnityEngine.InputSystem.TouchPhase.Ended)
        {
            data.endPhaseTimeMS = GetTimeMS();
            data.endPhaseCount++;
        }
    }


    private long GetTimeMS()
    {
        return m_JavaClass.CallStatic<long>("getTimeMS");
    }

    private long GetLastBeginTouchTimeMS()
    {
        return m_JavaClass.CallStatic<long>("getLastBeginTouchTimeMS");
    }

    private long GetLastEndTouchTimeMS()
    {
        return m_JavaClass.CallStatic<long>("getLastEndTouchTimeMS");
    }

    // Update is called once per frame
    void Update()
    {
        int count = 0;
        if (m_UsingNewInputSystem)
        {
            count = Touch.activeTouches.Count;

            if (count == 1)
            {
                var touch = Touch.activeTouches[0];
                var data = GetData(TouchProcessType.ViaNewInputEnhancedTouch);
                if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    data.beginPhaseTimeMS = GetTimeMS();
                    data.endPhaseTimeMS = -1;
                    data.beginPhaseCount++;
                }
                if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended)
                {
                    data.endPhaseTimeMS = GetTimeMS();
                    data.endPhaseCount++;
                }
            }
        }

        if (m_UsingOldInputSystem)
        {
            count = Input.touchCount;

            if (count == 1)
            {
                var touch = Input.GetTouch(0);
                var data = GetData(TouchProcessType.ViaOldInputTouch);
                if (touch.phase == UnityEngine.TouchPhase.Began)
                {
                    data.beginPhaseTimeMS = GetTimeMS();
                    data.endPhaseTimeMS = -1;
                    data.beginPhaseCount++;
                }
                if (touch.phase == UnityEngine.TouchPhase.Ended)
                {
                    data.endPhaseTimeMS = GetTimeMS();
                    data.endPhaseCount++;
                }
            }
        }

        m_Error = count > 1 ? "Touch screen only with one finger!" : string.Empty;
    }

    private void DoGUI(TouchProcessType type)
    {
        var data = GetData(type);
        var result = "";
        GUILayout.Label($"Begin Phase {data.beginPhaseTimeMS - GetLastBeginTouchTimeMS()}ms");
        
        if (GetLastEndTouchTimeMS() == -1)
        {
            result = "<Waiting for end phase in Java>";
        }
        else if (data.endPhaseTimeMS == -1)
        {
            result = "<Waiting for end phase in C#";
        }
        else
        {
            result = $"{data.endPhaseTimeMS - GetLastEndTouchTimeMS()}";
        }
        GUILayout.Label($"End Phase {result} ms");

        GUILayout.Label($"Begin Phase Count {data.beginPhaseCount}, End Phase Count {data.endPhaseCount}");
    }
    private void OnGUI()
    {
        if (boldStyle == null)
        {
            boldStyle = new GUIStyle("label")
            {
                fontSize = 16,
                normal = new GUIStyleState() { textColor = Color.green }
            };
        }

        if (errorStyle == null)
        {
            errorStyle = new GUIStyle("label")
            {
                fontSize = 16,
                normal = new GUIStyleState() { textColor = Color.red }
            };
        }
#if !UNITY_EDITOR
        GUI.matrix = Matrix4x4.Scale(Vector3.one * 4);
#endif
        GUILayout.Label($"Move one finger around the screen");
        if (!string.IsNullOrEmpty(m_Error))
        {
            GUILayout.Label(m_Error, errorStyle);
            return;
        }

        GUILayout.Label("Delta times are calculated between:");
        GUILayout.Label("Java UI thread (where event is received) and");
        GUILayout.Label("Unity thread (where event is processed)");

        if (m_UsingNewInputSystem)
        {
            GUILayout.Label($"[New]Enhanced Touch", boldStyle);
            DoGUI(TouchProcessType.ViaNewInputEnhancedTouch);
            GUILayout.Label("[New]Event Trace", boldStyle);
            DoGUI(TouchProcessType.ViaNewInputEventTrace);
        }

        if (m_UsingOldInputSystem)
        {
            GUILayout.Label($"[Old]Input.GetTouch", boldStyle);
            DoGUI(TouchProcessType.ViaOldInputTouch);
        }
    }
}
