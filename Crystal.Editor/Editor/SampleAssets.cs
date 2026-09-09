// The sample assets: models written from code with GltfBuilder, to show what a mod's own
// glTF can be - a weapon and a shield in the hand, a chest, a small environment to walk in,
// a floating crystal with a clip - and to give the Showcase sample mod (Samples/Showcase)
// something to stand on. Every picture is drawn here too (Png.Encode), so the whole set is
// made from nothing by:
//
//   crystal sample-assets <out-dir>
//
// Sizes are the game's: a hero is about 8 units tall, a sword about 7 long. The weapons are
// in the hand's frame (grip at the origin, blade along +z, guard across y; a shield hangs
// down -y from the arm, its face toward +z); the environment is y-up with the ground at 0,
// as a scene's Mesh expects.

using System;
using System.Collections.Generic;
using System.IO;

namespace Crystal.Editor
{
	internal static class SampleAssets
	{
		/// <summary>Writes every sample into the directory; returns what was written and its size.</summary>
		public static List<(string file, int bytes, string note)> WriteAll(string directory)
		{
			Directory.CreateDirectory(directory);
			List<(string, int, string)> written = new List<(string, int, string)>();
			void Put(string name, byte[] glb, string note)
			{
				string path = Path.Combine(directory, name);
				File.WriteAllBytes(path, glb);
				written.Add((name, glb.Length, note));
			}
			Put("rune-blade.glb", RuneBlade(), "a sword for the hand: steel with glowing runes, a gold guard with two gems, a leather grip; clip \"Glow\" turns a ring of light and breathes the gems");
			Put("oak-shield.glb", OakShield(), "a round shield for the left arm: oak planks, an iron rim and boss, rivets, straps");
			Put("treasure-chest.glb", TreasureChest(), "a chest of oak and iron; clip \"Open\" lifts the lid");
			Put("shrine-ground.glb", ShrineGround(), "a ruined shrine to stand on (Solid): grass, a cobbled plaza, eight pillars, an altar, a wall, a path, the tree trunks, a bank around the edge");
			Put("shrine-trees.glb", ShrineTrees(), "the tree crowns and the rocks (decor, not solid)");
			Put("shrine-crystal.glb", ShrineCrystal(), "the shrine's crystal; clip \"Float\" turns it and lets it hover");
			return written;
		}

		// ------------------------------------------------------------------ pictures

		private sealed class Canvas
		{
			public readonly int W, H;
			public readonly byte[] Rgba;
			private uint _seed;
			public Canvas(int w, int h, uint seed) { W = w; H = h; Rgba = new byte[w * h * 4]; _seed = seed; }
			public float Random() { _seed = _seed * 1664525u + 1013904223u; return (_seed >> 8) / 16777216f; }
			public void Fill(int r, int g, int b) { for (int i = 0; i < W * H; i++) Set(i % W, i / W, r, g, b); }
			public void Set(int x, int y, int r, int g, int b, int a = 255)
			{
				x = ((x % W) + W) % W; y = ((y % H) + H) % H;
				int o = (y * W + x) * 4;
				Rgba[o] = (byte)Math.Clamp(r, 0, 255); Rgba[o + 1] = (byte)Math.Clamp(g, 0, 255); Rgba[o + 2] = (byte)Math.Clamp(b, 0, 255); Rgba[o + 3] = (byte)Math.Clamp(a, 0, 255);
			}
			public (int r, int g, int b) Get(int x, int y)
			{
				x = ((x % W) + W) % W; y = ((y % H) + H) % H;
				int o = (y * W + x) * 4;
				return (Rgba[o], Rgba[o + 1], Rgba[o + 2]);
			}
			public void Add(int x, int y, int d) { (int r, int g, int b) = Get(x, y); Set(x, y, r + d, g + d, b + d); }
			public void Noise(int amount) { for (int y = 0; y < H; y++) for (int x = 0; x < W; x++) Add(x, y, (int)((Random() - 0.5f) * 2 * amount)); }
			public void Rect(int x0, int y0, int w, int h, int r, int g, int b) { for (int y = y0; y < y0 + h; y++) for (int x = x0; x < x0 + w; x++) Set(x, y, r, g, b); }
			public byte[] Png() => Crystal.Png.Encode(W, H, Rgba);
		}

