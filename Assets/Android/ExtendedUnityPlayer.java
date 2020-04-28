package com.unity3d.player;

import android.content.Context;
import android.os.SystemClock;
import android.view.MotionEvent;

public class ExtendedUnityPlayer extends UnityPlayer
{
    static long m_LastBeginTouchTimeMS = 0;
    static long m_LastEndTouchTimeMS = 0;

    public ExtendedUnityPlayer(Context context, IUnityPlayerLifecycleEvents lifecycleEventListener)
    {
        super(context, lifecycleEventListener);
    }

    @Override public boolean onTouchEvent(MotionEvent event)
    {
        if (event.getAction() == MotionEvent.ACTION_DOWN || event.getAction() == MotionEvent.ACTION_POINTER_DOWN) {
            m_LastBeginTouchTimeMS = getTimeMS();
            m_LastEndTouchTimeMS = -1;
        }
        if (event.getAction() == MotionEvent.ACTION_UP || event.getAction() == MotionEvent.ACTION_POINTER_UP)
            m_LastEndTouchTimeMS = getTimeMS();
        return super.onTouchEvent(event);
    }

    public static long getTimeMS()
    {
        return SystemClock.uptimeMillis();
    }

    public static long getLastBeginTouchTimeMS()
    {
        return m_LastBeginTouchTimeMS;
    }

    public static long getLastEndTouchTimeMS()
    {
        return m_LastEndTouchTimeMS;
    }


}