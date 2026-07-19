using UnityEngine;

namespace WFFramework.Gameplay.Interaction
{
    /// <summary>
    /// Component Raycast từ Camera để tìm IInteractable.
    ///
    /// Component này chỉ:
    /// - Raycast.
    /// - Quản lý Focus.
    /// - Gọi Interact.
    ///
    /// Nó không chứa logic cụ thể của cửa, item hoặc patient.
    /// </summary>
    public sealed class RaycastInteractor : MonoBehaviour
    {
        /// <summary>
        /// Camera dùng để bắn Ray.
        /// </summary>
        [SerializeField]
        private Camera interactionCamera;

        /// <summary>
        /// Khoảng cách tương tác tối đa.
        /// </summary>
        [SerializeField, Min(0.1f)]
        private float interactionDistance = 3f;

        /// <summary>
        /// Layer được phép tương tác.
        /// </summary>
        [SerializeField]
        private LayerMask interactionMask;

        /// <summary>
        /// Interactable đang được Focus.
        /// </summary>
        private IInteractable _currentInteractable;

        /// <summary>
        /// Component MonoBehaviour chứa current interactable.
        ///
        /// Dùng để kiểm tra object đã bị Destroy hay chưa.
        /// </summary>
        private MonoBehaviour _currentBehaviour;

        /// <summary>
        /// Payload tùy chọn được gửi trong lần Interaction tiếp theo.
        /// </summary>
        private object _interactionPayload;

        /// <summary>
        /// Prompt hiện tại để UI đọc.
        /// </summary>
        public string CurrentPrompt { get; private set; }

        /// <summary>
        /// True nếu đang Focus một Interactable hợp lệ.
        /// </summary>
        public bool HasInteractable =>
            _currentInteractable != null &&
            _currentBehaviour != null;

        private void Awake()
        {
            if (interactionCamera == null)
            {
                interactionCamera = Camera.main;
            }
        }

        /// <summary>
        /// Chỉ kiểm tra Focus trong Update.
        ///
        /// Input System nên gọi TryInteract riêng khi người chơi nhấn nút.
        /// </summary>
        private void Update()
        {
            UpdateFocus();
        }

        /// <summary>
        /// Gán Payload cho Interaction tiếp theo.
        ///
        /// Ví dụ Player đang cầm thuốc hoặc đơn đăng ký.
        /// </summary>
        public void SetPayload(object payload)
        {
            _interactionPayload = payload;
        }

        /// <summary>
        /// Thử tương tác với object đang được Focus.
        /// </summary>
        public InteractionResult TryInteract()
        {
            if (!HasInteractable)
            {
                return InteractionResult.Failed(
                    "Không có đối tượng để tương tác.");
            }

            InteractionContext context =
                CreateContext();

            if (!_currentInteractable.CanInteract(context))
            {
                return InteractionResult.Failed(
                    "Không thể tương tác lúc này.");
            }

            return _currentInteractable.Interact(context);
        }

        /// <summary>
        /// Raycast và cập nhật object đang được Focus.
        /// </summary>
        private void UpdateFocus()
        {
            Ray ray = interactionCamera.ViewportPointToRay(
                new Vector3(0.5f, 0.5f, 0f));

            if (!Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    interactionDistance,
                    interactionMask,
                    QueryTriggerInteraction.Collide))
            {
                ClearFocus();
                return;
            }

            if (!TryFindInteractable(
                    hit.collider,
                    out IInteractable interactable,
                    out MonoBehaviour behaviour))
            {
                ClearFocus();
                return;
            }

            if (ReferenceEquals(
                    interactable,
                    _currentInteractable))
            {
                UpdatePrompt();
                return;
            }

            ClearFocus();

            _currentInteractable = interactable;
            _currentBehaviour = behaviour;

            InteractionContext context =
                CreateContext();

            if (_currentInteractable is IInteractionFocus focusable)
            {
                focusable.OnFocusEnter(context);
            }

            UpdatePrompt();
        }

        /// <summary>
        /// Xóa Focus hiện tại và gửi FocusExit.
        /// </summary>
        private void ClearFocus()
        {
            if (_currentInteractable != null)
            {
                InteractionContext context =
                    CreateContext();

                if (_currentInteractable
                    is IInteractionFocus focusable)
                {
                    focusable.OnFocusExit(context);
                }
            }

            _currentInteractable = null;
            _currentBehaviour = null;
            CurrentPrompt = null;
        }

        /// <summary>
        /// Cập nhật prompt từ Interactable hiện tại.
        /// </summary>
        private void UpdatePrompt()
        {
            if (!HasInteractable)
            {
                CurrentPrompt = null;
                return;
            }

            CurrentPrompt =
                _currentInteractable.GetInteractionPrompt(
                    CreateContext());
        }

        /// <summary>
        /// Tạo InteractionContext từ dữ liệu hiện tại.
        /// </summary>
        private InteractionContext CreateContext()
        {
            return new InteractionContext(
                gameObject,
                _interactionPayload);
        }

        /// <summary>
        /// Tìm IInteractable trong Collider hoặc Parent.
        ///
        /// Duyệt MonoBehaviour vì một số phiên bản Unity
        /// không tìm Interface ổn định bằng GetComponentInParent.
        /// </summary>
        private static bool TryFindInteractable(
            Collider targetCollider,
            out IInteractable interactable,
            out MonoBehaviour behaviour)
        {
            interactable = null;
            behaviour = null;

            MonoBehaviour[] behaviours =
                targetCollider.GetComponentsInParent<MonoBehaviour>(
                    true);

            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is IInteractable found)
                {
                    interactable = found;
                    behaviour = behaviours[i];
                    return true;
                }
            }

            return false;
        }

        private void OnDisable()
        {
            ClearFocus();
        }
    }
}