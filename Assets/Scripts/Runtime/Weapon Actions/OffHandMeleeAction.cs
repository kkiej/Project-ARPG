using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    [CreateAssetMenu(menuName = "Character Actions/Weapon Actions/Off Hand Melee Action")]
    public class OffHandMeleeAction : WeaponItemAction
    {
        // 问：为何称其为“副手近战动作”而非“格挡动作”？

        // 答：这是为了系统扩展性考虑。若角色将来同时装备主手与副手同类武器时，副手动作将不再是格挡，而是会转变为双武器协同攻击。

        public override void AttemptToPerformAction(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction)
        {
            base.AttemptToPerformAction(playerPerformingAction, weaponPerformingAction);

            // 检查是否触发强力姿态动作（双武器攻击）

            // 检查是否可格挡
            if (!playerPerformingAction.playerCombatManager.canBlock)
                return;

            //  IF WE ARE USING AN ITEM, DO NOT PROCEED
            if (playerPerformingAction.playerCombatManager.isUsingItem)
                return;

            //  CHECK FOR ATTACK STATUS
            if (playerPerformingAction.playerNetworkManager.isAttacking.Value)
            {
                // 禁用格挡（使用短矛/中矛时允许在轻攻击期间进行格挡反击。此逻辑由其他动作类处理）
                if (playerPerformingAction.IsOwner)
                    playerPerformingAction.playerNetworkManager.isBlocking.Value = false;

                return;
            }

            if (playerPerformingAction.playerNetworkManager.isBlocking.Value)
                return;

            if (playerPerformingAction.IsOwner)
                playerPerformingAction.playerNetworkManager.isBlocking.Value = true;
        }
    }
}