		private static byte[] SteelTexture()
		{
			Canvas c = new Canvas(256, 64, 7);
			c.Fill(168, 174, 186);
			// Brushed along the blade: each row a little lighter or darker.
			for (int y = 0; y < c.H; y++) { int d = (int)((c.Random() - 0.5f) * 18); for (int x = 0; x < c.W; x++) c.Add(x, y, d); }
			c.Noise(6);
			// The fuller: a darker groove along the middle third.
			for (int y = 20; y < 44; y++) for (int x = 20; x < 236; x++) c.Add(x, y, -22);
			// Runes in the groove, glowing: 3x6 cells, three pixels a cell.
			int[][] glyphs =
			{
				new[] { 0b010, 0b111, 0b010, 0b010, 0b111, 0b010 },
				new[] { 0b111, 0b100, 0b111, 0b001, 0b111, 0b000 },
				new[] { 0b101, 0b101, 0b111, 0b010, 0b010, 0b010 },
				new[] { 0b010, 0b101, 0b101, 0b101, 0b010, 0b000 },
				new[] { 0b100, 0b110, 0b101, 0b101, 0b110, 0b100 },
				new[] { 0b111, 0b010, 0b010, 0b010, 0b010, 0b111 },
			};
			for (int k = 0; k < 9; k++)
			{
				int[] g = glyphs[k % glyphs.Length];
				int x0 = 30 + k * 22, y0 = 23;
				// A soft halo first, then the marks over it.
				for (int row = 0; row < 6; row++)
					for (int col = 0; col < 3; col++)
						if ((g[row] & (4 >> col)) != 0)
							for (int dy = -1; dy <= 3; dy++) for (int dx = -1; dx <= 3; dx++) { (int r, int gg, int b) = c.Get(x0 + col * 3 + dx, y0 + row * 3 + dy); c.Set(x0 + col * 3 + dx, y0 + row * 3 + dy, Math.Max(r - 20, 70), Math.Max(gg, 170), Math.Max(b, 215)); }
				for (int row = 0; row < 6; row++)
					for (int col = 0; col < 3; col++)
						if ((g[row] & (4 >> col)) != 0)
							for (int dy = 0; dy < 3; dy++) for (int dx = 0; dx < 3; dx++) c.Set(x0 + col * 3 + dx, y0 + row * 3 + dy, 140, 245, 255);
			}
			// Bright edges.
			for (int x = 0; x < c.W; x++) { for (int y = 0; y < 3; y++) c.Add(x, y, 40); for (int y = 61; y < 64; y++) c.Add(x, y, 40); }
			return c.Png();
		}

		private static byte[] LeatherTexture()
		{
			Canvas c = new Canvas(64, 64, 11);
			c.Fill(96, 56, 30);
			c.Noise(10);
			// The wrap: diagonal bands, a dark seam between.
			for (int y = 0; y < 64; y++) for (int x = 0; x < 64; x++) { int band = ((x + y * 2) / 10) % 2; if (band == 1) c.Add(x, y, -14); if ((x + y * 2) % 10 == 0) c.Add(x, y, -40); }
			return c.Png();
		}

		private static byte[] GoldTexture()
		{
			Canvas c = new Canvas(32, 32, 3);
			c.Fill(226, 178, 58);
			c.Noise(12);
			for (int y = 0; y < 32; y += 8) for (int x = 0; x < 32; x++) c.Add(x, y, 30);
			return c.Png();
		}

		private static byte[] GemTexture()
		{
			Canvas c = new Canvas(32, 32, 5);
			for (int y = 0; y < 32; y++)
				for (int x = 0; x < 32; x++)
				{
					float dx = (x - 15.5f) / 16f, dy = (y - 15.5f) / 16f, d = MathF.Sqrt(dx * dx + dy * dy);
					float t = Math.Clamp(1 - d, 0, 1);
					c.Set(x, y, (int)(60 + 195 * t * t), (int)(200 + 55 * t), 255);
				}
			return c.Png();
		}

		private static byte[] CobbleTexture()
		{
			Canvas c = new Canvas(256, 256, 13);
			c.Fill(78, 76, 72);   // the mortar
			int cell = 32;
			for (int row = 0; row < 256 / cell; row++)
				for (int col = -1; col < 256 / cell + 1; col++)
				{
					int x0 = col * cell + (row % 2 == 0 ? 0 : cell / 2) + 2, y0 = row * cell + 2;
					int shade = 112 + (int)(c.Random() * 40);
					int w = cell - 4 - (int)(c.Random() * 4), h = cell - 4 - (int)(c.Random() * 4);
					c.Rect(x0, y0, w, h, shade + 4, shade, shade - 6);
					// A lit top edge and a shadowed bottom edge on every stone.
					for (int x = x0; x < x0 + w; x++) { c.Add(x, y0, 26); c.Add(x, y0 + h - 1, -26); }
				}
			c.Noise(9);
			return c.Png();
		}

		private static byte[] GrassTexture()
		{
			Canvas c = new Canvas(256, 256, 17);
			c.Fill(72, 122, 52);
			c.Noise(16);
			for (int i = 0; i < 2600; i++) { int x = (int)(c.Random() * 256), y = (int)(c.Random() * 256); int len = 2 + (int)(c.Random() * 4); for (int k = 0; k < len; k++) { (int r, int g, int b) = c.Get(x, y - k); c.Set(x, y - k, r + 10, g + 34, b + 8); } }
			for (int i = 0; i < 400; i++) { int x = (int)(c.Random() * 256), y = (int)(c.Random() * 256); c.Set(x, y, 128, 118, 58); }
			return c.Png();
		}

		private static byte[] BarkTexture()
		{
			Canvas c = new Canvas(64, 128, 19);
			c.Fill(84, 58, 38);
			c.Noise(12);
			for (int i = 0; i < 40; i++) { int x = (int)(c.Random() * 64), y0 = (int)(c.Random() * 128), len = 20 + (int)(c.Random() * 60); for (int y = y0; y < y0 + len; y++) { c.Add(x, y, -34); c.Add(x + 1, y, -14); } }
			return c.Png();
		}

