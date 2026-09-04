using System;
using System.Collections.Generic;
using _GameToolkit.Updater;
using UnityEngine;

namespace _TDS.Battle
{
    /// <summary>
    /// TickRunner riêng cho Projectile bay — chạy CÙNG nhịp 30Hz với ColliderTickRunner
    /// (đều nằm trong UpdateRunner.updatables, ProjectileRunner đứng TRƯỚC ColliderTickRunner)
    /// để đạn di chuyển xong 1 bước rồi collision detect ngay trong cùng tick -> không xuyên, không lệch.
    /// </summary>
    public sealed class ProjectileTickRunner : TickRunner<Projectile>, IDisposable
    {
        public static ProjectileTickRunner Instance { get; private set; }

        public ProjectileTickRunner()
        {
            if (Instance == null) Instance = this;
        }

        public void Dispose()
        {
            if (Instance != null && Instance == this) Instance = null;
        }
    }
}
