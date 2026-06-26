# Elden Ring 动画类别号总表（aXXX）

> 来源：社区整理（With the help of Syyyke's documentation）。
> 仅作参考。ER 动画命名 `aXXX_YYYYYY`：`aXXX` = 类别号（本表），`YYYYYY` = 该类别内的具体动作槽。
>
> 与本项目的对应关系：
> - **武器类别号**（下方"Weapon Motion Category" a020-a062）即 `MovesetSlotConvention` 的 `wepMotionCategory`（如直剑 a023 → category 23）。
> - **通用动画**（a000 的 idle/翻滚/死亡/环境交互等）由 `CommonAnimationConvention` / `CommonAnimationSet` 接入（见 `docs/Combat-StateMachine-Refactor.md` §8.6）。
> - 项目内 animId 统一表示：`animId = wepMotionCategory * 1_000_000 + 六位槽位`（见 `MovesetSlotConvention.ComposeAnimId`）。

---

## 1. Idle / 持武姿态（1H / 2H）

按所持武器大类决定的站姿、待机，以及 emote、翻滚、死亡、全部环境交互动画。

| ID | 含义 |
|----|------|
| a000 | 直剑 / 太刀 / 法杖 等的 idle，以及 emote、翻滚、死亡、所有环境交互 |
| a002 | 大剑 / 曲面大剑 / 大太刀 / 巨剑·巨型武器 |
| a003 | 矛 / 戟 / 镰刀 |
| a010 | 双手：直剑 / 太刀 / 法杖 等 |
| a012 | 双手：大剑 / 曲面大剑 / 大太刀 / 巨剑·巨型武器 |
| a013 | 双手：矛 / 戟 / 镰刀 |
| a014 | 弓 |
| a015 | 弩 |
| a016 | 双手：圣印记 |

---

## 2. 武器类别（Weapon Motion Category）

对应本项目 `wepMotionCategory`。

| ID | 武器类别 |
|----|----------|
| a020 | Dagger 匕首 |
| a021 | Torch 火把 |
| a022 | Claw 拳爪 |
| a023 | Straight Sword 直剑 |
| a024 | Twinblade 双头剑 |
| a025 | Greatsword 大剑 |
| a026 | Colossal Sword 巨剑 |
| a027 | Thrusting Sword 刺剑 |
| a028 | Curved Sword 曲剑 |
| a029 | Katana 太刀 |
| a030 | Axe 斧 |
| a031 | Colossal Weapon 巨型武器 |
| a032 | Greataxe 大斧 |
| a033 | Hammer 锤 |
| a034 | Flail 连枷 |
| a035 | Great Hammer 大锤 |
| a036 | Spear 矛 |
| a037 | Great Spear 大矛 |
| a038 | Halberd 戟 |
| a039 | Heavy Thrusting Sword 重刺剑 |
| a040 | Curved Greatsword 曲面大剑 |
| a041 | Catalyst 法杖 |
| a042 | Fist 拳 |
| a043 | Whip 鞭 |
| a044 | Bow 弓 |
| a045 | Greatbow 大弓 |
| a046 | Crossbow 弩 |
| a047 | Greatshield 大盾 |
| a048 | Small Shield 小盾 |
| a049 | Medium Shield 中盾 |
| a050 | Scythe 镰刀 |
| a051 | Light Bow 轻弓 |
| a052 | Ballista 弩炮 |
| a053 | Smithscript Dagger 铸文匕首 |
| a055 | Hand to Hand 徒手格斗 |
| a056 | Perfume Bottles 香水瓶 |
| a057 | Thrust Shield 刺盾 |
| a058 | Backhand Blades 反手刃 |
| a060 | Light Greatsword 轻大剑 |
| a061 | Great Katana 大太刀 |
| a062 | Beast Claw 兽爪 |

---

## 3. 武器专属动作（Special Motion Category）

特定武器的独有动作集。

