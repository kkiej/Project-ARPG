using UnityEngine;

namespace LZ
{
    public class PlayerCombatManager : CharacterCombatManager
    {
        private PlayerManager player;
        
        public WeaponItem currentWeaponBeingUsed;

        protected override void Awake()
        {
            base.Awake();

            player = GetComponent<PlayerManager>();
        }

        public void PerformWeaponBasedAction(WeaponItemAction weaponAction, WeaponItem weaponPerformingAction)
        {
            if (player.IsOwner)
            {
                // 执行动作
                weaponAction.AttemptToPerformAction(player, weaponPerformingAction);
            
                // 通知服务器我们执行了动作，因此我们也从服务器端执行它
                
            }
            
            
        }
    }
}