		private static byte[] LeavesTexture()
		{
			Canvas c = new Canvas(128, 128, 23);
			c.Fill(44, 108, 46);
			c.Noise(18);
			for (int i = 0; i < 900; i++) { int x = (int)(c.Random() * 128), y = (int)(c.Random() * 128); c.Rect(x, y, 2, 2, 92, 160, 70); }
			for (int i = 0; i < 300; i++) { int x = (int)(c.Random() * 128), y = (int)(c.Random() * 128); c.Rect(x, y, 2, 1, 26, 72, 34); }
			return c.Png();
		}

		private static byte[] PlanksTexture()
		{
			Canvas c = new Canvas(128, 128, 29);
			for (int row = 0; row < 4; row++)
			{
				int shade = (int)(c.Random() * 24);
				c.Rect(0, row * 32, 128, 32, 150 + shade, 108 + shade, 62 + shade / 2);
				for (int i = 0; i < 12; i++) { int y = row * 32 + 2 + (int)(c.Random() * 28); for (int x = 0; x < 128; x++) c.Add(x, y + (int)(MathF.Sin(x / 17f + i) * 1.5f), -18); }
				for (int x = 0; x < 128; x++) { c.Add(x, row * 32, -50); c.Add(x, row * 32 + 31, -30); }
			}
			c.Noise(7);
			return c.Png();
		}

		private static byte[] IronTexture()
		{
			Canvas c = new Canvas(64, 64, 31);
			c.Fill(92, 94, 100);
			c.Noise(14);
			for (int i = 0; i < 60; i++) { int x = (int)(c.Random() * 64), y = (int)(c.Random() * 64); c.Rect(x, y, 2, 2, 120, 118, 122); }
			return c.Png();
		}

		private static byte[] CrystalTexture()
		{
			Canvas c = new Canvas(64, 64, 37);
			for (int y = 0; y < 64; y++) for (int x = 0; x < 64; x++) { float t = y / 63f; c.Set(x, y, (int)(150 + 90 * t), (int)(190 + 50 * t), 255); }
			for (int i = 0; i < 40; i++) { int x = (int)(c.Random() * 64), y0 = (int)(c.Random() * 64), len = 6 + (int)(c.Random() * 20); for (int y = y0; y < y0 + len; y++) c.Add(x, y, 30); }
			return c.Png();
		}

		private static byte[] StoneTexture(bool moss)
		{
			Canvas c = new Canvas(128, 128, moss ? 41u : 43u);
			c.Fill(134, 130, 122);
			c.Noise(14);
			for (int y = 0; y < 128; y += 32) for (int x = 0; x < 128; x++) c.Add(x, y + (x / 64) * 0, -40);   // the courses
			for (int y = 0; y < 128; y++) { int shift = (y / 32) % 2 == 0 ? 0 : 32; for (int x = shift; x < 128; x += 64) c.Add(x, y, -40); }
			if (moss)
				for (int i = 0; i < 700; i++)
				{
					int x = (int)(c.Random() * 128), y = 64 + (int)(c.Random() * c.Random() * 64);
					c.Rect(x, y, 3, 2, 70, 112, 52);
				}
			return c.Png();
		}

		// ------------------------------------------------------------------ the sword