| ID | 武器 |
|----|------|
| a100 | Great Knife / Ivory Sickle / Celebrant's Sickle |
| a101 | Misericorde / Scorpion's Stinger / Glintstone Kris |
| a103 | Erdsteel Dagger / Blade of Calling / Black Knife |
| a104 | Wakizashi |
| a110 | Broadsword / Cane Sword |
| a111 | Short Sword |
| a112 | Miquellan Knight's Sword |
| a113 | Carian Knight's Sword / Lazuli Glintstone Sword |
| a117 | Warhawk's Talon |
| a120 | Eleonora's Poleblade |
| a121 | Godskin Peeler |
| a125 | Claymore |
| a126 | Flamberge |
| a127 | Sword of Milos / Death's Poker |
| a128 | Knight's Greatsword / Banished Knight's Greatsword / Inseparable Sword |
| a129 | Dark Moon Greatsword |
| a130 | Marais Executioner's Sword |
| a135 | Zweihander / Troll Knight's Sword |
| a136 | Greatsword |
| a137 | Godslayer's Greatsword |
| a138 | Ruins Greatsword |
| a145 | Estoc / Noble's Estoc |
| a146 | Rogier's Rapier |
| a147 | Frozen Needle |
| a150 | Godskin Stitcher |
| a151 | Great Épée |
| a155 | Shotel / Eclipse Shotel / Nox Flowing Sword |
| a156 | Scimitar / Shamshir |
| a157 | Flowing Curved Sword |
| a158 | Mantis Blade |
| a159 | Wing of Astel |
| a161 | Beastman's Curved Sword |
| a165 | Serpentbone Blade |
| a166 | Meteoric Ore Blade |
| a167 | Nagakiba |
| a170 | Hand Axe / Icerind Hatchet / Forked Hatchet |
| a171 | Warped Axe / Ripple Blade |
| a172 | Iron Cleaver / Celebrant's Cleaver |
| a175 | Butchering Knife |
| a176 | Pickaxe |
| a177 | Axe of Godrick / Crescent Moon Axe |
| a180 | Club / Stone Club |
| a181 | Spiked Club |
| a182 | Morning Star / Scepter of the All-Knowing |
| a183 | Mace |
| a184 | Monk's Flamemace |
| a195 | Great Club |
| a196 | Prelate's Inferno Crozier |
| a197 | Giant-Crusher |
| a198 | Golem's Halberd |
| a200 | Partisan / Spiked Spear / Death Ritual Spear |
| a201 | Pike |
| a202 | Cross-Naginata |
| a203 | Short Spear / Cleanrot Spear |
| a205 | Vyke's War Spear / Siluria's Tree |
| a206 | Treespear |
| a207 | Serpent-Hunter |
| a210 | Halberd / Commander's Standard / Banished Knight's Halberd / Dragon Halberd（原文 a210 重复） |
| a211 | Lucerne / Golden Halberd / Nightrider Glaive |
| a212 | Guardian's Swordspear / Loretta's War Sickle |
| a215 | Zamor Curved Sword |
| a216 | Omen Cleaver / Magma Wyrm's Scalesword |
| a220 | Katar / Veteran's Prosthesis |
| a221 | Caestus |
| a225 | Scythe |
| a226 | Urumi |
| a227 | Raptor Talons |
| a230 | Albinauric Bow |
| a232 | Harp Bow |
| a233 | Pulley Crossbow |
| a236 | Full Moon Crossbow |
| a240 | Seal 圣印记 |
| a246 | Dane's Footwork |
| a247 | Smithscript Spear |
| a248 | Smithscript Axe |
| a249 | Rellana Twin Blades |
| a250 | Ghostflame Torch |
| a251 | St. Trina's Torch |
| a252 | Swift Spear |
| a253 | Black Steel Greathammer |
| a254 | Claws of Night |
| a255 | Falx / Horned Warrior's Sword |
| a257 | Dancing Blade of Ranah |
| a258 | Death Knight's Twin Axes |
| a259 | Golem Fist |
| a261 | Rabbath's Cannon |
| a262 | Main-Gauche |
| a263 | Fire Knight's Greatsword |
| a264 | Lizard Greatsword |
| a265 | Curseblade's Cirque |
| a266 | Spear of the Impaler |
| a267 | Bloodfiend's Arm |
| a268 | Putrescence Cleaver |
| a831 | Axe of Godfrey（注：a831 在"剑技"段亦出现 Regal Roar，原文如此） |
| a832 | Starscourge Greatsword |
| a839 | Ghiza's Wheel |
| a852 | Ornamental Straight Sword |
| a935 | Repeating Crossbow |
| a953 | Rakshasa's Great Katana |

---

## 4. 魔法（Magic：Sorcery / Incantation）

