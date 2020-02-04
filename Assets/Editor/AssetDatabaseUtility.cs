using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class AssetDatabaseUtility
{

    public static string AbsoluteUrlToAssets(string absoluteUrl)
    {
        Uri fullPath = new Uri(absoluteUrl, UriKind.Absolute);
        Uri relRoot = new Uri(Application.dataPath, UriKind.Absolute);

        return relRoot.MakeRelativeUri(fullPath).ToString();


    }
}
