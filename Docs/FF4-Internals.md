# FF4 internals, read from the binary

What the FF4 code does, as read out of `libff4.so` (FF4 3D v2.0.4, Android arm64 - the phone
build, the complete game with every animation; the Steam build is the same code with a
different window and the phone UI drawn into a 1136 x 640 space). One place for it, by
subsystem, with pseudo-code where a routine was read instruction by instruction, so the next
session does not have to read it again. The disassembly itself is on disk under
`Reference/libff4/` (`Tools/ff4_dump_all.py`; `INDEX.md` there says what is where); the
single-function tools are `Tools/ff4_disasm.py <so> <symbol substring>` (`--max=` caps the
instructions printed, default 400), `Tools/ff4_callers.py` (who calls what, through the PLT
too) and `Tools/ff4_strings.py` (the string at an `= 0x...` address). Data symbols
(`CAMERA_BATTLE_POSITION` and the like) are read with pyelftools straight off the segments;
a `ldr xN, [xM, #off]` after an `adrp` is a GOT slot - the relocation at that address names
the symbol. Where the client implements a piece, the file is named.

Conventions of the code base: fixed point `fx32` = value x 4096 (`VecFx32` is three of
them); angles as 16-bit indices (65536 = 360 degrees, `FX_SinIdx`); the NNS 3D library of
the DS underneath (row-vector matrices, `NNS_G3dGlbPerspective(fovySin, fovyCos, aspect,
near, far)` takes the sine and cosine of HALF the vertical angle); files by bare name
through `ds::CFile` (`g_File`), archive members included; the C++ namespaces are `btl`
(battle), `world`/`wld` (field), `menu`, `ui`, `layout`, `mgs` (menu graphics), `mon`
(monsters), `itm` (items), `pl` (players), `evt` (events/scenes), `ds`/`sys3d`/`sys2d` (the
engine), `common`, `ys`.

## Battle

### The battle stage

FF4 fights on a stage of its own: `battle_map.dat` holds `b00..b30` (`.nmdp` model + `.namp`
animation; the `b_soto`/`b_naka`/`b_uti` entries are empty on Steam). The stage for a fight
comes from the map's land-form parameter (see MAPPARAMETER below): `world::battleMapID(land
form)` = `u16 at chain0 + 0x18 + 2 x landForm`, anything over 30 = none. b01 is the Baron
plain, b08 the Watery Pass. The b01 model carries its own backdrop: a plane ("sky_pl",
about 290 wide, 60 tall) along the far edge at z about -67 plus "sky"/"sora"/"kumo" nodes.
Client: `Compat/Ff4BattleStage.cs` (a map jump to bNN and back).

### The battle camera (`btl::CBattleDisplay`)

```
initialize():
    btlCamera.setFOV(641, 4046)        // sin/cos of 9 degrees -> an 18-degree vertical field of view
    btlCamera.setClip(10 * 4096, 2000 * 4096)
    ... scenes, characterMng, stageMng, entryStage()

readyOpeningCamera():
    camera.position = (0, 32.7, 166)   // immediates: x1 = 0x00020B00_00000000, w2 = 0x000A6000
    camera.target   = (0, 0, -34)      //             x1 = 0,                  w2 = 0xFFFDE000
    moveFrame = 5; openingStep = 0; flag = 0

goOpeningCamera():                     // each frame while moveFrame > 0
    type = monsterParty()->byte[3]     // the encounter group's camera type (0 for 515 of 520 groups)
    if moveFrame > 0:
        moveFrame--
        position += (CAMERA_BATTLE_POSITION[type] - position) * 0.2     // 0x66666667 >> 32, >> 1
        target   += (CAMERA_BATTLE_TARGET[type]   - target)   * 0.2
    else:
        position = CAMERA_BATTLE_POSITION[type]; target = CAMERA_BATTLE_TARGET[type]
        state = 4; flag = 1

setBattleCamera():                     // the standing shot, also used to reset
    type = monsterParty()->byte[3]
    camera.setPosition(CAMERA_BATTLE_POSITION[type]); camera.setTarget(CAMERA_BATTLE_TARGET[type])

