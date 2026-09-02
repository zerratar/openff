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

namespace FF3.ContentTool.Editor
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

		/// <summary>What stands on the map.</summary>
		public List<SceneObject> Objects { get; set; } = new List<SceneObject>();

		/// <summary>
		/// Rows that are script rather than scenery - "Logic" entries with no position.
		/// They belong in the hierarchy but not in the view.
		/// </summary>
		public List<SceneObject> Logic { get; set; } = new List<SceneObject>();

		public List<MapExit> Exits { get; set; } = new List<MapExit>();

		public static MapScene Load(Workspace workspace, string map,
			Func<uint, string> lookupMessage)
		{
			// The 2D view has already joined the .hich to the script and the messages,
			// so this takes that rather than doing it a second time and risking the two
			// disagreeing about what a character says.
			MapData loaded = MapModel.Load(workspace, map, lookupMessage);

			MapScene scene = new MapScene
			{
				Map = map,
				Terrain = Package(workspace, map),
				Exits = loaded.Exits
			};

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
}
