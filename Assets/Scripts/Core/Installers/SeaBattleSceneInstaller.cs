using Zenject;

namespace SeaBattle.Core.Installers
{
    public class SeaBattleSceneInstaller : MonoInstaller
    {
        /// <summary>
        /// Installs the match when the scene context wakes up.
        /// </summary>
        public override void InstallBindings() => SeaBattleInstaller.Install(Container);
    }
}