| ID | 法术 |
|----|------|
| a400 | [Incantation] Inescapable Frenzy |
| a401 | [Sorcery] Pebble / Great Glintstone Shard / Glintstone Icecrag |
| a402 | [Sorcery] Glintstone Cometshard / Comet / Shard Spiral / Night Comet |
| a404 | [Sorcery] Crystal Barrage |
| a405 | [Sorcery] Loretta's Greatbow / Loretta's Mastery |
| a406 | [Sorcery] Rennala's Full Moon / Ranni's Dark Moon |
| a407 | [Sorcery] Comet Azur / Crystal Torrent |
| a408 | [Sorcery] Glintblade Phalanx / Carian Phalanx / Eternal Darkness |
| a409 | [Sorcery] Carian Greatsword / Adula's Moonblade |
| a410 | [Sorcery] Carian Piercer |
| a411 | [Sorcery] Scholar's Armament / Unseen Blade |
| a412 | [Sorcery] Scholar's Shield |
| a413 | [Sorcery] Terra Magicus / Starlight / Lucidity / Frozen Armament |
| a414 | [Sorcery] Zamor Ice Storm |
| a415 | [Sorcery] Meteorite / Meteorite of Aste |
| a416 | [Sorcery] Glintstone Arc |
| a417 | [Incantation] Flame Sling / Flame, Fall Upon Them / Giantsflame Take Thee / Black Flame |
| a419 | [Incantation] Flame of the Fell God |
| a420 | [Incantation] Whirl, O Flame! |
| a422 | [Incantation] Scouring Black Flame |
| a423 | [Incantation] Surge, O Flame! |
| a424 | [Incantation] Burn, O Flame! / Fire's Deadly Sin |
| a425 | [Incantation] O, Flame! |
| a426 | [Incantation] Shadow Bait / Darkness / Swarm of Flies / Poison Mist |
| a427 | [Incantation] Discus of Light / Triple Rings of Light |
| a428 | [Incantation] Rejection |
| a429 | [Incantation] Wrath of Gold |
| a431 | [Incantation] Flame, Cleanse Me / Grant Me Strength / Protect Me / Black Flame's Protection / Bestial Vitality / Bestial Constitution |
| a432 | [Incantation] Black Flame Blade |
| a433 | [Incantation] Urgent Heal / Cure Poison / Flame·Magic·Lightning·Divine Fortification |
| a434 | [Incantation] Barrier of Gold / Protection of the Erdtree / Heal 系列 / Lord's 系列 / Blessing 系列 |
| a435 | [Incantation] Golden Vow |
| a436 | [Incantation] Lightning Spear |
| a437 | [Incantation] Stone of Gurranq |
| a438 | [Incantation] Bestial Sling |
| a440 | [Incantation] Beast Claw |
| a441 | [Incantation] Gurranq's Beast Claw |
| a442 | [Incantation] Death Lightning |
| a444 | [Incantation] Ancient Dragons' Lightning Spear |
| a445 | [Incantation] Lansseax's Glaive |
| a448 | [Incantation] Radagon's Rings of Light |
| a449 | [Incantation] Immutable Shield |
| a450 | [Incantation] Agheel's Flame / Borealis's Mist / Ekzykes's Decay / Smarag's Glintstone Breath |
| a451 | [Incantation] Placidusax's Ruin |
| a452 | [Incantation] Dragonclaw |
| a454 | [Incantation] Dragonmaw |
| a455 | [Incantation] Greyoll's Roar |
| a456 | [Sorcery] Shatter Earth |
| a457 | [Sorcery] Rock Blaster |
| a458 | [Sorcery] Crystal Release |
| a459 | [Incantation] Electrify Armament / Vyke's Dragonbolt |
| a460 | [Incantation] Dragonbolt Blessing |
| a466 | [Incantation] The Flame of Frenzy |
| a467 | [Incantation] Dragonfire / Dragonice / Rotten Breath / Glintstone Breath |
| a470 | [Incantation] Aspects of the Crucible: Tail |
| a471 | [Incantation] Aspects of the Crucible: Horns |
| a474 | [Incantation] Pest Threads |
| a475 | [Sorcery] Oracle Bubbles |
| a476 | [Sorcery] Great Oracular Bubble |
| a478 | [Sorcery] Gavel of Haima |
| a479 | [Sorcery] Swift Glintstone Shard / Night Shard |
| a480 | [Sorcery] Carian Slicer |
| a481 | [Sorcery] Briars of Sin |
| a482 | [Sorcery] Briars of Punishment |
| a483 | [Sorcery] Ambush Shard |
| a484 | [Sorcery] Carian Retaliation |
| a486 | [Incantation] Law of Regression / Law of Causality / Order Healing |
| a487 | [Incantation] Litany of Proper Death |
| a488 | [Incantation] Unendurable Frenzy |
| a489 | [Incantation] Frenzied Burst |
| a490 | [Incantation] Howl of Shabriri |
| a491 | [Incantation] Elden Stars |
| a492 | [Sorcery] Cannon of Haima |
| a493 | [Incantation] Bloodboon |
| a494 | [Incantation] Bloodflame Talons |
| a495 | [Incantation] Greatblade Phalanx / Rykard's Rancor |
| a498 | [Incantation] Fortissax's Lightning Spear |
| a499 | [Incantation] Black Flame Ritual |
| a500 | [Incantation] Aspects of the Crucible: Breath |
| a502 | [Sorcery] Gelmir's Fury |
| a503 | [Sorcery] Rancorcall / Ancient Death Rancor |
| a504 | [Incantation] Order's Blade |
| a505 | [Incantation] Noble Presence |
| a506 | [Incantation] Catch Flame |
| a507 | [Sorcery] Glintstone Stars / Star Shower / Stars of Ruin / Magic Downpour |
| a508 | [Sorcery] Rock Sling |
| a509 | [Sorcery] Unseen Form |
| a510 | [Sorcery] Thops's Barrier |
| a511 | [Incantation] Honed Bolt |
| a512 | [Incantation] Bloodflame Blade / Poison Armament |
| a513 | [Sorcery] Crystal Burst / Freezing Mist / Shattering Crystal / Fia's Mist / Night Maiden's Mist |
| a514 | [Sorcery] Assassin's Approach |
| a515 | [Sorcery] Explosive Ghostflame |
| a516 | [Incantation] Black Blade |
| a517 | [Sorcery] Magma Shot / Roiling Magma |
| a518 | [Sorcery] Magic Glintblade |
| a519 | [Incantation] Frozen Lightning Spear |
| a520 | [Sorcery] Gravity Well / Collapsing Stars |
| a521 | [Sorcery] Tibia's Summons |
| a522 | [Incantation] Scarlet Aeonia |
| a523 | [Sorcery] Magma Breath / Theodorix's Magma |
| a524 | [Sorcery] Founding Rain of Stars |
| a525 | [Incantation] Aspects of the Crucible: Thorns |
| a526 | [Sorcery] Vortex of Putrescence |
| a527 | [Sorcery] Miriam's Vanishing |
| a528 | [Incantation] Minor Erdtree |
| a529 | [Incantation] Aspects of the Crucible: Bloom |
| a531 | [Incantation] Roar of Rugalea |
| a533 | [Incantation] Bayle's Tyranny |
| a534 | [Incantation] Bayle's Flame Lightning |
| a535 | [Incantation] Rotten Butterflies |
| a536 | [Incantation] Pest-Thread Spears |
| a537 | [Incantation] Midra's Flame of Frenzy |
| a538 | [Sorcery] Glintblade Trio |
| a539 | [Incantation] Knight's Lightning Spear |
| a540 | [Incantation] Furious Blade of Ansbach |
| a541 | [Incantation] Messmer's Orb |
| a543 | [Sorcery] Rings of Spectral Light |
| a544 | [Incantation] Dragonbolt of Florissax |
| a545 | [Incantation] Light of Miquella |
| a546 | [Sorcery] Spira |
| a547 | [Incantation] Divine Beast Tornado |
| a548 | [Sorcery] Golden Arcs |
| a549 | [Incantation] Multilayered Ring of Light |
| a550 | [Sorcery] Mantle of Thorns |
| a551 | [Incantation] Wrath from Afar |
| a552 | [Incantation] Divine Bird Feathers |
| a553 | [Incantation] Fire Serpent |
| a554 | [Sorcery] Giant Golden Arc |
| a556 | [Sorcery] Impenetrable Thorns |
| a557 | [Sorcery] Cherishing Fingers |
| a558 | [Sorcery] Mass of Putrescence |
| a559 | [Incantation] Rain of Fire |
| a561 | [Sorcery] Rellana's Twin Moons |