CAMERA_BATTLE_POSITION = { (0, 45, 240), (0, 36, 117.6), (0, 45, 240), (0, 0, 0) }   // @0x1bc550
CAMERA_BATTLE_TARGET   = { (0, -5, -20), (0, -10, -44), (0, -3, -40), (0, 0, 0) }    // @0x1bc580
```

So the standing view is a long shot from z 240, 45 up, looking a little down the stage
towards -z at the backdrop; the monsters stand left (negative x), the party right. Client:
`Ff4BattleStage.CameraPosition/CameraTarget/OpeningPosition/CameraFov`, `MonsterParty.CameraType`.

`BTL_CAMERA.dat` (`s00_00.dsc` .. `s91_00.dsc`, CMS2 sets - the format under Scenes below) is
NOT this camera: those are the ability and summon camera motions played through
`ds::sys3d::CameraHandle` (`btl::AbilityInvokeCameraController`, `BattlePlayerBehavior::
setAbilityCamera`, `readyBossAppearCamera`). Their coordinates are small and relative to an
actor. `s00_00` motion 1 (127 frames, (0, 3, 28) up to (0, 17, 96), 43 degrees) is one of
them. Not played by the client yet.

### Where the party stands (`btl::BattleParameter`, `battle_parameter.chain`)

`battle_parameter.chain.lz` is a chain pack of 29 chains. Chain 0 is the party's roots:

```
partyRoot(id):                     // records of 164 bytes, u16 id first; walks up to three
    for rec in chain0 (stride 0xa4): if rec.u16[0] == id: return rec
BattlePartyPosition::position(formation, slot) = rec + 4 + formation * 0x50 + slot * 16
    -> { s32 x, s32 y, s32 z, s32 facing }  (fx32; facing in degrees)
BattlePlayer::rootPosition() = position(member's formation (front 0 / back 1), member's slot)
```

Record 0 (the normal fight): front row x 18, 17, 19, 17, 19; back row x 30, 33, 30, 29, 29;
z -25, -5, 12, 35, 50 (slot 0 at the top of the screen); facing -90 (towards -x, the
monsters). Record 1 (back attack): x 33, 20, 11, 0, -11, facing -140..-175 (turned). Record 2
(pincer): every slot (10, 0, 10) / (20, 0, 20). Client: `OpenFF.Data.PartyRoot`,
`Ff4Tables.ReadBattleParameter`, `Ff4BattleStage.PartySpot`. The other 28 chains are not
read yet.

### Encounter groups (`monster_party_table.bbd`)

520 records of 140 bytes (`mon::MonsterPartyManager::load` divides by 0x8C; `monsterParty(id)`
walks them comparing the s16 at 0). Layout: `s16 id @0`, `s16 flags @2` (byte 3 = the camera
type above), then up to six 20-byte slots from 4: `s16 monster id` (-1 ends the list), `s16
flag`, `fx32 x, y, z` (stage units already: x -8..-37 towards the monsters' side, z -35..32
along the line), `fx32 w` (66 for most, 80 for Goblins, 20 for a boss - not named). The
scripted battles are 900..934. Client: `Ff4Tables.ReadMonsterParties`.

### Formulas (`btl::NewMagicFormula`, `btl::NewAttackFormula`)

```
attack magic damage = power * casterLevel * stat / (targetWill + targetLevel + targetMdef) * rand(1.0..1.3)
    stat = will for white magic, wisdom otherwise; spread over n targets: x (90 - 10 n) %
healing            = (targetVitality / 8 + casterWill / 2) * power * (100 - rand(0..10)) %
MP cost            = byte 5 of the spell (halved by ability 0x51)
physical damage    = attack * level * strength / (defence + level + vitality) * rand(1.0..1.3)
                     x 1.2 when a player hits a monster, x 0.7 the other way
