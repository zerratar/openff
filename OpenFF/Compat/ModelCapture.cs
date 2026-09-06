// Captures a real geometry batch (and its texture) out of the running game, so it
// can be inspected on its own in the model viewer.
//
//   --capture-model        grab the first large textured 3D batch and write it to
//                          <exe>/captures/, then keep playing
//   --test=model           load that capture and draw it with known-good state
//
// Persisting the capture matters: reaching a 3D scene takes about a minute of
// driving, but once a batch is on disk the viewer starts in seconds.

using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenFF.Client
{
	// MonoGame's types by name: the engine's OpenFF.Game, GameTime, Color and vectors sit a namespace up.
	using Game = Microsoft.Xna.Framework.Game;
	using Color = Microsoft.Xna.Framework.Color;
	using Vector2 = Microsoft.Xna.Framework.Vector2;
	using Vector3 = Microsoft.Xna.Framework.Vector3;

	internal static class ModelCapture
	{
		private const int Magic = 0x33464601;   // "FF3\x01"

		public static readonly bool Enabled = Options.Get("capture-model") != null;

		private static bool _done;

		private static string Directory_ =>
			Path.Combine(AppContext.BaseDirectory, "captures");

		/// <summary>Called from the GL layer for every batch; grabs the first big textured one.</summary>
		public static void Offer(VertexPositionColorTexture[] v, int first, int count,
			Texture2D texture, Matrix projection)
		{
			if (!Enabled || _done || v == null || texture == null)
			{
				return;
			}
			// Perspective (M44 == 0) and big enough to be real scene geometry.
			if (count < 300 || projection.M44 != 0f)
			{
				return;
			}
			_done = true;

			try
			{
				System.IO.Directory.CreateDirectory(Directory_);

				using (FileStream fs = File.Create(Path.Combine(Directory_, "model.bin")))
				using (BinaryWriter w = new BinaryWriter(fs))
				{
					w.Write(Magic);
					w.Write(count);
					for (int i = first; i < first + count; i++)
					{
						w.Write(v[i].Position.X);
						w.Write(v[i].Position.Y);
						w.Write(v[i].Position.Z);
						w.Write(v[i].Color.PackedValue);
						w.Write(v[i].TextureCoordinate.X);
						w.Write(v[i].TextureCoordinate.Y);
					}
				}

				using (FileStream fs = File.Create(Path.Combine(Directory_, "model.png")))
				{
					texture.SaveAsPng(fs, texture.Width, texture.Height);
				}

				Log.Write(LogChannel.General, string.Format(
					"captured model: {0} vertices, texture {1}x{2} -> {3}",
					count, texture.Width, texture.Height, Directory_));
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.General, "model capture failed: " + ex.Message);
			}
		}

		/// <summary>Loads a previously captured batch, or null if there is none.</summary>
		public static VertexPositionColorTexture[] Load(GraphicsDevice device, out Texture2D texture)
		{
			texture = null;
			string vertexPath = Path.Combine(Directory_, "model.bin");
			string texturePath = Path.Combine(Directory_, "model.png");
			if (!File.Exists(vertexPath))
			{
				return null;
			}

			try
			{
				VertexPositionColorTexture[] verts;
				using (FileStream fs = File.OpenRead(vertexPath))
				using (BinaryReader r = new BinaryReader(fs))
				{
					if (r.ReadInt32() != Magic)
					{
						return null;
					}
					int count = r.ReadInt32();
					verts = new VertexPositionColorTexture[count];
					for (int i = 0; i < count; i++)
					{
						verts[i].Position = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
						verts[i].Color = new Color { PackedValue = r.ReadUInt32() };
						verts[i].TextureCoordinate = new Vector2(r.ReadSingle(), r.ReadSingle());
					}
				}

				if (File.Exists(texturePath))
				{
					using FileStream fs = File.OpenRead(texturePath);
					texture = Texture2D.FromStream(device, fs);
				}
				return verts;
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.General, "model load failed: " + ex.Message);
				return null;
			}
		}
	}
}
