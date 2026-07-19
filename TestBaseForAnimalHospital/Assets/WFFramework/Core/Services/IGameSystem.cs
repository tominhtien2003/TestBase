namespace WFFramework.Core.Services
{
    /// <summary>
    /// Interface định nghĩa vòng đời cơ bản của một Game Service.
    ///
    /// Game Service là các hệ thống tồn tại lâu dài như:
    /// - EventBus.
    /// - SaveSystem.
    /// - TickScheduler.
    /// - AudioSystem.
    /// - SceneSystem.
    ///
    /// Service không nhất thiết phải là MonoBehaviour.
    /// </summary>
    public interface IGameSystem
    {
        /// <summary>
        /// Được gọi sau khi tất cả Installer đã đăng ký service.
        ///
        /// Tại đây service có thể lấy dependency từ GameContext.
        /// </summary>
        void Initialize(GameContext context);
        
        /// <summary>
        /// Được gọi sau khi tất cả service đã Initialize thành công.
        /// </summary>


        void StartGame();
        /// <summary>
        /// Được gọi khi game đóng hoặc Bootstrap bị hủy.
        ///
        /// Dùng để:
        /// - Unsubscribe event.
        /// - Hủy dữ liệu runtime.
        /// - Release tài nguyên.
        /// </summary>
        void Release();
    }
}