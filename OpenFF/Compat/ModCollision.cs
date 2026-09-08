// The mod's own models as ground and walls. A Mesh marked Solid hands its triangles here in
// world fixed-point coordinates, and dgs.CRestrictor - the collision the characters ask,
// for the ground under their feet (an arrow down) and the walls in their way (a sphere
// along their step) - asks this after the map's own collision objects. The tests are the
// game's own primitives (ds.pri), so a step onto a glTF floor and a push off a glTF wall
// behave exactly as on the map's MCL: a polygon facing up is GROUND, one facing sideways
// is WALL_01, a ceiling is nothing. That is what a new map for the OpenFF target stands on:
// a glTF for the look and the same glTF for the floor.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace OpenFF.Client
{
	using XnaVector3 = Microsoft.Xna.Framework.Vector3;

	internal static class ModCollision
	{
		private sealed class Triangle
		{
			public GlobalScope.VecFx32 A, B, C, Normal;
			public GlobalScope.mcl.CMaterialData Material;
		}

		private sealed class Solid
		{
			public object Owner;
			public List<Triangle> Triangles = new List<Triangle>();
			public int MinX, MinZ, MaxX, MaxZ, MinY, MaxY;
		}

		private static readonly List<Solid> _solids = new List<Solid>();
		private static GlobalScope.mcl.CMaterialData _ground, _wall;

		private static void Materials()
		{
			if (_ground != null) return;
			// The attribute flags the map's polygons carry: GROUND (1) for a floor, WALL_01 (2) for a wall.
			_ground = new GlobalScope.mcl.CMaterialData();
			_wall = new GlobalScope.mcl.CMaterialData();
			SetFlag(_ground, (uint)GlobalScope.mcl.MCL_ATTRIBUTE.ATTRIBUTE_GROUND);
			SetFlag(_wall, (uint)GlobalScope.mcl.MCL_ATTRIBUTE.ATTRIBUTE_WALL_01);
		}

		private static void SetFlag(GlobalScope.mcl.CMaterialData material, uint flag)
		{
			// CAttributeData has no setter; its flags are a protected uint[] read by index >> 5.
			System.Reflection.FieldInfo field = typeof(GlobalScope.mcl.CAttributeData).GetField("m_auiFlags", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
			uint[] flags = (uint[])field.GetValue(material.getAttribute());
			flags[flag >> 5] |= 1u << (int)(flag & 31);
		}

		/// <summary>Replaces the owner's solid triangles: positions in world units, three per triangle (a posed model's vertices through its pose matrix).</summary>
		public static void Set(object owner, IEnumerable<XnaVector3> worldTriangles)
		{
			Materials();
			Remove(owner);
			Solid solid = new Solid { Owner = owner, MinX = int.MaxValue, MinZ = int.MaxValue, MinY = int.MaxValue, MaxX = int.MinValue, MaxZ = int.MinValue, MaxY = int.MinValue };
			XnaVector3[] tri = new XnaVector3[3];
			int n = 0;
			foreach (XnaVector3 v in worldTriangles)
			{
				tri[n++] = v;
				if (n < 3) continue;
				n = 0;
				XnaVector3 normal = XnaVector3.Cross(tri[1] - tri[0], tri[2] - tri[0]);
				if (normal.LengthSquared() < 1e-9f) continue;
				normal.Normalize();
				if (normal.Y < -0.5f) continue;   // a ceiling: nothing to stand on or bump into
				Triangle t = new Triangle
				{
					A = Fx(tri[0]), B = Fx(tri[1]), C = Fx(tri[2]),
					Normal = new GlobalScope.VecFx32((int)(normal.X * 4096), (int)(normal.Y * 4096), (int)(normal.Z * 4096)),
					Material = normal.Y > 0.5f ? _ground : _wall
				};
				solid.Triangles.Add(t);
				foreach (GlobalScope.VecFx32 p in new[] { t.A, t.B, t.C })
				{
					solid.MinX = Math.Min(solid.MinX, p.x); solid.MaxX = Math.Max(solid.MaxX, p.x);
					solid.MinY = Math.Min(solid.MinY, p.y); solid.MaxY = Math.Max(solid.MaxY, p.y);
					solid.MinZ = Math.Min(solid.MinZ, p.z); solid.MaxZ = Math.Max(solid.MaxZ, p.z);
				}
			}
			if (solid.Triangles.Count > 0) _solids.Add(solid);
		}

		public static void Remove(object owner)
		{
			_solids.RemoveAll(s => s.Owner == owner);
		}

		public static void Clear() => _solids.Clear();

		public static int Count => _solids.Count;

		private static GlobalScope.VecFx32 Fx(XnaVector3 v) => new GlobalScope.VecFx32((int)Math.Round(v.X * 4096), (int)Math.Round(v.Y * 4096), (int)Math.Round(v.Z * 4096));

		private static bool Near(Solid s, GlobalScope.VecFx32 p, int reach)
		{
			return p.x >= s.MinX - reach && p.x <= s.MaxX + reach && p.z >= s.MinZ - reach && p.z <= s.MaxZ + reach && p.y >= s.MinY - reach && p.y <= s.MaxY + reach;
		}

		private static bool Wants(Triangle t, int mat) => mat == -1 || t.Material.getAttribute().isEnableFlag((uint)mat);

		/// <summary>
		/// The arrow the ground query casts (mcl.CObject.evaluateArrowImp's arithmetic): the
		/// nearest solid triangle along it, taken when nearer than what the map found - ret.length
		/// is the distance to beat, the arrow's whole length when the map found nothing.
		/// </summary>
		public static bool Arrow(GlobalScope.VecFx32 pos, GlobalScope.VecFx32 dir, int len, int mat, GlobalScope.mcl.CollisionResult ret)
		{
			if (_solids.Count == 0) return false;
			bool hit = false;
			GlobalScope.VecFx32 toEdge = new GlobalScope.VecFx32(), toTri = new GlobalScope.VecFx32(), cross = new GlobalScope.VecFx32();
			GlobalScope.ds.pri.DSTriangle triangle = new GlobalScope.ds.pri.DSTriangle();
			foreach (Solid s in _solids)
			{
				if (!Near(s, pos, len)) continue;
				foreach (Triangle t in s.Triangles)
				{
					if (!Wants(t, mat)) continue;
					if (GlobalScope.VEC_DotProduct(dir, t.Normal) >= 0) continue;
					toEdge.x = GlobalScope.FX_Mul(dir.x, len); toEdge.y = GlobalScope.FX_Mul(dir.y, len); toEdge.z = GlobalScope.FX_Mul(dir.z, len);
					GlobalScope.VEC_Subtract(t.A, pos, toTri);
					int denom = GlobalScope.VEC_DotProduct(toEdge, t.Normal);
					int numer = GlobalScope.VEC_DotProduct(toTri, t.Normal);
					if (denom == 0) continue;
					int along = GlobalScope.FX_Mul32x64c(len, GlobalScope.FX_DivFx64c(numer, denom));
					if (along < 0 || along >= ret.length) continue;
					cross.x = pos.x + GlobalScope.FX_Mul(dir.x, along); cross.y = pos.y + GlobalScope.FX_Mul(dir.y, along); cross.z = pos.z + GlobalScope.FX_Mul(dir.z, along);
					triangle.set(t.A, t.B, t.C);
					if (!GlobalScope.ds.pri.PrimitiveTest.insidePointTriangle(cross, triangle)) continue;
					ret.hit = 1;
					ret.normal.copy(t.Normal);
					ret.length = along;
					ret.pos.copy(cross);
					ret.v0.copy(t.A); ret.v1.copy(t.B); ret.v2.copy(t.C);
					ret.material.copy(t.Material);
					hit = true;
				}
			}
			return hit;
		}

		/// <summary>The sphere the wall query pushes (mcl.CObject.evaluateSphereImp's arithmetic), when the map found nothing.</summary>
		public static bool Sphere(GlobalScope.VecFx32 center, GlobalScope.VecFx32 dir, int radius, int mat, GlobalScope.mcl.CollisionResult ret)
		{
			if (_solids.Count == 0) return false;
			bool any = false;
			int count = 0;
			GlobalScope.ds.pri.DSSphere sphere = new GlobalScope.ds.pri.DSSphere();
			GlobalScope.ds.pri.DSTriangle triangle = new GlobalScope.ds.pri.DSTriangle();
			GlobalScope.ds.pri.DSPlane plane = new GlobalScope.ds.pri.DSPlane();
			int[] sqLen = new int[1];
			foreach (Solid s in _solids)
			{
				if (!Near(s, center, radius * 2)) continue;
				foreach (Triangle t in s.Triangles)
				{
					if (!Wants(t, mat)) continue;
					if (GlobalScope.VEC_DotProduct(dir, t.Normal) > 0) continue;
					plane.set(t.A, t.B, t.C);
					if (!plane.isValidate()) continue;
					int distance = GlobalScope.ds.pri.PrimitiveTest.distPlanePoint(plane, center);
					sphere.set(center, radius);
					if (Math.Abs(distance) > radius || !GlobalScope.ds.pri.PrimitiveTest.testSphereHalfSpace(sphere, plane)) continue;
					// The game scales the test down by 32 to keep the fixed-point products in range.
					const int shift = 5;
					sphere.c.x = GlobalScope.FX_Div(center.x, 4096 << shift); sphere.c.y = GlobalScope.FX_Div(center.y, 4096 << shift); sphere.c.z = GlobalScope.FX_Div(center.z, 4096 << shift);
					sphere.r = GlobalScope.FX_Div(radius, 4096 << shift);
					triangle.set(Scaled(t.A, shift), Scaled(t.B, shift), Scaled(t.C, shift));
					if (!GlobalScope.ds.pri.PrimitiveTest.testSphereTriangle(sphere, triangle, sqLen, null)) continue;
					if (!any) { ret.clean(); }
					any = true;
					count += 4096;
					ret.hit = 1;
					GlobalScope.VEC_Add(t.Normal, ret.normal, ret.normal);
					ret.length += distance;
					ret.material.copy(t.Material);
				}
			}
			if (!any) return false;
			ret.normal.x = GlobalScope.FX_Div(ret.normal.x, count); ret.normal.y = GlobalScope.FX_Div(ret.normal.y, count); ret.normal.z = GlobalScope.FX_Div(ret.normal.z, count);
			GlobalScope.VEC_Normalize(ret.normal, ret.normal);
			ret.length = GlobalScope.FX_Div(ret.length, count);
			return true;
		}

		private static GlobalScope.VecFx32 Scaled(GlobalScope.VecFx32 v, int shift) => new GlobalScope.VecFx32(GlobalScope.FX_Div(v.x, 4096 << shift), GlobalScope.FX_Div(v.y, 4096 << shift), GlobalScope.FX_Div(v.z, 4096 << shift));
	}
}