		private static byte[] RuneBlade()
		{
			GltfBuilder g = new GltfBuilder();
			int steel = g.Material("Steel", 1, 1, 1, 1, SteelTexture());
			int gold = g.Material("Gold", 1, 1, 1, 1, GoldTexture());
			int leather = g.Material("Leather", 1, 1, 1, 1, LeatherTexture());
			int gem = g.Material("Gem", 1, 1, 1, 1, GemTexture());

			// The blade: a diamond section (edges on +-y, the spine on +-x) tapering to the point.
			Part blade = new Part(steel);
			float[][] stations = { new[] { 0.58f, 0.92f, 0.16f }, new[] { 1.6f, 0.9f, 0.15f }, new[] { 3.4f, 0.84f, 0.14f }, new[] { 4.9f, 0.7f, 0.11f }, new[] { 5.8f, 0.42f, 0.07f }, new[] { 6.5f, 0f, 0f } };
			for (int i = 0; i + 1 < stations.Length; i++)
			{
				float z0 = stations[i][0], w0 = stations[i][1] / 2, t0 = stations[i][2] / 2, z1 = stations[i + 1][0], w1 = stations[i + 1][1] / 2, t1 = stations[i + 1][2] / 2;
				float u0 = z0 / 6.5f, u1 = z1 / 6.5f;
				// Four faces between the edge on +y, the spine on +x, the edge on -y, the spine on -x.
				float[][] c0 = { new[] { 0, w0, z0 }, new[] { t0, 0, z0 }, new[] { 0, -w0, z0 }, new[] { -t0, 0, z0 } };
				float[][] c1 = { new[] { 0, w1, z1 }, new[] { t1, 0, z1 }, new[] { 0, -w1, z1 }, new[] { -t1, 0, z1 } };
				for (int side = 0; side < 4; side++)
				{
					float[] a = c0[side], b = c0[(side + 1) % 4], c = c1[(side + 1) % 4], d = c1[side];
					float[] n = Part.Normal(a, b, c);
					float v0 = side % 2 == 0 ? 0.5f : 0f, v1 = side % 2 == 0 ? 1f : 0.5f;   // the edge half of the picture at the edges
					int ia = blade.Vertex(a[0], a[1], a[2], n[0], n[1], n[2], u0, v0), ib = blade.Vertex(b[0], b[1], b[2], n[0], n[1], n[2], u0, v1);
					int ic = blade.Vertex(c[0], c[1], c[2], n[0], n[1], n[2], u1, v1), id = blade.Vertex(d[0], d[1], d[2], n[0], n[1], n[2], u1, v0);
					if (w1 == 0 && t1 == 0) blade.Triangle(ia, ib, ic); else blade.Quad(ia, ib, ic, id);
				}
			}

			// The guard: a bar across y in five pieces, its ends swept toward the blade, capped with knobs.
			Part guard = new Part(gold);
			for (int k = -2; k <= 2; k++)
			{
				float y = k * 0.46f, zLift = 0.09f * Math.Abs(k);
				int from = guard.VertexCount;
				guard.Box(-0.13f, -0.24f, 0.30f, 0.13f, 0.24f, 0.60f);
				guard.Transform(from, Xform.RotateX(k * 9f).Then(Xform.Translate(0, y, zLift)));
			}
			foreach (float y in new[] { -1.18f, 1.18f })
			{
				int from = guard.VertexCount;
				guard.Sphere(0.17f, 6, 10);
				guard.Transform(from, Xform.Translate(0, y, 0.66f));
			}
			// The collar at the grip's top and the pommel.
			{
				int from = guard.VertexCount;
				guard.Cylinder(0.24f, 0.22f, 0, 0.1f, 12);
				guard.Transform(from, Xform.RotateX(90).Then(Xform.Translate(0, 0, 0.22f)));
				from = guard.VertexCount;
				guard.Sphere(0.25f, 7, 12);
				guard.Transform(from, Xform.Scale(1f, 1f, 0.8f).Then(Xform.Translate(0, 0, -0.98f)));
			}

			// The grip: a lathe along y turned to z, with a swell for the hand.
			Part grip = new Part(leather);
			{
				int from = grip.VertexCount;
				grip.Lathe(new[] { new[] { 0.14f, -0.8f }, new[] { 0.17f, -0.5f }, new[] { 0.16f, -0.1f }, new[] { 0.18f, 0.24f } }, 10, true, true);
				grip.Transform(from, Xform.RotateX(90));
			}

			// The gems: one each side of the guard's middle, on a node that breathes.
			Part gems = new Part(gem);
			foreach (float x in new[] { -0.16f, 0.16f })
			{
				int from = gems.VertexCount;
				gems.Sphere(0.13f, 6, 10);
				gems.Transform(from, Xform.Translate(x, 0, 0));
			}

			// The ring of light around the blade, on a node that turns and drifts.
			Part ring = new Part(gem);
			{
				int from = ring.VertexCount;
				ring.Torus(0.62f, 0.045f, 32, 8);
				ring.Transform(from, Xform.RotateX(90));
				// Three studs on the ring so its turning shows.
				for (int k = 0; k < 3; k++)
				{
					from = ring.VertexCount;
					ring.Sphere(0.09f, 5, 8);
					float a = k * 120f * MathF.PI / 180;
					ring.Transform(from, Xform.Translate(0.62f * MathF.Cos(a), 0.62f * MathF.Sin(a), 0));
				}
			}

			int bladeMesh = g.Mesh("Blade", blade, guard, grip);
			int gemMesh = g.Mesh("Gems", gems);
			int ringMesh = g.Mesh("Ring", ring);
			int root = g.Node("RuneBlade");
			g.Node("Blade", bladeMesh, root);
			int gemNode = g.Node("Gems", gemMesh, root, new[] { 0f, 0f, 0.45f });
			int ringNode = g.Node("Ring", ringMesh, root, new[] { 0f, 0f, 1.25f });

			// "Glow": the ring turns once in 3 s and drifts along the blade; the gems breathe over 1.5 s.
			List<float> turnTimes = new List<float>(), turnValues = new List<float>();
			for (int k = 0; k <= 8; k++)
			{
				turnTimes.Add(k * 3f / 8);
				turnValues.AddRange(GltfBuilder.Quaternion(0, 0, 1, k * 45f));
			}
			float[] driftTimes = { 0f, 1.5f, 3f }, driftValues = { 0, 0, 1.25f, 0, 0, 1.75f, 0, 0, 1.25f };
			float[] breatheTimes = { 0f, 0.75f, 1.5f, 2.25f, 3f }, breatheValues = { 1, 1, 1, 1.35f, 1.35f, 1.35f, 1, 1, 1, 1.35f, 1.35f, 1.35f, 1, 1, 1 };
			g.Animation("Glow",
				(ringNode, "rotation", turnTimes.ToArray(), turnValues.ToArray()),
				(ringNode, "translation", driftTimes, driftValues),
				(gemNode, "scale", breatheTimes, breatheValues));
			return g.Glb();
		}

