// The smallest glTF the editor writes itself: a box, for the ground a new map of the mod's
// own starts with when there is no Blender export to hand. One mesh, one flat-coloured
// material, positions and normals in an embedded buffer - the client's reader and the
// editor's 3D view take it like any other file in assets/.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Crystal.Editor
{
	internal static class GltfWriter
	{
		/// <summary>
		/// Writes a box standing on y = 0: width along X, height up, depth along Z, centred
		/// on the origin in X and Z, its top and four sides (no underside), in one colour.
		/// </summary>
		public static void WriteBox(string path, float width, float height, float depth, float r, float g, float b)
		{
			float hx = width / 2, hz = depth / 2, h = height;
			// Each face: its normal, then two triangles wound counter-clockwise seen from outside.
			(float[] n, float[][] v)[] faces =
			{
				(new[] { 0f, 0f, 1f }, new[] { V(-hx, 0, hz), V(hx, 0, hz), V(hx, h, hz), V(-hx, 0, hz), V(hx, h, hz), V(-hx, h, hz) }),
				(new[] { 0f, 0f, -1f }, new[] { V(hx, 0, -hz), V(-hx, 0, -hz), V(-hx, h, -hz), V(hx, 0, -hz), V(-hx, h, -hz), V(hx, h, -hz) }),
				(new[] { 1f, 0f, 0f }, new[] { V(hx, 0, hz), V(hx, 0, -hz), V(hx, h, -hz), V(hx, 0, hz), V(hx, h, -hz), V(hx, h, hz) }),
				(new[] { -1f, 0f, 0f }, new[] { V(-hx, 0, -hz), V(-hx, 0, hz), V(-hx, h, hz), V(-hx, 0, -hz), V(-hx, h, hz), V(-hx, h, -hz) }),
				(new[] { 0f, 1f, 0f }, new[] { V(-hx, h, hz), V(hx, h, hz), V(hx, h, -hz), V(-hx, h, hz), V(hx, h, -hz), V(-hx, h, -hz) }),
			};
			List<byte> positions = new List<byte>(), normals = new List<byte>();
			int count = 0;
			foreach ((float[] n, float[][] v) face in faces)
			{
				foreach (float[] vertex in face.v)
				{
					foreach (float x in vertex) positions.AddRange(BitConverter.GetBytes(x));
					foreach (float x in face.n) normals.AddRange(BitConverter.GetBytes(x));
					count++;
				}
			}
			byte[] buffer = new byte[positions.Count + normals.Count];
			positions.CopyTo(buffer, 0);
			normals.CopyTo(buffer, positions.Count);
			string F(float x) => x.ToString("0.###", CultureInfo.InvariantCulture);
			StringBuilder json = new StringBuilder();
			json.Append("{ \"asset\": { \"version\": \"2.0\", \"generator\": \"Crystal\" }, \"scene\": 0, \"scenes\": [ { \"nodes\": [0] } ], \"nodes\": [ { \"mesh\": 0, \"name\": \"Box\" } ],\n");
			json.Append("  \"meshes\": [ { \"name\": \"Box\", \"primitives\": [ { \"attributes\": { \"POSITION\": 0, \"NORMAL\": 1 }, \"material\": 0 } ] } ],\n");
			json.Append("  \"materials\": [ { \"name\": \"Flat\", \"pbrMetallicRoughness\": { \"baseColorFactor\": [" + F(r) + ", " + F(g) + ", " + F(b) + ", 1.0], \"metallicFactor\": 0.0 } } ],\n");
			json.Append("  \"buffers\": [ { \"byteLength\": " + buffer.Length + ", \"uri\": \"data:application/octet-stream;base64," + Convert.ToBase64String(buffer) + "\" } ],\n");
			json.Append("  \"bufferViews\": [ { \"buffer\": 0, \"byteOffset\": 0, \"byteLength\": " + positions.Count + " }, { \"buffer\": 0, \"byteOffset\": " + positions.Count + ", \"byteLength\": " + normals.Count + " } ],\n");
			json.Append("  \"accessors\": [ { \"bufferView\": 0, \"componentType\": 5126, \"count\": " + count + ", \"type\": \"VEC3\", \"min\": [" + F(-hx) + ", 0, " + F(-hz) + "], \"max\": [" + F(hx) + ", " + F(h) + ", " + F(hz) + "] },\n");
			json.Append("                 { \"bufferView\": 1, \"componentType\": 5126, \"count\": " + count + ", \"type\": \"VEC3\" } ] }\n");
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			File.WriteAllText(path, json.ToString(), new UTF8Encoding(false));
		}

		private static float[] V(float x, float y, float z) => new[] { x, y, z };
	}
}