---

## 5. 战技 / 战灰（Sword Arts / Ashes of War）

| ID | 战技 |
|----|------|
| a600 | Lion's Claw |
| a601 | Impaling Thrust |
| a602 | Piercing Fang |
| a603 | Spinning Slash |
| a605 | Charge Forth |
| a606 | Stamp (Upward Cut) |
| a607 | Stamp (Sweep) |
| a608 | Blood Tax |
| a609 | Repeating Thrust |
| a610 | Wild Strikes |
| a611 | Spinning Strikes |
| a612 | Double Slash |
| a613 | Prelate's Charge |
| a614 | Unsheathe |
| a615 | Square Off |
| a616 | Giant Hunt |
| a617 | Torch Attack |
| a618 | Loretta's Slash |
| a619 | Poison Moth Flight |
| a620 | Spinning Weapon |
| a622 | Storm Assault |
| a623 | Stormcaller |
| a624 | Sword Dance |
| a625 | Spinning Chain |
| a650 | Glintblade Phalanx |
| a651 | Sacred Blade |
| a652 | Ice Spear |
| a653 | Glintstone Pebble |
| a654 | Bloody Slash |
| a655 | Lifesteal Fist |
| a656 | Eruption |
| a657 | Prayerful Strike |
| a658 | Gravitas |
| a659 | Storm Blade |
| a661 | Earthshaker |
| a662 | Golden Land |
| a663 | Flaming Strike |
| a664 | Thunderbolt |
| a665 | Lightning Slash |
| a666 | Carian Grandeur |
| a667 | Carian Greatsword |
| a668 | Vacuum Slice |
| a669 | Black Flame Tornado |
| a670 | Sacred Ring of Light |
| a671 | Firebreather |
| a672 | Blood Blade |
| a673 | Phantom Slash |
| a674 | Spectral Lance |
| a675 | Chilling Mist |
| a676 | Poisonous Mist |
| a690 | Shield Bash |
| a691 | Barricade Shield |
| a692 | Parry |
| a693 | Buckler Parry |
| a695 | Carian Retaliation |
| a696 | Storm Wall |
| a697 | Golden Parry |
| a698 | Shield Crash |
| a699 | Thops's Barrier |
| a700 | Through and Through |
| a701 | Barrage |
| a702 | Mighty Shot |
| a703 | Enchanted Shot |
| a705 | Rain of Arrows |
| a708 | Sky Shot |
| a710 | Hoarfrost Stomp |
| a711 | Storm Stomp |
| a712 | Kick |
| a713 | Lightning Ram |
| a714 | Flame of the Redmanes |
| a715 | Ground Slam |
| a716 | Golden Slam |
| a717 | Waves of Darkness |
| a718 | Hoarah Loux's Earthshaker |
| a730 | Determination |
| a731 | Royal Knight's Resolve |
| a732 | Assassin's Gambit |
| a733 | Golden Vow |
| a734 | Sacred Order |
| a735 | Shared Order |
| a736 | Seppuku |
| a737 | Cragblade |
| a740 | Barbaric Roar |
| a741 | War Cry |
| a742 | Beast's Roar |
| a743 | Troll's Roar |
| a744 | Braggart's Roar |
| a750 | Endure |
| a751 | Vow of the Indomitable |
| a752 | Holy Ground |
| a755 | Quickstep |
| a756 | Bloodhound's Step |
| a757 | Raptor of the Mists |
| a760 | White Shadow's Lure |
| a767 | Corpse Wax Cutter |
| a768 | Zamor Ice Storm |
| a769 | Radahn's Rain |
| a770 | The Queen's Black Flame |
| a771 | Dynast's Finesse |
| a772 | Magma Shower |
| a773 | Nebula |
| a774 | Death Flare |
| a775 | Bloodhound's Finesse |
| a776 | Magma Guillotine |
| a777 | Corpse Piler |
| a778 | Transient Moonlight |
| a779 | Bloodblade Dance |
| a782 | Knowledge Above All |
| a783 | Devourer of Worlds |
| a784 | Familial Rancor |
| a785 | Rosus's Summons |
| a786 | Thunderstorm |
| a787 | Sacred Phalanx |
| a788 | Great-Serpent Hunt |
| a789 | Angel's Wings |
| a790 | Storm Kick |
| a791 | Unblockable Blade |
| a792 | Sorcery of the Crozier |
| a793 | Erdtree Slam |
| a794 | Gravity Bolt |
| a795 | Fires of Slumber |
| a796 | Golden Retaliation |
| a797 | Contagious Fury |
| a798 | Ordovis's Vortex |
| a799 | Spinning Weapon |
| a800 | Surge of Faith |
| a801 | Flame Spit |
| a802 | Tongues of Fire |
| a803 | Oracular Bubble |
| a804 | Bubble Shower |
| a805 | Great Oracular Bubble |
| a806 | Sea of Magma |
| a807 | Viper Bite |
| a808 | Moonlight Greatsword |
| a809 | Siluria's Woe |
| a810 | Rallying Standard |
| a811 | Bear Witness! |
| a812 | Eochaid's Dancing Blade |
| a813 | Soul Stifler |
| a814 | Taker's Flames |
| a815 | Shriek of Milos |
| a816 | Reduvia Blood Blade |
| a817 | Glintstone Dart |
| a818 | Flowing Form |
| a819 | Night-and-Flame Stance |
| a820 | Wave of Gold |
| a821 | Ruinous Ghostflame |
| a822 | Establish Order |
| a823 | Mists of Slumber |
| a824 | Spearcall Ritual |
| a825 | Wolf's Assault |
| a826 | Thundercloud Form |
| a827 | Cursed-Blood Slice |
| a828 | Waterfowl Dance |
| a829 | Gold Breaker |
| a830 | I Command Thee, Kneel! |
| a831 | Regal Roar |
| a832 | Starcaller Cry |
| a833 | Wave of Destruction |
| a834 | Bloodboon Ritual |
| a835 | Flowing Form |
| a836 | Blade of Death |
| a837 | Blade of Gold |
| a838 | Destined Death |
| a839 | Spinning Wheel |
| a840 | Alabaster Lords' Pull |
| a841 | Onyx Lords' Repulsion |
| a842 | Oath of Vengeance |
| a843 | Ice Lightning Sword |
| a844 | Regal Beastclaw |
| a845 | Flame Dance |
| a846 | Claw Flick |
| a847 | Nebula |
| a848 | Ghostflame Ignition |
| a849 | Ancient Lightning Spear |
| a850 | Frenzyflame Thrust |
| a851 | Miquella's Ring of Light |
| a852 | Golden Tempering |
| a853 | Last Rites |
| a854 | Unblockable Blade |
| a856 | Aspect of the Crucible: Wings |
| a858 | Dryleaf Whirlwind |
| a860 | Spinning Gravity Thrust |
| a861 | Palm Blast |
| a862 | Piercing Throw |
| a863 | Scattershot Throw |
| a864 | Wall of Sparks |
| a865 | Rolling Sparks |
| a869 | Painful Strike |
| a872 | Hone Blade |
| a873 | Raging Beast |
| a874 | Savage Claws |
| a875 | Red Bear Hunt |
| a876 | Blind Spot |
| a877 | Swift Slash |
| a878 | Overhead Stance |
| a879 | Wing Stance |
| a880 | Blackbolt |
| a881 | Flame Skewer |
| a882 | Savage Lion's Claw |
| a883 | Divine Beast Frost Stomp |
| a884 | Flame Spear |
| a885 | Carian Sovereignty |
| a886 | Shriek of Sorrow |
| a900 | Dragonwound Slash |
| a901 | Needle Piercer |
| a902 | Light |
| a903 | Darkness |
| a904 | Onze's Line of Stars |
| a905 | The Poison Flower Blooms Twice |
| a908 | Spinning Guillotine |
| a909 | Unending Dance |
| a910 | Revenger's Blade |
| a911 | Mists of Eternal Sleep |
| a913 | Dynastic Sickleplay |
| a914 | Blinkbolt: Twinaxe |
| a915 | Blinkbolt: Long-hafted Axe |
| a916 | Promised Consort |
| a917 | Shadow Sunflower Headbutt |
| a918 | Moon-and-Fire Stance |
| a919 | Devonia's Vortex |
| a920 | Messmer's Assault |
| a922 | Sleep Evermore |
| a923 | Golden Crux |
| a924 | Moore's Charge |
| a925 | White Light Charge |
| a927 | Witching Hour Slash |
| a928 | Euporia Vortex |
| a929 | Smithing Art Spears |
| a931 | Romina's Purification |
| a932 | Poison Spear-Hand Strike |
| a933 | Madding Spear-Hand Strike |
| a934 | Feeble Lord's Frenzied Flame |
| a935 | Repeating Fire |
| a936 | Deadly Dance |
| a937 | Fan Shot |
| a948 | Discus Hurl |
| a949 | Flower Dragonbolt |
| a950 | Kowtower's Resentment |
| a951 | Solitary Moon Slash |
| a952 | Revenge of the Night |
| a953 | Weed Cutter |
| a954 | Blindfold of Happiness |
| a956 | Jori's Inquisition |
| a957 | Igon's Drake Hunt |
| a958 | Deadly Poison Spray |
| a959 | Roaring Bash |
| a960 | Flare, O Serpent |
| a961 | Scattershot (Claws) |
| a962 | Horn Calling |
| a963 | Horn Calling: Storm |
| a964 | Ghostflame Call |
| a965 | Rancor Slash |
| a967 | Tremendous Phalanx |
| a968 | Bloodfiends' Bloodboon |
| a969 | Dragonform Flame |
| a970 | Lightspeed Slash |
| a971 | Rancor Shot |

