using Zenject;

namespace SeaBattle.Core.Installers
{
    public class SeaBattleSceneInstaller : MonoInstaller
    {
        public override void InstallBindings() => SeaBattleInstaller.Install(Container);
    }
}
