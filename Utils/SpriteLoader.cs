using System.IO;
using System.Reflection;
using UnityEngine;

namespace SimpleCall.Utils;

public static class SpriteLoader
{
    private static Sprite _signalSprite;

    public static Sprite GetSignalSprite()
    {
        if (_signalSprite != null)
            return _signalSprite;

        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"{assembly.GetName().Name}.assets.signal.jpg";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
            return null;

        var buffer = new byte[stream.Length];
        stream.Read(buffer, 0, buffer.Length);

        var texture = new Texture2D(2, 2);
        if (!ImageConversion.LoadImage(texture, buffer))
            return null;

        _signalSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        return _signalSprite;
    }
}
