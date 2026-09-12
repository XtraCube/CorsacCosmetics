using System;
using UnityEngine.SceneManagement;

namespace CorsacCosmetics;

public static class ModStamp
{
    public static void Initialize()
    {
        SceneManager.add_sceneLoaded((Action<Scene, LoadSceneMode>)((scene, _) =>
        {
            if (scene.name == "MainMenu")
            {
                ModManager.Instance.ShowModStamp();
            }
        }));
    }
}