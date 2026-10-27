# 古剑魂梦 · Unity 重建工程

这是从附件 APK 的原始资源和可读 Java 代码重建的 **Unity 2022.3.62f3** 工程，可编辑并继续开发。历史里程碑为 **95% 工作量估算**；当前逐项还原状态、实测证据和剩余范围见 [还原进度](Reference/还原进度.md)。该百分比不代表原版逐帧相似度或正常难度通关率。

当前逐项状态见[还原工作清单](Reference/还原工作清单.md)。选择框原软键9确认专项见[报告](Reference/Validation/choice-soft-ok-batch.json)；人物对话专项见[9组汇总](Reference/Validation/character-dialogue-batch.json)；最新消息及选择说明文字描边／阴影验收见[38组汇总](Reference/Validation/text-style-batch.json)，四类脚本奖励提示见[50组汇总](Reference/Validation/reward-prompts-batch.json)。音效原版调用点与 Unity 源绑定见[专项核验](Reference/Validation/audio-source-audit.json)。暂不打包，后续继续在现有工程添加功能。

本轮已将原版菜单左右切页、上下列表循环、确认／返回入口接入 `OriginalGameMenus`，世界地图按反编译 `GameCanvas.handleKey_map` 恢复固定方向邻接；系统设置新增原版五档总音量和自动用药阈值。Unity 后台运行探针验证了0/25/50/75/100音量、静音门控和音效重播。菜单输入专项报告见[menu-input-batch.json](Reference/Validation/menu-input-batch.json)，音量专项见[audio-source-audit.json](Reference/Validation/audio-source-audit.json)；实体 GameView 键盘／触屏及听音验收尚未完成。

## 打开与运行

1. Unity Hub 添加 `UnityProject`，选择 Unity 2022.3.62f3。
2. 打开 `Assets/Scenes/GuJian.unity`，Play 后选择“开始游戏”。
3. WASD／方向键移动，E／空格交谈，J 普攻，K 武器技能，L 崩击，I 合体，O 葬剑，M 当前区域小地图，Esc 菜单。对话可点击继续或按空格／回车。

当前测试入口为 Unity Game 窗口。历史 Mac 构建及其报告保留作归档，不覆盖本轮源码。

原版 HUD、对话、角色、装备、物品、剑灵、任务、系统、世界地图、商店、强化和试穿已接入原图集，菜单保留实际操作。界面继续使用 IMGUI 绘制，坐标与纹理引用保存在 Unity 资产中。触屏和鼠标的操作区域支持多点输入；方向盘与按键使用原始图集，默认命中区域和图片中心按原代码迁移。普攻键可优先交谈，右上角按钮打开当前区域小地图。

固定地图、场景物件、角色拼帧节点、Sprite、音源与界面资源引用均在编辑器生成并保存。动态角色、弹体和特效实例化现有 Prefab。开发入口和资源同步步骤见 [开发使用说明](Reference/开发使用说明.md)。

## 最新验收

当前只在 Unity Game 窗口验证，保持编辑器后台运行。

- [暴击反馈与Boss演出专项](Reference/Validation/critical-hit-batch.json)：恢复原暴击覆盖特效、五周期与重复触发、指定Boss调用点及Boss致死普通命中声提前返回；真实Update及4,486点原图混合检查通过。固定引用已保存，见[前中后画面](Reference/Validation/critical-hit-contact.png)。

- [人物对话专项](Reference/Validation/character-dialogue-batch.json)：x27两行分页、颜色／描边／阴影、六个头像映射、空名字显式头像、空文本、VM等待与状态清理通过；268×90原版对话框资产已保存。5张后台GameView截图覆盖右／左／无头像、显式头像和空文本。原设备字体栅格、真实输入与逐帧时序仍待验收。

