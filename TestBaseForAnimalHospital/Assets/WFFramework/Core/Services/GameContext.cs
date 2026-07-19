using System;
using System.Collections.Generic;

namespace WFFramework.Core.Services
{
    /// <summary>
    /// Container lưu trữ các service của game.
    ///
    /// Vai trò:
    /// - Đăng ký service.
    /// - Cung cấp dependency cho các hệ thống khác.
    /// - Quản lý lifecycle của service.
    ///
    /// GameContext không tự tạo service.
    /// Installer chịu trách nhiệm tạo và đăng ký service.
    /// </summary>
    public sealed class GameContext
    {
         /// <summary>
        /// Dictionary lưu service theo interface hoặc class được đăng ký.
        ///
        /// Ví dụ:
        /// typeof(IEventBus) → EventBus instance.
        /// typeof(ITickService) → TickScheduler instance.
        /// </summary>
        private readonly Dictionary<Type, object> _services =
            new Dictionary<Type, object>();

        /// <summary>
        /// Danh sách service có lifecycle Initialize và Shutdown.
        ///
        /// Thứ tự trong danh sách là thứ tự đăng ký.
        /// Shutdown sẽ chạy theo thứ tự ngược lại.
        /// </summary>
        private readonly List<IGameSystem> _lifecycleServices =
            new List<IGameSystem>();

        /// <summary>
        /// Đăng ký một service vào GameContext.
        ///
        /// TSystem thường là interface:
        /// context.Bind&lt;IEventBus&gt;(eventBus);
        /// </summary>
        /// <typeparam name="TSystem">
        /// Kiểu interface hoặc class dùng để Resolve service.
        /// </typeparam>
        /// <param name="service">
        /// Instance thật của service.
        /// </param>
        public void Bind<TSystem>(TSystem service)
            where TSystem : class
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            Type serviceType = typeof(TSystem);

            if (_services.ContainsKey(serviceType))
            {
                throw new InvalidOperationException(
                    $"Service đã được đăng ký: {serviceType.FullName}");
            }

            _services.Add(serviceType, service);

            // Một service có thể được Bind bằng nhiều interface.
            // Vì vậy phải kiểm tra reference để không Initialize hai lần.
            if (service is IGameSystem gameService &&
                !ContainsLifecycleSystem(gameService))
            {
                _lifecycleServices.Add(gameService);
            }
        }

        /// <summary>
        /// Lấy service đã được đăng ký.
        ///
        /// Hàm sẽ ném exception nếu không tìm thấy.
        /// Dùng khi service là dependency bắt buộc.
        /// </summary>
        public TSystem Resolve<TSystem>()
            where TSystem : class
        {
            Type serviceType = typeof(TSystem);

            if (_services.TryGetValue(
                    serviceType,
                    out object service))
            {
                return (TSystem)service;
            }

            throw new InvalidOperationException(
                $"Không tìm thấy service: {serviceType.FullName}");
        }

        /// <summary>
        /// Thử lấy một service.
        ///
        /// Không ném exception nếu service không tồn tại.
        /// Dùng cho dependency không bắt buộc.
        /// </summary>
        public bool TryResolve<TSystem>(out TSystem service) where TSystem : class
        {
            if (_services.TryGetValue(
                    typeof(TSystem),
                    out object instance))
            {
                service = instance as TSystem;
                return service != null;
            }

            service = null;
            return false;
        }

        /// <summary>
        /// Initialize toàn bộ service theo thứ tự đăng ký.
        ///
        /// Chỉ GameBootstrap được phép gọi hàm này.
        /// </summary>
        internal void InitializeSystem()
        {
            for (int i = 0; i < _lifecycleServices.Count; i++)
            {
                _lifecycleServices[i].Initialize(this);
            }
        }

        /// <summary>
        /// Gọi StartGame trên các service implement IGameStartable.
        ///
        /// Hàm này chạy sau khi tất cả service đã Initialize.
        /// </summary>
        internal void StartSystem()
        {
            for (int i = 0; i < _lifecycleServices.Count; i++)
            {
                _lifecycleServices[i].StartGame();
            }
        }

        /// <summary>
        /// Shutdown toàn bộ service theo thứ tự ngược.
        ///
        /// Service được tạo cuối thường phụ thuộc service được tạo trước,
        /// vì vậy cần hủy nó trước.
        /// </summary>
        internal void ReleaseServices()
        {
            for (int i = _lifecycleServices.Count - 1; i >= 0; i--)
            {
                try
                {
                    _lifecycleServices[i].Release();
                }
                catch (Exception exception)
                {
                    UnityEngine.Debug.LogException(exception);
                }
            }

            _lifecycleServices.Clear();
            _services.Clear();
        }

        /// <summary>
        /// Kiểm tra service đã nằm trong danh sách lifecycle chưa.
        ///
        /// Dùng ReferenceEquals vì cùng một service có thể được Bind
        /// bằng nhiều interface khác nhau.
        /// </summary>
        private bool ContainsLifecycleSystem(
            IGameSystem system)
        {
            for (int i = 0; i < _lifecycleServices.Count; i++)
            {
                if (ReferenceEquals(
                        _lifecycleServices[i],
                        system))
                {
                    return true;
                }
            }

            return false;
        }
    }
}