---

> 备注（原文存在的重复 / 冲突，已原样保留待核对）：
> - `a210` 在"武器专属动作"段出现两次（Halberd 系 与 Dragon Halberd）。
> - `a831` / `a832` / `a839` / `a852` / `a935` / `a953` 在"武器专属动作"与"战技"两段都出现，含义不同，需按实际 FBX 命名核对。

---

## 6. a000 段通用动作明细 + 负重·姿态编码规则（来源：Animation ID Sheet，社区）

### 6.0 编码规则（重要，权威来源解码）

通用动作（idle/翻滚/后撤步/手势等）的 ID 由三部分编码：

```
a{stance}_{action + load*10 + direction}
```

- **stance（姿态/武器大类）= aXXX 前缀**，不是 a000 内的子号：
  | 前缀 | 姿态 |
  |------|------|
  | a000 | 单手 / 双武器·轻型武器姿态（直剑/太刀/法杖…） |
  | a002 | 双武器·右手重型武器姿态（大剑/巨剑…） |
  | a003 | 双武器·右手长柄武器姿态（矛/戟/镰…） |
  | a010 | 双手·轻型武器姿态 |
  | a012 | 双手·重型武器姿态 |
  | a013 | 双手·长柄武器姿态 |
  | a014 | 双手·弓姿态 |
  | a015 | 双手·盾姿态（注：与 §1 表的"a015=弩"冲突，待核对） |
  | a016 | 双手·弩姿态（注：与 §1 表的"a016=2H 圣印记"冲突，待核对） |
