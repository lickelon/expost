using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Expost.RuleReconstruction
{
    public static class RuleReconstructionIconFactory
    {
        private static readonly Dictionary<ButtonIconKind, Sprite> Sprites = new();

        public static void ApplyToButton(Button button, ButtonIconKind kind, float size)
        {
            var text = button.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.text = string.Empty;
            }

            var icon = button.transform.Find("Icon")?.GetComponent<Image>();
            RectTransform iconRect;
            if (icon == null)
            {
                var iconObject = new GameObject("Icon");
                iconObject.transform.SetParent(button.transform, false);
                iconRect = iconObject.AddComponent<RectTransform>();
                icon = iconObject.AddComponent<Image>();
                icon.raycastTarget = false;
            }
            else
            {
                iconRect = icon.GetComponent<RectTransform>();
            }

            icon.sprite = Get(kind);
            icon.color = Color.white;
            icon.preserveAspect = true;
            iconRect.SetAsLastSibling();
            RuleReconstructionUiFactory.Anchor(
                iconRect,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(-size * 0.5f, -size * 0.5f),
                new Vector2(size * 0.5f, size * 0.5f));
        }

        public static Sprite Get(ButtonIconKind kind)
        {
            if (!Sprites.TryGetValue(kind, out var sprite) || sprite == null)
            {
                sprite = Create(kind);
                Sprites[kind] = sprite;
            }

            return sprite;
        }

        private static Sprite Create(ButtonIconKind kind)
        {
            var texture = new Texture2D(32, 32, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                hideFlags = HideFlags.HideAndDontSave
            };

            var pixels = texture.GetPixels();
            for (var index = 0; index < pixels.Length; index++)
            {
                pixels[index] = new Color(1f, 1f, 1f, 0f);
            }

            texture.SetPixels(pixels);

            switch (kind)
            {
                case ButtonIconKind.Previous:
                    FillTriangle(texture, new Vector2Int(23, 7), new Vector2Int(8, 16), new Vector2Int(23, 25));
                    break;
                case ButtonIconKind.Next:
                case ButtonIconKind.Run:
                    FillTriangle(texture, new Vector2Int(9, 7), new Vector2Int(24, 16), new Vector2Int(9, 25));
                    break;
                case ButtonIconKind.Target:
                    FillTarget(texture);
                    break;
                case ButtonIconKind.Check:
                    FillCheck(texture);
                    break;
                case ButtonIconKind.Cross:
                    FillCross(texture);
                    break;
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, 32f, 32f), new Vector2(0.5f, 0.5f), 32f);
        }

        private static void FillTarget(Texture2D texture)
        {
            for (var y = 7; y <= 24; y += 8)
            {
                for (var x = 7; x <= 24; x += 8)
                {
                    FillRect(texture, x, y, 5, 5);
                }
            }
        }

        private static void FillCheck(Texture2D texture)
        {
            FillRect(texture, 7, 14, 5, 5);
            FillRect(texture, 11, 10, 5, 5);
            FillRect(texture, 15, 6, 5, 5);
            FillRect(texture, 19, 18, 5, 5);
            FillRect(texture, 23, 22, 5, 5);
        }

        private static void FillCross(Texture2D texture)
        {
            for (var offset = 0; offset < 16; offset += 4)
            {
                FillRect(texture, 8 + offset, 8 + offset, 5, 5);
                FillRect(texture, 20 - offset, 8 + offset, 5, 5);
            }
        }

        private static void FillTriangle(Texture2D texture, Vector2Int a, Vector2Int b, Vector2Int c)
        {
            for (var y = 0; y < texture.height; y++)
            {
                for (var x = 0; x < texture.width; x++)
                {
                    var point = new Vector2(x + 0.5f, y + 0.5f);
                    if (IsInsideTriangle(point, a, b, c))
                    {
                        texture.SetPixel(x, y, Color.white);
                    }
                }
            }
        }

        private static bool IsInsideTriangle(Vector2 point, Vector2 a, Vector2 b, Vector2 c)
        {
            var d1 = Sign(point, a, b);
            var d2 = Sign(point, b, c);
            var d3 = Sign(point, c, a);
            return !(d1 < 0f || d2 < 0f || d3 < 0f) || !(d1 > 0f || d2 > 0f || d3 > 0f);
        }

        private static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
        }

        private static void FillRect(Texture2D texture, int x, int y, int width, int height)
        {
            for (var yy = y; yy < y + height; yy++)
            {
                for (var xx = x; xx < x + width; xx++)
                {
                    texture.SetPixel(xx, yy, Color.white);
                }
            }
        }
    }
}
