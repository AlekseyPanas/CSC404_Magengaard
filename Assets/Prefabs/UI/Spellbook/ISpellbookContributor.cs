using System;
using System.Collections.Generic;
using UnityEngine;

public delegate Texture2D ReturnTexture();
public delegate Vector2 GetContentDims();

/** 
* Anything that wishes to add content to the spellbook needs to implement this class
*/
public interface ISpellbookContributor {
    // Fire this event to acquire the base texture of size dims to blit on
    public static ReturnTexture GetBaseTextureEvent = delegate { return null; };
    // Fire this event to acquire the base normal map of size dims to blit on
    public static ReturnTexture GetBaseNormalMapEvent = delegate { return null; };
    // Fire this event to acquire the base notification texture (of size dims)
    public static ReturnTexture GetNotifTextureEvent = delegate { return null; };
    // Fire this event to acquire the dimensions of the spellbook
    public static GetContentDims GetContentDimsEvent = delegate { return new Vector2(); };

    // Fire this event providing the blitted dim-sized content texture, content with new notification variant, and normal texture
    public static Action<Texture2D, Texture2D, Texture2D> OnContributeContent = delegate {}; 

    /** 
    * Scale the given texture on the GPU to the new size
    */
    public static Texture2D RescaleTexture(Texture2D texture2D, int targetX, int targetY) {
        RenderTexture rt = new RenderTexture(targetX, targetY, 24);
        RenderTexture.active = rt;
        Graphics.Blit(texture2D, rt);
        Texture2D result = new Texture2D(targetX, targetY);
        result.ReadPixels(new Rect(0, 0, targetX, targetY), 0, 0);
        result.Apply();
        return result;
    }

    /** 
    * Blit src onto dest with the given offset including alphas using expensive CPU-side SetPixel ops
    */
    public static void CPUAlphaBlit(Texture2D src, Vector2 offset, Texture2D dest) {
        // Looks expensive af, hoping its not noticeable given how rarely this runs
        // Alpha blits the new scaled content onto newContentBase. Graphics.blit sadly doesnt support alphas hence this CPU side monstrosity
        for (int x = 0; x < src.width; x++) {
            for (int y = 0; y < src.height; y++) {
                Color pixSrc = src.GetPixel(x, y);

                if (pixSrc.a > 0) {
                    Color destSrc = dest.GetPixel((int)(offset.x + x), (int)(offset.y + y));

                    Color blendedCol = (pixSrc * pixSrc.a) + (destSrc * (1 - pixSrc.a));
                    blendedCol.a = 1;

                    dest.SetPixel((int)(offset.x + x), (int)(offset.y + y), blendedCol);
                }
            }
        } dest.Apply();
    }

    /**
    * Get base textures and prepare the final baked versions in a tuple <texture, notif texture, normal texture>
    */
    public static Tuple<Texture2D, Texture2D, Texture2D> GetBaked(Texture2D texture, Texture2D normal) {
        Vector2 dims = GetContentDimsEvent();
        Texture2D tex = GetBaseTextureEvent();
        Texture2D notifOverlay = GetNotifTextureEvent();
        Texture2D norm = GetBaseNormalMapEvent();

        var scaledTexture = RescaleTexture(texture, (int)dims.x, (int)dims.y);
        var scaledNormal = RescaleTexture(normal, (int)dims.x, (int)dims.y);

        Texture2D finalTexture = new Texture2D((int)dims.x, (int)dims.y);
        Texture2D finalTextureNotif = new Texture2D((int)dims.x, (int)dims.y);
        Texture2D finalNormal = new Texture2D((int)dims.x, (int)dims.y);
        
        Graphics.CopyTexture(tex, 0, 0, finalTexture, 0, 0);
        Graphics.CopyTexture(tex, 0, 0, finalTextureNotif, 0, 0);
        Graphics.CopyTexture(norm, 0, 0, finalNormal, 0, 0);

        CPUAlphaBlit(scaledTexture, new Vector2(0, 0), finalTexture);
        CPUAlphaBlit(scaledTexture, new Vector2(0, 0), finalTextureNotif);
        CPUAlphaBlit(notifOverlay, new Vector2(0, 0), finalTextureNotif);
        CPUAlphaBlit(scaledNormal, new Vector2(0, 0), finalNormal);

        int i = 0;
        foreach (Texture2D t in new List<Texture2D>(){finalTexture, finalNormal, finalTextureNotif}) {
            byte[] _bytes = t.EncodeToPNG();
            System.IO.File.WriteAllBytes("C:/Users/Badlek/" + i + ".png", _bytes);
            i++;
        }
        
        return new(finalTexture, finalTextureNotif, finalNormal);
    }
}



