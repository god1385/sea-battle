using SeaBattle.Common.Config;
using SeaBattle.Common.Debug;
using SeaBattle.Core.Client;
using SeaBattle.Core.Game;
using SeaBattle.Core.Match;
using SeaBattle.Core.Network;
using SeaBattle.Core.UI;
using SeaBattle.Core.UI.Presenters;
using SeaBattle.Core.UI.Views;
using UnityEngine;
using Zenject;

namespace SeaBattle.Core.Installers
{
    public static class SeaBattleInstaller
    {
        /// <summary>
        /// Binds the match, both clients, and the reactive screen. The presenter is initialized after the runtime so it sees the joined state.
        /// </summary>
        public static void Install(DiContainer container)
        {
            container.Bind<GameConfig>().FromMethod(_ => Resources.Load<GameConfig>("GameConfig")).AsSingle();
            container.Bind<BoardArt>().AsSingle();
            container.Bind<NetworkLog>().FromMethod(CreateLog).AsSingle();
            container.Bind<IShipPlacer>().To<RandomShipPlacer>().AsSingle();
            container.Bind<IMatchServer>().To<MatchServer>().AsSingle();
            container.Bind<InProcessLink>().WithId(SeatId.First).FromMethod(ctx => CreateLink(ctx, "Игрок 1")).AsCached();
            container.Bind<InProcessLink>().WithId(SeatId.Second).FromMethod(ctx => CreateLink(ctx, "Игрок 2")).AsCached();
            container.Bind<ClientSession>().WithId(SeatId.First).FromMethod(ctx => CreateSession(ctx, PlayerId.First, SeatId.First)).AsCached();
            container.Bind<ClientSession>().WithId(SeatId.Second).FromMethod(ctx => CreateSession(ctx, PlayerId.Second, SeatId.Second)).AsCached();
            container.Bind<MatchGateway>().FromMethod(CreateGateway).AsSingle();
            container.Bind<MatchScreenRefs>().FromComponentInHierarchy().AsSingle();
            container.Bind<MatchUi>().AsSingle();
            container.BindInterfacesAndSelfTo<MatchRuntime>().AsSingle().NonLazy();
            container.BindInterfacesAndSelfTo<MatchPresenter>().AsSingle().NonLazy();
            container.BindExecutionOrder<MatchRuntime>(0);
            container.BindExecutionOrder<MatchPresenter>(10);
        }

        private static NetworkLog CreateLog(InjectContext context)
        {
            var config = context.Container.Resolve<GameConfig>();
            return new NetworkLog { Enabled = config.MessageLogEnabled };
        }

        private static InProcessLink CreateLink(InjectContext context, string label)
        {
            var config = context.Container.Resolve<GameConfig>();
            var log = context.Container.Resolve<NetworkLog>();
            return new InProcessLink(label, config.DeliveryDelayMilliseconds, log.Append);
        }

        private static ClientSession CreateSession(InjectContext context, PlayerId player, string seat)
        {
            var link = context.Container.ResolveId<InProcessLink>(seat);
            return new ClientSession(player, link.Client);
        }

        private static MatchGateway CreateGateway(InjectContext context)
        {
            var server = context.Container.Resolve<IMatchServer>();
            var first = context.Container.ResolveId<InProcessLink>(SeatId.First);
            var second = context.Container.ResolveId<InProcessLink>(SeatId.Second);
            return new MatchGateway(server, first.Server, second.Server);
        }
    }
}
