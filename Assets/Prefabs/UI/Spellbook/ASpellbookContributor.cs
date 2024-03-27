using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public delegate Texture2D ReturnTexture();
public delegate Vector2 GetContentDims();

/** 
* Anything that wishes to add content to the spellbook needs to implement this class
*/
public abstract class ASpellbookContributor: NetworkBehaviour {
    // Fire this event to acquire the base texture of size dims to blit on
    public static event ReturnTexture GetBaseHalfPageTexture = delegate { return null; };
    // Fire this event to acquire the base normal map of size dims to blit on
    public static event ReturnTexture GetBaseHalfPageNormalMap = delegate { return null; };
    // Fire this event to acquire the base notification texture (of size dims)
    public static event ReturnTexture GetHalfPageNotifTexture = delegate { return null; };
    // Fire this event to acquire the dimensions of the full page texture (includes sides and flip side, which are useless)
    public static event GetContentDims GetFullPageDims = delegate { return new Vector2(); };
    // Fire this event to acquire the dimensions of a half page -- this is the size of the provided textures above
    public static event GetContentDims GetHalfPageDims = delegate { return new Vector2(); };

    // Fire this event providing the blitted dim-sized content texture, content with new notification variant, and normal texture
    public static event Action<Texture2D, Texture2D, Texture2D> OnContributeContent = delegate {}; 

    private Texture2D _bakedFullTexture;
    private Texture2D _bakedFullTextureNotif;
    private Texture2D _bakedFullNormalMap;

    /** 
    * Scale the given texture on the GPU to the new size
    */
    private Texture2D _RescaleTexture(Texture2D texture2D, int targetX, int targetY) {
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
    private void _CPUAlphaBlit(Texture2D src, Vector2 offset, Texture2D dest) {
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
    protected void Bake(Texture2D texture, Texture2D normal) {
        // Acquire details
        Vector2 fullDims = GetFullPageDims();
        Vector2 halfDims = GetHalfPageDims();
        Texture2D tex = GetBaseHalfPageTexture();
        Texture2D notifOverlay = GetHalfPageNotifTexture();
        Texture2D norm = GetBaseHalfPageNormalMap();

        _bakedFullTexture = new Texture2D((int)fullDims.x, (int)fullDims.y);
        _bakedFullTextureNotif = new Texture2D((int)fullDims.x, (int)fullDims.y);
        _bakedFullNormalMap = new Texture2D((int)fullDims.x, (int)fullDims.y);

        // Scale given textures to half page size
        var scaledTexture = _RescaleTexture(texture, (int)halfDims.x, (int)halfDims.y);
        var scaledNormal = _RescaleTexture(normal, (int)halfDims.x, (int)halfDims.y);

        // Draw baked halfpage
        Texture2D finalTexture = new Texture2D((int)halfDims.x, (int)halfDims.y);
        Texture2D finalTextureNotif = new Texture2D((int)halfDims.x, (int)halfDims.y);
        Texture2D finalNormal = new Texture2D((int)halfDims.x, (int)halfDims.y);
        
        Graphics.CopyTexture(tex, 0, 0, finalTexture, 0, 0);
        Graphics.CopyTexture(tex, 0, 0, finalTextureNotif, 0, 0);
        Graphics.CopyTexture(norm, 0, 0, finalNormal, 0, 0);

        _CPUAlphaBlit(scaledTexture, new Vector2(0, 0), finalTexture);
        _CPUAlphaBlit(scaledTexture, new Vector2(0, 0), finalTextureNotif);
        _CPUAlphaBlit(notifOverlay, new Vector2(0, 0), finalTextureNotif);
        _CPUAlphaBlit(scaledNormal, new Vector2(0, 0), finalNormal);

        // Tile blit half page across full texture and save
        RenderTexture rt = new RenderTexture((int)fullDims.x, (int)fullDims.y, 32, RenderTextureFormat.ARGB32);
        Graphics.Blit(finalTexture, rt, new Vector2(fullDims.x / halfDims.x, fullDims.y / halfDims.y), new Vector2(0, 0));
        _bakedFullTexture.ReadPixels(new Rect(0, 0, fullDims.x, fullDims.y), 0, 0);
        _bakedFullTexture.Apply();

        rt = new RenderTexture((int)fullDims.x, (int)fullDims.y, 32, RenderTextureFormat.ARGB32);
        Graphics.Blit(finalTextureNotif, rt, new Vector2(fullDims.x / halfDims.x, fullDims.y / halfDims.y), new Vector2(0, 0));
        _bakedFullTextureNotif.ReadPixels(new Rect(0, 0, fullDims.x, fullDims.y), 0, 0);
        _bakedFullTextureNotif.Apply();

        rt = new RenderTexture((int)fullDims.x, (int)fullDims.y, 32, RenderTextureFormat.ARGB32);
        Graphics.Blit(finalNormal, rt, new Vector2(fullDims.x / halfDims.x, fullDims.y / halfDims.y), new Vector2(0, 0));

        //Graphics.CopyTexture(finalNormal, 0, 0, 0, 0, (int)halfDims.x, (int)halfDims.y, _bakedFullNormalMap, 0, 0, 0, 0);

        _bakedFullNormalMap.ReadPixels(new Rect(0, 0, fullDims.x, fullDims.y), 0, 0);
        _bakedFullNormalMap.Apply();

        byte[] _bytes = _bakedFullTexture.EncodeToPNG();
        System.IO.File.WriteAllBytes("C:/Users/Badlek/baked.png", _bytes);
        _bytes = _bakedFullTextureNotif.EncodeToPNG();
        System.IO.File.WriteAllBytes("C:/Users/Badlek/baked1.png", _bytes);
        _bytes = _bakedFullNormalMap.EncodeToPNG();
        System.IO.File.WriteAllBytes("C:/Users/Badlek/baked2.png", _bytes);
    }

    protected void AddBakedContentToSpellbook() {
        if (_bakedFullTexture != null) {
            OnContributeContent(_bakedFullTexture, _bakedFullTextureNotif, _bakedFullNormalMap);
        }
    }
}



