using System.Collections.Generic;
using UnityEngine;
using WFFramework.Core.Services;


namespace WFFramework.Core.Bootstrap
{
    /// <summary>
    /// Composition Root chính của toàn bộ game.
    ///
    /// Trách nhiệm:
    /// - Tạo GameContext.
    /// - Chạy các Installer.
    /// - Initialize service.
    /// - Khởi động game.
    /// - Shutdown service khi bị hủy.
    ///
    /// Trong game chỉ nên tồn tại một GameBootstrap.
    /// </summary>
    
    [DefaultExecutionOrder(-10000)]
    public class GameBootstrap : MonoBehaviour
    {
        /// <summary>
        /// Context hiện tại của game.
        ///
        /// Static chỉ được sử dụng như entry point ban đầu.
        /// Không nên Resolve service liên tục trong Update.
        /// </summary>
        public static GameContext Context { get; private set; }

        /// <summary>
        /// Có giữ Bootstrap khi chuyển Scene hay không.
        ///
        /// Với game nhiều Scene nên bật true.
        /// </summary>
        [SerializeField]
        private bool dontDestroyOnLoad = true;

        /// <summary>
        /// Danh sách Installer được chạy theo thứ tự từ trên xuống.
        ///
        /// CoreInstaller nên đặt đầu tiên.
        /// UIInstaller thường đặt cuối.
        /// </summary>
        [SerializeField]
        private List<GameInstaller> installers =
            new List<GameInstaller>();

        /// <summary>
        /// Xác định GameObject này có thực sự sở hữu Context không.
        ///
        /// Bootstrap duplicate sẽ bị Destroy nhưng không được phép
        /// Shutdown Context của Bootstrap gốc.
        /// </summary>
        private bool _ownsContext;

        /// <summary>
        /// Được Unity gọi trước Start.
        ///
        /// Đây là nơi khởi tạo toàn bộ framework.
        /// </summary>
        private void Awake()
        {
            // Nếu đã tồn tại Bootstrap khác thì hủy bản duplicate.
            if (Context != null)
            {
                Destroy(gameObject);
                return;
            }

            _ownsContext = true;
            Context = new GameContext();

            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }

            InstallModules();

            Context.InitializeSystem();
            Context.StartSystem();
        }

        /// <summary>
        /// Chạy từng Installer theo đúng thứ tự trong Inspector.
        /// </summary>
        private void InstallModules()
        {
            for (int i = 0; i < installers.Count; i++)
            {
                GameInstaller installer = installers[i];

                if (installer == null)
                {
                    Debug.LogError(
                        $"Installer tại index {i} đang null.",
                        this);

                    continue;
                }

                installer.Install(Context);
            }
        }

        /// <summary>
        /// Được gọi khi Bootstrap bị Destroy.
        ///
        /// Chỉ Bootstrap sở hữu Context mới được Shutdown service.
        /// </summary>
        private void OnDestroy()
        {
            if (!_ownsContext || Context == null)
            {
                return;
            }

            Context.ReleaseServices();
            Context = null;
        }
    }
}