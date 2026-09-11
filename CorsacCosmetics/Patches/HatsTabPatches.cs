using System.Collections;
using System.Diagnostics;
using System.Linq;
using AmongUs.Data;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using CorsacCosmetics.Components;
using CorsacCosmetics.Cosmetics;
using CorsacCosmetics.Tools;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;

namespace CorsacCosmetics.Patches;

public static class HatsTabPatches
{
    private static InventoryTabPaginationBehaviour _pagination = null!;

    private static string GetText()
    {
        string name;
        if (!_pagination || _pagination.CurrentTab == 0)
        {
            name = TranslationController.Instance.GetString(StringNames.HatLabel);
        }
        else
        {
            name = CosmeticsCatalog.Instance.HatGroups.GetGroupNameByIndex(_pagination.CurrentTab - 1);
        }

        var max = CosmeticsCatalog.Instance.HatGroups.Count + 1;
        return $"{name} ({_pagination.CurrentTab + 1} / {max})";
    }

    private static bool ShowOnPage(string id)
    {
        if (!_pagination) return true;

        var data = CosmeticsCatalog.Instance.Get(id);

        if (_pagination.CurrentTab == 0) return data == null;
        if (data == null) return false;

        var currentGroup = CosmeticsCatalog.Instance.HatGroups.GetGroupIdByIndex(_pagination.CurrentTab - 1);
        return currentGroup == data.GroupId;
    }

    [HarmonyPatch(typeof(HatsTab), nameof(HatsTab.OnEnable))]
    public static class HatsTabOnEnablePatch
    {
        public static bool Prefix(HatsTab __instance)
        {
            // --------- Pagination ----------------
            _pagination = __instance.GetComponent<InventoryTabPaginationBehaviour>();
            if (!_pagination)
            {
                _pagination = __instance.gameObject.AddComponent<InventoryTabPaginationBehaviour>();
            }

            _pagination.Setup(
                __instance,
                CosmeticsCatalog.Instance.HatGroups.Count,
                GetText);

            // ---------- Original Game Code -----------
            InventoryTabReversePatch.OnEnable(__instance);

            // ---------- Coroutine -----------
            __instance.StartCoroutine(CoOnEnable(__instance).WrapToIl2Cpp());
            return false;
        }

        private static IEnumerator CoOnEnable(HatsTab hatsTab)
        {
            var hats = HatManager.Instance.GetUnlockedHats().Where(h => ShowOnPage(h.ProductId)).ToArray();

            hatsTab.currentHat = HatManager.Instance.GetHatById(DataManager.Player.Customization.Hat);
            hatsTab.currentHatIsEquipped = true;
            hatsTab.scroller.CalculateAndSetYBounds(hats.Length, hatsTab.NumPerRow, hatsTab.NumRowsVisible,
                hatsTab.YOffset);

            var num = 0;
            var targetFrameTime = Utilities.GetTargetFrameTimeMilliseconds();
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            foreach (var hat in hats)
            {
                if (!ShowOnPage(hat.ProductId)) continue;

                if (stopwatch.ElapsedMilliseconds >= targetFrameTime)
                {
                    stopwatch.Restart();
                    yield return null;
                }

                var num2 = hatsTab.XRange.Lerp(num % hatsTab.NumPerRow / (hatsTab.NumPerRow - 1f));
                var num3 = hatsTab.YStart - num / hatsTab.NumPerRow * hatsTab.YOffset;
                var colorChip = Object.Instantiate(hatsTab.ColorTabPrefab, hatsTab.scroller.Inner);
                colorChip.transform.localPosition = new Vector3(num2, num3, -1f);
                if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Keyboard)
                {
                    var hat1 = hat;
                    colorChip.Button.OnMouseOver.AddListener((UnityAction)(() => { hatsTab.SelectHat(hat1); }));
                    colorChip.Button.OnMouseOut.AddListener((UnityAction)(() =>
                    {
                        hatsTab.SelectHat(HatManager.Instance.GetHatById(DataManager.Player.Customization.Hat));
                    }));
                    colorChip.Button.OnClick.AddListener((UnityAction)(() => { hatsTab.ClickEquip(); }));
                }
                else
                {
                    colorChip.Button.OnClick.AddListener((UnityAction)(() => { hatsTab.SelectHat(hat); }));
                }

                colorChip.Button.ClickMask = hatsTab.scroller.Hitbox;
                colorChip.Inner.SetMaskType(PlayerMaterial.MaskType.SimpleUI);
                hatsTab.UpdateMaterials(colorChip.Inner.FrontLayer, hat);
                hat.SetPreview(colorChip.Inner.FrontLayer, hatsTab.GetDisplayColor());
                colorChip.Tag = hat;
                colorChip.SelectionHighlight.gameObject.SetActive(false);
                hatsTab.ColorChips.Add(colorChip);
                num++;
                if (!HatManager.Instance.CheckLongModeValidCosmetic(hat.ProdId,
                        hatsTab.PlayerPreview.GetIgnoreLongMode()))
                {
                    colorChip.SetUnavailable();
                }
            }
        }
    }
}