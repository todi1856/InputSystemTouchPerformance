using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor.Android;

public class PostProcess : IPostGenerateGradleAndroidProject
{
    public int callbackOrder => throw new System.NotImplementedException();

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        var files = Directory.GetFiles(path, "UnityPlayerActivity.java", SearchOption.AllDirectories);
        if (files.Length == 0)
            throw new System.Exception("Failed to find UnityPlayerActivity.java");
        var activity = files[0];
        var contents = File.ReadAllText(activity);
        var tag = "new UnityPlayer";
        if (contents.IndexOf(tag) == -1)
        {
            throw new System.Exception("Failed to find tag: " + tag);
        }
        contents = contents.Replace(tag, "new ExtendedUnityPlayer");
        File.WriteAllText(activity, contents);
        Debug.Log("Patch succesful");
    }
}
