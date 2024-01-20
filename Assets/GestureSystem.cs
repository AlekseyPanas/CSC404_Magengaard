using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GestureSystem : MonoBehaviour
{

    // Threshold for accuracy required to spawn fireball
    public static readonly double ACC_THRESH = 0.1;

    private Gest genGest;
    private Gest playGest;
    private bool generating;

    private Texture2D drawing;
    

    // Start is called before the first frame update
    void Start()
    {
        genGest = new Gest();
        playGest = new Gest();
        generating = false;

        RectTransform rt = transform.Find("Gesture").GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(Screen.width, Screen.height);
        rt = transform.Find("Drawing").GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(Screen.width, Screen.height);

        transform.Find("Drawing").GetComponent<RawImage>().texture = new Texture2D(100, 100);
        //Debug.Log("FUCK");
        //Debug.Log(drawing.GetComponent<RawImage>().texture);
        drawing = transform.Find("Drawing").GetComponent<RawImage>().texture as Texture2D;
        clearDrawing();
    }

    void clearDrawing() {
        Color c = new Color();
        c.r = 0;
        c.g = 0;
        c.b = 0;
        c.a = 0;
        Color[] pixels = Enumerable.Repeat(c, drawing.width * drawing.height).ToArray();
        drawing.SetPixels(pixels);
        drawing.Apply();
    }

    void drawPixel(Vector2 percent_pos) {
        Color c = new Color();
        c.r = 0;
        c.g = 255;
        c.b = 0;
        c.a = 50;
        drawing.SetPixel((int)(percent_pos.x * drawing.width), (int)(percent_pos.y * drawing.height), c);
        drawing.Apply();
    }

    // Update is called once per frame
    void Update()
    {
        // if (Input.GetKeyDown("space")) {
        //     generating = !generating;
        //     Debug.Log(generating);
        // }

        if (playGest.isEmpty()) {
            transform.Find("Gesture").gameObject.SetActive(false);
        } else {
            transform.Find("Gesture").gameObject.SetActive(true);
        }

        if (Input.GetMouseButton(0)) {
            float percent_x = Input.mousePosition.x / Screen.width;
            float percent_y = Input.mousePosition.y / Screen.height;

            Vector2 p = new Vector2(percent_x, percent_y);
            if (generating) {
                genGest.addPos(p);
            } else {
                playGest.addPos(p);
                drawPixel(p);
            }

        } else {
            if (!genGest.isEmpty()) {
                genGest.printPositions();
                
                genGest.clear();
            }

            if (!playGest.isEmpty()) {
                if (playGest.getAccuracy(Const.GESTURE) <= ACC_THRESH) {
                    Debug.Log("SPAWN FIREBALL");
                };

                playGest.clear();
                clearDrawing();
            }
        }


        
		
    }
}