- [四类脚本奖励提示](Reference/Validation/reward-prompts-batch.json)：银两、道具、装备与经验恢复原消息文案和前景色，保留经验提示不主动暂停脚本的规则；奖励专项、生产VM领奖、19种支线与存档回归及消息回归共50组通过。见[四类画面](Reference/Validation/reward-messages-contact.png)。其他菜单／物品反馈、字体描边及原设备对照仍待验收。
- [消息队列与原继续提示](Reference/Validation/message-queue-batch.json)：恢复消息逐条推进、原追加计时语义及队列结束后VM恢复；原jiantou提示使用保存的图集／动作和全局80ms时钟。9组队列、9组消息回归、7组像素检查通过；1,848个原提示采样点完全一致，见[实际画面](Reference/Validation/message-hint-0.png)。奖励提示调用点、字体／描边和原设备逐帧对照仍待验收。
- [选择框绘制与文本滚动](Reference/Validation/choice-render-batch.json)：恢复原边框、120px列表、选中底色、滚动条及文字阴影；完整说明与80ms滚动已有专项。5组选择、4组像素及9组消息回归通过。60,772个非文字采样点最大RGB差为1，见[无说明画面](Reference/Validation/choice-render-empty-info.png)、[长说明画面](Reference/Validation/choice-render-info.png)。原设备字体、说明描边和真实输入仍待验收。
- [消息边框与彩色文字](Reference/Validation/message-panel-batch.json)：使用原u_40图集恢复平铺边框与填充，恢复原脚本前景颜色；6组逻辑／资产、9组消息机制回归与3组像素检查通过。18,587个边框采样点一致，见[短消息](Reference/Validation/message-panel-short.png)、[长消息](Reference/Validation/message-panel-tall.png)。文字背景色／描边、字体及原提示动画仍待还原。
- [type11敌人光环](Reference/Validation/enemy-aura-batch.json)：补齐10种精英／鬼系敌人的原单帧光环，保存嵌套节点和4个图集裁切。4份定向报告29组通过，实际验证下层角色遮挡、绝杀隐藏和HUD隐藏时保留，见[GameView对照](Reference/Validation/enemy-aura-contact.png)。后续按[模块差异清单](Reference/模块差异清单.md)推进。
- [普通敌人受击血条](Reference/Validation/enemy-health-batch.json)：恢复type9／11受击状态的小血条，20档Sprite与39个角色的血条节点编辑期保存；生产命中、自然回待机、整数截断、镜像与隐藏规则已验。画面见[血量对照](Reference/Validation/enemy-health-contact.png)、[原桥栏遮挡](Reference/Validation/enemy-health-map-contact.png)。
- [角色整体排序与战斗反馈遮挡](Reference/Validation/world-presentation-batch.json)：本批21份报告、165组通过；按原脚底高度与地图层排序，数字和闪避跟随角色参与遮挡，分组与节点编辑期保存。实际GameView验证前方敌人与原版桥栏遮挡，见[角色前后对照](Reference/Validation/world-presentation-contact.png)、[桥栏对照](Reference/Validation/world-map-contact.png)。
- [普通武器破剑与敌方耐久攻击](Reference/Validation/sword-break-batch.json)：本批16份报告135组通过，恢复原四帧、第五周期清除暂停，接入技能／崩击／葬剑／脚本／Boss破坏，补齐天草三成耐久攻击和整包武器付费修理。真实后台暂停／恢复、30种天草命中情形及存档已验；画面见[六阶段对照](Reference/Validation/sword-break-contact.png)。
- [战斗数字、闪避与气血尾条](Reference/Validation/combat-feedback-batch.json)：15份报告128组通过，恢复原四种数字图集、每角色五条／十周期上浮、闪避原图和特定Boss白色尾条；封顶恢复仍显示原计算值。包含新源码连续主线及真实脚本重载。画面见[反馈对照](Reference/Validation/combat-feedback-contact.png)。
- [伤害结算架构优化](Reference/Validation/damage-calculation-batch.json)：本批13份定向报告133组通过。`CombatDamageCalculator`集中主角／敌人基础伤害、浮动、暴击与闪避概率；普通命中、敌方弹体、葬剑和Boss基础伤害复用同一入口，保留发弹时快照、随机抽样顺序、整数边界和Boss家族特殊规则。弹体真实后台Update双发命中、Boss自主行为与242捕获连击均已验证；技能施放、资源消耗、HP事件和表现仍由主控协调。
- [场景名演出](Reference/Validation/stage-info-batch.json)：本批7份报告28组通过，恢复横线滑入、文字逐像素上移、停留及消失；菜单／对话期间由真实后台Update验证继续播放。画面见[四阶段对照](Reference/Validation/stage-info-contact.png)。
- [HUD命中与冷却反馈](Reference/Validation/hud-feedback-batch.json)：9份报告67组通过，恢复原版三档连击图、AP数字、17种技能图标／冷却遮罩与末两帧白闪、满EP首次六帧动画；修正小地图按键显示与光效越出黑边。后台GameView画面见[连击与冷却](Reference/Validation/hud-feedback-contact.png)、[满EP六帧](Reference/Validation/hud-ep-fill-contact.png)。
- [触屏与区域小地图](Reference/Validation/touch-map.json)：方向盘5帧、按钮普通／按下纹理、原图尺寸与中心、空引用修复及自定义布局保留7组通过；小地图保存13区域／174房间及出口与标记。本批9份报告共64组检查通过，见[汇总](Reference/Validation/touch-map-batch.json)。画面见[普通状态](Reference/Validation/touch-normal.png)、[四向高亮](Reference/Validation/touch-directions-contact.png)、[区域小地图](Reference/Validation/mini-map-qixia.png)。
- [落地与拾取](Reference/Validation/ground-drops.json)：52个专用Prefab与固定Sprite已保存；道具／银两弹起、吸附、过期，武器落地／接触拾取，多件九宫格和存档清理6组通过；生产死亡至真实Update拾取、完整主线及相关回归合计50组通过。[本批汇总](Reference/Validation/ground-drops-batch.json)。
- [击杀奖励数值](Reference/Validation/rewards.json)：6组通过，补齐经验随机浮动、训练倍率整数次序、银两随机边界与掉落先于经验的结算顺序；落地和拾取当前见下方掉落专项。
- [擂台与剑冢五连战](Reference/Validation/training-batch.json)：原长老交互、收费、300秒擂台、五Boss顺序、随机奖励与中途存档5组通过；真实Update连续三次死亡／隐藏／复活及倒计时4组通过，战斗行为9组和编辑存档5组回归通过。训练不再泄漏至普通地图，无限复活不扣成负次数。采用流程fixture，正常难度五连战仍待验。
- [剑灵分段吸收](Reference/Validation/spirit-absorption.json)：80ms逐周期经验、跨级边界、自然／快速完成、吸收途中真实存档恢复、各档表情反馈共6组通过；相关剑灵、编辑存档和道具交易共23组回归通过。GameView已核对吸收、升级闪光和结束反馈画面，原数字、增量箭头和技能图集已接入。

