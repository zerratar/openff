// Generated from the decompiled parameter classes - see Docs/Tables.md.
//
// A .pak holds chains, each chain an array of fixed size records. These layouts come
// from each record type's parse(ArrayReader) - nested sub records followed, constant
// bound loops unrolled - so the field names are the game's own.
//
// Every layout with a known stride is checked against it: the game counts weapons in
// steps of 56 and monsters in steps of 100, so a layout that does not add up is
// rejected rather than shifting every record after the first.
//
// Do not edit by hand - run Tools/gen_records.py, which rebuilds it from the sources.

namespace FF3.ContentTool
{
	internal enum FieldType
	{
		U8,
		S8,
		U16,
		S16,
		U32,
		S32,
		F32
	}

	internal sealed class PakField
	{
		public readonly string Name;
		public readonly FieldType Type;

		/// <summary>Elements, or -1 for an array that fills the rest of the chain.</summary>
		public readonly int Count;

		public PakField(string name, FieldType type, int count)
		{
			Name = name;
			Type = type;
			Count = count;
		}
	}

	internal sealed class PakChain
	{
		public readonly string Family;
		public readonly int Index;
		public readonly string Label;
		public readonly string Source;

		/// <summary>Fixed bytes per record; with a filling field, the size of the head.</summary>
		public readonly int Stride;
		public readonly PakField[] Fields;

		public PakChain(string family, int index, string label, string source, int stride,
			PakField[] fields)
		{
			Family = family;
			Index = index;
			Label = label;
			Source = source;
			Stride = stride;
			Fields = fields;
		}
	}

