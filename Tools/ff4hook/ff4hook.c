// ff4hook: a version.dll for the Steam FF4.exe's folder that lets a script drive the game and see it - for
// comparing OpenFF's FF4 against the real one, frame by frame, without touching the desktop's keyboard,
// mouse or focus.
//
// FF4.exe (32-bit, SDL2 + OpenGL) loads SDL2.dll, which loads VERSION.dll; Windows looks in the game's
// folder first, so this file is loaded there and passes all 17 of version.dll's functions to the system's.
// Inside FF4.exe only, it patches two of the game's imports from SDL2.dll:
//
//   SDL_PollEvent       keys from the command file are handed to the game as SDL key events (the game
//                       reads its keys only from events: Select is Return, Cancel Backspace, Menu Tab -
//                       FF4.ini); the window's focus-lost event is dropped, so a game set to pause in the
//                       background (PauseInBG = 1) keeps running behind other windows.
//   SDL_GL_SwapWindow   counts frames and, when asked, reads the finished frame back (glReadPixels) into
//                       a .bmp before it is shown.
//   SDL_RenderPresent   the same for the frames drawn with SDL's 2D renderer (the opening movie).
//
// Commands: lines appended to %TEMP%\ff4hook\cmd.txt, read every frame (Tools/ff4hook/ff4steam.py writes them):
//   key <enter|back|tab|esc|up|down|left|right|space> [frames]   pressed, released after [frames] (4)
//   shot <path.bmp>                                             the next frame
//   every <frames> <dir>                                        a frame every <frames> into <dir>\NNNNN.bmp (0 stops)
//   exec <index> <hex>                                          a script command run now, its operands the bytes given
//   endstate                                                    the field's running state ended: what is queued goes next
//   quit                                                        ends the process
// %TEMP%\ff4hook\status.txt is rewritten every 30 frames ("frame N"); hook.log says what was hooked.
//
// The game's own files are not touched. Remove version.dll (ff4steam.py uninstall) and the game is as shipped.

#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <GL/gl.h>

#pragma comment(lib, "opengl32.lib")
#pragma comment(lib, "user32.lib")

// ---- version.dll, passed on ----

static HMODULE g_version;
static FARPROC g_real[17];
static const char *g_names[17] = {
	"GetFileVersionInfoA", "GetFileVersionInfoByHandle", "GetFileVersionInfoExA", "GetFileVersionInfoExW",
	"GetFileVersionInfoSizeA", "GetFileVersionInfoSizeExA", "GetFileVersionInfoSizeExW", "GetFileVersionInfoSizeW",
	"GetFileVersionInfoW", "VerFindFileA", "VerFindFileW", "VerInstallFileA", "VerInstallFileW",
	"VerLanguageNameA", "VerLanguageNameW", "VerQueryValueA", "VerQueryValueW",
};

static void LoadVersion(void)
{
	char path[MAX_PATH];
	GetSystemDirectoryA(path, MAX_PATH);   // SysWOW64 for a 32-bit process
	strcat_s(path, MAX_PATH, "\\version.dll");
	g_version = LoadLibraryA(path);
	for (int i = 0; i < 17; i++) g_real[i] = g_version ? GetProcAddress(g_version, g_names[i]) : NULL;
}

#define PASS(i, name) __declspec(naked) void Pass_##name(void) { __asm { jmp dword ptr [g_real + i * 4] } }
PASS(0, GetFileVersionInfoA) PASS(1, GetFileVersionInfoByHandle) PASS(2, GetFileVersionInfoExA)
PASS(3, GetFileVersionInfoExW) PASS(4, GetFileVersionInfoSizeA) PASS(5, GetFileVersionInfoSizeExA)
PASS(6, GetFileVersionInfoSizeExW) PASS(7, GetFileVersionInfoSizeW) PASS(8, GetFileVersionInfoW)
PASS(9, VerFindFileA) PASS(10, VerFindFileW) PASS(11, VerInstallFileA) PASS(12, VerInstallFileW)
PASS(13, VerLanguageNameA) PASS(14, VerLanguageNameW) PASS(15, VerQueryValueA) PASS(16, VerQueryValueW)

// ---- files ----

static char g_dir[MAX_PATH], g_cmdPath[MAX_PATH], g_statusPath[MAX_PATH], g_logPath[MAX_PATH];

static void Log(const char *fmt, ...)
{
	FILE *f;
	if (fopen_s(&f, g_logPath, "a") != 0 || !f) return;
	va_list a; va_start(a, fmt); vfprintf(f, fmt, a); va_end(a);
	fputc('\n', f); fclose(f);
}

// ---- SDL, as much of it as this needs ----

#define SDL_WINDOWEVENT 0x200
#define SDL_KEYDOWN 0x300
#define SDL_KEYUP 0x301
#define SDL_WINDOWEVENT_FOCUS_LOST 13

typedef struct { unsigned type, timestamp, windowID; unsigned char state, repeat, pad2, pad3; int scancode, sym; unsigned short mod; unsigned unused; } KeyEvent;
typedef struct { unsigned type, timestamp, windowID; unsigned char event, pad1, pad2, pad3; int data1, data2; } WindowEvent;

typedef int (__cdecl *PollEventFn)(void *event);
typedef void (__cdecl *SwapWindowFn)(void *window);
typedef void (__cdecl *GetWindowSizeFn)(void *window, int *w, int *h);
typedef void (__cdecl *RenderPresentFn)(void *renderer);
typedef int (__cdecl *RendererOutputSizeFn)(void *renderer, int *w, int *h);
typedef int (__cdecl *RenderReadPixelsFn)(void *renderer, const void *rect, unsigned format, void *pixels, int pitch);
static PollEventFn g_pollEvent;
static SwapWindowFn g_swapWindow;
static GetWindowSizeFn g_getDrawableSize;
static RenderPresentFn g_renderPresent;
static RendererOutputSizeFn g_rendererOutputSize;
static RenderReadPixelsFn g_renderReadPixels;

