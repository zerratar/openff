# libff4.so, disassembled

Source: `libff4.so` (FF4 3D v2.0.4, Android arm64, the phone build - the complete game).
Regenerate: `python Tools/ff4_dump_all.py <path to libff4.so>` (13781 functions, 15192 symbols).

- `symbols.txt` - every function and data symbol with address, size and demangled name.
- `strings.txt` - the C strings of .rodata/.data with addresses (what `= 0x...` notes point at).
- `asm/<namespace>.asm` - the code by top-level C++ namespace (not committed; regenerate). Each function
  starts with `== demangled @address (size) mangled`; `bl` targets are named, `adrp/add` pairs resolved
  to `= address`, `adrp/ldr` pairs to `[address]` (a GOT slot - see the relocations for the symbol).

What has been read out of it lives in `Docs/FF4-Internals.md`.

Namespaces:

- `AchievementCheckFuncs`
- `AchievementChecker`
- `AchievementContext`
- `AchievementObserver`
- `AchievementReporter`
- `AchievementResource`
- `BPDivide`
- `BPIronChopper`
- `BPSlantVanish`
- `BPTranslucence`
- `BattlePerformer`
- `CCastCommandTransit`
- `CCharacterMng`
- `CMotionDataMng`
- `CObjectDataMng`
- `CTextureDataMng`
- `CastInfo`
- `Category`
- `FBSpriteFactory`
- `FBTextFactory`
- `FBTextSCCFactory`
- `FlagManager`
- `Font`
- `Layout`
- `LayoutDebugMenu`
- `Logic`
- `LogicContext`
- `LogicManager`
- `MDEquipmentDebugMenu`
- `MMAbilityBox`
- `MPDataManager`
- `MSSEItemEquipableList`
- `MSSMenuStatusBGScroll`
- `MSSShopDebug`
- `ManNoManager`
- `MovieFileRegister`
- `NHMotionData`
- `NHObjectData`
- `NHTextureData`
- `Performer`
- `SPBlurRotate`
- `SPMosaicNarrow`
- `SQEX`
- `ScreenPerformance`
- `ScriptData`
- `ScriptEngine`
- `ScriptFunctionTable`
- `Sound`
- `SoundSystem`
- `StreamReader`
- `TexDivideLoader`
- `TexVramList`
- `ToonTable`
- `ValueManager`
- `WTEESubMenuDesert`
- `WorldBG`
- `WorldBGControl`
- `WorldBGEffect`
- `Xbn`
- `XbnNode`
- `_JNIEnv`
- `backup`
- `btl`
- `card`
- `cmplg`
- `common`
- `debug`
- `dgs`
- `ds`
- `eff`
- `egs`
- `eld`
- `et`
- `evt`
- `global`
- `itm`
- `layout`
- `map2d`
- `mcl`
- `menu`
- `mgs`
- `mon`
- `movie`
- `mr`
- `msg`
- `music`
- `naming`
- `object`
- `ovl`
- `pack`
- `part`
- `pl`
- `spr`
- `stg`
- `sys`
- `sys2d`
- `title`
- `u2d`
- `ui`
- `utl`
- `view_chr`
- `world`
- `ys`