		// ------------------------------------------------------------------ the shield

		// Built face-on at the origin (the boss toward +z, the straps toward -z), then put where the
		// game's own shields sit in the hand joint: the game's w260 is a round shield of radius 1.6 whose
		// face tilts about 35 degrees from the joint's -z toward +y, centred a little above and behind
		// the joint - the forearm's angle. The same numbers a modder's Blender export needs, or the
		// definition's modelRotation/modelOffset to match a model made facing +z.
		private static byte[] OakShield()
		{
			GltfBuilder g = new GltfBuilder();
			int planks = g.Material("Oak", 1, 1, 1, 1, PlanksTexture());
			int iron = g.Material("Iron", 1, 1, 1, 1, IronTexture());
			int leather = g.Material("Leather", 1, 1, 1, 1, LeatherTexture());
			const float radius = 3.1f, half = 0.16f, centreY = 0f;
			const int segments = 24;

			// The plate: two faces with the planks laid flat across them, and a rim band.
			Part plate = new Part(planks);
			foreach (float z in new[] { half, -half })
			{
				float nz = z > 0 ? 1f : -1f;
				int centre = plate.Vertex(0, centreY, z, 0, 0, nz, 0.5f, 0.5f);
				int[] rim = new int[segments + 1];
				for (int s = 0; s <= segments; s++)
				{
					float a = s * MathF.PI * 2 / segments, x = radius * MathF.Cos(a), y = centreY + radius * MathF.Sin(a);
					rim[s] = plate.Vertex(x, y, z, 0, 0, nz, 0.5f + 0.5f * MathF.Cos(a), 0.5f - 0.5f * MathF.Sin(a));
				}
				for (int s = 0; s < segments; s++) { if (z > 0) plate.Triangle(centre, rim[s], rim[s + 1]); else plate.Triangle(centre, rim[s + 1], rim[s]); }
			}
			Part band = new Part(iron);
			{
				int from = band.VertexCount;
				band.Lathe(new[] { new[] { radius, -half }, new[] { radius, half } }, segments);
				band.Transform(from, Xform.RotateX(90).Then(Xform.Translate(0, centreY, 0)));
				// The rim.
				from = band.VertexCount;
				band.Torus(radius, 0.2f, segments, 8);
				band.Transform(from, Xform.RotateX(90).Then(Xform.Translate(0, centreY, 0)));
				// The boss: a dome toward +z.
				from = band.VertexCount;
				float[][] dome = new float[7][];
				for (int r = 0; r <= 6; r++) { float t = MathF.PI / 2 * r / 6; dome[r] = new[] { 0.95f * MathF.Sin(t), 0.9f * MathF.Cos(t) }; }
				Array.Reverse(dome);
				band.Lathe(dome, 16);
				band.Transform(from, Xform.RotateX(90).Then(Xform.Translate(0, centreY, half - 0.05f)));
				// Rivets around the face.
				for (int k = 0; k < 8; k++)
				{
					from = band.VertexCount;
					band.Sphere(0.14f, 4, 8);
					float a = k * MathF.PI * 2 / 8 + MathF.PI / 8;
					band.Transform(from, Xform.Translate(2.55f * MathF.Cos(a), centreY + 2.55f * MathF.Sin(a), half + 0.04f));
				}
			}
			// The straps at the back, where the arm goes.
			Part straps = new Part(leather);
			straps.Box(-0.4f, centreY - 2.2f, -half - 0.5f, 0.4f, centreY + 2.2f, -half);
			straps.Box(-1.6f, centreY - 0.3f, -half - 0.5f, 1.6f, centreY + 0.3f, -half);

			// Into the hand: a touch larger than the game's own (radius 1.9), turned so the boss faces the
			// joint's -z, tilted to the forearm, and set where w260 sits.
			Xform inHand = Xform.Scale(0.61f, 0.61f, 0.61f).Then(Xform.RotateY(180)).Then(Xform.RotateX(35)).Then(Xform.Translate(0, 0.3f, -0.4f));
			plate.Transform(0, inHand);
			band.Transform(0, inHand);
			straps.Transform(0, inHand);

			int mesh = g.Mesh("Shield", plate, band, straps);
			g.Node("OakShield", mesh);
			return g.Glb();
		}

		// ------------------------------------------------------------------ the chest