static struct { const char *name; int scancode, sym; } g_keys[] = {
	{ "enter", 40, 13 }, { "back", 42, 8 }, { "tab", 43, 9 }, { "esc", 41, 27 }, { "space", 44, 32 },
	{ "right", 79, 0x4000004F }, { "left", 80, 0x40000050 }, { "down", 81, 0x40000051 }, { "up", 82, 0x40000052 },
};

// Key events waiting to be handed over, each from a frame on.
typedef struct { unsigned type; int scancode, sym; unsigned long long atFrame; } Pending;
static Pending g_queue[256];
static int g_queued;
static unsigned long long g_frame;
static int g_traceCalls;
static unsigned g_windowID = 1;
static CRITICAL_SECTION g_lock;

static void Queue(unsigned type, int scancode, int sym, unsigned long long atFrame)
{
	if (g_queued >= 256) return;
	g_queue[g_queued].type = type; g_queue[g_queued].scancode = scancode; g_queue[g_queued].sym = sym; g_queue[g_queued].atFrame = atFrame;
	g_queued++;
}

// ---- commands ----

static long g_cmdRead;
static char g_shotPath[MAX_PATH];
static int g_everyFrames;
static char g_everyDir[MAX_PATH];
static int g_everyCount;

// ---- the script trace: every event-script command FF4.exe runs ----
//
// The script engine calls its commands through a table of 500 function pointers in .rdata (0x5a8658 in the Steam
// build; the order is OpenFF's Shared/Script/ScriptOpsFf4.cs, every entry a babilCommand_*). Each entry is pointed at
// a stub of its own - push the index, jump to TraceEntry - which logs the command and goes on to the original with
// every register as it came. The engine (ScriptEngine: code at +8, position at +0xC, redo flag at +0x10 - its
// getWord / getDword, __fastcall) comes in ECX. A command that redoes itself frame after frame is logged once.

#define TABLE_ADDRESS 0x5a8658
#define TABLE_COUNT 500
#define TABLE_CHECK_INDEX 306          // babilCommand_CE_SetupCameraMotion
#define TABLE_CHECK_VALUE 0x4f7260

static void *g_origCommand[TABLE_COUNT];
static FILE *g_trace;
static void *g_lastEngine[16];
static unsigned g_lastPc[16], g_lastOp[16];


// The engine: ECX (__fastcall, as the operand readers take it) or else the first stack argument.
static unsigned char *Engine(unsigned char *a, unsigned char *b)
{
	unsigned char *c[2] = { a, b };
	for (int i = 0; i < 2; i++)
	{
		if (!c[i] || IsBadReadPtr(c[i], 0x14)) continue;
		unsigned char *code = *(unsigned char **)(c[i] + 8);
		unsigned pc = *(unsigned *)(c[i] + 0xc);
		if (code && pc < 0x1000000 && !IsBadReadPtr(code + pc, 48)) return c[i];
	}
	return NULL;
}

// autokey <command index> <frames>: Return pressed <frames> after the scripts reach that command (StartMessage, the
// line that waits for a key) at a place they were not at a moment before - OpenFF's --autokey does the same, so two
// runs move through a scene's lines on the same frames.
static int g_autoIndex = -1, g_autoFrames = 60;
static unsigned g_autoPc[64];
static unsigned long long g_autoSeen[64];
static void Queue(unsigned type, int scancode, int sym, unsigned long long atFrame);

static void AutoKey(unsigned index, unsigned pc)
{
	if ((int)index != g_autoIndex) return;
	int k, free = -1;
	for (k = 0; k < 64; k++)
	{
		if (g_autoSeen[k] && g_autoPc[k] == pc) break;
		if (free < 0 && (!g_autoSeen[k] || g_frame - g_autoSeen[k] > 600)) free = k;
	}
	int fresh = k == 64 || g_frame - g_autoSeen[k] > 2;
	if (k == 64) k = free >= 0 ? free : 0;
	g_autoPc[k] = pc;
	g_autoSeen[k] = g_frame;
	if (!fresh) return;
	Queue(0x300 /* SDL_KEYDOWN */, 40, 13, g_frame + g_autoFrames);
	Queue(0x301 /* SDL_KEYUP */, 40, 13, g_frame + g_autoFrames + 4);
	Log("frame %llu: autokey at %u, Return at %llu", g_frame, pc, g_frame + g_autoFrames);
}

static void __cdecl TraceCall(unsigned index, unsigned char *ecx, unsigned char *stackArg)
{
	if (!g_trace && g_autoIndex < 0) return;
	unsigned char *engine = Engine(ecx, stackArg);
	if (!engine) return;
	unsigned char *code = *(unsigned char **)(engine + 8);
	unsigned pc = *(unsigned *)(engine + 0xc);
	if (!code || IsBadReadPtr(code + pc, 48)) return;
	AutoKey(index, pc);
	if (!g_trace) return;
	// once per engine, position and command: a wait redone every frame is one line
	int slot = -1, empty = -1;
	for (int i = 0; i < 16; i++)
	{
		if (g_lastEngine[i] == engine) { slot = i; break; }
		if (!g_lastEngine[i] && empty < 0) empty = i;
	}
	if (slot < 0) slot = empty >= 0 ? empty : (int)(((size_t)engine >> 4) & 15);
	if (g_lastEngine[slot] == engine && g_lastPc[slot] == pc && g_lastOp[slot] == index) return;
	g_lastEngine[slot] = engine; g_lastPc[slot] = pc; g_lastOp[slot] = index;
	fprintf(g_trace, "%llu\t%p\t%u\t%u\t", g_frame, (void *)engine, index, pc);
	for (int i = 0; i < 48; i++) fprintf(g_trace, "%02x", code[pc + i]);
	fputc('\n', g_trace);
}

