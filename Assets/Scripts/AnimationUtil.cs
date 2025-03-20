using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class AnimationUtil
{
    private const float kToFloatFactor = 1 / 10000.0f;
    
    public static List<Event> ReadEventsFromFile(string filePath, out bool loop)
    {
        loop = false;
        var eventList = new List<Event>();
        using (var sr = new StreamReader(filePath))
        {
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.Length == 0)
                    continue;
                if (line == "loop")
                {
                    loop = true;
                    continue;
                }

                var separators = new string[] { ",", "_", " " };
                var values = line.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                if (values.Length != 2)
                    continue;

                var eventName = values[0];
                var eventValue = values[1];
                if (string.IsNullOrEmpty(eventName) || string.IsNullOrEmpty(eventValue))
                    continue;
                
                eventList.Add(new Event()
                {
                    type = (int)Enum.Parse(typeof(EEvent), eventName),
                    time = int.Parse(eventValue),
                });
            }
        }
        eventList.Sort((l, r) => l.time.CompareTo(r.time));
        return eventList;
    }

    private static Curve GenerateCurve(AnimationCurve animationCurve)
    {
        var curve = new Curve();
        curve.points = new List<KeyFrame>();
        var keyFrame = new KeyFrame();
        for (int i = 0; i < animationCurve.length; i++)
        {
            keyFrame.val = (int)(animationCurve[i].value / kToFloatFactor);
            keyFrame.time = (int)(animationCurve[i].time / kToFloatFactor);
            keyFrame.inTan = (int)(animationCurve[i].inTangent / kToFloatFactor);
            keyFrame.outTan = (int)(animationCurve[i].outTangent / kToFloatFactor);
            curve.points.Add(keyFrame);
        }
        return curve;
    }

    private static void MakeAnimationCurves(AnimationClip clip, AnimationData data)
    {
        var bindings = AnimationUtility.GetCurveBindings(clip);
        foreach (var binding in bindings)
        {
            if (binding.propertyName.ToLower().Contains("scale"))
                AnimationUtility.SetEditorCurve(clip, binding, null);
            
            var curve = AnimationUtility.GetEditorCurve(clip, binding);
            if (binding.propertyName == "m_LocalPosition.x")
                data.positionCurve.x = GenerateCurve(curve);
            else if (binding.propertyName == "m_LocalPosition.y")
                data.positionCurve.y = GenerateCurve(curve);
            else if (binding.propertyName == "m_LocalPosition.z")
                data.positionCurve.z = GenerateCurve(curve);
            else if (binding.propertyName == "m_LocalRotation.x")
                data.rotationCurve.x = GenerateCurve(curve);
            else if (binding.propertyName == "m_LocalRotation.y")
                data.rotationCurve.y = GenerateCurve(curve);
            else if (binding.propertyName == "m_LocalRotation.z")
                data.rotationCurve.z = GenerateCurve(curve);
            else if (binding.propertyName == "m_LocalRotation.w")
                data.rotationCurve.w = GenerateCurve(curve);
        }
    }

    public static void ImportAnimationDataAndClip(string srcPath, string destDir)
    {
        var fileName = Path.GetFileNameWithoutExtension(srcPath);
        var srcAnimClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(srcPath);
        if (srcAnimClip == null)
        {
            Debug.Log($"ImportAnimationDataAndClip srcAnimClip null! srcPath: {srcPath}");
            return;
        }
        var modelImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(srcAnimClip)) as ModelImporter;
        if (modelImporter == null)
        {
            Debug.Log($"ImportAnimationDataAndClip modelImporter null!");
            return;
        }

        modelImporter.animationType = ModelImporterAnimationType.Generic;
        modelImporter.animationCompression = ModelImporterAnimationCompression.Optimal;
        modelImporter.resampleCurves = true;
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(modelImporter));

        var clip = Object.Instantiate(srcAnimClip);
        clip.name = fileName;
        
        var serializedObject = new SerializedObject(clip);
        var serializedObjectProperty = serializedObject.FindProperty("m_AnimationClipSettings");
        var srcDir = Path.GetDirectoryName(srcPath);
        if (string.IsNullOrEmpty(srcDir))
            return;
        var clipEvents = ReadEventsFromFile(Path.Combine(srcDir, $"{fileName}.txt"), out var loop);
        serializedObjectProperty.FindPropertyRelative("m_LoopTime").boolValue = loop;
        serializedObject.ApplyModifiedProperties();

        AnimationData animationData = null;
        if (!loop)
        {
            animationData = ScriptableObject.CreateInstance<AnimationData>();
            animationData.length = (int)clip.length;
            animationData.eventList = clipEvents;
            MakeAnimationCurves(clip, animationData);
        }
        var animDestFile = Path.Combine(destDir, $"{fileName}.anim");
        Debug.Log($"ImportAnimationDataAndClip animDestFile: {animDestFile}");
        AssetDatabase.CreateAsset(clip, animDestFile);
        AssetDatabase.ImportAsset(animDestFile);

        if (animationData != null)
        {
            var assetDestFile = Path.Combine(destDir, $"{fileName}.asset");
            Debug.Log($"ImportAnimationDataAndClip assetDestFile: {assetDestFile}");
            AssetDatabase.CreateAsset(animationData, assetDestFile);
            AssetDatabase.ImportAsset(assetDestFile);
        }
        AssetDatabase.Refresh();
    }
    
}