- [新档至结局连续主线](Reference/Validation/full-journey.json)：一次新游戏经原 NPC 交互、264 次注册出口、20 次区域选择、1,358 页对话，完成原版结局并回到标题，中途真实 Save/Load。原商店与剑灵菜单教程均通过实际选择和返回完成。
- [自主 AI 实战](Reference/Validation/live-combat.json)：保留敌人原始属性与 AI，通过生产移动、普攻和技能操作击败四种普通敌人，并与两个 Boss 完成有实际伤害的限时交战。
- [编辑与存档](Reference/Validation/editable-workflow.json)：碰撞编辑／导入／撤销、成长表修改生效、脚本计时器小数秒与暂停状态、剑灵恢复节拍存档、吸收演出及35组原菜单动作引用。
- [95%阶段汇总](Reference/Validation/milestone-95.json)：本轮回归与独立应用结果、工作量分项、源码校验和及验收限制。

连续主线验收锁定敌人 AI，任务靶子气血设为1，Boss 使用生产伤害／死亡回调，主角护血并预置交互位置；没有直接写后续剧情事件或排入后续脚本。它证明剧情连接，正常难度全程战斗与步行碰撞仍需验收。实战样本也不代表全部 Boss 已正常击败。

## 已恢复的数据

| 内容 | 数量 | 位置 |
| --- | ---: | --- |
| APK 提取文件 | 2,323 | `Reference/Extracted` |
| 原版图片 | 392 | `UnityProject/Assets/Resources/Original` |
| 地图 | 52 | `Original/Maps`，含三层预合成 PNG 与碰撞 JSON |
| 动作文件 | 136 | `Original/Animations`，含拼帧、攻击框、受击框和逐帧移动 |
| 角色与场景物件定义 | 137 | `OriginalDatabase.asset` |
| 场景脚本入口记录 | 209 | 数据库 stages，与 52 张地图存在复用 |
| 剧情脚本 | 1,533 | 768 份主目录脚本 + 765 份嵌套目录版本 |
| 装备 / 道具 / 技能 / 敌人定义 | 30 / 24 / 26 / 38 | `OriginalDatabase.asset` |
| 原版 MIDI | 6 | `Original/Audio/*.mid.bytes`，另有离线合成 MP3 |