__declspec(naked) static void TraceEntry(void)
{
	__asm {
		pushad                       // esp -> 8 registers; the index above them, then the return address
		mov eax, [esp + 32]          // the index the stub pushed
		mov edx, [esp + 40]          // the first stack argument (above the return address)
		push edx
		push ecx                     // the engine, as the operand readers take it
		push eax
		call TraceCall
		add esp, 12
		popad
		push eax
		mov eax, [esp + 4]           // the index
		mov eax, [g_origCommand + eax * 4]
		mov [esp + 4], eax           // in its place, the original command
		pop eax
		ret                          // to it, with the caller's return address on top as if called directly
	}
}

static void WrapCommandTable(void)
{
	// The addresses are the file's (image base 0x400000); the exe may be loaded elsewhere (ASLR).
	size_t shift = (size_t)GetModuleHandleA(NULL) - 0x400000;
	void **table = (void **)(TABLE_ADDRESS + shift);
	if (IsBadReadPtr(table, TABLE_COUNT * 4) || (size_t)table[TABLE_CHECK_INDEX] != TABLE_CHECK_VALUE + shift)
	{
		Log("script table not where the Steam build keeps it (entry %d: %p) - no script trace", TABLE_CHECK_INDEX, IsBadReadPtr(table, TABLE_COUNT * 4) ? NULL : table[TABLE_CHECK_INDEX]);
		return;
	}
	unsigned char *stubs = (unsigned char *)VirtualAlloc(NULL, TABLE_COUNT * 10, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
	if (!stubs) return;
	DWORD old;
	VirtualProtect(table, TABLE_COUNT * 4, PAGE_READWRITE, &old);
	for (int i = 0; i < TABLE_COUNT; i++)
	{
		unsigned char *s = stubs + i * 10;
		g_origCommand[i] = table[i];
		s[0] = 0x68; *(unsigned *)(s + 1) = (unsigned)i;                                  // push i
		s[5] = 0xE9; *(int *)(s + 6) = (int)((unsigned char *)TraceEntry - (s + 10));     // jmp TraceEntry
		if (table[i]) table[i] = s;
	}
	VirtualProtect(table, TABLE_COUNT * 4, old, &old);
	FlushInstructionCache(GetCurrentProcess(), stubs, TABLE_COUNT * 10);
	Log("script table wrapped: %d commands at %p (the exe at %p)", TABLE_COUNT, (void *)table, (void *)GetModuleHandleA(NULL));
}

// ---- exec: one event-script command run as a script would run it (the test's own party, items, a battle) ----
//
// A ScriptEngine of our own - zeroed, its code (+8) our operand bytes, its position (+0xC) 0 - handed to the command's
// original function from the table, between two frames on the game's thread. The engine goes in ECX and on the stack
// both (the operand readers are __fastcall; the commands take a ScriptEngine&), the stack put back after whichever
// way the command returned. Only for the commands that read nothing of the engine but their operands - AddPartyPC,
// SubPartyPC, SetPlayerLevel, AddItem, SetPartyPCEquipItem, BootEventBattle, the flags.

static unsigned char g_execEngine[0x400];
static unsigned char g_execCode[256];

static void Exec(int index, const char *hex)
{
	if (index < 0 || index >= TABLE_COUNT || !g_origCommand[index])
	{
		Log("frame %llu: exec %d - no such command (or the table was not wrapped)", g_frame, index);
		return;
	}
	memset(g_execEngine, 0, sizeof g_execEngine);
	memset(g_execCode, 0, sizeof g_execCode);
	int n = 0;
	for (const char *h = hex; h[0] && h[1] && n < (int)sizeof g_execCode; h += 2)
	{
		unsigned v;
		if (sscanf_s(h, "%2x", &v) != 1) break;
		g_execCode[n++] = (unsigned char)v;
	}
	*(unsigned char **)(g_execEngine + 8) = g_execCode;
	*(unsigned *)(g_execEngine + 0xc) = 0;
	void *fn = g_origCommand[index];
	unsigned char *engine = g_execEngine;
	__asm {
		push esi
		mov esi, esp
		push engine
		mov ecx, engine
		call fn
		mov esp, esi
		pop esi
	}
	Log("frame %llu: exec %d with %d operand byte(s) %s - read %u", g_frame, index, n, hex, *(unsigned *)(g_execEngine + 0xc));
}

// ---- endstate: the field's running WorldState over, so the scheduler goes on to what is queued ----
//
// The world's root context is the global at 0x628d30 (the Steam build), its WorldStateScheduler at +0x2c:
// the queue's states at +0x10c (their count at +0x210), the running one at +0x214 (wssUpdate, 0x575e40). A state ends
// when its byte at +4 is set (WorldState::wsIsEnd, 0x4166d0); then wssUpdate finalizes it and starts the next queued.
// BootEventBattle only queues "world encount2" and "event encount set" behind the field's "world move", which does not
// end while the hero stands - in a script it is the event's own state that ends. So a battle from exec is this after it.

#define WORLD_ROOT 0x628d30

static void EndState(void)
{
	size_t shift = (size_t)GetModuleHandleA(NULL) - 0x400000;
	unsigned char **rootAt = (unsigned char **)(WORLD_ROOT + shift);
	unsigned char *root = IsBadReadPtr(rootAt, 4) ? NULL : *rootAt;
	unsigned char *scheduler = root && !IsBadReadPtr(root + 0x2c, 4) ? *(unsigned char **)(root + 0x2c) : NULL;
	if (!scheduler || IsBadReadPtr(scheduler, 0x220))
	{
		Log("frame %llu: endstate - no world scheduler (root %p)", g_frame, (void *)root);
		return;
	}
	unsigned char *running = *(unsigned char **)(scheduler + 0x214);
	int queued = *(int *)(scheduler + 0x210);
	if (!running || IsBadWritePtr(running + 4, 1))
	{
		Log("frame %llu: endstate - nothing running (%d queued)", g_frame, queued);
		return;
	}
	running[4] = 1;
	Log("frame %llu: endstate - state %p ended, %d queued", g_frame, (void *)running, queued);
}

// ---- the characters: what each of CCharacterMng's slots holds, read straight from the game's memory ----
//
// FF4.exe's characterMng (a global at 0x61e808; every call site loads it into ECX - CCharacterMng::getPosition,
// setPosition, getMotionIndex, isHidden): a count byte at +0, the slots at *(+4), 0x13cc bytes each. In a slot
// (CCharacterMng's own getters, disassembled): +0x1391 flags (1 in use, 8 hidden); +0xf0 the CMotSet (+0x108 the
// entry playing, -1 none; entries of 0x30 from +0x128, the motion number at +0x150 of the set plus 0x30 an entry,
// the animation's frame at *(+8) of it in 1/4096ths, its rate at +0x14); +0xe1c the CRenderObject (+0x8c the position, three fx32;
// +0xa4 the rotation, three u16; +0x20 the material alpha, 0..31); +0xf4c the transparency; +0x13a9 the model's name.

#define CHARMNG_ADDRESS 0x61e808
#define SLOT_SIZE 0x13cc

static FILE *g_chars;
static int g_charsEvery;

static unsigned char *CharacterManager(void)
{
	return (unsigned char *)(CHARMNG_ADDRESS + (size_t)GetModuleHandleA(NULL) - 0x400000);
}

static void WriteCharacters(void)
{
	unsigned char *mng = CharacterManager();
	if (!g_chars || IsBadReadPtr(mng, 8)) return;
	int count = mng[0];
	unsigned char *slots = *(unsigned char **)(mng + 4);
	if (!slots || IsBadReadPtr(slots, count * SLOT_SIZE)) return;
	for (int i = 0; i < count; i++)
	{
		unsigned char *c = slots + i * SLOT_SIZE;
		unsigned char flags = c[0x1391];
		if (!(flags & 1)) continue;
		unsigned char *mot = c + 0xf0, *ro = c + 0xe1c;
		int playing = *(int *)(mot + 0x108);
		int motion = playing >= 0 ? *(int *)(mot + 0x150 + playing * 0x30) : -1;
		// the animation's frame: behind the pointer at +8 of the entry's CAnimation (+0x14 is its rate)
		int frame = 0;
		if (playing >= 0)
		{
			int **current = (int **)(mot + 0x128 + playing * 0x30 + 8);
			if (!IsBadReadPtr(*current, 4)) frame = **current;
		}
		int *pos = (int *)(ro + 0x8c);
		unsigned short *rot = (unsigned short *)(ro + 0xa4);
		char name[24] = { 0 };
		memcpy(name, c + 0x13a9, 23);   // the model's name (found in a dump of the slots)
		for (int k = 0; k < 23 && name[k]; k++) if (name[k] < 32 || name[k] > 126) { name[k] = 0; break; }
		fprintf(g_chars, "%llu\t%d\t%d\t%.3f\t%.3f\t%.3f\t%u\t%u\t%u\t%d\t%.2f\t%d\t%d\t%s\n", g_frame, i, (flags & 8) ? 1 : 0,
			pos[0] / 4096.0, pos[1] / 4096.0, pos[2] / 4096.0, rot[0], rot[1], rot[2],
			motion, frame / 4096.0, ro[0x20], *(int *)(c + 0xf4c), name);
	}
}

// ---- the camera: NitroSystem's global one (NNS_G3dGlbGetCameraPos / Up / Target return 0x608cb8 / 0x608cc4 /
// 0x608cd0, three fx32 each; NNS_G3dGlbPerspective writes the projection, 4x4 fx32, at 0x608a80) ----

static FILE *g_camera;
static int g_cameraEvery;

static void WriteCamera(void)
{
	size_t shift = (size_t)GetModuleHandleA(NULL) - 0x400000;
	int *pos = (int *)(0x608cb8 + shift), *up = (int *)(0x608cc4 + shift), *target = (int *)(0x608cd0 + shift), *proj = (int *)(0x608a80 + shift);
	if (!g_camera || IsBadReadPtr(pos, 0x30) || IsBadReadPtr(proj, 64)) return;
	fprintf(g_camera, "%llu\t%.3f\t%.3f\t%.3f\t%.3f\t%.3f\t%.3f\t%.4f\t%.4f\t%.4f\t%.4f\t%.4f\n", g_frame,
		pos[0] / 4096.0, pos[1] / 4096.0, pos[2] / 4096.0, target[0] / 4096.0, target[1] / 4096.0, target[2] / 4096.0,
		up[0] / 4096.0, up[1] / 4096.0, up[2] / 4096.0, proj[0] / 4096.0, proj[5] / 4096.0);
}

// ---- joints: where each named joint of a model stands in the world. The game reads a joint back only once it is
// reserved (CRenderObject::getJntMtx, 0x45bf30): the render object (+0xe1c of a character slot) keeps 12 such
// joints at +0x194, 0x48 bytes each - the 4x3 matrix (fx32, the translation at +0x24), the node's name at +0x30,
// the flags at +0x44 (1 reserved, 2 filled in as the model is drawn). A joint asked for is reserved in a free one
// and read from the next frame on. ----

static FILE *g_joints;
static int g_jointsEvery;
static char g_jointsModel[32];
static char g_jointNames[12][20];
static int g_jointCount;

static void WriteJoints(void)
{
	unsigned char *mng = CharacterManager();
	if (!g_joints || IsBadReadPtr(mng, 8)) return;
	int count = mng[0];
	unsigned char *slots = *(unsigned char **)(mng + 4);
	if (!slots || IsBadReadPtr(slots, count * SLOT_SIZE)) return;
	for (int i = 0; i < count; i++)
	{
		unsigned char *c = slots + i * SLOT_SIZE;
		// '*' takes every character: one set up asynchronously keeps no name where +0x13a9 reads it, and a joint its
		// model has not is never filled in
		if (!(c[0x1391] & 1) || (strcmp(g_jointsModel, "*") != 0 && strncmp((const char *)(c + 0x13a9), g_jointsModel, sizeof g_jointsModel) != 0)) continue;
		unsigned char *jnt = c + 0xe1c + 0x194;
		for (int j = 0; j < g_jointCount; j++)
		{
			int found = -1, empty = -1;
			for (int k = 0; k < 12; k++)
			{
				unsigned char *e = jnt + k * 0x48;
				unsigned flags = *(unsigned *)(e + 0x44);
				if ((flags & 1) && strncmp((const char *)(e + 0x30), g_jointNames[j], 20) == 0) { found = k; break; }
				if (!(flags & 1) && empty < 0) empty = k;
			}
			if (found < 0)
			{
				if (empty >= 0)
				{
					unsigned char *e = jnt + empty * 0x48;
					memset(e + 0x30, 0, 20);
					strncpy_s((char *)(e + 0x30), 20, g_jointNames[j], _TRUNCATE);
					*(unsigned *)(e + 0x44) = 1;
				}
				continue;
			}
			unsigned char *e = jnt + found * 0x48;
			if (!(*(unsigned *)(e + 0x44) & 2)) continue;
			int *m = (int *)e;
			fprintf(g_joints, "%llu\t%d\t%s\t%s\t%.3f\t%.3f\t%.3f\n", g_frame, i, g_jointsModel, g_jointNames[j], m[9] / 4096.0, m[10] / 4096.0, m[11] / 4096.0);
		}
	}
}

static void DumpCharacters(const char *path)
{
	unsigned char *mng = CharacterManager();
	FILE *f;
	if (IsBadReadPtr(mng, 8) || fopen_s(&f, path, "wb") != 0 || !f) return;
	int count = mng[0];
	unsigned char *slots = *(unsigned char **)(mng + 4);
	if (slots && !IsBadReadPtr(slots, count * SLOT_SIZE)) fwrite(slots, SLOT_SIZE, count, f);
	fclose(f);
	Log("frame %llu: %d character slot(s) dumped to %s", g_frame, count, path);
}

static void Command(char *line)
{
	char word[32] = { 0 }, arg[MAX_PATH] = { 0 };
	int n = 0;
	if (sscanf_s(line, "%31s", word, (unsigned)sizeof word) != 1) return;
	char *rest = line + strlen(word);
	while (*rest == ' ') rest++;
	if (strcmp(word, "key") == 0)
	{
		int frames = 4;
		char name[32] = { 0 };
		sscanf_s(rest, "%31s %d", name, (unsigned)sizeof name, &frames);
		for (int i = 0; i < (int)(sizeof g_keys / sizeof g_keys[0]); i++)
		{
			if (_stricmp(name, g_keys[i].name) != 0) continue;
			Queue(SDL_KEYDOWN, g_keys[i].scancode, g_keys[i].sym, g_frame);
			Queue(SDL_KEYUP, g_keys[i].scancode, g_keys[i].sym, g_frame + (frames > 0 ? frames : 1));
			Log("frame %llu: key %s for %d frame(s)", g_frame, name, frames);
			return;
		}
		Log("frame %llu: unknown key '%s'", g_frame, name);
	}
	else if (strcmp(word, "shot") == 0)
	{
		strcpy_s(g_shotPath, MAX_PATH, rest);
	}
	else if (strcmp(word, "every") == 0)
	{
		if (sscanf_s(rest, "%d %259[^\r\n]", &n, arg, (unsigned)sizeof arg) >= 1)
		{
			g_everyFrames = n;
			if (arg[0]) { strcpy_s(g_everyDir, MAX_PATH, arg); CreateDirectoryA(g_everyDir, NULL); }
			g_everyCount = 0;
			Log("frame %llu: a frame every %d into %s", g_frame, n, g_everyDir);
		}
	}
	else if (strcmp(word, "trace") == 0)
	{
		if (g_trace) { fclose(g_trace); g_trace = NULL; }
		if (_stricmp(rest, "off") != 0 && rest[0])
		{
			fopen_s(&g_trace, rest, "a");
			memset(g_lastEngine, 0, sizeof g_lastEngine);
		}
		Log("frame %llu: script trace %s", g_frame, g_trace ? rest : "off");
	}
	else if (strcmp(word, "autokey") == 0)
	{
		int frames = 60;
		if (sscanf_s(rest, "%d %d", &n, &frames) >= 1) { g_autoIndex = n; g_autoFrames = frames; }
		Log("frame %llu: autokey on command %d, %d frame(s) after", g_frame, g_autoIndex, g_autoFrames);
	}
	else if (strcmp(word, "chars") == 0)
	{
		// chars <every N frames> <file>: each slot in use - frame, slot, hidden, x y z, rotation x y z, motion, frame, alpha, transparency
		if (g_chars) { fclose(g_chars); g_chars = NULL; }
		if (sscanf_s(rest, "%d %259[^\r\n]", &n, arg, (unsigned)sizeof arg) == 2 && n > 0)
		{
			g_charsEvery = n;
			fopen_s(&g_chars, arg, "a");
		}
		Log("frame %llu: characters %s", g_frame, g_chars ? arg : "off");
	}
	else if (strcmp(word, "camera") == 0)
	{
		// camera <every N frames> <file>: frame, position x y z, target x y z, up x y z, projection [0][0] and [1][1]
		if (g_camera) { fclose(g_camera); g_camera = NULL; }
		if (sscanf_s(rest, "%d %259[^\r\n]", &n, arg, (unsigned)sizeof arg) == 2 && n > 0)
		{
			g_cameraEvery = n;
			fopen_s(&g_camera, arg, "a");
		}
		Log("frame %llu: camera %s", g_frame, g_camera ? arg : "off");
	}
	else if (strcmp(word, "joints") == 0)
	{
		// joints <every N frames> <file> <model> <node,node,...>: each joint's world position for every character of that model
		char file[MAX_PATH] = { 0 }, model[32] = { 0 }, names[1024] = { 0 };
		if (g_joints) { fclose(g_joints); g_joints = NULL; }
		if (sscanf_s(rest, "%d %259s %31s %1023s", &n, file, (unsigned)sizeof file, model, (unsigned)sizeof model, names, (unsigned)sizeof names) == 4 && n > 0)
		{
			g_jointsEvery = n;
			strcpy_s(g_jointsModel, sizeof g_jointsModel, model);
			g_jointCount = 0;
			char *ctx = NULL;
			for (char *t = strtok_s(names, ",", &ctx); t && g_jointCount < 12; t = strtok_s(NULL, ",", &ctx)) strncpy_s(g_jointNames[g_jointCount++], 20, t, _TRUNCATE);
			fopen_s(&g_joints, file, "a");
		}
		Log("frame %llu: joints of %s %s", g_frame, g_jointsModel, g_joints ? file : "off");
	}
	else if (strcmp(word, "endstate") == 0)
	{
		EndState();
	}
	else if (strcmp(word, "exec") == 0)
	{
		if (sscanf_s(rest, "%d %259s", &n, arg, (unsigned)sizeof arg) >= 1) Exec(n, arg);
	}
	else if (strcmp(word, "dumpchars") == 0)
	{
		DumpCharacters(rest);
	}
	else if (strcmp(word, "quit") == 0)
	{
		if (g_trace) fclose(g_trace);
		if (g_chars) fclose(g_chars);
		if (g_camera) fclose(g_camera);
		if (g_joints) fclose(g_joints);
		Log("frame %llu: quit", g_frame);
		TerminateProcess(GetCurrentProcess(), 0);
	}
}

static void ReadCommands(void)
{
	HANDLE h = CreateFileA(g_cmdPath, GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE | FILE_SHARE_DELETE, NULL, OPEN_EXISTING, 0, NULL);
	if (h == INVALID_HANDLE_VALUE) return;
	DWORD size = GetFileSize(h, NULL);
	if (size < (DWORD)g_cmdRead) g_cmdRead = 0;   // the file was started over
	if (size > (DWORD)g_cmdRead && size - g_cmdRead < 65536)
	{
		char buf[65537];
		DWORD got = 0;
		SetFilePointer(h, g_cmdRead, NULL, FILE_BEGIN);
		ReadFile(h, buf, size - g_cmdRead, &got, NULL);
		buf[got] = 0;
		// Whole lines only; a line still being written waits for the next frame.
		char *end = strrchr(buf, '\n');
		if (end)
		{
			*end = 0;
			g_cmdRead += (long)(end - buf + 1);
			char *ctx = NULL;
			for (char *line = strtok_s(buf, "\r\n", &ctx); line; line = strtok_s(NULL, "\r\n", &ctx)) Command(line);
		}
	}
	CloseHandle(h);
}

// ---- the hooks ----

static DWORD g_lastRead;

static int __cdecl Hook_PollEvent(void *event)
{
	EnterCriticalSection(&g_lock);
	// Commands every 10 ms here too: the movie part draws with SDL's renderer, not by swapping.
	if (GetTickCount() - g_lastRead >= 10) { g_lastRead = GetTickCount(); ReadCommands(); }
	for (int i = 0; i < g_queued; i++)
	{
		if (g_queue[i].atFrame > g_frame) continue;
		if (event)
		{
			KeyEvent *k = (KeyEvent *)event;
			memset(event, 0, 56);
			k->type = g_queue[i].type;
			k->timestamp = GetTickCount();
			k->windowID = g_windowID;
			k->state = g_queue[i].type == SDL_KEYDOWN ? 1 : 0;
			k->scancode = g_queue[i].scancode;
			k->sym = g_queue[i].sym;
			memmove(&g_queue[i], &g_queue[i + 1], (g_queued - i - 1) * sizeof(Pending));
			g_queued--;
		}
		LeaveCriticalSection(&g_lock);
		return 1;
	}
	LeaveCriticalSection(&g_lock);
	for (;;)
	{
		int got = g_pollEvent(event);
		if (!got || !event) return got;
		WindowEvent *w = (WindowEvent *)event;
		if (w->type == SDL_WINDOWEVENT)
		{
			g_windowID = w->windowID;
			if (w->event == SDL_WINDOWEVENT_FOCUS_LOST) continue;   // keeps a game set to pause in the background running
		}
		return got;
	}
}

static void SaveFrame(void *window, const char *path)
{
	int w = 0, h = 0;
	if (g_getDrawableSize) g_getDrawableSize(window, &w, &h);
	if (w <= 0 || h <= 0)
	{
		GLint vp[4];
		glGetIntegerv(GL_VIEWPORT, vp);
		w = vp[2]; h = vp[3];
	}
	if (w <= 0 || h <= 0) return;
	int stride = (w * 3 + 3) & ~3;
	unsigned char *pixels = (unsigned char *)malloc((size_t)stride * h);
	if (!pixels) return;
	glPixelStorei(GL_PACK_ALIGNMENT, 4);
	glReadBuffer(GL_BACK);
	glReadPixels(0, 0, w, h, 0x80E0 /* GL_BGR */, GL_UNSIGNED_BYTE, pixels);   // bottom row first, as a .bmp keeps it
	FILE *f;
	if (fopen_s(&f, path, "wb") == 0 && f)
	{
		BITMAPFILEHEADER fh = { 0 };
		BITMAPINFOHEADER ih = { 0 };
		fh.bfType = 0x4D42;
		fh.bfOffBits = sizeof fh + sizeof ih;
		fh.bfSize = fh.bfOffBits + (DWORD)stride * h;
		ih.biSize = sizeof ih; ih.biWidth = w; ih.biHeight = h; ih.biPlanes = 1; ih.biBitCount = 24; ih.biSizeImage = (DWORD)stride * h;
		fwrite(&fh, sizeof fh, 1, f); fwrite(&ih, sizeof ih, 1, f); fwrite(pixels, 1, (size_t)stride * h, f);
		fclose(f);
	}
	free(pixels);
}

static void WriteBmp(const char *path, const unsigned char *pixels, int w, int h, int stride, int topDown)
{
	FILE *f;
	if (fopen_s(&f, path, "wb") != 0 || !f) return;
	BITMAPFILEHEADER fh = { 0 };
	BITMAPINFOHEADER ih = { 0 };
	fh.bfType = 0x4D42;
	fh.bfOffBits = sizeof fh + sizeof ih;
	fh.bfSize = fh.bfOffBits + (DWORD)stride * h;
	ih.biSize = sizeof ih; ih.biWidth = w; ih.biHeight = topDown ? -h : h; ih.biPlanes = 1; ih.biBitCount = 24; ih.biSizeImage = (DWORD)stride * h;
	fwrite(&fh, sizeof fh, 1, f); fwrite(&ih, sizeof ih, 1, f); fwrite(pixels, 1, (size_t)stride * h, f);
	fclose(f);
}

// The frame read back from SDL's 2D renderer (the movie part), top row first.
static void SaveRendererFrame(void *renderer, const char *path)
{
	int w = 0, h = 0;
	if (!g_rendererOutputSize || !g_renderReadPixels || g_rendererOutputSize(renderer, &w, &h) != 0 || w <= 0 || h <= 0) return;
	int stride = (w * 3 + 3) & ~3;
	unsigned char *pixels = (unsigned char *)malloc((size_t)stride * h);
	if (!pixels) return;
	if (g_renderReadPixels(renderer, NULL, 0x17101803 /* SDL_PIXELFORMAT_BGR24 */, pixels, stride) == 0) WriteBmp(path, pixels, w, h, stride, 1);
	free(pixels);
}

// Once a frame, whichever way it is shown: commands, the screenshots asked for, the status file.
static void Frame(void *target, int renderer)
{
	EnterCriticalSection(&g_lock);
	ReadCommands();
	g_lastRead = GetTickCount();
	LeaveCriticalSection(&g_lock);
	if (g_shotPath[0])
	{
		if (renderer) SaveRendererFrame(target, g_shotPath); else SaveFrame(target, g_shotPath);
		Log("frame %llu: shot %s%s", g_frame, g_shotPath, renderer ? " (renderer)" : "");
		g_shotPath[0] = 0;
	}
	if (g_everyFrames > 0 && g_everyDir[0] && g_frame % (unsigned long long)g_everyFrames == 0)
	{
		char path[MAX_PATH];
		sprintf_s(path, MAX_PATH, "%s\\%06llu.bmp", g_everyDir, g_frame);
		if (renderer) SaveRendererFrame(target, path); else SaveFrame(target, path);
		g_everyCount++;
	}
	if (g_trace) fflush(g_trace);
	if (g_joints && g_jointsEvery > 0 && g_frame % (unsigned long long)g_jointsEvery == 0) { WriteJoints(); fflush(g_joints); }
	if (g_camera && g_cameraEvery > 0 && g_frame % (unsigned long long)g_cameraEvery == 0) { WriteCamera(); fflush(g_camera); }
	if (g_chars && g_charsEvery > 0 && g_frame % (unsigned long long)g_charsEvery == 0) { WriteCharacters(); fflush(g_chars); }
	if (g_frame % 30 == 0)
	{
		FILE *f;
		if (fopen_s(&f, g_statusPath, "w") == 0 && f) { fprintf(f, "frame %llu\n", g_frame); fclose(f); }
	}
}

static void __cdecl Hook_SwapWindow(void *window)
{
	Frame(window, 0);
	g_swapWindow(window);
	g_frame++;
}

static void __cdecl Hook_RenderPresent(void *renderer)
{
	Frame(renderer, 1);
	g_renderPresent(renderer);
	g_frame++;
}

// FF4.exe's import of an SDL2 function pointed at the hook; the real one kept.
static void *PatchImport(HMODULE exe, const char *dll, const char *name, void *hook)
{
	BYTE *base = (BYTE *)exe;
	IMAGE_NT_HEADERS *nt = (IMAGE_NT_HEADERS *)(base + ((IMAGE_DOS_HEADER *)base)->e_lfanew);
	IMAGE_DATA_DIRECTORY dir = nt->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_IMPORT];
	for (IMAGE_IMPORT_DESCRIPTOR *d = (IMAGE_IMPORT_DESCRIPTOR *)(base + dir.VirtualAddress); d->Name; d++)
	{
		if (_stricmp((char *)(base + d->Name), dll) != 0) continue;
		IMAGE_THUNK_DATA *names = (IMAGE_THUNK_DATA *)(base + (d->OriginalFirstThunk ? d->OriginalFirstThunk : d->FirstThunk));
		IMAGE_THUNK_DATA *slots = (IMAGE_THUNK_DATA *)(base + d->FirstThunk);
		for (; names->u1.AddressOfData; names++, slots++)
		{
			if (IMAGE_SNAP_BY_ORDINAL(names->u1.Ordinal)) continue;
			IMAGE_IMPORT_BY_NAME *by = (IMAGE_IMPORT_BY_NAME *)(base + names->u1.AddressOfData);
			if (strcmp((char *)by->Name, name) != 0) continue;
			DWORD old;
			void *real = (void *)slots->u1.Function;
			VirtualProtect(&slots->u1.Function, sizeof(void *), PAGE_READWRITE, &old);
			slots->u1.Function = (ULONG_PTR)hook;
			VirtualProtect(&slots->u1.Function, sizeof(void *), old, &old);
			return real;
		}
	}
	return NULL;
}

