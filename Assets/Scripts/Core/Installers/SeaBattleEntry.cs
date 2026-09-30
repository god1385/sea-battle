using UnityEngine;
using Zenject;

namespace SeaBattle.Core.Installers
{
    public static class SeaBattleEntry
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Launch()
        {
            if (Object.FindAnyObjectByType<SceneContext>() != null)
                return;

            SceneContext.ExtraBindingsInstallMethod = SeaBattleInstaller.Install;
            new GameObject("SceneContext").AddComponent<SceneContext>();
        }
    }
}