hit                = clamp(weaponHit + agility - (evade + targetAgility) + 20, 0, 100) %
```

Client: `Ff4Battle.Damage/Hits/AttackMagicDamage/HealingValue`. Statuses, criticals, rows
and elements are not applied yet.

### Battle motions

Party battle models are `pNN_00` (`pNN_01` walks the field; the two have different joints -
binding a battle motion set onto the field model crashes the joint animation); their motion
sets `b_p_player_NN` (ids 2007 idle, 2008 attack, 2009 hurt, 2010 victory). Monsters:
`mNNN_00` models, `b_mNNN` sets (101 idle, 201 attack, 202 special). `PLAYER_TYPE` order:
0 Cecil DK, 1 Cecil Paladin, 2 Kain, 3 Rosa, 4 Rydia child, 5 Rydia adult, 6 Tellah, 7 Porom,
8 Palom, 9 Yang, 10 Cid, 11 Edward, 12 Edge, 13 FuSoYa, 14 Golbez.

### The battle UI (`btl::TouchWindow`, `BattleCommandWindow`, `ui::CWidgetMng`)

The battle windows have no layout file; they are built in code. `ui::CWidgetMng::addWidget
(id, x, y, w, h, type, ...)` keeps a list of widgets and, for type 3, constructs a
`menu::BasicWindow` (the same class that draws the dialogue window). `btl::TouchWindow::
createTouchWindow` adds the widgets (an `addWidget(..., 0x100 ..., 3)` and a second), the
command list is `BattleCommandWindow::create(BattlePlayer*)`. Constants seen: `btl::
COMMAND_WINDOW_POSITION = 1968` (probably y x 16 = 123 of 192), `SELECT_WINDOW_POSITION =
160`, `BtlMagicMenu::BMTEXT_POS` = three columns x 24, 98, 172 by rows y 18, 28, 38, 48 (the
magic grid is three columns wide). The one caller of `BasicBattleWindow::create(x, y, w, h, kind)` is `BattleStatus2DManager::
setStatusWindow(BATTLE_WINDOW_TYPE, char, int, int)`, whose numbers come out of
`Battle2DManager::setIPadSize(NNSG2dSVec2, int)` - the layout adapts to the device's screen,
so there is no fixed rectangle to read; `create` then adds five row widgets (ids 0x2f..0x33,
y +2, +21, +39, +57, +75, heights 19/18/18/18/19 in DS units - 18 units = the 106-px rows of
the 1122-px Steam window, so the widget space is the DS's 192-unit height, 341 wide at 16:9,
and the sheets are drawn at a third of a unit per pixel). The client's layout
(`Ff4Battle.Draw`) is measured from the Steam screenshots, which is the same code's answer
for a 16:9 screen.

Art (`MENU_Common.dat`, `battle2d_Common.dat`, `battle2d.dat`; the phone build's "NCGR" and
"NCBR" are PNG sheets, "NCER" the port's cell banks - see 2D below): `window_frame_00..05`
(two frame styles per sheet, eight cells each in the bank: TL corner, left edge, BL, top
edge, TR, right edge, BR, bottom edge; 16-px corners, 64-px edges; style 0 a white line,
style 1 the blue bevel), `cursor` (two 48 x 48 gloves at (-48, -12) from the spot: cell 0
pointing, cell 1 pressed; `cursor.NANR` sequences 0/1 hold cell 0, 2 = cell 1), `gauge_atb`
(cell 0 the 80 x 12 trough at (-4, -6); cells 1..3 72 x 6 fills - grey, yellow, red - at
(0, -3)), `battle_number` (digit strip, "Hit!!", "CRITICAL!", "MISS!", "WEAKNESS",
"AUTO-BATTLE"), `battle_icon`, `button_battle_00..05`, `icon_16dot` (arrows), `winsample`
(a 256 x 97 navy gradient; `winsample.NSCR` tiles it across 2048 px). `BasicWindow::bwAlloc`
fills its windows in code; the client tints a white texel 0x4A2214 (blue-green-red) for it.
`np00..np11.NCBR` are the intro name plates ("Cecil / Lord Captain / Baron Red Wings").
Client: `Compat/Ff4Ui.cs`.

## The field

### Map parameters (`MAPPARAMETER.dat`, `<map>.pak`)

A chain pack of four chains per map, kept in this order by `world::MapParameterManager::load`:

```
chain 0  landFormParameter   50 bytes: u16 encounterRate[12] @0 (0 = none; 11 Watery Pass,
                             1..9 by terrain on an overworld chip), u16 battleStage[12] @0x18
                             (> 30 = none)
chain 1  monsterPartyParameter  8-byte entries of four u16 encounter group ids (0xFFFF none);
                             the party's land form names the entry, a group is rolled among
                             its four, re-rolled up to five times when it repeats the last