static void Install(void)
{
	char exe[MAX_PATH];
	GetModuleFileNameA(NULL, exe, MAX_PATH);
	const char *file = strrchr(exe, '\\');
	file = file ? file + 1 : exe;
	if (_stricmp(file, "FF4.exe") != 0) return;   // the launcher and anything else: version.dll only

	GetTempPathA(MAX_PATH, g_dir);
	strcat_s(g_dir, MAX_PATH, "ff4hook");
	CreateDirectoryA(g_dir, NULL);
	sprintf_s(g_cmdPath, MAX_PATH, "%s\\cmd.txt", g_dir);
	sprintf_s(g_statusPath, MAX_PATH, "%s\\status.txt", g_dir);
	sprintf_s(g_logPath, MAX_PATH, "%s\\hook.log", g_dir);
	InitializeCriticalSection(&g_lock);

	// Commands written before the game started are not replayed.
	WIN32_FILE_ATTRIBUTE_DATA a;
	if (GetFileAttributesExA(g_cmdPath, GetFileExInfoStandard, &a)) g_cmdRead = (long)a.nFileSizeLow;

	HMODULE sdl = GetModuleHandleA("SDL2.dll");
	if (sdl)
	{
		g_getDrawableSize = (GetWindowSizeFn)GetProcAddress(sdl, "SDL_GL_GetDrawableSize");
		g_rendererOutputSize = (RendererOutputSizeFn)GetProcAddress(sdl, "SDL_GetRendererOutputSize");
		g_renderReadPixels = (RenderReadPixelsFn)GetProcAddress(sdl, "SDL_RenderReadPixels");
	}
	HMODULE self = GetModuleHandleA(NULL);
	g_pollEvent = (PollEventFn)PatchImport(self, "SDL2.dll", "SDL_PollEvent", (void *)Hook_PollEvent);
	g_swapWindow = (SwapWindowFn)PatchImport(self, "SDL2.dll", "SDL_GL_SwapWindow", (void *)Hook_SwapWindow);
	g_renderPresent = (RenderPresentFn)PatchImport(self, "SDL2.dll", "SDL_RenderPresent", (void *)Hook_RenderPresent);
	WrapCommandTable();
	Log("ff4hook in %s (pid %lu): SDL_PollEvent %s, SDL_GL_SwapWindow %s; commands from %s",
		exe, GetCurrentProcessId(), g_pollEvent ? "hooked" : "NOT FOUND", g_swapWindow ? "hooked" : "NOT FOUND", g_cmdPath);
	// An import not yet bound would leave a hook calling nothing; keep the real one from SDL2 itself then.
	if (sdl && !g_pollEvent) g_pollEvent = (PollEventFn)GetProcAddress(sdl, "SDL_PollEvent");
	if (sdl && !g_swapWindow) g_swapWindow = (SwapWindowFn)GetProcAddress(sdl, "SDL_GL_SwapWindow");
	if (sdl && !g_renderPresent) g_renderPresent = (RenderPresentFn)GetProcAddress(sdl, "SDL_RenderPresent");
}

BOOL WINAPI DllMain(HINSTANCE instance, DWORD reason, LPVOID reserved)
{
	if (reason == DLL_PROCESS_ATTACH)
	{
		DisableThreadLibraryCalls(instance);
		LoadVersion();
		Install();
	}
	return TRUE;
}
