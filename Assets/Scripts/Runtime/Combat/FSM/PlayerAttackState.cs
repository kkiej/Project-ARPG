using UnityEngine;

namespace LZ
{
    /// <summary>
    /// 玩家攻击状态：按 <see cref="MovesetData"/> 的攻击图驱动数据驱动 N 段连招（设计文档 §3.2）。
    /// （命名加 Player 前缀以区别敌人 AI 的 <c>LZ.AttackState : AIState</c>。）
    /// <para/>
    /// 阶段 4 范围：地面普通轻 / 重连招 + 重击蓄力 + 冲刺攻击 + 空中跳跃攻击。
    /// <list type="bullet">
    /// <item>进入时按上下文裁决起手节点（冲刺 / 普通，见 <see cref="ResolveOpener"/>）。</item>
    /// <item>连招开窗来自 ER TAE 的 Input-Common 窗（节点 comboWindowStart/End），开窗内缓冲到轻 / 重输入
    /// 则按 nextOnLight / nextOnHeavy 跳到下一节点。不依赖旧的 EnableCanDoCombo 动画事件。</item>
    /// <item>可蓄力节点（<see cref="AttackNode.canCharge"/>）播放 Attack→Hold→Release/FullRelease 蓄力链
    /// （执行层 <see cref="CharacterAnimatorManager.PlayHeavyAttackChainAnimation"/>，早松/满蓄由 isChargingAttack 决定）；
    /// 蓄力链期间不推进连招，整链播完 → 回到移动状态。</item>
    /// <item>取消窗（同连招窗，源自攻击 clip 的 TAE Input-Common）内缓冲到闪避 / 跳跃 → 取消进
    /// <see cref="DodgeState"/> / <see cref="JumpState"/>（经 ReturnToController 解锁攻击标志）。</item>
    /// <item>动作结束（执行层 ReturnToController 复位 isPerformingAction）→ 回到 <see cref="LocomotionState"/>。</item>
    /// </list>
    /// 不含：翻滚 / 后撤步攻击（Phase 5 DodgeState，TAE 开窗）、双持特殊 —— 这些暂走旧 WeaponItemAction 路径。
    /// 执行 / 播放 / RPC 复用 <see cref="CharacterAnimatorManager"/> 现有方法，不重写。
    /// </summary>
    public class PlayerAttackState : CharacterState
    {
        private readonly WeaponItem _weapon;
        private readonly HandMoveset _moveset;
        private readonly bool _isJumpAttack;
        private int _comboIndex;

        private PlayerAttackState(WeaponItem weapon, HandMoveset moveset, int nodeIndex, bool isJumpAttack = false)
        {
            _weapon = weapon;
            _moveset = moveset;
            _comboIndex = nodeIndex;
            _isJumpAttack = isJumpAttack;
        }

        /// <summary>
        /// 尝试按输入创建起手攻击。无主手武器 / 无 MovesetData / 无对应起手节点时返回 null（此时应回退旧路径或不动作）。
        /// </summary>
        public static PlayerAttackState TryCreateOpener(PlayerManager player, InputCommand command)
        {
            WeaponItem weapon = player.playerInventoryManager.currentRightHandWeapon;
            if (weapon == null || weapon.moveset == null)
                return null;

            HandMoveset hm = weapon.moveset.GetMoveset(GetHandState(player));
            if (hm == null)
                return null;

            int opener = ResolveOpener(player, hm, command);
            if (!hm.HasNode(opener))
                return null;

            return new PlayerAttackState(weapon, hm, opener);
        }

        /// <summary>
        /// 上下文起手裁决：冲刺攻击 &gt; 普通起手。
        /// 冲刺由 <see cref="CharacterNetworkManager.isSprinting"/> 状态量裁决（非动画事件，符合 ER：冲刺中按攻击出冲刺攻击）。
        /// 翻滚 / 后撤步攻击不在此处：其 cancel-into-attack 开窗须来自翻滚 / 后撤步 clip 的 ER TAE，
        /// 留到 Phase 5 <c>DodgeState</c>（翻滚由 FSM 驱动）内实现，避免依赖旧的 EnableCanDoRollingAttack 等动画事件。
        /// </summary>
        private static int ResolveOpener(PlayerManager player, HandMoveset hm, InputCommand command)
        {
            if (player.characterNetworkManager.isSprinting.Value && hm.HasNode(hm.runAttack))
                return hm.runAttack;

            return command == InputCommand.HeavyAttack ? hm.heavyOpener : hm.lightOpener;
        }

