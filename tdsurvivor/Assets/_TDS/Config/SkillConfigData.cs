using System;
using UnityEngine;

namespace _TDS.Config
{
    [Serializable]
    public struct SkillConfigData
    {
        /*
         * id của skill
         */
        public int skillId;
        
        /*
         * projectile asset
         */
        public string prefabName; // asset key của projectile prefab

        /*
         * Mặc định là 0: sát thương trực tiếp
         * Nếu lớn hơn 0: thì là DOT
         */
        public float damageTickInterval;

        /*
         * Kiểu tìm target khi cast skill
         */
        public FindTargetType findTarget;
        
        /*
         * Phạm vi tìm kiếm mục tiêu tấn công
         */
        public float attackRange;

        /*
         * Kiểu quỹ đạo bay của đạn
         */
        public TrajectoryType trajectory;

        /*
         * Trường hợp trajectory = Stationary.
         * Thì bắt đầu sử dụng pivot cho vị trí đạn
         */
        public PivotStationaryType pivotStationary;
        
        /*
         * Vận tốc chung của đạn
         */
        public float projectileSpeed;
        
        /*
         * Thời gian bắt đầu kích hoạt của đạn (với trường hợp đạn có nhiều phase như: Spline, Boomerange...)
         */
        public float projectileStartDuration;

        /*
         * Thời gian đạn kích hoạt ở phase chính. Thường thì dùng cho tất cả các loại đạn
         */
        public float projectileDuration;
        
        /*
         * Thời gian bắt đầu kết thúc của đạn (với trường hợp đạn có nhiều phase như: Spline, Boomerange...)
         */
        public float projectileEndDuration;
        
        /*
         * Scale của đạn: sẽ nhân với hệ số của đạn bao gồm: visual, collider...
         */
        public float projectileSize;

        /*
         * Delay 1 khoảng thời gian trớc khi kích hoạt xác định va chạm
         */
        public float collisionDelayInit;
        
        /*
         * Tổng thời gian kích hoạt xác định va chạm
         */
        public float collisionDuration;

        /*
         * Tổng số lần hit mục tiêu
         */
        public int hitCount;
        
        /*
         * Thời gian check va chạm trên cùng 1 kẻ địch. kích hoạt 
         */
        public float hitInterval;
        
        /*
         * đây là khoảng cách giữa mỗi viên đạn
         */
        public float projectileDistanceStep;

        /*
         * đây là số góc của mỗi viên đạn
         */
        public float projectileAngleStep;
        
        /*
         * Số lượng đạn hình quạt 
         */
        public float spreadBonusProjectileCount;
        
        /*
         * Hệ số sát thương gây ra bởi mỗi viên đạn hình quạt
         */
        public float spreadDamageScale;
        
        /*
         * Số lượng đạn bắn song song (đạn phụ)
         */
        public int parallelBonusProjectileCount;
        
        /*
         * Hệ số sát thương gây ra bởi mỗi viên đạn bắn song song (ko phải đạn gốc)
         */
        public float parallelDamageScale;
        
        /*
         * Aura vfx asset
         */
        public string explosiveAssetName;
        
        /*
         * Bán kinh vụ nổ
         */
        public float explosiveRadius;
        
        /*
         * Hệ số sát thương của vụ nổ
         */
        public float explosiveDamageScale;

        /*
         * Mục tiêu dưới % máu này bị tiêu diệt ngay lập tuc
         */
        public float instantKillTargetBelowHealthPercent;
    }
}
