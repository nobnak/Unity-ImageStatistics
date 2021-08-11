using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity_ImageStatistics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;

public class TestStatistics {

    public const byte W = 255;

    [Test]
    public void TestStatisticsSimplePasses() {

        Texture2D tex = null;
        try {
            tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            var data = new byte[] {
                W, 0, 0, W,     0, 0, 0, W,     0, W, 0, W,     0, W, 0, W,
                0, 0, 0, W,     0, 0, 0, W,     0, W, 0, W,     0, W, 0, W,
                0, 0, W, W,     0, 0, W, W,     0, W, W, W,     0, W, W, W,
                0, 0, W, W,     0, 0, W, W,     0, W, W, W,     0, W, W, W,
            };
            tex.SetPixelData(data, 0);
            tex.Apply();

            var stat = tex.Analyze();
            Debug.Log($"{stat}");

        } finally {
            CoreUtils.Destroy(tex);
        }
    }
}