脚本保留完整指令记录及字符串内换行，共 51,282 条。嵌套目录的 765 份中有 23 份与主目录不同，均保留；运行时默认使用主目录版本。

## 架构优化状态

背包与道具交易已抽成独立C#模块，库存、堆叠、阶梯价格和成交规则由模块集中维护，剧情／菜单／掉落／存档复用同一入口。7份定向报告共59组通过，见[本批汇总](Reference/Validation/inventory-trading-batch.json)。装备列表、穿戴身份、结构移除保护、强化入口和独立快照已接入`EquipmentInventory`，18份报告177组通过，见[装备批次](Reference/Validation/equipment-ownership-batch.json)。`WeaponDurability`进一步收拢耐久映射、修理和合体门槛，移除活动耐久副本，修正合体读档对普通剑耐久的覆盖；19份报告165组通过，见[耐久批次](Reference/Validation/durability-integration-batch.json)。属性公式已集中到`CombatAttributeCalculator`，15份报告113组通过，见[属性批次](Reference/Validation/attribute-calculation-batch.json)。伤害数值结算已集中到`CombatDamageCalculator`，13份报告133组通过，见[伤害批次](Reference/Validation/damage-calculation-batch.json)。实例字段、属性刷新协调、技能资源、金钱和跨系统表现仍需继续梳理；开发入口见[开发使用说明](Reference/开发使用说明.md)，现状和清单见[工程优化与架构评估](Reference/工程优化与架构评估.md)。

## 如何编辑

