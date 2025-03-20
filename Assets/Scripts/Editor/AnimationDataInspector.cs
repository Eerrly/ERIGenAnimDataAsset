using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AnimationData))]
public class AnimationDataInspector : Editor
{
    private const float kToFloatFactor = 1 / 10000.0f;
    
    private AnimationCurve _rpx, _rpy, _rpz;
    private AnimationCurve _rrx, _rry, _rrz, _rrw;
    
    private void GetCurve(List<KeyFrame> keyframes, out AnimationCurve animationCurve)
    {
        animationCurve = new AnimationCurve();
        for (var i = 0; i < keyframes.Count; ++i)
        {
            animationCurve.AddKey(new Keyframe()
            {
                time = keyframes[i].time * kToFloatFactor,
                value = keyframes[i].val * kToFloatFactor,
                inTangent = keyframes[i].inTan * kToFloatFactor,
                outTangent = keyframes[i].outTan * kToFloatFactor,
            });
            AnimationUtility.SetKeyLeftTangentMode(animationCurve, i, AnimationUtility.TangentMode.Free);
            AnimationUtility.SetKeyRightTangentMode(animationCurve, i, AnimationUtility.TangentMode.Free);
        }
    }

    public void OnEnable()
    {
        var animationData = this.target as AnimationData;
        if (animationData == null)
            return;
        
        GetCurve(animationData.positionCurve.x.points, out _rpx);
        GetCurve(animationData.positionCurve.y.points, out _rpy);
        GetCurve(animationData.positionCurve.z.points, out _rpz);
        GetCurve(animationData.rotationCurve.x.points, out _rrx);
        GetCurve(animationData.rotationCurve.y.points, out _rry);
        GetCurve(animationData.rotationCurve.z.points, out _rrz);
        GetCurve(animationData.rotationCurve.w.points, out _rrw);
    }

    private bool _showPosition = true;
    private bool _showRotation = true;
    private bool _showEvent = true;
    public override void OnInspectorGUI()
    {
        var animationData = this.target as AnimationData;
        if (animationData == null)
            return;
        
        EditorGUILayout.FloatField("Length:", animationData.length);
        EditorGUILayout.Space();
        _showPosition = EditorGUILayout.Foldout(_showPosition, "Position", true);
        if (_showPosition)
        {
            EditorGUI.indentLevel = 1;
            EditorGUILayout.CurveField("X", _rpx);
            EditorGUILayout.CurveField("Y", _rpy);
            EditorGUILayout.CurveField("Z", _rpz);
            EditorGUI.indentLevel = 0;
        }
        EditorGUILayout.Space();
        _showRotation = EditorGUILayout.Foldout(_showRotation, "Rotation", true);
        if (_showRotation)
        {
            EditorGUI.indentLevel = 1;
            EditorGUILayout.CurveField("X", _rrx);
            EditorGUILayout.CurveField("Y", _rry);
            EditorGUILayout.CurveField("Z", _rrz);
            EditorGUILayout.CurveField("W", _rrw);
            EditorGUI.indentLevel = 0;
        }
        EditorGUILayout.Space();
        _showEvent = EditorGUILayout.Foldout(_showEvent, "Events", true);
        if (_showEvent)
        {
            EditorGUI.indentLevel = 1;
            EditorGUILayout.IntField("Size:", animationData.eventList.Count);
            EditorGUILayout.Space();
            for (int index = 0; index < animationData.eventList.Count; ++index)
            {
                EditorGUILayout.TextField("Event:", ((EEvent)animationData.eventList[index].type).ToString());
                EditorGUILayout.TextField("Time:", (animationData.eventList[index].time).ToString());
                EditorGUILayout.Space();
            }
            EditorGUI.indentLevel = 0;
        }
    }
}