		private static byte[] TreasureChest()
		{
			GltfBuilder g = new GltfBuilder();
			int planks = g.Material("Oak", 1, 1, 1, 1, PlanksTexture());
			int iron = g.Material("Iron", 1, 1, 1, 1, IronTexture());
			int gold = g.Material("Gold", 1, 1, 1, 1, GoldTexture());
			const float w = 3.2f, d = 2.1f, h = 2.2f;   // half width, half depth, body height

			Part body = new Part(planks);
			body.Box(-w, 0, -d, w, h, d, 2.2f);
			Part bands = new Part(iron);
			foreach (float x in new[] { -w + 0.5f, w - 0.5f })
			{
				bands.Box(x - 0.18f, -0.02f, -d - 0.06f, x + 0.18f, h + 0.02f, d + 0.06f);
			}
			bands.Box(-w - 0.06f, 0, -d - 0.06f, w + 0.06f, 0.3f, d + 0.06f);   // the foot band
			Part clasp = new Part(gold);
			clasp.Box(-0.4f, h - 0.7f, d - 0.02f, 0.4f, h + 0.1f, d + 0.22f);

			// The lid: a half-cylinder along x, hinged at the back top edge; on its own node so a clip can lift it.
			Part lid = new Part(planks);
			{
				int from = lid.VertexCount;
				float[][] profile = new float[9][];
				for (int r = 0; r <= 8; r++) { float t = MathF.PI * r / 8; profile[r] = new[] { d * MathF.Sin(t), -d * MathF.Cos(t) }; }
				// A lathe gives a full round; only the upper half is wanted, so build the arch by faces instead.
				for (int s = 0; s < 8; s++)
				{
					float a0 = MathF.PI * s / 8, a1 = MathF.PI * (s + 1) / 8;
					float[] p0 = { -w, d * MathF.Sin(a0), -d * MathF.Cos(a0) }, p1 = { w, d * MathF.Sin(a0), -d * MathF.Cos(a0) };
					float[] p2 = { w, d * MathF.Sin(a1), -d * MathF.Cos(a1) }, p3 = { -w, d * MathF.Sin(a1), -d * MathF.Cos(a1) };
					lid.Face(p1, p0, p3, p2, 2.2f, 0.4f);
				}
				// The two ends of the arch.
				for (int s = 0; s < 8; s++)
				{
					float a0 = MathF.PI * s / 8, a1 = MathF.PI * (s + 1) / 8;
					lid.Tri(new[] { -w, 0, 0 }, new[] { -w, d * MathF.Sin(a1), -d * MathF.Cos(a1) }, new[] { -w, d * MathF.Sin(a0), -d * MathF.Cos(a0) });
					lid.Tri(new[] { w, 0, 0 }, new[] { w, d * MathF.Sin(a0), -d * MathF.Cos(a0) }, new[] { w, d * MathF.Sin(a1), -d * MathF.Cos(a1) });
				}
				// The lid's frame sits with its hinge (the back edge) at the origin: shift so the arch's centre is d in front of the hinge.
				lid.Transform(from, Xform.Translate(0, 0, d));
			}
			Part lidBands = new Part(iron);
			foreach (float x in new[] { -w + 0.5f, w - 0.5f })
			{
				int from = lidBands.VertexCount;
				for (int s = 0; s < 8; s++)
				{
					float a0 = MathF.PI * s / 8, a1 = MathF.PI * (s + 1) / 8, r = d + 0.06f;
					lidBands.Face(new[] { x + 0.18f, r * MathF.Sin(a0), -r * MathF.Cos(a0) }, new[] { x - 0.18f, r * MathF.Sin(a0), -r * MathF.Cos(a0) }, new[] { x - 0.18f, r * MathF.Sin(a1), -r * MathF.Cos(a1) }, new[] { x + 0.18f, r * MathF.Sin(a1), -r * MathF.Cos(a1) });
				}
				lidBands.Transform(from, Xform.Translate(0, 0, d));
			}

			int bodyMesh = g.Mesh("Body", body, bands, clasp);
			int lidMesh = g.Mesh("Lid", lid, lidBands);
			int root = g.Node("Chest");
			g.Node("Body", bodyMesh, root);
			int lidNode = g.Node("Lid", lidMesh, root, new[] { 0f, h, -d });
			// "Open": the lid swings back 110 degrees about its hinge in 0.6 s and stays.
			List<float> times = new List<float>(), values = new List<float>();
			for (int k = 0; k <= 6; k++) { times.Add(k * 0.1f); values.AddRange(GltfBuilder.Quaternion(1, 0, 0, -110f * k / 6)); }
			times.Add(2f); values.AddRange(GltfBuilder.Quaternion(1, 0, 0, -110f));
			g.Animation("Open", (lidNode, "rotation", times.ToArray(), values.ToArray()));
			return g.Glb();
		}

		// ------------------------------------------------------------------ the shrine

		private static float Hash(int i, int salt) { uint h = (uint)(i * 374761393 + salt * 668265263); h = (h ^ (h >> 13)) * 1274126177u; return ((h ^ (h >> 16)) & 0xFFFF) / 65535f; }