        /// <summary>
        /// 尝试创建空中跳跃攻击（!isGrounded 时由 <see cref="LocomotionState"/> 调用）。
        /// 轻 / 重均用 jumpLight（jumpHeavy 缺省时回退）。序列 Attack→AirIdle→Landing 由执行层
        /// <see cref="CharacterAnimatorManager.PlayJumpAttackSequenceAnimation"/> 按 isGrounded 驱动（状态量，非动画事件）。
        /// </summary>
        public static PlayerAttackState TryCreateJumpAttack(PlayerManager player, InputCommand command)
        {
            WeaponItem weapon = player.playerInventoryManager.currentRightHandWeapon;
            if (weapon == null || weapon.moveset == null)
                return null;

            HandMoveset hm = weapon.moveset.GetMoveset(GetHandState(player));
            if (hm == null)
                return null;

            int idx = command == InputCommand.HeavyAttack ? hm.jumpHeavy : hm.jumpLight;
            if (!hm.HasNode(idx))
                idx = hm.jumpLight;     // 无独立重跳攻 → 回退轻跳攻
            if (!hm.HasNode(idx))
                return null;

            return new PlayerAttackState(weapon, hm, idx, isJumpAttack: true);
        }

        /// <summary>
        /// 尝试创建闪避取消攻击（翻滚 / 后撤步攻击），由 <see cref="DodgeState"/> 在闪避动画期间缓冲到攻击时调用。
        /// 取消窗为闪避状态本身（非动画事件）；对应节点缺省时返回 null（不打断闪避）。
        /// </summary>
        public static PlayerAttackState TryCreateDodgeAttack(PlayerManager player, bool isBackstep)
        {
            WeaponItem weapon = player.playerInventoryManager.currentRightHandWeapon;
            if (weapon == null || weapon.moveset == null)
                return null;

            HandMoveset hm = weapon.moveset.GetMoveset(GetHandState(player));
            if (hm == null)
                return null;

            int idx = isBackstep ? hm.backstepAttack : hm.rollAttack;
            if (!hm.HasNode(idx))
                return null;

            return new PlayerAttackState(weapon, hm, idx);
        }

        /// <summary>下蹲攻击（a023_030310）。无下蹲攻击节点的武器返回 null（调用方据此忽略输入）。</summary>
        public static PlayerAttackState TryCreateCrouchAttack(PlayerManager player, InputCommand command)
        {
            WeaponItem weapon = player.playerInventoryManager.currentRightHandWeapon;
            if (weapon == null || weapon.moveset == null)
                return null;

            HandMoveset hm = weapon.moveset.GetMoveset(GetHandState(player));
            if (hm == null || !hm.HasNode(hm.crouchAttack))
                return null;

            return new PlayerAttackState(weapon, hm, hm.crouchAttack);
        }

        public override void OnEnter(CharacterStateMachine machine)
        {
            PlayerManager player = machine.player;

            // 与旧 RB/RT 路径一致：标记主手 + 当前所用武器（供伤害 / 网络逻辑使用）。
            player.playerNetworkManager.SetCharacterActionHand(true);
            player.playerNetworkManager.currentWeaponBeingUsed.Value = _weapon.itemID;

            PlayNode(player, _comboIndex);
        }

        public override CharacterState Tick(CharacterStateMachine machine)
        {
            PlayerManager player = machine.player;

            // 执行层每帧仍需驱动（按 attack 期间设置的 canMove/canRotate 标志归零速度 / 处理旋转），
            // 与旧系统 PlayerManager.Update 每帧调用 HandleAllMovement 行为一致。
            player.playerLocomotionManager.HandleAllMovement();

            // 连招推进：当前 clip 播放时间落在 TAE 开窗内 + 缓冲到轻 / 重输入 → 跳到下一节点。
            // 开窗来自 ER TAE（Input-Common 窗），不再依赖旧的 EnableCanDoCombo 动画事件。
            // 可蓄力节点在蓄力链期间不推进连招（让整链 Attack→Hold→Release 播完）；跳攻为一次性序列，不连招。
            if (!_isJumpAttack && _moveset.TryGetNode(_comboIndex, out AttackNode node) && !node.canCharge && IsInComboWindow(player, node))
            {
                InputCommand cmd = machine.PeekBuffered();

                // 取消到闪避 / 跳跃：取消窗 = 攻击 clip 的 TAE Input-Common 窗（与连招窗同源）。
                // 先 ReturnToController 清掉攻击锁与 isAttacking/canRoll/isJumping 等标志，
                // 使执行层 AttemptToPerformDodge / AttemptToPerformJump 的前置门通过。
                if (cmd == InputCommand.Dodge && player.characterLocomotionManager.isGrounded)
                {
                    machine.ClearBuffer();
                    player.playerAnimatorManager.ReturnToController(0f);
                    return new DodgeState();
                }
                if (cmd == InputCommand.Jump && player.characterLocomotionManager.isGrounded)
                {
                    machine.ClearBuffer();
                    player.playerAnimatorManager.ReturnToController(0f);
                    return new JumpState();
                }

                // 连招推进
                int next = -1;
                if (cmd == InputCommand.LightAttack) next = node.nextOnLight;
                else if (cmd == InputCommand.HeavyAttack) next = node.nextOnHeavy;

                if (_moveset.HasNode(next))
                {
                    machine.ClearBuffer();
                    _comboIndex = next;
                    PlayNode(player, next);
                    return null;
                }
            }

            // 动作播放结束：执行层 OnEnd → ReturnToController 已把 isPerformingAction 复位。回到移动状态。
            if (!player.isPerformingAction)
                return new LocomotionState();

            return null;
        }

