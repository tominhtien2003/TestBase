using UnityEngine;
using WFFramework.Core.Events;
using WFFramework.Core.Services;
using WFFramework.Core.Tick;

namespace WFFramework.Core.Bootstrap
{
    /// <summary>
    /// Installer đăng ký các service nền tảng của framework.
    ///
    /// Installer này nên được chạy đầu tiên.
    /// </summary>
    public sealed class CoreInstaller : GameInstaller
    {
        /// <summary>
        /// TickScheduler nằm trên Bootstrap GameObject hoặc child.
        ///
        /// Đây là MonoBehaviour nên cần kéo reference từ Inspector.
        /// </summary>
        [SerializeField]
        private TickScheduler tickScheduler;

        /// <summary>
        /// Tạo và đăng ký toàn bộ Core Service.
        /// </summary>
        public override void Install(GameContext context)
        {
            if (tickScheduler == null)
            {
                throw new MissingReferenceException(
                    "CoreInstaller chưa được gán TickScheduler.");
            }

            // EventBus là pure C# class nên có thể tạo bằng new.
            EventBus eventBus = new EventBus();

            // Bind theo interface để module khác không phụ thuộc implementation.
            context.Bind<IEventBus>(eventBus);
            context.Bind<ITickSystem>(tickScheduler);

            // Bind concrete class khi cần truy cập tính năng riêng
            // trong debug hoặc editor.
            context.Bind<EventBus>(eventBus);
            context.Bind<TickScheduler>(tickScheduler);
        }
    }
}