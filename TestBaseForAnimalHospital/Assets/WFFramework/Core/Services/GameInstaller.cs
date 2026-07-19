using UnityEngine;

namespace WFFramework.Core.Services
{
    /// <summary>
    /// Base class dùng để đăng ký service vào GameContext.
    ///
    /// Mỗi module nên có Installer riêng:
    /// - CoreInstaller.
    /// - GameplayInstaller.
    /// - AnimalHospitalInstaller.
    /// - UIInstaller.
    ///
    /// Installer chỉ chịu trách nhiệm tạo và Bind dependency.
    /// Installer không chạy gameplay.
    /// </summary>
    public abstract class GameInstaller : MonoBehaviour
    {
        /// <summary>
        /// Được GameBootstrap gọi trong Awake.
        ///
        /// Các class kế thừa sẽ tạo service và đăng ký vào context.
        /// </summary>
        public abstract void Install(GameContext context);
    }
}