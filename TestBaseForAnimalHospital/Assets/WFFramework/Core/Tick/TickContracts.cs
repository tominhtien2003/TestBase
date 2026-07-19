using System;

namespace WFFramework.Core.Tick
{
    /// <summary>
    /// Các nhóm Tick có tần suất khác nhau.
    /// </summary>
    public enum TickGroup
    {
        /// <summary>
        /// Dùng cho logic cần phản hồi nhanh.
        /// Ví dụ: task execution hoặc combat logic.
        /// </summary>
        Fast,

        /// <summary>
        /// Dùng cho AI decision và state machine.
        /// </summary>
        Normal,

        /// <summary>
        /// Dùng cho hunger, health decay, economy hoặc timer chậm.
        /// </summary>
        Slow
    }

    /// <summary>
    /// Object muốn nhận Tick phải implement interface này.
    /// </summary>
    public interface ITickable
    {
        /// <summary>
        /// Được TickScheduler gọi theo interval đã đăng ký.
        /// </summary>
        void Tick(float deltaTime);
    }

    /// <summary>
    /// Interface public của Tick Scheduler.
    /// </summary>
    public interface ITickSystem
    {
        /// <summary>
        /// Đăng ký một object vào nhóm Tick.
        ///
        /// Trả về IDisposable để unregister.
        /// </summary>
        IDisposable Register(
            TickGroup group,
            ITickable tickable);
    }
}