		private static byte[] ShrineGround()
		{
			GltfBuilder g = new GltfBuilder();
			int grass = g.Material("Grass", 1, 1, 1, 1, GrassTexture());
			int cobble = g.Material("Cobbles", 1, 1, 1, 1, CobbleTexture());
			int stone = g.Material("Stone", 1, 1, 1, 1, StoneTexture(false));
			int moss = g.Material("MossStone", 1, 1, 1, 1, StoneTexture(true));

			// The ground: grass, 160 across, in a grid so the lighting has vertices to shade.
			Part ground = new Part(grass);
			{
				const int n = 8; const float size = 160f;
				int[][] grid = new int[n + 1][];
				for (int i = 0; i <= n; i++)
				{
					grid[i] = new int[n + 1];
					for (int j = 0; j <= n; j++)
					{
						float x = -size / 2 + size * i / n, z = -size / 2 + size * j / n;
						grid[i][j] = ground.Vertex(x, 0, z, 0, 1, 0, x / 10f, z / 10f);
					}
				}
				for (int i = 0; i < n; i++) for (int j = 0; j < n; j++) ground.Quad(grid[i][j], grid[i][j + 1], grid[i + 1][j + 1], grid[i + 1][j]);
			}

			// The plaza: a cobbled disc raised 0.4, its stones laid flat, a stone kerb around it.
			Part plaza = new Part(cobble);
			{
				const float r = 26f, top = 0.4f; const int seg = 48;
				int centre = plaza.Vertex(0, top, 0, 0, 1, 0, 0, 0);
				int[] rim = new int[seg + 1];
				for (int s = 0; s <= seg; s++) { float a = s * MathF.PI * 2 / seg, x = r * MathF.Cos(a), z = r * MathF.Sin(a); rim[s] = plaza.Vertex(x, top, z, 0, 1, 0, x / 8f, z / 8f); }
				for (int s = 0; s < seg; s++) plaza.Triangle(centre, rim[s + 1], rim[s]);
				// The path out to the front (+z): a cobbled strip from the plaza to the edge of the trees.
				for (int k = 0; k < 6; k++)
				{
					float z0 = 24 + k * 6, z1 = z0 + 6;
					plaza.Face(new[] { -4f, 0.12f, z1 }, new[] { 4f, 0.12f, z1 }, new[] { 4f, 0.12f, z0 }, new[] { -4f, 0.12f, z0 }, 1f, 0.75f);
				}
			}
			// The kerb: a bevel down to the grass, so the plaza is a step up, not a wall.
			Part kerb = new Part(stone);
			kerb.Lathe(new[] { new[] { 27.4f, 0f }, new[] { 26.1f, 0.4f } }, 48);

			// The altar in the middle: a block and a slab.
			Part altar = new Part(moss);
			altar.Box(-2.6f, 0.4f, -2.6f, 2.6f, 2.9f, 2.6f, 2.6f);
			altar.Box(-3.2f, 2.9f, -3.2f, 3.2f, 3.4f, 3.2f, 3.2f);

			// Eight pillars on a ring, some broken.
			Part pillars = new Part(stone);
			float[] heights = { 14, 9, 14, 6, 14, 14, 11, 14 };
			for (int k = 0; k < 8; k++)
			{
				float a = k * MathF.PI * 2 / 8 + MathF.PI / 8, x = 20 * MathF.Cos(a), z = 20 * MathF.Sin(a), hgt = heights[k];
				int from = pillars.VertexCount;
				pillars.Box(-1.8f, 0.4f, -1.8f, 1.8f, 1.4f, 1.8f);                                   // the plinth
				pillars.Lathe(new[] { new[] { 1.35f, 1.4f }, new[] { 1.25f, hgt - 0.5f }, new[] { hgt >= 14 ? 1.3f : 0.9f, hgt } }, 12, false, true);
				if (hgt >= 14) pillars.Box(-1.7f, hgt, -1.7f, 1.7f, hgt + 0.8f, 1.7f);              // the capital
				pillars.Transform(from, Xform.RotateY(k * 23f).Then(Xform.Translate(x, 0, z)));
			}

			// A low wall around the back half, in pieces, with a gap or two.
			Part wall = new Part(moss);
			for (int k = 0; k < 14; k++)
			{
				if (k == 4 || k == 9) continue;
				float a = (200f + k * 11.5f) * MathF.PI / 180, x = 30.5f * MathF.Cos(a), z = 30.5f * MathF.Sin(a);
				int from = wall.VertexCount;
				float hgt = 2.4f + Hash(k, 1) * 0.8f;
				wall.Box(-3.1f, 0, -0.6f, 3.1f, hgt, 0.6f, 3f);
				wall.Transform(from, Xform.RotateY(200f + k * 11.5f + 90f).Then(Xform.Translate(x, 0, z)));
			}

			// The tree trunks are solid (the crowns, in shrine-trees.glb, are not - nothing walks up there).
			Part trunks = new Part(g.Material("Bark", 1, 1, 1, 1, BarkTexture()));
			foreach ((float x, float z, float scale, float _) in TreePlaces())
			{
				int from = trunks.VertexCount;
				trunks.Lathe(new[] { new[] { 1.5f, 0f }, new[] { 1.1f, 6f }, new[] { 0.8f, 9.5f } }, 8, false, true);
				trunks.Transform(from, Xform.Scale(scale).Then(Xform.Translate(x, 0, z)));
			}

			// A grassy bank around the edge: the wall that keeps everyone on the map (a mod's map has
			// no edge of its own; a Solid Mesh's steep faces are walls to the hero and to a Roam).
			Part bank = new Part(grass);
			{
				const float r = 74f, top = 7f; const int seg = 48;
				for (int s = 0; s < seg; s++)
				{
					float a0 = s * MathF.PI * 2 / seg, a1 = (s + 1) * MathF.PI * 2 / seg;
					float[] p0 = { r * MathF.Cos(a0), 0, r * MathF.Sin(a0) }, p1 = { r * MathF.Cos(a1), 0, r * MathF.Sin(a1) };
					float[] p2 = { (r + 3) * MathF.Cos(a1), top, (r + 3) * MathF.Sin(a1) }, p3 = { (r + 3) * MathF.Cos(a0), top, (r + 3) * MathF.Sin(a0) };
					bank.Face(p0, p1, p2, p3, 1f, 0.7f);   // facing the middle
				}
			}

			int mesh = g.Mesh("Shrine", ground, plaza, kerb, altar, pillars, wall, bank, trunks);
			g.Node("ShrineGround", mesh);
			return g.Glb();
		}