chain 2  encountParameter    16 bytes: s16 for world::attackType (back attacks etc.) + 3 floats
chain 3  (8 bytes, unnamed)
```

`WSEncountSetting::wsProcess` reads the rate at `chain0 + 2 x landForm`. The overworld's
`f00.pak` holds 256 chip records of 192 bytes, each its own four-chain pack; the chip under
the party (`stageMng.getChipName()`, "f00_48" -> 0x48) picks it. The land form under the
party (`PCObject + 0x340/0x348`, from the ground polygon's attribute) is not read by the
client yet - it uses the first land form with a rate. Client: `Compat/Ff4Encounters.cs`.

### The field camera (`WSPrepare::wsProcessSetupCamera`, `world::WorldCamera::initialize_usr`)

The camera stands at the leader + positionOffset and looks at the leader + targetOffset, by
the map's kind letter: 'f' (field) (0, 100, 110) / (0, 30, 30); 'd' (dungeon) (0, 80, 105) /
(0, 27, 25); everything else (towns) (0, 90, 85) / (0, 17, 5) (tables at 0x1bc944 and
0x1bc95c; the client negates z for FF3's axes). The field's field of view is about 30
degrees (`setFOV(1060, 3956)` in the port's `setupCamera`). The overworld enables fog
(`WSPrepare`: range 0x80000..0x200000, colour 0x73f5); the clear colour is black
(`ContEventPart::initialize`) - skies are geometry. Scene cameras: `world::EventCamera`
(position and target each moved linearly over N frames: `setPositionLinerMove`,
`setTargetLinerMove`; `setCameraOffset` = follow the leader at fixed offsets,
`CUFollowCamera::set`). Client: `Compat/Ff4EventCamera.cs`, `wld.CBaseSystem.setupCamera`.

### Scene messages

`babilCommand_CE_ShowMessageWindow(engine)`: `byte b = getByte(); if (conte->flag[0x5fb]) return;
EventConteManager::enableMessageWindow(b != 0)` - the scene's message bar (a frameless, dark,
translucent BasicWindow across the bottom) is on by default and the scripts turn it off between
lines with `ce_ShowMessageWindow(0)`; `startMessage(pos, id, 2, 0)` in a scene shows the text
in it (type 2 = no wait for a press; the voice line runs alongside, `ce_StartVoice`). Field
talks use the framed window with the speaker's name tag. Client: `menu.BasicWindow.SetBarStyle`,
`menu.MessageWindow.mwSetBarWindow`, `Ff4Cutscene.ShowMessageWindow`.

### Scene camera motions (`EVT_CAMERA.dat`, `.dsc` = CMS2)

```
set    "CMS2" | u32 version 0x30000 | u16 0x2c | char name[..] | @0x28 u32 count | @0x2c count x { u32 id, u32 offset }
motion "CM4\0" | @4 u32 loop count (0 = forever) | @8 u32 start wait | @0xc u32 loop wait | @0x10 u32 frames | @0x14 u32 channelOffset[8]
channel u16 keys | u16 type | keys...
    type 0 (u8 frames, s8 delta)  type 1 (u16, s16)  type 2 (u32, s32)  type 3 float per frame  type 4 one s32 constant
    a delta key adds its delta once per frame for its frames; the value starts at 0
channels: quaternion x, y, z, w (fx12); position x, y, z (fx32); field of view as a 16-bit angle index
CameraHandle::calculatePosition: M = rotation(q) x translation; camera at the translation,
    looks along (0,0,-1) x R, up (0,1,0) x R (row vectors)
