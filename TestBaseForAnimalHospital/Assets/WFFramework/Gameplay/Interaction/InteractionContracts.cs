using UnityEngine;

namespace WFFramework.Gameplay.Interaction
{
    /// <summary>
    /// Context chứa thông tin của một lần Interaction.
    /// </summary>
    public readonly struct InteractionContext
    {
        /// <summary>
        /// GameObject thực hiện Interaction.
        ///
        /// Thường là Player hoặc Bot.
        /// </summary>
        public readonly GameObject Interactor;

        /// <summary>
        /// Dữ liệu bổ sung tùy chọn.
        ///
        /// Ví dụ:
        /// - Item đang cầm.
        /// - Registration Document.
        /// - Interaction Command.
        /// </summary>
        public readonly object Payload;

        public InteractionContext(
            GameObject interactor,
            object payload = null)
        {
            Interactor = interactor;
            Payload = payload;
        }
    }

    /// <summary>
    /// Kết quả của một lần Interaction.
    /// </summary>
    public readonly struct InteractionResult
    {
        /// <summary>
        /// True nếu Interaction thành công.
        /// </summary>
        public readonly bool Success;

        /// <summary>
        /// Thông báo tùy chọn cho UI hoặc Debug.
        /// </summary>
        public readonly string Message;

        public InteractionResult(
            bool success,
            string message)
        {
            Success = success;
            Message = message;
        }

        public static InteractionResult Successful(
            string message = null)
        {
            return new InteractionResult(true, message);
        }

        public static InteractionResult Failed(
            string message = null)
        {
            return new InteractionResult(false, message);
        }
    }

    /// <summary>
    /// Interface cho mọi object có thể tương tác.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// Nội dung hiển thị trên Interaction UI.
        ///
        /// Ví dụ:
        /// "Mở tủ thuốc"
        /// "Nói chuyện"
        /// "Sử dụng camera"
        /// </summary>
        string GetInteractionPrompt(
            InteractionContext context);

        /// <summary>
        /// Kiểm tra Interaction hiện tại có hợp lệ hay không.
        /// </summary>
        bool CanInteract(
            InteractionContext context);

        /// <summary>
        /// Thực hiện Interaction.
        /// </summary>
        InteractionResult Interact(
            InteractionContext context);
    }

    /// <summary>
    /// Interface tùy chọn cho object cần nhận Focus Enter/Exit.
    /// </summary>
    public interface IInteractionFocus
    {
        /// <summary>
        /// Được gọi khi Interactor bắt đầu nhìn vào object.
        /// </summary>
        void OnFocusEnter(
            InteractionContext context);

        /// <summary>
        /// Được gọi khi Interactor không còn nhìn vào object.
        /// </summary>
        void OnFocusExit(
            InteractionContext context);
    }
}