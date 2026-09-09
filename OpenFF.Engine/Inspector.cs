// What a behaviour's public fields look like in Crystal's inspector.
//
// A public field or settable property of a Behaviour is an editor field as it stands: the
// inspector shows it with its type's input and the code's default, and a scene file sets it
// (SceneLoader.SetFields). These attributes say more about how, the way Unity's do: a
// heading over a group of fields, a tooltip, a slider's range, an item picker for an item
// id, or that a field is not for the inspector at all. The XML summary on a field is the
// tooltip when there is no Tooltip attribute, so documented code needs none of these.

using System;

namespace OpenFF
{
	/// <summary>A heading in the inspector above this field and the ones after it.</summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public sealed class HeaderAttribute : Attribute
	{
		public string Text { get; }
		public HeaderAttribute(string text) { Text = text; }
	}

	/// <summary>What the inspector says when the pointer rests on the field (else the XML summary).</summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public sealed class TooltipAttribute : Attribute
	{
		public string Text { get; }
		public TooltipAttribute(string text) { Text = text; }
	}

	/// <summary>A number the inspector edits with a slider between two bounds.</summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public sealed class RangeAttribute : Attribute
	{
		public float Min { get; }
		public float Max { get; }
		public RangeAttribute(float min, float max) { Min = min; Max = max; }
	}

	/// <summary>An int that is an item id: the inspector offers the game's item list to pick from.</summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public sealed class ItemFieldAttribute : Attribute
	{
	}

	/// <summary>
	/// A string that is a flag expression - "0:14 !0:11", alternatives with | - as WhenFlags
	/// and Talk read them: the inspector offers the map's flags to pick from (the ones its
	/// script tests and sets, with who does), and checks the shape.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public sealed class FlagFieldAttribute : Attribute
	{
	}

	/// <summary>An int field that holds a monster party (formation) number: the editor offers the game's and the mod's formations to pick from.</summary>
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class FormationFieldAttribute : Attribute
	{
	}

	/// <summary>A string field that names a map (t01_01, or one of the mod's own): the editor offers the game's maps and the mod's to pick from.</summary>
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class MapFieldAttribute : Attribute
	{
	}

	/// <summary>An int field that holds a tune's number in the game's music table: the editor offers the Audio library's BGM list to pick from.</summary>
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class BgmFieldAttribute : Attribute
	{
	}

	/// <summary>A public field the inspector leaves out (still set from a scene file when named).</summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public sealed class HideInInspectorAttribute : Attribute
	{
	}
}