- **load（负重）= 十位 ×10，4 档**：light=0 / medium=10 / heavy=20 / overweight(超重)=30。
  （运行期实测确认：穿重甲/持重武使总负重%升档，移动 ID 的十位随之变化。当前项目暂只用 light=0，未接负重系统。）
- **direction（方向）= 个位 +0..3**：前 0 / 后 1 / 左 2 / 右 3。

> 即 `animId = 前缀号 * 1_000_000 + (action基址 + load*10 + dir)`，与 `MovesetSlotConvention.ComposeAnimId` 一致；
> 多前缀（a000/a010/a012…）天然由 `CharacterAnimationLibrary` 按 animId 区分。

#### 6.0.1 运行期实测验证（Elden-Ring-Debug-Tool + 实际 clip 文件比对）

调试工具显示的"运行时动画 ID"就是上式算出的 `animId`（a000 前缀=0 时省略前缀，故看着像纯后缀）：

| 运行时 ID | 拆解 | 文件 | 含义 | 核对 |
|---|---|---|---|---|
| `0` | 0×1e6 + 0 | a000_000000 | idle | ✓ |
| `20000/1/2/3` | 0 + 20000 + 0 + dir | a000_020000.. | 慢走 前/后/左/右 | ✓ |
| `20100/1/2/3` | 0 + 20100 + 0 + dir | a000_020100.. | 快走 四向 | ✓ |
| `20200` | 0 + 20200 | a000_020200 | 奔跑(前) | ✓ |
| `2020010` | **2**×1e6 + 20000 + **10** + 0 | a002_020010 | 大剑·走·**中载**·前 | ✓ |
| `10020000` | **10**×1e6 + 20000 | a010_020000 | 双手轻·走·前 | ✓ |
| `10000000` | 10×1e6 + 0 | a010_000000 | 双手轻·idle | ✓ |

**结论**：`animId = stance×1,000,000 + (base + load×10 + dir)` 即 ER 运行期真实编码，与项目 `CommonAnimationConvention.ComposeId` 完全一致。