        /// <summary>
        /// 判断当前 Action 层 clip 播放时间是否落在该节点的连招开窗内。
        /// 优先用节点的 TAE 窗口（秒）；若无（comboWindowEnd&lt;=0）回退到归一化 [0.35, 0.95]。
        /// </summary>
        private static bool IsInComboWindow(PlayerManager player, AttackNode node)
        {
            CharacterAnimatorManager anim = player.playerAnimatorManager;
            float length = anim.CurrentActionLength;
            if (length <= 0f)
                return false;

            float time = anim.CurrentActionTime;
            float start = node.comboWindowStart;
            float end = node.comboWindowEnd;

            // 无 TAE 数据时的回退窗口（按 clip 归一化时间）。
            if (end <= 0f)
            {
                start = 0.35f * length;
                end = 0.95f * length;
            }

            return time >= start && time <= end;
        }

        private void PlayNode(PlayerManager player, int index)
        {
            if (!_moveset.TryGetNode(index, out AttackNode node) || node.clip == null)
                return;

            // 跳跃攻击（ER 落地融合）：空中攻 03x030 →[060/jumpIdle 维持]→ 触地攻 03x070 →[落地恢复 071/072/081/082 四选一]，落地检测驱动。
            // 落地恢复按「落地距离长短 × 前序进度快慢」选 clip（执行层 SelectJumpLandingRecovery）；clip 来自节点配置（MovesetData），缺省时回退通用跳跃 idle/end。
            if (_isJumpAttack)
            {
                CharacterAnimatorManager anim = player.playerAnimatorManager;
                CharacterAnimationData ad = anim.animData;
                bool twoH = player.playerNetworkManager.isTwoHandingWeapon.Value;
                AnimationClip jumpIdle = ad == null ? null : (twoH && ad.jumpIdle2H != null ? ad.jumpIdle2H : ad.jumpIdle);
                AnimationClip jumpEnd  = ad == null ? null : (twoH && ad.jumpEnd2H  != null ? ad.jumpEnd2H  : ad.jumpEnd);

                // 空中维持优先用节点配的 060，缺省回退通用 jumpIdle。
                AnimationClip airHold = node.airHoldClip != null ? node.airHoldClip : jumpIdle;

                anim.PlayJumpAttackSequenceAnimation(
                    _weapon,
                    node.attackType,            // 空中攻命中
                    node.clip,                  // 030 空中攻
                    airHold,                    // 060 / jumpIdle
                    node.landingAttackClip,     // 070 触地攻（命中）
                    node.landingAttackType,
                    node.landingRecoveryClip,   // 071 落地恢复
                    jumpEnd,                    // 未配 070 时的回退落地
                    true,
                    node.applyRootMotion,
                    node.landingLookahead,
                    canRotate: false,
                    canMove: false,
                    canRoll: false,
                    airDamageWindowStart: node.damageWindowStart,        // 030 空中攻命中窗
                    airDamageWindowEnd: node.damageWindowEnd,
                    landingDamageWindowStart: node.landingDamageWindowStart, // 070 触地攻命中窗
                    landingDamageWindowEnd: node.landingDamageWindowEnd,
                    landingRecoveryLongClip: node.landingRecoveryLongClip,         // 072 落得高恢复
                    landingRecoveryFastClip: node.landingRecoveryFastClip,         // 081 落得低·快速
                    landingRecoveryFastLongClip: node.landingRecoveryFastLongClip, // 082 落得高·快速
                    landingLongFallThreshold: node.landingLongFallThreshold,       // 下落高度阈值
                    landingFastProgressThreshold: node.landingFastProgressThreshold); // 前序快慢阈值
                return;
            }

            // 可蓄力重击（ER 接法）：起手即播蓄满 clip(0500，前期长蓄力)；
            // 起手 minHold 内不判松手，之后松手→切短按直接出手(0505)，按到 commit→蓄满 0500 播完。
            if (node.canCharge)
            {
                player.playerAnimatorManager.PlayChargeAttackAnimation(
                    _weapon,
                    node.attackType,            // 短按/未蓄满
                    node.chargedAttackType,     // 蓄满
                    node.clip,                  // 0500 蓄满 clip
                    node.quickAttackClip,       // 0505 短按 clip
                    node.chargeMinHoldTime,
                    node.chargeCommitTime,
                    true,
                    node.applyRootMotion);
                return;
            }

            player.playerAnimatorManager.PlayTargetAttackActionAnimation(
                _weapon, node.attackType, node.clip, true, node.applyRootMotion,
                damageWindowStart: node.damageWindowStart, damageWindowEnd: node.damageWindowEnd);
        }

        private static HandState GetHandState(PlayerManager player)
        {
            if (player.playerNetworkManager.isTwoHandingWeapon.Value)
                return HandState.TwoHand;

            // 双持(power stance) 暂回退单手，后续 slice 再处理。
            return HandState.OneHandRight;
        }
    }
}
