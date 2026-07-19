using System;

namespace WFFramework.Core.Identity
{
    /// <summary>
    /// Interface cho object có ID runtime duy nhất.
    /// </summary>
    public interface IRuntimeEntity
    {
        /// <summary>
        /// ID duy nhất của object trong phiên chơi hoặc SaveData.
        /// </summary>
        string RuntimeId { get; }
    }

    /// <summary>
    /// Utility tạo ID runtime.
    /// </summary>
    public static class RuntimeId
    {
        /// <summary>
        /// Tạo một GUID không có dấu gạch ngang.
        ///
        /// Ví dụ:
        /// 278d4d687bcc473596d23572472c2503
        /// </summary>
        public static string Create()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}