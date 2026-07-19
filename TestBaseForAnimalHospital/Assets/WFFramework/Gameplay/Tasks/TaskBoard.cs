using System.Collections.Generic;

namespace WFFramework.Gameplay.Tasks
{
    /// <summary>
    /// Lưu các Task đang chờ Agent nhận.
    /// </summary>
    public sealed class TaskBoard<TAgent>
    {
        /// <summary>
        /// Danh sách Task runtime.
        /// </summary>
        private readonly List<IGameTask<TAgent>> _tasks =
            new List<IGameTask<TAgent>>();

        /// <summary>
        /// Thêm Task vào Board.
        /// </summary>
        public bool Add(IGameTask<TAgent> task)
        {
            if (task == null || _tasks.Contains(task))
            {
                return false;
            }

            _tasks.Add(task);
            return true;
        }

        /// <summary>
        /// Xóa Task khỏi Board.
        /// </summary>
        public bool Remove(IGameTask<TAgent> task)
        {
            return _tasks.Remove(task);
        }

        /// <summary>
        /// Tìm Task có điểm cao nhất mà Agent có thể nhận.
        /// </summary>
        public IGameTask<TAgent> FindBestTask(
            TAgent agent)
        {
            IGameTask<TAgent> bestTask = null;
            float bestScore = float.MinValue;

            for (int i = 0; i < _tasks.Count; i++)
            {
                IGameTask<TAgent> task = _tasks[i];

                if (!task.CanAssign(agent))
                {
                    continue;
                }

                float score =
                    task.CalculateScore(agent);

                if (score <= bestScore)
                {
                    continue;
                }

                bestScore = score;
                bestTask = task;
            }

            return bestTask;
        }

        /// <summary>
        /// Xóa các Task đã kết thúc khỏi Board.
        /// </summary>
        public void RemoveFinishedTasks()
        {
            for (int i = _tasks.Count - 1; i >= 0; i--)
            {
                GameTaskStatus status =
                    _tasks[i].Status;

                if (status == GameTaskStatus.Completed ||
                    status == GameTaskStatus.Failed ||
                    status == GameTaskStatus.Cancelled)
                {
                    _tasks.RemoveAt(i);
                }
            }
        }
    }
}