```

**The FOV channel is never applied.** `calculatePosition` sets the FOV from channel 7 only when
the handle's byte at +0x81 is set, and nothing in the game sets it - the constructor and `clear()`
zero it, and no other store to that offset exists (the one at +0x141 is `mgs::vs::EffectViewer`,
a debug viewer). A scene's FOV is `evt::EventCamera::initializeDefaultParameter`'s
`setFOV(0x424, 0xf74)` - a 15-degree half angle, 30 degrees vertical, set in
`ContEventPart::initialize`, so once per story map - and `eventCameraSetFovyMove(degrees, frames)`
from the script (`EventCamera::update` runs the fovy move, then `CameraHandle::nextMotion(1)`,
then `CCamera::execute`). The scripts rely on it: e01_00's `eventCameraSetFovyMove(30, 0)` follows
shot 103 whose channel says 43 degrees, and eight of its nineteen shots set nothing and keep the
previous value (all 627 shipped calls checked: the channel constants disagree with the script on
most shots). The default's other numbers: `setDistance(0x400000)`, `setClip(0x2000, 0x800000)`
(2..2048), position (65, 25, -5) looking at (0, 10, 0) before the first motion.

**`ce_PlayCameraMotion(slot, id, blend, loop)`** = `EventCamera::startCameraMotion` =
`CameraHandle::start(slot, id, loop, blend)` + `play()`. `start` calls `saveOldPosition` (the
displayed pose: it lerps the old pose and the current channel values by the running blend ratio
first, so a shot started mid-slide does not jump), then `blendFrames = wasActive ? blend : 0`
(flag bit 0 at +0xd0 = a motion was up), `blendTick = 0`, and steps the decoders once (the frame
shown first is frame 1). `nextMotion(steps)`: nothing advances during the start wait (+8 frames
since start); otherwise per step `frame++` and `blendTick++` together; at `frames` the loop
counter compares with +4 (0 = forever) or bit 4 marks the end (`isEndOfMotion` = flags & 0x18).
`calculatePosition`: `t = FX_Div(blendTick, blendFrames)` or 1.0 when zero, position and FOV
lerped, rotation through `ds::Quaternion::leap` (a plain component-wise fx12 lerp, no sign flip,
no normalise), then `MTX_Concat43` with the handle's reference transform (+0x20; identity for the
event camera, `setupCameraMotionSet` sets it so - the summon cameras will use it). No shipped
script passes a blend or a loop (627 calls, all `0, 0`), and every e01_00 motion has zero at +4,
+8 and +0xc; the client logs those and does not slide.

Client: `Compat/Ff4CameraMotion.cs` (plays them on the field camera; `Ff4Cutscene` calls
`Setup(slot, name)` / `Play(slot, id, blend, loop)` / `SceneStarted()` for the default FOV at
`ce_StartEvent`). `--ff4cam=off` skips the motions.

### Scene shadows (`CE_ShadowSetting`, `CE_ShadowVisiblity`)

A scene cast has no shadow until the script gives it one. `babilCommand_CE_ShadowSetting(slot,
type, joint, x, y, z, scale)` (byte, byte, string, dword x4) calls, on the cast's character index,
`CCharacterMng::setShadowEnable(true)`, `setShadowJntName(joint, true)`, `setPolygonID(0x3f)`,
`setShadowType(type)`, `setShadowOffsetEnable(true)` and the offset and scale;
`CE_ShadowVisiblity(slot, on)` (byte, dword) is `setShadowEnable(on != 0)`. Every shipped setting
is `(0, "kosi", 0, 0, 0, 4096)` but one at scale 0xA000; 182 visibility calls turn on, 27 off;
e01_00 gives shadows to Cecil and the four soldiers (slots 0..4) and to nobody else (slot 20,
`n215_01`, is two more soldiers in one model - `polygon0_baron3`, `polygon1_baron4`, exactly twice
`n024_00_01`'s 510 vertices). `ds::sys3d::CShadowObject::drawShadowPolygon`: the position is the
render object's; when the render object names a joint (flag +0x1c0, name at +0x1c2) and is not
clipped, x and z come from that joint's stored matrix (the render object keeps twelve 0x48-byte
joint slots at +0x238), plus the offset when enabled; y is then `ground + height + 0x40` when the
shadow has a ground interface (+0x18, a virtual call) and `height + 0x29` - absolute, a hair over
the floor - when not. No event map ships an `.mcl`, so the scenes take the second path: the casts
are moved by their motions' root joint, the object position stays at the origin, and only the
joint keeps the disc under the body. `CE_AddShadowVolume(slot, name)` / `CE_ShadowVolumeONOFF(slot,
n)` appear in e02_00 only. Client: `CCharacterMng.setShadowVisible` / `setShadowJntName` (PORT),
`CShadowObject.setJointName` + `GroundQuery`, `Ff4Cutscene.ShadowSetting` / `ShadowVisibility`; the
FF3 path is untouched (`setupCharacter` gives every model a polygon shadow, as it always did).

### Scene bind objects, scale, animations

`babilCommand_CE_SetBindObject(model, slot, joint, x, y, z, rx, ry, rz)` (string, byte, string, dword
x6): `CCharacterMng::setCharacterWithTexture(model, model, 1)`, `setShadowType(idx, 2)` (no shadow),
`setShadowEnable(idx, false)`, `setViewVolumeClip(idx, ...)`, then `evt::EventConteManager::
setBindObject(hostIdx, boundIdx, VecFx32 pos, VecFx32 rot, joint)`. `CE_SetBindObject2(slot, host
slot, joint, ...)` (byte, byte, string, dword x6) resolves both casts and calls the same with -1 for
a model. `CE_BindObjectVisiblity(slot, on)` (byte, dword). 25, 9 and 2 uses; the intro's is
`("p02_01b", 4, "L_wepon", 0, 0, 0, 0, 0, 0)`, a soldier's spear. `CE_setScale(slot, x, y, z)` (byte,
dword x3) is fx12 - `(7, 8192, 8192, 0xFFFFE000)` doubles and mirrors the hall's `o032`.
`CE_PauseAnimation(slot, type, on)` and `CE_StartAnimation(slot, index, type, ?)` work the model's
own animation set by kind (`CAnimSet.enTYPE`: 0 texture SRT, 1 texture pattern, 2 material, 3
visibility); the last operand of `StartAnimation` is not read yet. Client: `Ff4Cutscene`
`SetBindObject` / `SetBindObject2` / `BindObjectVisibility` / `SetScale` / `PauseAnimation` /
`StartAnimation`.

## Data files

### Chain packs (`.pak`, `.chaindata`, `.chain`)

`u32 count` at 0, then from byte 16 one `(u32 offset, u32 size)` pair per chain, offsets from
the file start; a chain's records follow each other at a stride the reader knows. Used by
`item_parameter.pak` (4 chains: consumables 48 bytes, weapons 88, armour 84, key items 32),
`player.chaindata`, `monster.chaindata`, `battle_parameter.chain`, a map's `.pak`. Client:
`Shared/Data/ChainPack.cs`.

### Players (`player.chaindata`)

Chains 17..31 are the learn lists per `PLAYER_TYPE` (u32 = ability << 16 | level; commands
are < 1500: 6 white, 5 black, 13 summon, 4 sing, 0x53 ninjutsu; every list ends with the
Bardsong augment songs, real only for type 11 Edward). Per-level rows give HP/MP and stats;
the save block per character keeps equipment at +0x24, abilities at +0x164, learning at
+0x184. Client: `Ff4Tables.ReadPlayers`, `Character.Learn`.

### Magic (`magic_parameter.bbd`, `efficacy.beld.lz`)

36-byte records, id first: `s16 id @0, s16 power @2, u8 school @4, u8 mp @5, u16 hit @6,
group @10, rank @12, element @22, inflicts @24, grants @26/@28, u8 target @32` (target bits:
0x01 hits all, 0x08 can spread, 0x10 usable in battle, 0x20 usable in the menu). Names:
`babil_ability.msd`, message id = ability id (spells 4501.., summons 1501..). `efficacy.beld`:
"BELD", section count byte @4, counts, offsets; section 1 12-byte (id, hp, mp): 10 Potion
100, 11 500, 12 1000, 13 Ether 50 MP, 15 Elixir 9999, 17 Phoenix Down; section 2 20-byte (id,
0, 0, ability, effect). Client: `Ff4Tables.ReadMagic/ReadEfficacies`.

### Monsters (`monster.chaindata`)

Chain 0: 252 records of 152 bytes - FF3's head (nameId, textId, familyId, modelId, monsterId
@8, level, size, maxHp s32 @0xC), five attribute bytes @0x12, attack @0x20, hit @0x22,
defence @0x4C, evade @0x50, magic defence @0x68 (tentative), four drop pairs (item, chance
of 4096) from 0x6C, experience s32 @0x88, gil @0x8C. Names only in `babil_battle.msd`.
Client: `Ff4Tables.ReadMonsters`.

### Shops (`babil_shop.bbd`)

124-byte rows: label[32], title message id, six line ids (`babil_menu.msd` 51220 Weaponsmith /
51221 Armorer / 51222 Sundries, lines 51260..51265), 16 item ids. `bootShop(row)` is the
absolute row. Steam Baron: t01_40 rows 1, 2; t01_60 rows 3, 40; the inn is t01_30 (50 gil);
row 0 is the debug shop. Client: `Compat/Ff4Shop.cs`.

## 2D and menus

### Sheets and cell banks

The phone and Steam builds keep the DS file names but not the formats: a `.NCGR`/`.NCBR` is
a PNG (8-bit palette with tRNS, or RGBA); a `.NCER` (and most `.NSCR`) is a cell bank whose
parts are seven plain s16 words: x, y (from the cell's origin), width, height, source x,
source y, flags (1 flip across, 2 flip down, 4 half size, 8 squash 0.6 x 2/3) - read straight
off the game's `drawImage(x, y, w, h, sx, sy, sw, sh)`. Blocks keep the NDS layout (16-byte
header, blocks tagged back to front: "KBEC"). Cell headers first (parts count, attributes, 8
bytes each), then every part list. `.NANR`: sequences of (cell, hold) frames. Client:
`Shared/Content/CellBanks.cs`; Crystal: `Crystal.Editor/Cells.cs`.

### Menu layouts (`MENU_LAYOUT.dat`, XBN)

34 `MenuLayout_*.xbn.lz` (Root, Item, ItemWnd, Magic, Equipment(+_Below), Status(+SubAbility),
Ability(+Sub), Formation, Config, Save(+Sub), Suspend(+Sub), Name, Title, ShopSpr, Sightro,
Chokobo, BackupErr, PartyPlaneAbove, MinigameSelectS, the CS*/Chk* extras). XBN is Square's
binary XML, the same the FF3 client reads (`XbnFile`/`XbnNode`): header `"XBN " u32 version
u32 nodeCount u32 reserved`, then per node five int32 - name offset (from the end of the
record table), data type (0 void, 1 string, 2 decimal), data (int, or a string offset),
child count, family size (the node and everything under it) - and the Shift-JIS strings
after. Schema: `layout > unit {name, display} > frame {group, id, x, y, width, height,
choices, link, top, behavior "FBText" | "FBTextSCC" {suspend, parameter = message id, ...},
parameter}`, nested frames relative to their parent, in the DS's 256 x 192 units. RootMenu:
a 72 x 136 frame at (20, 0) with eight 64 x 16 text rows every 16 px (messages 50002..).
Binary classes: `Layout::makeup(layout::Frame*, XbnNode*)`, `layout::Frame::setup`,
`FrameBehavior`, `FBText`, `FBTextSCC`, `FBSprite`, `FrameBehaviorFactory`. Tool:
`Tools/xbn_dump.py`. Not drawn by the client yet.

### The field menu (`world::WSMenu`, the `MenuSubState`s)

The menu FF4 opens on the field is `world::WSMenu` (41 functions; `wsmLoadData` pushes XBN
mass 0x18 = MENU_LAYOUT.dat's layouts, `wsmReleaseData` frees them) running one
`world::MenuSubState` at a time: `MSSRoot` (the command list), `MSSItem` (30), `MSSMagic`
(12), `MSSEquipment` (16), `MSSStatus` + `MSSStatusWindow`, `MSSAbility`, `MSSFormation`,
`MSSConfig`, `MSSSave`, `MSSLoad`, `MSSSuspend`, `MSSShop`, `MSSBackupErr`, `MSSSightro`;
the shared planes `MSSPartyStatusMainPlane`/`SubPlane` (the party rows), `MSSMenuWindow`,
`MSSMenuSlideWindow` (the windows that slide in), `MSSSaveDataPlane`, `MSSCurtain`; the 3D
character on the menu's left (`MSSCharacter`, `MSSCharacterShadow`, `MSSParameterCamera`,
`MSSModelDirection`, `MSSTouchRotate` - the model turns when dragged, `MSSCharaLoader2`,
`MSSMotionLoader`); `MSSBridge`/`MSSBridgeToRoot` (the transitions; `MSSBridge::mssCommand`
reads XBN nodes 26 times - it builds the command frames from the layouts). The command list
on the right is `world::CurrentCommandFrame` / `ChildCommandFrame` / `DecantCommandFrame` /
`AutoCommandFrame` (`init(int, void*)`, `regist`, `moveV(layout::Frame*, bool, int)` - the
vertical slide, `draw`, `erase`), each a `layout::Frame` from the XBN. Item use is
`world::DecantItemUse` (open/decide/cancel/close). `Layout::build(name, LayoutBehavior*)`
itself is called only by `MSSBackupErr`; the other screens take the frames from the pushed
mass through `Layout::makeup`. `layout::Frame::setup` (read in full) takes from a frame's
node: id, name, x, y, width, height (s16), the `choices` flag (bit 2 of the flags byte),
group (inherited from the parent when absent, 0xff at the root), and `behavior` through
`FrameBehaviorFactory::createFrameBehavior(name)`; x and y become ABSOLUTE by adding the
parent's, and `setPosition(x, y, propagate)` moves a frame and its children by the same
delta. So the layouts are absolute DS-unit rectangles; where the phone puts them on a
16:9 screen is the screens' own code (not read yet). `Reference/libff4/menu-layouts.txt`
lists every layout's frames with absolute positions.

The Steam build (`FF4.exe`, Qt + SDL2) draws these windows with two files of its own beside
the executable: `window.png` (598 x 288, the blue-violet gradient panel with a thin light
border seen in every Steam screenshot) and `point.png` (48 x 48, the glove); `menu.txt` is
its pause menu's texts in eight languages (Resume / Quit / Yes / No / skip-scene prompt),
`strings.dat` the launcher's Qt strings, `arial.ttf` and `lucon.ttf` its fonts. The
Steam-only additions (keyboard hints, auto-battle, the pause menu) are not in the phone
binary. Client: `Ff4Ui` uses window.png and point.png when the content is a Steam install.

### The phone UI's space

The Steam window (2000 x 1122 in the reference shots) is the phone's 1136 x 640 UI space: the glove
is 85 px tall there (48 x 640/... ), text about 40 px. The client draws at 800 x 480 and scales
every piece by 800/1136. Fonts: the phone build has `BABIL_SYMBOL.NFTR`; Steam draws text with
`arial.ttf` through SDL2_ttf.

## The Steam shell (FF4.exe), from the Babil Decompilation Project

A pointer to `D:\Git\Babil-Decompilation-Project` (GlitchedDeveloper on GitHub): an early
Ghidra decompilation of the Steam `FF4.exe` - eight files so far, the Win32/SDL/Steam shell
around the engine (ini, joystick, the pause screen with `menu.txt`'s texts, `TTF_RenderText`)
- and, more useful now, `Structure/src`: the engine's original source tree, 228 file paths
recovered from its debug strings (`Reference/libff4/source-tree.txt` keeps the list). The
paths name the modules the disassembly's namespaces come from: `system/ds/{device, sys2d,
sys3d, sound, movie, utility}` and `system/dgs` (the DS-era engine: file system, sprites,
models, camera motions, messages, `mcl` collision), `user/battle/...` (`battle_formula.cpp`,
`battle_parameter.cpp`, `battle_status/battle_status_2d_mng.cpp`, `command_select/`, `script/`),
`user/character/{player, monster, common, condition}`, `user/event/{cast/babil_commands*.cpp,
main/event_conte_manager.cpp, main/event_camera.cpp}`, `user/menu/{layout.cpp, xbn.cpp,
frame.cpp, behavior/fb_text.cpp, basic_window.cpp, message_window.cpp, map_name_window.cpp}`,
`user/world/state/user/menu/mss_*.cpp` (the field menu: root, item, magic, equipment,
config, save, shop, decant, chara_model, menu_camera, ability), `user/world/state/user/
world_state_*.cpp` (move, encount, mapjump, menu, field_event, tresure, vehicle...),
`user/world/param/map_parameter.cpp`, `user/world/misc/world_camera.cpp`, `user/part/
{battle_display.cpp, battle_part.cpp, main/world_part.cpp}`, `user/2d/u2d_pop_up.cpp`.

`Main::CalculateViewportDimensions` (read in full) settles the UI space: the game renders a
**480 x 320 logical screen** - the phone's - letterboxed to 4:3 when the window is narrower,
widened to the window's aspect when wider and capped at 21:9 (`RenderHeight = 320;
RenderWidth = ViewportWidth * 320 / WindowHeight`). So the legacy 2D plane of 480 x 320 (the
message window at (5, 233) size 470 x 84) IS the UI space, the phone's sheets are 2x (the
48-px glove is 24 logical pixels, 7.5 % of the height as in the screenshots), and in the
port's 800 x 480 a sheet pixel is 0.75 px (`Ff4Ui.Scale`). The pause screen (Resume / Quit,
Yes / No, the skip-scene prompt) is drawn by the shell with SDL over a frame grab, using
`window.png`, `point.png` and `button_on/off.png`.

## Not yet read

`btl::TouchWindow`/`BattleCommandWindow` rectangles; `BattleParameter` chains 1..28;
`world::attackType`; the land form under the party; the fog and light setup per map; the
`s05..s91` ability cameras' anchors; `mgs::vs` (472 symbols, the menu graphics); the Steam
build's own additions (keyboard hints, auto-battle) are not in this binary at all.
