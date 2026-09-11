using System.Collections;
using System.Diagnostics;
using AmongUs.Data;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using CorsacCosmetics.Components;
using CorsacCosmetics.Cosmetics;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;

namespace CorsacCosmetics.Patches;

public static class VisorsTabPatches
{
    private static InventoryTabPaginationBehaviour _pagination = null!;

    private static string GetText()
    {
        string name;
        if (!_pagination || _pagination.CurrentTab == 0)
        {
            name = TranslationController.Instance.GetString(StringNames.Visors);
        }
        else
        {
            name = CosmeticsCatalog.Instance.VisorGroups.GetGroupNameByIndex(_pagination.CurrentTab - 1);
        }

        var max = CosmeticsCatalog.Instance.VisorGroups.Count + 1;
        return $"{name} ({_pagination.CurrentTab + 1} / {max})";
    }

    private static bool ShowOnPage(string id)
    {
        if (!_pagination) return true;
        
        var data = CosmeticsCatalog.Instance.Get(id);

        if (_pagination.CurrentTab == 0) return data == null;
        if (data == null) return false;
        
        var currentGroup = CosmeticsCatalog.Instance.VisorGroups.GetGroupIdByIndex(_pagination.CurrentTab - 1);
        return currentGroup == data.GroupId;
    }

    [HarmonyPatch(typeof(VisorsTab), nameof(VisorsTab.OnEnable))]
    public static class VisorsTabOnEnablePatch
    {
        public static bool Prefix(VisorsTab __instance)
        {
            // --------- Pagination ----------------
            _pagination = __instance.GetComponent<InventoryTabPaginationBehaviour>();
            if (!_pagination)
            {
                _pagination = __instance.gameObject.AddComponent<InventoryTabPaginationBehaviour>();
            }

            _pagination.Setup(
                __instance,
                CosmeticsCatalog.Instance.VisorGroups.Count,
                GetText);

            // ---------- Original Game Code -----------
            InventoryTabReversePatch.OnEnable(__instance);

            // --------- Coroutine ----------
            __instance.StartCoroutine(CoOnEnable(__instance).WrapToIl2Cpp());
            return false;
        }

        private static IEnumerator CoOnEnable(VisorsTab tab)
        {
            VisorData[] unlockedVisors = DestroyableSingleton<HatManager>.Instance.GetUnlockedVisors();
            var num = 0;
            // half the frame time in milliseconds
            var targetFrameTime = 1000f / Application.targetFrameRate * 0.75f;
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            foreach (var visor in unlockedVisors)
            {
                if (!ShowOnPage(visor.ProductId)) continue;

                if (stopwatch.ElapsedMilliseconds >= targetFrameTime)
                {
                    stopwatch.Restart();
                    yield return null;
                }

                var num2 = tab.XRange.Lerp(num % tab.NumPerRow / (tab.NumPerRow - 1f));
                var num3 = tab.YStart - num / tab.NumPerRow * tab.YOffset;
                var colorChip = Object.Instantiate(tab.ColorTabPrefab, tab.scroller.Inner);
                colorChip.transform.localPosition = new Vector3(num2, num3, -1f);
                if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Keyboard)
                {
                    var visor1 = visor;
                    colorChip.Button.OnMouseOver.AddListener((UnityAction)(() => { tab.SelectVisor(visor1); }));
                    colorChip.Button.OnMouseOut.AddListener((UnityAction)(() =>
                    {
                        tab.SelectVisor(
                            DestroyableSingleton<HatManager>.Instance.GetVisorById(DataManager.Player.Customization
                                .Visor));
                    }));
                    colorChip.Button.OnClick.AddListener((UnityAction)(() => { tab.ClickEquip(); }));
                }
                else
                {
                    colorChip.Button.OnClick.AddListener((UnityAction)(() => { tab.SelectVisor(visor); }));
                }

                colorChip.Button.ClickMask = tab.scroller.Hitbox;
                colorChip.ProductId = visor.ProductId;
                tab.UpdateMaterials(colorChip.Inner.FrontLayer, visor);
                visor.SetPreview(colorChip.Inner.FrontLayer, tab.GetDisplayColor());
                colorChip.Tag = visor.ProdId;
                colorChip.SelectionHighlight.gameObject.SetActive(false);
                tab.ColorChips.Add(colorChip);
                num++;
                if (!DestroyableSingleton<HatManager>.Instance.CheckLongModeValidCosmetic(visor.ProdId,
                        tab.PlayerPreview.GetIgnoreLongMode()))
                {
                    colorChip.SetUnavailable();
                }
            }

            if (unlockedVisors.Length != 0)
            {
                tab.GetDefaultSelectable().PlayerEquippedForeground.SetActive(true);
            }

            tab.visorId = DataManager.Player.Customization.Visor;
            tab.currentVisorIsEquipped = true;
            tab.SetScrollerBounds();
        }
    }
}