- `Assets/Resources/OriginalDatabase.asset`：直接在 Inspector 编辑角色定义、装备、技能、成长、场景索引等。运行时优先读取这份 Unity 原生数据库。
- Unity 顶部 **古剑 → 原版资源浏览器**：浏览地图层与角色图集，定位对应运行时剧情资产。
- **古剑 → 内容 → 内容工作台**：按名称或编号查找地图、剧情和精灵，定位碰撞、固定布局、角色Prefab与图集。运行时剧情优先读取 `Assets/Resources/Stories/*.asset`；在Inspector编辑剧情步骤，保存后重新开局或载入相关剧情。原 `Original/Scripts/*.json` 和 `Original/script/*.txt` 保留作导入与核对。
- `Assets/Scripts`：可继续开发的 C# 运行时，包含原版脚本解释器、拼帧角色、战斗公式、任务回调、装备实例、菜单和存档。
- `Assets/Resources/Baked/Stages/*.prefab`：209 个可在 Prefab Mode / Scene 视图编辑的场景布局。双击 `200.prefab` 可编辑剑冢入口；固定物件的 Transform、朝向和 layer 使用布局里的值，原脚本的初始坐标不会覆盖手改位置。条件出现的人物、敌人仍由剧情创建。
- `Assets/Resources/Baked/Maps/*.prefab`：52 个地图，三层 SpriteRenderer 已保存，可在编辑模式调整。碰撞数据位于 `OriginalAssetCatalog.asset` 的 maps/data；可通过“古剑 → 地图碰撞编辑器”直接绘制、撤销并导入／导出碰撞；仍使用碰撞格数据。
- `Assets/Resources/Baked/Actors`、`Effects`：137 个角色和 119 个特效 / 动作 Prefab，拼帧节点已预建；`SpriteLibrary.asset` 保存 3321 个裁切 / 整图 Sprite。运行时加载预制资源，播放动作时切换现有 Sprite，不创建 Sprite 或拼帧节点。
- `Assets/Resources/Baked/Drops`：银两、24种道具、27张武器图集对应的52个掉落Prefab；节点、裁切Sprite及武器18帧落地动作均编辑期保存。
- 启动场景和游戏 Prefab 已保存相机、Map / Actors 容器、音乐与三个原版音效 AudioSource。动态角色、弹体和特效按游戏状态实例化对应 Prefab。
- `Assets/Resources/OriginalTouchLayout.asset`：11 个触屏／鼠标操作区的矩形、标签、透明度、原方向盘与普通／按下纹理，可直接在 Inspector 调整。键盘可反向取消闪避／普攻为后撤，支持普通与合体动作。
- `Assets/Resources/OriginalUiLayout.asset`：原版 HUD、连击三档与数字、技能快捷图集、35组菜单动作、8张菜单图集、世界地图、立绘映射及界面位置，均预存为资产；运行时直接绘制原图集。
- Unity 顶部 **古剑 → 剧情调试器**：选择场景、初始事件、主角／剑灵等级，播放模式中加载入口、查看／修改 VM 变量、执行后续脚本。预览使用独立存档 `gujian-story-preview.json`，可直接定位场景布局 Prefab 与剧情资产。新增剧情及架构优化状态见[工程优化与架构评估](Reference/工程优化与架构评估.md)。
- `Reference/Decompiled/sources/com/dobetter`：原版反编译 Java，作为进一步移植的依据。

`古剑 → 从原版 JSON 导入数据库` 会覆盖数据库中的手工数值修改。恢复工具也会覆盖对应生成资源，运行前先保留自己的修改。

`古剑 → 生成缺少的预制资源（保留已有编辑）` 在编辑模式补充缺失资源，保留已有地图、角色、特效和场景布局 Prefab。原版源数据改动不会自动重写已编辑的 Prefab；需要同步时应先备份，再明确修改相应资源。检查入口为“验证预制资源与固定布局”。

## 剩余工作

- 全部 Boss 和全程正常难度平衡、其他可选路径回归；擂台及剑冢五连战已覆盖流程与刷新专项。
- 原版逐帧演出与视觉对照、全部音效时序；剑灵分段吸收及各档表情反馈已完成数值专项，画面对照仍待验收。
- Android 构建与真机输入、字体、性能验证；当前 Unity 未安装 Android Build Support。
- 未引用的异常动作文件、`event.tbd` 用途及 MIDI 的系统 GM 音色差异，详见进度文档。

## 资源恢复工具

- `Tools/recover_assets.py`：安全解包、gzip 和脚本记录恢复；仅标准库。
- `Tools/import_original.py`：PNG、地图、动作和数据表解析，需要 Pillow。
- `Tools/render_midi.swift`：macOS 系统 General MIDI 离线渲染；随后可用 FFmpeg 编码 MP3。最终 Unity 音频不依赖 macOS。
- `Tools/audit_scripts.py`：统计主目录脚本指令使用情况；“有分发分支”不等于完整实现。

原始文件及 SHA256 清单保留在 `Reference/inventory.json`；解析工具未运行原始 APK。
