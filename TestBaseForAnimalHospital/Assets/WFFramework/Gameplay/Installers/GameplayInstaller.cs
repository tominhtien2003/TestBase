using UnityEngine;
using WFFramework.Core.Services;
using WFFramework.Gameplay.Inventory;
using WFFramework.Gameplay.Reservation;

namespace WFFramework.Gameplay.Installers
{
    /// <summary>
    /// Installer đăng ký các Gameplay Service dùng chung.
    ///
    /// Installer này chạy sau CoreInstaller.
    /// </summary>
    public sealed class GameplayInstaller :
        GameInstaller
    {
        /// <summary>
        /// Catalog chứa toàn bộ Item của game hiện tại.
        /// </summary>
        [SerializeField]
        private ItemCatalog itemCatalog;

        public override void Install(
            GameContext context)
        {
            if (itemCatalog == null)
            {
                throw new MissingReferenceException(
                    "GameplayInstaller chưa được gán ItemCatalog.");
            }

            ReservationSystem reservationSystem =
                new ReservationSystem();

            context.Bind<IReservationService>(
                reservationSystem);

            context.Bind<ReservationSystem>(
                reservationSystem);

            context.Bind<IItemCatalog>(
                itemCatalog);

            context.Bind<ItemCatalog>(
                itemCatalog);
        }
    }
}