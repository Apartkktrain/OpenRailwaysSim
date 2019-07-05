﻿using UnityEngine;
using UnityEngine.UI;

public class TextEntity : MonoBehaviour
{

    public string str;
    public MapObject obj;
    public Color normalColor = new Color(1f, 1f, 1f, 0.75f);

    private Text text;
    private Button button;

    void Update()
    {
        if (!GameCanvas.runPanel.isShowing() && Main.main.showGuide)
        {
            if ((Quaternion.Inverse(Camera.main.transform.rotation) * (obj.pos - Camera.main.transform.position)).z > 0)
            {
                if (text == null)
                {
                    (text = gameObject.AddComponent<Text>()).font = Main.main.mapPinFont;
                    text.rectTransform.sizeDelta = new Vector2(160f, 20f);
                    text.alignment = TextAnchor.MiddleCenter;
                    text.resizeTextForBestFit = true;
                    text.resizeTextMinSize = 1;
                    text.resizeTextMaxSize = text.fontSize;
                    transform.SetAsFirstSibling();

                    (button = gameObject.AddComponent<Button>()).onClick.AddListener(TaskOnClick);
                    var a = button.colors;
                    a.highlightedColor = new Color(1f, 1f, 0, 0.75f);
                    button.colors = a;

                    transform.SetParent(GameCanvas.canvas.transform);
                }
                text.text = str;
                if (obj.useSelectingMat)
                    text.color = new Color(1f, 1f, 0, 0.75f);
                else
                    text.color = normalColor;
                text.raycastTarget = Main.main.mode == 0;

                var p = Camera.main.WorldToViewportPoint(obj.pos);
                transform.position = new Vector3(Screen.width * p.x, Screen.height * p.y);
            }
            else if (text != null)
            {
                GameObject.Destroy(text);
                text = null;
                GameObject.Destroy(button);
                button = null;
            }
        }
        else if (text != null)
        {
            GameObject.Destroy(text);
            text = null;
            GameObject.Destroy(button);
            button = null;
        }
    }

    void TaskOnClick()
    {
        Main.selectObj(obj);
    }
}