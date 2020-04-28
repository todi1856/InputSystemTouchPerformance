This project is showing how much time it takes for touch event to reach Unity script where touch event is processed.

To perform a test:
* Simply Build & Run to Android
* And try touching the screen with one finger (Don't use more than one finger)
* Touch event is registered in Assets/ExtendedUnityPlayer.java onTouchEvent, at this point we memorize the time for begin phase and end phase.
  Note: onTouchEvent is called on Java's UI thread
* Depending on your input settings, the event is queued and then processed on Unity's main thread
* Assets\TouchPerformanceTests.cs contains 3 responder types for touch events:
  - Input.GetTouch (old input)
  - EnhancedTouch (new input)
  - Event Tracing (new input)
* When the touch event is received via those responders, we again memorize the time when it happened
* By using two memorized times (for phases Begin, End), we display delta time (Unity script response time minus onTouchEvent time), this shows how much time it took for touch event to reach the Unity script


I tested on Google Pixel 2, using 2019.3.11f Release Mono verison. Both Begin/Phase response time varied from 5ms to 18ms.
But important thing to note, both old input and new input response time seems to be the same.


Note: There seems to be a bug with EnhancedTouch at the time of writing this test, the first time you'll touch the screen, seems first Begin phase is not captured by Enhanced input.