		/// <summary>Where the trees stand: on a ring outside the shrine, the path to the front left open. Shared by the trunks (solid, in the ground file) and the crowns (decor).</summary>
		private static List<(float x, float z, float scale, float yaw)> TreePlaces()
		{
			List<(float, float, float, float)> places = new List<(float, float, float, float)>();
			for (int k = 0; k < 14; k++)
			{
				float a = (k * 25.7f + Hash(k, 11) * 12f) * MathF.PI / 180, r = 44 + Hash(k, 12) * 16;
				float x = r * MathF.Cos(a), z = r * MathF.Sin(a);
				if (Math.Abs(x) < 9 && z > 20) continue;   // the path stays open
				places.Add((x, z, 0.85f + Hash(k, 13) * 0.5f, Hash(k, 14) * 360));
			}
			return places;
		}

		private static byte[] ShrineTrees()
		{
			GltfBuilder g = new GltfBuilder();
			int leaves = g.Material("Leaves", 1, 1, 1, 1, LeavesTexture());
			int stone = g.Material("Stone", 1, 1, 1, 1, StoneTexture(false));
			Part crowns = new Part(leaves);
			foreach ((float x, float z, float scale, float yaw) in TreePlaces())
			{
				int from = crowns.VertexCount;
				crowns.Cylinder(6.5f, 0.4f, 6.5f, 12f, 9, true);
				crowns.Cylinder(5.2f, 0.3f, 10f, 15.5f, 9, true);
				crowns.Cylinder(3.4f, 0f, 13.5f, 19f, 9, true);
				crowns.Transform(from, Xform.Scale(scale).Then(Xform.RotateY(yaw)).Then(Xform.Translate(x, 0, z)));
			}
			// Rocks about the grass - decor, walked over rather than bumped into.
			Part rocks = new Part(stone);
			for (int k = 0; k < 9; k++)
			{
				float a = (Hash(k, 2) * 300f + 30f) * MathF.PI / 180, r = 38 + Hash(k, 3) * 22;
				if (Math.Abs(MathF.Cos(a - MathF.PI / 2)) > 0.93f && MathF.Sin(a) > 0) continue;   // not on the path
				int from = rocks.VertexCount;
				rocks.Sphere(1.6f + Hash(k, 4) * 1.6f, 4, 7);
				rocks.Transform(from, Xform.Scale(1f, 0.55f + Hash(k, 5) * 0.4f, 0.8f + Hash(k, 6) * 0.5f).Then(Xform.RotateY(Hash(k, 7) * 360)).Then(Xform.Translate(r * MathF.Cos(a), 0.2f, r * MathF.Sin(a))));
			}
			int mesh = g.Mesh("Trees", crowns, rocks);
			g.Node("ShrineTrees", mesh);
			return g.Glb();
		}

		private static byte[] ShrineCrystal()
		{
			GltfBuilder g = new GltfBuilder();
			int crystal = g.Material("Crystal", 1, 1, 1, 1, CrystalTexture());
			int gold = g.Material("Gold", 1, 1, 1, 1, GoldTexture());
			// An elongated octahedron, faceted (the lathe with 4 segments gives flat faces).
			Part shard = new Part(crystal);
			shard.Lathe(new[] { new[] { 0f, -2.6f }, new[] { 1.4f, -0.4f }, new[] { 1.1f, 0.6f }, new[] { 0f, 3.2f } }, 5);
			// A gold band about its waist.
			Part band = new Part(gold);
			band.Torus(1.45f, 0.12f, 20, 6);
			int mesh = g.Mesh("Crystal", shard, band);
			int node = g.Node("Crystal", mesh, -1, new[] { 0f, 3.2f, 0f });
			// "Float": a slow turn about y and a hover.
			List<float> turnTimes = new List<float>(), turnValues = new List<float>();
			for (int k = 0; k <= 8; k++) { turnTimes.Add(k * 6f / 8); turnValues.AddRange(GltfBuilder.Quaternion(0, 1, 0, k * 45f)); }
			g.Animation("Float",
				(node, "rotation", turnTimes.ToArray(), turnValues.ToArray()),
				(node, "translation", new[] { 0f, 1.5f, 3f, 4.5f, 6f }, new[] { 0, 3.2f, 0, 0, 3.9f, 0, 0, 3.2f, 0, 0, 2.7f, 0, 0, 3.2f, 0 }));
			return g.Glb();
		}
	}
}