	internal static class PakRecords
	{
		public static readonly PakChain[] Chains =
		{
			new PakChain("Item", 0, "consumables", "ConsumptionParameter", 44, new[]
			{
				new PakField("system", FieldType.U8, 1),
				new PakField("_pad0", FieldType.U8, 1),
				new PakField("itemId", FieldType.S16, 1),
				new PakField("nameId", FieldType.S16, 1),
				new PakField("captionId", FieldType.S16, 1),
				new PakField("graphId", FieldType.S16, 1),
				new PakField("strength", FieldType.U8, 1),
				new PakField("vitality", FieldType.U8, 1),
				new PakField("dexterity", FieldType.U8, 1),
				new PakField("intellect", FieldType.U8, 1),
				new PakField("mind", FieldType.U8, 1),
				new PakField("weight", FieldType.U8, 1),
				new PakField("useBattle", FieldType.U8, 1),
				new PakField("useField", FieldType.U8, 1),
				new PakField("allTarget", FieldType.U8, 1),
				new PakField("_pad1", FieldType.U8, 1),
				new PakField("useItemId", FieldType.S16, 1),
				new PakField("targetPossible", FieldType.S16, 1),
				new PakField("targetPosition", FieldType.S16, 1),
				new PakField("_pad2", FieldType.U8, 1),
				new PakField("_pad3", FieldType.U8, 1),
				new PakField("buy", FieldType.S32, 1),
				new PakField("price", FieldType.S32, 1),
				new PakField("usedPower", FieldType.S16, 1),
				new PakField("itemType", FieldType.S16, 1),
				new PakField("changeCondition", FieldType.S16, 1),
				new PakField("_pad0", FieldType.U8, 1),
				new PakField("_pad1", FieldType.U8, 1),
			}),
			new PakChain("Item", 1, "weapons", "WeaponParameter", 56, new[]
			{
				new PakField("system", FieldType.U8, 1),
				new PakField("_pad0", FieldType.U8, 1),
				new PakField("itemId", FieldType.S16, 1),
				new PakField("nameId", FieldType.S16, 1),
				new PakField("captionId", FieldType.S16, 1),
				new PakField("graphId", FieldType.S16, 1),
				new PakField("strength", FieldType.U8, 1),
				new PakField("vitality", FieldType.U8, 1),
				new PakField("dexterity", FieldType.U8, 1),
				new PakField("intellect", FieldType.U8, 1),
				new PakField("mind", FieldType.U8, 1),
				new PakField("weight", FieldType.U8, 1),
				new PakField("useBattle", FieldType.U8, 1),
				new PakField("useField", FieldType.U8, 1),
				new PakField("allTarget", FieldType.U8, 1),
				new PakField("_pad1", FieldType.U8, 1),
				new PakField("useItemId", FieldType.S16, 1),
				new PakField("targetPossible", FieldType.S16, 1),
				new PakField("targetPosition", FieldType.S16, 1),
				new PakField("_pad2", FieldType.U8, 1),
				new PakField("_pad3", FieldType.U8, 1),
				new PakField("buy", FieldType.S32, 1),
				new PakField("price", FieldType.S32, 1),
				new PakField("equipJob", FieldType.S32, 1),
				new PakField("aggressivity", FieldType.S16, 1),
				new PakField("hitProbability", FieldType.U8, 1),
				new PakField("optionProbability", FieldType.U8, 1),
				new PakField("optionMagicItemId", FieldType.S16, 1),
				new PakField("armsAttribute", FieldType.S16, 1),
				new PakField("atckType", FieldType.S16, 1),
				new PakField("atckOption", FieldType.S16, 1),
				new PakField("equipOption", FieldType.S16, 1),
				new PakField("_pad0", FieldType.U8, 1),
				new PakField("_pad1", FieldType.U8, 1),
			}),
			new PakChain("Item", 2, "armour", "ProtectionParameter", 60, new[]
			{
				new PakField("system", FieldType.U8, 1),
				new PakField("_pad0", FieldType.U8, 1),
				new PakField("itemId", FieldType.S16, 1),
				new PakField("nameId", FieldType.S16, 1),
				new PakField("captionId", FieldType.S16, 1),
				new PakField("graphId", FieldType.S16, 1),
				new PakField("strength", FieldType.U8, 1),
				new PakField("vitality", FieldType.U8, 1),
				new PakField("dexterity", FieldType.U8, 1),
				new PakField("intellect", FieldType.U8, 1),
				new PakField("mind", FieldType.U8, 1),
				new PakField("weight", FieldType.U8, 1),
				new PakField("useBattle", FieldType.U8, 1),
				new PakField("useField", FieldType.U8, 1),
				new PakField("allTarget", FieldType.U8, 1),
				new PakField("_pad1", FieldType.U8, 1),
				new PakField("useItemId", FieldType.S16, 1),
				new PakField("targetPossible", FieldType.S16, 1),
				new PakField("targetPosition", FieldType.S16, 1),
				new PakField("_pad2", FieldType.U8, 1),
				new PakField("_pad3", FieldType.U8, 1),
				new PakField("buy", FieldType.S32, 1),
				new PakField("price", FieldType.S32, 1),
				new PakField("equipJob", FieldType.S32, 1),
				new PakField("phylacticPower", FieldType.S16, 1),
				new PakField("magicPhylacticPower", FieldType.S16, 1),
				new PakField("avoidanceProbability", FieldType.U8, 1),
				new PakField("magicAvoidanceProbability", FieldType.U8, 1),
				new PakField("evasionNum", FieldType.U8, 1),
				new PakField("_pad0", FieldType.U8, 1),
				new PakField("armsWeakAttribute", FieldType.S16, 1),
				new PakField("armsAttribute", FieldType.S16, 1),
				new PakField("weakType", FieldType.S16, 1),
				new PakField("antiType", FieldType.S16, 1),
				new PakField("antiOption", FieldType.S16, 1),
				new PakField("equipOption", FieldType.S16, 1),
			}),
			new PakChain("Item", 3, "magic", "MagicParameter", 52, new[]
			{
				new PakField("system", FieldType.U8, 1),
				new PakField("_pad0", FieldType.U8, 1),
				new PakField("itemId", FieldType.S16, 1),
				new PakField("nameId", FieldType.S16, 1),
				new PakField("captionId", FieldType.S16, 1),
				new PakField("graphId", FieldType.S16, 1),
				new PakField("strength", FieldType.U8, 1),
				new PakField("vitality", FieldType.U8, 1),
				new PakField("dexterity", FieldType.U8, 1),
				new PakField("intellect", FieldType.U8, 1),
				new PakField("mind", FieldType.U8, 1),
				new PakField("weight", FieldType.U8, 1),
				new PakField("useBattle", FieldType.U8, 1),
				new PakField("useField", FieldType.U8, 1),
				new PakField("allTarget", FieldType.U8, 1),
				new PakField("_pad1", FieldType.U8, 1),
				new PakField("useItemId", FieldType.S16, 1),
				new PakField("targetPossible", FieldType.S16, 1),
				new PakField("targetPosition", FieldType.S16, 1),
				new PakField("_pad2", FieldType.U8, 1),
				new PakField("_pad3", FieldType.U8, 1),
				new PakField("buy", FieldType.S32, 1),
				new PakField("price", FieldType.S32, 1),
				new PakField("equipJob", FieldType.S32, 1),
				new PakField("magicClass", FieldType.U8, 1),
				new PakField("_pad0", FieldType.U8, 1),
				new PakField("magicAggressivity", FieldType.S16, 1),
				new PakField("successProbability", FieldType.U8, 1),
				new PakField("magicUseKind", FieldType.U8, 1),
				new PakField("magicType", FieldType.S16, 1),
				new PakField("changeCondition", FieldType.S16, 1),
				new PakField("calculate", FieldType.U8, 1),
				new PakField("reflect", FieldType.U8, 1),
			}),
			new PakChain("Item", 4, "keyItems", "ImportantParameter", 28, new[]
			{
				new PakField("system", FieldType.U8, 1),
				new PakField("_pad0", FieldType.U8, 1),
				new PakField("itemId", FieldType.S16, 1),
				new PakField("nameId", FieldType.S16, 1),
				new PakField("captionId", FieldType.S16, 1),
				new PakField("graphId", FieldType.S16, 1),
				new PakField("strength", FieldType.U8, 1),
				new PakField("vitality", FieldType.U8, 1),
				new PakField("dexterity", FieldType.U8, 1),
				new PakField("intellect", FieldType.U8, 1),
				new PakField("mind", FieldType.U8, 1),
				new PakField("weight", FieldType.U8, 1),
				new PakField("useBattle", FieldType.U8, 1),
				new PakField("useField", FieldType.U8, 1),
				new PakField("allTarget", FieldType.U8, 1),
				new PakField("_pad1", FieldType.U8, 1),
				new PakField("useItemId", FieldType.S16, 1),
				new PakField("targetPossible", FieldType.S16, 1),
				new PakField("targetPosition", FieldType.S16, 1),
				new PakField("specialOptionId", FieldType.U8, 1),
				new PakField("_pad0", FieldType.U8, 1),
			}),
			new PakChain("Monster", 0, "monsters", "MonsterParameter", 100, new[]
			{
				new PakField("nameId", FieldType.S16, 1),
				new PakField("textId", FieldType.S16, 1),
				new PakField("familyId", FieldType.S16, 1),
				new PakField("modelId", FieldType.S16, 1),
				new PakField("monsterId", FieldType.S16, 1),
				new PakField("level", FieldType.U8, 1),
				new PakField("size", FieldType.U8, 1),
				new PakField("maxHp", FieldType.S32, 1),
				new PakField("body.strength", FieldType.U8, 1),
				new PakField("body.vitality", FieldType.U8, 1),
				new PakField("body.dexterity", FieldType.U8, 1),
				new PakField("body.intelligence", FieldType.U8, 1),
				new PakField("body.mind", FieldType.U8, 1),
				new PakField("aiLevel", FieldType.U8, 1),
				new PakField("magicSkill", FieldType.U8, 1),
				new PakField("weight", FieldType.U8, 1),
				new PakField("actionNumber", FieldType.S16, 1),
				new PakField("devide", FieldType.S16, 1),
				new PakField("physicsAttack.aggressivity", FieldType.S32, 1),
				new PakField("physicsAttack.hitProbability", FieldType.U8, 1),
				new PakField("physicsAttack.optionProbability", FieldType.U8, 1),
				new PakField("physicsAttack.optionMagicId", FieldType.S16, 1),
				new PakField("physicsAttack.armsAttribute", FieldType.S16, 1),
				new PakField("physicsAttack.attackType", FieldType.S16, 1),
				new PakField("physicsAttack.attackOption", FieldType.S16, 1),
				new PakField("physicsAttack.equipOption", FieldType.S16, 1),
				new PakField("physicsDefense.phylacticPower", FieldType.S32, 1),
				new PakField("physicsDefense.avoidanceNumber", FieldType.S32, 1),
				new PakField("physicsDefense.armsWeakAttribute", FieldType.S16, 1),
				new PakField("physicsDefense.armsAttribute", FieldType.S16, 1),
				new PakField("physicsDefense.antiType", FieldType.S16, 1),
				new PakField("physicsDefense.antiOption", FieldType.S16, 1),
				new PakField("physicsDefense.equipOption", FieldType.S16, 1),
				new PakField("physicsDefense._pad0", FieldType.U8, 1),
				new PakField("physicsDefense._pad1", FieldType.U8, 1),
				new PakField("magicDefense.weakType", FieldType.S16, 1),
				new PakField("magicDefense.magicPhylacticPower", FieldType.S16, 1),
				new PakField("0.specialAction.specialActionId", FieldType.S16, 1),
				new PakField("0.specialAction.specialActionProbability", FieldType.S16, 1),
				new PakField("0.specialAction.actStartHP", FieldType.S32, 1),
				new PakField("1.specialAction.specialActionId", FieldType.S16, 1),
				new PakField("1.specialAction.specialActionProbability", FieldType.S16, 1),
				new PakField("1.specialAction.actStartHP", FieldType.S32, 1),
				new PakField("droppingParameter.droppingItemProbability", FieldType.S16, 1),
				new PakField("droppingParameter.droppingItemTableId", FieldType.S16, 1),
				new PakField("droppingParameter.gold", FieldType.S32, 1),
				new PakField("droppingParameter.exp", FieldType.S32, 1),
				new PakField("drawMapId", FieldType.U8, 1),
				new PakField("_pad0", FieldType.U8, 1),
				new PakField("_pad1", FieldType.U8, 1),
				new PakField("_pad2", FieldType.U8, 1),
			}),
			new PakChain("Monster", 1, "drops", "DropItemParameter", 18, new[]
			{
				new PakField("droppingItemTableId", FieldType.S16, 1),
				new PakField("normalItem", FieldType.S16, 8),
			}),
			new PakChain("Monster", 2, "normalAttacks", "MonsterNormalAttackParameter", 28, new[]
			{
				new PakField("0.effects.frameCounter", FieldType.S32, 1),
				new PakField("0.effects.type", FieldType.S16, 1),
				new PakField("0.effects.category", FieldType.S16, 1),
				new PakField("0.effects.member", FieldType.S16, 1),
				new PakField("0.effects.isLoop", FieldType.S8, 1),
				new PakField("0.effects._pad0", FieldType.U8, 1),
				new PakField("1.effects.frameCounter", FieldType.S32, 1),
				new PakField("1.effects.type", FieldType.S16, 1),
				new PakField("1.effects.category", FieldType.S16, 1),
				new PakField("1.effects.member", FieldType.S16, 1),
				new PakField("1.effects.isLoop", FieldType.S8, 1),
				new PakField("1.effects._pad0", FieldType.U8, 1),
				new PakField("damageMotion", FieldType.S16, 1),
				new PakField("damageValue", FieldType.S16, 1),
			}),
			new PakChain("Monster", 3, "specialAttacks", "MonsterSpecialAttackParameter", 16, new[]
			{
				new PakField("specialAttackId", FieldType.S16, 1),
				new PakField("command", FieldType.S16, 1),
				new PakField("param", FieldType.S16, 6),
			}),
			new PakChain("Monster", 4, "offsets", "MonsterOffsetParameter", 160, new[]
			{
				new PakField("monsterId", FieldType.S32, 1),
				new PakField("hitEffect.cameraDistance", FieldType.S32, 1),
				new PakField("hitEffect.offsetY", FieldType.S32, 1),
				new PakField("hitEffect.bone", FieldType.S32, 1),
				new PakField("conditionEffect.cameraDistance", FieldType.S32, 1),
				new PakField("conditionEffect.offsetY", FieldType.S32, 1),
				new PakField("conditionEffect.bone", FieldType.S32, 1),
				new PakField("cursorPosition.x", FieldType.S32, 1),
				new PakField("cursorPosition.y", FieldType.S32, 1),
				new PakField("cursorPosition.z", FieldType.S32, 1),
				new PakField("damagePosition.x", FieldType.S32, 1),
				new PakField("damagePosition.y", FieldType.S32, 1),
				new PakField("damagePosition.z", FieldType.S32, 1),
				new PakField("criticalPosition.x", FieldType.S32, 1),
				new PakField("criticalPosition.y", FieldType.S32, 1),
				new PakField("criticalPosition.z", FieldType.S32, 1),
				new PakField("initializePosition.x", FieldType.S32, 1),
				new PakField("initializePosition.y", FieldType.S32, 1),
				new PakField("initializePosition.z", FieldType.S32, 1),
				new PakField("touchPosition.x", FieldType.S32, 1),
				new PakField("touchPosition.y", FieldType.S32, 1),
				new PakField("touchPosition.z", FieldType.S32, 1),
				new PakField("touchRadius", FieldType.S32, 1),
				new PakField("height", FieldType.S32, 1),
				new PakField("rotate", FieldType.S32, 1),
				new PakField("scale", FieldType.S32, 1),
				new PakField("shadowX", FieldType.S32, 1),
				new PakField("shadowZ", FieldType.S32, 1),
				new PakField("startCameraPosition.x", FieldType.S32, 1),
				new PakField("startCameraPosition.y", FieldType.S32, 1),
				new PakField("startCameraPosition.z", FieldType.S32, 1),
				new PakField("startCameraTarget.x", FieldType.S32, 1),
				new PakField("startCameraTarget.y", FieldType.S32, 1),
				new PakField("startCameraTarget.z", FieldType.S32, 1),
				new PakField("finishCameraPosition.x", FieldType.S32, 1),
				new PakField("finishCameraPosition.y", FieldType.S32, 1),
				new PakField("finishCameraPosition.z", FieldType.S32, 1),
				new PakField("finishCameraTarget.x", FieldType.S32, 1),
				new PakField("finishCameraTarget.y", FieldType.S32, 1),
				new PakField("finishCameraTarget.z", FieldType.S32, 1),
			}),
			new PakChain("Monster", 5, "specialAttackEffects", "MonsterSpecialAttackEffects", 56, new[]
			{
				new PakField("specialAttackId", FieldType.S16, 1),
				new PakField("changeMotionIndex", FieldType.S16, 1),
				new PakField("0.effectInfo.timingInfo.motionIndex", FieldType.S16, 1),
				new PakField("0.effectInfo.timingInfo.frame", FieldType.S16, 1),
				new PakField("0.effectInfo.category", FieldType.S16, 1),
				new PakField("0.effectInfo.member", FieldType.S16, 1),
				new PakField("0.effectInfo.loop", FieldType.U8, 1),
				new PakField("0.effectInfo.trace", FieldType.U8, 1),
				new PakField("0.effectInfo.target", FieldType.U8, 1),
				new PakField("0.effectInfo.positionType", FieldType.U8, 1),
				new PakField("0.effectInfo.playFrame", FieldType.S32, 1),
				new PakField("1.effectInfo.timingInfo.motionIndex", FieldType.S16, 1),
				new PakField("1.effectInfo.timingInfo.frame", FieldType.S16, 1),
				new PakField("1.effectInfo.category", FieldType.S16, 1),
				new PakField("1.effectInfo.member", FieldType.S16, 1),
				new PakField("1.effectInfo.loop", FieldType.U8, 1),
				new PakField("1.effectInfo.trace", FieldType.U8, 1),
				new PakField("1.effectInfo.target", FieldType.U8, 1),
				new PakField("1.effectInfo.positionType", FieldType.U8, 1),
				new PakField("1.effectInfo.playFrame", FieldType.S32, 1),
				new PakField("0.seInfo.timingInfo.motionIndex", FieldType.S16, 1),
				new PakField("0.seInfo.timingInfo.frame", FieldType.S16, 1),
				new PakField("0.seInfo.category", FieldType.S16, 1),
				new PakField("0.seInfo.member", FieldType.S16, 1),
				new PakField("1.seInfo.timingInfo.motionIndex", FieldType.S16, 1),
				new PakField("1.seInfo.timingInfo.frame", FieldType.S16, 1),
				new PakField("1.seInfo.category", FieldType.S16, 1),
				new PakField("1.seInfo.member", FieldType.S16, 1),
				new PakField("damageTimingInfo.motionIndex", FieldType.S16, 1),
				new PakField("damageTimingInfo.frame", FieldType.S16, 1),
			}),
			new PakChain("Map", 0, "jumps", "CMapJumpParameter", 44, new[]
			{
				new PakField("m_PlPos", FieldType.S32, 3),
				new PakField("m_PlRot", FieldType.S32, 1),
				new PakField("array", FieldType.U8, 16),
				new PakField("m_NextMapIndex", FieldType.S32, 1),
				new PakField("m_ConditionFlag", FieldType.S32, 1),
				new PakField("m_Kind", FieldType.S32, 1),
			}),
			new PakChain("Map", 1, "landForms", "CMapLandFormParameter", 48, new[]
			{
				new PakField("m_LandAttr", FieldType.S16, 12),
				new PakField("m_BattleFieldIndex", FieldType.S16, 12),
			}),
			new PakChain("Map", 2, "monsterParties", "CMapMonsterPartyParameter", 40, new[]
			{
				new PakField("0.0.unnamed0", FieldType.S16, 1),
				new PakField("0.1.unnamed0", FieldType.S16, 1),
				new PakField("0.2.unnamed0", FieldType.S16, 1),
				new PakField("0.3.unnamed0", FieldType.S16, 1),
				new PakField("1.0.unnamed0", FieldType.S16, 1),
				new PakField("1.1.unnamed0", FieldType.S16, 1),
				new PakField("1.2.unnamed0", FieldType.S16, 1),
				new PakField("1.3.unnamed0", FieldType.S16, 1),
				new PakField("2.0.unnamed0", FieldType.S16, 1),
				new PakField("2.1.unnamed0", FieldType.S16, 1),
				new PakField("2.2.unnamed0", FieldType.S16, 1),
				new PakField("2.3.unnamed0", FieldType.S16, 1),
				new PakField("3.0.unnamed0", FieldType.S16, 1),
				new PakField("3.1.unnamed0", FieldType.S16, 1),
				new PakField("3.2.unnamed0", FieldType.S16, 1),
				new PakField("3.3.unnamed0", FieldType.S16, 1),
				new PakField("4.0.unnamed0", FieldType.S16, 1),
				new PakField("4.1.unnamed0", FieldType.S16, 1),
				new PakField("4.2.unnamed0", FieldType.S16, 1),
				new PakField("4.3.unnamed0", FieldType.S16, 1),
			}),
			new PakChain("Map", 3, "sounds", "CMapSoundParameter", 6, new[]
			{
				new PakField("m_BGMIndex", FieldType.S16, 1),
				new PakField("m_CheckFlag", FieldType.S16, 1),
				new PakField("m_ChangeBGMIndex", FieldType.S16, 1),
			}),
			new PakChain("Map", 4, "encounters", "CMapEnCountParameter", 4, new[]
			{
				new PakField("m_AreaLevel", FieldType.S16, 1),
				new PakField("_pad0", FieldType.U16, 1),
				new PakField("m_EncountRevise", FieldType.F32, -1),
			}),
			new PakChain("Map", 5, "cameras", "CMapCameraParameter", 30, new[]
			{
				new PakField("m_Collision", FieldType.S16, 1),
				new PakField("m_ClipNear", FieldType.S16, 1),
				new PakField("m_ClipFar", FieldType.S16, 1),
				new PakField("m_Mode", FieldType.S16, 1),
				new PakField("m_PositionOffset", FieldType.S16, 3),
				new PakField("m_TargetOffset", FieldType.S16, 3),
				new PakField("m_ZoomOnOff", FieldType.S16, 1),
				new PakField("m_ZoomType", FieldType.S16, 1),
				new PakField("m_ZoomMax", FieldType.S16, 1),
				new PakField("m_ZoomMin", FieldType.S16, 1),
				new PakField("m_ZoomSpeed", FieldType.S16, 1),
			}),
		};

		public static PakChain Find(string family, int index)
		{
			foreach (PakChain chain in Chains)
			{
				if (chain.Family == family && chain.Index == index)
				{
					return chain;
				}
			}
			return null;
		}
	}
}
