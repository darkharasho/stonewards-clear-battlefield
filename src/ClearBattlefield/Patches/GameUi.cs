using UnityEngine.UIElements;

namespace ClearBattlefield.Patches
{
    /// <summary>Helpers for adding elements to the game's UI documents. Same approach as darkharasho-ModSettings.</summary>
    internal static class GameUi
    {
        /// <summary>Copies the look of a game button: its classes, inline styles and a text label child if it uses one.</summary>
        public static Button CloneButton(Button source, string text)
        {
            var button = new Button { name = "ClearBattlefieldButton" };
            foreach (var cls in source.GetClasses())
                button.AddToClassList(cls);
            CopyInlineStyle(source.style, button.style);

            var sourceLabel = source.Q<Label>();
            if (string.IsNullOrEmpty(source.text) && sourceLabel != null)
            {
                var label = new Label(text) { pickingMode = PickingMode.Ignore };
                foreach (var cls in sourceLabel.GetClasses())
                    label.AddToClassList(cls);
                CopyInlineStyle(sourceLabel.style, label.style);
                button.Add(label);
            }
            else
            {
                button.text = text;
            }
            return button;
        }

        /// <summary>Visual properties set inline in the game's UXML. Unset properties copy as unset, so class styles still apply.</summary>
        private static void CopyInlineStyle(IStyle from, IStyle to)
        {
            to.backgroundColor = from.backgroundColor;
            to.backgroundImage = from.backgroundImage;
            to.unityBackgroundImageTintColor = from.unityBackgroundImageTintColor;
            to.unitySliceLeft = from.unitySliceLeft;
            to.unitySliceRight = from.unitySliceRight;
            to.unitySliceTop = from.unitySliceTop;
            to.unitySliceBottom = from.unitySliceBottom;
            to.unitySliceScale = from.unitySliceScale;
            to.borderTopWidth = from.borderTopWidth;
            to.borderBottomWidth = from.borderBottomWidth;
            to.borderLeftWidth = from.borderLeftWidth;
            to.borderRightWidth = from.borderRightWidth;
            to.borderTopColor = from.borderTopColor;
            to.borderBottomColor = from.borderBottomColor;
            to.borderLeftColor = from.borderLeftColor;
            to.borderRightColor = from.borderRightColor;
            to.borderTopLeftRadius = from.borderTopLeftRadius;
            to.borderTopRightRadius = from.borderTopRightRadius;
            to.borderBottomLeftRadius = from.borderBottomLeftRadius;
            to.borderBottomRightRadius = from.borderBottomRightRadius;
            to.color = from.color;
            to.fontSize = from.fontSize;
            to.unityFont = from.unityFont;
            to.unityFontDefinition = from.unityFontDefinition;
            to.unityFontStyleAndWeight = from.unityFontStyleAndWeight;
            to.unityTextAlign = from.unityTextAlign;
            to.unityTextOutlineColor = from.unityTextOutlineColor;
            to.unityTextOutlineWidth = from.unityTextOutlineWidth;
            to.textShadow = from.textShadow;
            to.letterSpacing = from.letterSpacing;
            to.paddingTop = from.paddingTop;
            to.paddingBottom = from.paddingBottom;
            to.paddingLeft = from.paddingLeft;
            to.paddingRight = from.paddingRight;
        }
    }
}