#### 6.0.2 Locomotion 六位 suffix 的完整结构

移动类 suffix（`02xxxx`）实为 4 段:

```
suffix = 0 2 [d3 状态] [d4 速度] [d5 负重] [d6 方向]
```
- **d3 状态**：`0`=移动循环 / `2`=刹停（"停"，如 `022100`）。
- **d4 速度**：`0`=慢走 / `1`=快走 / `2`=奔跑。
- **d5 负重**：`0`轻 / `1`中 / `2`重 / `3`超重（即上面的 load）。
- **d6 方向**：`0`前 / `1`后 / `2`左 / `3`右。

故 `base + load*10 + dir` 公式中：walk base=`020000`、jog base=`020100`、run base=`020200`；刹停 base=`022100`(jog)/`022200`(run)。

### 6.1 手势（Gestures，a000_080xxx）

| ID | 手势 |
|----|------|
| a000_080200 | Point Forwards |
| a000_080210 | Point Upwards |
| a000_080220 | Point Downwards |
| a000_080240 | Wait! |
| a000_080250 | Calm Down! |
| a000_080070 | Wave |
| a000_080230 | Beckon |
| a000_080080 | Casual Greeting |
| a000_080060 | Warm Welcome |
| a000_080600 | Bravo! |
| a000_080090 | Strength! |
| a000_080700 | Jump for Joy |
| a000_080710 | Triumphant Delight |
| a000_080720 | Fancy Spin |
| a000_080300 | Nod in Thought |
| a000_080730 | Finger Snap |
| a000_080500 | Rallying Cry |
| a000_080510 | Heartening Cry |
| a000_080000 | Bow |
| a000_080010 | Polite Bow |
| a000_080100 | As You Wish |
| a000_080020 | My Thanks |
| a000_080030 | Curtsy |
| a000_080040 | Reverential Bow |
| a000_080520 | By My Sword |
| a000_080530 | Hoslow's Oath |
| a000_080400 | Extreme Repentance |
| a000_080410 | Grovel for Mercy |

### 6.2 后撤步（Backward hop，action 基址 027000，前缀=姿态，+load*10，无方向）

| 姿态 | light(00) | medium(10) | heavy(20) |
|------|-----------|------------|-----------|
| 单手/双武器·轻 (a000) | a000_027000 | a000_027010 | a000_027020 |
| 双手·轻 (a010) | a010_027000 | a010_027010 | a010_027020 |
| 双手·重 (a012) | a012_027000 | a012_027010 | a012_027020 |
| 双武器·右手重 (a002) | a002_027000 | a002_027010 | a002_027020 |
| 双武器·右手长 (a003) | a003_027000 | a003_027010 | a003_027020 |
| 双手·长 (a013) | a013_027000 | a013_027010 | a013_027020 |
| 双手·弓 (a014) | a014_027000 | a014_027010 | a014_027020 |
| 双手·弩 (a016) | a016_027000 | a016_027010 | a016_027020 |
| 双手·盾 (a015) | a015_027000 | a015_027010 | a015_027020 |

### 6.3 翻滚（Dodging，a000，action 基址 027100，+load*10，+dir）

> 未锁定目标时只有前向翻滚；锁定后才有四向。

| 负重 | 前(0) | 后(1) | 左(2) | 右(3) |
|------|-------|-------|-------|-------|
| light (00) | a000_027100 | a000_027101 | a000_027102 | a000_027103 |
| medium (10) | a000_027110 | a000_027111 | a000_027112 | a000_027113 |
| heavy (20) | a000_027120 | a000_027121 | a000_027122 | a000_027123 |

> 注：本权威表每档负重只列 4 向（…100-103）。早前 `CommonAnimationInstruction.md` 手记里出现的
> `…104-107`（第二组四向）在此未确认，可能不是标准翻滚，待核对。

### 6.4 Locomotion 移动循环（base 随姿态前缀，+load*10，+dir）

> 运行期实测确认（单手 a000、轻载 d5=0）。十位接负重、个位接方向。

| 动作 | base(后缀) | 前(0) | 后(1) | 左(2) | 右(3) |
|------|-----------|-------|-------|-------|-------|
| idle | `000000` | a000_000000 | — | — | — |
| 慢走 walk | `020000` | a000_020000 | a000_020001 | a000_020002 | a000_020003 |
| 快走 jog | `020100` | a000_020100 | a000_020101 | a000_020102 | a000_020103 |
| 奔跑 run | `020200` | a000_020200 | (a000_020210=冲刺?) | — | — |
| 快走刹停 jog-stop | `022100` | a000_022100 | a000_022101 | a000_022102 | a000_022103 |
| 奔跑刹停 run-stop | `022200` | a000_022200 | — | — | — |

> 双手轻=a010、大剑(双武器右手重)=a002 等其它姿态同构，仅前缀不同（如 a010_020000、a002_020010）。

### 6.5 跳跃 / 落地（a000_202xxx，base + 速度档*10）

> 注意跳跃族前缀是 `202`（不是移动的 `02`）。速度档：站 0 / 走 1 / 快走 2 / 奔跑 3，进十位。

