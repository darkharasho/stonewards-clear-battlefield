using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UIElements;

namespace ClearBattlefield.Patches
{
    /// <summary>
    /// Adds a "Clear Battlefield" button to the top-right corner of the pause screen, styled like the game's own
    /// buttons, and asks for confirmation with the pause menu's confirmation popup.
    /// </summary>
    internal static class PauseMenuButton
    {
        private static readonly AccessTools.FieldRef<UIPauseMenu, Button> PlayButton =
            AccessTools.FieldRefAccess<UIPauseMenu, Button>("playButton");
        private static readonly AccessTools.FieldRef<UIPauseMenu, Button> SettingsButton =
            AccessTools.FieldRefAccess<UIPauseMenu, Button>("settingsButton");
        private static readonly AccessTools.FieldRef<UIPauseMenu, UIConfirmationPopup> ConfirmationPopup =
            AccessTools.FieldRefAccess<UIPauseMenu, UIConfirmationPopup>("confirmationPopup");
        private static readonly AccessTools.FieldRef<BaseMenu, List<VisualElement>> NavigableElements =
            AccessTools.FieldRefAccess<BaseMenu, List<VisualElement>>("navigableElements");
        private static readonly Action<BaseMenu, bool> SetNavigable =
            AccessTools.MethodDelegate<Action<BaseMenu, bool>>(AccessTools.Method(typeof(BaseMenu), "SetNavigable"));

        private static UIPauseMenu _menu;
        private static Button _button;

        public static void Attach(UIPauseMenu menu)
        {
            var settingsButton = SettingsButton(menu);
            var root = menu.GetComponent<UIDocument>()?.rootVisualElement;
            // UIConfirmationPopup is a BaseMenu the game news up instead of adding as a component, so Unity's == null
            // always reports it as destroyed. Compare references instead.
            if (settingsButton == null || root == null || ReferenceEquals(ConfirmationPopup(menu), null))
            {
                Plugin.Log.LogWarning("Pause menu layout not recognised; the Clear Battlefield button was not added");
                return;
            }

            _button?.RemoveFromHierarchy();
            _menu = menu;
            _button = GameUi.CloneButton(settingsButton, "Clear Battlefield");
            _button.style.position = Position.Absolute;
            _button.style.top = 32;
            _button.style.right = 32;
            _button.style.width = StyleKeyword.Auto;
            _button.style.display = DisplayStyle.None;
            _button.clicked += OnClick;
            root.Add(_button);
            NavigableElements(menu).Add(_button);
        }

        /// <summary>
        /// Called every frame. Shown only while the pause screen itself is interactive: hidden when the menu is closed,
        /// when a confirmation popup, settings or another mod's panel is on top, and for clients.
        /// </summary>
        public static void Update()
        {
            if (_button == null)
                return;
            var play = _menu != null ? PlayButton(_menu) : null;
            var visible = _menu != null && _menu.IsPauseMenuOpen && play != null && play.focusable
                && BattlefieldClearer.Available;
            var display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            if (_button.style.display != display)
                _button.style.display = display;
        }

        /// <summary>The keybind: clear straight away, or open the pause menu on the confirmation.</summary>
        public static void OnHotkey()
        {
            if (!BattlefieldClearer.Available)
                return;
            if (!Plugin.RequireConfirmation.Value)
            {
                BattlefieldClearer.Clear();
                return;
            }

            var menu = UIPauseMenu.Instance;
            if (menu == null || menu != _menu || InputManager.Instance == null)
                return;
            var state = InputManager.Instance.CurrentInputState;
            if (state == InputManager.InputState.Gameplay)
                menu.OpenMenu(_Show: true);
            else if (state != InputManager.InputState.PauseMenu || !PlayButton(menu).focusable)
                return;
            ShowConfirmation();
        }

        private static void OnClick()
        {
            if (!BattlefieldClearer.Available)
                return;
            AudioManager.Instance?.PlayUIButton();
            if (Plugin.RequireConfirmation.Value)
                ShowConfirmation();
            else
                BattlefieldClearer.Clear();
        }

        private static void ShowConfirmation()
        {
            var popup = ConfirmationPopup(_menu);
            var (drops, scrap) = BattlefieldClearer.CountClearable();
            SetNavigable(_menu, false);
            if (drops + scrap == 0)
            {
                // One-button form; its OK button just hides the popup.
                popup.Show(ClearText.Confirmation(0, 0), "OK", _button);
                popup.Root?.schedule.Execute(RestoreWhenClosed).Every(100).Until(() => !popup.IsOpen);
                return;
            }
            popup.Show(ClearText.Confirmation(drops, scrap), "Clear", "Cancel", _button, OnConfirm, OnCancel);
        }

        private static void RestoreWhenClosed()
        {
            var popup = _menu != null ? ConfirmationPopup(_menu) : null;
            if (!ReferenceEquals(popup, null) && !popup.IsOpen && _menu.IsPauseMenuOpen)
                SetNavigable(_menu, true);
        }

        private static void OnConfirm()
        {
            // Unlike the game's own confirmations, this one leaves the pause menu open.
            ConfirmationPopup(_menu).Hide();
            SetNavigable(_menu, true);
            BattlefieldClearer.Clear();
        }

        private static void OnCancel()
        {
            AudioManager.Instance?.PlayUIButton();
            SetNavigable(_menu, true);
        }
    }

    [HarmonyPatch(typeof(UIPauseMenu), nameof(UIPauseMenu.Init))]
    internal static class PauseMenuInitPatch
    {
        private static void Postfix(UIPauseMenu __instance)
        {
            try
            {
                PauseMenuButton.Attach(__instance);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Could not add the Clear Battlefield button to the pause menu: {ex}");
            }
        }
    }
}
