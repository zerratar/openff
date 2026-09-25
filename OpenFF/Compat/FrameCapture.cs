// The game's frame, kept, and drawn again - in between two of them, interpolated.
//
// The game runs thirty steps a second and draws as it goes: a part's logic and its draws
// are one pass, so there is no draw phase to call a second time. But every draw reaches
// the device through two doors - NativeRenderer.Draw and Clear for everything with
// geometry (the 3D world, the sprites, the windows, the cursor), Graphics.DrawString for
// the text - and this stands in both. While a step runs (Recording), a draw is written down
// instead of done: the primitive, its vertices copied into an arena, its matrices, texture
// and state, who drew it (Owner - the model, shadow, sprite or particle, which those draw
// sites say around their draw calls) and the camera it was drawn under; the text with its
// position and colour. Replay then draws the list, on every display frame until the next step.
//
// Smoothing stands the display between the step before (t 0) and the last one (t 1),
// FramePacer.Blend saying where. Once a step, each draw is paired with one of the step
// before's: the same owner's draw with the same primitive, texture and states, taken in the
// order they came (a queue of the step before's draws for each of those, so a list that
// gains or loses draws anywhere stays paired after it); a text with the nearest of the same
// words. A pair's matrices and vertex positions are interpolated, its colours when no channel
// changes by more than a quarter (a fade, not a flash), its texture coordinates when they all
// slide together (a scrolling surface). A pair is drawn as it is when it cannot be motion:
// its middle would cross over a third of the screen (a spawn), its vertices would shrink
// toward their pivot on the way (a turn too big for a straight line), it was not to be seen
// the step before, or the camera cut - the game saying so, the camera leaping, or a quarter
// of the 3D scene jumping at once. What cannot be interpolated - a draw only one of the steps
// has, a flash, a changed texture, the battle camera's shake - comes from the step nearer in
// time: the list before is drawn while t is under a half, the last one from then on - unless
// a texture the list before was drawn with has gone since (the game let it go during the
// step and drew its replacement), when the last one is drawn throughout.
// Nothing here changes what the game computes: the same steps run at the same rate; only
// what stands on the screen between them is new.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenFF.Client
{
	// MonoGame's types by name: the engine's OpenFF.Color and vectors sit a namespace up.
	using Color = Microsoft.Xna.Framework.Color;
	using Vector2 = Microsoft.Xna.Framework.Vector2;
	using Vector3 = Microsoft.Xna.Framework.Vector3;
	using Vector4 = Microsoft.Xna.Framework.Vector4;
	using Matrix = Microsoft.Xna.Framework.Matrix;
	using Quaternion = Microsoft.Xna.Framework.Quaternion;

	internal static class FrameCapture
	{
		/// <summary>Only the native renderer's draws come through the doors this stands in.</summary>
		public static bool Supported => NativeRenderer.Enabled;

		/// <summary>True while a step of the game runs and its draws are being written down.</summary>
		public static bool Recording { get; private set; }

		/// <summary>Whether a frame has been taken to draw.</summary>
		public static bool HasFrame => _current.Count > 0;

		/// <summary>The last replay's draws that were interpolated, and those paired but drawn as they were (for the overlay).</summary>
		public static int Blended, Snapped;

		/// <summary>Whether the last step's frame is a cut of the whole scene - the camera's (once a replay between the steps has paired it) or a part changing - drawn as one step has it; the mods' drawing over it follows (ModDrawBlend).</summary>
		public static bool SceneCut => _parted || (_prepared && _cutScene);

		/// <summary>
		/// What the replays drew since the last TakeWindowStats. Frames and Draws count every display
		/// frame and draw. Of the draws on frames between two steps, Blended were interpolated, Snapped
		/// were paired but drawn as they were (a spawn, a cut, a turn too big to blend) and Unmatched had
		/// nothing in the other step to pair with; a frame drawn right on a step (t 1, as always with
		/// smoothing off) adds to Frames and Draws only. Cuts counts the frames between steps that a
		/// cut of the whole scene stood in (the camera's, or a part changing); Stale, the frames between
		/// steps drawn from the last step before halfway because a texture of the step before had gone.
		/// </summary>
		public struct WindowStats
		{
			public int Frames, Draws, Blended, Snapped, Unmatched, Cuts, Stale;
		}

		private static WindowStats _window;

		/// <summary>The counts since the last call, which starts them again.</summary>
		public static WindowStats TakeWindowStats()
		{
			WindowStats taken = _window;
			_window = default;
			return taken;
		}

		// ---- who is drawing

		/// <summary>
		/// Who is drawing (a model, a shadow, a sprite, a particle), 0 for nobody in particular: set by
		/// those draw sites around their draw calls (Own), and part of each draw's pairing, so a draw pairs
		/// with the same thing's draw of the step before rather than with the next one that looks like it.
		/// </summary>
		public static int Owner { get; private set; }

		/// <summary>The draws until the returned scope is disposed are this object's - and this generation of it: a particle born again in its slot, or a sprite registered again, is a new thing.</summary>
		public static Owning Own(object who, int generation = 0)
		{
			Owning scope = new Owning(Owner);
			int id = who == null ? 0 : RuntimeHelpers.GetHashCode(who);
			if (who != null && generation != 0) id = HashCode.Combine(id, generation);
			Owner = who != null && id == 0 ? 1 : id;
			return scope;
		}

		/// <summary>Puts the owner back as it was before Own, so no draw after the scope inherits it.</summary>
		public readonly struct Owning : IDisposable
		{
			private readonly int _was;

			internal Owning(int was)
			{
				_was = was;
			}

			public void Dispose()
			{
				Owner = _was;
			}
		}

		// ---- what is kept

		private enum Kind : byte { Clear, Draw, TextBegin, Text, TextEnd }

		private struct Record
		{
			public Kind Kind;
			public int Key;             // what it is: primitive, count, texture and states; a text's words and size
			public int Owner;           // who drew it; 0, nobody said
			public bool Full;           // drawn into the whole back buffer: drawn again into the whole of it, whatever its size by then
			public Viewport Viewport;
			// Draw
			public uint Mode;
			public int First, Count;
			public Vector3 Centre;      // the vertices' middle, before World
			public int Camera;          // the frame's camera it was drawn under (Frame.Cameras)
			public Matrix World, Projection;
			public Texture2D Texture;
			public TextureFilter Filter;
			public TextureAddressMode AddressU, AddressV;
			public bool AlphaTest, DepthTest, DepthWrite, Cull;
			public float AlphaReference;
			public CompareFunction AlphaFunction, DepthFunction;
			public CullMode CullMode;
			public Blend DestinationBlend;
			// Clear
			public ClearOptions Options;
			public Color Colour;
			public float Depth;
			public int Stencil;
			// Text
			public string Text;
			public float X, Y, Rotation;
			public int Size;
			public Vector2 Origin, Scale;
			public SpriteEffects Flip;
		}

		/// <summary>The game's 3D camera as a step's draws saw it (NNS_G3dGlbLookAt).</summary>
		private struct Camera
		{
			public int Serial;          // a new one each time the camera changes; 0, no camera set yet
			public Vector3 Eye, Target, Up, Steady;   // in the game's units (fx32 / 4096); Steady, the eye without a shake
			public int Cuts;            // the cuts the game had made (CameraCut, the camera then set elsewhere) by the time this camera was set, its own among them
			public long WrapX, WrapY, WrapZ;   // how far the looping map had moved everything by then (CameraShift), in fx32
			public bool Shaken;
			public Matrix Shake;        // from the steady camera's view to this one's: the battle camera's shake
		}

		private sealed class Frame
		{
			public Record[] Records = new Record[2048];
			public int Count;
			public VertexPositionColorTexture[] Arena = new VertexPositionColorTexture[1 << 16];
			public int Used;
			public Camera[] Cameras = new Camera[8];
			public int CameraCount;

			public void Reset()
			{
				// The records hold textures and strings: they go with the frame.
				Array.Clear(Records, 0, Count);
				Count = 0;
				Used = 0;
				CameraCount = 0;
			}

			public ref Record Add()
			{
				if (Count == Records.Length) Array.Resize(ref Records, Count * 2);
				return ref Records[Count++];
			}

			public int Take(VertexPositionColorTexture[] source, int first, int count, out Vector3 centre)
			{
				if (Used + count > Arena.Length)
				{
					int size = Arena.Length;
					while (size < Used + count) size *= 2;
					Array.Resize(ref Arena, size);
				}
				Array.Copy(source, first, Arena, Used, count);
				Vector3 sum = Vector3.Zero;
				for (int v = Used, end = Used + count; v < end; v++) sum += Arena[v].Position;
				centre = sum / count;
				int at = Used;
				Used += count;
				return at;
			}

			public void AddCamera(in Camera camera)
			{
				if (CameraCount == Cameras.Length) Array.Resize(ref Cameras, CameraCount * 2);
				Cameras[CameraCount++] = camera;
			}
		}

		private static Frame _current = new Frame();
		private static Frame _previous = new Frame();
		private static VertexPositionColorTexture[] _scratch = new VertexPositionColorTexture[4096];
		private static GraphicsDevice _device;
		private static bool _parted;    // Cut() emptied the frame before: nothing to pair with until the next step
		private static bool _previousGone;  // a texture the frame before was drawn with has gone since: this step's frames are all the last one's

		/// <summary>A step is about to run: the last frame becomes the one before, and the draws go into a fresh one.</summary>
		public static void Begin()
		{
			Frame t = _previous;
			_previous = _current;
			_current = t;
			_current.Reset();
			// The camera the step starts under was set by the step before (the battle sets its camera at the
			// end of a step, for the next).
			_current.AddCamera(_camera);
			Owner = 0;
			_prepared = false;
			_parted = false;
			_previousGone = false;
			// A replay's viewport never reaches a step's recording (or the text's scale, read from it there).
			if (_device != null) SetViewport(_device, WholeOf(_device));
			Recording = true;
		}

		public static void End()
		{
			Recording = false;
		}

		/// <summary>The scene changed under the frames (a new map, a battle): the one before is no longer worth blending toward.</summary>
		public static void Cut()
		{
			_previous.Reset();
			_prepared = false;
			_parted = true;
			_previousGone = false;
		}

		// ---- the doors: what the game draws while a step runs

		public static void Clear(GraphicsDevice device, ClearOptions options, Color colour, float depth, int stencil)
		{
			_device = device;
			ref Record r = ref _current.Add();
			r.Kind = Kind.Clear;
			r.Viewport = device.Viewport;
			r.Full = IsWhole(device, r.Viewport);
			r.Options = options;
			r.Colour = colour;
			r.Depth = depth;
			r.Stencil = stencil;
		}

		public static void Draw(GraphicsDevice device, uint mode, VertexPositionColorTexture[] vertices, int first, int count,
			Matrix world, Matrix projection, Texture2D texture,
			TextureFilter filter, TextureAddressMode addressU, TextureAddressMode addressV,
			bool alphaTest, float alphaReference, CompareFunction alphaFunction,
			bool depthTest, bool depthWrite, CompareFunction depthFunction,
			bool cull, CullMode cullMode, Blend destinationBlend)
		{
			if (count <= 0 || vertices == null || first < 0 || first + count > vertices.Length) return;
			_device = device;
			int owner = Owner;
			// An owned 2D draw stays paired when its cell changes shape (a flipbook sprite: the same sprite,
			// another cell of other pieces); anything else is only the same draw with the same count.
			bool loose = owner != 0 && projection.M34 == 0f;
			ref Record r = ref _current.Add();
			r.Kind = Kind.Draw;
			r.Key = HashCode.Combine((int)mode, loose ? -1 : count, texture == null ? 0 : RuntimeHelpers.GetHashCode(texture), alphaTest, (int)destinationBlend, depthTest);
			r.Owner = owner;
			r.Viewport = device.Viewport;
			r.Full = IsWhole(device, r.Viewport);
			r.Mode = mode;
			r.First = _current.Take(vertices, first, count, out r.Centre);
			r.Count = count;
			r.Camera = _current.CameraCount - 1;
			r.World = world;
			r.Projection = projection;
			r.Texture = texture;
			r.Filter = filter;
			r.AddressU = addressU;
			r.AddressV = addressV;
			r.AlphaTest = alphaTest;
			r.AlphaReference = alphaReference;
			r.AlphaFunction = alphaFunction;
			r.DepthTest = depthTest;
			r.DepthWrite = depthWrite;
			r.DepthFunction = depthFunction;
			r.Cull = cull;
			r.CullMode = cullMode;
			r.DestinationBlend = destinationBlend;
		}

		public static void TextBegin(GraphicsDevice device)
		{
			_device = device;
			ref Record r = ref _current.Add();
			r.Kind = Kind.TextBegin;
			r.Viewport = device.Viewport;
			r.Full = IsWhole(device, r.Viewport);
		}

		public static void Text(string text, float x, float y, int size, Color colour, float rotation, Vector2 origin, Vector2 scale, SpriteEffects flip)
		{
			if (string.IsNullOrEmpty(text)) return;
			ref Record r = ref _current.Add();
			r.Kind = Kind.Text;
			r.Key = HashCode.Combine(text, size);
			r.Text = text;
			r.X = x;
			r.Y = y;
			r.Size = size;
			r.Colour = colour;
			r.Rotation = rotation;
			r.Origin = origin;
			r.Scale = scale;
			r.Flip = flip;
		}

		public static void TextEnd()
		{
			ref Record r = ref _current.Add();
			r.Kind = Kind.TextEnd;
		}

		// ---- the camera: cuts and shakes

		private static Camera _camera;
		private static int _cameraSerial;
		private static bool _cutDeclared;
		private static int _cuts;
		private static long _wrapX, _wrapY, _wrapZ;
		private static bool _shakePending;
		private static Vector3 _shakeSteady, _shakeEye;

		/// <summary>The game set its 3D camera (NNS_G3dGlbLookAt): kept with the draws that use it, to tell a cut from motion and a shake from the camera's path.</summary>
		public static void LookAt(GlobalScope.VecFx32 eye, GlobalScope.VecFx32 up, GlobalScope.VecFx32 target)
		{
			Vector3 e = Units(eye), g = Units(target), u = Units(up);
			bool shaken = false;
			Matrix shake = Matrix.Identity;
			Vector3 steady = e;
			if (_shakePending && !FreeCamera.Active && e == _shakeEye && _shakeSteady != e)
			{
				Matrix still = Matrix.CreateLookAt(_shakeSteady, g, u);
				Matrix now = Matrix.CreateLookAt(e, g, u);
				Matrix s = Matrix.Invert(still) * now;
				if (!float.IsNaN(s.M11) && !float.IsNaN(s.M41))
				{
					shaken = true;
					shake = s;
					steady = _shakeSteady;
				}
			}
			_shakePending = false;
			if (_camera.Serial != 0 && !_cutDeclared && e == _camera.Eye && g == _camera.Target && u == _camera.Up
				&& shaken == _camera.Shaken && (!shaken || shake == _camera.Shake)
				&& _wrapX == _camera.WrapX && _wrapY == _camera.WrapY && _wrapZ == _camera.WrapZ)
			{
				return;
			}
			Camera next = new Camera
			{
				Serial = ++_cameraSerial, Eye = e, Target = g, Up = u, Steady = steady,
				Cuts = _cuts, WrapX = _wrapX, WrapY = _wrapY, WrapZ = _wrapZ, Shaken = shaken, Shake = shake
			};
			if (_cutDeclared && Moved(_camera, next)) next.Cuts = ++_cuts;
			_camera = next;
			_cutDeclared = false;
			if (Recording) _current.AddCamera(_camera);
		}

		/// <summary>
		/// The game put its camera straight somewhere (an ability's close-up, the victory pose, a summon's shot,
		/// a scene's new shot, a script putting the field camera somewhere at once): the next camera set is a
		/// cut, unless it left the camera where it was. The cut holds for the draws made under any camera set
		/// after it against those made under one set before: a step of several of the game's frames (a catch-up,
		/// fast-forward) sets the camera in each of them, and draws only in the last.
		/// </summary>
		public static void CameraCut()
		{
			_cutDeclared = true;
		}

		/// <summary>
		/// The looping world map moved everything by so much at once - the party walked off one edge and on at
		/// the other (CWorldSystem.update) - and the camera with it: the next camera set stands that far from the
		/// last without having moved, and the scene in its view has not moved either.
		/// </summary>
		public static void CameraShift(GlobalScope.VecFx32 by)
		{
			if (by == null) return;
			_wrapX += by.x;
			_wrapY += by.y;
			_wrapZ += by.z;
		}

		/// <summary>
		/// The battle camera stands shaken this step (CBattleDisplay.doShakeCamera): where it would stand
		/// without the shake. The next camera set, if it is this shaken eye, is drawn as a steady camera's
		/// path with the shake of the nearer step on top - the shake keeps the hard cadence of its random
		/// offsets rather than being smoothed into a wobble, and nothing else loses its smoothing for it.
		/// </summary>
		public static void Shake(GlobalScope.VecFx32 steady, GlobalScope.VecFx32 shaken)
		{
			_shakeSteady = Units(steady);
			_shakeEye = Units(shaken);
			_shakePending = true;
		}

		private static Vector3 Units(GlobalScope.VecFx32 v) => v == null ? Vector3.Zero : new Vector3(v.x / 4096f, v.y / 4096f, v.z / 4096f);

		/// <summary>Whether the camera between two steps cut rather than moved: turned over 20 degrees, or its eye leapt three quarters of the way to what it looks at; when the game said it cut somewhere between the two, whenever the camera moved at all.</summary>
		private static bool Cutting(in Camera a, in Camera b)
		{
			if (b.Cuts != a.Cuts) return Moved(a, b);
			Vector3 fa = a.Target - a.Eye, fb = b.Target - b.Eye;
			float la = fa.Length(), lb = fb.Length();
			if (la < 1e-4f || lb < 1e-4f) return false;
			return Vector3.Dot(fa, fb) / (la * lb) < 0.9397f || Vector3.Distance(a.Steady + Wrapped(a, b), b.Steady) > 0.75f * Math.Max(4f, Math.Max(la, lb));
		}

		/// <summary>Whether the camera stands anywhere else: its eye half a unit away, or turned over 2 degrees.</summary>
		private static bool Moved(in Camera a, in Camera b)
		{
			Vector3 fa = a.Target - a.Eye, fb = b.Target - b.Eye;
			float la = fa.Length(), lb = fb.Length();
			if (la < 1e-4f || lb < 1e-4f) return false;
			return Vector3.Dot(fa, fb) / (la * lb) < 0.9994f || Vector3.Distance(a.Steady + Wrapped(a, b), b.Steady) > 0.5f;
		}

		/// <summary>How far the looping map moved everything, the camera with it, between two cameras: not the camera's own move.</summary>
		private static Vector3 Wrapped(in Camera a, in Camera b)
		{
			return new Vector3((b.WrapX - a.WrapX) / 4096f, (b.WrapY - a.WrapY) / 4096f, (b.WrapZ - a.WrapZ) / 4096f);
		}

		// ---- pairing, once a step

		[Flags]
		private enum How : byte
		{
			None = 0,
			Snap = 1,           // drawn as it is (from the nearer step)
			Vertices = 2,       // vertex positions interpolated
			Tint = 4,           // vertex colours interpolated
			Slide = 8,          // texture coordinates slid
			ShakeVertices = 16  // the shake is in the vertices (the game's own 3D draws), not in World
		}

		private struct Pair
		{
			public How How;
			public int Shake;           // _shakes, or -1
			public int Turn;            // _turns, or -1
			public Vector2 Slide;       // the texture coordinates' common slide from the step before to the last
		}

		private struct Shaking
		{
			public Matrix ToCur, ToPrev;    // the step before's view into the last one's shake, and back
		}

		private struct Turning
		{
			public Vector3 S0, T0, S1, T1;
			public Quaternion R0, R1;
		}

		private static bool _prepared;
		private static bool _cutScene;
		private static int[] _curToPrev = new int[2048], _prevToCur = new int[2048], _link = new int[2048], _chainFirst = new int[1024];
		private static Pair[] _pairs = new Pair[2048];
		private static readonly Dictionary<long, int> _chains = new Dictionary<long, int>(2048);
		private static Shaking[] _shakes = new Shaking[4];
		private static int _shakeCount, _shakeFrom, _shakeTo;
		private static Turning[] _turns = new Turning[16];
		private static int _turnCount;
		private static long _sceneWeight, _jumpedWeight;
		private static int _jumpedDraws;

		private static void Room<T>(ref T[] array, int size)
		{
			if (array.Length < size) Array.Resize(ref array, Math.Max(size, array.Length * 2));
		}

		private static long ChainKey(in Record r) => ((long)r.Key << 32) | (uint)r.Owner;

		/// <summary>
		/// Pairs the last step's draws and texts with the step before's, and decides how each pair is drawn:
		/// once a step, the first time a frame between the two is drawn. A pairing that fails leaves the step
		/// drawn unpaired - each frame from the nearer step, as it is - rather than blank, and is not tried (and
		/// failed) again on every frame of the step.
		/// </summary>
		private static void Prepare()
		{
			_prepared = true;
			Frame prev = _previous, cur = _current;
			try
			{
				Match(prev, cur);
			}
			catch (Exception ex)
			{
				_cutScene = false;
				_shakeCount = 0;
				_turnCount = 0;
				for (int i = Math.Min(cur.Count, _curToPrev.Length) - 1; i >= 0; i--) _curToPrev[i] = -1;
				for (int k = Math.Min(prev.Count, _prevToCur.Length) - 1; k >= 0; k--) _prevToCur[k] = -1;
				Log.First(LogChannel.General, "frame-prepare", 3, () => "frame: pairing failed, the step drawn unpaired: " + ex.GetType().Name + ": " + ex.Message);
			}
		}

		private static void Match(Frame prev, Frame cur)
		{
			_cutScene = false;
			_shakeCount = 0;
			_turnCount = 0;
			_sceneWeight = _jumpedWeight = 0;
			_jumpedDraws = 0;
			Room(ref _prevToCur, prev.Count);
			Room(ref _link, prev.Count);
			Room(ref _curToPrev, cur.Count);
			Room(ref _pairs, cur.Count);
			// A queue of the step before's draws for each owner and key, in their order: built from the
			// end so each queue's first is the earliest.
			_chains.Clear();
			int chains = 0;
			for (int k = prev.Count - 1; k >= 0; k--)
			{
				_prevToCur[k] = -1;
				ref Record p = ref prev.Records[k];
				if (p.Kind != Kind.Draw && p.Kind != Kind.Text) continue;
				long key = ChainKey(p);
				if (_chains.TryGetValue(key, out int c))
				{
					_link[k] = _chainFirst[c];
				}
				else
				{
					c = chains++;
					Room(ref _chainFirst, chains);
					_chains.Add(key, c);
					_link[k] = -1;
				}
				_chainFirst[c] = k;
			}
			for (int i = 0; i < cur.Count; i++)
			{
				_curToPrev[i] = -1;
				_pairs[i] = new Pair { Shake = -1, Turn = -1 };
				ref Record r = ref cur.Records[i];
				if (r.Kind != Kind.Draw && r.Kind != Kind.Text) continue;
				if (!_chains.TryGetValue(ChainKey(r), out int chain)) continue;
				int k = r.Kind == Kind.Text ? NearestText(prev, chain, r) : FirstDraw(prev, chain, r);
				if (k < 0) continue;
				_curToPrev[i] = k;
				_prevToCur[k] = i;
				if (r.Kind == Kind.Text)
				{
					JudgeText(prev.Records[k], r, ref _pairs[i]);
					if (Relabelled(prev, prev.Records[k], r)) _pairs[i].How = How.Snap;
				}
				else JudgeDraw(prev, k, cur, i, ref _pairs[i]);
			}
			// A quarter of the 3D scene (by its vertices) jumping at once is the camera cutting, not a
			// quarter of the scene moving: the rest of it goes as it is too, not a frame torn between two views.
			if (_jumpedDraws >= 4 && _jumpedWeight * 4 >= _sceneWeight)
			{
				_cutScene = true;
				for (int i = 0; i < cur.Count; i++)
				{
					if (_curToPrev[i] >= 0 && cur.Records[i].Kind == Kind.Draw && cur.Records[i].Projection.M34 != 0f) _pairs[i].How = How.Snap;
				}
			}
		}

		/// <summary>The first of the step before's draws still unpaired in the queue that is this one.</summary>
		private static int FirstDraw(Frame prev, int chain, in Record r)
		{
			int k = _chainFirst[chain];
			while (k >= 0 && _prevToCur[k] >= 0) k = _link[k];
			_chainFirst[chain] = k;
			for (; k >= 0; k = _link[k])
			{
				if (_prevToCur[k] < 0 && SameDraw(prev.Records[k], r)) return k;
			}
			return -1;
		}

		private static bool SameDraw(in Record p, in Record r)
		{
			return p.Kind == Kind.Draw && p.Owner == r.Owner && p.Mode == r.Mode && p.Texture == r.Texture
				&& p.AlphaTest == r.AlphaTest && p.DestinationBlend == r.DestinationBlend && p.DepthTest == r.DepthTest
				&& (p.Count == r.Count || (r.Owner != 0 && r.Projection.M34 == 0f && p.Projection.M34 == 0f));
		}

		/// <summary>
		/// The nearest unpaired text of the same words and size. Text has no owner to go by - the game
		/// hands its text slots out afresh as windows are redrawn - so among several copies of the same
		/// words (a count on every row) the nearest is the one; one further than a line and a half away,
		/// with another copy somewhere else as likely, is not taken at all.
		/// </summary>
		private static int NearestText(Frame prev, int chain, in Record r)
		{
			int k = _chainFirst[chain];
			while (k >= 0 && _prevToCur[k] >= 0) k = _link[k];
			_chainFirst[chain] = k;
			int best = -1, seen = 0;
			float bestD = float.MaxValue;
			for (int at = k; at >= 0 && seen < 64; at = _link[at])
			{
				if (_prevToCur[at] >= 0 || !SameText(prev.Records[at], r)) continue;
				seen++;
				float dx = prev.Records[at].X - r.X, dy = prev.Records[at].Y - r.Y, d = dx * dx + dy * dy;
				if (d < bestD)
				{
					bestD = d;
					best = at;
				}
			}
			float line = 1.5f * Math.Max(1, r.Size);
			if (best >= 0 && seen > 1 && bestD > line * line)
			{
				// A copy a few pixels from the best is the same text's shadow (drawn twice, one pixel apart), not another row.
				float bx = prev.Records[best].X, by = prev.Records[best].Y;
				seen = 0;
				for (int at = k; at >= 0 && seen < 64; at = _link[at])
				{
					if (at == best || _prevToCur[at] >= 0 || !SameText(prev.Records[at], r)) continue;
					seen++;
					float dx = prev.Records[at].X - bx, dy = prev.Records[at].Y - by;
					if (dx * dx + dy * dy > 16f) return -1;
				}
			}
			return best;
		}

		private static bool SameText(in Record p, in Record r) => p.Kind == Kind.Text && p.Size == r.Size && string.Equals(p.Text, r.Text);

		/// <summary>
		/// Words that moved into a place other words held the step before: a list scrolled (the battle's
		/// commands, a menu's rows) and its slots were relabelled, not text travelling - drawn between the two
		/// places it flies through the rows. Text that slides (a window opening) moves into a place nothing held.
		/// </summary>
		private static bool Relabelled(Frame prev, in Record p, in Record r)
		{
			if (Math.Abs(p.X - r.X) <= 2f && Math.Abs(p.Y - r.Y) <= 2f) return false;
			for (int at = 0; at < prev.Count; at++)
			{
				ref Record q = ref prev.Records[at];
				if (q.Kind != Kind.Text || q.Size != r.Size || string.Equals(q.Text, r.Text)) continue;
				if (Math.Abs(q.X - r.X) <= 2f && Math.Abs(q.Y - r.Y) <= 2f) return true;
			}
			return false;
		}

		private static void JudgeText(in Record p, in Record r, ref Pair pair)
		{
			// Text that moved a screen's width has not slid there: a page turned.
			if (Math.Abs(p.X - r.X) >= 240f || Math.Abs(p.Y - r.Y) >= 160f)
			{
				pair.How = How.Snap;
				return;
			}
			int tint = Delta(p.Colour, r.Colour);
			if (tint > 0 && tint <= 64) pair.How |= How.Tint;
		}

		private static int Delta(Color a, Color b)
		{
			return Math.Max(Math.Max(Math.Abs(a.R - b.R), Math.Abs(a.G - b.G)), Math.Max(Math.Abs(a.B - b.B), Math.Abs(a.A - b.A)));
		}

		private static void JudgeDraw(Frame prev, int k, Frame cur, int i, ref Pair pair)
		{
			ref Record p = ref prev.Records[k];
			ref Record r = ref cur.Records[i];
			// Not to be seen the step before (a particle dead in its slot, drawn at alpha 0 where it died): not moving from there.
			// Nor a matrix that is not all numbers (a mod's mesh given a NaN or infinite pose): nothing to move between, or
			// to take apart (Decompose throws on it), and no part of the scene's vote on a cut.
			if (Unseen(prev.Arena, p.First, p.Count) || !Finite(p.World) || !Finite(r.World) || !Finite(p.Projection) || !Finite(r.Projection))
			{
				pair.How = How.Snap;
				return;
			}
			bool deep = r.Projection.M34 != 0f;
			bool shaken = false, inVertices = false;
			Matrix toCur = Matrix.Identity;
			if (deep && p.Camera >= 0 && p.Camera < prev.CameraCount && r.Camera >= 0 && r.Camera < cur.CameraCount)
			{
				ref Camera a = ref prev.Cameras[p.Camera];
				ref Camera b = ref cur.Cameras[r.Camera];
				if (a.Serial != 0 && b.Serial != 0 && a.Serial != b.Serial)
				{
					if (Cutting(a, b))
					{
						pair.How = How.Snap;
						_cutScene = true;
						return;
					}
					if (a.Shaken || b.Shaken)
					{
						pair.Shake = ShakeBetween(prev, p.Camera, cur, r.Camera);
						toCur = _shakes[pair.Shake].ToCur;
						shaken = true;
						// The game's own 3D draws carry the camera in their vertices (World is identity); a glued-in mesh carries it in World.
						inVertices = p.World == Matrix.Identity && r.World == Matrix.Identity;
						if (inVertices) pair.How |= How.ShakeVertices;
					}
				}
			}
			Vector3 from = p.Centre;
			Matrix fromWorld = p.World;
			if (shaken)
			{
				if (inVertices) from = Vector3.Transform(from, toCur);
				else fromWorld = p.World * toCur;
			}
			bool jumped = Jumped(from, fromWorld, p.Projection, r.Centre, r.World, r.Projection);
			if (deep)
			{
				_sceneWeight += r.Count;
				if (jumped)
				{
					_jumpedWeight += r.Count;
					_jumpedDraws++;
				}
			}
			if (jumped)
			{
				pair.How = How.Snap;
				return;
			}
			if (p.Count == r.Count) Compare(prev.Arena, p.First, cur.Arena, r.First, r.Count, deep, inVertices, toCur, ref pair);
			if ((pair.How & How.Snap) != 0) return;
			if (fromWorld != r.World) Turn(fromWorld, r.World, ref pair);
		}

		private static bool Unseen(VertexPositionColorTexture[] arena, int first, int count)
		{
			for (int v = first, end = first + count; v < end; v++)
			{
				if (arena[v].Color.A != 0) return false;
			}
			return true;
		}

		private static bool Finite(in Matrix m)
		{
			return float.IsFinite(m.M11) && float.IsFinite(m.M12) && float.IsFinite(m.M13) && float.IsFinite(m.M14)
				&& float.IsFinite(m.M21) && float.IsFinite(m.M22) && float.IsFinite(m.M23) && float.IsFinite(m.M24)
				&& float.IsFinite(m.M31) && float.IsFinite(m.M32) && float.IsFinite(m.M33) && float.IsFinite(m.M34)
				&& float.IsFinite(m.M41) && float.IsFinite(m.M42) && float.IsFinite(m.M43) && float.IsFinite(m.M44);
		}

		/// <summary>Whether the draw's middle crosses more than a third of the screen between the two steps (or goes behind the eye): a cut or a spawn, not motion.</summary>
		private static bool Jumped(Vector3 pc, in Matrix pw, in Matrix pp, Vector3 rc, in Matrix rw, in Matrix rp)
		{
			Vector4 a = Vector4.Transform(new Vector4(pc, 1f), pw * pp);
			Vector4 b = Vector4.Transform(new Vector4(rc, 1f), rw * rp);
			bool behindA = a.W <= 1e-5f, behindB = b.W <= 1e-5f;
			if (behindA || behindB) return behindA != behindB;
			float dx = a.X / a.W - b.X / b.W, dy = a.Y / a.W - b.Y / b.W;
			return dx * dx + dy * dy > 0.66f * 0.66f;
		}

		/// <summary>
		/// Vertex by vertex: whether the positions can be interpolated (always in 3D; in 2D only when the
		/// texels stayed - otherwise another cell has come in, and only World is), the colours (no channel
		/// changing by more than a quarter over the whole draw), the texture coordinates (all sliding
		/// together, a little); and whether the straight-line middle would shrink toward its pivot - a
		/// turn too big to blend, drawn as it is.
		/// </summary>
		private static void Compare(VertexPositionColorTexture[] a, int af, VertexPositionColorTexture[] b, int bf, int n,
			bool deep, bool shaken, in Matrix toCur, ref Pair pair)
		{
			Vector2 slide = b[bf].TextureCoordinate - a[af].TextureCoordinate;
			bool still = true, together = true;
			int tint = 0;
			Vector3 a0 = shaken ? Vector3.Transform(a[af].Position, toCur) : a[af].Position, b0 = b[bf].Position;
			// Spreads as offsets from the first vertex, in doubles: view-space positions are far from their own middle.
			double sax = 0, say = 0, saz = 0, saa = 0, sbx = 0, sby = 0, sbz = 0, sbb = 0, smx = 0, smy = 0, smz = 0, smm = 0;
			for (int v = 0; v < n; v++)
			{
				ref VertexPositionColorTexture va = ref a[af + v];
				ref VertexPositionColorTexture vb = ref b[bf + v];
				Vector2 d = vb.TextureCoordinate - va.TextureCoordinate;
				if (d.X != 0f || d.Y != 0f) still = false;
				if (Math.Abs(d.X - slide.X) > 1e-4f || Math.Abs(d.Y - slide.Y) > 1e-4f) together = false;
				tint = Math.Max(tint, Delta(va.Color, vb.Color));
				Vector3 pa = (shaken ? Vector3.Transform(va.Position, toCur) : va.Position) - a0;
				Vector3 pb = vb.Position - b0;
				Vector3 pm = (pa + pb) * 0.5f;
				sax += pa.X; say += pa.Y; saz += pa.Z; saa += pa.X * pa.X + pa.Y * pa.Y + pa.Z * pa.Z;
				sbx += pb.X; sby += pb.Y; sbz += pb.Z; sbb += pb.X * pb.X + pb.Y * pb.Y + pb.Z * pb.Z;
				smx += pm.X; smy += pm.Y; smz += pm.Z; smm += pm.X * pm.X + pm.Y * pm.Y + pm.Z * pm.Z;
			}
			bool slides = !still && together && Math.Abs(slide.X) < 0.03f && Math.Abs(slide.Y) < 0.03f;
			if (deep || still || slides) pair.How |= How.Vertices;
			if (slides)
			{
				pair.How |= How.Slide;
				pair.Slide = slide;
			}
			if (tint > 0 && tint <= 64) pair.How |= How.Tint;
			if ((pair.How & How.Vertices) != 0 && n >= 3)
			{
				double nn = (double)n * n;
				double spreadA = saa / n - (sax * sax + say * say + saz * saz) / nn;
				double spreadB = sbb / n - (sbx * sbx + sby * sby + sbz * sbz) / nn;
				double spreadM = smm / n - (smx * smx + smy * smy + smz * smz) / nn;
				double least = Math.Min(spreadA, spreadB);
				// 0.95 of the size, squared: a turn of about 36 degrees in a step.
				if (least > 1e-10 && spreadM < 0.9025 * least) pair.How = How.Snap;
			}
		}

		/// <summary>A World that turns far in a step would shrink under a straight interpolation too: its rotation is interpolated as a rotation instead, or the draw goes as it is when World is not a plain scale, turn and move.</summary>
		private static void Turn(in Matrix a, in Matrix b, ref Pair pair)
		{
			float least = Math.Min(Basis(a), Basis(b));
			if (Basis(Matrix.Lerp(a, b, 0.5f)) >= 0.96f * least) return;
			Matrix ma = a, mb = b;
			if (ma.Decompose(out Vector3 s0, out Quaternion r0, out Vector3 t0) && mb.Decompose(out Vector3 s1, out Quaternion r1, out Vector3 t1))
			{
				Turning turning = new Turning { S0 = s0, R0 = r0, T0 = t0, S1 = s1, R1 = r1, T1 = t1 };
				// Only when taking it apart gives the same matrices back (a mirrored one may not).
				if (Near(Compose(turning, 0f), a) && Near(Compose(turning, 1f), b))
				{
					Room(ref _turns, _turnCount + 1);
					_turns[_turnCount] = turning;
					pair.Turn = _turnCount++;
					return;
				}
			}
			pair.How = How.Snap;
		}

		private static float Basis(in Matrix m)
		{
			return m.M11 * m.M11 + m.M12 * m.M12 + m.M13 * m.M13
				+ m.M21 * m.M21 + m.M22 * m.M22 + m.M23 * m.M23
				+ m.M31 * m.M31 + m.M32 * m.M32 + m.M33 * m.M33;
		}

		private static Matrix Compose(in Turning turning, float t)
		{
			return Matrix.CreateScale(Vector3.Lerp(turning.S0, turning.S1, t))
				* Matrix.CreateFromQuaternion(Quaternion.Slerp(turning.R0, turning.R1, t))
				* Matrix.CreateTranslation(Vector3.Lerp(turning.T0, turning.T1, t));
		}

		private static bool Near(in Matrix a, in Matrix b)
		{
			float scale = 1e-3f * (1f + Math.Max(Math.Abs(b.M41), Math.Max(Math.Abs(b.M42), Math.Abs(b.M43))));
			return Math.Abs(a.M11 - b.M11) < 1e-3f && Math.Abs(a.M12 - b.M12) < 1e-3f && Math.Abs(a.M13 - b.M13) < 1e-3f
				&& Math.Abs(a.M21 - b.M21) < 1e-3f && Math.Abs(a.M22 - b.M22) < 1e-3f && Math.Abs(a.M23 - b.M23) < 1e-3f
				&& Math.Abs(a.M31 - b.M31) < 1e-3f && Math.Abs(a.M32 - b.M32) < 1e-3f && Math.Abs(a.M33 - b.M33) < 1e-3f
				&& Math.Abs(a.M41 - b.M41) < scale && Math.Abs(a.M42 - b.M42) < scale && Math.Abs(a.M43 - b.M43) < scale;
		}

		/// <summary>The shake between two cameras: the step before's view carried into the last step's shake (and back), so what is interpolated is the steady camera's path.</summary>
		private static int ShakeBetween(Frame prev, int from, Frame cur, int to)
		{
			if (_shakeCount > 0 && _shakeFrom == from && _shakeTo == to) return _shakeCount - 1;
			ref Camera a = ref prev.Cameras[from];
			ref Camera b = ref cur.Cameras[to];
			Matrix toCur = Matrix.Invert(a.Shaken ? a.Shake : Matrix.Identity) * (b.Shaken ? b.Shake : Matrix.Identity);
			Room(ref _shakes, _shakeCount + 1);
			_shakes[_shakeCount] = new Shaking { ToCur = toCur, ToPrev = Matrix.Invert(toCur) };
			_shakeFrom = from;
			_shakeTo = to;
			return _shakeCount++;
		}

		// ---- drawing the frame

		private static int _drawn, _blended, _snapped, _unmatched;

		/// <summary>Draws the kept frame: at t 1 as the last step drew it, under 1 part of the way back toward the step before (t 0).</summary>
		public static void Replay(GraphicsDevice device, GlobalScope.Graphics graphics, float t)
		{
			bool inText = false;
			_device = device;
			try
			{
				Replay(device, graphics, t, ref inText);
			}
			catch (Exception ex)
			{
				Log.First(LogChannel.General, "frame-replay", 3, () => "frame: replay failed: " + ex.GetType().Name + ": " + ex.Message);
			}
			finally
			{
				// A throw between the text's Begin and End would leave the batch open for everything drawn after.
				if (inText && graphics != null) { try { graphics.DrawStringEnd(); } catch (Exception) { } }
				// The replay's viewports stop with it: the overlay, the mods' drawing and the next step start from the whole back buffer.
				try { SetViewport(device, WholeOf(device)); } catch (Exception) { }
			}
		}

		private static void Replay(GraphicsDevice device, GlobalScope.Graphics graphics, float blend, ref bool inText)
		{
			float t = float.IsNaN(blend) ? 1f : MathHelper.Clamp(blend, 0f, 1f);
			Frame prev = _previous;
			bool between = t < 0.999f;
			bool lerp = between && prev.Count > 0;
			if (lerp && !_prepared) Prepare();
			// What cannot be interpolated comes from the nearer step: the list before, until halfway - unless a
			// texture it was drawn with has gone since. The game let it go during the last step and drew what
			// replaces it (a new face or background, a model's palette bound again), or took every texture away
			// when the window lost focus: the gone texture's draw cannot be drawn, and what stands in for it is
			// paired with nothing, in the last list only. So the step's frames all come from the last list - its
			// pairs interpolated as ever, what cannot be a little early, nothing missing for a frame.
			bool fromPrev = lerp && t < 0.5f;
			if (fromPrev && (_previousGone || (_previousGone = Gone(prev))))
			{
				fromPrev = false;
				_window.Stale++;
			}
			Frame near = fromPrev ? prev : _current;
			int[] partner = !lerp ? null : fromPrev ? _prevToCur : _curToPrev;
			Viewport whole = WholeOf(device);
			_drawn = _blended = _snapped = _unmatched = 0;
			for (int n = 0; n < near.Count; n++)
			{
				ref Record r = ref near.Records[n];
				switch (r.Kind)
				{
					case Kind.Clear:
						SetViewport(device, r.Full ? whole : r.Viewport);
						NativeRenderer.Clear(device, r.Options, r.Colour, r.Depth, r.Stencil);
						break;

					case Kind.TextBegin:
						SetViewport(device, r.Full ? whole : r.Viewport);
						if (graphics != null && !inText) { graphics.DrawStringStart(); inText = true; }
						break;

					case Kind.TextEnd:
						if (graphics != null && inText) { graphics.DrawStringEnd(); inText = false; }
						break;

					case Kind.Text:
					{
						if (graphics == null || !inText) break;
						float x = r.X, y = r.Y;
						Color colour = r.Colour;
						int f = partner == null ? -1 : partner[n];
						if (f >= 0 && (_pairs[fromPrev ? f : n].How & How.Snap) == 0)
						{
							ref Record p = ref (fromPrev ? ref r : ref prev.Records[f]);
							ref Record q = ref (fromPrev ? ref _current.Records[f] : ref r);
							x = MathHelper.Lerp(p.X, q.X, t);
							y = MathHelper.Lerp(p.Y, q.Y, t);
							if ((_pairs[fromPrev ? f : n].How & How.Tint) != 0) colour = Color.Lerp(p.Colour, q.Colour, t);
						}
						graphics.DrawStringAs(r.Text, x, y, r.Size, colour, r.Rotation, r.Origin, r.Scale, r.Flip);
						break;
					}

					case Kind.Draw:
						ReplayDraw(device, whole, between, near, n, partner == null ? -1 : partner[n], fromPrev, t);
						break;
				}
			}
			if (inText && graphics != null) { graphics.DrawStringEnd(); inText = false; }
			Blended = _blended;
			Snapped = _snapped;
			_window.Frames++;
			_window.Draws += _drawn;
			_window.Blended += _blended;
			_window.Snapped += _snapped;
			_window.Unmatched += _unmatched;
			if (between && (lerp ? _cutScene : _parted)) _window.Cuts++;
		}

		/// <summary>Whether a texture one of the frame's draws was drawn with has been let go (Dispose) since.</summary>
		private static bool Gone(Frame frame)
		{
			for (int k = 0; k < frame.Count; k++)
			{
				ref Record r = ref frame.Records[k];
				if (r.Kind == Kind.Draw && r.Texture != null && r.Texture.IsDisposed) return true;
			}
			return false;
		}

		/// <summary>One of the nearer step's draws: as it is, or with its pair in the other step interpolated to t.</summary>
		private static void ReplayDraw(GraphicsDevice device, in Viewport whole, bool between, Frame near, int n, int f, bool fromPrev, float t)
		{
			ref Record r = ref near.Records[n];
			if (r.Texture != null && r.Texture.IsDisposed) return;
			SetViewport(device, r.Full ? whole : r.Viewport);
			VertexPositionColorTexture[] vertices = near.Arena;
			int first = r.First;
			Matrix world = r.World, projection = r.Projection;
			_drawn++;
			if (between)
			{
				if (f < 0)
				{
					_unmatched++;
				}
				else if ((_pairs[fromPrev ? f : n].How & How.Snap) != 0)
				{
					_snapped++;
				}
				else
				{
					ref Pair pair = ref _pairs[fromPrev ? f : n];
					Frame prev = _previous, cur = _current;
					ref Record p = ref prev.Records[fromPrev ? n : f];
					ref Record q = ref cur.Records[fromPrev ? f : n];
					bool shaken = pair.Shake >= 0;
					bool shakeInWorld = shaken && (pair.How & How.ShakeVertices) == 0;
					if (pair.Turn >= 0)
					{
						world = Compose(_turns[pair.Turn], t);
					}
					else
					{
						Matrix from = shakeInWorld ? p.World * _shakes[pair.Shake].ToCur : p.World;
						world = Matrix.Lerp(from, q.World, t);
					}
					// The shake is the nearer step's: while the step before is drawn, back into its shake.
					if (shakeInWorld && fromPrev) world *= _shakes[pair.Shake].ToPrev;
					projection = Matrix.Lerp(p.Projection, q.Projection, t);
					if ((pair.How & (How.Vertices | How.Tint | How.Slide)) != 0 && p.Count == q.Count)
					{
						Room(ref _scratch, r.Count);
						VertexPositionColorTexture[] pa = prev.Arena, qa = cur.Arena;
						bool positions = (pair.How & How.Vertices) != 0;
						bool tint = (pair.How & How.Tint) != 0;
						bool slide = (pair.How & How.Slide) != 0;
						bool reshake = positions && (pair.How & How.ShakeVertices) != 0;
						Matrix toNear = reshake ? (fromPrev ? _shakes[pair.Shake].ToPrev : _shakes[pair.Shake].ToCur) : Matrix.Identity;
						Vector2 slideBy = fromPrev ? pair.Slide * t : pair.Slide * (t - 1f);
						for (int v = 0; v < r.Count; v++)
						{
							VertexPositionColorTexture vertex = vertices[first + v];
							if (positions)
							{
								Vector3 a = pa[p.First + v].Position, b = qa[q.First + v].Position;
								if (reshake)
								{
									if (fromPrev) b = Vector3.Transform(b, toNear);
									else a = Vector3.Transform(a, toNear);
								}
								vertex.Position = Vector3.Lerp(a, b, t);
							}
							if (tint) vertex.Color = Color.Lerp(pa[p.First + v].Color, qa[q.First + v].Color, t);
							if (slide) vertex.TextureCoordinate += slideBy;
							_scratch[v] = vertex;
						}
						vertices = _scratch;
						first = 0;
					}
					_blended++;
				}
			}
			NativeRenderer.Draw(device, r.Mode, vertices, first, r.Count, world, projection, r.Texture,
				r.Filter, r.AddressU, r.AddressV, r.AlphaTest, r.AlphaReference, r.AlphaFunction,
				r.DepthTest, r.DepthWrite, r.DepthFunction, r.Cull, r.CullMode, r.DestinationBlend);
		}

		// ---- the viewport

		/// <summary>The whole back buffer as it is now (a window resized since a step recorded is drawn at its new size).</summary>
		private static Viewport WholeOf(GraphicsDevice device)
		{
			PresentationParameters pp = device.PresentationParameters;
			return new Viewport(0, 0, pp.BackBufferWidth, pp.BackBufferHeight);
		}

		private static bool IsWhole(GraphicsDevice device, in Viewport v)
		{
			PresentationParameters pp = device.PresentationParameters;
			return v.X == 0 && v.Y == 0 && v.Width == pp.BackBufferWidth && v.Height == pp.BackBufferHeight;
		}

		private static void SetViewport(GraphicsDevice device, in Viewport v)
		{
			Viewport now = device.Viewport;
			if (now.X != v.X || now.Y != v.Y || now.Width != v.Width || now.Height != v.Height)
			{
				if (v.Width > 0 && v.Height > 0) device.Viewport = v;
			}
		}
	}
}