| 动作 | 站立(0) | 走(10) | 快走(20) | 奔跑(30) |
|------|---------|--------|----------|----------|
| 起跳 jump | a000_202000 | a000_202010 | a000_202020 | a000_202030 |
| 落地 land | a000_202100 | a000_202100 | a000_202110 | a000_202120 |

> 实测：站跳/前走跳落地都用 `202100`；快走跳落地 `202110`、其"落地后接走"= `202115`；奔跑跳落地 `202120`。

### 6.6 换武 / 喝药 / 无道具（a000_0xxxxx）

| 动作 | base | 说明 |
|------|------|------|
| 换右手武器 | `029001`~`029021` | 武器→武器 |
| 右手 盾→武器 | `029000`~`029020` | |
| 换左手武器 | `029031`~`029051` | |
| 左手 盾 | `029030`~`029050` | |
| 喝药（有药） | `050110` / `050111` / `050112` | 起手 / 饮 / 收（一组三段，DSAS 已核实） |
| **空手/无道具** | `050050` | **未装备药、或药已喝光时，"喝药"动作改播此动画** |

> ✅ **TAE 事件 type / flag 的权威全量表已迁到 `TAE_Events/README.md` §5**，来源是 DSAS 导出的
> **c0000 完整 TAE 转储 `c0000.anibnd.dcx.txt`**（13,700 个动画，名称/type/flag 全对）。不再需要 DSAS 逐个看。
> `TAE_Events/*.json` 里每个事件的 `name`/`category` 字段是早期脚本错误猜测的，**严禁按 `name` 匹配**，
> 运行期一律按 **`type` + `params`** 匹配，含义查 README §5。
>
> 几处此前被改错 / 待核实、现已由转储定论的要点：
> - `type 0`=`ChrActionFlag`（无敌/霸体/连招窗/取消窗/输入窗**全是它的 flag**，不是独立 type）；
>   `type 1`=`AttackBehavior`（近战判定）、`2`=`BulletBehavior`、`65`=**`ConsumeCurrentGoods`（喝药回血本体）**。
> - `type 144/145`=**`RumbleCam_Local/Global`**（手柄/镜头震动），**不是** Invulnerability/SuperArmor——后者根本不存在为独立 type。
> - `112`=`SpawnFFX_FloorDetermined`、`114`=`SpawnFFX_GoodsAndMagic`、`116`=`SpawnFFX_Throw`、`120`=`SpawnFFX_ChrType`（均特效，非判定盒）。
> - `605`=`SetTimeActEditorHavokVariable`、`700`=`EnableTwistModifier`、`792`=`FootSfxParam_Entity`（`RootMotion_Mult`/`WeaponTrail`/`MovementFlag` 都是旧错名）。
>
> **无敌 / 霸体全是 `type0` 的 flag**：`8`=Flag As Dodging（翻滚无敌帧）、`24`=Super Armor（霸体）、
> `67`=Invincible Excluding Throw、`94`=Perfect Invincibility、`132`=Jump Frames (Lower Body IFrames)、
> `143`=PvE Only IFrames、`134`=Falling Jump Frames(配合 51)。
>
> **翻滚无敌帧（iframe，c0000 转储实测）**：
> - 前滚 `a000_027100`：flag 8 (Flag As Dodging) 帧 **0–16** → **0~0.533s@30fps**（clip 约 60 帧/2.0s）。已写入
>   `CommonAnimationConvention.rollIFrameStartSeconds/EndSeconds`（0 / 16⁄30），`DodgeState` 据此开关 `isInvulnerable`。
> - 后撤步 `a000_027000`：flag 8 帧 **0–7**（≈0~0.233s）+ flag 143 (PvE Only IFrames) 帧 **7–11**。
>   即后撤步**也有短无敌**（前 7 帧全无敌、7–11 帧仅对 AI 无敌），并非"无 iframe"。
>
> **连招窗语义注意**：`flag 87 (Input-Common)` 是「输入缓冲窗」（何时能按下并记住下一击），范围很宽（a023_030000
> 实测 0.367~2.767s）；真正「能执行下一段」的取消门是 `flag 115 (Cancel-R1Attack, 0.567~0.7s)` /
> `flag 4 (Cancel-RHAttack, 0.7~2.767s)`。当前 `MovesetAutoFiller` 用 flag 87 取窗 → 连招最早可在挥砍中途接出，
> 比 ER 实际可取消点偏早。更贴 ER 的做法：输入缓冲用 flag 87，开窗起点用 flag 4/115（待定，未改）。
>
> **喝药回血时点**：`a000_050111`（饮）含 `ConsumeCurrentGoods` 事件（消耗道具/生效），DSAS 实测在
> **第 8 帧**、clip 共 **30 帧(1s@30fps)** → 归一化 **8/30 ≈ 0.2667**，已写入
> `CommonAnimationConvention.flaskConsumeNormalizedTime`。运行时（本机 + 远端复制体）以 Animancer 定时事件在此
> 触发 `SuccessfullyUseQuickSlotItem`，替代 ER clip 缺失的 Unity AnimationEvent。
