// A map as a scene, rather than as a table.
//
// The 2D view draws a map as pins on a plan, which is the right thing when you are
// moving somebody two steps to the left. This is the other half: the terrain model with
// everything actually standing on it, which is the only way to see whether a character
// is inside a wall or facing away from the door.
//
// Nothing new is decoded here. A .hich row already says which model a character wears,
// where it stands and which way it faces; the map's own .nmdp is the terrain. The only
// thing worth writing down is that those two agree about units, which was worth checking
// rather than assuming:
//
//   * positions are in the same units as the geometry, one to one. Measured across 179
//     maps, every character sits inside its terrain's bounding box.
//   * posture is in DEGREES, not the fixed point the scripts use. 1901 of 2067 rows are
//     0 and almost all the rest are 90, 180 or 270.
//   * scale is a whole multiplier and is 1 in 2063 of 2067 rows.
//
// The browser fetches the model bundles itself, one per distinct model, so what goes
// over the wire here is a placement list rather than geometry.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Crystal.Editor
{
	internal sealed class SceneObject
	{
		/// <summary>Its row in the .hich, which is what a save writes back to.</summary>
		public int Index { get; set; }

		public string Name { get; set; }
		public string Model { get; set; }

		/// <summary>The workspace entry to fetch the geometry from, or null if there is none.</summary>
		public string Package { get; set; }

		public int X { get; set; }
		public int Y { get; set; }
		public int Z { get; set; }

		/// <summary>Degrees about the vertical axis.</summary>
		public int RotationY { get; set; }

		public int Scale { get; set; }
		public int Cast { get; set; }
		public int Kind { get; set; }
		public string KindName { get; set; }
		public bool HasScript { get; set; }
		public List<string> Lines { get; set; }
		public int Instructions { get; set; }
	}

	internal sealed class MapScene
	{
		public string Map { get; set; }

		/// <summary>The terrain package, or null for the 32 maps that have no model.</summary>
		public string Terrain { get; set; }
		/// <summary>
		/// A field's terrain: the chips of its grid, each a pack in the content, placed
		/// as the game's stage manager places them - size * (spot - centre) in world
		/// units, FF3's layout. Null for every map that is not a field.
		/// </summary>
		public FieldGrid Field { get; set; }

		/// <summary>What stands on the map.</summary>
		public List<SceneObject> Objects { get; set; } = new List<SceneObject>();

		/// <summary>
		/// Rows that are script rather than scenery - "Logic" entries with no position.
		/// They belong in the hierarchy but not in the view.
		/// </summary>
		public List<SceneObject> Logic { get; set; } = new List<SceneObject>();

		public List<MapExit> Exits { get; set; } = new List<MapExit>();

		/// <summary>
		/// The map this one borrows its scenery and collision from, or null when it has
		/// its own. 21 maps have neither a model nor a mesh: the houses of a town, drawn
		/// in one of five shared interiors that the door leading in names. Without this
		/// they came up as an empty room with a character standing in the dark.
		/// </summary>
		public string Borrowed { get; set; }

		/// <summary>The other maps drawn in that same interior, when one is borrowed.</summary>
		public List<string> SharedWith { get; set; } = new List<string>();

		/// <summary>
		/// Reads the collision mesh once and hands each exit the region that fires it.
		/// A map with no mesh, or one that will not read, simply leaves them null - the
		/// scene is still worth drawing.
		/// </summary>
		private static void AddRegions(Workspace workspace, string map,
			List<MapExit> exits)
		{
			string mesh = MapExits.MeshName(map);
			if (exits.Count == 0 || !workspace.Exists(mesh)) return;

			try
			{
				MclFile decoded = Mcl.Read(Lz.Decompress(workspace.Read(mesh)));
				for (int i = 0; i < exits.Count; i++)
				{
					exits[i].Region = Mcl.JumpRegionAt(decoded, i + 1);
				}
			}
			catch (Exception)
			{
			}
		}

		public static MapScene Load(Workspace workspace, string map,
			Func<uint, string> lookupMessage, References references = null)
		{
			// The 2D view has already joined the .hich to the script and the messages,
			// so this takes that rather than doing it a second time and risking the two
			// disagreeing about what a character says.
			MapData loaded = MapModel.Load(workspace, map, lookupMessage);

			MapScene scene = new MapScene
			{
				Map = map,
				Terrain = Package(workspace, map),
				Field = FieldGrid.Load(workspace, map),
				Exits = loaded.Exits
			};

			// A house has no scenery of its own. What it looks like is decided by the
			// door that leads into it, so if this map has no model, ask what leads here.
			if (scene.Terrain == null && references != null)
			{
				string interior = references.InteriorOf(map);
				if (interior != null)
				{
					scene.Borrowed = interior;
					scene.Terrain = Package(workspace, interior);
					scene.SharedWith = references.DrawnIn(interior)
						.Where(other => !string.Equals(other, map,
							StringComparison.OrdinalIgnoreCase))
						.ToList();
				}
			}

			// Where each exit is actually triggered, which is not where its row says the
			// player arrives - one is the doorway, the other is where you come out. The
			// scene draws both, because moving one is not moving the other. A borrowed
			// map's doorways are in the interior's mesh, which is the same mesh every
			// other house sharing it uses.
			AddRegions(workspace, scene.Borrowed ?? map, scene.Exits);

			foreach (MapCharacter character in loaded.Characters)
			{
				SceneObject item = new SceneObject
				{
					Index = character.Index,
					Name = Label(character),
					Model = character.Model,
					Package = Package(workspace, character.Model),
					X = character.X,
					Y = character.Y,
					Z = character.Z,
					RotationY = character.RotationY,
					Scale = 1,
					Cast = character.Cast,
					Kind = character.Kind,
					KindName = character.KindName,
					HasScript = character.HasScript,
					Lines = character.Lines,
					Instructions = character.Instructions
				};

				if (character.Kind == 0)
				{
					scene.Objects.Add(item);
				}
				else
				{
					scene.Logic.Add(item);
				}
			}

			return scene;
		}

		/// <summary>
		/// Something readable for the hierarchy. A row's own name is the model it wears,
		/// which repeats, so the cast number goes with it - that is what makes one
		/// townsperson different from the next.
		/// </summary>
		private static string Label(MapCharacter character)
		{
			string model = string.IsNullOrWhiteSpace(character.Model) ? "?" : character.Model;
			return character.Kind == 0
				? model + " (cast " + character.Cast + ")"
				: character.KindName + " (cast " + character.Cast + ")";
		}

		/// <summary>The package a model name lives in, if the content has one.</summary>
		private static string Package(Workspace workspace, string model)
		{
			if (string.IsNullOrWhiteSpace(model))
			{
				return null;
			}

			string name = "files/" + model + ".nmdp.lz";
			return workspace.Exists(name) ? name : null;
		}
	}

	/// <summary>One chip of a field, and where the game puts it.</summary>
	internal sealed class FieldChip
	{
		public string Package { get; set; }
		public int SpotX { get; set; }
		public int SpotZ { get; set; }
		/// <summary>A stand-in sea chip rather than the spot's own file.</summary>
		public bool Sea { get; set; }
		public float X { get; set; }
		public float Z { get; set; }
	}

	/// <summary>
	/// A field (overworld) as its stage profile describes it: a grid of chips, each a
	/// pack holding a model, an animation and a collision mesh, placed by the stage
	/// manager at size * (spot - centre). The profile (fNN.stgprf) carries the centre
	/// chip at bytes 2 and 3, the chip counts at 14 and 15 and the chip size as two
	/// 20.12 numbers at 20 and 24 - read the same way JumpPart.WithChip reads it in
	/// the client. FF4 places the same grid with z negated relative to FF3's layout
	/// (measured 2026-09-04 against its scripted arrivals); MirrorZ says so, and the
	/// page applies it, so the two layouts can be compared against the exits.
	/// </summary>
	internal sealed class FieldGrid
	{
		public string Name { get; set; }
		public int ChipsX { get; set; }
		public int ChipsZ { get; set; }
		public int CentreX { get; set; }
		public int CentreZ { get; set; }
		public float SizeX { get; set; }
		public float SizeZ { get; set; }
		public bool MirrorZ { get; set; }
		/// <summary>The chip the map being looked at is, for an FF3 chip map; null for a whole field.</summary>
		public string Focus { get; set; }
		public List<FieldChip> Chips { get; set; } = new List<FieldChip>();

		private static readonly System.Text.RegularExpressions.Regex FieldMap =
			new System.Text.RegularExpressions.Regex(@"^(f\d\d)(_[0-9a-fA-F]{2})?$",
				System.Text.RegularExpressions.RegexOptions.Compiled | System.Text.RegularExpressions.RegexOptions.IgnoreCase);

		public static FieldGrid Load(Workspace workspace, string map)
		{
			System.Text.RegularExpressions.Match match = FieldMap.Match(map ?? "");
			if (!match.Success)
			{
				return null;
			}
			string field = match.Groups[1].Value.ToLowerInvariant();
			byte[] profile = null;
			foreach (string candidate in new[] { "files/" + field + ".stgprf", "files/" + field + ".stgprf.lz" })
			{
				if (workspace.Exists(candidate))
				{
					try
					{
						byte[] read = workspace.Read(candidate);
						profile = Lz.IsCompressed(read) ? Lz.Decompress(read) : read;
					}
					catch (Exception)
					{
						profile = null;
					}
					break;
				}
			}
			if (profile == null || profile.Length < 28)
			{
				return null;
			}
			FieldGrid grid = new FieldGrid
			{
				Name = field,
				CentreX = profile[2],
				CentreZ = profile[3],
				ChipsX = profile[14],
				ChipsZ = profile[15],
				SizeX = BitConverter.ToInt32(profile, 20) / 4096f,
				SizeZ = BitConverter.ToInt32(profile, 24) / 4096f,
				MirrorZ = workspace.Game == "ff4",
				Focus = match.Groups[2].Success ? (field + match.Groups[2].Value).ToLowerInvariant() : null
			};
			if (grid.ChipsX <= 0 || grid.ChipsZ <= 0 || grid.SizeX <= 0 || grid.SizeZ <= 0)
			{
				return null;
			}
			// Open sea has no chip of its own: the profile's table (one byte per spot after
			// the header, x + z * chipsX) names which of four sea chips (bytes 6..9 and
			// 10..13) stands in, 0 meaning the spot's own file. The chips are lower-case hex
			// on disk; a spot whose file is still missing is left out.
			int table = 28;
			for (int x = 0; x < grid.ChipsX; x++)
			{
				for (int z = 0; z < grid.ChipsZ; z++)
				{
					int at = table + x + z * grid.ChipsX;
					int sea = at < profile.Length ? profile[at] : 0;
					int fileX = x, fileZ = z;
					if (sea >= 1 && sea <= 4)
					{
						fileX = (sbyte)profile[6 + sea - 1];
						fileZ = (sbyte)profile[10 + sea - 1];
					}
					string package = "files/" + field + "_" + fileX.ToString("x") + fileZ.ToString("x") + ".flsc.lz";
					if (!workspace.Exists(package))
					{
						continue;
					}
					grid.Chips.Add(new FieldChip
					{
						Package = package,
						SpotX = x,
						SpotZ = z,
						Sea = sea != 0,
						X = grid.SizeX * (x - grid.CentreX),
						Z = grid.SizeZ * (z - grid.CentreZ)
					});
				}
			}
			return grid;
		}
	}
}
