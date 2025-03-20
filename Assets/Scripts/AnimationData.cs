using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct KeyFrame
{
    public int val;
    public int time;
    public int inTan;
    public int outTan;
}

[System.Serializable]
public struct Curve
{
    public List<KeyFrame> points;
}

[System.Serializable]
public struct Vector3Curve
{
    public Curve x;
    public Curve y;
    public Curve z;
}

[System.Serializable]
public struct QuaternionCurve
{
    public Curve x;
    public Curve y;
    public Curve z;
    public Curve w;
}

[System.Serializable]
public struct Event
{
    public int type;
    public int time;
}

[System.Serializable]
public class AnimationData : ScriptableObject
{
    public int length;

    public List<Event> eventList;

    public Vector3Curve positionCurve;

    public QuaternionCurve rotationCurve;
}
