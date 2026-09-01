using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using android.content;
using android.text;
using android.widget;
using java.io;
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
						internal const int AREAMODE_JAPAN = 0;

						internal const int AREAMODE_AMERICA = 1;

						internal const int AREAMODE_EUROPE = 2;

						internal const int TARGET_AREAMODE = 0;

						internal const int NULL = 0;

						internal const int TRUE = 1;

						internal const int FALSE = 0;

						internal const int HW_LCD_WIDTH = 480;

						internal const int HW_LCD_HEIGHT = 320;

						internal const int FONT_S_SIZE = 12;

						internal const int FONT_L_SIZE = 16;

						internal const int ICON_SIZE = 16;

						internal const int HW_BG_VRAM_SIZE = 1;

						internal const int HW_OBJ_VRAM_SIZE = 1;

						internal const int HW_PLTT_SIZE = 1;

						internal const int HW_BG_PLTT_SIZE = 1;

						internal const int HW_DB_BG_VRAM_SIZE = 1;

						internal const int HW_DB_OBJ_VRAM_SIZE = 1;

						internal const int HW_DB_PLTT_SIZE = 1;

						internal const int HW_LCDC_VRAM_SIZE = 1;

						internal const int HW_OAM_SIZE = 1;

						internal const int HW_DB_OAM_SIZE = 1;

						internal const int HW_VRAM_A_SIZE = 1;

						internal const int HW_DTCM_SIZE = 1;

						internal const int HW_SYSTEM_CLOCK = 33514000;

						internal const int NVRAM_CONFIG_LANG_JAPANESE = 0;

						internal const int NVRAM_CONFIG_LANG_ENGLISH = 1;

						internal const int NVRAM_CONFIG_LANG_FRENCH = 2;

						internal const int NVRAM_CONFIG_LANG_GERMAN = 3;

						internal const int NVRAM_CONFIG_LANG_ITALIAN = 4;

						internal const int NVRAM_CONFIG_LANG_SPANISH = 5;

						internal const int NVRAM_CONFIG_LANG_CHINESE_CN = 6;

						internal const int NVRAM_CONFIG_LANG_CHINESE_TW = 7;

						internal const int NVRAM_CONFIG_LANG_CODE_MAX = 8;

						internal const int LANGUAGE_JAPAN = 0;

						internal const int LANGUAGE_AMERICA = 1;

						internal const int LANGUAGE_EUROPE = 2;

						internal const int TARGET_LANGUAGE = 0;

						internal const int OS_LOCK_ID_ERROR = 0;

						internal const int OS_CONSOLE_ISDEBUGGER = 0;

						internal const int OS_CONSOLE_NITRO = 0;

						internal const int OS_ARENA_DTCM = 0;

						internal const int OS_ARENA_MAINEX = 0;

						internal const int REG_OS_IE_VB_SHIFT = 0;

						internal const int REG_OS_IE_HB_SHIFT = 1;

						internal const int REG_OS_IE_IFN_SHIFT = 18;

						internal const uint OS_IE_V_BLANK = 1u;

						internal const uint OS_IE_H_BLANK = 2u;

						internal const uint OS_IE_SPFIFO_RECV = 262144u;

						internal const uint OS_IE_FIFO_RECV = 262144u;

						internal const int OS_VRAM_BANK_ID_A = 1;

						internal const int OS_VRAM_BANK_ID_B = 2;

						internal const int OS_VRAM_BANK_ID_C = 4;

						internal const int OS_VRAM_BANK_ID_D = 8;

						internal const int OS_VRAM_BANK_ID_E = 16;

						internal const int OS_VRAM_BANK_ID_F = 32;

						internal const int OS_VRAM_BANK_ID_G = 64;

						internal const int OS_VRAM_BANK_ID_H = 128;

						internal const int OS_VRAM_BANK_ID_I = 256;

						internal const int OS_VRAM_BANK_ID_ALL = 511;

						internal const int OS_VRAM_BANK_KINDS = 9;

						internal const int OS_SYSTEM_CLOCK = 33514000;

						internal const int FX32_SHIFT = 12;

						internal const int FX32_MAX = int.MaxValue;

						internal const int FX16_SHIFT = 12;

						internal const int FX16_MAX = 32767;

						internal const int FX64C_SHIFT = 32;

						internal const int FX32_ONE = 4096;

						internal const int FX32_HALF = 2048;

						internal const int FX32_SQRT1_3 = 2365;

						internal const int FX32_SIN3 = 214;

						internal const int FX32_SIN8 = 570;

						internal const int FX32_SIN12 = 852;

						internal const int FX32_SIN15 = 1060;

						internal const int FX32_SIN30 = 2048;

						internal const int FX32_SIN90 = 4096;

						internal const int FX32_SIN150 = 2048;

						internal const int FX32_COS3 = 4090;

						internal const int FX32_COS8 = 4056;

						internal const int FX32_COS12 = 4006;

						internal const int FX32_COS15 = 3956;

						internal const int FX32_COS30 = 3547;

						internal const int FX32_COS90 = 0;

						internal const int FX32_COS150 = -3547;

						internal const int FX16_ONE = 4096;

						internal const int FX16_HALF = 2048;

						internal const long FX64C_TWOPI_360 = 74961321L;

						internal const long FX64C_360_TWOPI = 246083499208L;

						internal const long FX64C_65536_TWOPI = 44798133900177L;

						internal const long FX64C_65536_360 = 781874935307L;

						internal const long FX64C_TWOPI_65536 = 411775L;

						internal const long FX64C_360_65536 = 23592960L;

						internal const int GX_RGB_R_SHIFT = 0;

						internal const int GX_RGB_R_MASK = 31;

						internal const int GX_RGB_G_SHIFT = 5;

						internal const int GX_RGB_G_MASK = 992;

						internal const int GX_RGB_B_SHIFT = 10;

						internal const int GX_RGB_B_MASK = 31744;

						internal const int GX_SCRFMT_TEXT_CHARNAME_SHIFT = 0;

						internal const int GX_SCRFMT_TEXT_CHARNAME_MASK = 1023;

						internal const int GX_SCRFMT_TEXT_HF_SHIFT = 10;

						internal const int GX_SCRFMT_TEXT_HF_MASK = 1024;

						internal const int GX_SCRFMT_TEXT_VF_SHIFT = 11;

						internal const int GX_SCRFMT_TEXT_VF_MASK = 2048;

						internal const int GX_SCRFMT_TEXT_COLORPLTT_SHIFT = 12;

						internal const int GX_SCRFMT_TEXT_COLORPLTT_MASK = 61440;

						internal const int GX_SHADING_TOON = 0;

						internal const int GX_SHADING_HIGHLIGHT = 0;

						internal const int GX_WNDMASK_NONE = 0;

						internal const int GX_WNDMASK_W0 = 0;

						internal const int GX_BG0_AS_3D = 0;

						internal const int GX_BG0_AS_2D = 0;

						internal const int GX_BG_SCRSIZE_TEXT_256x256 = 0;

						internal const int GX_BG_COLORMODE_256 = 0;

						internal const int GX_BG_EXTPLTT_01 = 0;

						internal const int GX_BG_COLORMODE_16 = 0;

						internal const int GX_BG_EXTPLTT_23 = 0;

						internal const int GX_BGCHAROFFSET_0x00000 = 0;

						internal const int GX_BGSCROFFSET_0x00000 = 0;

						internal const int GX_VRAM_LCDC_ALL = 0;

						internal const int GX_BUFFERMODE_Z = 0;

						internal const int GX_TEXGEN_NONE = 0;

						internal const int GX_TEXREPEAT_NONE = 0;

						internal const int GX_TEXFLIP_NONE = 0;

						internal const int GX_TEXPLTTCOLOR0_TRNS = 0;

						internal const int GX_VRAM_LCDC_NONE = 0;

						internal const int GX_SORTMODE_AUTO = 0;

						internal const int GX_BUFFERMODE_W = 0;

						internal const int GX_POWER_ALL = 0;

						internal const int GX_SORTMODE_MANUAL = 0;

						internal const int GX_VRAM_LCDC_A = 0;

						internal const int GX_VRAM_LCDC_B = 0;

						internal const int GX_OBJVRAMMODE_BMP_2D_W256 = 0;

						internal const int GX_OAM_MODE_BITMAPOBJ = 0;

						internal const int GX_OAM_EFFECT_NONE = 0;

						internal const int GX_OAM_SHAPE_64x64 = 0;

						internal const int GX_OAM_COLOR_16 = 0;

						internal const int GX_BG_SCRSIZE_DCBMP_256x256 = 0;

						internal const int GX_BG_AREAOVER_XLU = 0;

						internal const int GX_BG_BMPSCRBASE_0x00000 = 0;

						internal const int GX_BG_EXTMODE_256x16PLTT = 0;

						internal const int GX_BG_EXTMODE_256BITMAP = 1;

						internal const int GX_BG_EXTMODE_DCBITMAP = 2;

						internal const int GX_OBJVRAMMODE_CHAR_2D = 0;

						internal const int GX_TEXPLTTCOLOR0_USE = 0;

						internal const int REG_G3_POLYGON_ATTR_ID_SHIFT = 24;

						internal const int REG_G3_POLYGON_ATTR_ID_SIZE = 6;

						internal const int REG_G3_POLYGON_ATTR_ID_MASK = 1056964608;

						internal const int REG_G3_POLYGON_ATTR_ALPHA_SHIFT = 16;

						internal const int REG_G3_POLYGON_ATTR_ALPHA_SIZE = 5;

						internal const int REG_G3_POLYGON_ATTR_ALPHA_MASK = 2031616;

						internal const int REG_G3_POLYGON_ATTR_FE_SHIFT = 15;

						internal const int REG_G3_POLYGON_ATTR_FE_SIZE = 1;

						internal const int REG_G3_POLYGON_ATTR_FE_MASK = 32768;

						internal const int REG_G3_POLYGON_ATTR_DT_SHIFT = 14;

						internal const int REG_G3_POLYGON_ATTR_DT_SIZE = 1;

						internal const int REG_G3_POLYGON_ATTR_DT_MASK = 16384;

						internal const int REG_G3_POLYGON_ATTR_D1_SHIFT = 13;

						internal const int REG_G3_POLYGON_ATTR_D1_SIZE = 1;

						internal const int REG_G3_POLYGON_ATTR_D1_MASK = 8192;

						internal const int REG_G3_POLYGON_ATTR_FC_SHIFT = 12;

						internal const int REG_G3_POLYGON_ATTR_FC_SIZE = 1;

						internal const int REG_G3_POLYGON_ATTR_FC_MASK = 4096;

						internal const int REG_G3_POLYGON_ATTR_XL_SHIFT = 11;

						internal const int REG_G3_POLYGON_ATTR_XL_SIZE = 1;

						internal const int REG_G3_POLYGON_ATTR_XL_MASK = 2048;

						internal const int REG_G3_POLYGON_ATTR_FR_SHIFT = 7;

						internal const int REG_G3_POLYGON_ATTR_FR_SIZE = 1;

						internal const int REG_G3_POLYGON_ATTR_FR_MASK = 128;

						internal const int REG_G3_POLYGON_ATTR_BK_SHIFT = 6;

						internal const int REG_G3_POLYGON_ATTR_BK_SIZE = 1;

						internal const int REG_G3_POLYGON_ATTR_BK_MASK = 64;

						internal const int REG_G3_POLYGON_ATTR_PM_SHIFT = 4;

						internal const int REG_G3_POLYGON_ATTR_PM_SIZE = 2;

						internal const int REG_G3_POLYGON_ATTR_PM_MASK = 48;

						internal const int REG_G3_POLYGON_ATTR_LE_SHIFT = 0;

						internal const int REG_G3_POLYGON_ATTR_LE_SIZE = 4;

						internal const int REG_G3_POLYGON_ATTR_LE_MASK = 15;

						internal const GXVRamSubBGExtPltt GX_VRAM_SUB_BGEXTPLTT_32_H = GXVRamSubBGExtPltt.GX_VRAM_SUB_BGEXTPLTT_0123_H;

						internal const GXVRamSubOBJExtPltt GX_VRAM_SUB_OBJEXTPLTT_16_I = GXVRamSubOBJExtPltt.GX_VRAM_SUB_OBJEXTPLTT_0_I;

						internal const int G3OP_NOP = 0;

						internal const int G3OP_MTX_MODE = 16;

						internal const int G3OP_MTX_PUSH = 17;

						internal const int G3OP_MTX_POP = 18;

						internal const int G3OP_MTX_STORE = 19;

						internal const int G3OP_MTX_RESTORE = 20;

						internal const int G3OP_MTX_IDENTITY = 21;

						internal const int G3OP_MTX_LOAD_4x4 = 22;

						internal const int G3OP_MTX_LOAD_4x3 = 23;

						internal const int G3OP_MTX_MULT_4x4 = 24;

						internal const int G3OP_MTX_MULT_4x3 = 25;

						internal const int G3OP_MTX_MULT_3x3 = 26;

						internal const int G3OP_MTX_SCALE = 27;

						internal const int G3OP_MTX_TRANS = 28;

						internal const int G3OP_COLOR = 32;

						internal const int G3OP_NORMAL = 33;

						internal const int G3OP_TEXCOORD = 34;

						internal const int G3OP_VTX_16 = 35;

						internal const int G3OP_VTX_10 = 36;

						internal const int G3OP_VTX_XY = 37;

						internal const int G3OP_VTX_XZ = 38;

						internal const int G3OP_VTX_YZ = 39;

						internal const int G3OP_VTX_DIFF = 40;

						internal const int G3OP_POLYGON_ATTR = 41;

						internal const int G3OP_TEXIMAGE_PARAM = 42;

						internal const int G3OP_TEXPLTT_BASE = 43;

						internal const int G3OP_DIF_AMB = 48;

						internal const int G3OP_SPE_EMI = 49;

						internal const int G3OP_LIGHT_VECTOR = 50;

						internal const int G3OP_LIGHT_COLOR = 51;

						internal const int G3OP_SHININESS = 52;

						internal const int G3OP_BEGIN = 64;

						internal const int G3OP_END = 65;

						internal const int G3OP_SWAP_BUFFERS = 80;

						internal const int G3OP_VIEWPORT = 96;

						internal const int G3OP_BOX_TEST = 112;

						internal const int G3OP_POS_TEST = 113;

						internal const int G3OP_VEC_TEST = 114;

						internal const int G3OP_DUMMY_COMMAND = 255;

						internal const int MI_DMA_MAX_NUM = 3;

						internal const int PAD_PLUS_KEY_MASK = 240;

						internal const int PAD_BUTTON_MASK = 12047;

						internal const int PAD_DEBUG_BUTTON_MASK = 8192;

						internal const int PAD_ALL_MASK = 12287;

						internal const int PAD_RCNTPORT_MASK = 11264;

						internal const int PAD_KEYPORT_MASK = 1023;

						internal const int PAD_DETECT_FOLD_MASK = 32768;

						internal const int PAD_BUTTON_A = 1;

						internal const int PAD_BUTTON_B = 2;

						internal const int PAD_BUTTON_SELECT = 4;

						internal const int PAD_BUTTON_START = 8;

						internal const int PAD_KEY_RIGHT = 16;

						internal const int PAD_KEY_LEFT = 32;

						internal const int PAD_KEY_UP = 64;

						internal const int PAD_KEY_DOWN = 128;

						internal const int PAD_BUTTON_R = 256;

						internal const int PAD_BUTTON_L = 512;

						internal const int PAD_BUTTON_X = 1024;

						internal const int PAD_BUTTON_Y = 2048;

						internal const int PAD_BUTTON_DEBUG = 8192;

						internal const int TP_VALIDITY_VALID = 0;

						internal const int TP_VALIDITY_INVALID_X = 1;

						internal const int TP_VALIDITY_INVALID_Y = 2;

						internal const int TP_VALIDITY_INVALID_XY = 3;

						internal const int PM_TRIGGER_COVER_OPEN = 0;

						internal const int PM_TRIGGER_CARD = 0;

						internal const int PM_PAD_LOGIC_AND = 0;

						internal const int PM_LCD_POWER_OFF = 0;

						internal const int PM_LCD_POWER_ON = 0;

						internal const int RTC_RESULT_SUCCESS = 0;

						internal const int SND_CHANNEL_DATASHIFT_NONE = 0;

						internal const int SND_COMMAND_NOBLOCK = 0;

						internal const int SND_COMMAND_IMMEDIATE = 0;

						internal const int SND_TIMER_CLOCK = 0;

						internal const int SND_WAVE_FORMAT_PCM16 = 0;

						internal const int SND_CHANNEL_LOOP_REPEAT = 0;

						internal const int SND_COMMAND_BLOCK = 0;

						internal const int SAVE_VRAM_A = 0;

						internal const int SAVE_VRAM_B = 1;

						internal const int SAVE_VRAM_C = 2;

						internal const int SAVE_VRAM_D = 3;

						internal const int REG_G2_BG0CNT_CHARBASE_MASK = 0;

						internal const int REG_G2_BG0CNT_CHARBASE_SHIFT = 0;

						internal const int REG_G2_BG0CNT_SCREENBASE_MASK = 0;

						internal const int REG_G2_BG0CNT_SCREENBASE_SHIFT = 0;

						internal const int REG_GX_DISPCNT_BGCHAROFFSET_MASK = 0;

						internal const int REG_GX_DISPCNT_BGCHAROFFSET_SHIFT = 0;

						internal const int REG_GX_DISPCNT_BGSCREENOFFSET_MASK = 0;

						internal const int REG_GX_DISPCNT_BGSCREENOFFSET_SHIFT = 0;

						internal const int MB_COMM_PSTATE_NONE = 0;

						internal const int MB_COMM_PSTATE_INIT_COMPLETE = 0;

						internal const int MB_COMM_PSTATE_CONNECTED = 0;

						internal const int MB_COMM_PSTATE_DISCONNECTED = 0;

						internal const int MB_COMM_PSTATE_KICKED = 0;

						internal const int MB_COMM_PSTATE_REQ_ACCEPTED = 0;

						internal const int MB_COMM_PSTATE_SEND_PROCEED = 0;

						internal const int MB_COMM_PSTATE_SEND_COMPLETE = 0;

						internal const int MB_COMM_PSTATE_BOOT_REQUEST = 0;

						internal const int MB_COMM_PSTATE_BOOT_STARTABLE = 0;

						internal const int MB_COMM_PSTATE_REQUESTED = 0;

						internal const int MB_COMM_PSTATE_MEMBER_FULL = 0;

						internal const int MB_COMM_PSTATE_END = 0;

						internal const int MB_COMM_PSTATE_ERROR = 0;

						internal const int MB_COMM_PSTATE_WAIT_TO_SEND = 0;

						internal const int MB_COMM_PSTATE_WM_EVENT = 0;

						internal const int MB_ERRCODE_SUCCESS = 0;

						internal const int MB_ERRCODE_INVALID_PARAM = 0;

						internal const int MB_ERRCODE_INVALID_STATE = 0;

						internal const int MB_ERRCODE_INVALID_DLFILEINFO = 0;

						internal const int MB_ERRCODE_INVALID_BLOCK_NO = 0;

						internal const int MB_ERRCODE_INVALID_BLOCK_NUM = 0;

						internal const int MB_ERRCODE_INVALID_FILE = 0;

						internal const int MB_ERRCODE_INVALID_RECV_ADDR = 0;

						internal const int MB_ERRCODE_WM_FAILURE = 0;

						internal const int MB_ERRCODE_FATAL = 0;

						internal const int VX_INVALID_HANDLE = 0;

						internal const int VX_NB_BUFFERED_FRAME_DEFAULT = 0;

						internal const int VX_LOOP_ENABLE = 0;

						internal const int VX_LOOP_DISABLE = 0;

						internal const int VX_NB_SAMPLE_PER_SOUND_PACKET = 0;

						internal const double PI = 3.14159265358979;

						internal const string SAVE_FILE_NAME = "/data/data/com.square_enix.FFIII_J/files/save.bin";

						internal const int SAVE_FILE_SIZE = 65536;

						internal const int NNS_FND_EXPHEAP_ALLOC_MODE_NEAR = 0;

						internal const TexVram NNS_GFD_ALLOC_ERROR_TEXKEY = null;

						internal const TexVram NNS_GFD_ALLOC_ERROR_PLTTKEY = null;

						internal const int NNS_G2D_BGSELECT_FACE = 8;

						internal const string NNS_G2D_BINFILE_EXT_CHARACTERDATA_BMP = "NCBR";

						internal const string NNS_G2D_BINFILE_EXT_CHARACTERDATA = "NCGR";

						internal const string NNS_G2D_BINFILE_EXT_PALETTEDATA = "NCLR";

						internal const string NNS_G2D_BINFILE_EXT_SCRDATA = "NSCR";

						internal const string NNS_G2D_BINFILE_EXT_CELL = "NCER";

						internal const string NNS_G2D_BINFILE_EXT_CELLANIM = "NANR";

						internal const int NNS_G3D_RESTEX_LOADED = 1;

						internal const int NNS_G3D_RESPLTT_LOADED = 1;

						internal const int NNS_G3D_RESTEX4x4_LOADED = 1;

						internal const int NNS_G3D_SBC_NOP = 0;

						internal const int NNS_G3D_SBC_RET = 1;

						internal const int NNS_G3D_SBC_NODE = 2;

						internal const int NNS_G3D_SBC_MTX = 3;

						internal const int NNS_G3D_SBC_MAT = 4;

						internal const int NNS_G3D_SBC_SHP = 5;

						internal const int NNS_G3D_SBC_NODEDESC = 6;

						internal const int NNS_G3D_SBC_BB = 7;

						internal const int NNS_G3D_SBC_BBY = 8;

						internal const int NNS_G3D_SBC_NODEMIX = 9;

						internal const int NNS_G3D_SBC_CALLDL = 10;

						internal const int NNS_G3D_SBC_POSSCALE = 11;

						internal const int NNS_G3D_SBC_ENVMAP = 12;

						internal const int NNS_G3D_SBC_PRJMAP = 13;

						internal const int NNS_G3D_RESNAME_SIZE = 16;

						internal const int NNS_G3D_RESNAME_VALSIZE = 4;

						internal const int NNS_G3D_UTIL_RESNAME_LEN = 17;

						internal const int NNS_G3D_ANMFMT_MAX = 10;

						internal const int NNS_G3D_SIZE_JNT_MAX = 64;

						internal const int NNS_G3D_SIZE_MAT_MAX = 64;

						internal const int NNS_G3D_SIZE_SHP_MAX = 256;

						internal const int NNS_G3D_SIZE_COMBUFFER = 192;

						internal const int NNS_G3D_SKIP_TRANS = 1;

						internal const int NNS_G3D_SKIP_NOT_TRANS = 2;

						internal const int NNS_SND_ARC_LOAD_WAVE = 0;

						internal const int NNS_SND_ARC_LOAD_BANK = 0;

						internal const int NNS_SND_ARC_LOAD_SEQ = 0;

						internal const int NNS_SND_CAPTURE_FORMAT_PCM8 = 0;

						internal const int NNS_SND_CAPTURE_FORMAT_PCM16 = 0;

						internal const int NNS_SNDARC_SNDTYPE_INVALID = 0;

						internal const int NNS_SNDARC_SNDTYPE_SEQ = 1;

						internal const int NNS_SNDARC_SNDTYPE_BANK = 2;

						internal const int NNS_SNDARC_SNDTYPE_WAVEARC = 3;

						internal const int NNS_SNDARC_SNDTYPE_SEQARC = 4;

						internal const int NNS_SND_ARC_BANK_TO_WAVEARC_NUM = 0;

						internal const int NNS_SND_ARC_INVALID_WAVEARC_NO = 0;

						internal const int NNS_SND_ARC_STRM_CALLBACK_DATA_END = 0;

						internal const int NNS_SND_HEAP_INVALID_HANDLE = 0;

						internal const int NNS_SND_PLAYER_BGM = 0;

						internal const int NNS_SND_PLAYER_SE = 1;

						internal const int NNS_MCS_FILEIO_FLAG_WRITE = 0;

						internal const int NNS_MCS_FILEIO_FLAG_FORCE = 0;

						internal const int NNS_MCS_FILEIO_FLAG_INCENVVAR = 0;

						internal const int NNS_MCS_FILEIO_FLAG_READ = 0;

						internal const int NNS_SND_INTRO = 1;

						internal const int NNS_SND_PLAY = 2;

						internal const int NNS_SND_FADE_OUT = 4;

						internal const int NNS_SND_REQUEST_STOP = 16777216;

						internal const int NNS_SND_REQUEST_BGM = 33554432;

						internal const int NNS_SND_REQUEST_SE = 50331648;

						internal const int NNS_SND_REQUEST_MASK = -16777216;

						internal const int SWC_INIT_WORK_SIZE = 0;

						internal const int SWC_LANGUAGE_JAPANESE = 0;

						internal const int SWC_UTILITY_TOP_MENU_FOR_JPN = 0;

						internal const int SWC_INIT_RESULT_DESTROY_OTHER_SETTING = 1;

						internal const int SWC_ETYPE_FATAL = 0;

						internal const int SWC_ERROR_NONE = 0;

						internal const int SWC_CONNECTINET_AUTH_TEST = 0;

						internal const int SWC_INGAMESN_INVALID = 0;

						internal const int SWC_ETYPE_DISCONNECT = 0;

						internal const int SWC_ETYPE_LIGHT = 1;

						internal const int SWC_CONNECTINET_AUTH_RELEASE = 0;

						internal const int SWC_ETYPE_NO_ERROR = 0;

						internal const int DWC_LANGUAGE_JAPANESE = 0;

						internal const int DWC_LANGUAGE_ENGLISH = 0;

						internal const int DWC_LANGUAGE_FRENCH = 0;

						internal const int DWC_LANGUAGE_GERMAN = 0;

						internal const int DWC_LANGUAGE_ITALIAN = 0;

						internal const int DWC_LANGUAGE_SPANISH = 0;

						internal const int DWC_UTIL_RESULT_SUCCESS = 0;

						internal const int DWC_UTIL_RESULT_FAILED = 0;

						internal const int DWC_UTIL_FLAG0_TOP = 0;

						internal const int DWC_UTIL_FLAG0_SETTING = 0;

						internal const int DWC_UTIL_FLAG1_RAKU = 0;

						internal const int DWC_UTIL_FLAG1_USA = 0;

						internal const int DWC_ERROR_NONE = 0;

						internal const int DWC_ERROR_DS_MEMORY_ANY = 0;

						internal const int DWC_ERROR_AUTH_ANY = 0;

						internal const int DWC_ERROR_AUTH_OUT_OF_SERVICE = 0;

						internal const int DWC_ERROR_AUTH_STOP_SERVICE = 0;

						internal const int DWC_ERROR_AC_ANY = 0;

						internal const int DWC_ERROR_NETWORK = 0;

						internal const int DWC_ERROR_GHTTP_ANY = 0;

						internal const int DWC_ERROR_DISCONNECTED = 0;

						internal const int DWC_ERROR_FATAL = 0;

						internal const int DWC_ERROR_FRIENDS_SHORTAGE = 0;

						internal const int DWC_ERROR_NOT_FRIEND_SERVER = 0;

						internal const int DWC_ERROR_MO_SC_CONNECT_BLOCK = 0;

						internal const int DWC_ERROR_SERVER_FULL = 0;

						internal const int DWC_ERROR_ND_ANY = 0;

						internal const int DWC_ERROR_ND_HTTP = 0;

						internal const int DWC_ERROR_SVL_ANY = 0;

						internal const int DWC_ERROR_SVL_HTTP = 0;

						internal const int DWC_ERROR_NUM = 0;

						internal const int DWC_ETYPE_NO_ERROR = 0;

						internal const int DWC_ETYPE_LIGHT = 0;

						internal const int DWC_ETYPE_SHOW_ERROR = 0;

						internal const int DWC_ETYPE_SHUTDOWN_FM = 0;

						internal const int DWC_ETYPE_SHUTDOWN_GHTTP = 0;

						internal const int DWC_ETYPE_SHUTDOWN_ND = 0;

						internal const int DWC_ETYPE_DISCONNECT = 0;

						internal const int DWC_ETYPE_FATAL = 0;

						internal const int DWC_ETYPE_NUM = 0;

						internal const int DWC_ECODE_SEQ_LOGIN = 0;

						internal const int DWC_ECODE_SEQ_FRIEND = 0;

						internal const int DWC_ECODE_SEQ_MATCH = 0;

						internal const int DWC_ECODE_SEQ_ETC = 0;

						internal const int DWC_ECODE_GS_GP = 0;

						internal const int DWC_ECODE_GS_PERS = 0;

						internal const int DWC_ECODE_GS_STATS = 0;

						internal const int DWC_ECODE_GS_QR2 = 0;

						internal const int DWC_ECODE_GS_SB = 0;

						internal const int DWC_ECODE_GS_NN = 0;

						internal const int DWC_ECODE_GS_GT2 = 0;

						internal const int DWC_ECODE_GS_HTTP = 0;

						internal const int DWC_ECODE_GS_ETC = 0;

						internal const int DWC_ECODE_TYPE_NETWORK = 0;

						internal const int DWC_ECODE_TYPE_SERVER = 0;

						internal const int DWC_ECODE_TYPE_DNS = 0;

						internal const int DWC_ECODE_TYPE_DATA = 0;

						internal const int DWC_ECODE_TYPE_SOCKET = 0;

						internal const int DWC_ECODE_TYPE_BIND = 0;

						internal const int DWC_ECODE_TYPE_TIMEOUT = 0;

						internal const int DWC_ECODE_TYPE_PEER = 0;

						internal const int DWC_ECODE_TYPE_CONN_OVER = 0;

						internal const int DWC_ECODE_TYPE_STATS_AUTH = 0;

						internal const int DWC_ECODE_TYPE_STATS_LOAD = 0;

						internal const int DWC_ECODE_TYPE_STATS_SAVE = 0;

						internal const int DWC_ECODE_TYPE_NOT_FRIEND = 0;

						internal const int DWC_ECODE_TYPE_OTHER = 0;

						internal const int DWC_ECODE_TYPE_MUCH_FAILURE = 0;

						internal const int DWC_ECODE_TYPE_SC_CL_FAIL = 0;

						internal const int DWC_ECODE_TYPE_CLOSE = 0;

						internal const int DWC_ECODE_TYPE_TRANS_HEADER = 0;

						internal const int DWC_ECODE_TYPE_TRANS_BODY = 0;

						internal const int DWC_ECODE_TYPE_AC_FATAL = 0;

						internal const int DWC_ECODE_TYPE_OPEN_FILE = 0;

						internal const int DWC_ECODE_TYPE_INVALID_POST = 0;

						internal const int DWC_ECODE_TYPE_REQ_INVALID = 0;

						internal const int DWC_ECODE_TYPE_UNSPECIFIED = 0;

						internal const int DWC_ECODE_TYPE_BUFF_OVER = 0;

						internal const int DWC_ECODE_TYPE_PARSE_URL = 0;

						internal const int DWC_ECODE_TYPE_BAD_RESPONSE = 0;

						internal const int DWC_ECODE_TYPE_REJECTED = 0;

						internal const int DWC_ECODE_TYPE_FILE_RW = 0;

						internal const int DWC_ECODE_TYPE_INCOMPLETE = 0;

						internal const int DWC_ECODE_TYPE_TO_BIG = 0;

						internal const int DWC_ECODE_TYPE_ENCRYPTION = 0;

						internal const int DWC_ECODE_TYPE_ALLOC = 0;

						internal const int DWC_ECODE_TYPE_PARAM = 0;

						internal const int DWC_ECODE_TYPE_SO_SOCKET = 0;

						internal const int DWC_ECODE_TYPE_NOT_INIT = 0;

						internal const int DWC_ECODE_TYPE_DUP_INIT = 0;

						internal const int DWC_ECODE_TYPE_WM_INIT = 0;

						internal const int DWC_ECODE_TYPE_UNEXPECTED = 0;

						internal const int DWC_ECODE_SEQ_ADDINS = 0;

						internal const int DWC_ECODE_FUNC_ND = 0;

						internal const int DWC_ECODE_TYPE_ND_ALLOC = 0;

						internal const int DWC_ECODE_TYPE_ND_FATAL = 0;

						internal const int DWC_ECODE_TYPE_ND_BUSY = 0;

						internal const int DWC_ECODE_TYPE_ND_HTTP = 0;

						internal const int DWC_ECODE_TYPE_ND_SERVER = 0;

						internal const int DWC_REPORTFLAG_INFO = 0;

						internal const int DWC_REPORTFLAG_ERROR = 0;

						internal const int DWC_REPORTFLAG_DEBUG = 0;

						internal const int DWC_REPORTFLAG_WARNING = 0;

						internal const int DWC_REPORTFLAG_ACHECK = 0;

						internal const int DWC_REPORTFLAG_LOGIN = 0;

						internal const int DWC_REPORTFLAG_MATCH_NN = 0;

						internal const int DWC_REPORTFLAG_MATCH_GT2 = 0;

						internal const int DWC_REPORTFLAG_TRANSPORT = 0;

						internal const int DWC_REPORTFLAG_QR2_REQ = 0;

						internal const int DWC_REPORTFLAG_SB_UPDATE = 0;

						internal const int DWC_REPORTFLAG_SEND_INFO = 0;

						internal const int DWC_REPORTFLAG_RECV_INFO = 0;

						internal const int DWC_REPORTFLAG_UPDATE_SV = 0;

						internal const int DWC_REPORTFLAG_CONNECTINET = 0;

						internal const int DWC_REPORTFLAG_AUTH = 0;

						internal const int DWC_REPORTFLAG_AC = 0;

						internal const int DWC_REPORTFLAG_BM = 0;

						internal const int DWC_REPORTFLAG_UTIL = 0;

						internal const int DWC_REPORTFLAG_GAMESPY = 0;

						internal const int DWC_REPORTFLAG_ALL = 0;

						internal const int DWC_GS_STATUS_STRING_LEN = 0;

						internal const int DWC_PERS_KEY_ID_MASK = 0;

						internal const int DWC_PERS_KEY_ID_PLAYER_NAME = 0;

						internal const int DWC_GP_PROCESS_INTERVAL = 0;

						internal const int DWC_FRIEND_UPDATE_WAIT_COUNT = 0;

						internal const int DWC_GP_KEEP_ALIVE_COUNT = 0;

						internal const int DWC_GP_SSTR_KEY_MATCH_VERSION = 0;

						internal const int DWC_GP_SSTR_KEY_MATCH_FRIEND_NUM = 0;

						internal const int DWC_GP_SSTR_KEY_DISTANT_FRIEND = 0;

						internal const int DWC_GP_SSTR_KEY_MATCH_SC_MAX = 0;

						internal const int DWC_GP_SSTR_KEY_MATCH_SC_NUM = 0;

						internal const int DWC_GP_STATUS_NO_CHANGE = 0;

						internal const int DWC_STATUS_OFFLINE = 0;

						internal const int DWC_STATUS_ONLINE = 0;

						internal const int DWC_STATUS_PLAYING = 0;

						internal const int DWC_STATUS_MATCH_ANYBODY = 0;

						internal const int DWC_STATUS_MATCH_FRIEND = 0;

						internal const int DWC_STATUS_MATCH_SC_CL = 0;

						internal const int DWC_STATUS_MATCH_SC_SV = 0;

						internal const int DWC_STATUS_NUM = 0;

						internal const int DWC_MAX_CONNECTIONS = 1;

						internal const int DWC_STATE_INIT = 0;

						internal const int DWC_STATE_AVAILABLE_CHECK = 0;

						internal const int DWC_STATE_LOGIN = 0;

						internal const int DWC_STATE_ONLINE = 0;

						internal const int DWC_STATE_UPDATE_SERVERS = 0;

						internal const int DWC_STATE_MATCHING = 0;

						internal const int DWC_STATE_CONNECTED = 0;

						internal const int DWC_STATE_NUM = 0;

						internal const int DWC_ALLOCTYPE_AUTH = 0;

						internal const int DWC_ALLOCTYPE_AC = 0;

						internal const int DWC_ALLOCTYPE_BM = 0;

						internal const int DWC_ALLOCTYPE_UTIL = 0;

						internal const int DWC_ALLOCTYPE_BASE = 0;

						internal const int DWC_ALLOCTYPE_GS = 0;

						internal const int DWC_ALLOCTYPE_LAST = 0;

						internal const int DWC_CONNECTINET_STATE_NOT_INITIALIZED = 0;

						internal const int DWC_CONNECTINET_STATE_IDLE = 0;

						internal const int DWC_CONNECTINET_STATE_OPERATING = 0;

						internal const int DWC_CONNECTINET_STATE_OPERATED = 0;

						internal const int DWC_CONNECTINET_STATE_CONNECTED = 0;

						internal const int DWC_CONNECTINET_STATE_DISCONNECTING = 0;

						internal const int DWC_CONNECTINET_STATE_DISCONNECTED = 0;

						internal const int DWC_CONNECTINET_STATE_ERROR = 0;

						internal const int DWC_CONNECTINET_STATE_FATAL_ERROR = 0;

						internal const int DWC_CONNECTINET_STATE_LAST = 0;

						internal const int DWC_CONNECTINET_AUTH_TEST = 0;

						internal const int DWC_CONNECTINET_AUTH_DEVELOP = 0;

						internal const int DWC_CONNECTINET_AUTH_RELEASE = 0;

						internal const int DWC_CONNECTINET_AUTH_LAST = 0;

						internal const int DWC_APINFO_AREA_JPN = 0;

						internal const int DWC_APINFO_AREA_USA = 0;

						internal const int DWC_APINFO_AREA_EUR = 0;

						internal const int DWC_APINFO_AREA_AUS = 0;

						internal const int DWC_APINFO_AREA_UNKNOWN = 0;

						internal const int DWC_APINFO_TYPE_USER0 = 0;

						internal const int DWC_APINFO_TYPE_USER1 = 0;

						internal const int DWC_APINFO_TYPE_USER2 = 0;

						internal const int DWC_APINFO_TYPE_USB = 0;

						internal const int DWC_APINFO_TYPE_SHOP = 0;

						internal const int DWC_APINFO_TYPE_FREESPOT = 0;

						internal const int DWC_APINFO_TYPE_WAYPORT = 0;

						internal const int DWC_APINFO_TYPE_OTHER = 0;

						internal const int DWC_APINFO_TYPE_FALSE = 0;

						internal const int DWC_INIT_RESULT_NOERROR = 0;

						internal const int DWC_INIT_RESULT_CREATE_USERID = 0;

						internal const int DWC_INIT_RESULT_DESTROY_USERID = 0;

						internal const int DWC_INIT_RESULT_DESTROY_OTHER_SETTING = 0;

						internal const int DWC_INIT_RESULT_LAST = 0;

						internal const int DWC_INIT_RESULT_DESTORY_USERID = 0;

						internal const int DWC_INIT_RESULT_DESTORY_OTHER_SETTING = 0;

						internal const int DWC_GHTTP_FALSE = 0;

						internal const int DWC_GHTTP_HOST_LOOKUP = 0;

						internal const int DWC_GHTTP_CONNECTING = 0;

						internal const int DWC_GHTTP_SECURING_SESSION = 0;

						internal const int DWC_GHTTP_SENDING_REQUEST = 0;

						internal const int DWC_GHTTP_POSTING = 0;

						internal const int DWC_GHTTP_WAITING = 0;

						internal const int DWC_GHTTP_RECEIVING_STATUS = 0;

						internal const int DWC_GHTTP_RECEIVING_HEADERS = 0;

						internal const int DWC_GHTTP_RECEIVING_FILE = 0;

						internal const int DWC_GHTTP_ERROR_START = 0;

						internal const int DWC_GHTTP_IN_ERROR = 0;

						internal const int DWC_GHTTP_FAILED_TO_OPEN_FILE = 0;

						internal const int DWC_GHTTP_INVALID_POST = 0;

						internal const int DWC_GHTTP_INSUFFICIENT_MEMORY = 0;

						internal const int DWC_GHTTP_INVALID_FILE_NAME = 0;

						internal const int DWC_GHTTP_INVALID_BUFFER_SIZE = 0;

						internal const int DWC_GHTTP_INVALID_URL = 0;

						internal const int DWC_GHTTP_UNSPECIFIED_ERROR = 0;

						internal const int DWC_GHTTP_SUCCESS = 0;

						internal const int DWC_GHTTP_OUT_OF_MEMORY = 0;

						internal const int DWC_GHTTP_BUFFER_OVERFLOW = 0;

						internal const int DWC_GHTTP_PARSE_URL_FAILED = 0;

						internal const int DWC_GHTTP_HOST_LOOKUP_FAILED = 0;

						internal const int DWC_GHTTP_SOCKET_FAILED = 0;

						internal const int DWC_GHTTP_CONNECT_FAILED = 0;

						internal const int DWC_GHTTP_BAD_RESPONSE = 0;

						internal const int DWC_GHTTP_REQUEST_REJECTED = 0;

						internal const int DWC_GHTTP_UNAUTHORIZED = 0;

						internal const int DWC_GHTTP_FORBIDDEN = 0;

						internal const int DWC_GHTTP_FILE_NOT_FOUND = 0;

						internal const int DWC_GHTTP_SERVER_ERROR = 0;

						internal const int DWC_GHTTP_FILE_WRITE_FAILED = 0;

						internal const int DWC_GHTTP_FILE_READ_FAILED = 0;

						internal const int DWC_GHTTP_FILE_INCOMPLETE = 0;

						internal const int DWC_GHTTP_FILE_TOO_BIG = 0;

						internal const int DWC_GHTTP_ENCRYPTION_ERROR = 0;

						internal const int DWC_GHTTP_NUM = 0;

						internal const int DWC_GHTTP_MEMORY_ERROR = 0;

						internal const int DWC_INGAMESN_NOT_CHECKED = 0;

						internal const int DWC_INGAMESN_VALID = 0;

						internal const int DWC_INGAMESN_INVALID = 0;

						internal const int DWC_INIT_WORK_SIZE = 0;

						internal const int SCREEN_HEIGHT = 192;

						internal const int SCREEN_WIDTH = 256;

						internal const int SCREEN_CHARA_SIZE = 8;

						internal const int SCR_PLTT_SHIFT = 12;

						internal const int FONT_COLOR = 4;

						internal const int SCREEN_HEIGHT_CHARA = 24;

						internal const int SCREEN_WIDTH_CHARA = 32;

						internal const int NEXT_MOTION_MAX = 3;

						internal const int DEBUG_MESSAGE_MODE_ID = 0;

						internal const int DEBUG_MESSAGE_MODE_FILE = 1;

						internal const int DEBUG_MESSAGE_MODE_MAX = 2;

						internal const int DEBUG_MESSAGE_MODE_SEARCH = 10;

						internal const int DEBUG_SOUND_MODE_BGM = 0;

						internal const int DEBUG_SOUND_MODE_SE = 1;

						internal const int DEBUG_SOUND_MODE_MAX = 2;

						internal const int PAD_KEY_HIDE = 65536;

						internal const int PAD_KEY_BOOST = 131072;

						internal const uint dfSEQ_TIME_START = 1u;

						internal const uint dfSEQ_TIME_END = uint.MaxValue;

						internal const uint dfSEQ_PATH_END = uint.MaxValue;

						internal const int SEQUENCE_PATH_FILE_NAME_MAX = 48;

						internal const int DEBUG_PRINT = 0;

						internal const int CHARACTER_OFFSET = 0;

						internal const int TXT_DRAWTEXT_FLAG_DEFAULT = 521;

						internal const int PALETTE_NUMBER = 15;

						internal const int PALETTE_OFFSET = 480;

						internal const int LIMIT_OF_MESSAGE = 30;

						internal const int LIMIT_LENGTH_OF_STR = 64;

						internal const int LIMIT_OF_FONT_CACHE = 88;

						internal const int BUTTON_FRAME_UL = 0;

						internal const int BUTTON_FRAME_CL = 1;

						internal const int BUTTON_FRAME_DL = 2;

						internal const int BUTTON_FRAME_DC = 3;

						internal const int BUTTON_FRAME_DR = 4;

						internal const int BUTTON_FRAME_CR = 5;

						internal const int BUTTON_FRAME_UR = 6;

						internal const int BUTTON_FRAME_UC = 7;

						internal const int BUTTON_FRAME_MAX = 8;

						internal const int CAMERA_SPEED = 512;

						internal const int ROT_SPEED = 256;

						internal const int MAX_ROTATION = 16128;

						internal const int MIN_ROTATION = -16128;

						internal const int MAX_DISTANCE = 614400;

						internal const int MIN_DISTANCE = 16384;

						internal const int LEFT_WEAPON = 1;

						internal const int RIGHT_WEAPON = 2;

						internal const int HEAD = 4;

						internal const int ARMOR = 8;

						internal const int GUNTRET = 16;

						internal const int MAP_DEBUG = 1;

						internal const int NUM_OF_ACCEPTER = 32;

						internal const int SUSPEND_DATA_ADDR = 41568;

						internal const int OPTION_DATA_ADDR = 55424;

						internal const int COMMON_DATA_ADDR = 55504;

						internal const int BGM_INFO_MAX = 64;

						internal const int BGM_0000 = 0;

						internal const int BGM_0010 = 1;

						internal const int BGM_0020 = 2;

						internal const int BGM_0030 = 3;

						internal const int BGM_0040 = 4;

						internal const int BGM_0050 = 5;

						internal const int BGM_0060 = 6;

						internal const int BGM_0070 = 7;

						internal const int BGM_0080 = 8;

						internal const int BGM_0090 = 9;

						internal const int BGM_0100 = 10;

						internal const int BGM_0110 = 11;

						internal const int BGM_0120 = 12;

						internal const int BGM_0130 = 13;

						internal const int BGM_0140 = 14;

						internal const int BGM_0150 = 15;

						internal const int BGM_0160 = 16;

						internal const int BGM_0170 = 17;

						internal const int BGM_0180 = 18;

						internal const int BGM_0190 = 19;

						internal const int BGM_0200 = 20;

						internal const int BGM_0210 = 21;

						internal const int BGM_0220 = 22;

						internal const int BGM_0230 = 23;

						internal const int BGM_0240 = 24;

						internal const int BGM_0250 = 25;

						internal const int BGM_0260 = 26;

						internal const int BGM_0270 = 27;

						internal const int BGM_0280 = 28;

						internal const int BGM_0290 = 29;

						internal const int BGM_0300 = 30;

						internal const int BGM_0310 = 31;

						internal const int BGM_0320 = 32;

						internal const int BGM_0330 = 33;

						internal const int BGM_0340 = 34;

						internal const int BGM_0350 = 35;

						internal const int BGM_0360 = 36;

						internal const int BGM_0370 = 37;

						internal const int BGM_0380 = 38;

						internal const int BGM_0390 = 39;

						internal const int BGM_0400 = 40;

						internal const int BGM_0410 = 41;

						internal const int BGM_0420 = 42;

						internal const int BGM_0430 = 43;

						internal const int BGM_0440 = 44;

						internal const int BGM_0450 = 45;

						internal const int BGM_0460 = 46;

						internal const int BGM_0470 = 47;

						internal const int BGM_0480 = 48;

						internal const int BGM_0490 = 49;

						internal const int BGM_0500 = 50;

						internal const int BGM_0510 = 51;

						internal const int BGM_0520 = 52;

						internal const int BGM_0530 = 53;

						internal const int BGM_0540 = 54;

						internal const int BGM_0550 = 55;

						internal const int BGM_0560 = 56;

						internal const int BGM_0570 = 57;

						internal const int BGM_0580 = 58;

						internal const int BGM_0590 = 59;

						internal const int SE_INFO_MAX = 4;

						internal const int SE_0010 = 0;

						internal const int SE_0020 = 1;

						internal const int SE_0030 = 2;

						internal const int SE_0040 = 3;

						internal const int SE_HOGE = 4;

						internal const int OP = 2;

						internal const int FLAG = 4;

						internal const int VALUE = 4;

						internal const int INTEGER = 4;

						internal const int FLOAT = 4;

						internal const int POSITION = 12;

						internal const int VECTOR = 12;

						internal const int RANGE = 44;

						internal const int LABEL = 4;

						internal const int MESSAGE = 4;

						internal const int SELECT = 4;

						internal const int __BYTE = 1;

						internal const int __WORD = 2;

						internal const int CAST = 2;

						internal const int FUNCTION = 8;

						internal const int STRING = 16;

						internal const int PAGE_0 = 0;

						internal const int PAGE_1 = 1;

						internal const int PAGE_TIPS_LIST = 2;

						internal const int PAGE_TIPS_TEXT = 3;

						internal const int LINK_Secret_of_Mana = 0;

						internal const int LINK_CHAOS_RINGS = 1;

						internal const int LINK_FINAL_FANTASY = 2;

						internal const int LINK_FINAL_FANTASY_II = 3;

						internal const int LINK_FFVII_Compilation_Wallpaper = 4;

						internal const int LINK_FINAL_FANTASY_VII_ACC_Gallery = 5;

						internal const int LINK_FINAL_FANTASY_XIII_Gallery = 6;

						internal const int LINK_Voice_Fantasy = 7;

						internal const int LINK_SONG_SUMMONER = 8;

						internal const int LINK_Hills_and_Rivers_Remain = 9;

						internal const int LINK_CRYSTAL_DEFENDERS = 10;

						internal const int LINK_VANGUARD_STORM = 11;

						internal const int LINK_SLIDING_HEROES = 12;

						internal const int LINK_FULLMETAL_ALCHEMIST_Gallery = 13;

						internal const int LINK_CHAOS_RINGS_for_iPad = 14;

						internal const int LINK_FINAL_FANTASY_XIII_Gallery_for_iPad = 15;

						internal const int LINK_Chocobo_Panic = 16;

						internal const int LINK_CRYSTAL_DEFENDERS_for_iPad = 17;

						internal const int TEXT_X = 408;

						internal const int TEXT_Y = 76;

						internal const int TEXT_H = 24;

						internal const int WEEK_POINT_SANDA = 1;

						internal const int WEEK_POINT_FAIA = 2;

						internal const int WEEK_POINT_ICE = 4;

						internal const int WEEK_POINT_WOTER = 8;

						internal const int WEEK_POINT_EARTH = 16;

						internal const int OFFSET_TO_USERDATA = 0;

						internal const int OFFSET_TO_FRIENDLIST = 0;

						internal const int OFFSET_TO_DATETIME = 0;

						internal const int SIZE_OF_USERDATA = 0;

						internal const int SIZE_OF_FRIENDLIST = 0;

						internal const int SIZE_OF_DATETIME = 36;

						internal const int SIZE_OF_CRC_AREA = 0;

						internal const int MNM_INITIAL_CODE = 1246119489;

						internal const int EUREKA_GGID = 582;

						internal const int NUM_MAX_CHILD = 1;

						internal const int DEFAULT_CHANNEL = 1;

						internal const int DEFAULT_PORT = 5;

						internal const int PARENT_DATA_SIZE = 448;

						internal const int CHILD_DATA_SIZE = 448;

						internal const int DEFAULT_SCAN_TMIE = 60;

						internal const int DEFAULT_BUFFER_SIZE = 512;

						internal const int MN_SIZE_GAMEINFO = 32;

						internal const int PARENT_LIFETIME = 450;

						internal const int MN_TIMEOUT = 1800;

						internal const int LOCALE_ID_J = 1041;

						internal const int LOCALE_ID_N = 1033;

						internal const int LOCALE_ID_UNKNOWN = 0;

						internal const int LOCALE_ID = 1041;

						internal const int MOGNET_EVENT_TRIGGER = 479;

						internal const int MOGNET_EVENTFLAG_HEAD = 480;

						internal const int NUM_OF_MOGNET_EVENTFLAGS = 10;

						internal const int MOGNET_MASTER_FLAG_HEAD = 450;

						internal const int NUMBER_OF_MASTER_FLAGS = 24;

						internal const int BATTLE_COMMAND_W = 128;

						internal const int BATTLE_COMMAND_H = 40;

						internal const int BATTLE_TARGET_W = 144;

						internal const int BATTLE_TARGET_H = 40;

						internal const int ADD_RATE = 5;

						internal const int ENCOUNT = 0;

						internal const int BATTLE_PLAYER_X = 284;

						internal const int BATTLE_PLAYER_HP_X = 424;

						internal const int BATTLE_PLAYER_H = 16;

						internal const int DEAD_START_FLASH_FRAME_MAX = 10;

						internal const int DEAD_START_FRAME_MAX = 10;

						internal const int PLAYER_BGM = 0;

						internal const int PLAYER_SE = 1;

						internal const int PLAYER_SYSTEM = 2;

						internal const int PLAYER_BATTLE = 3;

						internal const int PLAYER_MAGIC = 4;

						internal const int PLAYER_SUMMON = 5;

						internal const int PLAYER_CAMP = 6;

						internal const int PLAYER_FIELD = 7;

						internal const int WAVE_BGM00 = 0;

						internal const int WAVE_BGM01 = 1;

						internal const int WAVE_BGM02 = 2;

						internal const int WAVE_BGM03 = 3;

						internal const int WAVE_BGM04 = 4;

						internal const int WAVE_BGM05 = 5;

						internal const int WAVE_BGM06 = 6;

						internal const int WAVE_BGM07 = 7;

						internal const int WAVE_BGM08 = 8;

						internal const int WAVE_BGM09 = 9;

						internal const int WAVE_BGM10 = 10;

						internal const int WAVE_BGM11 = 11;

						internal const int WAVE_BGM12 = 12;

						internal const int WAVE_BGM13 = 13;

						internal const int WAVE_BGM14 = 14;

						internal const int WAVE_BGM15 = 15;

						internal const int WAVE_BGM16 = 16;

						internal const int WAVE_BGM17 = 17;

						internal const int WAVE_BGM18 = 18;

						internal const int WAVE_BGM19 = 19;

						internal const int WAVE_BGM20 = 20;

						internal const int WAVE_BGM21 = 21;

						internal const int WAVE_BGM22 = 22;

						internal const int WAVE_BGM23 = 23;

						internal const int WAVE_BGM24 = 24;

						internal const int WAVE_BGM25 = 25;

						internal const int WAVE_BGM26 = 26;

						internal const int WAVE_BGM27 = 27;

						internal const int WAVE_BGM28 = 28;

						internal const int WAVE_BGM29 = 29;

						internal const int WAVE_BGM30 = 30;

						internal const int WAVE_BGM31 = 31;

						internal const int WAVE_BGM32 = 32;

						internal const int WAVE_BGM33 = 33;

						internal const int WAVE_BGM34 = 34;

						internal const int WAVE_BGM35 = 35;

						internal const int WAVE_BGM36 = 36;

						internal const int WAVE_BGM37 = 37;

						internal const int WAVE_BGM38 = 38;

						internal const int WAVE_BGM39 = 39;

						internal const int WAVE_BGM40 = 40;

						internal const int WAVE_BGM41 = 41;

						internal const int WAVE_BGM42 = 42;

						internal const int WAVE_BGM43 = 43;

						internal const int WAVE_BGM44 = 44;

						internal const int WAVE_BGM45 = 45;

						internal const int WAVE_BGM46 = 46;

						internal const int WAVE_BGM47 = 47;

						internal const int WAVE_BGM48 = 48;

						internal const int WAVE_BGM49 = 49;

						internal const int WAVE_BGM50 = 50;

						internal const int WAVE_BGM51 = 51;

						internal const int WAVE_BGM52 = 52;

						internal const int WAVE_BGM53 = 53;

						internal const int WAVE_BGM54 = 54;

						internal const int WAVE_BGM55 = 55;

						internal const int WAVE_BGM56 = 56;

						internal const int WAVE_BGM57 = 57;

						internal const int WAVE_BGM58 = 58;

						internal const int WAVE_SE = 59;

						internal const int WAVE_MENU = 60;

						internal const int WAVE_FIELD = 61;

						internal const int WAVE_SAIDAN = 62;

						internal const int WAVE_KAZUSU = 63;

						internal const int WAVE_SASOON = 64;

						internal const int WAVE_BAHAMUT = 65;

						internal const int WAVE_VIKING = 66;

						internal const int WAVE_NEPT = 67;

						internal const int WAVE_OWEN = 68;

						internal const int WAVE_DWARF = 69;

						internal const int WAVE_TITEIKO = 70;

						internal const int WAVE_GISARL = 71;

						internal const int WAVE_HEIN = 72;

						internal const int WAVE_WATER = 73;

						internal const int WAVE_CLOACA = 74;

						internal const int WAVE_MAGICDANGEON = 75;

						internal const int WAVE_GOLD = 76;

						internal const int WAVE_SARONIA = 77;

						internal const int WAVE_ANCIENT = 78;

						internal const int WAVE_CRYSTAL = 79;

						internal const int WAVE_FUUIN = 80;

						internal const int WAVE_FIRE = 81;

						internal const int WAVE_KANAN = 82;

						internal const int WAVE_UNE = 83;

						internal const int WAVE_YAMI = 84;

						internal const int WAVE_YAMI2 = 85;

						internal const int WAVE_TETSU = 86;

						internal const int WAVE_WORLD1 = 87;

						internal const int WAVE_WORLD2 = 88;

						internal const int WAVE_WORLD4 = 89;

						internal const int WAVE_MARSH = 90;

						internal const int WAVE_WORLD5 = 91;

						internal const int WAVE_ANCIENTS = 92;

						internal const int WAVE_END3 = 93;

						internal const int WAVE_MONZUKAN = 94;

						internal const int WAVE_WIFI = 95;

						internal const int WAVE_TITLE = 96;

						internal const int WAVE_CAMP = 97;

						internal const int WAVE_E3OP = 98;

						internal const int WAVE_BATTLE = 99;

						internal const int WAVE_PHYSIC = 100;

						internal const int WAVE_SPECIAL = 101;

						internal const int WAVE_ABILITY = 102;

						internal const int WAVE_GEO = 103;

						internal const int WAVE_SONG = 104;

						internal const int WAVE_SUPPORT = 105;

						internal const int WAVE_ITEM_BATTLE = 106;

						internal const int WAVE_WHITE1 = 107;

						internal const int WAVE_WHITE2 = 108;

						internal const int WAVE_WHITE3 = 109;

						internal const int WAVE_WHITE4 = 110;

						internal const int WAVE_WHITE5 = 111;

						internal const int WAVE_WHITE6 = 112;

						internal const int WAVE_WHITE7 = 113;

						internal const int WAVE_WHITE8 = 114;

						internal const int WAVE_BLACK1 = 115;

						internal const int WAVE_BLACK2 = 116;

						internal const int WAVE_BLACK3 = 117;

						internal const int WAVE_BLACK4 = 118;

						internal const int WAVE_BLACK5 = 119;

						internal const int WAVE_BLACK6 = 120;

						internal const int WAVE_BLACK7 = 121;

						internal const int WAVE_BLACK8 = 122;

						internal const int WAVE_SUMMON1 = 123;

						internal const int WAVE_SUMMON2 = 124;

						internal const int WAVE_SUMMON3 = 125;

						internal const int WAVE_SUMMON4 = 126;

						internal const int WAVE_SUMMON5 = 127;

						internal const int WAVE_SUMMON6 = 128;

						internal const int WAVE_SUMMON7 = 129;

						internal const int WAVE_SUMMON8 = 130;

						internal const int WAVE_TEST = 131;

						internal const int WAVE_TEST01 = 132;

						internal const int BANK_BGM00 = 0;

						internal const int BANK_BGM01 = 1;

						internal const int BANK_BGM02 = 2;

						internal const int BANK_BGM03 = 3;

						internal const int BANK_BGM04 = 4;

						internal const int BANK_BGM05 = 5;

						internal const int BANK_BGM06 = 6;

						internal const int BANK_BGM07 = 7;

						internal const int BANK_BGM08 = 8;

						internal const int BANK_BGM09 = 9;

						internal const int BANK_BGM10 = 10;

						internal const int BANK_BGM11 = 11;

						internal const int BANK_BGM12 = 12;

						internal const int BANK_BGM13 = 13;

						internal const int BANK_BGM14 = 14;

						internal const int BANK_BGM15 = 15;

						internal const int BANK_BGM16 = 16;

						internal const int BANK_BGM17 = 17;

						internal const int BANK_BGM18 = 18;

						internal const int BANK_BGM19 = 19;

						internal const int BANK_BGM20 = 20;

						internal const int BANK_BGM21 = 21;

						internal const int BANK_BGM22 = 22;

						internal const int BANK_BGM23 = 23;

						internal const int BANK_BGM24 = 24;

						internal const int BANK_BGM25 = 25;

						internal const int BANK_BGM26 = 26;

						internal const int BANK_BGM27 = 27;

						internal const int BANK_BGM28 = 28;

						internal const int BANK_BGM29 = 29;

						internal const int BANK_BGM30 = 30;

						internal const int BANK_BGM31 = 31;

						internal const int BANK_BGM32 = 32;

						internal const int BANK_BGM33 = 33;

						internal const int BANK_BGM34 = 34;

						internal const int BANK_BGM35 = 35;

						internal const int BANK_BGM36 = 36;

						internal const int BANK_BGM37 = 37;

						internal const int BANK_BGM38 = 38;

						internal const int BANK_BGM39 = 39;

						internal const int BANK_BGM40 = 40;

						internal const int BANK_BGM41 = 41;

						internal const int BANK_BGM42 = 42;

						internal const int BANK_BGM43 = 43;

						internal const int BANK_BGM44 = 44;

						internal const int BANK_BGM45 = 45;

						internal const int BANK_BGM46 = 46;

						internal const int BANK_BGM47 = 47;

						internal const int BANK_BGM48 = 48;

						internal const int BANK_BGM49 = 49;

						internal const int BANK_BGM50 = 50;

						internal const int BANK_BGM51 = 51;

						internal const int BANK_BGM52 = 52;

						internal const int BANK_BGM53 = 53;

						internal const int BANK_BGM54 = 54;

						internal const int BANK_BGM55 = 55;

						internal const int BANK_BGM56 = 56;

						internal const int BANK_BGM57 = 57;

						internal const int BANK_BGM58 = 58;

						internal const int BANK_SE = 59;

						internal const int BANK_MENU = 60;

						internal const int BANK_FIELD = 61;

						internal const int BANK_SAIDAN = 62;

						internal const int BANK_KAZUSU = 63;

						internal const int BANK_SASOON = 64;

						internal const int BANK_BAHAMUT = 65;

						internal const int BANK_VIKING = 66;

						internal const int BANK_NEPT = 67;

						internal const int BANK_OWEN = 68;

						internal const int BANK_DWARF = 69;

						internal const int BANK_TITEIKO = 70;

						internal const int BANK_GISARL = 71;

						internal const int BANK_HEIN = 72;

						internal const int BANK_WATER = 73;

						internal const int BANK_CLOACA = 74;

						internal const int BANK_MAGICDANGEON = 75;

						internal const int BANK_GOLD = 76;

						internal const int BANK_SARONIA = 77;

						internal const int BANK_ANCIENT = 78;

						internal const int BANK_CRYSTAL = 79;

						internal const int BANK_FUUIN = 80;

						internal const int BANK_FIRE = 81;

						internal const int BANK_KANAN = 82;

						internal const int BANK_UNE = 83;

						internal const int BANK_YAMI = 84;

						internal const int BANK_YAMI2 = 85;

						internal const int BANK_TETSU = 86;

						internal const int BANK_WORLD1 = 87;

						internal const int BANK_WORLD2 = 88;

						internal const int BANK_WORLD4 = 89;

						internal const int BANK_MARSH = 90;

						internal const int BANK_WORLD5 = 91;

						internal const int BANK_ANCIENTS = 92;

						internal const int BANK_END3 = 93;

						internal const int BANK_MONZUKAN = 94;

						internal const int BANK_WIFI = 95;

						internal const int BANK_TITLE = 96;

						internal const int BANK_CAMP = 97;

						internal const int BANK_E3OP = 98;

						internal const int BANK_BATTLE = 99;

						internal const int BANK_PHYSIC = 100;

						internal const int BANK_SPECIAL = 101;

						internal const int BANK_ABILITY = 102;

						internal const int BANK_GEO = 103;

						internal const int BANK_SONG = 104;

						internal const int BANK_SUPPORT = 105;

						internal const int BANK_ITEM_BATTLE = 106;

						internal const int BANK_WHITE1 = 107;

						internal const int BANK_WHITE2 = 108;

						internal const int BANK_WHITE3 = 109;

						internal const int BANK_WHITE4 = 110;

						internal const int BANK_WHITE5 = 111;

						internal const int BANK_WHITE6 = 112;

						internal const int BANK_WHITE7 = 113;

						internal const int BANK_WHITE8 = 114;

						internal const int BANK_BLACK1 = 115;

						internal const int BANK_BLACK2 = 116;

						internal const int BANK_BLACK3 = 117;

						internal const int BANK_BLACK4 = 118;

						internal const int BANK_BLACK5 = 119;

						internal const int BANK_BLACK6 = 120;

						internal const int BANK_BLACK7 = 121;

						internal const int BANK_BLACK8 = 122;

						internal const int BANK_SUMMON1 = 123;

						internal const int BANK_SUMMON2 = 124;

						internal const int BANK_SUMMON3 = 125;

						internal const int BANK_SUMMON4 = 126;

						internal const int BANK_SUMMON5 = 127;

						internal const int BANK_SUMMON6 = 128;

						internal const int BANK_SUMMON7 = 129;

						internal const int BANK_SUMMON8 = 130;

						internal const int BGM00_MID = 0;

						internal const int BGM01 = 1;

						internal const int BGM02 = 2;

						internal const int BGM03 = 3;

						internal const int BGM04 = 4;

						internal const int BGM05 = 5;

						internal const int BGM06 = 6;

						internal const int BGM07 = 7;

						internal const int BGM08 = 8;

						internal const int BGM09 = 9;

						internal const int BGM10 = 10;

						internal const int BGM11 = 11;

						internal const int BGM12 = 12;

						internal const int BGM13 = 13;

						internal const int BGM14 = 14;

						internal const int BGM15 = 15;

						internal const int BGM16 = 16;

						internal const int BGM17 = 17;

						internal const int BGM18 = 18;

						internal const int BGM19 = 19;

						internal const int BGM20 = 20;

						internal const int BGM21 = 21;

						internal const int BGM22 = 22;

						internal const int BGM23 = 23;

						internal const int BGM24 = 24;

						internal const int BGM25 = 25;

						internal const int BGM26 = 26;

						internal const int BGM27 = 27;

						internal const int BGM28 = 28;

						internal const int BGM29 = 29;

						internal const int BGM30 = 30;

						internal const int BGM31 = 31;

						internal const int BGM32 = 32;

						internal const int BGM33 = 33;

						internal const int BGM34 = 34;

						internal const int BGM35 = 35;

						internal const int BGM36 = 36;

						internal const int BGM37 = 37;

						internal const int BGM38 = 38;

						internal const int BGM39 = 39;

						internal const int BGM40 = 40;

						internal const int BGM41 = 41;

						internal const int BGM42 = 42;

						internal const int BGM43 = 43;

						internal const int BGM44 = 44;

						internal const int BGM45 = 45;

						internal const int BGM46 = 46;

						internal const int BGM47 = 47;

						internal const int BGM48 = 48;

						internal const int BGM49 = 49;

						internal const int BGM50 = 50;

						internal const int BGM51 = 51;

						internal const int BGM52 = 52;

						internal const int BGM53 = 53;

						internal const int BGM54 = 54;

						internal const int BGM55 = 55;

						internal const int BGM56 = 56;

						internal const int BGM57 = 57;

						internal const int BGM58 = 58;

						internal const int SEQ_SE_MENU = 0;

						internal const int SEQ_SE_FIELD = 1;

						internal const int SEQ_SE_SAIDAN = 2;

						internal const int SEQ_SE_KAZUSU = 3;

						internal const int SEQ_SE_SASOON = 4;

						internal const int SEQ_SE_BAHAMUT = 5;

						internal const int SEQ_SE_VIKING = 6;

						internal const int SEQ_SE_NEPT = 7;

						internal const int SEQ_SE_OWEN = 8;

						internal const int SEQ_SE_DWARF = 9;

						internal const int SEQ_SE_GISARL = 10;

						internal const int SEQ_SE_HEIN = 11;

						internal const int SEQ_SE_WATER = 12;

						internal const int SEQ_SE_CLOACA = 13;

						internal const int SEQ_SE_MAGICDANGEON = 14;

						internal const int SEQ_SE_GOLD = 15;

						internal const int SEQ_SE_SARONIA = 16;

						internal const int SEQ_SE_ANCIENT = 17;

						internal const int SEQ_SE_CRYSTAL = 18;

						internal const int SEQ_SE_FUUIN = 19;

						internal const int SEQ_SE_FIRE = 20;

						internal const int SEQ_SE_KANAN = 21;

						internal const int SEQ_SE_UNE = 22;

						internal const int SEQ_SE_YAMI = 23;

						internal const int SEQ_SE_YAMI2 = 24;

						internal const int SEQ_SE_TETSU = 25;

						internal const int SEQ_SE_WORLD1 = 30;

						internal const int SEQ_SE_WORLD2 = 31;

						internal const int SEQ_SE_WORLD4 = 32;

						internal const int SEQ_SE_MARSH = 33;

						internal const int SEQ_SE_WORLD5 = 34;

						internal const int SEQ_SE_ANCIENTS = 35;

						internal const int SEQ_SE_END3 = 94;

						internal const int SEQ_SE_MONZUKAN = 95;

						internal const int SEQ_SE_WIFI = 96;

						internal const int SEQ_SE_TITLE = 97;

						internal const int SEQ_SE_CAMP = 98;

						internal const int SEQ_SE_E3OP = 99;

						internal const int SEQ_SE_BATTLE = 200;

						internal const int SEQ_SE_PHYSIC = 201;

						internal const int SEQ_SE_SPECIAL = 202;

						internal const int SEQ_SE_ABILITY = 203;

						internal const int SEQ_SE_GEO = 204;

						internal const int SEQ_SE_SONG = 205;

						internal const int SEQ_SE_SUPPORT = 206;

						internal const int SEQ_SE_ITEM_BATTLE = 210;

						internal const int SEQ_SE_WHITE1 = 250;

						internal const int SEQ_SE_WHITE2 = 251;

						internal const int SEQ_SE_WHITE3 = 252;

						internal const int SEQ_SE_WHITE4 = 253;

						internal const int SEQ_SE_WHITE5 = 254;

						internal const int SEQ_SE_WHITE6 = 255;

						internal const int SEQ_SE_WHITE7 = 256;

						internal const int SEQ_SE_WHITE8 = 257;

						internal const int SEQ_SE_BLACK1 = 260;

						internal const int SEQ_SE_BLACK2 = 261;

						internal const int SEQ_SE_BLACK3 = 262;

						internal const int SEQ_SE_BLACK4 = 263;

						internal const int SEQ_SE_BLACK5 = 264;

						internal const int SEQ_SE_BLACK6 = 265;

						internal const int SEQ_SE_BLACK7 = 266;

						internal const int SEQ_SE_BLACK8 = 267;

						internal const int SEQ_SE_SUMMON1 = 270;

						internal const int SEQ_SE_SUMMON2 = 271;

						internal const int SEQ_SE_SUMMON3 = 272;

						internal const int SEQ_SE_SUMMON4 = 273;

						internal const int SEQ_SE_SUMMON5 = 274;

						internal const int SEQ_SE_SUMMON6 = 275;

						internal const int SEQ_SE_SUMMON7 = 276;

						internal const int SEQ_SE_SUMMON8 = 277;

						internal const int GROUP_SE_MENU = 0;

						internal const int GROUP_SE_FIELD = 1;

						internal const int GROUP_SE_SAIDAN = 2;

						internal const int GROUP_SE_KAZUSU = 3;

						internal const int GROUP_SE_SASOON = 4;

						internal const int GROUP_SE_BAHAMUT = 5;

						internal const int GROUP_SE_VIKING = 6;

						internal const int GROUP_SE_NEPT = 7;

						internal const int GROUP_SE_OWEN = 8;

						internal const int GROUP_SE_DWARF = 9;

						internal const int GROUP_SE_GISARL = 10;

						internal const int GROUP_SE_HEIN = 11;

						internal const int GROUP_SE_WATER = 12;

						internal const int GROUP_SE_CLOACA = 13;

						internal const int GROUP_SE_MAGICDANGEON = 14;

						internal const int GROUP_SE_GOLD = 15;

						internal const int GROUP_SE_SARONIA = 16;

						internal const int GROUP_SE_ANCIENT = 17;

						internal const int GROUP_SE_CRYSTAL = 18;

						internal const int GROUP_SE_FUUIN = 19;

						internal const int GROUP_SE_FIRE = 20;

						internal const int GROUP_SE_KANAN = 21;

						internal const int GROUP_SE_UNE = 22;

						internal const int GROUP_SE_YAMI = 23;

						internal const int GROUP_SE_YAMI2 = 24;

						internal const int GROUP_SE_TETSU = 25;

						internal const int GROUP_SE_WORLD1 = 30;

						internal const int GROUP_SE_WORLD2 = 31;

						internal const int GROUP_SE_WORLD4 = 32;

						internal const int GROUP_SE_MARSH = 33;

						internal const int GROUP_SE_WORLD5 = 34;

						internal const int GROUP_SE_ANCIENTS = 35;

						internal const int GROUP_SE_END3 = 94;

						internal const int GROUP_SE_MONZUKAN = 95;

						internal const int GROUP_SE_WIFI = 96;

						internal const int GROUP_SE_CAMP = 98;

						internal const int GROUP_SE_E3OP = 99;

						internal const int GROUP_BGM_00 = 100;

						internal const int GROUP_BGM_01 = 101;

						internal const int GROUP_BGM_02 = 102;

						internal const int GROUP_BGM_03 = 103;

						internal const int GROUP_BGM_04 = 104;

						internal const int GROUP_BGM_05 = 105;

						internal const int GROUP_BGM_06 = 106;

						internal const int GROUP_BGM_07 = 107;

						internal const int GROUP_BGM_08 = 108;

						internal const int GROUP_BGM_09 = 109;

						internal const int GROUP_BGM_10 = 110;

						internal const int GROUP_BGM_11 = 111;

						internal const int GROUP_BGM_12 = 112;

						internal const int GROUP_BGM_13 = 113;

						internal const int GROUP_BGM_14 = 114;

						internal const int GROUP_BGM_15 = 115;

						internal const int GROUP_BGM_16 = 116;

						internal const int GROUP_BGM_17 = 117;

						internal const int GROUP_BGM_18 = 118;

						internal const int GROUP_BGM_19 = 119;

						internal const int GROUP_BGM_20 = 120;

						internal const int GROUP_BGM_21 = 121;

						internal const int GROUP_BGM_22 = 122;

						internal const int GROUP_BGM_23 = 123;

						internal const int GROUP_BGM_24 = 124;

						internal const int GROUP_BGM_25 = 125;

						internal const int GROUP_BGM_26 = 126;

						internal const int GROUP_BGM_27 = 127;

						internal const int GROUP_BGM_28 = 128;

						internal const int GROUP_BGM_29 = 129;

						internal const int GROUP_BGM_30 = 130;

						internal const int GROUP_BGM_31 = 131;

						internal const int GROUP_BGM_32 = 132;

						internal const int GROUP_BGM_33 = 133;

						internal const int GROUP_BGM_34 = 134;

						internal const int GROUP_BGM_35 = 135;

						internal const int GROUP_BGM_36 = 136;

						internal const int GROUP_BGM_37 = 137;

						internal const int GROUP_BGM_38 = 138;

						internal const int GROUP_BGM_39 = 139;

						internal const int GROUP_BGM_40 = 140;

						internal const int GROUP_BGM_41 = 141;

						internal const int GROUP_BGM_42 = 142;

						internal const int GROUP_BGM_43 = 143;

						internal const int GROUP_BGM_44 = 144;

						internal const int GROUP_BGM_45 = 145;

						internal const int GROUP_BGM_46 = 146;

						internal const int GROUP_BGM_47 = 147;

						internal const int GROUP_BGM_48 = 148;

						internal const int GROUP_BGM_49 = 149;

						internal const int GROUP_BGM_50 = 150;

						internal const int GROUP_BGM_51 = 151;

						internal const int GROUP_BGM_52 = 152;

						internal const int GROUP_BGM_53 = 153;

						internal const int GROUP_BGM_54 = 154;

						internal const int GROUP_BGM_55 = 155;

						internal const int GROUP_BGM_56 = 156;

						internal const int GROUP_BGM_57 = 157;

						internal const int GROUP_BGM_58 = 158;

						internal const int GROUP_SE_BATTLE = 200;

						internal const int GROUP_SE_PHYSIC = 201;

						internal const int GROUP_SE_SPECIAL = 202;

						internal const int GROUP_SE_ABILITY = 203;

						internal const int GROUP_SE_GEO = 204;

						internal const int GROUP_SE_SONG = 205;

						internal const int GROUP_SE_SUPPORT = 206;

						internal const int GROUP_SE_ITEM_BATTLE = 210;

						internal const int GROUP_SE_WHITE1 = 250;

						internal const int GROUP_SE_WHITE2 = 251;

						internal const int GROUP_SE_WHITE3 = 252;

						internal const int GROUP_SE_WHITE4 = 253;

						internal const int GROUP_SE_WHITE5 = 254;

						internal const int GROUP_SE_WHITE6 = 255;

						internal const int GROUP_SE_WHITE7 = 256;

						internal const int GROUP_SE_WHITE8 = 257;

						internal const int GROUP_SE_BLACK1 = 260;

						internal const int GROUP_SE_BLACK2 = 261;

						internal const int GROUP_SE_BLACK3 = 262;

						internal const int GROUP_SE_BLACK4 = 263;

						internal const int GROUP_SE_BLACK5 = 264;

						internal const int GROUP_SE_BLACK6 = 265;

						internal const int GROUP_SE_BLACK7 = 266;

						internal const int GROUP_SE_BLACK8 = 267;

						internal const int GROUP_SE_SUMMON1 = 270;

						internal const int GROUP_SE_SUMMON2 = 271;

						internal const int GROUP_SE_SUMMON3 = 272;

						internal const int GROUP_SE_SUMMON4 = 273;

						internal const int GROUP_SE_SUMMON5 = 274;

						internal const int GROUP_SE_SUMMON6 = 275;

						internal const int GROUP_SE_SUMMON7 = 276;

						internal const int GROUP_SE_SUMMON8 = 277;

						internal const int SE_MENU_BEEP = 0;

						internal const int SE_MENU_01 = 1;

						internal const int SE_MENU_02 = 2;

						internal const int SE_MENU_03 = 3;

						internal const int SE_MENU_06 = 6;

						internal const int SE_MENU_07 = 7;

						internal const int SE_FIELDMOVE_04 = 9;

						internal const int SE_FIELDMOVE_05 = 10;

						internal const int SE_FIELDMOVE_06 = 11;

						internal const int SE_WORLD3_01 = 22;

						internal const int SE_TOWN_06 = 31;

						internal const int SE_SPECIAL_05 = 40;

						internal const int SE_16CH_ALLMUTE = 41;

						internal const int SE_FIELDGEN_01 = 0;

						internal const int SE_FIELDGEN_02 = 1;

						internal const int SE_FIELDGEN_03 = 2;

						internal const int SE_FIELDGEN_04 = 3;

						internal const int SE_FIELDMOVE_01 = 6;

						internal const int SE_FIELDMOVE_02 = 7;

						internal const int SE_FIELDMOVE_03 = 8;

						internal const int SE_FIELDMOVE_07 = 12;

						internal const int SE_FIELDMOVE_08 = 13;

						internal const int SE_FIELDMOVE_09 = 14;

						internal const int SE_FIELDMOVE_10 = 15;

						internal const int SE_FIELDMOVE_11 = 16;

						internal const int SE_FIELDMOVE_12 = 17;

						internal const int SE_FIELDMOVE_13 = 18;

						internal const int SE_TOWN_01 = 24;

						internal const int SE_TOWN_02 = 25;

						internal const int SE_TOWN_03 = 26;

						internal const int SE_TOWN_05 = 28;

						internal const int SE_TOWN_12 = 29;

						internal const int SE_TOWN_07 = 32;

						internal const int SE_TOWN_08 = 33;

						internal const int SE_TOWN_09 = 34;

						internal const int SE_EVENT_04 = 35;

						internal const int SE_EVENT_05 = 36;

						internal const int SE_EVENT_06 = 37;

						internal const int SE_EVENT_07 = 38;

						internal const int SE_EVENT_08 = 39;

						internal const int SE_EVENT_09 = 40;

						internal const int SE_EVENT_10 = 41;

						internal const int SE_EVENT_11 = 42;

						internal const int SE_EVENT_12 = 43;

						internal const int SE_EVENT_13 = 44;

						internal const int SE_EVENT_14 = 45;

						internal const int SE_EVENT_15 = 46;

						internal const int SE_EVENT_16 = 47;

						internal const int SE_EVENT_17 = 48;

						internal const int SE_TOWN_10 = 49;

						internal const int SE_TOWN_11 = 50;

						internal const int SE_EVENT_18 = 51;

						internal const int SE_EVENT_19 = 52;

						internal const int SE_EVENT_20 = 53;

						internal const int SE_EVENT_21 = 54;

						internal const int SE_SAIDAN_01 = 0;

						internal const int SE_SAIDAN_02 = 1;

						internal const int SE_SAIDAN_03 = 2;

						internal const int SE_SAIDAN_04 = 3;

						internal const int SE_SAIDAN_05 = 4;

						internal const int SE_SAIDAN_06 = 5;

						internal const int SE_SAIDAN_07 = 6;

						internal const int SE_SAIDAN_09 = 8;

						internal const int SE_SAIDAN_10 = 9;

						internal const int SE_SAIDAN_11 = 10;

						internal const int SE_SAIDAN_13 = 12;

						internal const int SE_KAZUSU_01 = 0;

						internal const int SE_KAZUSU_02 = 1;

						internal const int SE_SASOON_01 = 0;

						internal const int SE_SASOON_02 = 1;

						internal const int SE_SASOON_03 = 2;

						internal const int SE_BAHAMUT_01 = 0;

						internal const int SE_BAHAMUT_02 = 1;

						internal const int SE_BAHAMUT_04 = 3;

						internal const int SE_BAHAMUT_06 = 5;

						internal const int SE_BAHAMUT_07 = 6;

						internal const int SE_VIKING_01 = 0;

						internal const int SE_VIKING_02 = 1;

						internal const int SE_NEPT_01 = 0;

						internal const int SE_NEPT_02 = 1;

						internal const int SE_NEPT_03 = 2;

						internal const int SE_NEPT_04 = 3;

						internal const int SE_OWEN_03 = 2;

						internal const int SE_OWEN_05 = 4;

						internal const int SE_OWEN_06 = 5;

						internal const int SE_DWARF_01 = 0;

						internal const int SE_DWARF_02 = 1;

						internal const int SE_DWARF_04 = 3;

						internal const int SE_DWARF_07 = 6;

						internal const int SE_DWARF_08 = 7;

						internal const int SE_DWARF_09 = 8;

						internal const int SE_DWARF_10 = 9;

						internal const int SE_DWARF_11 = 10;

						internal const int SE_GISARL_01 = 0;

						internal const int SE_GISARL_02 = 1;

						internal const int SE_HEIN_01 = 0;

						internal const int SE_HEIN_02 = 1;

						internal const int SE_HEIN_03 = 2;

						internal const int SE_HEIN_04 = 3;

						internal const int SE_HEIN_05 = 4;

						internal const int SE_HEIN_07 = 6;

						internal const int SE_HEIN_08 = 7;

						internal const int SE_WATER_02 = 1;

						internal const int SE_WATER_04 = 3;

						internal const int SE_WATER_05 = 4;

						internal const int SE_WATER_06 = 5;

						internal const int SE_WATER_07 = 6;

						internal const int SE_WATER_08 = 7;

						internal const int SE_WATER_09 = 8;

						internal const int SE_WATER_10 = 9;

						internal const int SE_CLOACA_01 = 0;

						internal const int SE_CLOACA_05 = 4;

						internal const int SE_CLOACA_06 = 5;

						internal const int SE_CLOACA_07 = 6;

						internal const int SE_CLOACA_08 = 7;

						internal const int SE_CLOACA_09 = 8;

						internal const int SE_MAGICDANGEON_01 = 0;

						internal const int SE_MAGICDANGEON_02 = 1;

						internal const int SE_MAGICDANGEON_04 = 3;

						internal const int SE_MAGICDANGEON_05 = 4;

						internal const int SE_MAGICDANGEON_06 = 5;

						internal const int SE_MAGICDANGEON_07 = 6;

						internal const int SE_MAGICDANGEON_08 = 7;

						internal const int SE_GOLD_01 = 0;

						internal const int SE_GOLD_02 = 1;

						internal const int SE_GOLD_03 = 2;

						internal const int SE_GOLD_04 = 3;

						internal const int SE_SARONIA_01 = 0;

						internal const int SE_SARONIA_03 = 2;

						internal const int SE_SARONIA_05 = 4;

						internal const int SE_SARONIA_06 = 5;

						internal const int SE_ANCIENT_01 = 0;

						internal const int SE_ANCIENT_02 = 1;

						internal const int SE_ANCIENT_03 = 2;

						internal const int SE_CRYSTAL_02 = 1;

						internal const int SE_CRYSTAL_03 = 2;

						internal const int SE_CRYSTAL_04 = 3;

						internal const int SE_CRYSTAL_05 = 4;

						internal const int SE_CRYSTAL_06 = 5;

						internal const int SE_CRYSTAL_07 = 6;

						internal const int SE_CRYSTAL_08 = 7;

						internal const int SE_CRYSTAL_09 = 8;

						internal const int SE_CRYSTAL_10 = 9;

						internal const int SE_CRYSTAL_11 = 10;

						internal const int SE_CRYSTAL_12 = 11;

						internal const int SE_CRYSTAL_16 = 15;

						internal const int SE_CRYSTAL_17 = 16;

						internal const int SE_CRYSTAL_18 = 17;

						internal const int SE_CRYSTAL_19 = 18;

						internal const int SE_CRYSTAL_21 = 20;

						internal const int SE_CRYSTAL_22 = 21;

						internal const int SE_FUUIN_01 = 0;

						internal const int SE_FUUIN_02 = 1;

						internal const int SE_FUUIN_03 = 2;

						internal const int SE_FUUIN_04 = 3;

						internal const int SE_FUUIN_05 = 4;

						internal const int SE_FIRE_01 = 0;

						internal const int SE_FIRE_02 = 1;

						internal const int SE_FIRE_03 = 2;

						internal const int SE_KANAN_01 = 0;

						internal const int SE_KANAN_02 = 1;

						internal const int SE_KANAN_03 = 2;

						internal const int SE_KANAN_04 = 3;

						internal const int SE_KANAN_05 = 4;

						internal const int SE_UNE_01 = 0;

						internal const int SE_UNE_02 = 1;

						internal const int SE_CRYSTAL_01 = 0;

						internal const int SE_CRYSTAL_13 = 12;

						internal const int SE_CRYSTAL_14 = 13;

						internal const int SE_CRYSTAL_15 = 14;

						internal const int SE_CRYSTAL_23 = 22;

						internal const int SE_CRYSTAL_20 = 19;

						internal const int SE_TETSU_01 = 0;

						internal const int SE_WORLD1_01 = 0;

						internal const int SE_WORLD1_02 = 1;

						internal const int SE_WORLD2_01 = 0;

						internal const int SE_WORLD2_02 = 1;

						internal const int SE_WORLD2_03 = 2;

						internal const int SE_WORLD2_04 = 3;

						internal const int SE_WORLD2_05 = 4;

						internal const int SE_WORLD2_06 = 5;

						internal const int SE_WORLD2_07 = 6;

						internal const int SE_WORLD4_01 = 0;

						internal const int SE_WORLD4_02 = 1;

						internal const int SE_WORLD4_03 = 2;

						internal const int SE_MARSH_01 = 0;

						internal const int SE_MARSH_02 = 1;

						internal const int SE_WATER_03_ = 2;

						internal const int SE_ANCIENTS_01 = 0;

						internal const int SE_END3_01 = 0;

						internal const int SE_MONZUKAN_01 = 0;

						internal const int SE_MONZUKAN_02 = 1;

						internal const int SE_MONZUKAN_03 = 2;

						internal const int SE_WIFI_01 = 0;

						internal const int SE_WIFI_02 = 1;

						internal const int SE_WIFI_03 = 2;

						internal const int SE_WIFI_04 = 3;

						internal const int SE_WIFI_06 = 4;

						internal const int SE_TITLE_BEEP = 0;

						internal const int SE_TITLE_01 = 1;

						internal const int SE_TITLE_02 = 2;

						internal const int SE_TITLE_03 = 3;

						internal const int SE_CAMP_01 = 0;

						internal const int SE_CAMP_02 = 1;

						internal const int SE_CAMP_03 = 2;

						internal const int SE_CAMP_04 = 3;

						internal const int SE_CAMP_05 = 4;

						internal const int SE_CAMP_06 = 5;

						internal const int SE_CAMP_07 = 6;

						internal const int SE_CAMP_11 = 10;

						internal const int SE_CAMP_12 = 11;

						internal const int SE_CAMP_13 = 12;

						internal const int SE_CAMP_14 = 13;

						internal const int SE_CAMP_15 = 14;

						internal const int SE_CAMP_16 = 15;

						internal const int SE_CAMP_17 = 16;

						internal const int SE_CAMP_18 = 17;

						internal const int SE_CAMP_20 = 19;

						internal const int SE_CAMP_21 = 20;

						internal const int SE_CAMP_31 = 30;

						internal const int SE_CAMP_32 = 31;

						internal const int SE_CAMP_34 = 33;

						internal const int SE_CAMP_35 = 34;

						internal const int SE_CAMP_36 = 35;

						internal const int SE_CAMP_38 = 37;

						internal const int SE_CAMP_40 = 39;

						internal const int SE_CAMP_41 = 40;

						internal const int SE_CAMP_42 = 41;

						internal const int SE_CAMP_43 = 42;

						internal const int SE_CAMP_44 = 43;

						internal const int SE_CAMP_45 = 44;

						internal const int SE_E3MOG_01 = 0;

						internal const int SE_E3MOG_02 = 1;

						internal const int SE_E3MOG_03 = 2;

						internal const int SE_BATTLE_01 = 0;

						internal const int SE_BATTLE_02 = 1;

						internal const int SE_BATTLE_03 = 2;

						internal const int SE_BATTLE_04 = 3;

						internal const int SE_BATTLE_05 = 4;

						internal const int SE_BATTLE_06 = 5;

						internal const int SE_BATTLE_07 = 6;

						internal const int SE_BATTLE_08 = 7;

						internal const int SE_BATTLE_09 = 8;

						internal const int SE_BATTLE_10 = 9;

						internal const int SE_BATTLE_11 = 10;

						internal const int SE_BATTLE_12 = 11;

						internal const int SE_BATTLE_13 = 12;

						internal const int SE_BATTLE_14 = 13;

						internal const int SE_BATTLE_15 = 14;

						internal const int SE_BATTLE_16 = 15;

						internal const int SE_BATTLE_17 = 16;

						internal const int SE_BATTLE_18 = 17;

						internal const int SE_PHYSIC_01 = 0;

						internal const int SE_PHYSIC_02 = 1;

						internal const int SE_PHYSIC_03 = 2;

						internal const int SE_PHYSIC_04 = 3;

						internal const int SE_PHYSIC_05 = 4;

						internal const int SE_PHYSIC_06 = 5;

						internal const int SE_PHYSIC_07 = 6;

						internal const int SE_PHYSIC_08 = 7;

						internal const int SE_PHYSIC_09 = 8;

						internal const int SE_PHYSIC_10 = 9;

						internal const int SE_PHYSIC_11 = 10;

						internal const int SE_PHYSIC_12 = 11;

						internal const int SE_PHYSIC_13 = 12;

						internal const int SE_PHYSIC_14 = 13;

						internal const int SE_PHYSIC_15 = 14;

						internal const int SE_PHYSIC_16 = 15;

						internal const int SE_PHYSIC_17 = 16;

						internal const int SE_PHYSIC_18 = 17;

						internal const int SE_PHYSIC_19 = 18;

						internal const int SE_PHYSIC_20 = 19;

						internal const int SE_PHYSIC_21 = 20;

						internal const int SE_PHYSIC_22 = 21;

						internal const int SE_PHYSIC_23 = 22;

						internal const int tume_old = 23;

						internal const int SE_SPECIAL_01 = 0;

						internal const int SE_SPECIAL_02 = 1;

						internal const int SE_SPECIAL_03 = 2;

						internal const int SE_SPECIAL_06 = 5;

						internal const int SE_SPECIAL_07 = 6;

						internal const int SE_SPECIAL_08 = 7;

						internal const int SE_SPECIAL_09 = 8;

						internal const int SE_SPECIAL_10 = 9;

						internal const int SE_SPECIAL_11 = 10;

						internal const int SE_SPECIAL_13 = 12;

						internal const int SE_SPECIAL_15 = 14;

						internal const int SE_SPECIAL_16 = 15;

						internal const int SE_SPECIAL_17 = 16;

						internal const int SE_ABILITY_02 = 1;

						internal const int SE_ABILITY_03 = 2;

						internal const int SE_ABILITY_04 = 3;

						internal const int SE_ABILITY_06 = 5;

						internal const int SE_ABILITY_07 = 6;

						internal const int SE_ABILITY_08 = 7;

						internal const int SE_ABILITY_09 = 8;

						internal const int SE_ABILITY_10 = 9;

						internal const int SE_ABILITY_11 = 10;

						internal const int SE_ABILITY_12 = 11;

						internal const int SE_ABILITY_13 = 12;

						internal const int SE_ABILITY_14 = 13;

						internal const int SE_ABILITY_15 = 14;

						internal const int SE_GEO_01 = 0;

						internal const int SE_GEO_02 = 1;

						internal const int SE_GEO_03 = 2;

						internal const int SE_GEO_04 = 3;

						internal const int SE_GEO_09 = 8;

						internal const int SE_GEO_10 = 9;

						internal const int SE_GEO_11 = 10;

						internal const int SE_GEO_12 = 11;

						internal const int SE_GEO_13 = 12;

						internal const int SE_GEO_14 = 13;

						internal const int SE_GEO_15 = 14;

						internal const int SE_GEO_16 = 15;

						internal const int SE_SONG_01 = 0;

						internal const int SE_SUPPORT_01 = 0;

						internal const int SE_ITEM_BATTLE_01 = 0;

						internal const int SE_ITEM_BATTLE_02 = 1;

						internal const int SE_ITEM_BATTLE_03 = 2;

						internal const int SE_ITEM_BATTLE_04 = 3;

						internal const int SE_ITEM_BATTLE_05 = 4;

						internal const int SE_ITEM_BATTLE_06 = 5;

						internal const int SE_ITEM_BATTLE_07 = 6;

						internal const int SE_ITEM_BATTLE_08 = 7;

						internal const int SE_ITEM_BATTLE_09 = 8;

						internal const int SE_ITEM_BATTLE_10 = 9;

						internal const int SE_WHITE1_01 = 0;

						internal const int SE_WHITE1_02 = 1;

						internal const int SE_WHITE1_03 = 2;

						internal const int SE_WHITE2_01 = 0;

						internal const int SE_WHITE2_02 = 1;

						internal const int SE_WHITE2_03 = 2;

						internal const int SE_WHITE3_01 = 0;

						internal const int SE_WHITE3_02 = 1;

						internal const int SE_WHITE3_03 = 2;

						internal const int SE_WHITE4_01 = 0;

						internal const int SE_WHITE4_02 = 1;

						internal const int SE_WHITE4_03 = 2;

						internal const int SE_WHITE5_01 = 0;

						internal const int SE_WHITE5_02 = 1;

						internal const int SE_WHITE5_03 = 2;

						internal const int SE_WHITE6_01 = 0;

						internal const int SE_WHITE6_02 = 1;

						internal const int SE_WHITE6_03 = 2;

						internal const int SE_WHITE7_01 = 0;

						internal const int SE_WHITE7_02 = 1;

						internal const int SE_WHITE7_03 = 2;

						internal const int SE_WHITE8_01 = 0;

						internal const int SE_WHITE8_02 = 1;

						internal const int SE_WHITE8_03 = 2;

						internal const int SE_BLACK1_01 = 0;

						internal const int SE_BLACK1_02 = 1;

						internal const int SE_BLACK1_03 = 2;

						internal const int SE_BLACK2_01 = 0;

						internal const int SE_BLACK2_02 = 1;

						internal const int SE_BLACK2_03 = 2;

						internal const int SE_BLACK3_01 = 0;

						internal const int SE_BLACK3_02 = 1;

						internal const int SE_BLACK3_03 = 2;

						internal const int SE_BLACK4_01 = 0;

						internal const int SE_BLACK4_02 = 1;

						internal const int SE_BLACK4_03 = 2;

						internal const int SE_BLACK5_01 = 0;

						internal const int SE_BLACK5_02 = 1;

						internal const int SE_BLACK5_03 = 2;

						internal const int SE_BLACK6_01 = 0;

						internal const int SE_BLACK6_02 = 1;

						internal const int SE_BLACK6_03 = 2;

						internal const int SE_BLACK7_01 = 0;

						internal const int SE_BLACK7_02 = 1;

						internal const int SE_BLACK7_03 = 2;

						internal const int SE_BLACK8_01 = 0;

						internal const int SE_BLACK8_02 = 1;

						internal const int SE_BLACK8_03 = 2;

						internal const int SE_SUMMON1_01 = 0;

						internal const int SE_SUMMON1_02 = 1;

						internal const int SE_SUMMON1_04 = 3;

						internal const int SE_SUMMON1_06 = 5;

						internal const int SE_SUMMON2_01 = 0;

						internal const int SE_SUMMON2_04 = 3;

						internal const int SE_SUMMON3_01 = 0;

						internal const int SE_SUMMON3_04 = 3;

						internal const int SE_SUMMON4_01 = 0;

						internal const int SE_SUMMON4_04 = 3;

						internal const int SE_SUMMON5_01 = 0;

						internal const int SE_SUMMON5_02 = 1;

						internal const int SE_SUMMON5_03 = 2;

						internal const int SE_SUMMON5_04 = 3;

						internal const int SE_SUMMON6_01 = 0;

						internal const int SE_SUMMON6_03 = 2;

						internal const int SE_SUMMON6_04 = 3;

						internal const int SE_SUMMON7_01 = 0;

						internal const int SE_SUMMON7_04 = 3;

						internal const int SE_SUMMON8_01 = 0;

						internal const int SE_SUMMON8_02 = 1;

						internal const int SE_SUMMON8_03 = 2;

						internal const int SE_SUMMON8_04 = 3;

						internal const string __FILE__ = "";

						internal const string __LINE__ = "";

						internal const int SEEK_SET = 0;

						internal const int SEEK_CUR = 1;

						internal const int SEEK_END = 2;

						internal const int sizeof_Node = 8;

						internal const int sizeof_SaveHeader = 36;

						internal const int sizeof_CSaveData = 13842;

						internal const int sizeof_SaveDataNormal = 13842;

						internal const int sizeof_SaveDataAddress = 13846;

						internal const int sizeof_SaveDataOption = 68;

						internal const int sizeof_ConsumptionParameter = 44;

						internal const int sizeof_WeaponParameter = 56;

						internal const int sizeof_ProtectionParameter = 60;

						internal const int sizeof_MagicParameter = 52;

						internal const int sizeof_ImportantParameter = 28;

						internal const int sizeof_PlayerExp = 396;

						internal const int sizeof_GrowUp = 792;

						internal const int sizeof_JobGrowUpType = 138;

						internal const int sizeof_PlayerNormalAttackParameter = 64;

						internal const int sizeof_GrowUpMp = 792;

						internal const int sizeof_JobEquipInfo = 92;

						internal const int sizeof_PlayerNormalMagicParameter = 32;

						internal const int sizeof_AbilityParameter = 8;

						internal const int sizeof_PlayerAbility = 12;

						internal const int sizeof_MonsterParameter = 100;

						internal const int sizeof_DropItemParameter = 18;

						internal const int sizeof_MonsterNormalAttackParameter = 28;

						internal const int sizeof_MonsterSpecialAttackParameter = 16;

						internal const int sizeof_MonsterOffsetParameter = 160;

						internal const int sizeof_MonsterSpecialAttackEffects = 56;

						internal const int sizeof_CMapJumpParameter = 44;

						internal const int sizeof_CMapLandFormParameter = 48;

						internal const int sizeof_CMapMonsterPartyParameter = 40;

						internal const int sizeof_CMapSoundParameter = 6;

						internal const int sizeof_CMapCameraParameter = 30;

						internal const int sizeof_CNPCWorldRandomMoveParameter = 8;

						internal const int sizeof_CNPCWorldAutoFollowParameter = 8;

						internal const int sizeof_CPlayerWorldMoveParameter = 44;

						internal const int sizeof_CPlayerWorldEnterParameter = 15;

						internal const int sizeof_CPlayerVehicleWorldMoveParameter = 10;

						internal const int sizeof_CPlayerVehicleWorldEnterParameter = 10;

						internal const int sizeof_CPlayerWorldSeEffectPlayParameter = 10;

						internal const int sizeof_CPlayerWorldSeEffectMapParameter = 10;

						internal const int sizeof_CShopParameter = 26;

						internal const int sizeof_GeographyData = 44;

						internal const int sizeof_CommandParameter = 36;

						internal const int sizeof_BattleNpcData = 40;

						internal const int sizeof_MonsterParty = 18;

						internal const int sizeof_SWCUserData = 44;

						internal const int sizeof_RTCDate = 24;

						internal const int sizeof_RTCTime = 12;

						internal const int sizeof_MNMementoData = 1188;

						internal const int sizeof_MNMail = 256;

						internal const int sizeof_HeapBlockInfo = 8;

						internal const int offsetof_MNMementoData_user = 0;

						internal const int offsetof_MNMementoData_friends = 0;

						internal const int offsetof_MNMementoData_last_send_date = 0;

						internal const int offsetof_MNMementoData_user_crc = 0;

						internal const int offsetof_MNMementoData_datetime_crc = 0;

						internal const int offsetof_MNMementoData_friend_crc = 0;

						public const OSLanguage OS_LANGUAGE_JAPANESE = OSLanguage.OS_LANGUAGE_JAPANESE;

						public const OSLanguage OS_LANGUAGE_ENGLISH = OSLanguage.OS_LANGUAGE_ENGLISH;

						public const OSLanguage OS_LANGUAGE_FRENCH = OSLanguage.OS_LANGUAGE_FRENCH;

						public const OSLanguage OS_LANGUAGE_GERMAN = OSLanguage.OS_LANGUAGE_GERMAN;

						public const OSLanguage OS_LANGUAGE_ITALIAN = OSLanguage.OS_LANGUAGE_ITALIAN;

						public const OSLanguage OS_LANGUAGE_SPANISH = OSLanguage.OS_LANGUAGE_SPANISH;

						public const OSLanguage OS_LANGUAGE_CHINESE_CN = OSLanguage.OS_LANGUAGE_CHINESE_CN;

						public const OSLanguage OS_LANGUAGE_CHINESE_TW = OSLanguage.OS_LANGUAGE_CHINESE_TW;

						public const OSLanguage OS_LANGUAGE_CODE_MAX = OSLanguage.OS_LANGUAGE_CODE_MAX;

						public const GXDispSelect GX_DISP_SELECT_SUB_MAIN = GXDispSelect.GX_DISP_SELECT_SUB_MAIN;

						public const GXDispSelect GX_DISP_SELECT_MAIN_SUB = GXDispSelect.GX_DISP_SELECT_MAIN_SUB;

						public const GXCull GX_CULL_ALL = GXCull.GX_CULL_ALL;

						public const GXCull GX_CULL_FRONT = GXCull.GX_CULL_FRONT;

						public const GXCull GX_CULL_BACK = GXCull.GX_CULL_BACK;

						public const GXCull GX_CULL_NONE = GXCull.GX_CULL_NONE;

						public const GXPlaneMask GX_PLANEMASK_NONE = GXPlaneMask.GX_PLANEMASK_NONE;

						public const GXPlaneMask GX_PLANEMASK_BG0 = GXPlaneMask.GX_PLANEMASK_BG0;

						public const GXPlaneMask GX_PLANEMASK_BG1 = GXPlaneMask.GX_PLANEMASK_BG1;

						public const GXPlaneMask GX_PLANEMASK_BG2 = GXPlaneMask.GX_PLANEMASK_BG2;

						public const GXPlaneMask GX_PLANEMASK_BG3 = GXPlaneMask.GX_PLANEMASK_BG3;

						public const GXPlaneMask GX_PLANEMASK_OBJ = GXPlaneMask.GX_PLANEMASK_OBJ;

						public const GXMtxMode GX_MTXMODE_PROJECTION = GXMtxMode.GX_MTXMODE_PROJECTION;

						public const GXMtxMode GX_MTXMODE_POSITION = GXMtxMode.GX_MTXMODE_POSITION;

						public const GXMtxMode GX_MTXMODE_POSITION_VECTOR = GXMtxMode.GX_MTXMODE_POSITION_VECTOR;

						public const GXMtxMode GX_MTXMODE_TEXTURE = GXMtxMode.GX_MTXMODE_TEXTURE;

						public const GXLightMask GX_LIGHTMASK_NONE = GXLightMask.GX_LIGHTMASK_NONE;

						public const GXLightMask GX_LIGHTMASK_0 = GXLightMask.GX_LIGHTMASK_0;

						public const GXLightMask GX_LIGHTMASK_1 = GXLightMask.GX_LIGHTMASK_1;

						public const GXLightMask GX_LIGHTMASK_01 = GXLightMask.GX_LIGHTMASK_01;

						public const GXLightMask GX_LIGHTMASK_2 = GXLightMask.GX_LIGHTMASK_2;

						public const GXLightMask GX_LIGHTMASK_02 = GXLightMask.GX_LIGHTMASK_02;

						public const GXLightMask GX_LIGHTMASK_12 = GXLightMask.GX_LIGHTMASK_12;

						public const GXLightMask GX_LIGHTMASK_012 = GXLightMask.GX_LIGHTMASK_012;

						public const GXLightMask GX_LIGHTMASK_3 = GXLightMask.GX_LIGHTMASK_3;

						public const GXLightMask GX_LIGHTMASK_03 = GXLightMask.GX_LIGHTMASK_03;

						public const GXLightMask GX_LIGHTMASK_13 = GXLightMask.GX_LIGHTMASK_13;

						public const GXLightMask GX_LIGHTMASK_013 = GXLightMask.GX_LIGHTMASK_013;

						public const GXLightMask GX_LIGHTMASK_23 = GXLightMask.GX_LIGHTMASK_23;

						public const GXLightMask GX_LIGHTMASK_023 = GXLightMask.GX_LIGHTMASK_023;

						public const GXLightMask GX_LIGHTMASK_123 = GXLightMask.GX_LIGHTMASK_123;

						public const GXLightMask GX_LIGHTMASK_0123 = GXLightMask.GX_LIGHTMASK_0123;

						public const GXWndPlaneMask GX_WND_PLANEMASK_NONE = GXWndPlaneMask.GX_WND_PLANEMASK_NONE;

						public const GXWndPlaneMask GX_WND_PLANEMASK_BG0 = GXWndPlaneMask.GX_WND_PLANEMASK_BG0;

						public const GXWndPlaneMask GX_WND_PLANEMASK_BG1 = GXWndPlaneMask.GX_WND_PLANEMASK_BG1;

						public const GXWndPlaneMask GX_WND_PLANEMASK_BG2 = GXWndPlaneMask.GX_WND_PLANEMASK_BG2;

						public const GXWndPlaneMask GX_WND_PLANEMASK_BG3 = GXWndPlaneMask.GX_WND_PLANEMASK_BG3;

						public const GXWndPlaneMask GX_WND_PLANEMASK_OBJ = GXWndPlaneMask.GX_WND_PLANEMASK_OBJ;

						public const GXBlendPlaneMask GX_BLEND_PLANEMASK_NONE = GXBlendPlaneMask.GX_BLEND_PLANEMASK_NONE;

						public const GXBlendPlaneMask GX_BLEND_PLANEMASK_BG0 = GXBlendPlaneMask.GX_BLEND_PLANEMASK_BG0;

						public const GXBlendPlaneMask GX_BLEND_PLANEMASK_BG1 = GXBlendPlaneMask.GX_BLEND_PLANEMASK_BG1;

						public const GXBlendPlaneMask GX_BLEND_PLANEMASK_BG2 = GXBlendPlaneMask.GX_BLEND_PLANEMASK_BG2;

						public const GXBlendPlaneMask GX_BLEND_PLANEMASK_BG3 = GXBlendPlaneMask.GX_BLEND_PLANEMASK_BG3;

						public const GXBlendPlaneMask GX_BLEND_PLANEMASK_OBJ = GXBlendPlaneMask.GX_BLEND_PLANEMASK_OBJ;

						public const GXBlendPlaneMask GX_BLEND_PLANEMASK_BD = GXBlendPlaneMask.GX_BLEND_PLANEMASK_BD;

						public const GXBGCharBase GX_BG_CHARBASE_0x00000 = GXBGCharBase.GX_BG_CHARBASE_0x00000;

						public const GXBGCharBase GX_BG_CHARBASE_0x04000 = GXBGCharBase.GX_BG_CHARBASE_0x04000;

						public const GXBGCharBase GX_BG_CHARBASE_0x08000 = GXBGCharBase.GX_BG_CHARBASE_0x08000;

						public const GXBGCharBase GX_BG_CHARBASE_0x0c000 = GXBGCharBase.GX_BG_CHARBASE_0x0c000;

						public const GXBGCharBase GX_BG_CHARBASE_0x10000 = GXBGCharBase.GX_BG_CHARBASE_0x10000;

						public const GXBGCharBase GX_BG_CHARBASE_0x14000 = GXBGCharBase.GX_BG_CHARBASE_0x14000;

						public const GXBGCharBase GX_BG_CHARBASE_0x18000 = GXBGCharBase.GX_BG_CHARBASE_0x18000;

						public const GXBGCharBase GX_BG_CHARBASE_0x1c000 = GXBGCharBase.GX_BG_CHARBASE_0x1c000;

						public const GXBGCharBase GX_BG_CHARBASE_0x20000 = GXBGCharBase.GX_BG_CHARBASE_0x20000;

						public const GXBGCharBase GX_BG_CHARBASE_0x24000 = GXBGCharBase.GX_BG_CHARBASE_0x24000;

						public const GXBGCharBase GX_BG_CHARBASE_0x28000 = GXBGCharBase.GX_BG_CHARBASE_0x28000;

						public const GXBGCharBase GX_BG_CHARBASE_0x2c000 = GXBGCharBase.GX_BG_CHARBASE_0x2c000;

						public const GXBGCharBase GX_BG_CHARBASE_0x30000 = GXBGCharBase.GX_BG_CHARBASE_0x30000;

						public const GXBGCharBase GX_BG_CHARBASE_0x34000 = GXBGCharBase.GX_BG_CHARBASE_0x34000;

						public const GXBGCharBase GX_BG_CHARBASE_0x38000 = GXBGCharBase.GX_BG_CHARBASE_0x38000;

						public const GXBGCharBase GX_BG_CHARBASE_0x3c000 = GXBGCharBase.GX_BG_CHARBASE_0x3c000;

						public const GXBGScrBase GX_BG_SCRBASE_0x0000 = GXBGScrBase.GX_BG_SCRBASE_0x0000;

						public const GXBGScrBase GX_BG_SCRBASE_0x0800 = GXBGScrBase.GX_BG_SCRBASE_0x0800;

						public const GXBGScrBase GX_BG_SCRBASE_0x1000 = GXBGScrBase.GX_BG_SCRBASE_0x1000;

						public const GXBGScrBase GX_BG_SCRBASE_0x1800 = GXBGScrBase.GX_BG_SCRBASE_0x1800;

						public const GXBGScrBase GX_BG_SCRBASE_0x2000 = GXBGScrBase.GX_BG_SCRBASE_0x2000;

						public const GXBGScrBase GX_BG_SCRBASE_0x2800 = GXBGScrBase.GX_BG_SCRBASE_0x2800;

						public const GXBGScrBase GX_BG_SCRBASE_0x3000 = GXBGScrBase.GX_BG_SCRBASE_0x3000;

						public const GXBGScrBase GX_BG_SCRBASE_0x3800 = GXBGScrBase.GX_BG_SCRBASE_0x3800;

						public const GXBGScrBase GX_BG_SCRBASE_0x4000 = GXBGScrBase.GX_BG_SCRBASE_0x4000;

						public const GXBGScrBase GX_BG_SCRBASE_0x4800 = GXBGScrBase.GX_BG_SCRBASE_0x4800;

						public const GXBGScrBase GX_BG_SCRBASE_0x5000 = GXBGScrBase.GX_BG_SCRBASE_0x5000;

						public const GXBGScrBase GX_BG_SCRBASE_0x5800 = GXBGScrBase.GX_BG_SCRBASE_0x5800;

						public const GXBGScrBase GX_BG_SCRBASE_0x6000 = GXBGScrBase.GX_BG_SCRBASE_0x6000;

						public const GXBGScrBase GX_BG_SCRBASE_0x6800 = GXBGScrBase.GX_BG_SCRBASE_0x6800;

						public const GXBGScrBase GX_BG_SCRBASE_0x7000 = GXBGScrBase.GX_BG_SCRBASE_0x7000;

						public const GXBGScrBase GX_BG_SCRBASE_0x7800 = GXBGScrBase.GX_BG_SCRBASE_0x7800;

						public const GXBGScrBase GX_BG_SCRBASE_0x8000 = GXBGScrBase.GX_BG_SCRBASE_0x8000;

						public const GXBGScrBase GX_BG_SCRBASE_0x8800 = GXBGScrBase.GX_BG_SCRBASE_0x8800;

						public const GXBGScrBase GX_BG_SCRBASE_0x9000 = GXBGScrBase.GX_BG_SCRBASE_0x9000;

						public const GXBGScrBase GX_BG_SCRBASE_0x9800 = GXBGScrBase.GX_BG_SCRBASE_0x9800;

						public const GXBGScrBase GX_BG_SCRBASE_0xa000 = GXBGScrBase.GX_BG_SCRBASE_0xa000;

						public const GXBGScrBase GX_BG_SCRBASE_0xa800 = GXBGScrBase.GX_BG_SCRBASE_0xa800;

						public const GXBGScrBase GX_BG_SCRBASE_0xb000 = GXBGScrBase.GX_BG_SCRBASE_0xb000;

						public const GXBGScrBase GX_BG_SCRBASE_0xb800 = GXBGScrBase.GX_BG_SCRBASE_0xb800;

						public const GXBGScrBase GX_BG_SCRBASE_0xc000 = GXBGScrBase.GX_BG_SCRBASE_0xc000;

						public const GXBGScrBase GX_BG_SCRBASE_0xc800 = GXBGScrBase.GX_BG_SCRBASE_0xc800;

						public const GXBGScrBase GX_BG_SCRBASE_0xd000 = GXBGScrBase.GX_BG_SCRBASE_0xd000;

						public const GXBGScrBase GX_BG_SCRBASE_0xd800 = GXBGScrBase.GX_BG_SCRBASE_0xd800;

						public const GXBGScrBase GX_BG_SCRBASE_0xe000 = GXBGScrBase.GX_BG_SCRBASE_0xe000;

						public const GXBGScrBase GX_BG_SCRBASE_0xe800 = GXBGScrBase.GX_BG_SCRBASE_0xe800;

						public const GXBGScrBase GX_BG_SCRBASE_0xf000 = GXBGScrBase.GX_BG_SCRBASE_0xf000;

						public const GXBGScrBase GX_BG_SCRBASE_0xf800 = GXBGScrBase.GX_BG_SCRBASE_0xf800;

						public const GXLightId GX_LIGHTID_0 = GXLightId.GX_LIGHTID_0;

						public const GXLightId GX_LIGHTID_1 = GXLightId.GX_LIGHTID_1;

						public const GXLightId GX_LIGHTID_2 = GXLightId.GX_LIGHTID_2;

						public const GXLightId GX_LIGHTID_3 = GXLightId.GX_LIGHTID_3;

						public const GXPolygonMode GX_POLYGONMODE_MODULATE = GXPolygonMode.GX_POLYGONMODE_MODULATE;

						public const GXPolygonMode GX_POLYGONMODE_DECAL = GXPolygonMode.GX_POLYGONMODE_DECAL;

						public const GXPolygonMode GX_POLYGONMODE_TOON = GXPolygonMode.GX_POLYGONMODE_TOON;

						public const GXPolygonMode GX_POLYGONMODE_SHADOW = GXPolygonMode.GX_POLYGONMODE_SHADOW;

						public const GXPolygonAttrMisc GX_POLYGON_ATTR_MISC_NONE = GXPolygonAttrMisc.GX_POLYGON_ATTR_MISC_NONE;

						public const GXPolygonAttrMisc GX_POLYGON_ATTR_MISC_XLU_DEPTH_UPDATE = GXPolygonAttrMisc.GX_POLYGON_ATTR_MISC_XLU_DEPTH_UPDATE;

						public const GXPolygonAttrMisc GX_POLYGON_ATTR_MISC_FAR_CLIPPING = GXPolygonAttrMisc.GX_POLYGON_ATTR_MISC_FAR_CLIPPING;

						public const GXPolygonAttrMisc GX_POLYGON_ATTR_MISC_DISP_1DOT = GXPolygonAttrMisc.GX_POLYGON_ATTR_MISC_DISP_1DOT;

						public const GXPolygonAttrMisc GX_POLYGON_ATTR_MISC_DEPTHTEST_DECAL = GXPolygonAttrMisc.GX_POLYGON_ATTR_MISC_DEPTHTEST_DECAL;

						public const GXPolygonAttrMisc GX_POLYGON_ATTR_MISC_FOG = GXPolygonAttrMisc.GX_POLYGON_ATTR_MISC_FOG;

						public const GXTexSizeS GX_TEXSIZE_S8 = GXTexSizeS.GX_TEXSIZE_S8;

						public const GXTexSizeS GX_TEXSIZE_S16 = GXTexSizeS.GX_TEXSIZE_S16;

						public const GXTexSizeS GX_TEXSIZE_S32 = GXTexSizeS.GX_TEXSIZE_S32;

						public const GXTexSizeS GX_TEXSIZE_S64 = GXTexSizeS.GX_TEXSIZE_S64;

						public const GXTexSizeS GX_TEXSIZE_S128 = GXTexSizeS.GX_TEXSIZE_S128;

						public const GXTexSizeS GX_TEXSIZE_S256 = GXTexSizeS.GX_TEXSIZE_S256;

						public const GXTexSizeS GX_TEXSIZE_S512 = GXTexSizeS.GX_TEXSIZE_S512;

						public const GXTexSizeS GX_TEXSIZE_S1024 = GXTexSizeS.GX_TEXSIZE_S1024;

						public const GXTexSizeT GX_TEXSIZE_T8 = GXTexSizeT.GX_TEXSIZE_T8;

						public const GXTexSizeT GX_TEXSIZE_T16 = GXTexSizeT.GX_TEXSIZE_T16;

						public const GXTexSizeT GX_TEXSIZE_T32 = GXTexSizeT.GX_TEXSIZE_T32;

						public const GXTexSizeT GX_TEXSIZE_T64 = GXTexSizeT.GX_TEXSIZE_T64;

						public const GXTexSizeT GX_TEXSIZE_T128 = GXTexSizeT.GX_TEXSIZE_T128;

						public const GXTexSizeT GX_TEXSIZE_T256 = GXTexSizeT.GX_TEXSIZE_T256;

						public const GXTexSizeT GX_TEXSIZE_T512 = GXTexSizeT.GX_TEXSIZE_T512;

						public const GXTexSizeT GX_TEXSIZE_T1024 = GXTexSizeT.GX_TEXSIZE_T1024;

						public const GXTexFmt GX_TEXFMT_NONE = GXTexFmt.GX_TEXFMT_NONE;

						public const GXTexFmt GX_TEXFMT_A3I5 = GXTexFmt.GX_TEXFMT_A3I5;

						public const GXTexFmt GX_TEXFMT_PLTT4 = GXTexFmt.GX_TEXFMT_PLTT4;

						public const GXTexFmt GX_TEXFMT_PLTT16 = GXTexFmt.GX_TEXFMT_PLTT16;

						public const GXTexFmt GX_TEXFMT_PLTT256 = GXTexFmt.GX_TEXFMT_PLTT256;

						public const GXTexFmt GX_TEXFMT_COMP4x4 = GXTexFmt.GX_TEXFMT_COMP4x4;

						public const GXTexFmt GX_TEXFMT_A5I3 = GXTexFmt.GX_TEXFMT_A5I3;

						public const GXTexFmt GX_TEXFMT_DIRECT = GXTexFmt.GX_TEXFMT_DIRECT;

						public const GXDispMode GX_DISPMODE_GRAPHICS = GXDispMode.GX_DISPMODE_GRAPHICS;

						public const GXDispMode GX_DISPMODE_VRAM_A = GXDispMode.GX_DISPMODE_VRAM_A;

						public const GXDispMode GX_DISPMODE_VRAM_B = GXDispMode.GX_DISPMODE_VRAM_B;

						public const GXDispMode GX_DISPMODE_VRAM_C = GXDispMode.GX_DISPMODE_VRAM_C;

						public const GXDispMode GX_DISPMODE_VRAM_D = GXDispMode.GX_DISPMODE_VRAM_D;

						public const GXDispMode GX_DISPMODE_MMEM = GXDispMode.GX_DISPMODE_MMEM;

						public const GXVRam GX_VRAM_A = GXVRam.GX_VRAM_A;

						public const GXVRam GX_VRAM_B = GXVRam.GX_VRAM_B;

						public const GXVRam GX_VRAM_C = GXVRam.GX_VRAM_C;

						public const GXVRam GX_VRAM_D = GXVRam.GX_VRAM_D;

						public const GXVRam GX_VRAM_E = GXVRam.GX_VRAM_E;

						public const GXVRam GX_VRAM_F = GXVRam.GX_VRAM_F;

						public const GXVRam GX_VRAM_G = GXVRam.GX_VRAM_G;

						public const GXVRam GX_VRAM_H = GXVRam.GX_VRAM_H;

						public const GXVRam GX_VRAM_I = GXVRam.GX_VRAM_I;

						public const GXVRam GX_VRAM_ALL = GXVRam.GX_VRAM_ALL;

						public const GXVRamOBJ GX_VRAM_OBJ_NONE = GXVRamOBJ.GX_VRAM_OBJ_NONE;

						public const GXVRamOBJ GX_VRAM_OBJ_16_F = GXVRamOBJ.GX_VRAM_OBJ_16_F;

						public const GXVRamOBJ GX_VRAM_OBJ_16_G = GXVRamOBJ.GX_VRAM_OBJ_16_G;

						public const GXVRamOBJ GX_VRAM_OBJ_32_FG = GXVRamOBJ.GX_VRAM_OBJ_32_FG;

						public const GXVRamOBJ GX_VRAM_OBJ_64_E = GXVRamOBJ.GX_VRAM_OBJ_64_E;

						public const GXVRamOBJ GX_VRAM_OBJ_80_EF = GXVRamOBJ.GX_VRAM_OBJ_80_EF;

						public const GXVRamOBJ GX_VRAM_OBJ_80_EG = GXVRamOBJ.GX_VRAM_OBJ_80_EG;

						public const GXVRamOBJ GX_VRAM_OBJ_96_EFG = GXVRamOBJ.GX_VRAM_OBJ_96_EFG;

						public const GXVRamOBJ GX_VRAM_OBJ_128_A = GXVRamOBJ.GX_VRAM_OBJ_128_A;

						public const GXVRamOBJ GX_VRAM_OBJ_128_B = GXVRamOBJ.GX_VRAM_OBJ_128_B;

						public const GXVRamOBJ GX_VRAM_OBJ_256_AB = GXVRamOBJ.GX_VRAM_OBJ_256_AB;

						public const GXVRamBG GX_VRAM_BG_NONE = GXVRamBG.GX_VRAM_BG_NONE;

						public const GXVRamBG GX_VRAM_BG_16_F = GXVRamBG.GX_VRAM_BG_16_F;

						public const GXVRamBG GX_VRAM_BG_16_G = GXVRamBG.GX_VRAM_BG_16_G;

						public const GXVRamBG GX_VRAM_BG_32_FG = GXVRamBG.GX_VRAM_BG_32_FG;

						public const GXVRamBG GX_VRAM_BG_64_E = GXVRamBG.GX_VRAM_BG_64_E;

						public const GXVRamBG GX_VRAM_BG_80_EF = GXVRamBG.GX_VRAM_BG_80_EF;

						public const GXVRamBG GX_VRAM_BG_96_EFG = GXVRamBG.GX_VRAM_BG_96_EFG;

						public const GXVRamBG GX_VRAM_BG_128_A = GXVRamBG.GX_VRAM_BG_128_A;

						public const GXVRamBG GX_VRAM_BG_128_B = GXVRamBG.GX_VRAM_BG_128_B;

						public const GXVRamBG GX_VRAM_BG_128_C = GXVRamBG.GX_VRAM_BG_128_C;

						public const GXVRamBG GX_VRAM_BG_128_D = GXVRamBG.GX_VRAM_BG_128_D;

						public const GXVRamBG GX_VRAM_BG_256_AB = GXVRamBG.GX_VRAM_BG_256_AB;

						public const GXVRamBG GX_VRAM_BG_256_BC = GXVRamBG.GX_VRAM_BG_256_BC;

						public const GXVRamBG GX_VRAM_BG_256_CD = GXVRamBG.GX_VRAM_BG_256_CD;

						public const GXVRamBG GX_VRAM_BG_384_ABC = GXVRamBG.GX_VRAM_BG_384_ABC;

						public const GXVRamBG GX_VRAM_BG_384_BCD = GXVRamBG.GX_VRAM_BG_384_BCD;

						public const GXVRamBG GX_VRAM_BG_512_ABCD = GXVRamBG.GX_VRAM_BG_512_ABCD;

						public const GXVRamBG GX_VRAM_BG_80_EG = GXVRamBG.GX_VRAM_BG_80_EG;

						public const GXVRamBG GX_VRAM_BG_256_AC = GXVRamBG.GX_VRAM_BG_256_AC;

						public const GXVRamBG GX_VRAM_BG_256_AD = GXVRamBG.GX_VRAM_BG_256_AD;

						public const GXVRamBG GX_VRAM_BG_256_BD = GXVRamBG.GX_VRAM_BG_256_BD;

						public const GXVRamBG GX_VRAM_BG_384_ABD = GXVRamBG.GX_VRAM_BG_384_ABD;

						public const GXVRamBG GX_VRAM_BG_384_ACD = GXVRamBG.GX_VRAM_BG_384_ACD;

						public const GXVRamBGExtPltt GX_VRAM_BGEXTPLTT_NONE = GXVRamBGExtPltt.GX_VRAM_BGEXTPLTT_NONE;

						public const GXVRamBGExtPltt GX_VRAM_BGEXTPLTT_01_F = GXVRamBGExtPltt.GX_VRAM_BGEXTPLTT_01_F;

						public const GXVRamBGExtPltt GX_VRAM_BGEXTPLTT_23_G = GXVRamBGExtPltt.GX_VRAM_BGEXTPLTT_23_G;

						public const GXVRamBGExtPltt GX_VRAM_BGEXTPLTT_0123_E = GXVRamBGExtPltt.GX_VRAM_BGEXTPLTT_0123_E;

						public const GXVRamBGExtPltt GX_VRAM_BGEXTPLTT_0123_FG = GXVRamBGExtPltt.GX_VRAM_BGEXTPLTT_0123_FG;

						public const GXVRamOBJExtPltt GX_VRAM_OBJEXTPLTT_NONE = GXVRamOBJExtPltt.GX_VRAM_OBJEXTPLTT_NONE;

						public const GXVRamOBJExtPltt GX_VRAM_OBJEXTPLTT_0_F = GXVRamOBJExtPltt.GX_VRAM_OBJEXTPLTT_0_F;

						public const GXVRamOBJExtPltt GX_VRAM_OBJEXTPLTT_0_G = GXVRamOBJExtPltt.GX_VRAM_OBJEXTPLTT_0_G;

						public const GXVRamTex GX_VRAM_TEX_NONE = GXVRamTex.GX_VRAM_TEX_NONE;

						public const GXVRamTex GX_VRAM_TEX_0_A = GXVRamTex.GX_VRAM_TEX_0_A;

						public const GXVRamTex GX_VRAM_TEX_0_B = GXVRamTex.GX_VRAM_TEX_0_B;

						public const GXVRamTex GX_VRAM_TEX_0_C = GXVRamTex.GX_VRAM_TEX_0_C;

						public const GXVRamTex GX_VRAM_TEX_0_D = GXVRamTex.GX_VRAM_TEX_0_D;

						public const GXVRamTex GX_VRAM_TEX_01_AB = GXVRamTex.GX_VRAM_TEX_01_AB;

						public const GXVRamTex GX_VRAM_TEX_01_BC = GXVRamTex.GX_VRAM_TEX_01_BC;

						public const GXVRamTex GX_VRAM_TEX_01_CD = GXVRamTex.GX_VRAM_TEX_01_CD;

						public const GXVRamTex GX_VRAM_TEX_012_ABC = GXVRamTex.GX_VRAM_TEX_012_ABC;

						public const GXVRamTex GX_VRAM_TEX_012_BCD = GXVRamTex.GX_VRAM_TEX_012_BCD;

						public const GXVRamTex GX_VRAM_TEX_0123_ABCD = GXVRamTex.GX_VRAM_TEX_0123_ABCD;

						public const GXVRamTex GX_VRAM_TEX_01_AC = GXVRamTex.GX_VRAM_TEX_01_AC;

						public const GXVRamTex GX_VRAM_TEX_01_AD = GXVRamTex.GX_VRAM_TEX_01_AD;

						public const GXVRamTex GX_VRAM_TEX_01_BD = GXVRamTex.GX_VRAM_TEX_01_BD;

						public const GXVRamTex GX_VRAM_TEX_012_ABD = GXVRamTex.GX_VRAM_TEX_012_ABD;

						public const GXVRamTex GX_VRAM_TEX_012_ACD = GXVRamTex.GX_VRAM_TEX_012_ACD;

						public const GXVRamTexPltt GX_VRAM_TEXPLTT_NONE = GXVRamTexPltt.GX_VRAM_TEXPLTT_NONE;

						public const GXVRamTexPltt GX_VRAM_TEXPLTT_0_F = GXVRamTexPltt.GX_VRAM_TEXPLTT_0_F;

						public const GXVRamTexPltt GX_VRAM_TEXPLTT_0_G = GXVRamTexPltt.GX_VRAM_TEXPLTT_0_G;

						public const GXVRamTexPltt GX_VRAM_TEXPLTT_01_FG = GXVRamTexPltt.GX_VRAM_TEXPLTT_01_FG;

						public const GXVRamTexPltt GX_VRAM_TEXPLTT_0123_E = GXVRamTexPltt.GX_VRAM_TEXPLTT_0123_E;

						public const GXVRamTexPltt GX_VRAM_TEXPLTT_01234_EF = GXVRamTexPltt.GX_VRAM_TEXPLTT_01234_EF;

						public const GXVRamTexPltt GX_VRAM_TEXPLTT_012345_EFG = GXVRamTexPltt.GX_VRAM_TEXPLTT_012345_EFG;

						public const GXVRamSubBG GX_VRAM_SUB_BG_NONE = GXVRamSubBG.GX_VRAM_SUB_BG_NONE;

						public const GXVRamSubBG GX_VRAM_SUB_BG_128_C = GXVRamSubBG.GX_VRAM_SUB_BG_128_C;

						public const GXVRamSubBG GX_VRAM_SUB_BG_32_H = GXVRamSubBG.GX_VRAM_SUB_BG_32_H;

						public const GXVRamSubBG GX_VRAM_SUB_BG_48_HI = GXVRamSubBG.GX_VRAM_SUB_BG_48_HI;

						public const GXVRamSubOBJ GX_VRAM_SUB_OBJ_NONE = GXVRamSubOBJ.GX_VRAM_SUB_OBJ_NONE;

						public const GXVRamSubOBJ GX_VRAM_SUB_OBJ_128_D = GXVRamSubOBJ.GX_VRAM_SUB_OBJ_128_D;

						public const GXVRamSubOBJ GX_VRAM_SUB_OBJ_16_I = GXVRamSubOBJ.GX_VRAM_SUB_OBJ_16_I;

						public const GXVRamSubBGExtPltt GX_VRAM_SUB_BGEXTPLTT_NONE = GXVRamSubBGExtPltt.GX_VRAM_SUB_BGEXTPLTT_NONE;

						public const GXVRamSubBGExtPltt GX_VRAM_SUB_BGEXTPLTT_0123_H = GXVRamSubBGExtPltt.GX_VRAM_SUB_BGEXTPLTT_0123_H;

						public const GXVRamSubOBJExtPltt GX_VRAM_SUB_OBJEXTPLTT_NONE = GXVRamSubOBJExtPltt.GX_VRAM_SUB_OBJEXTPLTT_NONE;

						public const GXVRamSubOBJExtPltt GX_VRAM_SUB_OBJEXTPLTT_0_I = GXVRamSubOBJExtPltt.GX_VRAM_SUB_OBJEXTPLTT_0_I;

						public const GXCaptureDest GX_CAPTURE_DEST_VRAM_A_0x00000 = GXCaptureDest.GX_CAPTURE_DEST_VRAM_A_0x00000;

						public const GXCaptureDest GX_CAPTURE_DEST_VRAM_B_0x00000 = GXCaptureDest.GX_CAPTURE_DEST_VRAM_B_0x00000;

						public const GXCaptureDest GX_CAPTURE_DEST_VRAM_C_0x00000 = GXCaptureDest.GX_CAPTURE_DEST_VRAM_C_0x00000;

						public const GXCaptureDest GX_CAPTURE_DEST_VRAM_D_0x00000 = GXCaptureDest.GX_CAPTURE_DEST_VRAM_D_0x00000;

						public const GXCaptureDest GX_CAPTURE_DEST_VRAM_A_0x08000 = GXCaptureDest.GX_CAPTURE_DEST_VRAM_A_0x08000;

						public const GXCaptureDest GX_CAPTURE_DEST_VRAM_B_0x08000 = GXCaptureDest.GX_CAPTURE_DEST_VRAM_B_0x08000;

						public const GXCaptureDest GX_CAPTURE_DEST_VRAM_C_0x08000 = GXCaptureDest.GX_CAPTURE_DEST_VRAM_C_0x08000;

						public const GXCaptureDest GX_CAPTURE_DEST_VRAM_D_0x08000 = GXCaptureDest.GX_CAPTURE_DEST_VRAM_D_0x08000;

						public const GXCaptureDest GX_CAPTURE_DEST_VRAM_A_0x10000 = GXCaptureDest.GX_CAPTURE_DEST_VRAM_A_0x10000;

						public const GXCaptureDest GX_CAPTURE_DEST_VRAM_B_0x10000 = GXCaptureDest.GX_CAPTURE_DEST_VRAM_B_0x10000;

						public const GXCaptureDest GX_CAPTURE_DEST_VRAM_C_0x10000 = GXCaptureDest.GX_CAPTURE_DEST_VRAM_C_0x10000;

						public const GXCaptureDest GX_CAPTURE_DEST_VRAM_D_0x10000 = GXCaptureDest.GX_CAPTURE_DEST_VRAM_D_0x10000;

						public const GXCaptureDest GX_CAPTURE_DEST_VRAM_A_0x18000 = GXCaptureDest.GX_CAPTURE_DEST_VRAM_A_0x18000;

						public const GXCaptureDest GX_CAPTURE_DEST_VRAM_B_0x18000 = GXCaptureDest.GX_CAPTURE_DEST_VRAM_B_0x18000;

						public const GXCaptureDest GX_CAPTURE_DEST_VRAM_C_0x18000 = GXCaptureDest.GX_CAPTURE_DEST_VRAM_C_0x18000;

						public const GXCaptureDest GX_CAPTURE_DEST_VRAM_D_0x18000 = GXCaptureDest.GX_CAPTURE_DEST_VRAM_D_0x18000;

						public const GXCaptureSize GX_CAPTURE_SIZE_128x128 = GXCaptureSize.GX_CAPTURE_SIZE_128x128;

						public const GXCaptureSize GX_CAPTURE_SIZE_256x64 = GXCaptureSize.GX_CAPTURE_SIZE_256x64;

						public const GXCaptureSize GX_CAPTURE_SIZE_256x128 = GXCaptureSize.GX_CAPTURE_SIZE_256x128;

						public const GXCaptureSize GX_CAPTURE_SIZE_256x192 = GXCaptureSize.GX_CAPTURE_SIZE_256x192;

						public const GXCaptureSrcA GX_CAPTURE_SRCA_2D3D = GXCaptureSrcA.GX_CAPTURE_SRCA_2D3D;

						public const GXCaptureSrcA GX_CAPTURE_SRCA_3D = GXCaptureSrcA.GX_CAPTURE_SRCA_3D;

						public const GXCaptureSrcB GX_CAPTURE_SRCB_VRAM_0x00000 = GXCaptureSrcB.GX_CAPTURE_SRCB_VRAM_0x00000;

						public const GXCaptureSrcB GX_CAPTURE_SRCB_MRAM = GXCaptureSrcB.GX_CAPTURE_SRCB_MRAM;

						public const GXCaptureSrcB GX_CAPTURE_SRCB_VRAM_0x08000 = GXCaptureSrcB.GX_CAPTURE_SRCB_VRAM_0x08000;

						public const GXCaptureSrcB GX_CAPTURE_SRCB_VRAM_0x10000 = GXCaptureSrcB.GX_CAPTURE_SRCB_VRAM_0x10000;

						public const GXCaptureSrcB GX_CAPTURE_SRCB_VRAM_0x18000 = GXCaptureSrcB.GX_CAPTURE_SRCB_VRAM_0x18000;

						public const GXCaptureMode GX_CAPTURE_MODE_A = GXCaptureMode.GX_CAPTURE_MODE_A;

						public const GXCaptureMode GX_CAPTURE_MODE_B = GXCaptureMode.GX_CAPTURE_MODE_B;

						public const GXCaptureMode GX_CAPTURE_MODE_AB = GXCaptureMode.GX_CAPTURE_MODE_AB;

						public const GXBGMode GX_BGMODE_0 = GXBGMode.GX_BGMODE_0;

						public const GXBGMode GX_BGMODE_1 = GXBGMode.GX_BGMODE_1;

						public const GXBGMode GX_BGMODE_2 = GXBGMode.GX_BGMODE_2;

						public const GXBGMode GX_BGMODE_3 = GXBGMode.GX_BGMODE_3;

						public const GXBGMode GX_BGMODE_4 = GXBGMode.GX_BGMODE_4;

						public const GXBGMode GX_BGMODE_5 = GXBGMode.GX_BGMODE_5;

						public const GXBGMode GX_BGMODE_6 = GXBGMode.GX_BGMODE_6;

						public const GXBegin GX_BEGIN_TRIANGLES = GXBegin.GX_BEGIN_TRIANGLES;

						public const GXBegin GX_BEGIN_QUADS = GXBegin.GX_BEGIN_QUADS;

						public const GXBegin GX_BEGIN_TRIANGLE_STRIP = GXBegin.GX_BEGIN_TRIANGLE_STRIP;

						public const GXBegin GX_BEGIN_QUAD_STRIP = GXBegin.GX_BEGIN_QUAD_STRIP;

						public const MIProcessor MI_PROCESSOR_ARM9 = MIProcessor.MI_PROCESSOR_ARM9;

						public const MIProcessor MI_PROCESSOR_ARM7 = MIProcessor.MI_PROCESSOR_ARM7;

						public const MICompressionType MI_COMPRESSION_LZ = MICompressionType.MI_COMPRESSION_LZ;

						public const MICompressionType MI_COMPRESSION_HUFFMAN = MICompressionType.MI_COMPRESSION_HUFFMAN;

						public const MICompressionType MI_COMPRESSION_RL = MICompressionType.MI_COMPRESSION_RL;

						public const MICompressionType MI_COMPRESSION_DIFF = MICompressionType.MI_COMPRESSION_DIFF;

						public const MICompressionType MI_COMPRESSION_TYPE_MASK = MICompressionType.MI_COMPRESSION_TYPE_MASK;

						public const MICompressionType MI_COMPRESSION_TYPE_EX_MASK = MICompressionType.MI_COMPRESSION_TYPE_EX_MASK;

						public const TPState TP_UP = TPState.TP_UP;

						public const TPState TP_DOWN = TPState.TP_DOWN;

						public const TPState TP_PINCH = TPState.TP_PINCH;

						public const FSSeekFileMode FS_SEEK_SET = FSSeekFileMode.FS_SEEK_SET;

						public const FSSeekFileMode FS_SEEK_CUR = FSSeekFileMode.FS_SEEK_CUR;

						public const FSSeekFileMode FS_SEEK_END = FSSeekFileMode.FS_SEEK_END;

						public const CARDResult CARD_RESULT_SUCCESS = CARDResult.CARD_RESULT_SUCCESS;

						public const CARDResult CARD_RESULT_FAILURE = CARDResult.CARD_RESULT_FAILURE;

						public const CARDResult CARD_RESULT_INVALID_PARAM = CARDResult.CARD_RESULT_INVALID_PARAM;

						public const CARDResult CARD_RESULT_UNSUPPORTED = CARDResult.CARD_RESULT_UNSUPPORTED;

						public const CARDResult CARD_RESULT_TIMEOUT = CARDResult.CARD_RESULT_TIMEOUT;

						public const CARDResult CARD_RESULT_ERROR = CARDResult.CARD_RESULT_ERROR;

						public const CARDResult CARD_RESULT_NO_RESPONSE = CARDResult.CARD_RESULT_NO_RESPONSE;

						public const CARDResult CARD_RESULT_CANCELED = CARDResult.CARD_RESULT_CANCELED;

						public const CARDBackupType CARD_BACKUP_TYPE_NOT_USE = CARDBackupType.CARD_BACKUP_TYPE_NOT_USE;

						public const CARDBackupType CARD_BACKUP_TYPE_EEPROM_4KBITS = CARDBackupType.CARD_BACKUP_TYPE_EEPROM_4KBITS;

						public const CARDBackupType CARD_BACKUP_TYPE_EEPROM_64KBITS = CARDBackupType.CARD_BACKUP_TYPE_EEPROM_64KBITS;

						public const CARDBackupType CARD_BACKUP_TYPE_EEPROM_512KBITS = CARDBackupType.CARD_BACKUP_TYPE_EEPROM_512KBITS;

						public const CARDBackupType CARD_BACKUP_TYPE_FLASH_2MBITS = CARDBackupType.CARD_BACKUP_TYPE_FLASH_2MBITS;

						public const CARDBackupType CARD_BACKUP_TYPE_FLASH_4MBITS = CARDBackupType.CARD_BACKUP_TYPE_FLASH_4MBITS;

						public const CARDBackupType CARD_BACKUP_TYPE_FRAM_256KBITS = CARDBackupType.CARD_BACKUP_TYPE_FRAM_256KBITS;

						public const NNS_G2D_VRAM_TYPE NNS_G2D_VRAM_TYPE_3DMAIN = NNS_G2D_VRAM_TYPE.NNS_G2D_VRAM_TYPE_3DMAIN;

						public const NNS_G2D_VRAM_TYPE NNS_G2D_VRAM_TYPE_2DMAIN = NNS_G2D_VRAM_TYPE.NNS_G2D_VRAM_TYPE_2DMAIN;

						public const NNS_G2D_VRAM_TYPE NNS_G2D_VRAM_TYPE_2DSUB = NNS_G2D_VRAM_TYPE.NNS_G2D_VRAM_TYPE_2DSUB;

						public const NNS_G2D_VRAM_TYPE NNS_G2D_VRAM_TYPE_MAX = NNS_G2D_VRAM_TYPE.NNS_G2D_VRAM_TYPE_MAX;

						public const NNSG2dSurfaceType NNS_G2D_SURFACETYPE_MAIN3D = NNSG2dSurfaceType.NNS_G2D_SURFACETYPE_MAIN3D;

						public const NNSG2dSurfaceType NNS_G2D_SURFACETYPE_MAIN2D = NNSG2dSurfaceType.NNS_G2D_SURFACETYPE_MAIN2D;

						public const NNSG2dSurfaceType NNS_G2D_SURFACETYPE_SUB2D = NNSG2dSurfaceType.NNS_G2D_SURFACETYPE_SUB2D;

						public const NNSG2dSurfaceType NNS_G2D_SURFACETYPE_MAX = NNSG2dSurfaceType.NNS_G2D_SURFACETYPE_MAX;

						public const NNSG2dAnimationPlayMode NNS_G2D_ANIMATIONPLAYMODE_INVALID = NNSG2dAnimationPlayMode.NNS_G2D_ANIMATIONPLAYMODE_INVALID;

						public const NNSG2dAnimationPlayMode NNS_G2D_ANIMATIONPLAYMODE_FORWARD = NNSG2dAnimationPlayMode.NNS_G2D_ANIMATIONPLAYMODE_FORWARD;

						public const NNSG2dAnimationPlayMode NNS_G2D_ANIMATIONPLAYMODE_FORWARD_LOOP = NNSG2dAnimationPlayMode.NNS_G2D_ANIMATIONPLAYMODE_FORWARD_LOOP;

						public const NNSG2dAnimationPlayMode NNS_G2D_ANIMATIONPLAYMODE_REVERSE = NNSG2dAnimationPlayMode.NNS_G2D_ANIMATIONPLAYMODE_REVERSE;

						public const NNSG2dAnimationPlayMode NNS_G2D_ANIMATIONPLAYMODE_REVERSE_LOOP = NNSG2dAnimationPlayMode.NNS_G2D_ANIMATIONPLAYMODE_REVERSE_LOOP;

						public const NNSG2dAnimationPlayMode NNS_G2D_ANIMATIONPLAYMODE_MAX = NNSG2dAnimationPlayMode.NNS_G2D_ANIMATIONPLAYMODE_MAX;

						public const NNSG2dVerticalOrigin NNS_G2D_VERTICALORIGIN_TOP = NNSG2dVerticalOrigin.NNS_G2D_VERTICALORIGIN_TOP;

						public const NNSG2dVerticalOrigin NNS_G2D_VERTICALORIGIN_MIDDLE = NNSG2dVerticalOrigin.NNS_G2D_VERTICALORIGIN_MIDDLE;

						public const NNSG2dVerticalOrigin NNS_G2D_VERTICALORIGIN_BOTTOM = NNSG2dVerticalOrigin.NNS_G2D_VERTICALORIGIN_BOTTOM;

						public const NNSG2dHorizontalOrigin NNS_G2D_HORIZONTALORIGIN_LEFT = NNSG2dHorizontalOrigin.NNS_G2D_HORIZONTALORIGIN_LEFT;

						public const NNSG2dHorizontalOrigin NNS_G2D_HORIZONTALORIGIN_CENTER = NNSG2dHorizontalOrigin.NNS_G2D_HORIZONTALORIGIN_CENTER;

						public const NNSG2dHorizontalOrigin NNS_G2D_HORIZONTALORIGIN_RIGHT = NNSG2dHorizontalOrigin.NNS_G2D_HORIZONTALORIGIN_RIGHT;

						public const NNSG2dVerticalAlign NNS_G2D_VERTICALALIGN_TOP = NNSG2dVerticalAlign.NNS_G2D_VERTICALALIGN_TOP;

						public const NNSG2dVerticalAlign NNS_G2D_VERTICALALIGN_MIDDLE = NNSG2dVerticalAlign.NNS_G2D_VERTICALALIGN_MIDDLE;

						public const NNSG2dVerticalAlign NNS_G2D_VERTICALALIGN_BOTTOM = NNSG2dVerticalAlign.NNS_G2D_VERTICALALIGN_BOTTOM;

						public const NNSG2dHorizontalAlign NNS_G2D_HORIZONTALALIGN_LEFT = NNSG2dHorizontalAlign.NNS_G2D_HORIZONTALALIGN_LEFT;

						public const NNSG2dHorizontalAlign NNS_G2D_HORIZONTALALIGN_CENTER = NNSG2dHorizontalAlign.NNS_G2D_HORIZONTALALIGN_CENTER;

						public const NNSG2dHorizontalAlign NNS_G2D_HORIZONTALALIGN_RIGHT = NNSG2dHorizontalAlign.NNS_G2D_HORIZONTALALIGN_RIGHT;

						public const NNSG2dTextEffect NNS_G2D_TEXT_DRAG = NNSG2dTextEffect.NNS_G2D_TEXT_DRAG;

						public const NNSG2dTextEffect NNS_G2D_TEXT_SCROLL = NNSG2dTextEffect.NNS_G2D_TEXT_SCROLL;

						public const NNSG2dTextEffect NNS_G2D_TEXT_SHADOW = NNSG2dTextEffect.NNS_G2D_TEXT_SHADOW;

						public const NNSG2dBGSelect NNS_G2D_BGSELECT_MAIN0 = NNSG2dBGSelect.NNS_G2D_BGSELECT_MAIN0;

						public const NNSG2dBGSelect NNS_G2D_BGSELECT_MAIN1 = NNSG2dBGSelect.NNS_G2D_BGSELECT_MAIN1;

						public const NNSG2dBGSelect NNS_G2D_BGSELECT_MAIN2 = NNSG2dBGSelect.NNS_G2D_BGSELECT_MAIN2;

						public const NNSG2dBGSelect NNS_G2D_BGSELECT_MAIN3 = NNSG2dBGSelect.NNS_G2D_BGSELECT_MAIN3;

						public const NNSG2dBGSelect NNS_G2D_BGSELECT_SUB0 = NNSG2dBGSelect.NNS_G2D_BGSELECT_SUB0;

						public const NNSG2dBGSelect NNS_G2D_BGSELECT_SUB1 = NNSG2dBGSelect.NNS_G2D_BGSELECT_SUB1;

						public const NNSG2dBGSelect NNS_G2D_BGSELECT_SUB2 = NNSG2dBGSelect.NNS_G2D_BGSELECT_SUB2;

						public const NNSG2dBGSelect NNS_G2D_BGSELECT_SUB3 = NNSG2dBGSelect.NNS_G2D_BGSELECT_SUB3;

						public const NNSG2dBGSelect NNS_G2D_BGSELECT_NUM = NNSG2dBGSelect.NNS_G2D_BGSELECT_NUM;

						public const NNSG2dTextBGWidth NNS_G2D_TEXT_BG_WIDTH_256 = NNSG2dTextBGWidth.NNS_G2D_TEXT_BG_WIDTH_256;

						public const NNSG2dTextBGWidth NNS_G2D_TEXT_BG_WIDTH_512 = NNSG2dTextBGWidth.NNS_G2D_TEXT_BG_WIDTH_512;

						public const NNSG2dCharaColorMode NNS_G2D_CHARA_COLORMODE_16 = NNSG2dCharaColorMode.NNS_G2D_CHARA_COLORMODE_16;

						public const NNSG2dCharaColorMode NNS_G2D_CHARA_COLORMODE_256 = NNSG2dCharaColorMode.NNS_G2D_CHARA_COLORMODE_256;

						public const NNSG2dRendererOverwriteParam NNS_G2D_RND_OVERWRITE_NONE = NNSG2dRendererOverwriteParam.NNS_G2D_RND_OVERWRITE_NONE;

						public const NNSG2dRendererOverwriteParam NNS_G2D_RND_OVERWRITE_PRIORITY = NNSG2dRendererOverwriteParam.NNS_G2D_RND_OVERWRITE_PRIORITY;

						public const NNSG2dRendererOverwriteParam NNS_G2D_RND_OVERWRITE_PLTTNO = NNSG2dRendererOverwriteParam.NNS_G2D_RND_OVERWRITE_PLTTNO;

						public const NNSG2dRendererOverwriteParam NNS_G2D_RND_OVERWRITE_MOSAIC = NNSG2dRendererOverwriteParam.NNS_G2D_RND_OVERWRITE_MOSAIC;

						public const NNSG2dRendererOverwriteParam NNS_G2D_RND_OVERWRITE_OBJMODE = NNSG2dRendererOverwriteParam.NNS_G2D_RND_OVERWRITE_OBJMODE;

						public const NNSG2dRendererOverwriteParam NNS_G2D_RND_OVERWRITE_PLTTNO_OFFS = NNSG2dRendererOverwriteParam.NNS_G2D_RND_OVERWRITE_PLTTNO_OFFS;

						public const NNSG2dRendererOverwriteParam NNS_G2D_RND_OVERWRITE_MAX = NNSG2dRendererOverwriteParam.NNS_G2D_RND_OVERWRITE_MAX;

						public const NNSG2dRendererOptimizeHint NNS_G2D_RDR_OPZHINT_NONE = NNSG2dRendererOptimizeHint.NNS_G2D_RDR_OPZHINT_NONE;

						public const NNSG2dRendererOptimizeHint NNS_G2D_RDR_OPZHINT_NOT_SR = NNSG2dRendererOptimizeHint.NNS_G2D_RDR_OPZHINT_NOT_SR;

						public const NNSG2dRendererOptimizeHint NNS_G2D_RDR_OPZHINT_LOCK_PARAMS = NNSG2dRendererOptimizeHint.NNS_G2D_RDR_OPZHINT_LOCK_PARAMS;

						public const NNSG2dOamType NNS_G2D_OAMTYPE_MAIN = NNSG2dOamType.NNS_G2D_OAMTYPE_MAIN;

						public const NNSG2dOamType NNS_G2D_OAMTYPE_SUB = NNSG2dOamType.NNS_G2D_OAMTYPE_SUB;

						public const NNSG2dOamType NNS_G2D_OAMTYPE_SOFTWAREEMULATION = NNSG2dOamType.NNS_G2D_OAMTYPE_SOFTWAREEMULATION;

						public const NNSG2dOamType NNS_G2D_OAMTYPE_INVALID = NNSG2dOamType.NNS_G2D_OAMTYPE_INVALID;

						public const NNSG2dOamType NNS_G2D_OAMTYPE_MAX = NNSG2dOamType.NNS_G2D_OAMTYPE_MAX;

						public const NNSG3dSbcCallBackTiming NNS_G3D_SBC_CALLBACK_TIMING_NONE = NNSG3dSbcCallBackTiming.NNS_G3D_SBC_CALLBACK_TIMING_NONE;

						public const NNSG3dSbcCallBackTiming NNS_G3D_SBC_CALLBACK_TIMING_A = NNSG3dSbcCallBackTiming.NNS_G3D_SBC_CALLBACK_TIMING_A;

						public const NNSG3dSbcCallBackTiming NNS_G3D_SBC_CALLBACK_TIMING_B = NNSG3dSbcCallBackTiming.NNS_G3D_SBC_CALLBACK_TIMING_B;

						public const NNSG3dSbcCallBackTiming NNS_G3D_SBC_CALLBACK_TIMING_C = NNSG3dSbcCallBackTiming.NNS_G3D_SBC_CALLBACK_TIMING_C;

						public const NNSG3dRenderObjFlag NNS_G3D_RENDEROBJ_FLAG_RECORD = NNSG3dRenderObjFlag.NNS_G3D_RENDEROBJ_FLAG_RECORD;

						public const NNSG3dRenderObjFlag NNS_G3D_RENDEROBJ_FLAG_NOGECMD = NNSG3dRenderObjFlag.NNS_G3D_RENDEROBJ_FLAG_NOGECMD;

						public const NNSG3dRenderObjFlag NNS_G3D_RENDEROBJ_FLAG_SKIP_SBC_DRAW = NNSG3dRenderObjFlag.NNS_G3D_RENDEROBJ_FLAG_SKIP_SBC_DRAW;

						public const NNSG3dRenderObjFlag NNS_G3D_RENDEROBJ_FLAG_SKIP_SBC_MTXCALC = NNSG3dRenderObjFlag.NNS_G3D_RENDEROBJ_FLAG_SKIP_SBC_MTXCALC;

						public const NNSG3dRenderObjFlag NNS_G3D_RENDEROBJ_FLAG_HINT_OBSOLETE = NNSG3dRenderObjFlag.NNS_G3D_RENDEROBJ_FLAG_HINT_OBSOLETE;

						public const NNSG3dSRTFlag NNS_G3D_SRTFLAG_TRANS_ZERO = NNSG3dSRTFlag.NNS_G3D_SRTFLAG_TRANS_ZERO;

						public const NNSG3dSRTFlag NNS_G3D_SRTFLAG_ROT_ZERO = NNSG3dSRTFlag.NNS_G3D_SRTFLAG_ROT_ZERO;

						public const NNSG3dSRTFlag NNS_G3D_SRTFLAG_SCALE_ONE = NNSG3dSRTFlag.NNS_G3D_SRTFLAG_SCALE_ONE;

						public const NNSG3dSRTFlag NNS_G3D_SRTFLAG_PIVOT_EXIST = NNSG3dSRTFlag.NNS_G3D_SRTFLAG_PIVOT_EXIST;

						public const NNSG3dSRTFlag NNS_G3D_SRTFLAG_IDXPIVOT_MASK = NNSG3dSRTFlag.NNS_G3D_SRTFLAG_IDXPIVOT_MASK;

						public const NNSG3dSRTFlag NNS_G3D_SRTFLAG_PIVOT_MINUS = NNSG3dSRTFlag.NNS_G3D_SRTFLAG_PIVOT_MINUS;

						public const NNSG3dSRTFlag NNS_G3D_SRTFLAG_SIGN_REVC = NNSG3dSRTFlag.NNS_G3D_SRTFLAG_SIGN_REVC;

						public const NNSG3dSRTFlag NNS_G3D_SRTFLAG_SIGN_REVD = NNSG3dSRTFlag.NNS_G3D_SRTFLAG_SIGN_REVD;

						public const NNSG3dSRTFlag NNS_G3D_SRTFLAG_IDXMTXSTACK_MASK = NNSG3dSRTFlag.NNS_G3D_SRTFLAG_IDXMTXSTACK_MASK;

						public const NNSG3dSRTFlag NNS_G3D_SRTFLAG_IDENTITY = NNSG3dSRTFlag.NNS_G3D_SRTFLAG_IDENTITY;

						public const NNSG3dSRTFlag NNS_G3D_SRTFLAG_IDXPIVOT_SHIFT = NNSG3dSRTFlag.NNS_G3D_SRTFLAG_SCALE_ONE;

						public const NNSG3dSRTFlag NNS_G3D_SRTFLAG_IDXMTXSTACK_SHIFT = NNSG3dSRTFlag.NNS_G3D_SRTFLAG_IDXMTXSTACK_SHIFT;

						public const NNSG3dJntAnmSRTTag NNS_G3D_JNTANM_SRTINFO_IDENTITY = NNSG3dJntAnmSRTTag.NNS_G3D_JNTANM_SRTINFO_IDENTITY;

						public const NNSG3dJntAnmSRTTag NNS_G3D_JNTANM_SRTINFO_IDENTITY_T = NNSG3dJntAnmSRTTag.NNS_G3D_JNTANM_SRTINFO_IDENTITY_T;

						public const NNSG3dJntAnmSRTTag NNS_G3D_JNTANM_SRTINFO_BASE_T = NNSG3dJntAnmSRTTag.NNS_G3D_JNTANM_SRTINFO_BASE_T;

						public const NNSG3dJntAnmSRTTag NNS_G3D_JNTANM_SRTINFO_CONST_TX = NNSG3dJntAnmSRTTag.NNS_G3D_JNTANM_SRTINFO_CONST_TX;

						public const NNSG3dJntAnmSRTTag NNS_G3D_JNTANM_SRTINFO_CONST_TY = NNSG3dJntAnmSRTTag.NNS_G3D_JNTANM_SRTINFO_CONST_TY;

						public const NNSG3dJntAnmSRTTag NNS_G3D_JNTANM_SRTINFO_CONST_TZ = NNSG3dJntAnmSRTTag.NNS_G3D_JNTANM_SRTINFO_CONST_TZ;

						public const NNSG3dJntAnmSRTTag NNS_G3D_JNTANM_SRTINFO_IDENTITY_R = NNSG3dJntAnmSRTTag.NNS_G3D_JNTANM_SRTINFO_IDENTITY_R;

						public const NNSG3dJntAnmSRTTag NNS_G3D_JNTANM_SRTINFO_BASE_R = NNSG3dJntAnmSRTTag.NNS_G3D_JNTANM_SRTINFO_BASE_R;

						public const NNSG3dJntAnmSRTTag NNS_G3D_JNTANM_SRTINFO_CONST_R = NNSG3dJntAnmSRTTag.NNS_G3D_JNTANM_SRTINFO_CONST_R;

						public const NNSG3dJntAnmSRTTag NNS_G3D_JNTANM_SRTINFO_IDENTITY_S = NNSG3dJntAnmSRTTag.NNS_G3D_JNTANM_SRTINFO_IDENTITY_S;

						public const NNSG3dJntAnmSRTTag NNS_G3D_JNTANM_SRTINFO_BASE_S = NNSG3dJntAnmSRTTag.NNS_G3D_JNTANM_SRTINFO_BASE_S;

						public const NNSG3dJntAnmSRTTag NNS_G3D_JNTANM_SRTINFO_CONST_SX = NNSG3dJntAnmSRTTag.NNS_G3D_JNTANM_SRTINFO_CONST_SX;

						public const NNSG3dJntAnmSRTTag NNS_G3D_JNTANM_SRTINFO_CONST_SY = NNSG3dJntAnmSRTTag.NNS_G3D_JNTANM_SRTINFO_CONST_SY;

						public const NNSG3dJntAnmSRTTag NNS_G3D_JNTANM_SRTINFO_CONST_SZ = NNSG3dJntAnmSRTTag.NNS_G3D_JNTANM_SRTINFO_CONST_SZ;

						public const NNSG3dJntAnmSRTTag NNS_G3D_JNTANM_SRTINFO_NODE_MASK = NNSG3dJntAnmSRTTag.NNS_G3D_JNTANM_SRTINFO_NODE_MASK;

						public const NNSG3dJntAnmSRTTag NNS_G3D_JNTANM_SRTINFO_NODE_SHIFT = NNSG3dJntAnmSRTTag.NNS_G3D_JNTANM_SRTINFO_NODE_SHIFT;

						public const LANGUAGE_CODE LANGUAGE_CODE_ENGLISH = LANGUAGE_CODE.LANGUAGE_CODE_ENGLISH;

						public const LANGUAGE_CODE LANGUAGE_CODE_GERMAN = LANGUAGE_CODE.LANGUAGE_CODE_GERMAN;

						public const LANGUAGE_CODE LANGUAGE_CODE_SPANISH = LANGUAGE_CODE.LANGUAGE_CODE_SPANISH;

						public const LANGUAGE_CODE LANGUAGE_CODE_FRENCH = LANGUAGE_CODE.LANGUAGE_CODE_FRENCH;

						public const LANGUAGE_CODE LANGUAGE_CODE_ITALIAN = LANGUAGE_CODE.LANGUAGE_CODE_ITALIAN;

						public const LANGUAGE_CODE LANGUAGE_CODE_DEFAULT = LANGUAGE_CODE.LANGUAGE_CODE_ENGLISH;

						public const int G3DDEMO_COLOR_BLACK = 0;

						public const int G3DDEMO_COLOR_BLUE = 1;

						public const int G3DDEMO_COLOR_RED = 2;

						public const int G3DDEMO_COLOR_MAGENTA = 3;

						public const int G3DDEMO_COLOR_GREEN = 4;

						public const int G3DDEMO_COLOR_CYAN = 5;

						public const int G3DDEMO_COLOR_YELLOW = 6;

						public const int G3DDEMO_COLOR_WHITE = 7;

						public const int DEBUG_TEXT_BLACK = 0;

						public const int DEBUG_TEXT_BLUE = 1;

						public const int DEBUG_TEXT_RED = 2;

						public const int DEBUG_TEXT_MAGENTA = 3;

						public const int DEBUG_TEXT_GREEN = 4;

						public const int DEBUG_TEXT_CYAN = 5;

						public const int DEBUG_TEXT_YELLOW = 6;

						public const int DEBUG_TEXT_WHITE = 7;

						public const BATTLEPERFORM_TYPE BPT_SLANT_VANISH = BATTLEPERFORM_TYPE.BPT_SLANT_VANISH;

						public const BATTLEPERFORM_TYPE BPT_DIVIDE = BATTLEPERFORM_TYPE.BPT_DIVIDE;

						public const BATTLEPERFORM_TYPE BPT_IRON_CHOPPER = BATTLEPERFORM_TYPE.BPT_IRON_CHOPPER;

						public const BATTLEPERFORM_TYPE BPT_TRANSLUCENCE = BATTLEPERFORM_TYPE.BPT_TRANSLUCENCE;

						public const BATTLEPERFORM_TYPE NUMBER_OF_BP = BATTLEPERFORM_TYPE.NUMBER_OF_BP;

						public const uint NUM_GROUP = 3u;

						public const uint NUM_FLAG = 1000u;

						private const int LIMIT_OF_SCROLLBAR = 4;

						public const GAMEPART GAMEPART_DEBUG_MENU = GAMEPART.GAMEPART_DEBUG_MENU;

						public const GAMEPART GAMEPART_CAMPANY_LOGO = GAMEPART.GAMEPART_CAMPANY_LOGO;

						public const GAMEPART GAMEPART_TITLE = GAMEPART.GAMEPART_TITLE;

						public const GAMEPART GAMEPART_BATTLE = GAMEPART.GAMEPART_BATTLE;

						public const GAMEPART GAMEPART_WORLD = GAMEPART.GAMEPART_WORLD;

						public const GAMEPART GAMEPART_SPECIAL = GAMEPART.GAMEPART_SPECIAL;

						public const GAMEPART GAMEPART_LOAD = GAMEPART.GAMEPART_LOAD;

						public const GAMEPART GAMEPART_SUSPEND_LOAD = GAMEPART.GAMEPART_SUSPEND_LOAD;

						public const GAMEPART GAMEPART_MOG_NET = GAMEPART.GAMEPART_MOG_NET;

						public const GAMEPART GAMEPART_MOVIE = GAMEPART.GAMEPART_MOVIE;

						public const GAMEPART GAMEPART_WIFI_UTIL = GAMEPART.GAMEPART_WIFI_UTIL;

						public const GAMEPART GAMEPART_LINK = GAMEPART.GAMEPART_LINK;

						public const GAMEPART GAMEPART_MAX = GAMEPART.GAMEPART_MAX;

						public const MapMarkerDirection MARK_DIR_W = MapMarkerDirection.MARK_DIR_W;

						public const MapMarkerDirection MARK_DIR_E = MapMarkerDirection.MARK_DIR_E;

						public const MapMarkerDirection MARK_DIR_NW = MapMarkerDirection.MARK_DIR_NW;

						public const MapMarkerDirection MARK_DIR_NE = MapMarkerDirection.MARK_DIR_NE;

						public const MapMarkerDirection MARK_DIR_SW = MapMarkerDirection.MARK_DIR_SW;

						public const MapMarkerDirection MARK_DIR_SE = MapMarkerDirection.MARK_DIR_SE;

						public const MapMarkerDirection MARK_DIR_N = MapMarkerDirection.MARK_DIR_N;

						public const MapMarkerDirection MARK_DIR_S = MapMarkerDirection.MARK_DIR_S;

						public const WBE_SCREENSELECT WBE_SCREENSELECT_MAIN = WBE_SCREENSELECT.WBE_SCREENSELECT_MAIN;

						public const WBE_SCREENSELECT WBE_SCREENSELECT_SUB = WBE_SCREENSELECT.WBE_SCREENSELECT_SUB;

						public const WBE_SCREENSELECT WBE_SCREENSELECT_NUM = WBE_SCREENSELECT.WBE_SCREENSELECT_NUM;

						public const WBE_EFFECTTYPE WBE_EFFECTTYPE_ALPHA = WBE_EFFECTTYPE.WBE_EFFECTTYPE_ALPHA;

						public const WBE_EFFECTTYPE WBE_EFFECTTYPE_BRIGHTNESS = WBE_EFFECTTYPE.WBE_EFFECTTYPE_BRIGHTNESS;

						public const WBE_EFFECTTYPE WBE_EFFECTTYPE_NUM = WBE_EFFECTTYPE.WBE_EFFECTTYPE_NUM;

						public const int CBN_POSSESSION_ITEM_SELECTED = 0;

						public const int CBN_STOCK_ITEM_SELECTED = 1;

						public const int NUMBER_OF_CBN = 2;

						public const int CB_PROLOGUE_FADEOUT = 0;

						public const int CB_PROLOGUE_FADEIN = 1;

						public const int CB_MAIN = 2;

						public const int CB_POSSESSION_SELECT = 3;

						public const int CB_STOCK_SELECT = 4;

						public const int CB_EPILOGUE_FADEOUT = 5;

						public const int CB_EPILOGUE_FADEIN = 6;

						public const int NUMBER_OF_CB = 7;

						public const int CBBM_INITIALIZE = 0;

						public const int CBBM_FADEIN = 1;

						public const int CBBM_FADEOUT = 2;

						public const int CBBM_MAIN = 3;

						public const int NUMBER_OF_NEBM = 4;

						public const int NE_PROLOGUE_FADEOUT = 0;

						public const int NE_PROLOGUE_FADEIN = 1;

						public const int NE_MAIN = 2;

						public const int NE_EPILOGUE_FADEOUT = 3;

						public const int NE_EPILOGUE_FADEIN = 4;

						public const int NUMBER_OF_NE = 5;

						public const int MN_PROLOGUE_MESSAGE = 0;

						public const int MN_PROLOGUE_MESSAGE_WAIT = 1;

						public const int MN_PROLOGUE_FADEOUT = 2;

						public const int MN_PROLOGUE_FADEIN = 3;

						public const int MN_MAIN = 4;

						public const int MN_EPILOGUE_FADEOUT = 5;

						public const int MN_EPILOGUE_FADEIN = 6;

						public const int MN_EPILOGUE_MESSAGE = 7;

						public const int MN_EPILOGUE_MESSAGE_WAIT = 8;

						internal const int GL_FALSE = 0;

						internal const int GL_TRUE = 1;

						internal const int GL_ZERO = 0;

						internal const int GL_ONE = 1;

						internal const int GL_DEPTH_BUFFER_BIT = 256;

						internal const int GL_STENCIL_BUFFER_BIT = 1024;

						internal const int GL_COLOR_BUFFER_BIT = 16384;

						internal const int GL_LINE_LOOP = 2;

						internal const int GL_TRIANGLES = 4;

						internal const int GL_TRIANGLE_STRIP = 5;

						internal const int GL_TRIANGLE_FAN = 6;

						internal const int GL_LESS = 513;

						internal const int GL_EQUAL = 514;

						internal const int GL_LEQUAL = 515;

						internal const int GL_GREATER = 516;

						internal const int GL_GEQUAL = 518;

						internal const int GL_ALWAYS = 519;

						internal const int GL_SRC_ALPHA = 770;

						internal const int GL_ONE_MINUS_SRC_ALPHA = 771;

						internal const int GL_FRONT = 1028;

						internal const int GL_BACK = 1029;

						internal const int GL_FRONT_AND_BACK = 1032;

						internal const int GL_CULL_FACE = 2884;

						internal const int GL_FOG = 2912;

						internal const int GL_DEPTH_TEST = 2929;

						internal const int GL_STENCIL_TEST = 2960;

						internal const int GL_ALPHA_TEST = 3008;

						internal const int GL_BLEND = 3042;

						internal const int GL_TEXTURE_2D = 3553;

						internal const int GL_AMBIENT = 4608;

						internal const int GL_DIFFUSE = 4609;

						internal const int GL_POSITION = 4611;

						internal const int GL_UNSIGNED_BYTE = 5121;

						internal const int GL_FLOAT = 5126;

						internal const int GL_EMISSION = 5632;

						internal const int GL_MODELVIEW = 5888;

						internal const int GL_PROJECTION = 5889;

						internal const int GL_TEXTURE = 5890;

						internal const int GL_RGBA = 6408;

						internal const int GL_NEAREST = 9728;

						internal const int GL_LINEAR = 9729;

						internal const int GL_TEXTURE_MAG_FILTER = 10240;

						internal const int GL_TEXTURE_MIN_FILTER = 10241;

						internal const int GL_TEXTURE_WRAP_S = 10242;

						internal const int GL_TEXTURE_WRAP_T = 10243;

						internal const int GL_REPEAT = 10497;

						internal const int GL_LIGHT0 = 16384;

						internal const int GL_UNSIGNED_SHORT_5_5_5_1 = 32820;

						internal const int GL_VERTEX_ARRAY = 32884;

						internal const int GL_COLOR_ARRAY = 32886;

						internal const int GL_TEXTURE_COORD_ARRAY = 32888;

						internal const int GL_CLAMP_TO_EDGE = 33071;

						internal const int DISPLAY_WIDTH = 800;

						internal const int DISPLAY_HEIGHT = 480;

						internal static Random m_Random = new Random();

						internal static Type[] m_DummyType = new Type[0];

						internal static object[] m_DummyObject = new object[0];

						internal static byte[] m_abyReadCache = new byte[16];

						internal static string m_strStrtok = null;

						internal static int m_iStrtok = 0;

						internal static int texCount;

						internal static int memCount;

						internal static int polyCount;

						internal static int fontScale = 2;

						internal static int cont;

						internal static int contF;

						internal static int trg;

						internal static int hideCount;

						internal static int boost;

						internal static int hide;

						internal static float[] touchX = new float[2];

						internal static float[] touchY = new float[2];

						internal static int touchCount;

						internal static int touchPeak;

						internal static int LCD_WIDTH = 480;

						internal static int LCD_HEIGHT = 320;

						internal static int PERSPECTIVE_WIDTH = 533;

						internal static int PERSPECTIVE_HEIGHT = 320;

						internal static int tapCount;

						internal static float tapX;

						internal static float tapY;

						internal static JNIEnv env;

						internal static Type activity;

						internal static ulong prevFrame;

						internal static int fpsCount;

						internal static int fps;

						private static bool isAutoSave_;

						private static Font[] font = new Font[32];

						internal static int flipScreen;

						internal static int backButton;

						public static int skipFrame;

						public static int enable3D;

						public static int priority3D;

						public static int[] textBrightness = new int[2];

						public static int[] textAlpha = new int[2];

						public static int[] visiblePlane = new int[2];

						public static int[] bgPriority = new int[8];

						public static int[,] bgOffset = new int[8, 2];

						public static int[] screenOffset = new int[2];

						public static ushort toonTable;

						public static int masterBrightness;

						public static int boxTestResult;

						private static OSIrqFunction vBlankFunction;

						private static VecFx32 nitro_reuse_v0 = new VecFx32();

						private static ds.Vector3<int> nitro_reuse_v1 = new ds.Vector3<int>();

						private static MtxFx33 nitro_reuse_mtx33Src = new MtxFx33();

						private static MtxFx33 nitro_reuse_mtx33Dst = new MtxFx33();

						public static ushort[] screen = new ushort[1024];

						public static GXMtxMode currentMode;

						public static MtxFx43 currentMtx = new MtxFx43();

						public static MtxFx43 currentMtx2 = new MtxFx43();

						public static MtxFx44 projectionMtx = new MtxFx44();

						public static float texScaleU;

						public static float texScaleV;

						public static MtxFx43[] stack = new MtxFx43[32]
						{
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43()
						};

						public static int pStack = 0;

						public static _g3 g3 = new _g3();

						private static VecFx32 nitro_reuse_pos = new VecFx32();

						private static float[] nitro_reuse_f = new float[16];

						private static MtxFx43 nitro_reuse_m43 = new MtxFx43();

						private static MtxFx44 nitro_reuse_m44 = new MtxFx44();

						private static VecFx32 nitro_reuse_x = new VecFx32();

						private static VecFx32 nitro_reuse_y = new VecFx32();

						private static VecFx32 nitro_reuse_z = new VecFx32();

						public static TPData tp = new TPData();

						private static FSFile fileCache = new FSFile();

						public static int GXi_DmaId;

						public static int reg_G2_BG0CNT;

						public static int reg_G2_BG1CNT;

						public static int reg_G2_BG2CNT;

						public static int reg_G2_BG3CNT;

						public static int reg_G2S_DB_BG0CNT;

						public static int reg_G2S_DB_BG1CNT;

						public static int reg_G2S_DB_BG2CNT;

						public static int reg_G2S_DB_BG3CNT;

						public static int reg_GX_DISPCNT;

						public static int reg_G3_POLYGON_ATTR;

						public static int[] hw_mmap = new int[20];

						public static uint[] SDK_AUTOLOAD_DTCM_BSS_END = new uint[1];

						public static BG_CELL[] bgCell = new BG_CELL[16]
						{
							new BG_CELL(),
							new BG_CELL(),
							new BG_CELL(),
							new BG_CELL(),
							new BG_CELL(),
							new BG_CELL(),
							new BG_CELL(),
							new BG_CELL(),
							new BG_CELL(),
							new BG_CELL(),
							new BG_CELL(),
							new BG_CELL(),
							new BG_CELL(),
							new BG_CELL(),
							new BG_CELL(),
							new BG_CELL()
						};

						private static TexBank[] texBank = new TexBank[128]
						{
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank(),
							new TexBank()
						};

						public static IMAGE_TABLE[] imageTable = new IMAGE_TABLE[64]
						{
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE(),
							new IMAGE_TABLE()
						};

						private static NNSG2dRendererInstance renderer;

						private static float[] fnd_reuse_asx = new float[4];

						private static float[] fnd_reuse_asy = new float[4];

						private static float[] fnd_reuse_ax = new float[4];

						private static float[] fnd_reuse_ay = new float[4];

						private static int[] fnd_reuse_index = new int[108]
						{
							0, 0, 0, 1, 1, 1, 1, 1, 1, 0,
							0, 0, 1, 0, 1, 1, 2, 1, 2, 1,
							2, 0, 1, 0, 2, 0, 2, 1, 3, 1,
							3, 1, 3, 0, 2, 0, 0, 1, 0, 2,
							1, 2, 1, 2, 1, 1, 0, 1, 1, 1,
							1, 2, 2, 2, 2, 2, 2, 1, 1, 1,
							2, 1, 2, 2, 3, 2, 3, 2, 3, 1,
							2, 1, 0, 2, 0, 3, 1, 3, 1, 3,
							1, 2, 0, 2, 1, 2, 1, 3, 2, 3,
							2, 3, 2, 2, 1, 2, 2, 2, 2, 3,
							3, 3, 3, 3, 3, 2, 2, 2
						};

						private static int[] fnd_reuse_index2 = new int[12]
						{
							0, 0, 0, 3, 3, 3, 3, 3, 3, 0,
							0, 0
						};

						private static short[] fnd_reuse_oam = new short[7];

						private static float[] fnd_reuse_f = new float[16];

						private static TEXT_DATA[] textData = new TEXT_DATA[256]
						{
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA(),
							new TEXT_DATA()
						};

						private static int[] textColor = new int[16]
						{
							0, -1, 255, -16776961, 16711935, 65535, 16777215, -16711681, -65281, -32513,
							-2139029505, -8355585, -2139062017, -2139062017, -2139062017, -2139062017
						};

						private static HILIGHT_RECT hilightRect = new HILIGHT_RECT();

						private static TPData fnd_reuse_tp = new TPData();

						public static NNSG3dGlb NNS_G3dGlb = new NNSG3dGlb();

						private static float[] fnd_reuse_color = new float[4] { 1f, 1f, 1f, 1f };

						private static float[] fnd_reuse_position = new float[4] { 0f, 1f, 0f, 0f };

						private static uint drawMask;

						private static Vertex[] vertex = new Vertex[20480];

						private static VertexPositionColorTexture[] vtc = new VertexPositionColorTexture[32768];

						private static Shape[] shape = new Shape[256];

						private static MtxFx43[] stackMtx = new MtxFx43[64]
						{
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43()
						};

						private static MtxFx43[] stackMtxN = new MtxFx43[64]
						{
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43(),
							new MtxFx43()
						};

						private static Matrix[] texMtx = new Matrix[64]
						{
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix)
						};

						private static Matrix[] _matrix_stack = new Matrix[64]
						{
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix),
							default(Matrix)
						};

						private static MtxFx33 _mtx33 = new MtxFx33();

						private static VecFx32 _vec32_0 = new VecFx32();

						private static VecFx32 _vec32_1 = new VecFx32();

						private static VecFx32 _vec32_2 = new VecFx32();

						private static Matrix _matrix_0 = Matrix.Identity;

						private static Matrix _matrix_1 = default(Matrix);

						private static Matrix _matrix_current = default(Matrix);

						private static NNSG3dRS fnd_reuse_rs = new NNSG3dRS();

						private static VecFx32 fnd_reuse_pos = new VecFx32();

						private static MtxFx43 fnd_reuse_rootMtx = new MtxFx43();

						private static MtxFx43 fnd_reuse_currentMtxN = new MtxFx43();

						private static MtxFx33 fnd_reuse_bbMtx = new MtxFx33();

						private static MtxFx33 fnd_reuse_bbyMtx = new MtxFx33();

						private static MtxFx43 fnd_reuse_m = new MtxFx43();

						private static MtxFx43 fnd_reuse_m2 = new MtxFx43();

						private static MtxFx43 fnd_reuse_baseMtx = new MtxFx43();

						private static int[] fnd_reuse_baseScale = new int[3];

						private static int[] fnd_reuse__scale = new int[3];

						private static MtxFx43 fnd_reuse_mtx = new MtxFx43();

						private static MtxFx43 fnd_reuse_anmMtx = new MtxFx43();

						private static int[] fnd_reuse_anmScale = new int[3];

						private static VecFx32 fnd_reuse_pos2 = new VecFx32();

						private static int[] fnd_reuse_m_ta = new int[5];

						private static ushort[] fnd_reuse_m_ma = new ushort[5];

						private static NNSSndHandle pSoundTop;

						private static int[] baseVolume = new int[2] { 127, 127 };

						private static bool[] soundMute;

						private static int channelMask;

						private static OSOwnerInfo owner_info;

						private static ushort[,] sScreenBuf;

						private static int LIMIT_OF_FONT;

						public static ds.Vector<NNSG2dFont, ds.FastErasePolicy<NNSG2dFont>> dgsmFontVector;

						private static bool init;

						public static BGMInfo[] g_BGMInfoTable;

						public static SEInfo[] g_SEInfoTable;

						private static int PROGRESS_SPEED;

						private static int FX32_16_SCALE;

						private static int EFFECT_AREA_HEIGHT;

						public static int DIV_STEPS;

						public static int DIV_PROGRESS_SPEED;

						private static int GAP_LIMIT;

						private static int GAP_PER_FRAME;

						public static ds.Vector<ICTARGET, ds.FastErasePolicy<ICTARGET>> g_Targets;

						public static ds.Vector<int, ds.FastErasePolicy<int>> idList;

						public static CCharacterMng characterMng;

						public static CSceneMng sceneMng;

						public static stg.CStageMng stageMng;

						public static string ReplaceTextureCode;

						public static SCRIPT_COMMAND[] commandTable;

						private static VecFx32 ff3Command_reuse_v0;

						private static VecFx32 ff3Command_reuse_v1;

						private static VecFx32 ff3Command_reuse_v2;

						private static VecFx32 ff3Command_reuse_v3;

						private static VecFx32 ff3Command_reuse_va;

						public static ds.Vector<RECORD, ds.OrderSavedErasePolicy<RECORD>> g_SePlayRecord;

						private static int bgm;

						private static byte state_;

						public static byte[,] flags;

						public static uint NUM_VALUE;

						public static int[] values;

						public static ds.Vector<ScrollBar, ds.FastErasePolicy<ScrollBar>> g_ActiveScrollBars;

						private static int LIMIT_OF_NODELIST;

						private static VecFx32 CameraBattlePosition;

						private static VecFx32 CameraBattleTarget;

						private static VecFx32 CameraBattleAngle;

						private static int CameraBattleDistance;

						private static int MOBOOK_LIFE_X;

						private static int MOBOOK_LIFE_Y;

						private static int MOBOOK_EXP_X;

						private static int MOBOOK_EXP_Y;

						private static int MOBOOK_MONEY_X;

						private static int MOBOOK_MONEY_Y;

						public static MBChocoboBankFactory __MBChocoboBankFactory__;

						private static bool endFlag_;

						private static string name_;

						private static bool _flipScreen;

						public static int VEHICLE_MOTIONNO_WAIT;

						public static int VEHICLE_HEIGHT_AIR;

						public static int VEHICLE_HEIGHT_GROUND;

						public static int VEHICLE_HEIGHT_ONSEA;

						public static byte BATTLE_MAP_ONSEA;

						public static byte BATTLE_MAP_ONAIR;

						public static byte BATTLE_MAP_INDEEPSEA;

						public static byte MONSTER_GROUP_ONAIR;

						public static int EFFECT_CATEGORY_VEHICLE;

						public static int EFFECT_MEMBER_DROP;

						public static int EFFECT_MEMBER_SPRAY;

						public static byte BGM_FADEOUT_FRAME;

						public static ds.sys3d.CLightObject g_Light;

						private static menu.Medget m_pMedget;

						private static menu.MBPlayerGold pPlayerGold;

						private static sys2d.Cell dummyCursor_;

						private static int _waitCounter;

						internal static uint m_eMatrixMode;

						internal static Stack<Matrix> m_MatrixStack;

						internal static Color m_ClearColor;

						internal static float m_fClearDepth;

						internal static bool m_bAlphaTest;

						internal static Blend m_Blend;

						internal static bool m_bCullFace;

						internal static RasterizerState m_RasterizerState;

						internal static bool m_bDepthTest;

						internal static bool m_bDepthMask;

						internal static CompareFunction m_DepthFunc;

						internal static uint m_uiBindTexture;

						internal static uint m_uiApplyTexture;

						internal static bool m_bApplyEffect;

						internal static short[] m_asIndex;

						internal static byte[] m_abyCurrentColor;

						internal static bool m_bVertexArray;

						internal static bool m_bColorArray;

						internal static bool m_bCoordArray;

						internal static float[] m_afVertex;

						internal static float[] m_afCoord;

						internal static byte[] m_abyColor;

						internal static int m_iVertexSize;

						internal static int m_iVertexStride;

						internal static int m_iCoordSize;

						internal static int m_iCoordStride;

						internal static int m_iColorSize;

						internal static int m_iColorStride;

						internal static GlTexture[] m_aGlTexture;

						internal static Matrix m_TextureMatrix;

						internal static float[] m_afOrthoMatrix;

						internal static string __DATE__;

						internal static string __TIME__;

						public static string[] m_astrMessage;

						public static TestTimer[] m_aTestTimer;

						internal static Graphics m_Graphics;

						internal static int LENGTH(Array x)
						{
							return x.GetLength(0);
						}

						internal static bool isIPad()
						{
							if (LCD_WIDTH == 512)
							{
								return LCD_HEIGHT == 384;
							}
							return false;
						}

						internal static int OS_MicroSecondsToTicks(ulong usec)
						{
							return (int)(33514 * usec / 64 / 1000);
						}

						internal static float FX_FX32_TO_F32(int x)
						{
							return (float)x / 4096f;
						}

						internal static int FX_F32_TO_FX32(float x)
						{
							return (int)((x > 0f) ? (x * 4096f + 0.5f) : (x * 4096f - 0.5f));
						}

						internal static float FX_FX16_TO_F32(short x)
						{
							return (float)x / 4096f;
						}

						internal static short FX_F32_TO_FX16(float x)
						{
							if (!(x > 0f))
							{
								return (short)(x * 4096f - 0.5f);
							}
							return (short)(x * 4096f + 0.5f);
						}

						internal static float FX_FX64C_TO_F32(long x)
						{
							return (float)x / 4.2949673E+09f;
						}

						internal static long FX_F32_TO_FX64C(float x)
						{
							return (long)((x > 0f) ? (x * 4.2949673E+09f + 0.5f) : (x * 4.2949673E+09f - 0.5f));
						}

						internal static int FX32_CONST(float x)
						{
							return FX_F32_TO_FX32(x);
						}

						internal static int FX32_CAST(long res)
						{
							return (int)res;
						}

						internal static int FX_DEG_TO_RAD(int deg)
						{
							return (int)(74961321L * (long)deg + 2147483648u >> 32);
						}

						internal static int FX_DEG_TO_IDX(int deg)
						{
							return (ushort)(781874935307L * deg + 8796093022208L >> 44);
						}

						internal static int FX_RAD_TO_DEG(int rad)
						{
							return (int)(246083499208L * rad + 2147483648u >> 32);
						}

						internal static int FX_RAD_TO_IDX(int rad)
						{
							return (ushort)(44798133900177L * rad + 8796093022208L >> 44);
						}

						internal static int FX_IDX_TO_RAD(int idx)
						{
							return (int)(411775L * (long)idx + 524288 >> 20);
						}

						internal static int FX_IDX_TO_DEG(int idx)
						{
							return (int)(23592960L * (long)idx + 524288 >> 20);
						}

						internal static ushort GX_RGB(int r, int g, int b)
						{
							return (ushort)(r | (g << 5) | (b << 10));
						}

						internal static int GX_PACK_POLYGONATTR_PARAM(int a, int b, int c, int d, int e, int f)
						{
							return 0;
						}

						internal static uint GX_COMP4x4_PLTT_IDX(int image)
						{
							return (uint)(131072 + ((image & 0x1FFFF) >> 1) + ((image & 0x40000) >> 2));
						}

						internal static int MATH_ABS(int a)
						{
							if (a >= 0)
							{
								return a;
							}
							return -a;
						}

						internal static int MATH_MIN(int a, int b)
						{
							if (a > b)
							{
								return b;
							}
							return a;
						}

						internal static int MATH_MAX(int a, int b)
						{
							if (a < b)
							{
								return b;
							}
							return a;
						}

						internal static int MATH_CLAMP(int x, int low, int high)
						{
							return MATH_MIN(MATH_MAX(x, low), high);
						}

						internal static void FS_EXTERN_OVERLAY(int n)
						{
						}

						internal static int FS_OVERLAY_ID(int n)
						{
							return 0;
						}

						internal static void ATTRIBUTE_ALIGN(int n)
						{
						}

						internal static void NNS_G2D_BG_ASSERT(int n)
						{
						}

						internal static void NNS_G2D_POINTER_ASSERT(int n)
						{
						}

						internal static void NNS_G2D_FONT_ASSERT(int n)
						{
						}

						internal static void NNS_G2D_CHARCANVAS_ASSERT(int n)
						{
						}

						internal static void NNS_G2D_TEXTCANVAS_ASSERT(int n)
						{
						}

						internal static string NNS_G2D_TRANSCODE(char str)
						{
							return new string(new char[1] { str });
						}

						internal static ushort RGB555(int r, int g, int b)
						{
							return (ushort)((b << 10) | (g << 5) | r);
						}

						internal static int EFF_FX32_TO_F32(int x)
						{
							return x;
						}

						internal static float EFF_F32_TO_FX32(float x)
						{
							return x;
						}

						internal static int WIN_RAD_TO_DEG(int a)
						{
							return a;
						}

						internal static int WIN_FX32_TO_F32(int x)
						{
							return x;
						}

						internal static float WIN_F32_TO_FX32(float x)
						{
							return x;
						}

						internal static int ExecRand(int val)
						{
							return eld.EffRand(val);
						}

						internal static uint ARRY2POINT_NUM(uint _cnt)
						{
							return (_cnt >> 2) + 1;
						}

						internal static uint ROUND_UP(int value, int alignment)
						{
							return (uint)((value + (alignment - 1)) & ~(alignment - 1));
						}

						internal static uint ROUND_DOWN(int value, int alignment)
						{
							return (uint)(value & ~(alignment - 1));
						}

						internal static string TRANSCODE(string str)
						{
							return str;
						}

						internal static int gcscmp(string arg0, string arg1)
						{
							return strcmp(arg0, arg1);
						}

						internal static int gcslen(string arg0)
						{
							return strlen(arg0);
						}

						internal static uint KBITStoBYTES(int x)
						{
							return (uint)(x * 1024 / 8);
						}

						internal static bool IS_ZERO_NORM(VecFx32 vec)
						{
							if (vec.x == 0 && vec.y == 0)
							{
								return vec.z == 0;
							}
							return false;
						}

						internal static long ROUNDUP(int value, int align)
						{
							return ((uint)value + (align - 1)) & ~(align - 1);
						}

						internal static uint ALARM_COUNT(ulong x)
						{
							return (uint)OS_MicroSecondsToTicks(x);
						}

						internal static int GET_DECIMAL_PARAMETER(XbnNodeList XBN_NODE_LIST, int INDEX)
						{
							if (XBN_NODE_LIST.size() <= INDEX)
							{
								return -1;
							}
							return XBN_NODE_LIST[INDEX].nodeValueInt();
						}

						internal static string GET_STRING_PARAMETER(XbnNodeList XBN_NODE_LIST, int INDEX)
						{
							if (XBN_NODE_LIST.size() <= INDEX)
							{
								return null;
							}
							return XBN_NODE_LIST[INDEX].nodeValueString();
						}

						internal static void SET_ITEM_NUMBER(menu.Medget MEDGETPTR, int DECIMAL)
						{
							MEDGETPTR.setWork(DECIMAL);
						}

						internal static int GET_ITEM_NUMBER(menu.Medget MEDGETPTR)
						{
							return (int)MEDGETPTR.work();
						}

						internal static void SET_MSG_IDX(menu.Medget MEDGETPTR, int DECIMAL)
						{
							MEDGETPTR.setWork2(DECIMAL);
						}

						internal static int GET_MSG_IDX(menu.Medget MEDGETPTR)
						{
							return (int)MEDGETPTR.work2();
						}

						internal static void SET_ITEM_PTR(menu.Medget MEDGETPTR, object PTR)
						{
							MEDGETPTR.setWork3(PTR);
						}

						internal static object GET_ITEM_PTR(menu.Medget MEDGETPTR)
						{
							return MEDGETPTR.work3();
						}

						internal static void SET_JOB_NUMBER(menu.Medget MEDGETPTR, int DECIMAL)
						{
							MEDGETPTR.setWork(DECIMAL);
						}

						internal static int GET_JOB_NUMBER(menu.Medget MEDGETPTR)
						{
							return (int)MEDGETPTR.work();
						}

						internal static void SET_ACTIVITY(menu.Medget MEDGETPTR, int DECIMAL)
						{
							MEDGETPTR.setWork1(DECIMAL);
						}

						internal static int GET_ACTIVITY(menu.Medget MEDGETPTR)
						{
							return (int)MEDGETPTR.work1();
						}

						internal static void SET_MSG_JOB(menu.Medget MEDGETPTR, object PTR)
						{
							MEDGETPTR.setWork2(PTR);
						}

						internal static object GET_MSG_JOB(menu.Medget MEDGETPTR)
						{
							return MEDGETPTR.work2();
						}

						internal static void SET_MSG_SKILL(menu.Medget MEDGETPTR, object PTR)
						{
							MEDGETPTR.setWork3(PTR);
						}

						internal static object GET_MSG_SKILL(menu.Medget MEDGETPTR)
						{
							return MEDGETPTR.work3();
						}

						internal static void SET_LEVEL(menu.Medget MEDGETPTR, int DECIMAL)
						{
							MEDGETPTR.setWork(DECIMAL);
						}

						internal static int GET_LEVEL(menu.Medget MEDGETPTR)
						{
							return (int)MEDGETPTR.work();
						}

						internal static void SET_ITEM_USABILITY(menu.Medget MEDGETPTR, int DECIMAL)
						{
							MEDGETPTR.setWork3(DECIMAL);
						}

						internal static int GET_ITEM_USABILITY(menu.Medget MEDGETPTR)
						{
							return (int)MEDGETPTR.work3();
						}

						internal static void SET_TXT_NAME(menu.Medget MEDGETPTR, menu.MBText PTR)
						{
							MEDGETPTR.setWork1(PTR);
						}

						internal static menu.MBText GET_TXT_NAME(menu.Medget MEDGETPTR)
						{
							return (menu.MBText)MEDGETPTR.work1();
						}

						internal static void SET_TXT_NUM(menu.Medget MEDGETPTR, menu.MBText PTR)
						{
							MEDGETPTR.setWork2(PTR);
						}

						internal static menu.MBText GET_TXT_NUM(menu.Medget MEDGETPTR)
						{
							return (menu.MBText)MEDGETPTR.work2();
						}

						internal static void SET_CELL_IDX(menu.Medget MEDGETPTR, int DECIMAL)
						{
							MEDGETPTR.setWork(DECIMAL);
						}

						internal static int GET_CELL_IDX(menu.Medget MEDGETPTR)
						{
							return (int)MEDGETPTR.work();
						}

						internal static void SET_PREV(menu.Medget MEDGETPTR, menu.Medget PTR)
						{
							MEDGETPTR.setWork1(PTR);
						}

						internal static menu.Medget GET_PREV(menu.Medget MEDGETPTR)
						{
							return (menu.Medget)MEDGETPTR.work1();
						}

						internal static void SET_NEXT(menu.Medget MEDGETPTR, menu.Medget PTR)
						{
							MEDGETPTR.setWork2(PTR);
						}

						internal static menu.Medget GET_NEXT(menu.Medget MEDGETPTR)
						{
							return (menu.Medget)MEDGETPTR.work2();
						}

						internal static void SET_MSG_NUM(menu.Medget MEDGETPTR, dgs.DGSMessage PTR)
						{
							MEDGETPTR.setWork2(PTR);
						}

						internal static dgs.DGSMessage GET_MSG_NUM(menu.Medget MEDGETPTR)
						{
							return (dgs.DGSMessage)MEDGETPTR.work2();
						}

						internal static void SET_MSG_TOTAL(menu.Medget MEDGETPTR, dgs.DGSMessage PTR)
						{
							MEDGETPTR.setWork3(PTR);
						}

						internal static dgs.DGSMessage GET_MSG_TOTAL(menu.Medget MEDGETPTR)
						{
							return (dgs.DGSMessage)MEDGETPTR.work3();
						}

						internal static int TRANSMOBID(int n)
						{
							return n;
						}

						internal static int BATTLE_COMMAND_X()
						{
							return 16 + (isIPad() ? (-16) : 0);
						}

						internal static int BATTLE_COMMAND_Y()
						{
							return 157 + (isIPad() ? 16 : 0);
						}

						internal static int BATTLE_TARGET_X()
						{
							return BATTLE_COMMAND_X();
						}

						internal static int BATTLE_TARGET_Y()
						{
							return BATTLE_COMMAND_Y();
						}

						internal static int BATTLE_PLAYER_Y()
						{
							return 254 + (isIPad() ? 16 : 0);
						}

						internal static int COLOR_CODE(int r, int g, int b)
						{
							return (r << 3) | (g << 11) | (b << 19);
						}

						internal static int COLOR_ORANGE()
						{
							return COLOR_CODE(31, 22, 0);
						}

						internal static int COLOR_PINK()
						{
							return COLOR_CODE(31, 13, 13);
						}

						internal static int COLOR_GREEN()
						{
							return COLOR_CODE(4, 30, 16);
						}

						internal static T static_cast<T>(object arg0)
						{
							return (T)arg0;
						}

						internal static T const_cast<T>(object arg0)
						{
							return (T)arg0;
						}

						internal static T reinterpret_cast<T>(object arg0)
						{
							return (T)arg0;
						}

						internal static double fabs(double arg0)
						{
							return Math.Abs(arg0);
						}

						internal static Array malloc(int arg0)
						{
							return new byte[arg0];
						}

						internal static object malloc(Type arg0)
						{
							return arg0.GetConstructor(m_DummyType).Invoke(m_DummyObject);
						}

						internal static void free(Array arg0)
						{
						}

						internal static void free(object arg0)
						{
						}

						internal static Array memset(Array arg0, int arg1, int arg2)
						{
							for (int i = 0; i < arg2; i++)
							{
								Buffer.SetByte(arg0, i, (byte)arg1);
							}
							return arg0;
						}

						internal static Array memcpy(Array arg0, Array arg1, int arg2)
						{
							return memcpy(arg0, 0, arg1, 0, arg2);
						}

						internal static Array memcpy(Array arg0, int arg1, Array arg2, int arg3)
						{
							return memcpy(arg0, arg1, arg2, 0, arg3);
						}

						internal static Array memcpy(Array arg0, Array arg1, int arg2, int arg3)
						{
							return memcpy(arg0, 0, arg1, arg2, arg3);
						}

						internal static Array memcpy(Array arg0, int arg1, Array arg2, int arg3, int arg4)
						{
							Buffer.BlockCopy(arg2, arg3, arg0, arg1, arg4);
							return arg0;
						}

						internal static int memcmp(Array arg0, Array arg1, int arg2)
						{
							byte[] array = (byte[])arg0;
							byte[] array2 = (byte[])arg1;
							for (int i = 0; i < arg2; i++)
							{
								if (array[i] != array2[i])
								{
									return array[i] - array2[i];
								}
							}
							return 0;
						}

						internal static int memcmp(Array arg0, string arg1, int arg2)
						{
							byte[] array = (byte[])arg0;
							char[] array2 = arg1.ToCharArray();
							for (int i = 0; i < arg2; i++)
							{
								if (array[i] != array2[i])
								{
									return array[i] - (byte)array2[i];
								}
							}
							return 0;
						}

						internal static int strlen(string arg0)
						{
							return arg0.Length;
						}

						internal static string strcpy(out string arg0, string arg1)
						{
							arg0 = new string(arg1.ToCharArray());
							return arg0;
						}

						internal static char[] strcpy(char[] arg0, int arg1, string arg2)
						{
							arg2.CopyTo(0, arg0, arg1, arg2.Length);
							return arg0;
						}

						internal static string strncpy(out string arg0, string arg1, int arg2)
						{
							if (arg1.Length < arg2)
							{
								arg0 = new string(arg1.ToCharArray());
							}
							else
							{
								arg0 = arg1.Substring(0, arg2);
							}
							return arg0;
						}

						internal static int strcmp(string arg0, string arg1)
						{
							if (!(arg0 == arg1))
							{
								return 1;
							}
							return 0;
						}

						internal static int strncmp(string arg0, string arg1, int arg2)
						{
							if (arg0.Length < arg2)
							{
								return arg0.Length - arg2;
							}
							if (arg1.Length < arg2)
							{
								return arg2 - arg1.Length;
							}
							if (!(arg0.Substring(0, arg2) == arg1.Substring(0, arg2)))
							{
								return 1;
							}
							return 0;
						}

						internal static string strchr(string arg0, int arg1)
						{
							int num = arg0.IndexOf((char)arg1);
							if (num < 0)
							{
								return null;
							}
							return arg0.Substring(num);
						}

						internal static string strrchr(string arg0, int arg1)
						{
							int num = arg0.LastIndexOf((char)arg1);
							if (num < 0)
							{
								return null;
							}
							return arg0.Substring(num);
						}

						internal static string strtok(string arg0, string arg1)
						{
							if (arg0 != null)
							{
								m_strStrtok = arg0;
								m_iStrtok = 0;
							}
							if (m_strStrtok == null)
							{
								return null;
							}
							int num = m_strStrtok.Length;
							for (int i = 0; i < arg1.Length; i++)
							{
								int num2 = m_strStrtok.IndexOf(arg1[i], m_iStrtok);
								if (num2 >= 0 && num > num2)
								{
									num = num2;
								}
							}
							if (num >= m_strStrtok.Length)
							{
								m_strStrtok = null;
								return null;
							}
							string result = m_strStrtok.Substring(m_iStrtok, num - m_iStrtok);
							m_iStrtok = num + 1;
							if (m_iStrtok >= m_strStrtok.Length)
							{
								m_strStrtok = null;
							}
							return result;
						}

						internal static long strtol(string arg0, string arg1, int arg2)
						{
							try
							{
								return long.Parse(arg0, NumberStyles.HexNumber);
							}
							catch (Exception)
							{
							}
							return 0L;
						}

						internal static int sprintf(out string arg0, string arg1, params object[] arg2)
						{
							arg0 = StringUtil.format(arg1, arg2);
							return arg0.Length;
						}

						internal static int atoi(string arg0)
						{
							try
							{
								return int.Parse(arg0);
							}
							catch (Exception)
							{
							}
							return 0;
						}

						internal static int abs(int __n)
						{
							if (__n >= 0)
							{
								return __n;
							}
							return -__n;
						}

						internal static double sqrt(double arg0)
						{
							return Math.Sqrt(arg0);
						}

						internal static double sin(double arg0)
						{
							return Math.Sin(arg0);
						}

						internal static double cos(double arg0)
						{
							return Math.Cos(arg0);
						}

						internal static double atan(double arg0)
						{
							return Math.Atan(arg0);
						}

						internal static double atan2(double arg0, double arg1)
						{
							return Math.Atan2(arg0, arg1);
						}

						internal static int time(int[] arg0)
						{
							return (int)(DateTime.Now - DateTime.Parse("2000/1/1")).TotalSeconds;
						}

						internal static int rand()
						{
							return m_Random.Next();
						}

						internal static void srand(uint arg0)
						{
							m_Random = new Random((int)arg0);
						}

						internal static FILE fopen(string arg0, string arg1)
						{
							FILE fILE = new FILE();
							arg0 = arg0.Substring(arg0.LastIndexOf('/') + 1);
							if (arg1.Equals("rb") || arg1.Equals("r+b"))
							{
								IsolatedStorageFile userStoreForApplication = IsolatedStorageFile.GetUserStoreForApplication();
								if (!userStoreForApplication.FileExists(arg0))
								{
									return null;
								}
								fILE.m_Fs = userStoreForApplication.OpenFile(arg0, FileMode.Open);
							}
							else
							{
								arg1.Equals("wb");
							}
							return fILE;
						}

						internal static int fclose(FILE arg0)
						{
							arg0.m_Fs.Close();
							return 0;
						}

						internal static int fseek(FILE arg0, long arg1, int arg2)
						{
							int result = 0;
							switch (arg2)
							{
							case 0:
								result = (int)arg0.m_Fs.Seek(arg1, SeekOrigin.Begin);
								break;
							case 1:
								result = (int)arg0.m_Fs.Seek(arg1, SeekOrigin.Current);
								break;
							case 2:
								result = (int)arg0.m_Fs.Seek(arg1, SeekOrigin.End);
								break;
							}
							return result;
						}

						internal static long ftell(FILE arg0)
						{
							return arg0.m_Fs.Position;
						}

						internal static int fread(Array arg0, int arg1, int arg2, FILE arg3)
						{
							arg3.m_Fs.Read((byte[])arg0, 0, arg1 * arg2);
							return 0;
						}

						internal static int fwrite(Array arg0, int arg1, int arg2, FILE arg3)
						{
							arg3.m_Fs.Write((byte[])arg0, 0, arg1 * arg2);
							return 0;
						}

						internal static T _add<T>(T arg0, T arg1)
						{
							return Operator<T>.ADD(arg0, arg1);
						}

						internal static T _sub<T>(T arg0, T arg1)
						{
							return Operator<T>.SUB(arg0, arg1);
						}

						internal static T _mul<T>(T arg0, T arg1)
						{
							return Operator<T>.MUL(arg0, arg1);
						}

						internal static T _div<T>(T arg0, T arg1)
						{
							return Operator<T>.DIV(arg0, arg1);
						}

						internal static T _div2<T, U>(T arg0, U arg1)
						{
							return Operator<T, U>.DIV(arg0, arg1);
						}

						internal static T _shl<T>(T arg0, T arg1)
						{
							return Operator<T>.SHL(arg0, arg1);
						}

						internal static T _shr<T>(T arg0, T arg1)
						{
							return Operator<T>.SHR(arg0, arg1);
						}

						internal static T _and<T>(T arg0, T arg1)
						{
							return Operator<T>.AND(arg0, arg1);
						}

						internal static T _or<T>(T arg0, T arg1)
						{
							return Operator<T>.OR(arg0, arg1);
						}

						internal static T _not<T>(T arg0)
						{
							return Operator<T>.NOT(arg0);
						}

						internal static T _neg<T>(T arg0)
						{
							return Operator<T>.NEG(arg0);
						}

						internal static bool _isG<T>(T arg0, T arg1)
						{
							return Operator<T>.G(arg0, arg1).Equals(arg0);
						}

						internal static bool _isGE<T>(T arg0, T arg1)
						{
							return Operator<T>.GE(arg0, arg1).Equals(arg0);
						}

						internal static bool _isE<T>(T arg0, T arg1)
						{
							return Operator<T>.E(arg0, arg1).Equals(arg0);
						}

						internal static bool _isLE<T>(T arg0, T arg1)
						{
							return Operator<T>.LE(arg0, arg1).Equals(arg0);
						}

						internal static bool _isL<T>(T arg0, T arg1)
						{
							return Operator<T>.L(arg0, arg1).Equals(arg0);
						}

						internal static bool _isNE<T>(T arg0, T arg1)
						{
							return Operator<T>.NE(arg0, arg1).Equals(arg0);
						}

						internal static object createRes(ArrayReader src)
						{
							long position = src.getPosition();
							NNSG3dResDataBlockHeader nNSG3dResDataBlockHeader = (NNSG3dResDataBlockHeader)src;
							switch (nNSG3dResDataBlockHeader.kind)
							{
							case 810304589u:
								src.setPosition(position);
								return (NNSG3dResMdlSet)src;
							case 811091284u:
								src.setPosition(position);
								return (NNSG3dResTex)src;
							case 810831434u:
								src.setPosition(position);
								return (NNSG3dResAnmSet)src;
							case 810832467u:
								src.setPosition(position);
								return (NNSG3dResAnmSet)src;
							case 810764630u:
								src.setPosition(position);
								return (NNSG3dResAnmSet)src;
							default:
								src.setPosition(position);
								return (NNSG3dResAnmSet)src;
							}
						}

						internal static NNSG3dResAnmHeader createAnmRes(ArrayReader src)
						{
							NNSG3dResAnmHeader nNSG3dResAnmHeader = new NNSG3dResAnmHeader();
							long position = src.getPosition();
							nNSG3dResAnmHeader.category0 = src.readByte();
							nNSG3dResAnmHeader.revision = src.readByte();
							nNSG3dResAnmHeader.category1 = src.readUInt16();
							switch (nNSG3dResAnmHeader.category1)
							{
							default:
								src.setPosition(position);
								nNSG3dResAnmHeader.m_Anm = (NNSG3dResMatCAnm)src;
								break;
							case 21569:
								src.setPosition(position);
								nNSG3dResAnmHeader.m_Anm = (NNSG3dResTexSRTAnm)src;
								break;
							case 17217:
								src.setPosition(position);
								nNSG3dResAnmHeader.m_Anm = (NNSG3dResJntAnm)src;
								break;
							case 22081:
								src.setPosition(position);
								nNSG3dResAnmHeader.m_Anm = null;
								break;
							}
							return nNSG3dResAnmHeader;
						}

						internal static bool NNSG3dResNamecmp(NNSG3dResName arg0, NNSG3dResName arg1, int arg2)
						{
							return strcmp(arg0.name, arg1.name) == 0;
						}

						internal static bool VecFx32cmp(VecFx32 arg0, VecFx32 arg1)
						{
							if (arg0.x != arg1.x)
							{
								return false;
							}
							if (arg0.y != arg1.y)
							{
								return false;
							}
							if (arg0.z != arg1.z)
							{
								return false;
							}
							return true;
						}

						internal static void VecFx32cpy(VecFx32 arg0, VecFx32 arg1)
						{
							arg0.x = arg1.x;
							arg0.y = arg1.y;
							arg0.z = arg1.z;
						}

						internal static Array malloc_count(int size)
						{
							Array array = malloc(size);
							if (array != null)
							{
								memset(array, 0, size);
								memCount++;
							}
							return array;
						}

						internal static object malloc_count(Type size)
						{
							object obj = malloc(size);
							if (obj != null)
							{
								memCount++;
							}
							return obj;
						}

						internal static void free_count(Array p)
						{
							if (p != null)
							{
								free(p);
								memCount--;
							}
						}

						internal static void free_count(object p)
						{
							if (p != null)
							{
								free(p);
								memCount--;
							}
						}

						internal static void trace(string format, params object[] args)
						{
							MainActivity.trace(StringUtil.format(format, args));
						}

						internal static void webTo()
						{
							MainActivity.webTo();
						}

						internal static void startDownload()
						{
							MainActivity.startDownload();
						}

						internal static bool isAutoSave()
						{
							return isAutoSave_;
						}

						internal static void setAutoSave(bool b)
						{
							isAutoSave_ = b;
						}

						internal static int getStringWidth(string str, int size)
						{
							if (font[size] == null)
							{
								font[size] = new Font(size);
							}
							return (int)font[size].drawString(str, 0f, 0f, 0, 0);
						}

						internal static void drawString(string str, int x, int y, int color, int size)
						{
							if (font[size] == null)
							{
								font[size] = new Font(size);
							}
							font[size].drawString(str, x, y, color, 1);
						}

						internal static void fillRect(int x, int y, int w, int h, int color)
						{
							if (skipFrame == 0)
							{
								vtc[0].Position.X = (float)x - 0.5f;
								vtc[0].Position.Y = (float)y - 0.5f;
								vtc[0].Position.Z = 0f;
								vtc[0].Color.R = (byte)(color >> 24);
								vtc[0].Color.G = (byte)(color >> 16);
								vtc[0].Color.B = (byte)(color >> 8);
								vtc[0].Color.A = (byte)color;
								vtc[1].Position.X = (float)x - 0.5f;
								vtc[1].Position.Y = (float)(y + h) + 0.5f;
								vtc[1].Position.Z = 0f;
								vtc[1].Color.R = vtc[0].Color.R;
								vtc[1].Color.G = vtc[0].Color.G;
								vtc[1].Color.B = vtc[0].Color.B;
								vtc[1].Color.A = vtc[0].Color.A;
								vtc[2].Position.X = (float)(x + w) + 0.5f;
								vtc[2].Position.Y = (float)y - 0.5f;
								vtc[2].Position.Z = 0f;
								vtc[2].Color.R = vtc[0].Color.R;
								vtc[2].Color.G = vtc[0].Color.G;
								vtc[2].Color.B = vtc[0].Color.B;
								vtc[2].Color.A = vtc[0].Color.A;
								vtc[3].Position.X = (float)(x + w) + 0.5f;
								vtc[3].Position.Y = (float)(y + h) + 0.5f;
								vtc[3].Position.Z = 0f;
								vtc[3].Color.R = vtc[0].Color.R;
								vtc[3].Color.G = vtc[0].Color.G;
								vtc[3].Color.B = vtc[0].Color.B;
								vtc[3].Color.A = vtc[0].Color.A;
								glDisable(3553u);
								glDrawArrays(5u, 0, 4, vtc);
							}
						}

						internal static void GX_FixScreen(int fix)
						{
							flipScreen = fix;
						}

						internal static int GX_GetFlipScreen()
						{
							return flipScreen;
						}

						internal static void GX_FlipProjectionMatrix()
						{
							glLoadIdentity();
							if (flipScreen != 0)
							{
								float[] m = new float[16]
								{
									-1f, 0f, 0f, 0f, 0f, -1f, 0f, 0f, 0f, 0f,
									1f, 0f, 0f, 0f, 0f, 1f
								};
								glMultMatrixf(m);
							}
						}

						internal static void GX_FlipProjectionMatrixAdjust()
						{
							glLoadIdentity();
							if (flipScreen != 0)
							{
								float num = 0f;
								float num2 = 0f;
								float num3 = 200000f;
								if (!m_Graphics.isFlipScreen())
								{
									num = 0f - 480f / num3;
									num2 = 800f / num3;
								}
								else
								{
									num2 = 800f / num3;
								}
								float[] array = new float[16]
								{
									-1f, 0f, 0f, 0f, 0f, -1f, 0f, 0f, 0f, 0f,
									1f, 0f, 0f, 0f, 0f, 1f
								};
								array[12] = num;
								array[13] = num2;
								float[] m = array;
								glMultMatrixf(m);
							}
							else if (m_Graphics.isFlipScreen())
							{
								float num4 = 0f;
								float num5 = 0f;
								float num6 = 200000f;
								num4 = 480f / num6;
								num5 = 0f - 800f / num6;
								float[] array2 = new float[16]
								{
									1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f,
									1f, 0f, 0f, 0f, 0f, 1f
								};
								array2[12] = num4;
								array2[13] = num5;
								float[] m2 = array2;
								glMultMatrixf(m2);
							}
							else
							{
								float num7 = 0f;
								float num8 = 0f;
								float num9 = 200000f;
								if (flipScreen == 0)
								{
									num8 = 0f - 800f / num9;
								}
								float[] array3 = new float[16]
								{
									1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f,
									1f, 0f, 0f, 0f, 0f, 1f
								};
								array3[12] = num7;
								array3[13] = num8;
								float[] m3 = array3;
								glMultMatrixf(m3);
							}
						}

						internal static void OS_AssignBackButton(int assign)
						{
							backButton = assign;
						}

						internal static void SuspendFont()
						{
							for (int i = 0; i < LENGTH(font); i++)
							{
								if (font[i] != null)
								{
									font[i].destruct();
								}
							}
							for (int j = 0; j < LENGTH(font); j++)
							{
								font[j] = null;
							}
						}

						internal static void pause(JNIEnv _env, object thiz)
						{
							ds.GlobalPlayTimeCounter.getSingleton().pause(b: true);
							SuspendFont();
							SuspendTexture();
						}

						internal static void resume(JNIEnv _env, object thiz)
						{
							ds.GlobalPlayTimeCounter.getSingleton().pause(b: false);
							m_Graphics.DrawStringStart();
							drawString(" ", 0, 0, 0, 16);
							m_Graphics.DrawStringEnd();
						}

						internal static void touch(JNIEnv _env, object thiz, int count, int peak, float x0, float y0, float x1, float y1)
						{
							if (peak != 1)
							{
								tapCount = 0;
							}
							else if (tapCount == 0)
							{
								tapX = x0;
								tapY = y0;
								tapCount = 1;
							}
							else if (tapCount < 10 && fabs(x0 - tapX) < (double)(16f / (float)LCD_WIDTH) && fabs(y0 - tapY) < (double)(16f / (float)LCD_HEIGHT))
							{
								x0 = tapX;
								y0 = tapY;
							}
							touchCount = 0;
							touchPeak = 0;
							int num = cont;
							cont = 0;
							if (flipScreen != 0)
							{
								x0 = 1f - x0;
								y0 = 1f - y0;
								x1 = 1f - x1;
								y1 = 1f - y1;
							}
							for (int i = 0; i < peak; i++)
							{
								float num2 = ((i == 0) ? x0 : x1) * (float)LCD_WIDTH;
								float num3 = ((i == 0) ? y0 : y1) * (float)LCD_HEIGHT;
								if (touchPeak < 2)
								{
									touchX[touchPeak] = num2;
									touchY[touchPeak] = num3;
									touchPeak++;
									if (i < count)
									{
										touchCount++;
									}
								}
							}
							trg |= ~num & cont;
						}

						internal static void render(JNIEnv _env, object thiz)
						{
							env = _env;
							ulong currentFrame = (ulong)MainActivity.getCurrentFrame((long)prevFrame);
							int num = MATH_CLAMP((int)(currentFrame - prevFrame), 1, 3) * ((boost == 0) ? 1 : 3);
							cont |= MainActivity.getKeyEvent();
							int num2 = backButton;
							glClearColor(0f, 0f, 0f, 1f);
							glClear(16640u);
							fpsCount++;
							if (prevFrame / 30 != currentFrame / 30)
							{
								fps = fpsCount;
								fpsCount = 0;
							}
							TP_Update(num);
							prevFrame = currentFrame;
							polyCount = 0;
							if (tapCount != 0)
							{
								tapCount += num;
							}
							for (int i = 0; i < num; i++)
							{
								skipFrame = ((i != num - 1) ? 1 : 0);
								contF = cont & 0xFFFF & ~(contF & trg);
								trg = 0;
								backButton = 0;
								NNS_G2dResetMatrix(depthtest: false);
								glEnable(3042u);
								glBlendFunc(770u, 771u);
								glAlphaFunc(516u, 0.01f);
								glEnable(3008u);
								glEnable(16384u);
								glEnable(2884u);
								glCullFace(1029u);
								glEnable(2929u);
								glDepthFunc(515u);
								glDepthMask(1);
								NitroMain();
								NNS_G2dResetMatrix(depthtest: false);
								for (int j = 0; j < 12; j++)
								{
									CallVBlankIntr();
								}
								DrawFade();
								TP_Update2();
							}
							NNS_SndUpdate();
							if (backButton != num2)
							{
								MainActivity.assignBackButton(backButton);
							}
						}

						internal static void OS_Init()
						{
						}

						internal static int OS_GetLanguage()
						{
							return MainActivity.getLanguage();
						}

						internal static void OS_SetLanguage(int language)
						{
							MainActivity.setLanguage(language);
						}

						internal static void OS_EnableInterrupts()
						{
						}

						internal static int OS_DisableInterrupts()
						{
							return 0;
						}

						internal static void OS_RestoreInterrupts(int state)
						{
						}

						internal static void OS_Terminate()
						{
							while (true)
							{
							}
						}

						internal static void OS_EnableIrq()
						{
						}

						internal static void OS_EnableIrqMask(uint intr)
						{
						}

						internal static void OS_SetIrqFunction(uint intrBit, OSIrqFunction function)
						{
							if (intrBit == 1)
							{
								vBlankFunction = function;
							}
						}

						internal static void OS_SetIrqCheckFlag(uint intr)
						{
						}

						internal static uint OS_GetVBlankCount()
						{
							return 0u;
						}

						internal static void CallVBlankIntr()
						{
							if (vBlankFunction != null)
							{
								vBlankFunction();
							}
						}

						internal static void OS_InitAlarm()
						{
						}

						internal static void OS_CreateAlarm(int alarm)
						{
						}

						internal static void OS_SetPeriodicAlarm(int alarm, int start, int period, OSAlarmHandler handler, Array arg)
						{
						}

						internal static void OS_CancelAlarm(int alarm)
						{
						}

						internal static void OS_InitTick()
						{
						}

						internal static int OS_GetTick()
						{
							return 0;
						}

						internal static void OS_InitThread()
						{
						}

						internal static int OS_IsThreadAvailable()
						{
							return 0;
						}

						internal static int OS_GetCurrentThread()
						{
							return 0;
						}

						internal static int OS_GetLockID()
						{
							return 1;
						}

						internal static void DC_InvalidateRange(Array startAddr, uint nBytes)
						{
						}

						internal static void DC_StoreRange(Array startAddr, uint nBytes)
						{
						}

						internal static void DC_FlushRange(Array startAddr, uint nBytes)
						{
						}

						internal static void OS_EnableMainExArena()
						{
						}

						internal static void OS_SetDTCMArenaHi(Array newHi)
						{
						}

						internal static void OS_SetDTCMArenaLo(Array newLo)
						{
						}

						internal static int OS_GetArenaHi(int id)
						{
							return 0;
						}

						internal static int OS_GetMainArenaHi()
						{
							return 0;
						}

						internal static Array OS_GetDTCMArenaHi()
						{
							return null;
						}

						internal static int OS_GetArenaLo(int id)
						{
							return 0;
						}

						internal static int OS_GetMainArenaLo()
						{
							return 0;
						}

						internal static Array OS_GetDTCMArenaLo()
						{
							return null;
						}

						internal static Array OS_AllocFromArenaLo(int id, uint size, uint align)
						{
							return null;
						}

						internal static Array OS_AllocFromMainArenaLo(uint size, uint align)
						{
							return null;
						}

						internal static void OS_GetOwnerInfo(OSOwnerInfo info)
						{
							ushort[] array = new ushort[1];
							ushort[] nickName = array;
							info.language = 0;
							info.nickName = nickName;
							info.nickNameLength = 0;
						}

						internal static void OS_GetMacAddress(byte[] macAddress)
						{
						}

						internal static uint OS_GetConsoleType()
						{
							return 0u;
						}

						internal static void OS_Printf(string fmt, params object[] args)
						{
						}

						internal static void OSi_TWarning(string file, int line, string fmt, params object[] args)
						{
						}

						internal static void OSi_Panic(string file, int line, string fmt, params object[] args)
						{
							OS_Terminate();
						}

						internal static void OS_SetThreadStackWarningOffset(int thread, uint offset)
						{
						}

						internal static void FX_Init()
						{
						}

						internal static int FX_Whole(int v)
						{
							return v >> 12;
						}

						internal static int FX_Mul32x64c(int v32, long v64c)
						{
							long num = v64c * v32 + 2147483648u;
							return (int)(num >> 32);
						}

						internal static int FX_Div(int numer, int denom)
						{
							if (denom == 0)
							{
								return numer;
							}
							long num = (long)numer << 12;
							return (int)(num / denom);
						}

						internal static long FX_DivFx64c(int numer, int denom)
						{
							if (denom == 0)
							{
								return numer;
							}
							long num = (long)numer << 32;
							return num / denom;
						}

						internal static int FX_Sqrt(int x)
						{
							return (int)sqrt((float)x * 4096f);
						}

						internal static short FX_SinIdx(int idx)
						{
							double num = sin((double)idx * 9.587379924285248E-05);
							return FX_F32_TO_FX16((float)num);
						}

						internal static short FX_CosIdx(int idx)
						{
							double num = cos((double)idx * 9.587379924285248E-05);
							return FX_F32_TO_FX16((float)num);
						}

						internal static long FX_SinFx64c(int rad)
						{
							double num = sin(FX_FX32_TO_F32(rad));
							return FX_F32_TO_FX64C((float)num);
						}

						internal static long FX_CosFx64c(int rad)
						{
							double num = cos(FX_FX32_TO_F32(rad));
							return FX_F32_TO_FX64C((float)num);
						}

						internal static short FX_AtanIdx(int x)
						{
							double num = atan(FX_FX32_TO_F32(x)) * 10430.378350470464;
							return (short)((num > 0.0) ? (num + 0.5) : (num - 0.5));
						}

						internal static ushort FX_Atan2Idx(int y, int x)
						{
							double num = atan2(FX_FX32_TO_F32(y), FX_FX32_TO_F32(x)) * 10430.378350470464;
							return (ushort)(short)((num > 0.0) ? (num + 0.5) : (num - 0.5));
						}

						internal static void VEC_Set(VecFx32 a, int x, int y, int z)
						{
							a.x = x;
							a.y = y;
							a.z = z;
						}

						internal static void VEC_Add(VecFx32 a, VecFx32 b, VecFx32 ab)
						{
							ab.x = a.x + b.x;
							ab.y = a.y + b.y;
							ab.z = a.z + b.z;
						}

						internal static void VEC_Subtract(VecFx32 a, VecFx32 b, VecFx32 ab)
						{
							ab.x = a.x - b.x;
							ab.y = a.y - b.y;
							ab.z = a.z - b.z;
						}

						internal static void VEC_Subtract(ds.Vector3<int> a, ds.Vector3<int> b, ds.Vector3<int> ab)
						{
							ab.vx = a.vx - b.vx;
							ab.vy = a.vy - b.vy;
							ab.vz = a.vz - b.vz;
						}

						internal static int VEC_DotProduct(VecFx32 a, VecFx32 b)
						{
							return (int)((long)a.x * (long)b.x + (long)a.y * (long)b.y + (long)a.z * (long)b.z + 2048 >> 12);
						}

						internal static int VEC_DotProduct(ds.Vector3<int> a, ds.Vector3<int> b)
						{
							return (int)((long)a.vx * (long)b.vx + (long)a.vy * (long)b.vy + (long)a.vz * (long)b.vz + 2048 >> 12);
						}

						internal static void VEC_CrossProduct(VecFx32 a, VecFx32 b, VecFx32 axb)
						{
							long num = a.x;
							long num2 = a.y;
							long num3 = a.z;
							axb.x = (int)(num2 * b.z - num3 * b.y + 2048 >> 12);
							axb.y = (int)(num3 * b.x - num * b.z + 2048 >> 12);
							axb.z = (int)(num * b.y - num2 * b.x + 2048 >> 12);
						}

						internal static int VEC_Mag(VecFx32 v)
						{
							long num = v.x;
							long num2 = v.y;
							long num3 = v.z;
							return (int)sqrt(num * num + num2 * num2 + num3 * num3);
						}

						internal static int VEC_Mag(ds.Vector3<int> v)
						{
							long num = v.vx;
							long num2 = v.vy;
							long num3 = v.vz;
							return (int)sqrt(num * num + num2 * num2 + num3 * num3);
						}

						internal static int VEC_Mag(int vx, int vy, int vz)
						{
							long num = vx;
							long num2 = vy;
							long num3 = vz;
							return (int)sqrt(num * num + num2 * num2 + num3 * num3);
						}

						internal static int VEC_Distance(VecFx32 v1, VecFx32 v2)
						{
							VecFx32 vecFx = nitro_reuse_v0;
							VEC_Subtract(v1, v2, vecFx);
							return VEC_Mag(vecFx);
						}

						internal static int VEC_Distance(ds.Vector3<int> v1, ds.Vector3<int> v2)
						{
							ds.Vector3<int> vector = nitro_reuse_v1;
							VEC_Subtract(v1, v2, vector);
							return VEC_Mag(vector);
						}

						internal static void VEC_Normalize(VecFx32 pSrc, VecFx32 pDst)
						{
							int num = VEC_Mag(pSrc);
							if (num != 0)
							{
								pDst.x = FX_Div(pSrc.x, num);
								pDst.y = FX_Div(pSrc.y, num);
								pDst.z = FX_Div(pSrc.z, num);
							}
							else
							{
								pDst.x = 0;
								pDst.y = 0;
								pDst.z = 0;
							}
						}

						internal static void VEC_Normalize(ds.Vector3<int> pSrc, ds.Vector3<int> pDst)
						{
							int num = VEC_Mag(pSrc);
							if (num != 0)
							{
								pDst.vx = FX_Div(pSrc.vx, num);
								pDst.vy = FX_Div(pSrc.vy, num);
								pDst.vz = FX_Div(pSrc.vz, num);
							}
							else
							{
								pDst.vx = 0;
								pDst.vy = 0;
								pDst.vz = 0;
							}
						}

						internal static void VEC_MultAdd(int a, VecFx32 v1, VecFx32 v2, VecFx32 pDest)
						{
							pDest.x = FX_Mul(v1.x, a) + v2.x;
							pDest.y = FX_Mul(v1.y, a) + v2.y;
							pDest.z = FX_Mul(v1.z, a) + v2.z;
						}

						internal static void VEC_Fx16Set(VecFx16 a, short x, short y, short z)
						{
							a.x = x;
							a.y = y;
							a.z = z;
						}

						internal static void MTX_ScaleApply22(MtxFx22 pSrc, MtxFx22 pDst, int x, int y)
						{
							pDst._00 = FX_Mul(pSrc._00, x);
							pDst._01 = FX_Mul(pSrc._01, y);
							pDst._10 = FX_Mul(pSrc._10, x);
							pDst._11 = FX_Mul(pSrc._11, y);
						}

						internal static void MTX_Identity33(MtxFx33 pDst)
						{
							pDst._00 = (pDst._11 = (pDst._22 = 4096));
							pDst._01 = (pDst._02 = (pDst._10 = (pDst._12 = (pDst._20 = (pDst._21 = 0)))));
						}

						internal static void MTX_Copy33To43(MtxFx33 pSrc, MtxFx43 pDst)
						{
							pDst._00 = pSrc._00;
							pDst._01 = pSrc._01;
							pDst._02 = pSrc._02;
							pDst._10 = pSrc._10;
							pDst._11 = pSrc._11;
							pDst._12 = pSrc._12;
							pDst._20 = pSrc._20;
							pDst._21 = pSrc._21;
							pDst._22 = pSrc._22;
						}

						internal static void MTX_RotX33(MtxFx33 pDst, int sinVal, int cosVal)
						{
							pDst._00 = 4096;
							pDst._01 = 0;
							pDst._02 = 0;
							pDst._10 = 0;
							pDst._11 = cosVal;
							pDst._12 = sinVal;
							pDst._20 = 0;
							pDst._21 = -sinVal;
							pDst._22 = cosVal;
						}

						internal static void MTX_RotY33(MtxFx33 pDst, int sinVal, int cosVal)
						{
							pDst._11 = 4096;
							pDst._12 = 0;
							pDst._10 = 0;
							pDst._21 = 0;
							pDst._22 = cosVal;
							pDst._20 = sinVal;
							pDst._01 = 0;
							pDst._02 = -sinVal;
							pDst._00 = cosVal;
						}

						internal static void MTX_RotZ33(MtxFx33 pDst, int sinVal, int cosVal)
						{
							pDst._22 = 4096;
							pDst._20 = 0;
							pDst._21 = 0;
							pDst._02 = 0;
							pDst._00 = cosVal;
							pDst._01 = sinVal;
							pDst._12 = 0;
							pDst._10 = -sinVal;
							pDst._11 = cosVal;
						}

						internal static void MTX_Inverse33(MtxFx33 pSrc, MtxFx33 pDst)
						{
							int num = FX_Mul(FX_Mul(pSrc._00, pSrc._11), pSrc._22) + FX_Mul(FX_Mul(pSrc._10, pSrc._21), pSrc._02) + FX_Mul(FX_Mul(pSrc._20, pSrc._01), pSrc._12) - FX_Mul(FX_Mul(pSrc._00, pSrc._21), pSrc._12) - FX_Mul(FX_Mul(pSrc._10, pSrc._01), pSrc._22) - FX_Mul(FX_Mul(pSrc._20, pSrc._11), pSrc._02);
							if (num == 0)
							{
								MTX_Identity33(pDst);
								return;
							}
							num = FX_Div(4096, num);
							int v = FX_Mul(pSrc._11, pSrc._22) - FX_Mul(pSrc._12, pSrc._21);
							int v2 = FX_Mul(pSrc._21, pSrc._02) - FX_Mul(pSrc._22, pSrc._01);
							int v3 = FX_Mul(pSrc._01, pSrc._12) - FX_Mul(pSrc._02, pSrc._11);
							int v4 = FX_Mul(pSrc._12, pSrc._20) - FX_Mul(pSrc._10, pSrc._22);
							int v5 = FX_Mul(pSrc._22, pSrc._00) - FX_Mul(pSrc._20, pSrc._02);
							int v6 = FX_Mul(pSrc._02, pSrc._10) - FX_Mul(pSrc._00, pSrc._12);
							int v7 = FX_Mul(pSrc._10, pSrc._21) - FX_Mul(pSrc._11, pSrc._20);
							int v8 = FX_Mul(pSrc._20, pSrc._01) - FX_Mul(pSrc._21, pSrc._00);
							int v9 = FX_Mul(pSrc._00, pSrc._11) - FX_Mul(pSrc._01, pSrc._10);
							pDst._00 = FX_Mul(v, num);
							pDst._01 = FX_Mul(v2, num);
							pDst._02 = FX_Mul(v3, num);
							pDst._10 = FX_Mul(v4, num);
							pDst._11 = FX_Mul(v5, num);
							pDst._12 = FX_Mul(v6, num);
							pDst._20 = FX_Mul(v7, num);
							pDst._21 = FX_Mul(v8, num);
							pDst._22 = FX_Mul(v9, num);
						}

						internal static void MTX_Concat33(MtxFx33 a, MtxFx33 b, MtxFx33 ab)
						{
							long num = a._00;
							long num2 = a._01;
							long num3 = a._02;
							long num4 = a._10;
							long num5 = a._11;
							long num6 = a._12;
							long num7 = a._20;
							long num8 = a._21;
							long num9 = a._22;
							long num10 = b._00;
							long num11 = b._01;
							long num12 = b._02;
							long num13 = b._10;
							long num14 = b._11;
							long num15 = b._12;
							long num16 = b._20;
							long num17 = b._21;
							long num18 = b._22;
							ab._00 = (int)(num * num10 + num2 * num13 + num3 * num16 + 2048 >> 12);
							ab._01 = (int)(num * num11 + num2 * num14 + num3 * num17 + 2048 >> 12);
							ab._02 = (int)(num * num12 + num2 * num15 + num3 * num18 + 2048 >> 12);
							ab._10 = (int)(num4 * num10 + num5 * num13 + num6 * num16 + 2048 >> 12);
							ab._11 = (int)(num4 * num11 + num5 * num14 + num6 * num17 + 2048 >> 12);
							ab._12 = (int)(num4 * num12 + num5 * num15 + num6 * num18 + 2048 >> 12);
							ab._20 = (int)(num7 * num10 + num8 * num13 + num9 * num16 + 2048 >> 12);
							ab._21 = (int)(num7 * num11 + num8 * num14 + num9 * num17 + 2048 >> 12);
							ab._22 = (int)(num7 * num12 + num8 * num15 + num9 * num18 + 2048 >> 12);
						}

						internal static void MTX_MultVec33(VecFx32 vec, MtxFx33 m, VecFx32 dst)
						{
							long num = vec.x;
							long num2 = vec.y;
							long num3 = vec.z;
							dst.x = (int)(num * m._00 + num2 * m._10 + num3 * m._20 + 2048 >> 12);
							dst.y = (int)(num * m._01 + num2 * m._11 + num3 * m._21 + 2048 >> 12);
							dst.z = (int)(num * m._02 + num2 * m._12 + num3 * m._22 + 2048 >> 12);
						}

						internal static void MTX_Identity43(MtxFx43 pDst)
						{
							int num = (pDst._22 = 4096);
							int _ = (pDst._11 = num);
							pDst._00 = _;
							int num4 = (pDst._32 = 0);
							int num6 = (pDst._31 = num4);
							int num8 = (pDst._30 = num6);
							int num10 = (pDst._21 = num8);
							int num12 = (pDst._20 = num10);
							int num14 = (pDst._12 = num12);
							int num16 = (pDst._10 = num14);
							int _2 = (pDst._02 = num16);
							pDst._01 = _2;
						}

						internal static void MTX_Scale43(MtxFx43 pDst, int x, int y, int z)
						{
							pDst._00 = x;
							pDst._11 = y;
							pDst._22 = z;
							int num = (pDst._32 = 0);
							int num3 = (pDst._31 = num);
							int num5 = (pDst._30 = num3);
							int num7 = (pDst._21 = num5);
							int num9 = (pDst._20 = num7);
							int num11 = (pDst._12 = num9);
							int num13 = (pDst._10 = num11);
							int _ = (pDst._02 = num13);
							pDst._01 = _;
						}

						internal static void MTX_ScaleApply43(MtxFx43 pSrc, MtxFx43 pDst, int x, int y, int z)
						{
							pDst._00 = FX_Mul(pSrc._00, x);
							pDst._01 = FX_Mul(pSrc._01, x);
							pDst._02 = FX_Mul(pSrc._02, x);
							pDst._10 = FX_Mul(pSrc._10, y);
							pDst._11 = FX_Mul(pSrc._11, y);
							pDst._12 = FX_Mul(pSrc._12, y);
							pDst._20 = FX_Mul(pSrc._20, z);
							pDst._21 = FX_Mul(pSrc._21, z);
							pDst._22 = FX_Mul(pSrc._22, z);
							pDst._30 = pSrc._30;
							pDst._31 = pSrc._31;
							pDst._32 = pSrc._32;
						}

						internal static void MTX_RotX43(MtxFx43 pDst, int sinVal, int cosVal)
						{
							pDst._00 = 4096;
							pDst._01 = 0;
							pDst._02 = 0;
							pDst._10 = 0;
							pDst._11 = cosVal;
							pDst._12 = sinVal;
							pDst._20 = 0;
							pDst._21 = -sinVal;
							pDst._22 = cosVal;
							pDst._30 = 0;
							pDst._31 = 0;
							pDst._32 = 0;
						}

						internal static void MTX_RotY43(MtxFx43 pDst, int sinVal, int cosVal)
						{
							pDst._11 = 4096;
							pDst._12 = 0;
							pDst._10 = 0;
							pDst._21 = 0;
							pDst._22 = cosVal;
							pDst._20 = sinVal;
							pDst._01 = 0;
							pDst._02 = -sinVal;
							pDst._00 = cosVal;
							pDst._30 = 0;
							pDst._31 = 0;
							pDst._32 = 0;
						}

						internal static void MTX_RotZ43(MtxFx43 pDst, int sinVal, int cosVal)
						{
							pDst._22 = 4096;
							pDst._20 = 0;
							pDst._21 = 0;
							pDst._02 = 0;
							pDst._00 = cosVal;
							pDst._01 = sinVal;
							pDst._12 = 0;
							pDst._10 = -sinVal;
							pDst._11 = cosVal;
							pDst._30 = 0;
							pDst._31 = 0;
							pDst._32 = 0;
						}

						internal static void MTX_Inverse43(MtxFx43 pSrc, MtxFx43 pDst)
						{
							MtxFx33 mtxFx = nitro_reuse_mtx33Src;
							MtxFx33 mtxFx2 = nitro_reuse_mtx33Dst;
							mtxFx.copy(pSrc);
							mtxFx2.copy(pDst);
							MTX_Inverse33(mtxFx, mtxFx2);
							pSrc.copy(mtxFx);
							pDst.copy(mtxFx2);
							int _ = pSrc._30;
							int _2 = pSrc._31;
							int _3 = pSrc._32;
							pDst._30 = 0;
							pDst._31 = 0;
							pDst._32 = 0;
							MTX_TransApply43(pDst, pDst, -_, -_2, -_3);
						}

						internal static void MTX_Concat43(MtxFx43 a, MtxFx43 b, MtxFx43 ab)
						{
							long num = a.a[0];
							long num2 = a.a[1];
							long num3 = a.a[2];
							long num4 = a.a[3];
							long num5 = a.a[4];
							long num6 = a.a[5];
							long num7 = a.a[6];
							long num8 = a.a[7];
							long num9 = a.a[8];
							long num10 = a.a[9];
							long num11 = a.a[10];
							long num12 = a.a[11];
							long num13 = b.a[0];
							long num14 = b.a[1];
							long num15 = b.a[2];
							long num16 = b.a[3];
							long num17 = b.a[4];
							long num18 = b.a[5];
							long num19 = b.a[6];
							long num20 = b.a[7];
							long num21 = b.a[8];
							ab.a[0] = (int)(num * num13 + num2 * num16 + num3 * num19 + 2048 >> 12);
							ab.a[1] = (int)(num * num14 + num2 * num17 + num3 * num20 + 2048 >> 12);
							ab.a[2] = (int)(num * num15 + num2 * num18 + num3 * num21 + 2048 >> 12);
							ab.a[3] = (int)(num4 * num13 + num5 * num16 + num6 * num19 + 2048 >> 12);
							ab.a[4] = (int)(num4 * num14 + num5 * num17 + num6 * num20 + 2048 >> 12);
							ab.a[5] = (int)(num4 * num15 + num5 * num18 + num6 * num21 + 2048 >> 12);
							ab.a[6] = (int)(num7 * num13 + num8 * num16 + num9 * num19 + 2048 >> 12);
							ab.a[7] = (int)(num7 * num14 + num8 * num17 + num9 * num20 + 2048 >> 12);
							ab.a[8] = (int)(num7 * num15 + num8 * num18 + num9 * num21 + 2048 >> 12);
							ab.a[9] = (int)(num10 * num13 + num11 * num16 + num12 * num19 + 2048 >> 12) + b.a[9];
							ab.a[10] = (int)(num10 * num14 + num11 * num17 + num12 * num20 + 2048 >> 12) + b.a[10];
							ab.a[11] = (int)(num10 * num15 + num11 * num18 + num12 * num21 + 2048 >> 12) + b.a[11];
						}

						internal static void MTX_MultVec43(VecFx32 vec, MtxFx43 m, VecFx32 dst)
						{
							long num = vec.x;
							long num2 = vec.y;
							long num3 = vec.z;
							dst.x = (int)(num * m.a[0] + num2 * m.a[3] + num3 * m.a[6] + 2048 >> 12) + m.a[9];
							dst.y = (int)(num * m.a[1] + num2 * m.a[4] + num3 * m.a[7] + 2048 >> 12) + m.a[10];
							dst.z = (int)(num * m.a[2] + num2 * m.a[5] + num3 * m.a[8] + 2048 >> 12) + m.a[11];
						}

						internal static void MTX_MultVec43(ds.Vector3<int> vec, MtxFx43 m, ds.Vector3<int> dst)
						{
							long num = vec.vx;
							long num2 = vec.vy;
							long num3 = vec.vz;
							dst.vx = (int)(num * m.a[0] + num2 * m.a[3] + num3 * m.a[6] + 2048 >> 12) + m.a[9];
							dst.vy = (int)(num * m.a[1] + num2 * m.a[4] + num3 * m.a[7] + 2048 >> 12) + m.a[10];
							dst.vz = (int)(num * m.a[2] + num2 * m.a[5] + num3 * m.a[8] + 2048 >> 12) + m.a[11];
						}

						internal static void MTX_TransApply43(MtxFx43 pSrc, MtxFx43 pDst, int x, int y, int z)
						{
							pDst.copy(pSrc);
							pDst._30 += FX_Mul(x, pDst._00) + FX_Mul(y, pDst._10) + FX_Mul(z, pDst._20);
							pDst._31 += FX_Mul(x, pDst._01) + FX_Mul(y, pDst._11) + FX_Mul(z, pDst._21);
							pDst._32 += FX_Mul(x, pDst._02) + FX_Mul(y, pDst._12) + FX_Mul(z, pDst._22);
						}

						internal static void MTX_Copy43ToGLfloat(MtxFx43 pSrc, float[] pDst)
						{
							pDst[0] = FX_FX32_TO_F32(pSrc._00);
							pDst[1] = FX_FX32_TO_F32(pSrc._01);
							pDst[2] = FX_FX32_TO_F32(pSrc._02);
							pDst[3] = 0f;
							pDst[4] = FX_FX32_TO_F32(pSrc._10);
							pDst[5] = FX_FX32_TO_F32(pSrc._11);
							pDst[6] = FX_FX32_TO_F32(pSrc._12);
							pDst[7] = 0f;
							pDst[8] = FX_FX32_TO_F32(pSrc._20);
							pDst[9] = FX_FX32_TO_F32(pSrc._21);
							pDst[10] = FX_FX32_TO_F32(pSrc._22);
							pDst[11] = 0f;
							pDst[12] = FX_FX32_TO_F32(pSrc._30);
							pDst[13] = FX_FX32_TO_F32(pSrc._31);
							pDst[14] = FX_FX32_TO_F32(pSrc._32);
							pDst[15] = 1f;
						}

						internal static void MTX_Copy44ToGLfloat(MtxFx44 pSrc, float[] pDst)
						{
							pDst[0] = FX_FX32_TO_F32(pSrc._00);
							pDst[1] = FX_FX32_TO_F32(pSrc._01);
							pDst[2] = FX_FX32_TO_F32(pSrc._02);
							pDst[3] = FX_FX32_TO_F32(pSrc._03);
							pDst[4] = FX_FX32_TO_F32(pSrc._10);
							pDst[5] = FX_FX32_TO_F32(pSrc._11);
							pDst[6] = FX_FX32_TO_F32(pSrc._12);
							pDst[7] = FX_FX32_TO_F32(pSrc._13);
							pDst[8] = FX_FX32_TO_F32(pSrc._20);
							pDst[9] = FX_FX32_TO_F32(pSrc._21);
							pDst[10] = FX_FX32_TO_F32(pSrc._22);
							pDst[11] = FX_FX32_TO_F32(pSrc._23);
							pDst[12] = FX_FX32_TO_F32(pSrc._30);
							pDst[13] = FX_FX32_TO_F32(pSrc._31);
							pDst[14] = FX_FX32_TO_F32(pSrc._32);
							pDst[15] = FX_FX32_TO_F32(pSrc._33);
						}

						internal static void GX_SetPower(int gxbit_power)
						{
						}

						internal static void GX_Power3D(int enable)
						{
							enable3D = enable;
						}

						internal static void GX_SetDispSelect(GXDispSelect sel)
						{
						}

						internal static GXDispSelect GX_GetDispSelect()
						{
							return GXDispSelect.GX_DISP_SELECT_SUB_MAIN;
						}

						internal static void GX_SetPriority3D(int priority)
						{
							priority3D = priority;
						}

						internal static int GX_GetPrioriry3D()
						{
							return priority3D;
						}

						internal static void GX_Init()
						{
							for (int i = 0; i < vertex.Length; i++)
							{
								vertex[i] = new Vertex();
							}
							for (int i = 0; i < vtc.Length; i++)
							{
								vtc[i] = default(VertexPositionColorTexture);
							}
							for (int i = 0; i < shape.Length; i++)
							{
								shape[i] = new Shape();
							}
						}

						internal static uint GX_GetDefaultDMA()
						{
							return 0u;
						}

						internal static void GX_SetBankForBG(GXVRamBG n)
						{
						}

						internal static void GX_SetBankForOBJ(GXVRamOBJ n)
						{
						}

						internal static void GX_SetBankForBGExtPltt(GXVRamBGExtPltt bgExtPltt)
						{
						}

						internal static void GX_SetBankForOBJExtPltt(GXVRamOBJExtPltt objExtPltt)
						{
						}

						internal static void GX_SetBankForTex(GXVRamTex tex)
						{
						}

						internal static void GX_SetBankForTexPltt(GXVRamTexPltt texPltt)
						{
						}

						internal static void GX_SetBankForSubBG(GXVRamSubBG sub_bg)
						{
						}

						internal static void GX_SetBankForSubOBJ(GXVRamSubOBJ sub_obj)
						{
						}

						internal static void GX_SetBankForSubBGExtPltt(GXVRamSubBGExtPltt sub_bgExtPltt)
						{
						}

						internal static void GX_SetBankForSubOBJExtPltt(GXVRamSubOBJExtPltt sub_objExtPltt)
						{
						}

						internal static void GX_SetBankForLCDC(int lcdc)
						{
						}

						internal static GXVRamTex GX_GetBankForTex()
						{
							return GXVRamTex.GX_VRAM_TEX_0_A;
						}

						internal static GXVRamTexPltt GX_GetBankForTexPltt()
						{
							return GXVRamTexPltt.GX_VRAM_TEXPLTT_0_F;
						}

						internal static GXVRamSubBG GX_ResetBankForSubBG()
						{
							return GXVRamSubBG.GX_VRAM_SUB_BG_NONE;
						}

						internal static GXVRamSubOBJ GX_ResetBankForSubOBJ()
						{
							return GXVRamSubOBJ.GX_VRAM_SUB_OBJ_NONE;
						}

						internal static GXVRamBG GX_DisableBankForBG()
						{
							return GXVRamBG.GX_VRAM_BG_NONE;
						}

						internal static GXVRamOBJ GX_DisableBankForOBJ()
						{
							return GXVRamOBJ.GX_VRAM_OBJ_NONE;
						}

						internal static GXVRamBGExtPltt GX_DisableBankForBGExtPltt()
						{
							return GXVRamBGExtPltt.GX_VRAM_BGEXTPLTT_NONE;
						}

						internal static GXVRamOBJExtPltt GX_DisableBankForOBJExtPltt()
						{
							return GXVRamOBJExtPltt.GX_VRAM_OBJEXTPLTT_NONE;
						}

						internal static GXVRamTex GX_DisableBankForTex()
						{
							return GXVRamTex.GX_VRAM_TEX_NONE;
						}

						internal static GXVRamTexPltt GX_DisableBankForTexPltt()
						{
							return GXVRamTexPltt.GX_VRAM_TEXPLTT_NONE;
						}

						internal static int GX_DisableBankForClearImage()
						{
							return 0;
						}

						internal static GXVRamSubBG GX_DisableBankForSubBG()
						{
							return GXVRamSubBG.GX_VRAM_SUB_BG_NONE;
						}

						internal static GXVRamSubOBJ GX_DisableBankForSubOBJ()
						{
							return GXVRamSubOBJ.GX_VRAM_SUB_OBJ_NONE;
						}

						internal static GXVRamSubBGExtPltt GX_DisableBankForSubBGExtPltt()
						{
							return GXVRamSubBGExtPltt.GX_VRAM_SUB_BGEXTPLTT_NONE;
						}

						internal static GXVRamSubOBJExtPltt GX_DisableBankForSubOBJExtPltt()
						{
							return GXVRamSubOBJExtPltt.GX_VRAM_SUB_OBJEXTPLTT_NONE;
						}

						internal static void GX_DisableBankForARM7()
						{
						}

						internal static void GX_DisableBankForLCDC()
						{
						}

						internal static int GX_GetVCount()
						{
							return 0;
						}

						internal static int GX_HBlankIntr(int enable)
						{
							return 0;
						}

						internal static int GX_VBlankIntr(int enable)
						{
							return 0;
						}

						internal static void GX_SetGraphicsMode(GXDispMode dispMode, GXBGMode bgMode, int bg0_2d3d)
						{
						}

						internal static GXDispCnt GX_GetDispCnt()
						{
							return new GXDispCnt(0);
						}

						internal static void GX_SetVisiblePlane(int plane)
						{
							visiblePlane[0] = plane;
						}

						internal static int GX_GetVisiblePlane()
						{
							return visiblePlane[0];
						}

						internal static void GX_SetVisibleWnd(int window)
						{
						}

						internal static void GX_DispOn()
						{
						}

						internal static void GX_DispOff()
						{
						}

						internal static void GX_SetBGScrOffset(int offset)
						{
						}

						internal static void GX_SetBGCharOffset(int offset)
						{
						}

						internal static void GX_LoadBGPltt(Array pSrc, uint offset, uint szByte)
						{
						}

						internal static void GX_LoadBG0Scr(Array pSrc, uint offset, uint szByte)
						{
						}

						internal static void GX_LoadBG1Scr(Array pSrc, uint offset, uint szByte)
						{
						}

						internal static void GX_LoadBG2Scr(Array pSrc, uint offset, uint szByte)
						{
						}

						internal static void GX_LoadBG3Scr(Array pSrc, uint offset, uint szByte)
						{
						}

						internal static void GX_LoadBG0Char(Array pSrc, uint offset, uint szByte)
						{
						}

						internal static void GX_LoadBG1Char(Array pSrc, uint offset, uint szByte)
						{
						}

						internal static void GX_LoadBG2Char(Array pSrc, uint offset, uint szByte)
						{
						}

						internal static void GX_LoadBG3Char(Array pSrc, uint offset, uint szByte)
						{
						}

						internal static void GX_BeginLoadBGExtPltt()
						{
						}

						internal static void GX_LoadBGExtPltt(Array pSrc, uint destSlotAddr, uint szByte)
						{
						}

						internal static void GX_EndLoadBGExtPltt()
						{
						}

						internal static void GX_BeginLoadTex()
						{
						}

						internal static void GX_LoadTex(Array pSrc, uint destSlotAddr, uint szByte)
						{
						}

						internal static void GX_EndLoadTex()
						{
						}

						internal static void GX_BeginLoadTexPltt()
						{
						}

						internal static void GX_LoadTexPltt(Array pSrc, uint destSlotAddr, uint szByte)
						{
						}

						internal static void GX_EndLoadTexPltt()
						{
						}

						internal static void GX_SetCapture(GXCaptureSize sz, GXCaptureMode mode, GXCaptureSrcA a, GXCaptureSrcB b, GXCaptureDest dest, int eva, int evb)
						{
						}

						internal static void GX_SetMasterBrightness(int brightness)
						{
							masterBrightness = brightness;
						}

						internal static void DrawFade()
						{
							glMatrixMode(5889u);
							GX_FlipProjectionMatrix();
							glOrthof(0f, LCD_WIDTH, LCD_HEIGHT, 0f, -1024f, 1024f);
							glMatrixMode(5888u);
							glLoadIdentity();
							if (masterBrightness < 0)
							{
								fillRect(0, 0, LCD_WIDTH, LCD_HEIGHT, -masterBrightness * 255 / 16);
							}
							if (masterBrightness > 0)
							{
								fillRect(0, 0, LCD_WIDTH, LCD_HEIGHT, (int)(0xFFFFFF00u | (masterBrightness * 255 / 16)));
							}
						}

						internal static void GXS_SetGraphicsMode(GXBGMode bgMode)
						{
						}

						internal static GXSDispCnt GXS_GetDispCnt()
						{
							return new GXSDispCnt(0);
						}

						internal static void GXS_SetVisiblePlane(int plane)
						{
							visiblePlane[1] = plane;
						}

						internal static int GXS_GetVisiblePlane()
						{
							return visiblePlane[1];
						}

						internal static void GXS_SetVisibleWnd(int window)
						{
						}

						internal static void GXS_DispOn()
						{
						}

						internal static void GXS_DispOff()
						{
						}

						internal static void GXS_SetOBJVRamModeBmp(int mode)
						{
						}

						internal static void GXS_LoadOAM(Array pSrc, uint offset, uint szByte)
						{
						}

						internal static void GXS_LoadBGPltt(Array pSrc, uint offset, uint szByte)
						{
						}

						internal static void GXS_LoadBG0Scr(Array pSrc, uint offset, uint szByte)
						{
						}

						internal static void GXS_LoadBG1Scr(Array pSrc, uint offset, uint szByte)
						{
						}

						internal static void GXS_LoadBG2Scr(Array pSrc, uint offset, uint szByte)
						{
						}

						internal static void GXS_LoadBG3Scr(Array pSrc, uint offset, uint szByte)
						{
						}

						internal static void GXS_LoadBG0Char(Array pSrc, uint offset, uint szByte)
						{
						}

						internal static void GXS_LoadBG1Char(Array pSrc, uint offset, uint szByte)
						{
						}

						internal static void GXS_LoadBG2Char(Array pSrc, uint offset, uint szByte)
						{
						}

						internal static void GXS_LoadBG3Char(Array pSrc, uint offset, uint szByte)
						{
						}

						internal static void GXS_BeginLoadBGExtPltt()
						{
						}

						internal static void GXS_LoadBGExtPltt(Array pSrc, uint destSlotAddr, uint szByte)
						{
						}

						internal static void GXS_EndLoadBGExtPltt()
						{
						}

						internal static void GXS_SetMasterBrightness(int brightness)
						{
						}

						internal static void G2_SetOBJAttr(GXOamAttr oam, int x, int y, int priority, int mode, int mosaic, int effect, int shape, int color, int charName, int cParam, int rsParam)
						{
						}

						internal static void G2_SetBG0Control(int screenSize, int colorMode, GXBGScrBase screenBase, GXBGCharBase charBase, int bgExtPltt)
						{
						}

						internal static GXBg01Control G2_GetBG0Control()
						{
							return new GXBg01Control(0);
						}

						internal static void G2_SetBG1Control(int screenSize, int colorMode, GXBGScrBase screenBase, GXBGCharBase charBase, int bgExtPltt)
						{
						}

						internal static GXBg01Control G2_GetBG1Control()
						{
							return new GXBg01Control(0);
						}

						internal static void G2_SetBG2ControlText(int screenSize, int colorMode, GXBGScrBase screenBase, GXBGCharBase charBase)
						{
						}

						internal static GXBg23ControlText G2_GetBG2ControlText()
						{
							return new GXBg23ControlText(0);
						}

						internal static GXBg23Control256Bmp G2_GetBG2Control256Bmp()
						{
							return new GXBg23Control256Bmp(0);
						}

						internal static GXBg23ControlDCBmp G2_GetBG2ControlDCBmp()
						{
							return new GXBg23ControlDCBmp(0);
						}

						internal static void G2_SetBG3ControlText(int screenSize, int colorMode, GXBGScrBase screenBase, GXBGCharBase charBase)
						{
						}

						internal static GXBg23ControlText G2_GetBG3ControlText()
						{
							return new GXBg23ControlText(0);
						}

						internal static GXBg23Control256Bmp G2_GetBG3Control256Bmp()
						{
							return new GXBg23Control256Bmp(0);
						}

						internal static GXBg23ControlDCBmp G2_GetBG3ControlDCBmp()
						{
							return new GXBg23ControlDCBmp(0);
						}

						internal static int G2_GetBG2ExtMode()
						{
							return 0;
						}

						internal static int G2_GetBG3ExtMode()
						{
							return 0;
						}

						internal static GXBg23Control256x16Pltt G2_GetBG2Control256x16Pltt()
						{
							return new GXBg23Control256x16Pltt(0);
						}

						internal static GXBg23Control256x16Pltt G2_GetBG3Control256x16Pltt()
						{
							return new GXBg23Control256x16Pltt(0);
						}

						internal static void G2_SetBG0Priority(int priority)
						{
							bgPriority[0] = priority;
						}

						internal static void G2_SetBG1Priority(int priority)
						{
							bgPriority[1] = priority;
						}

						internal static void G2_SetBG2Priority(int priority)
						{
							bgPriority[2] = priority;
						}

						internal static void G2_SetBG3Priority(int priority)
						{
							bgPriority[3] = priority;
						}

						internal static Array G2_GetBG0ScrPtr()
						{
							return screen;
						}

						internal static Array G2_GetBG1ScrPtr()
						{
							return screen;
						}

						internal static Array G2_GetBG2ScrPtr()
						{
							return screen;
						}

						internal static Array G2_GetBG3ScrPtr()
						{
							return screen;
						}

						internal static Array G2_GetBG0CharPtr()
						{
							return null;
						}

						internal static Array G2_GetBG1CharPtr()
						{
							return null;
						}

						internal static Array G2_GetBG2CharPtr()
						{
							return null;
						}

						internal static Array G2_GetBG3CharPtr()
						{
							return null;
						}

						internal static void G2_SetBG0Offset(int hOffset, int vOffset)
						{
							bgOffset[0, 0] = hOffset;
							bgOffset[0, 1] = vOffset;
						}

						internal static void G2_SetBG1Offset(int hOffset, int vOffset)
						{
							bgOffset[1, 0] = hOffset;
							bgOffset[1, 1] = vOffset;
						}

						internal static void G2_SetBG2Offset(int hOffset, int vOffset)
						{
							bgOffset[2, 0] = hOffset;
							bgOffset[2, 1] = vOffset;
						}

						internal static void G2_SetBG3Offset(int hOffset, int vOffset)
						{
							bgOffset[3, 0] = hOffset;
							bgOffset[3, 1] = vOffset;
						}

						internal static void G2_SetScreenOffset(int hOffset, int vOffset)
						{
							screenOffset[0] = hOffset;
							screenOffset[1] = vOffset;
						}

						internal static void G2_SetWnd0InsidePlane(int wnd, int effect)
						{
						}

						internal static void G2_SetWndOutsidePlane(int wnd, int effect)
						{
						}

						internal static void G2_SetWnd0Position(int x1, int y1, int x2, int y2)
						{
						}

						internal static void G2_BlendNone()
						{
							textAlpha[0] = 16;
							textBrightness[0] = 0;
						}

						internal static void G2_SetBlendAlpha(int plane1, int plane2, int ev1, int ev2)
						{
							textAlpha[0] = MATH_MIN(ev1, 16);
						}

						internal static void G2_SetBlendBrightness(int plane, int brightness)
						{
							textBrightness[0] = brightness;
						}

						internal static void G2S_SetBG0Control(int screenSize, int colorMode, GXBGScrBase screenBase, GXBGCharBase charBase, int bgExtPltt)
						{
						}

						internal static GXBg01Control G2S_GetBG0Control()
						{
							return new GXBg01Control(0);
						}

						internal static void G2S_SetBG1Control(int screenSize, int colorMode, GXBGScrBase screenBase, GXBGCharBase charBase, int bgExtPltt)
						{
						}

						internal static GXBg01Control G2S_GetBG1Control()
						{
							return new GXBg01Control(0);
						}

						internal static void G2S_SetBG2ControlText(int screenSize, int colorMode, GXBGScrBase screenBase, GXBGCharBase charBase)
						{
						}

						internal static GXBg23ControlText G2S_GetBG2ControlText()
						{
							return new GXBg23ControlText(0);
						}

						internal static GXBg23Control256Bmp G2S_GetBG2Control256Bmp()
						{
							return new GXBg23Control256Bmp(0);
						}

						internal static void G2S_SetBG2ControlDCBmp(int screenSize, int areaOver, int screenBase)
						{
						}

						internal static GXBg23ControlDCBmp G2S_GetBG2ControlDCBmp()
						{
							return new GXBg23ControlDCBmp(0);
						}

						internal static void G2S_SetBG3ControlText(int screenSize, int colorMode, GXBGScrBase screenBase, GXBGCharBase charBase)
						{
						}

						internal static GXBg23ControlText G2S_GetBG3ControlText()
						{
							return new GXBg23ControlText(0);
						}

						internal static GXBg23Control256Bmp G2S_GetBG3Control256Bmp()
						{
							return new GXBg23Control256Bmp(0);
						}

						internal static GXBg23ControlDCBmp G2S_GetBG3ControlDCBmp()
						{
							return new GXBg23ControlDCBmp(0);
						}

						internal static int G2S_GetBG2ExtMode()
						{
							return 0;
						}

						internal static int G2S_GetBG3ExtMode()
						{
							return 0;
						}

						internal static GXBg23Control256x16Pltt G2S_GetBG2Control256x16Pltt()
						{
							return new GXBg23Control256x16Pltt(0);
						}

						internal static GXBg23Control256x16Pltt G2S_GetBG3Control256x16Pltt()
						{
							return new GXBg23Control256x16Pltt(0);
						}

						internal static void G2S_SetBG0Priority(int priority)
						{
							bgPriority[4] = priority;
						}

						internal static void G2S_SetBG1Priority(int priority)
						{
							bgPriority[5] = priority;
						}

						internal static void G2S_SetBG2Priority(int priority)
						{
							bgPriority[6] = priority;
						}

						internal static void G2S_SetBG3Priority(int priority)
						{
							bgPriority[7] = priority;
						}

						internal static void G2S_BG2Mosaic(int enable)
						{
						}

						internal static Array G2S_GetBG0ScrPtr()
						{
							return screen;
						}

						internal static Array G2S_GetBG1ScrPtr()
						{
							return screen;
						}

						internal static Array G2S_GetBG2ScrPtr()
						{
							return screen;
						}

						internal static Array G2S_GetBG3ScrPtr()
						{
							return screen;
						}

						internal static Array G2S_GetBG0CharPtr()
						{
							return null;
						}

						internal static Array G2S_GetBG1CharPtr()
						{
							return null;
						}

						internal static Array G2S_GetBG2CharPtr()
						{
							return null;
						}

						internal static Array G2S_GetBG3CharPtr()
						{
							return null;
						}

						internal static int G2S_GetOBJCharPtr()
						{
							return 0;
						}

						internal static void G2S_SetBG0Offset(int hOffset, int vOffset)
						{
							bgOffset[4, 0] = hOffset;
							bgOffset[4, 1] = vOffset;
						}

						internal static void G2S_SetBG1Offset(int hOffset, int vOffset)
						{
							bgOffset[5, 0] = hOffset;
							bgOffset[5, 1] = vOffset;
						}

						internal static void G2S_SetBG2Offset(int hOffset, int vOffset)
						{
							bgOffset[6, 0] = hOffset;
							bgOffset[6, 1] = vOffset;
						}

						internal static void G2S_SetBG3Offset(int hOffset, int vOffset)
						{
							bgOffset[7, 0] = hOffset;
							bgOffset[7, 1] = vOffset;
						}

						internal static void G2S_SetWnd0InsidePlane(int wnd, int effect)
						{
						}

						internal static void G2S_SetWndOutsidePlane(int wnd, int effect)
						{
						}

						internal static void G2S_SetWnd0Position(int x1, int y1, int x2, int y2)
						{
						}

						internal static void G2S_SetWnd1Position(int x1, int y1, int x2, int y2)
						{
						}

						internal static void G2S_BlendNone()
						{
							textAlpha[1] = 16;
							textBrightness[1] = 0;
						}

						internal static void G2S_SetBlendAlpha(int plane1, int plane2, int ev1, int ev2)
						{
							textAlpha[1] = MATH_MIN(ev1, 16);
						}

						internal static void G2S_SetBlendBrightness(int plane, int brightness)
						{
							textBrightness[1] = brightness;
						}

						internal static void G2S_ChangeBlendBrightness(int brightness)
						{
							textBrightness[1] = brightness;
						}

						internal static void transVertex(Vertex v0, Vertex v1, Vertex v2, float[] p, float[] t, byte[] c, ref int d)
						{
							int num = d * 3;
							int num2 = d * 2;
							int num3 = d * 4;
							p[num++] = v0.pos0;
							p[num++] = v0.pos1;
							p[num++] = v0.pos2;
							t[num2++] = v0.tex0;
							t[num2++] = v0.tex1;
							c[num3++] = v0.clr0;
							c[num3++] = v0.clr1;
							c[num3++] = v0.clr2;
							c[num3++] = v0.clr3;
							p[num++] = v1.pos0;
							p[num++] = v1.pos1;
							p[num++] = v1.pos2;
							t[num2++] = v1.tex0;
							t[num2++] = v1.tex1;
							c[num3++] = v1.clr0;
							c[num3++] = v1.clr1;
							c[num3++] = v1.clr2;
							c[num3++] = v1.clr3;
							p[num++] = v2.pos0;
							p[num++] = v2.pos1;
							p[num++] = v2.pos2;
							t[num2++] = v2.tex0;
							t[num2++] = v2.tex1;
							c[num3++] = v2.clr0;
							c[num3++] = v2.clr1;
							c[num3++] = v2.clr2;
							c[num3++] = v2.clr3;
							d += 3;
						}

						internal static void transVertex(Vertex v0, Vertex v1, Vertex v2, VertexPositionColorTexture[] ptc, ref int d)
						{
							ptc[d].Position.X = v0.pos0;
							ptc[d].Position.Y = v0.pos1;
							ptc[d].Position.Z = v0.pos2;
							ptc[d].TextureCoordinate.X = v0.tex0;
							ptc[d].TextureCoordinate.Y = v0.tex1;
							ptc[d].Color.PackedValue = (uint)((v0.clr0 & 0xFF) | ((v0.clr1 & 0xFF) << 8) | ((v0.clr2 & 0xFF) << 16) | ((v0.clr3 & 0xFF) << 24));
							d++;
							ptc[d].Position.X = v1.pos0;
							ptc[d].Position.Y = v1.pos1;
							ptc[d].Position.Z = v1.pos2;
							ptc[d].TextureCoordinate.X = v1.tex0;
							ptc[d].TextureCoordinate.Y = v1.tex1;
							ptc[d].Color.PackedValue = (uint)((v1.clr0 & 0xFF) | ((v1.clr1 & 0xFF) << 8) | ((v1.clr2 & 0xFF) << 16) | ((v1.clr3 & 0xFF) << 24));
							d++;
							ptc[d].Position.X = v2.pos0;
							ptc[d].Position.Y = v2.pos1;
							ptc[d].Position.Z = v2.pos2;
							ptc[d].TextureCoordinate.X = v2.tex0;
							ptc[d].TextureCoordinate.Y = v2.tex1;
							ptc[d].Color.PackedValue = (uint)((v2.clr0 & 0xFF) | ((v2.clr1 & 0xFF) << 8) | ((v2.clr2 & 0xFF) << 16) | ((v2.clr3 & 0xFF) << 24));
							d++;
						}

						internal static void G3_Begin(GXBegin primitive)
						{
							glDepthMask(0);
							g3.primitive = primitive;
							g3.index = 0;
						}

						internal static void G3_End()
						{
							glDepthMask(1);
						}

						internal static void G3_Vtx(short x, short y, short z)
						{
							g3.pos.x = x;
							g3.pos.y = y;
							g3.pos.z = z;
							if (skipFrame != 0 || enable3D == 0)
							{
								return;
							}
							VecFx32 vecFx = nitro_reuse_pos;
							MTX_MultVec43(g3.pos, currentMtx, vecFx);
							Vertex vertex = g3.vertex[g3.index++];
							vertex.pos0 = FX_FX32_TO_F32(vecFx.x);
							vertex.pos1 = FX_FX32_TO_F32(vecFx.y);
							vertex.pos2 = FX_FX32_TO_F32(vecFx.z);
							vertex.tex0 = g3.tex[0];
							vertex.tex1 = g3.tex[1];
							vertex.clr0 = g3.clr[0];
							vertex.clr1 = g3.clr[1];
							vertex.clr2 = g3.clr[2];
							vertex.clr3 = g3.clr[3];
							int d = 0;
							switch (g3.primitive)
							{
							default:
								return;
							case GXBegin.GX_BEGIN_TRIANGLES:
								if (g3.index < 3)
								{
									return;
								}
								transVertex(g3.vertex[0], g3.vertex[1], g3.vertex[2], vtc, ref d);
								g3.index = 0;
								break;
							case GXBegin.GX_BEGIN_TRIANGLE_STRIP:
								if (g3.index < 4)
								{
									return;
								}
								if (g3.index == 3)
								{
									transVertex(g3.vertex[0], g3.vertex[1], g3.vertex[2], vtc, ref d);
									break;
								}
								transVertex(g3.vertex[2], g3.vertex[1], g3.vertex[3], vtc, ref d);
								g3.vertex[0].copy(g3.vertex[2]);
								g3.vertex[1].copy(g3.vertex[3]);
								g3.index = 2;
								break;
							case GXBegin.GX_BEGIN_QUADS:
								if (g3.index < 4)
								{
									return;
								}
								transVertex(g3.vertex[0], g3.vertex[1], g3.vertex[2], vtc, ref d);
								transVertex(g3.vertex[2], g3.vertex[3], g3.vertex[0], vtc, ref d);
								g3.index = 0;
								break;
							case GXBegin.GX_BEGIN_QUAD_STRIP:
								if (g3.index < 4)
								{
									return;
								}
								transVertex(g3.vertex[0], g3.vertex[1], g3.vertex[3], vtc, ref d);
								transVertex(g3.vertex[3], g3.vertex[2], g3.vertex[0], vtc, ref d);
								g3.vertex[0].copy(g3.vertex[2]);
								g3.vertex[1].copy(g3.vertex[3]);
								g3.index = 2;
								break;
							}
							glDrawArrays(4u, 0, d, vtc);
							polyCount += d;
						}

						internal static void G3_VtxXY(short x, short y)
						{
							G3_Vtx(x, y, (short)g3.pos.z);
						}

						internal static void G3_Color(ushort rgb)
						{
							g3.clr[0] = (byte)((rgb & 0x1F) * 255 / 31);
							g3.clr[1] = (byte)(((rgb >> 5) & 0x1F) * 255 / 31);
							g3.clr[2] = (byte)(((rgb >> 10) & 0x1F) * 255 / 31);
						}

						internal static void G3_TexCoord(int s, int t)
						{
							g3.tex[0] = FX_FX32_TO_F32(s) * texScaleU;
							g3.tex[1] = FX_FX32_TO_F32(t) * texScaleV;
						}

						internal static void G3_PolygonAttr(int light, GXPolygonMode polyMode, GXCull cullMode, int polygonID, int alpha, int misc)
						{
							g3.clr[3] = (byte)(alpha * 255 / 31);
							if (cullMode == GXCull.GX_CULL_ALL)
							{
								g3.clr[3] = 0;
							}
							if (cullMode == GXCull.GX_CULL_FRONT)
							{
								glCullFace(1028u);
							}
							else
							{
								glCullFace(1029u);
							}
						}

						internal static void G3_MtxMode(GXMtxMode mode)
						{
							currentMode = mode;
						}

						internal static void G3_Identity()
						{
							if (currentMode == GXMtxMode.GX_MTXMODE_POSITION || currentMode == GXMtxMode.GX_MTXMODE_POSITION_VECTOR)
							{
								MTX_Identity43(currentMtx);
							}
						}

						internal static void G3_LoadMtx43(MtxFx43 m)
						{
							if (currentMode == GXMtxMode.GX_MTXMODE_POSITION || currentMode == GXMtxMode.GX_MTXMODE_POSITION_VECTOR)
							{
								currentMtx.copy(m);
							}
						}

						internal static void G3_LoadMtx44(MtxFx44 m)
						{
							if (currentMode == GXMtxMode.GX_MTXMODE_PROJECTION)
							{
								projectionMtx.copy(m);
								float[] array = nitro_reuse_f;
								MTX_Copy44ToGLfloat(m, array);
								glMatrixMode(5889u);
								GX_FlipProjectionMatrix();
								glMultMatrixf(array);
								glMatrixMode(5888u);
							}
						}

						internal static void G3_MultMtx43(MtxFx43 m)
						{
							if (currentMode == GXMtxMode.GX_MTXMODE_POSITION || currentMode == GXMtxMode.GX_MTXMODE_POSITION_VECTOR)
							{
								MTX_Concat43(m, currentMtx, currentMtx);
							}
						}

						internal static void G3_PushMtx()
						{
							stack[pStack].copy(currentMtx);
							pStack++;
						}

						internal static void G3_PopMtx(int num)
						{
							pStack -= num;
							if (currentMode == GXMtxMode.GX_MTXMODE_POSITION || currentMode == GXMtxMode.GX_MTXMODE_POSITION_VECTOR)
							{
								currentMtx.copy(stack[pStack]);
							}
						}

						internal static void G3_RestoreMtx(int num)
						{
							if (currentMode == GXMtxMode.GX_MTXMODE_POSITION || currentMode == GXMtxMode.GX_MTXMODE_POSITION_VECTOR)
							{
								currentMtx.copy(stack[pStack - num]);
							}
						}

						internal static void G3_Scale(int x, int y, int z)
						{
							MTX_ScaleApply43(currentMtx, currentMtx, x, y, z);
						}

						internal static void G3_Translate(int x, int y, int z)
						{
							MTX_TransApply43(currentMtx, currentMtx, x, y, z);
						}

						internal static void G3_ViewPort(int x1, int y1, int x2, int y2)
						{
						}

						internal static void G3_MaterialColorDiffAmb(ushort diffuse, ushort ambient, int IsSetVtxColor)
						{
						}

						internal static void G3_MaterialColorSpecEmi(ushort specular, ushort emission, int IsShininess)
						{
						}

						internal static void G3_TexImageParam(GXTexFmt texFmt, int texGen, GXTexSizeS s, GXTexSizeT t, int repeat, int flip, int pltt0, TexVram addr)
						{
							SendTextureParam(addr, s, t);
						}

						internal static void G3_TexPlttBase(uint addr, GXTexFmt texfmt)
						{
						}

						internal static bool G3_CheckTexImage()
						{
							return texScaleU != 0f;
						}

						internal static void G3_SwapBuffers(int am, int zw)
						{
						}

						internal static void G3_BoxTest(GXBoxTestParam box)
						{
							MtxFx43 mtxFx = nitro_reuse_m43;
							mtxFx.a[0] = projectionMtx._00;
							mtxFx.a[1] = projectionMtx._01;
							mtxFx.a[2] = projectionMtx._03;
							mtxFx.a[3] = projectionMtx._10;
							mtxFx.a[4] = projectionMtx._11;
							mtxFx.a[5] = projectionMtx._13;
							mtxFx.a[6] = projectionMtx._20;
							mtxFx.a[7] = projectionMtx._21;
							mtxFx.a[8] = projectionMtx._23;
							mtxFx.a[9] = projectionMtx._30;
							mtxFx.a[10] = projectionMtx._31;
							mtxFx.a[11] = projectionMtx._33;
							MTX_Concat43(currentMtx, mtxFx, mtxFx);
							int num = 0;
							int num2 = 0;
							int num3 = 0;
							int num4 = 0;
							int num5 = 0;
							for (int i = 0; i < 8; i++)
							{
								VecFx32 vecFx = nitro_reuse_pos;
								vecFx.x = (((i & 1) != 0) ? box.x : (box.x + box.width));
								vecFx.y = (((i & 2) != 0) ? box.y : (box.y + box.height));
								vecFx.z = (((i & 4) != 0) ? box.z : (box.z + box.depth));
								MTX_MultVec43(vecFx, mtxFx, vecFx);
								if (vecFx.z > 0)
								{
									int num6 = FX_Div(vecFx.x, vecFx.z);
									int num7 = FX_Div(vecFx.y, vecFx.z);
									if (num5 == 0)
									{
										num = (num2 = num6);
										num3 = (num4 = num7);
									}
									else
									{
										num = MATH_MIN(num, num6);
										num2 = MATH_MAX(num2, num6);
										num3 = MATH_MIN(num3, num7);
										num4 = MATH_MAX(num4, num7);
									}
									num5++;
								}
							}
							if (num5 == 0)
							{
								boxTestResult = 0;
							}
							else if (num5 < 8)
							{
								boxTestResult = 1;
							}
							else
							{
								boxTestResult = ((num <= 4096 && num2 >= -4096 && num3 <= 4096 && num4 >= -4096) ? 1 : 0);
							}
						}

						internal static void G3_Ortho(int t, int b, int l, int r, int n, int f, MtxFx44 mtx)
						{
							G3_OrthoW(t, b, l, r, n, f, 4096, mtx);
						}

						internal static void G3_OrthoW(int t, int b, int l, int r, int n, int f, int scaleW, MtxFx44 mtx)
						{
							int num = (b + t) / 2;
							int num2 = (r - l) * LCD_HEIGHT / (LCD_WIDTH * 2);
							t = num - num2;
							b = num + num2;
							MtxFx44 mtxFx = nitro_reuse_m44;
							mtxFx._00 = FX_Div(8192, r - l);
							mtxFx._01 = 0;
							mtxFx._02 = 0;
							mtxFx._03 = 0;
							mtxFx._10 = 0;
							mtxFx._11 = FX_Div(8192, t - b);
							mtxFx._12 = 0;
							mtxFx._13 = 0;
							mtxFx._20 = 0;
							mtxFx._21 = 0;
							mtxFx._22 = -FX_Div(8192, f - n);
							mtxFx._23 = 0;
							mtxFx._30 = -FX_Div(r + l, r - l);
							mtxFx._31 = -FX_Div(t + b, t - b);
							mtxFx._32 = -FX_Div(f + n, f - n);
							mtxFx._33 = 4096;
							float[] array = nitro_reuse_f;
							array[0] = 8192f / (float)(r - l);
							array[1] = 0f;
							array[2] = 0f;
							array[3] = 0f;
							array[4] = 0f;
							array[5] = 8192f / (float)(t - b);
							array[6] = 0f;
							array[7] = 0f;
							array[8] = 0f;
							array[9] = 0f;
							array[10] = -8192f / (float)(f - n);
							array[11] = 0f;
							array[12] = (0f - (float)(r + l)) / (float)(r - l);
							array[13] = (0f - (float)(t + b)) / (float)(t - b);
							array[14] = (0f - (float)(f + n)) / (float)(f - n);
							array[15] = 1f;
							glMatrixMode(5889u);
							GX_FlipProjectionMatrix();
							glMultMatrixf(array);
							glMatrixMode(5888u);
							projectionMtx.copy(mtxFx);
							mtx?.copy(mtxFx);
						}

						internal static void G3_LookAt(VecFx32 camPos, VecFx32 camUp, VecFx32 target, MtxFx43 mtx)
						{
							VecFx32 vecFx = nitro_reuse_x;
							VecFx32 vecFx2 = nitro_reuse_y;
							VecFx32 vecFx3 = nitro_reuse_z;
							VEC_Subtract(camPos, target, vecFx3);
							VEC_Normalize(vecFx3, vecFx3);
							VEC_CrossProduct(camUp, vecFx3, vecFx);
							VEC_CrossProduct(vecFx3, vecFx, vecFx2);
							VEC_Normalize(vecFx, vecFx);
							VEC_Normalize(vecFx2, vecFx2);
							MtxFx43 mtxFx = nitro_reuse_m43;
							mtxFx.a[0] = vecFx.x;
							mtxFx.a[1] = vecFx2.x;
							mtxFx.a[2] = vecFx3.x;
							mtxFx.a[3] = vecFx.y;
							mtxFx.a[4] = vecFx2.y;
							mtxFx.a[5] = vecFx3.y;
							mtxFx.a[6] = vecFx.z;
							mtxFx.a[7] = vecFx2.z;
							mtxFx.a[8] = vecFx3.z;
							mtxFx.a[9] = 0;
							mtxFx.a[10] = 0;
							mtxFx.a[11] = 0;
							MTX_TransApply43(mtxFx, mtxFx, -camPos.x, -camPos.y, -camPos.z);
							currentMtx.copy(mtxFx);
							mtx?.copy(currentMtx);
						}

						internal static void G3X_InitMtxStack()
						{
							pStack = 0;
						}

						internal static void G3X_Reset()
						{
						}

						internal static void G3X_ResetMtxStack()
						{
							pStack = 0;
						}

						internal static void G3X_AlphaTest(int enable, int @ref)
						{
						}

						internal static void G3X_AlphaBlend(int enable)
						{
						}

						internal static void G3X_AntiAlias(int enable)
						{
						}

						internal static void G3X_SetShading(int shading)
						{
						}

						internal static void G3X_SetToonTable(ushort[] rgb_32)
						{
							toonTable = rgb_32[0];
						}

						internal static void G3X_SetHOffset(int hOffset)
						{
						}

						internal static void G3X_SetClearColor(ushort rgb, int alpha, int depth, int polygonID, int fog)
						{
						}

						internal static int G3X_GetBoxTestResult(out int @in)
						{
							@in = boxTestResult;
							return 0;
						}

						internal static void G3X_ClearFifo()
						{
						}

						internal static int G3X_GetPolygonListRamCount()
						{
							return 0;
						}

						internal static int G3X_GetVtxListRamCount()
						{
							return 0;
						}

						internal static void MI_CpuCopy8(Array p, Array p2, int size)
						{
							memcpy(p2, p, size);
						}

						internal static void MI_CpuCopy32(Array p, Array p2, int size)
						{
							memcpy(p2, p, size);
						}

						internal static void MI_CpuCopyFast(Array p, Array p2, int size)
						{
							memcpy(p2, p, size);
						}

						internal static void MI_CpuFill16(Array p, int n, int size)
						{
							ushort[] array = (ushort[])p;
							size /= 2;
							for (int i = 0; i < size; i++)
							{
								array[i] = (ushort)n;
							}
						}

						internal static void MI_CpuFillFast(Array p, int n, int size)
						{
							memset(p, n, size);
						}

						internal static void MI_CpuClear8(Array p2, int size)
						{
							memset(p2, 0, size);
						}

						internal static void MI_CpuClearFast(Array p2, int size)
						{
							memset(p2, 0, size);
						}

						internal static void MI_DmaCopy16(uint dmaNo, Array src, Array dest, uint size)
						{
						}

						internal static void MI_UncompressLZ8(Array srcp, Array destp)
						{
						}

						internal static void MI_UncompressLZ16(Array srcp, Array destp)
						{
						}

						internal static void MI_UncompressHuffman(Array srcp, Array destp)
						{
						}

						internal static void MI_UncompressRL8(Array srcp, Array destp)
						{
						}

						internal static void MI_UncompressRL16(Array srcp, Array destp)
						{
						}

						internal static void MI_UnfilterDiff8(Array srcp, Array destp)
						{
						}

						internal static void MI_UnfilterDiff16(Array srcp, Array destp)
						{
						}

						internal static MICompressionType MI_GetCompressionType(Array srcp)
						{
							return (MICompressionType)(((uint[])srcp)[0] & 0xF0);
						}

						internal static void MI_InitUncompContextRL(int context, byte[] dest, MICompressionHeader header)
						{
						}

						internal static void MI_InitUncompContextLZ(MIUncompContextLZ context, byte[] dest, MICompressionHeader header)
						{
							context.setDefault();
							context.destp = dest;
							context.destCount = (int)header.destSize_24;
						}

						internal static void MI_InitUncompContextHuffman(int context, byte[] dest, MICompressionHeader header)
						{
						}

						internal static int MI_ReadUncompRL8(int context, byte[] data, uint len)
						{
							return 0;
						}

						internal static int MI_ReadUncompRL16(int context, byte[] data, uint len)
						{
							return 0;
						}

						internal static int MI_ReadUncompLZ8(MIUncompContextLZ context, byte[] data, uint len)
						{
							int num = 0;
							while (num < len && context.destCount != 0)
							{
								if (context.flagIndex == 0)
								{
									context.flags = data[num++];
									context.flagIndex = 8;
									continue;
								}
								context.flagIndex--;
								if ((context.flags & (1 << (int)context.flagIndex)) == 0)
								{
									context.destp[context.m_iOffset++] = data[num++];
									context.destCount--;
									continue;
								}
								int num2;
								if (context.destTmpCnt != 0)
								{
									num2 = context.destTmp;
									context.destTmpCnt = 0;
								}
								else
								{
									if (num + 1 >= len)
									{
										context.destTmpCnt = 1;
										context.destTmp = data[num++];
										context.flagIndex++;
										break;
									}
									num2 = data[num++];
								}
								int num3 = ((num2 & 0xF) << 8) + data[num++] + 1;
								num2 = (num2 >> 4) + 3;
								while (num2-- > 0)
								{
									context.destp[context.m_iOffset] = context.destp[context.m_iOffset - num3];
									context.m_iOffset++;
									context.destCount--;
								}
							}
							return context.destCount;
						}

						internal static int MI_ReadUncompLZ16(MIUncompContextLZ context, byte[] data, uint len)
						{
							return 0;
						}

						internal static int MI_ReadUncompHuffman(int context, byte[] data, uint len)
						{
							return 0;
						}

						internal static void CP_SetDivImm64_32(ulong arg0, uint arg1)
						{
						}

						internal static uint CP_GetDivRemainder32()
						{
							return 0u;
						}

						internal static void MATH_InitRand16(MATHRandContext16 context, uint seed)
						{
							srand((uint)time(null));
						}

						internal static void MATH_InitRand32(MATHRandContext32 context, ulong seed)
						{
							srand((uint)time(null));
						}

						internal static ushort MATH_Rand16(MATHRandContext16 context, ushort max)
						{
							return (ushort)(rand() % max);
						}

						internal static uint MATH_Rand32(MATHRandContext32 context, uint max)
						{
							return (uint)(rand() % max);
						}

						internal static void MATH_CRC32InitTable(int table)
						{
						}

						internal static uint MATH_CalcCRC32(int table, Array data, uint dataLength)
						{
							return 0u;
						}

						internal static ushort PAD_Read()
						{
							return (ushort)contF;
						}

						internal static int PAD_DetectFold()
						{
							return 0;
						}

						internal static void TP_Init()
						{
							tp.setDefault();
							tp.state = TPState.TP_UP;
							tp.validity = 0;
						}

						internal static void TP_SetCalibrateParam(TPCalibrateParam param)
						{
						}

						internal static void TP_Update(int frame)
						{
							short x = tp.x;
							short y = tp.y;
							if (touchPeak >= 1)
							{
								tp.x = (short)(touchX[0] - (float)((LCD_WIDTH - 480) / 2));
								tp.y = (short)(touchY[0] - (float)((LCD_HEIGHT - 320) / 2));
							}
							tp.touch = ((touchPeak == 1) ? ((ushort)1) : ((ushort)0));
							switch (tp.state)
							{
							case TPState.TP_UP:
								if (touchPeak == 1)
								{
									tp.time = 0;
									tp.state = TPState.TP_DOWN;
									tp.dragX = tp.x;
									tp.dragY = tp.y;
									tp.drag = (tp.hold = (tp.tap = (tp.cancel = 0)));
									tp.flickSpeedH = 0;
								}
								else if (touchPeak >= 2)
								{
									tp.state = TPState.TP_PINCH;
									int num6 = (int)(touchX[1] - touchX[0]);
									int num7 = (int)(touchY[1] - touchY[0]);
									tp.prevPinch = (int)sqrt(num6 * num6 + num7 * num7);
									tp.drag = (tp.hold = (tp.tap = (tp.cancel = 0)));
								}
								break;
							case TPState.TP_DOWN:
								if (touchPeak == 0)
								{
									tp.state = TPState.TP_UP;
									if (tp.drag == 0 && tp.cancel == 0)
									{
										tp.tap = 1;
									}
									tp.drag = (tp.hold = 0);
									if (abs(tp.flickSpeedH) > 80 && abs(tp.x - tp.dragX) > abs(tp.y - tp.dragY))
									{
										tp.flickOffsetH = (short)((tp.flickSpeedH >= 0) ? 1 : (-1));
									}
								}
								else if (touchPeak == 1)
								{
									tp.time += frame;
									if (abs(tp.x - tp.dragX) > 2 || abs(tp.y - tp.dragY) > 2)
									{
										tp.drag = 1;
									}
									if (tp.time >= 30 && tp.drag == 0)
									{
										tp.hold = 1;
									}
									tp.flickSpeed = -(tp.y - y) * 4096 / frame;
									tp.flickSpeedH += -(tp.x - x);
								}
								else if (touchPeak >= 2)
								{
									tp.state = TPState.TP_PINCH;
									tp.hold = (tp.drag = 0);
									int num4 = (int)(touchX[1] - touchX[0]);
									int num5 = (int)(touchY[1] - touchY[0]);
									tp.prevPinch = (int)sqrt(num4 * num4 + num5 * num5);
								}
								break;
							case TPState.TP_PINCH:
							{
								if (touchPeak < 2)
								{
									tp.state = TPState.TP_UP;
									break;
								}
								int num = (int)(touchX[1] - touchX[0]);
								int num2 = (int)(touchY[1] - touchY[0]);
								int num3 = (int)sqrt(num * num + num2 * num2);
								tp.pinch = num3 - tp.prevPinch;
								tp.prevPinch = num3;
								tp.drag = 0;
								break;
							}
							}
						}

						internal static void TP_Update2()
						{
							tp.tap = 0;
							tp.pinch = 0;
							touchPeak = touchCount;
							int num = 163840;
							tp.flickPos += tp.flickSpeed;
							tp.flickOffset = (short)((tp.flickPos + ((tp.flickPos > 0) ? (num / 2) : (-num / 2 + 1))) / num);
							tp.flickPos -= tp.flickOffset * num;
							if (tp.drag == 0)
							{
								tp.flickSpeed = tp.flickSpeed * 30 / 32;
								if (abs(tp.flickSpeed) < 16384)
								{
									tp.flickSpeed = 0;
								}
								if (tp.flickSpeed == 0)
								{
									tp.flickPos = 0;
									tp.flickOffset = 0;
								}
							}
							tp.flickOffsetH = 0;
						}

						internal static uint TP_RequestRawSampling(TPData result)
						{
							result.copy(tp);
							return 0u;
						}

						internal static int TP_GetUserInfo(TPCalibrateParam calibrate)
						{
							return 0;
						}

						internal static void TP_GetCalibratedPoint(TPData disp, TPData raw)
						{
							disp.copy(raw);
						}

						internal static void TP_CancelTap()
						{
							tp.cancel = 1;
							tp.flickPos = 0;
							tp.flickOffset = 0;
							tp.flickSpeed = 0;
							tp.flickPosH = 0;
							tp.flickOffsetH = 0;
							tp.flickSpeedH = 0;
						}

						internal static void PM_Init()
						{
						}

						internal static void PM_SetSleepCallbackInfo(PMSleepCallbackInfo arg0, PreCallback arg1, Array arg2)
						{
						}

						internal static void PM_AppendPreSleepCallback(PMSleepCallbackInfo arg0)
						{
						}

						internal static void PM_AppendPostSleepCallback(PMSleepCallbackInfo arg0)
						{
						}

						internal static void PM_DeletePreSleepCallback(PMSleepCallbackInfo arg0)
						{
						}

						internal static void PM_DeletePostSleepCallback(PMSleepCallbackInfo arg0)
						{
						}

						internal static void PM_GoSleepMode(int arg0, int arg1, int arg2)
						{
						}

						internal static bool PM_SetLCDPower(int arg0)
						{
							return false;
						}

						internal static int PM_GetLCDPower()
						{
							return 0;
						}

						internal static void RTC_Init()
						{
						}

						internal static int RTC_GetDateTime(RTCDate date, RTCTime time)
						{
							date.setDefault();
							time.setDefault();
							date.absoluteTime = GlobalScope.time(null);
							return 0;
						}

						internal static long RTC_ConvertDateTimeToSecond(RTCDate date, RTCTime time)
						{
							return date.absoluteTime;
						}

						internal static void SND_SetChannelVolume(int arg0, int arg1, int arg2)
						{
						}

						internal static bool SND_FlushCommand(int arg0)
						{
							return false;
						}

						internal static void SND_LockChannel(ulong arg0, int arg1)
						{
						}

						internal static void SND_SetupChannelPcm(int arg0, int arg1, Array arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9)
						{
						}

						internal static void SND_SetupAlarm(int arg0, int arg1, int arg2, SNDAlarmHandler arg3, object arg4)
						{
						}

						internal static void SND_StartTimer(int arg0, int arg1, int arg2, int arg3)
						{
						}

						internal static void SND_StopTimer(int arg0, int arg1, int arg2, int arg3)
						{
						}

						internal static void SND_UnlockChannel(int arg0, int arg1)
						{
						}

						internal static void SND_AssignWaveArc(int arg0, int arg1, int arg2)
						{
						}

						internal static void FS_Init(int default_dma_no)
						{
						}

						internal static int FS_IsAvailable()
						{
							return 1;
						}

						internal static int FS_LoadTable(Array mem, uint size)
						{
							return 1;
						}

						internal static Array FS_UnloadTable()
						{
							return null;
						}

						internal static uint FS_GetTableSize()
						{
							return 0u;
						}

						internal static void FS_InitFile(FSFile file)
						{
							file.name = null;
							file.data = null;
							file.size = 0;
							file.pos = 0;
						}

						internal static int FS_IsFile(FSFile file)
						{
							if (file.data == null)
							{
								return 0;
							}
							return 1;
						}

						internal static int FS_WaitAsync(FSFile file)
						{
							return 1;
						}

						internal static int FS_CloseFile(FSFile file)
						{
							if (file.pos != 0)
							{
								file.data = null;
								file.name = null;
							}
							else
							{
								fileCache.data = null;
								fileCache.name = null;
								fileCache = file;
							}
							FS_InitFile(file);
							return 1;
						}

						internal static int FS_SeekFile(FSFile file, int offset, FSSeekFileMode origin)
						{
							file.pos = offset;
							return 1;
						}

						internal static int FS_ReadFile(FSFile file, Array buffer, int length)
						{
							int num = MATH_MIN(length, file.size - file.pos);
							memcpy(buffer, file.data, file.pos, num);
							file.pos += num;
							return num;
						}

						internal static int FS_ReadFileAsync(FSFile file, Array buffer, int length)
						{
							return FS_ReadFile(file, buffer, length);
						}

						internal static int FS_LoadOverlay(MIProcessor target, int id)
						{
							return 0;
						}

						internal static int FS_UnloadOverlay(MIProcessor target, int id)
						{
							return 0;
						}

						internal static int FS_ChangeDir(string path)
						{
							return 1;
						}

						internal static sbyte[] loadFile(string _filename, ref int size)
						{
							Array array = MainActivity.loadFile(_filename);
							if (array == null)
							{
								return null;
							}
							size = env.GetArrayLength(array);
							sbyte[] byteArrayElements = env.GetByteArrayElements(array, null);
							sbyte[] array2 = new sbyte[size];
							for (int i = 0; i < size; i++)
							{
								array2[i] = byteArrayElements[i];
							}
							env.ReleaseByteArrayElements(array, byteArrayElements, 0);
							return array2;
						}

						internal static int FS_OpenFile(FSFile file, string path)
						{
							string text = strrchr(path, 47);
							text = ((text != null) ? text.Substring(1) : path);
							if (fileCache.name != null && strcmp(text, fileCache.name) == 0)
							{
								file = fileCache;
								FS_InitFile(fileCache);
								return 1;
							}
							file.pos = 0;
							file.size = 0;
							file.data = loadFile(text, ref file.size);
							if (file.data == null)
							{
								return 0;
							}
							file.name = "";
							strcpy(out file.name, text);
							return 1;
						}

						internal static uint FS_GetLength(FSFile file)
						{
							return (uint)file.size;
						}

						internal static uint FS_GetPosition(FSFile file)
						{
							return (uint)file.pos;
						}

						internal static void SVC_WaitVBlankIntr()
						{
						}

						internal static int CARD_IsAvailable()
						{
							return 1;
						}

						internal static CARDResult CARD_GetResultCode()
						{
							return CARDResult.CARD_RESULT_SUCCESS;
						}

						internal static void CARD_LockBackup(ushort lock_id)
						{
						}

						internal static void CARD_UnlockBackup(ushort lock_id)
						{
						}

						internal static void CARD_IdentifyBackup(CARDBackupType type)
						{
						}

						internal static uint CARD_GetBackupTotalSize()
						{
							return 0u;
						}

						internal static uint CARD_GetBackupPageSize()
						{
							return 0u;
						}

						internal static int CARD_TryWaitBackupAsync()
						{
							return 1;
						}

						internal static int CARD_IsBackupEeprom()
						{
							return 1;
						}

						internal static int CARD_ReadEeprom(uint src, Array dst, uint len)
						{
							memset(dst, 0, (int)len);
							FILE fILE = fopen("/data/data/com.square_enix.FFIII_J/files/save.bin", "rb");
							if (fILE == null)
							{
								return 1;
							}
							fseek(fILE, src, 0);
							fread(dst, 1, (int)len, fILE);
							fclose(fILE);
							return 1;
						}

						internal static void CARD_ReadEepromAsync(uint src, Array dst, uint len, MIDmaCallback callback, Array arg)
						{
							CARD_ReadEeprom(src, dst, len);
						}

						internal static int CARD_WriteAndVerifyEeprom(uint dst, Array src, uint len)
						{
							int num = 0;
							FILE fILE = fopen("/data/data/com.square_enix.FFIII_J/files/save.bin", "rb");
							if (fILE != null)
							{
								fseek(fILE, 0L, 2);
								num = (int)ftell(fILE);
								fclose(fILE);
							}
							if (num != 65536)
							{
								MainActivity.createSaveFile(65536);
							}
							fILE = fopen("/data/data/com.square_enix.FFIII_J/files/save.bin", "r+b");
							if (fILE == null)
							{
								return 0;
							}
							fseek(fILE, dst, 0);
							fwrite(src, 1, (int)len, fILE);
							fclose(fILE);
							return 1;
						}

						internal static void CARD_WriteAndVerifyEepromAsync(uint dst, Array src, uint len, MIDmaCallback callback, Array arg)
						{
							CARD_WriteAndVerifyEeprom(dst, src, len);
						}

						internal static int CARD_IsBackupFlash()
						{
							return 0;
						}

						internal static int CARD_ReadFlash(uint src, Array dst, uint len)
						{
							return 0;
						}

						internal static void CARD_ReadFlashAsync(uint src, Array dst, uint len, MIDmaCallback callback, Array arg)
						{
						}

						internal static int CARD_WriteAndVerifyFlash(uint dst, Array src, uint len)
						{
							return 0;
						}

						internal static void CARD_WriteAndVerifyFlashAsync(uint dst, Array src, uint len, MIDmaCallback callback, Array arg)
						{
						}

						internal static void CARD_SetPulledOutCallback(CARDPulledOutCallback callback)
						{
						}

						internal static void CARD_TerminateForPulledOut()
						{
						}

						internal static bool CTRDG_IsAgbCartridge()
						{
							return false;
						}

						internal static ushort CTRDG_GetAgbMakerCode()
						{
							return 0;
						}

						internal static uint CTRDG_GetAgbGameCode()
						{
							return 0u;
						}

						internal static uint VX_GetNbFrame(int arg0)
						{
							return 0u;
						}

						internal static uint VX_GetNbIFrame(int arg0)
						{
							return 0u;
						}

						internal static uint VX_GetCurrentFrameNumber(int arg0)
						{
							return 0u;
						}

						internal static uint VX_GetVideoFps(int arg0)
						{
							return 1u;
						}

						internal static uint VX_GetVideoWidth(int arg0)
						{
							return 0u;
						}

						internal static uint VX_GetVideoHeight(int arg0)
						{
							return 0u;
						}

						internal static uint VX_GetAudioFrequency(int arg0)
						{
							return 0u;
						}

						internal static uint VX_GetNbAudioTrack(int arg0)
						{
							return 0u;
						}

						internal static uint VX_ReadFrame(int arg0)
						{
							return 0u;
						}

						internal static ulong VX_GetFrameNbAudioPacket(int arg0)
						{
							return 0uL;
						}

						internal static void VX_CloseMovie(int arg0)
						{
						}

						internal static void VX_UnpackFrameImage(int arg0)
						{
						}

						internal static void VX_BlitFrameSoundOnePacket(int arg0, Array arg1)
						{
						}

						internal static void VX_BlitFrameImage(int arg0, Array arg1, int arg2)
						{
						}

						internal static void VX_SkipFrameImage(int arg0)
						{
						}

						internal static void VX_JumpBeginning(int arg0)
						{
						}

						internal static int VX_OpenMovieFromFile(FSFile arg0, int arg1, int arg2)
						{
							return 0;
						}

						internal static void SetDataSize(int size)
						{
						}

						internal static void PXI_Init()
						{
						}

						internal static int FX_Mul(int v1, int v2)
						{
							return (int)((long)v1 * (long)v2 + 2048 >> 12);
						}

						internal static int getImageSize(int size)
						{
							int num;
							for (num = 8; num < size; num *= 2)
							{
							}
							return num;
						}

						internal static uint GenTexture(int w, int h, byte[] pSrcData, uint wrap, uint filter, uint format)
						{
							FF3.Log.First(FF3.LogChannel.Texture, "GenTexture", 200, () => $"{w}x{h} bytes={(pSrcData == null ? -1 : pSrcData.Length)} wrap={wrap} filter={filter} fmt={format}"); /*FF3LOG*/
							for (int i = 1; i < LENGTH(texBank); i++)
							{
								if (texBank[i].data == null)
								{
									texBank[i].w = w;
									texBank[i].h = h;
									texBank[i].data = pSrcData;
									texBank[i].tex[0] = 0u;
									texBank[i].wrap = wrap;
									texBank[i].filter = filter;
									texBank[i].format = format;
									return (uint)i;
								}
							}
							return 0u;
						}

						internal static void BindTexture(uint target, uint tex)
						{
							FF3.Log.First(FF3.LogChannel.Texture, "BindTexture", 200, () => $"tex={tex} data={(tex != 0 && tex < texBank.Length && texBank[tex] != null && texBank[tex].data != null ? texBank[tex].w + "x" + texBank[tex].h : "NONE")}"); /*FF3LOG*/
							if (tex == 0 || texBank[tex].data == null)
							{
								glBindTexture(target, 0u);
								return;
							}
							if (texBank[tex].tex[0] == 0)
							{
								glGenTextures(1, texBank[tex].tex);
								glBindTexture(3553u, texBank[tex].tex[0]);
								glTexParameteri(3553u, 10241u, (int)texBank[tex].filter);
								glTexParameteri(3553u, 10240u, (int)texBank[tex].filter);
								glTexParameteri(3553u, 10242u, (int)texBank[tex].wrap);
								glTexParameteri(3553u, 10243u, (int)texBank[tex].wrap);
								glTexImage2D(3553u, 0, 6408, texBank[tex].w, texBank[tex].h, 0, 6408u, texBank[tex].format, texBank[tex].data);
							}
							glBindTexture(target, texBank[tex].tex[0]);
						}

						internal static void DeleteTexture(uint tex)
						{
							if (tex != 0)
							{
								glDeleteTextures(1, texBank[tex].tex);
								texBank[tex].data = null;
								texBank[tex].setDefault();
							}
						}

						internal static void SuspendTexture()
						{
							for (int i = 1; i < LENGTH(texBank); i++)
							{
								if (texBank[i].tex[0] != 0)
								{
									glDeleteTextures(1, texBank[i].tex);
									texBank[i].tex[0] = 0u;
								}
							}
						}

						internal static void NNS_FndDumpHeap(int heap)
						{
						}

						internal static int NNS_FndCreateExpHeap(Array startAddress, uint size)
						{
							return 0;
						}

						internal static void NNS_FndDestroyExpHeap(int heap)
						{
						}

						internal static Array NNS_FndAllocFromExpHeapEx(int heap, uint size, int alignment)
						{
							return malloc_count((int)size);
						}

						internal static object NNS_FndAllocFromExpHeapEx(int heap, Type size, int alignment)
						{
							return malloc_count(size);
						}

						internal static uint NNS_FndResizeForMBlockExpHeap(int heap, Array memBlock, uint size)
						{
							return size;
						}

						internal static void NNS_FndFreeToExpHeap(int heap, Array memBlock)
						{
							free_count(memBlock);
						}

						internal static void NNS_FndFreeToExpHeap(int heap, object memBlock)
						{
							free_count(memBlock);
						}

						internal static ushort NNS_FndSetAllocModeForExpHeap(int heap, ushort mode)
						{
							return 0;
						}

						internal static void NNS_FndSetGroupIDForExpHeap(int heap, ushort groupID)
						{
						}

						internal static ushort NNS_FndGetGroupIDForExpHeap(int heap)
						{
							return 0;
						}

						internal static void NNS_FndVisitAllocatedForExpHeap(int heap, NNSFndHeapVisitor visitor, uint userParam)
						{
						}

						internal static uint NNS_FndGetTotalFreeSizeForExpHeap(int heap)
						{
							return 0u;
						}

						internal static uint NNS_FndGetAllocatableSizeForExpHeap(int heap)
						{
							return 0u;
						}

						internal static uint NNS_FndGetSizeForMBlockExpHeap(Array memBlock)
						{
							return 0u;
						}

						internal static ushort NNS_FndGetGroupIDForMBlockExpHeap(Array memBlock)
						{
							return 0;
						}

						internal static void NNS_FndInitAllocatorForExpHeap(NNSFndAllocator pAllocator, int heap, int alignment)
						{
						}

						internal static TexVram NNS_GfdGetTexKeyAddr(TexVram memKey)
						{
							return memKey;
						}

						internal static uint NNS_GfdGetTexKeySize(TexVram memKey)
						{
							return 0u;
						}

						internal static int NNS_GfdGetTexKey4x4Flag(TexVram memKey)
						{
							return 0;
						}

						internal static void NNS_GfdInitFrmTexVramManager(ushort numSlot, int useAsDefault)
						{
						}

						internal static void NNS_GfdResetFrmTexVramState()
						{
						}

						internal static void NNS_GfdInitLnkTexVramManager(uint szByte, uint szByteFor4x4, Array pManagementWork, uint szByteManagementWork, int useAsDefault)
						{
						}

						internal static TexVram NNS_GfdAllocLnkTexVram(uint szByte, int is4x4comp, uint opt)
						{
							TexVram texVram = new TexVram();
							texVram.list = null;
							texVram.size = 0;
							texVram.b4x4 = is4x4comp;
							return texVram;
						}

						internal static int NNS_GfdFreeLnkTexVram(TexVram memKey)
						{
							if (memKey.list != null)
							{
								for (int i = 0; i < memKey.size; i++)
								{
									memKey.list[i].release();
								}
								memKey.list = null;
							}
							memKey.destruct();
							return 0;
						}

						internal static int NNS_GfdGetLnkTexVramManagerWorkSize(uint numMemBlk)
						{
							return 0;
						}

						internal static void NNS_GfdResetLnkTexVramState()
						{
						}

						internal static void NNS_GfdDumpLnkTexVramManager()
						{
						}

						internal static uint NNS_GfdGetPlttKeyAddr(TexVram plttKey)
						{
							return 0u;
						}

						internal static uint NNS_GfdGetPlttKeySize(TexVram plttKey)
						{
							return 0u;
						}

						internal static void NNS_GfdInitFrmPlttVramManager(uint szByte, int useAsDefault)
						{
						}

						internal static void NNS_GfdInitLnkPlttVramManager(uint szByte, Array pManagementWork, uint szByteManagementWork, int useAsDefault)
						{
						}

						internal static TexVram NNS_GfdAllocLnkPlttVram(uint szByte, int b4Pltt, uint opt)
						{
							return new TexVram();
						}

						internal static int NNS_GfdFreeLnkPlttVram(TexVram plttKey)
						{
							return 0;
						}

						internal static uint NNS_GfdGetLnkPlttVramManagerWorkSize(uint numMemBlk)
						{
							return 0u;
						}

						internal static void NNS_GfdResetLnkPlttVramState()
						{
						}

						internal static void NNS_GfdDumpLnkPlttVramManager()
						{
						}

						internal static NNSG2dCellData NNS_G2dGetCellDataByIdx(NNSG2dCellDataBank pCellData, ushort idx)
						{
							if (pCellData != null)
							{
								return pCellData.pCellDataArrayHead[idx];
							}
							return null;
						}

						internal static NNSG2dAnimSequenceData NNS_G2dGetAnimSequenceByIdx(NNSG2dAnimBankData pAnimBank, ushort idx)
						{
							if (pAnimBank != null)
							{
								return pAnimBank.pSequenceArrayHead[idx];
							}
							return null;
						}

						internal static NNSG2dCellBoundingRectS16 NNS_G2dGetCellBoundingRect(NNSG2dCellData pCell)
						{
							return null;
						}

						internal static int NNS_G2dGetUnpackedBank(Array pFile, string type, Array ppBank)
						{
							byte[] array = (byte[])pFile;
							byte[] bytes = StringUtil.getBytes(type);
							int num = 0;
							int num2 = num + ArrayReader.packInt32(array, num + 8);
							for (num += 16; num < num2; num += ArrayReader.packInt32(array, num + 4))
							{
								if (array[num] == bytes[3] && array[num + 1] == bytes[2] && array[num + 2] == bytes[1] && array[num + 3] == bytes[0])
								{
									Buffer.BlockCopy(array, num + 8, ppBank, 0, array.Length - (num + 8));
									return 1;
								}
							}
							return 0;
						}

						internal static int NNS_G2dGetUnpackedScreenData(Array pNscrFile, NNSG2dScreenData ppScrData)
						{
							byte[] array = new byte[pNscrFile.Length];
							int num = NNS_G2dGetUnpackedBank(pNscrFile, "SCRN", array);
							if (num != 0)
							{
								ppScrData.parse(array);
							}
							return num;
						}

						internal static int NNS_G2dGetUnpackedCellBank(Array pNcerFile, NNSG2dCellDataBank ppCellBank)
						{
							byte[] array = new byte[pNcerFile.Length];
							if (NNS_G2dGetUnpackedBank(pNcerFile, "CEBK", array) == 0)
							{
								return 0;
							}
							ppCellBank.parse(array);
							return 1;
						}

						internal static int NNS_G2dGetUnpackedAnimBank(Array pNanrFile, NNSG2dAnimBankData ppAnimBank)
						{
							byte[] array = new byte[pNanrFile.Length];
							if (NNS_G2dGetUnpackedBank(pNanrFile, "ABNK", array) == 0)
							{
								return 0;
							}
							ppAnimBank.parse(array);
							return 1;
						}

						internal static int NNS_G2dGetUnpackedCharacterData(Array pNcgrFile, NNSG2dCharacterData ppCharData)
						{
							if (memcmp(pNcgrFile, "\u0089PNG", 4) == 0)
							{
								ppCharData.m_aPng = pNcgrFile;
								ppCharData.parse(pNcgrFile);
								return 1;
							}
							byte[] array = new byte[pNcgrFile.Length];
							int num = NNS_G2dGetUnpackedBank(pNcgrFile, "CHAR", array);
							if (num != 0)
							{
								ppCharData.parse(array);
							}
							return num;
						}

						internal static int NNS_G2dGetUnpackedBGCharacterData(Array pNcgrFile, NNSG2dCharacterData ppCharData)
						{
							if (memcmp(pNcgrFile, "\u0089PNG", 4) == 0)
							{
								ppCharData.m_aPng = pNcgrFile;
								ppCharData.parse(pNcgrFile);
								return 1;
							}
							byte[] array = new byte[pNcgrFile.Length];
							int num = NNS_G2dGetUnpackedBank(pNcgrFile, "CHAR", array);
							if (num != 0)
							{
								ppCharData.parse(array);
							}
							return num;
						}

						internal static int NNS_G2dGetUnpackedPaletteData(Array pNclrFile, NNSG2dPaletteData[] ppPltData)
						{
							return NNS_G2dGetUnpackedBank(pNclrFile, "PLTT", ppPltData);
						}

						internal static void NNS_G2dStartAnimCtrl(NNSG2dAnimController pAnimCtrl)
						{
						}

						internal static void NNS_G2dInitCellAnimation(NNSG2dCellAnimation pCellAnim, NNSG2dAnimSequenceData pAnimSeq, NNSG2dCellDataBank pCellDataBank)
						{
							pCellAnim.setDefault();
							pCellAnim.pCellDataBank = pCellDataBank;
							NNS_G2dSetCellAnimationSequence(pCellAnim, pAnimSeq);
						}

						internal static void NNS_G2dSetCellAnimationSequence(NNSG2dCellAnimation pCellAnim, NNSG2dAnimSequenceData pAnimSeq)
						{
							pCellAnim.animCtrl.pAnimSequence = pAnimSeq;
							pCellAnim.animCtrl.pCurrent_idx = 0;
							pCellAnim.animCtrl.pCurrent = pAnimSeq.pAnmFrameArray[pCellAnim.animCtrl.pCurrent_idx];
							pCellAnim.animCtrl.currentTime = 0;
							pCellAnim.animCtrl.bActive = 1;
							NNS_G2dTickCellAnimation(pCellAnim, 0);
						}

						internal static NNSG2dAnimController NNS_G2dGetCellAnimationAnimCtrl(NNSG2dCellAnimation pCellAnim)
						{
							return pCellAnim.animCtrl;
						}

						internal static void NNS_G2dTickCellAnimation(NNSG2dCellAnimation pCellAnim, int frames)
						{
							if (pCellAnim.animCtrl.pCurrent == null)
							{
								return;
							}
							pCellAnim.animCtrl.currentTime += frames;
							while (pCellAnim.animCtrl.currentTime > pCellAnim.animCtrl.pCurrent.frames << 12)
							{
								pCellAnim.animCtrl.currentTime -= pCellAnim.animCtrl.pCurrent.frames << 12;
								pCellAnim.animCtrl.pCurrent_idx++;
								if (pCellAnim.animCtrl.pCurrent_idx == pCellAnim.animCtrl.pAnimSequence.numFrames)
								{
									pCellAnim.animCtrl.pCurrent_idx = pCellAnim.animCtrl.pAnimSequence.loopStartFrameIdx;
									pCellAnim.animCtrl.pCurrent = pCellAnim.animCtrl.pAnimSequence.pAnmFrameArray[pCellAnim.animCtrl.pAnimSequence.loopStartFrameIdx];
									pCellAnim.animCtrl.bActive = 0;
								}
								else
								{
									pCellAnim.animCtrl.pCurrent = pCellAnim.animCtrl.pAnimSequence.pAnmFrameArray[pCellAnim.animCtrl.pCurrent_idx];
								}
							}
							pCellAnim.pCurrentCell = NNS_G2dGetCellDataByIdx(pCellAnim.pCellDataBank, ArrayReader.packUInt16((byte[])pCellAnim.animCtrl.pCurrent.pContent, 0));
						}

						internal static void NNS_G2dApplyOamManagerToHW(int pMan)
						{
						}

						internal static ushort NNS_G2dEntryOamManagerAffine(int pMan, MtxFx22 mtx)
						{
							return 0;
						}

						internal static int NNS_G2dEntryOamManagerOamWithAffineIdx(int pMan, GXOamAttr pOam, ushort affineIdx)
						{
							return 0;
						}

						internal static int NNS_G2dGetNewOamManagerInstanceAsFastTransferMode(int pMan, ushort fromOBJ, ushort numOBJ, NNSG2dOamType type)
						{
							return 1;
						}

						internal static void NNS_G2dInitOamManagerModule()
						{
							for (int i = 0; i < LENGTH(imageTable); i++)
							{
								IMAGE_TABLE iMAGE_TABLE = imageTable[i];
								if (iMAGE_TABLE.tex != 0)
								{
									DeleteTexture(iMAGE_TABLE.tex);
									texCount--;
								}
							}
							for (int j = 0; j < LENGTH(imageTable); j++)
							{
								imageTable[j].setDefault();
							}
							NNS_G2dBGClear();
							NNS_G2dCharCanvasClear(null, 0);
							G2_BlendNone();
							G2S_BlendNone();
							GX_Power3D(1);
							GX_SetPriority3D(0);
						}

						internal static void NNS_G2dResetOamManagerBuffer(int pMan)
						{
						}

						internal static void NNS_G2dSetupSoftwareSpriteCamera()
						{
						}

						internal static void NNS_G2dInitImageProxy(NNSG2dImageProxy pImg)
						{
							pImg.setDefault();
						}

						internal static void NNS_G2dInitImagePaletteProxy(NNSG2dImagePaletteProxy pImg)
						{
							pImg.setDefault();
						}

						internal static uint LoadPNG(byte[] pSrcData, out int sizeW, out int sizeH)
						{
							FF3.Log.First(FF3.LogChannel.Texture, "LoadPNG", 200, () => $"src={(pSrcData == null ? -1 : pSrcData.Length)} bytes"); /*FF3LOG*/
							int num = 8;
							byte[] arg;
							do
							{
								arg = new byte[4]
								{
									pSrcData[num + 4],
									pSrcData[num + 4 + 1],
									pSrcData[num + 4 + 2],
									pSrcData[num + 4 + 3]
								};
								num += 12 + ((pSrcData[num] << 24) | (pSrcData[num + 1] << 16) | (pSrcData[num + 2] << 8) | pSrcData[num + 3]);
							}
							while (memcmp(arg, "IEND", 4) != 0);
							Array array = MainActivity.loadTexture(pSrcData);
							int[] intArrayElements = env.GetIntArrayElements(array, null);
							int num2 = intArrayElements[0];
							int num3 = intArrayElements[1];
							sizeW = getImageSize(num2);
							sizeH = getImageSize(num3);
							byte[] array2 = new byte[sizeW * sizeH * 4];
							for (int i = 0; i < num3; i++)
							{
								int num4 = i * sizeW * 4;
								for (int j = 0; j < num2; j++)
								{
									num = intArrayElements[j + i * num2 + 2];
									array2[num4++] = (byte)num;
									array2[num4++] = (byte)(num >> 8);
									array2[num4++] = (byte)(num >> 16);
									array2[num4++] = (byte)(num >> 24);
								}
							}
							env.ReleaseIntArrayElements(array, intArrayElements, 0);
							uint num5 = GenTexture(sizeW, sizeH, array2, 33071u, 9729u, 5121u);
							if (num5 != 0)
							{
								texCount++;
							}
							return num5;
						}

						internal static void NNS_G2dReleaseImageProxy(NNSG2dImageProxy pImgProxy)
						{
							IMAGE_TABLE data = pImgProxy.data;
							if (data != null)
							{
								if (data.tex != 0)
								{
									DeleteTexture(data.tex);
									texCount--;
								}
								data.setDefault();
								pImgProxy.setDefault();
							}
						}

						internal static void NNS_G2dLoadImage1DMapping(NNSG2dCharacterData pSrcData, TexVram baseAddr, NNS_G2D_VRAM_TYPE type, NNSG2dImageProxy pImgProxy)
						{
							if (memcmp(pSrcData.m_aPng, "\u0089PNG", 4) != 0)
							{
								return;
							}
							pImgProxy.characterFmt = 2u;
							pImgProxy.attr.fmt = GXTexFmt.GX_TEXFMT_NONE;
							pImgProxy.attr.bExtendedPlt = 0;
							pImgProxy.attr.plttUse = 0;
							pImgProxy.attr.mappingType = 0;
							pImgProxy.data = null;
							uint num = LoadPNG((byte[])pSrcData.m_aPng, out var sizeW, out var sizeH);
							if (num != 0)
							{
								int i;
								for (i = 0; i < LENGTH(imageTable) && imageTable[i].tex != 0; i++)
								{
								}
								IMAGE_TABLE iMAGE_TABLE = (pImgProxy.data = imageTable[i]);
								pImgProxy.W = (ushort)sizeW;
								pImgProxy.H = (ushort)sizeH;
								iMAGE_TABLE.tex = num;
								iMAGE_TABLE.texScaleU = 1f / (float)sizeW;
								iMAGE_TABLE.texScaleV = 1f / (float)sizeH;
							}
						}

						internal static void NNS_G2dLoadImage2DMapping(NNSG2dCharacterData pSrcData, TexVram baseAddr, NNS_G2D_VRAM_TYPE type, NNSG2dImageProxy pImgProxy)
						{
						}

						internal static void NNS_G2dLoadPalette(NNSG2dPaletteData pSrcData, uint addr, NNS_G2D_VRAM_TYPE type, NNSG2dImagePaletteProxy pPltProxy)
						{
						}

						internal static void NNS_G2dSetImageExtPaletteFlag(NNSG2dImageProxy pImgProxy, int bUseExtPlt)
						{
							pImgProxy.attr.bExtendedPlt = bUseExtPlt;
						}

						internal static void NNS_G2dInitRenderer(NNSG2dRendererInstance pRend)
						{
						}

						internal static void NNS_G2dAddRendererTargetSurface(NNSG2dRendererInstance pRend, NNSG2dRenderSurface pNew)
						{
						}

						internal static void NNS_G2dBeginRenderingEx(NNSG2dRendererInstance pRendererInstance, uint opzHint)
						{
							renderer = pRendererInstance;
							MTX_Identity43(currentMtx);
						}

						internal static void NNS_G2dEndRendering()
						{
							renderer = null;
						}

						internal static void NNS_G2dResetMatrix(bool depthtest)
						{
							glMatrixMode(5889u);
							glLoadIdentity();
							GX_FlipProjectionMatrixAdjust();
							glOrthof((480 - LCD_WIDTH) / 2, (480 + LCD_WIDTH) / 2, (320 + LCD_HEIGHT) / 2, (320 - LCD_HEIGHT) / 2, -1024f, 1024f);
							glMatrixMode(5888u);
							glLoadIdentity();
							if (depthtest)
							{
								glTranslatef(0f, 0f, 1000f);
								glEnable(2929u);
							}
							else
							{
								glDisable(2929u);
							}
							glCullFace(1029u);
						}

						internal static void drawImage(float[] pos, int pos_offset, float[] tex, int tex_offset, float x, float y, float w, float h, int sx, int sy, int sw, int sh)
						{
							float[] array = fnd_reuse_asx;
							float[] array2 = fnd_reuse_asy;
							float[] array3 = fnd_reuse_ax;
							float[] array4 = fnd_reuse_ay;
							array[0] = sx;
							array[3] = sx + sw;
							if (sw > 0)
							{
								array[1] = array[0] + 0.5f;
								array[2] = array[3] - 0.5f;
							}
							else if (sw < 0)
							{
								array[1] = array[0] - 0.5f;
								array[2] = array[3] + 0.5f;
							}
							else
							{
								array[1] = (array[2] = array[0]);
							}
							array2[0] = sy;
							array2[3] = sy + sh;
							if (sh > 0)
							{
								array2[1] = array2[0] + 0.5f;
								array2[2] = array2[3] - 0.5f;
							}
							else if (sh < 0)
							{
								array2[1] = array2[0] - 0.5f;
								array2[2] = array2[3] + 0.5f;
							}
							else
							{
								array2[1] = (array2[2] = array2[0]);
							}
							array3[0] = x;
							array3[3] = x + w;
							if (sw != 0)
							{
								array3[1] = array3[0] + (array[1] - array[0]) * w / (float)sw;
								array3[2] = array3[0] + (array[2] - array[0]) * w / (float)sw;
							}
							else
							{
								array3[1] = array3[0];
								array3[2] = array3[3];
							}
							array4[0] = y;
							array4[3] = y + h;
							if (sh != 0)
							{
								array4[1] = array4[0] + (array2[1] - array2[0]) * h / (float)sh;
								array4[2] = array4[0] + (array2[2] - array2[0]) * h / (float)sh;
							}
							else
							{
								array4[1] = array4[0];
								array4[2] = array4[3];
							}
							array[0] = (array[1] *= texScaleU);
							array[2] = (array[3] = array[2] * texScaleU);
							array2[0] = (array2[1] *= texScaleV);
							array2[2] = (array2[3] = array2[2] * texScaleV);
							int[] array5 = fnd_reuse_index;
							for (int i = 0; i < array5.Length; i += 2)
							{
								pos[i + pos_offset] = array3[array5[i]];
								pos[i + pos_offset + 1] = array4[array5[i + 1]];
								tex[i + tex_offset] = array[array5[i]];
								tex[i + tex_offset + 1] = array2[array5[i + 1]];
							}
						}

						internal static void drawImage(VertexPositionColorTexture[] ptc, int ptc_offset, float x, float y, float w, float h, int sx, int sy, int sw, int sh, byte[] color)
						{
							float[] array = fnd_reuse_asx;
							float[] array2 = fnd_reuse_asy;
							float[] array3 = fnd_reuse_ax;
							float[] array4 = fnd_reuse_ay;
							uint packedValue = (uint)((color[0] & 0xFF) | ((color[1] & 0xFF) << 8) | ((color[2] & 0xFF) << 16) | ((color[3] & 0xFF) << 24));
							array[0] = sx;
							array[3] = sx + sw;
							if (sw > 0)
							{
								array[1] = array[0] + 0.5f;
								array[2] = array[3] - 0.5f;
							}
							else if (sw < 0)
							{
								array[1] = array[0] - 0.5f;
								array[2] = array[3] + 0.5f;
							}
							else
							{
								array[1] = (array[2] = array[0]);
							}
							array2[0] = sy;
							array2[3] = sy + sh;
							if (sh > 0)
							{
								array2[1] = array2[0] + 0.5f;
								array2[2] = array2[3] - 0.5f;
							}
							else if (sh < 0)
							{
								array2[1] = array2[0] - 0.5f;
								array2[2] = array2[3] + 0.5f;
							}
							else
							{
								array2[1] = (array2[2] = array2[0]);
							}
							array3[0] = x;
							array3[3] = x + w;
							if (sw != 0)
							{
								array3[1] = array3[0] + (array[1] - array[0]) * w / (float)sw;
								array3[2] = array3[0] + (array[2] - array[0]) * w / (float)sw;
							}
							else
							{
								array3[1] = array3[0];
								array3[2] = array3[3];
							}
							array4[0] = y;
							array4[3] = y + h;
							if (sh != 0)
							{
								array4[1] = array4[0] + (array2[1] - array2[0]) * h / (float)sh;
								array4[2] = array4[0] + (array2[2] - array2[0]) * h / (float)sh;
							}
							else
							{
								array4[1] = array4[0];
								array4[2] = array4[3];
							}
							array[0] = (array[1] *= texScaleU);
							array[2] = (array[3] = array[2] * texScaleU);
							array2[0] = (array2[1] *= texScaleV);
							array2[2] = (array2[3] = array2[2] * texScaleV);
							int[] array5 = fnd_reuse_index2;
							int num = 0;
							int num2 = ptc_offset;
							while (num < array5.Length)
							{
								ptc[num2].Position.X = array3[array5[num]];
								ptc[num2].Position.Y = array4[array5[num + 1]];
								ptc[num2].Position.Z = 0f;
								ptc[num2].TextureCoordinate.X = array[array5[num]];
								ptc[num2].TextureCoordinate.Y = array2[array5[num + 1]];
								ptc[num2].Color.PackedValue = packedValue;
								num += 2;
								num2++;
							}
						}

						internal static void NNS_G2dDrawCell(NNSG2dCellData pCell)
						{
							if (skipFrame != 0)
							{
								return;
							}
							IMAGE_TABLE data = renderer.rendererCore.pImgProxy.data;
							if (data != null && data.tex != 0 && pCell != null && renderer.rendererCore.pImgProxy.characterFmt == 2)
							{
								texScaleU = data.texScaleU;
								texScaleV = data.texScaleV;
								glPushMatrix();
								float[] array = fnd_reuse_f;
								MTX_Copy43ToGLfloat(currentMtx, array);
								glMultMatrixf(array);
								for (int i = 0; i < pCell.numOAMAttrs; i++)
								{
									short[] array2 = fnd_reuse_oam;
									pCell.pOamAttrArray[i].copy(array2, 14);
									short num = array2[0];
									short num2 = array2[1];
									short num3 = array2[2];
									short num4 = array2[3];
									short num5 = array2[4];
									short num6 = array2[5];
									short num7 = array2[6];
									float num8 = (((num7 & 4) != 0) ? 0.5f : 1f);
									float num9 = (((num7 & 8) != 0) ? 0.6f : 1f);
									float num10 = (((num7 & 8) != 0) ? (2f / 3f) : 1f);
									drawImage(vtc, i * 6, (float)num * num9 - (float)screenOffset[0], (float)num2 * num10 - (float)screenOffset[1], (float)num3 * num9 * num8, (float)num4 * num10 * num8, ((num7 & 1) == 0) ? num5 : (num5 + num3), ((num7 & 2) == 0) ? num6 : (num6 + num4), ((num7 & 1) == 0) ? num3 : (-num3), ((num7 & 2) == 0) ? num4 : (-num4), renderer.color);
								}
								glEnable(3553u);
								BindTexture(3553u, data.tex);
								glDrawArrays(4u, 0, pCell.numOAMAttrs * 6, vtc);
								polyCount += pCell.numOAMAttrs * 6;
								glDisableClientState(32888u);
								glDisable(3553u);
								glPopMatrix();
							}
						}

						internal static void NNS_G2dDrawCellAnimation(NNSG2dCellAnimation pCellAnim)
						{
							if (pCellAnim.pCurrentCell != null)
							{
								NNS_G2dPushMtx();
								byte[] abyData = (byte[])pCellAnim.animCtrl.pCurrent.pContent;
								switch (pCellAnim.animCtrl.pAnimSequence.animType & 0xFF)
								{
								case 1u:
									NNS_G2dTranslate(ArrayReader.packInt16(abyData, 12) << 12, ArrayReader.packInt16(abyData, 14) << 12, 0);
									NNS_G2dRotZ(FX_SinIdx(ArrayReader.packInt16(abyData, 2)), FX_CosIdx(ArrayReader.packInt16(abyData, 2)));
									NNS_G2dScale(ArrayReader.packInt32(abyData, 4), ArrayReader.packInt32(abyData, 8), 4096);
									break;
								case 2u:
									NNS_G2dTranslate(ArrayReader.packInt16(abyData, 4) << 12, ArrayReader.packInt16(abyData, 6) << 12, 0);
									break;
								}
								NNS_G2dDrawCell(pCellAnim.pCurrentCell);
								NNS_G2dPopMtx();
							}
						}

						internal static void NNS_G2dInitRenderSurface(NNSG2dRenderSurface pSurface)
						{
						}

						internal static void NNS_G2dPopMtx()
						{
							G3_PopMtx(1);
						}

						internal static void NNS_G2dPushMtx()
						{
							G3_PushMtx();
						}

						internal static void NNS_G2dTranslate(int x, int y, int z)
						{
							G3_Translate(x, y, z);
						}

						internal static void NNS_G2dRotZ(int sin, int cos)
						{
							MtxFx43 mtxFx = fnd_reuse_m;
							MTX_RotZ43(mtxFx, sin, cos);
							MTX_Concat43(mtxFx, currentMtx, currentMtx);
						}

						internal static void NNS_G2dScale(int x, int y, int z)
						{
							G3_Scale(x, y, z);
						}

						internal static void NNS_G2dSetRendererImageProxy(NNSG2dRendererInstance pRend, NNSG2dImageProxy pImgProxy, NNSG2dImagePaletteProxy pPltProxy)
						{
							pRend.rendererCore.pImgProxy = pImgProxy;
							pRend.rendererCore.pPltProxy = pPltProxy;
						}

						internal static void NNS_G2dSetRendererSpriteZoffset(NNSG2dRendererInstance pRend, int spriteZoffset)
						{
						}

						internal static void NNS_G2dSetRendererOverwriteEnable(NNSG2dRendererInstance pRnd, NNSG2dRendererOverwriteParam flag)
						{
						}

						internal static void NNS_G2dSetRendererOverwriteDisable(NNSG2dRendererInstance pRnd, NNSG2dRendererOverwriteParam flag)
						{
						}

						internal static void NNS_G2dSetRendererOverwritePriority(NNSG2dRendererInstance pRend, ushort Priority)
						{
						}

						internal static void NNS_G2dSetRendererOverwritePlttNo(NNSG2dRendererInstance pRend, ushort plttNo)
						{
						}

						internal static void NNS_G3dSetRenderColor(NNSG2dRendererInstance pRend, int r, int g, int b, int a)
						{
							pRend.color[0] = (byte)r;
							pRend.color[1] = (byte)g;
							pRend.color[2] = (byte)b;
							pRend.color[3] = (byte)a;
						}

						internal static void NNS_G2dBGClear()
						{
							for (int i = 0; i < LENGTH(bgCell); i++)
							{
								if (bgCell[i].image != 0)
								{
									DeleteTexture(bgCell[i].image);
									texCount--;
									bgCell[i].image = 0u;
								}
								bgCell[i].oam = null;
								bgCell[i].oam = null;
								bgCell[i].numOAM = 0;
							}
						}

						internal static void NNS_G2dBGSetup(NNSG2dBGSelect bg, NNSG2dScreenData pScnData, NNSG2dCharacterData pChrData, NNSG2dPaletteData pPltData, GXBGScrBase scnBase, GXBGCharBase chrBase)
						{
						}

						internal static void NNS_G2dBGSetupCell(int index, NNSG2dCellDataBank pCellData, NNSG2dBGSelect bg, int x, int y)
						{
							bgCell[index].oam = null;
							bgCell[index].oam = null;
							bgCell[index].numOAM = 0;
							if (pCellData != null)
							{
								NNSG2dCellData nNSG2dCellData = pCellData.pCellDataArrayHead[0];
								bgCell[index].numOAM = nNSG2dCellData.numOAMAttrs;
								bgCell[index].oam = new short[nNSG2dCellData.numOAMAttrs * 7];
								NNSG2dCellOAMAttrData.copy(bgCell[index].oam, nNSG2dCellData.pOamAttrArray, nNSG2dCellData.numOAMAttrs * 7 * 2);
								bgCell[index].bg = bg;
								bgCell[index].x = x;
								bgCell[index].y = y;
								bgCell[index].scale = 1f;
								byte[] color = bgCell[index].color;
								byte[] color2 = bgCell[index].color;
								byte[] color3 = bgCell[index].color;
								byte b;
								bgCell[index].color[3] = (b = byte.MaxValue);
								color[0] = (color2[1] = (color3[2] = b));
							}
						}

						internal static void NNS_G2dSetBGCellPositon(int index, int x, int y)
						{
							bgCell[index].x = x;
							bgCell[index].y = y;
						}

						internal static void NNS_G2dSetBGCellScale(int index, float scale)
						{
							bgCell[index].scale = scale;
							if (bgCell[index].image != 0)
							{
								BindTexture(3553u, bgCell[index].image);
							}
						}

						internal static void NNS_G2dSetBGCellColor(int index, int r, int g, int b, int a)
						{
							bgCell[index].color[0] = (byte)r;
							bgCell[index].color[1] = (byte)g;
							bgCell[index].color[2] = (byte)b;
							bgCell[index].color[3] = (byte)a;
						}

						internal static void NNS_G2dBGSetupCell(int index, NNSG2dCellDataBank pCellData, NNSG2dBGSelect bg)
						{
							NNS_G2dBGSetupCell(index, pCellData, bg, 240, 160);
						}

						internal static void NNS_G2dBGSetupChar(int index, NNSG2dCharacterData pChrData)
						{
							if (bgCell[index].image != 0)
							{
								DeleteTexture(bgCell[index].image);
								texCount--;
								bgCell[index].image = 0u;
							}
							if (pChrData != null)
							{
								bgCell[index].image = LoadPNG((byte[])pChrData.m_aPng, out var sizeW, out var sizeH);
								bgCell[index].texScaleU = 1f / (float)sizeW;
								bgCell[index].texScaleV = 1f / (float)sizeH;
							}
						}

						internal static void NNS_G2dDrawBG(int priority)
						{
							if (skipFrame != 0)
							{
								return;
							}
							for (int num = 7; num >= 0; num--)
							{
								if (bgPriority[num] == priority && (visiblePlane[num >> 2] & (1 << (num & 3))) != 0)
								{
									for (int i = 0; i < LENGTH(bgCell); i++)
									{
										BG_CELL bG_CELL = bgCell[i];
										if (bG_CELL.image != 0 && bG_CELL.bg == (NNSG2dBGSelect)num)
										{
											texScaleU = bG_CELL.texScaleU;
											texScaleV = bG_CELL.texScaleV;
											for (int j = 0; j < bG_CELL.numOAM; j++)
											{
												short[] oam = bG_CELL.oam;
												short num2 = oam[j * 7];
												short num3 = oam[1 + j * 7];
												short num4 = oam[2 + j * 7];
												short num5 = oam[3 + j * 7];
												short num6 = oam[4 + j * 7];
												short num7 = oam[5 + j * 7];
												short num8 = oam[6 + j * 7];
												float num9 = (((num8 & 4) != 0) ? 0.5f : 1f) * bG_CELL.scale;
												float num10 = (((num8 & 8) != 0) ? 0.6f : 1f);
												float num11 = (((num8 & 8) != 0) ? (2f / 3f) : 1f);
												drawImage(vtc, j * 6, (float)bG_CELL.x + (float)num2 * num10 * bG_CELL.scale - (float)bgOffset[num, 0] - (float)screenOffset[0], (float)bG_CELL.y + (float)num3 * num11 * bG_CELL.scale - (float)bgOffset[num, 1] - (float)screenOffset[1], (float)num4 * num10 * num9, (float)num5 * num11 * num9, ((num8 & 1) == 0) ? num6 : (num6 + num4), ((num8 & 2) == 0) ? num7 : (num7 + num5), ((num8 & 1) == 0) ? num4 : (-num4), ((num8 & 2) == 0) ? num5 : (-num5), bG_CELL.color);
											}
											glEnable(3553u);
											BindTexture(3553u, bG_CELL.image);
											glDrawArrays(4u, 0, bG_CELL.numOAM * 6, vtc);
											polyCount += bG_CELL.numOAM * 6;
											glDisableClientState(32888u);
											glDisable(3553u);
										}
									}
								}
							}
						}

						internal static void NNS_G2dFontInitAuto(NNSG2dFont pFont, Array pNftrFile)
						{
							pFont.size = ((int[])pNftrFile)[0];
						}

						internal static byte NNS_G2dFontGetHeight(NNSG2dFont pFont)
						{
							return (byte)pFont.size;
						}

						internal static void NNS_G2dFontGetGlyph(NNSG2dGlyph pGlyph, NNSG2dFont pFont, ushort ccode)
						{
						}

						internal static NNSG2dTextRect NNS_G2dFontGetTextRect(NNSG2dFont pFont, int hSpace, int vSpace, string txt)
						{
							NNSG2dTextRect nNSG2dTextRect = new NNSG2dTextRect(0, pFont.size);
							string text = txt;
							while (true)
							{
								string text2 = strchr(text, 10);
								int length = ((text2 != null) ? (text.Length - text2.Length) : strlen(text));
								string text3 = text.Substring(0, length);
								int num = getStringWidth(text3, pFont.size);
								string text4 = strchr(text3, 37);
								if (text4 != null)
								{
									if (strncmp(text4, "%player_level", 13) == 0)
									{
										num -= getStringWidth("player_level1", pFont.size);
									}
									if (strncmp(text4, "%shuyaku", 8) == 0)
									{
										num -= getStringWidth("shuyaku1", pFont.size);
									}
								}
								text3 = null;
								nNSG2dTextRect.width = MATH_MAX(nNSG2dTextRect.width, num);
								if (text2 == null)
								{
									break;
								}
								text = text2.Substring(1);
								nNSG2dTextRect.height += pFont.size + vSpace;
							}
							return nNSG2dTextRect;
						}

						internal static void NNS_G2dCharCanvasInitForBG(NNSG2dCharCanvas pCC, Array charBase, int charBase_offset, int areaWidth, int areaHeight, NNSG2dCharaColorMode colorMode, int lcd)
						{
							pCC.charBase = (byte[])charBase;
							pCC.charBase_idx = charBase_offset;
							pCC.areaWidth = areaWidth;
							pCC.areaHeight = areaHeight;
							pCC.lcd = lcd;
							NNS_G2dCharCanvasClear(pCC, 0);
						}

						internal static void NNS_G2dCharCanvasInitForBG(NNSG2dCharCanvas pCC, Array charBase, int areaWidth, int areaHeight, NNSG2dCharaColorMode colorMode, int lcd)
						{
							NNS_G2dCharCanvasInitForBG(pCC, charBase, 0, areaWidth, areaHeight, colorMode, lcd);
						}

						internal static int NNS_G2dCharCanvasDrawChar(NNSG2dCharCanvas pCC, NNSG2dFont pFont, int x, int y, int cl, ushort ccode)
						{
							return 0;
						}

						internal static void NNS_G2dCharCanvasClear(NNSG2dCharCanvas pCC, int cl)
						{
							for (int i = 0; i < LENGTH(textData); i++)
							{
								TEXT_DATA tEXT_DATA = textData[i];
								if (tEXT_DATA.text != null && (pCC == null || tEXT_DATA.lcd == pCC.lcd))
								{
									tEXT_DATA.text = null;
									tEXT_DATA.text = null;
								}
							}
						}

						internal static void NNS_G2dCharCanvasClearArea(NNSG2dCharCanvas pCC, NNSG2dFont pFont, int cl, int x, int y, int w, int h)
						{
							int num = x + w;
							int num2 = y + h;
							for (int i = 0; i < LENGTH(textData); i++)
							{
								TEXT_DATA tEXT_DATA = textData[i];
								if (tEXT_DATA.text != null && tEXT_DATA.lcd == pCC.lcd && (pFont == null || tEXT_DATA.size == pFont.size))
								{
									int num3 = tEXT_DATA.x + getStringWidth(tEXT_DATA.text, tEXT_DATA.size) / 2;
									int num4 = tEXT_DATA.y + tEXT_DATA.size / 2;
									if (num3 >= x && num3 < num && num4 >= y && num4 < num2)
									{
										tEXT_DATA.text = null;
										tEXT_DATA.text = null;
									}
								}
							}
						}

						internal static void NNS_G2dMapScrToCharText(Array scnBase, int areaWidth, int areaHeight, int areaLeft, int areaTop, NNSG2dTextBGWidth scnWidth, int charNo, int cplt)
						{
						}

						internal static void NNS_G2dTextCanvasInit(NNSG2dTextCanvas pTxn, NNSG2dCharCanvas pCC, NNSG2dFont pFont, int hSpace, int vSpace)
						{
							pTxn.pCanvas = pCC;
							pTxn.pFont = pFont;
							pTxn.hSpace = hSpace;
							pTxn.vSpace = vSpace;
						}

						internal static NNSG2dFont NNS_G2dTextCanvasGetFont(NNSG2dTextCanvas pTxn)
						{
							return pTxn.pFont;
						}

						internal static void NNS_G2dTextCanvasDrawText(NNSG2dTextCanvas pTxn, int x, int y, int cl, uint flags, int priority, string txt)
						{
							cl = textColor[cl];
							NNSG2dTextRect nNSG2dTextRect = NNS_G2dFontGetTextRect(pTxn.pFont, pTxn.hSpace, pTxn.vSpace, txt);
							if ((flags & 0x10) != 0)
							{
								x -= nNSG2dTextRect.width / 2;
							}
							if ((flags & 0x20) != 0)
							{
								x -= nNSG2dTextRect.width;
							}
							if ((flags & 2) != 0)
							{
								y -= nNSG2dTextRect.height / 2;
							}
							if ((flags & 0x100) != 0)
							{
								y -= nNSG2dTextRect.height;
							}
							string text = txt;
							while (true)
							{
								string text2 = strchr(text, 10);
								int length = ((text2 != null) ? (text.Length - text2.Length) : strlen(text));
								int num = -1;
								for (int i = 0; i < LENGTH(textData); i++)
								{
									TEXT_DATA tEXT_DATA = textData[i];
									if (tEXT_DATA.text != null && tEXT_DATA.x == x && tEXT_DATA.y == y)
									{
										num = i;
										break;
									}
									if (num == -1 && tEXT_DATA.text == null)
									{
										num = i;
									}
								}
								if (num != -1)
								{
									TEXT_DATA tEXT_DATA2 = textData[num];
									tEXT_DATA2.text = null;
									tEXT_DATA2.text = text.Substring(0, length);
									tEXT_DATA2.x = (short)x;
									tEXT_DATA2.y = (short)y;
									if ((flags & 0x400) != 0)
									{
										tEXT_DATA2.x += (short)((nNSG2dTextRect.width - NNS_G2dFontGetTextRect(pTxn.pFont, 0, 0, tEXT_DATA2.text).width) / 2);
									}
									if ((flags & 0x800) != 0)
									{
										tEXT_DATA2.x += (short)(nNSG2dTextRect.width - NNS_G2dFontGetTextRect(pTxn.pFont, 0, 0, tEXT_DATA2.text).width);
									}
									tEXT_DATA2.color = cl;
									tEXT_DATA2.lcd = (sbyte)pTxn.pCanvas.lcd;
									tEXT_DATA2.size = (short)pTxn.pFont.size;
									tEXT_DATA2.priority = (sbyte)priority;
									tEXT_DATA2.flags = flags;
								}
								if (text2 == null)
								{
									break;
								}
								text = text2.Substring(1);
								y += pTxn.pFont.size + pTxn.vSpace;
							}
						}

						internal static void NNS_G2dTextCanvasDrawTaggedText(NNSG2dTextCanvas pTxn, int x, int y, int cl, string txt, NNSG2dTagCallback cbFunc, Array cbParam)
						{
						}

						internal static void NNS_G2dDragHilight(int x, int y, int w, int h, int priority)
						{
							hilightRect.x = x;
							hilightRect.y = y;
							hilightRect.w = w;
							hilightRect.h = h;
							hilightRect.priority = priority;
						}

						internal static void NNS_G2dDrawText(int priority)
						{
							TPData tPData = fnd_reuse_tp;
							TP_RequestRawSampling(tPData);
							if (hilightRect.w != 0 && hilightRect.h != 0 && hilightRect.priority == priority)
							{
								if (skipFrame == 0)
								{
									float num = hilightRect.x;
									float num2 = hilightRect.x + hilightRect.w;
									float num3 = hilightRect.y;
									float num4 = hilightRect.y + hilightRect.h;
									float num5 = num + 4f;
									float num6 = num3 + 4f;
									float num7 = num2 - 4f;
									float num8 = num4 - 4f;
									uint num9 = 1610678271u;
									uint num10 = 65535u;
									float[] array = new float[108]
									{
										num, num3, num, num6, num5, num6, num5, num6, num5, num3,
										num, num3, num5, num3, num5, num6, num7, num6, num7, num6,
										num7, num3, num5, num3, num7, num6, num2, num6, num2, num3,
										num2, num3, num7, num3, num7, num6, num, num6, num, num8,
										num5, num8, num5, num8, num5, num6, num, num6, num5, num6,
										num5, num8, num7, num8, num7, num8, num7, num6, num5, num6,
										num7, num6, num7, num8, num2, num8, num2, num8, num2, num6,
										num7, num6, num, num4, num5, num4, num5, num8, num5, num8,
										num, num8, num, num4, num5, num8, num5, num4, num7, num4,
										num7, num4, num7, num8, num5, num8, num7, num8, num7, num4,
										num2, num4, num2, num4, num2, num8, num7, num8
									};
									uint[] array2 = new uint[54]
									{
										num10, num10, num9, num9, num10, num10, num10, num9, num9, num9,
										num10, num10, num9, num10, num10, num10, num10, num9, num10, num10,
										num9, num9, num9, num10, num9, num9, num9, num9, num9, num9,
										num9, num9, num10, num10, num10, num9, num10, num10, num9, num9,
										num10, num10, num9, num10, num10, num10, num9, num9, num9, num10,
										num10, num10, num10, num9
									};
									for (int i = 0; i < 54; i++)
									{
										vtc[i].Position.X = array[i * 2];
										vtc[i].Position.Y = array[i * 2 + 1];
										vtc[i].Position.Z = 0f;
									}
									for (int j = 0; j < 54; j++)
									{
										vtc[j].Color.PackedValue = array2[j];
									}
									glDisable(3553u);
									glDrawArrays(4u, 0, 54, vtc);
								}
								hilightRect.w = (hilightRect.h = 0);
							}
							m_Graphics.DrawStringStart();
							for (int k = 0; k < LENGTH(textData); k++)
							{
								TEXT_DATA tEXT_DATA = textData[k];
								if (tEXT_DATA.text != null && tEXT_DATA.priority == priority)
								{
									uint num11 = (uint)tEXT_DATA.color;
									if (textBrightness[tEXT_DATA.lcd] < 0)
									{
										int num12 = 16 + textBrightness[tEXT_DATA.lcd];
										num11 >>= 4;
										num11 = (uint)((((num11 & 0xFF0) * num12) & 0xFF00) | (((num11 & 0xFF000) * num12) & 0xFF0000) | (((num11 & 0xFF00000) * num12) & 0xFF000000u));
									}
									if (textBrightness[tEXT_DATA.lcd] > 0)
									{
										int num13 = textBrightness[tEXT_DATA.lcd];
										int num14 = 16 - num13;
										num11 >>= 4;
										num11 = (uint)((((num11 & 0xFF0) * num14 + 4080 * num13) & 0xFF00) | (((num11 & 0xFF000) * num14 + 1044480 * num13) & 0xFF0000) | (((num11 & 0xFF00000) * num14 + 267386880 * num13) & 0xFF000000u));
									}
									num11 = (uint)((uint)((int)num11 & -256) | (textAlpha[tEXT_DATA.lcd] * 255 / 16));
									int num15 = tEXT_DATA.x - screenOffset[0];
									int num16 = tEXT_DATA.y - screenOffset[1];
									if ((tEXT_DATA.flags & 0x1000) != 0 && tPData.drag != 0)
									{
										num15 += tPData.x - tPData.dragX;
										num16 += tPData.y - tPData.dragY;
									}
									if ((tEXT_DATA.flags & 0x4000) != 0)
									{
										drawString(tEXT_DATA.text, num15 + 1, num16 + 1, 255, tEXT_DATA.size);
									}
									drawString(tEXT_DATA.text, num15, num16, (int)num11, tEXT_DATA.size);
								}
							}
							m_Graphics.DrawStringEnd();
						}

						internal static void NNS_G3dGlbFlushP()
						{
							float[] array = fnd_reuse_f;
							G3_MtxMode(GXMtxMode.GX_MTXMODE_PROJECTION);
							G3_LoadMtx44(NNS_G3dGlb.projMtx);
							G3_MtxMode(GXMtxMode.GX_MTXMODE_POSITION);
							G3_LoadMtx43(NNS_G3dGlb.cameraMtx);
							glMatrixMode(5888u);
							MTX_Copy43ToGLfloat(NNS_G3dGlb.cameraMtx, array);
							glLoadMatrixf(array);
							float[] array2 = fnd_reuse_color;
							float[] array3 = fnd_reuse_position;
							glLightfv(16384u, 4608u, array2);
							glLightfv(16384u, 4609u, array2);
							glLightfv(16384u, 5632u, array2);
							glLightfv(16384u, 4611u, array3);
							glLoadIdentity();
							glEnable(2929u);
							MtxFx43 mtxFx = fnd_reuse_m;
							MTX_Identity43(mtxFx);
							MTX_TransApply43(mtxFx, mtxFx, NNS_G3dGlb.prmBaseTrans.x, NNS_G3dGlb.prmBaseTrans.y, NNS_G3dGlb.prmBaseTrans.z);
							MTX_Copy33To43(NNS_G3dGlb.prmBaseRot, mtxFx);
							MTX_ScaleApply43(mtxFx, mtxFx, NNS_G3dGlb.prmBaseScale.x, NNS_G3dGlb.prmBaseScale.y, NNS_G3dGlb.prmBaseScale.z);
							G3_MultMtx43(mtxFx);
						}

						internal static void NNS_G3dGlbSetBaseScale(VecFx32 pScale)
						{
							NNS_G3dGlb.prmBaseScale.copy(pScale);
						}

						internal static void NNS_G3dGlbSetBaseRot(MtxFx33 pRot)
						{
							NNS_G3dGlb.prmBaseRot.copy(pRot);
						}

						internal static void NNS_G3dGlbSetBaseTrans(VecFx32 pTrans)
						{
							NNS_G3dGlb.prmBaseTrans.copy(pTrans);
						}

						internal static void NNS_G3dGlbLightVector(GXLightId lightID, short x, short y, short z)
						{
							NNS_G3dGlb.lightVec[(int)lightID] = (uint)(((x >> 3) & 0x3FF) | (((y >> 3) & 0x3FF) << 10) | (((z >> 3) & 0x3FF) << 20));
						}

						internal static void NNS_G3dGlbLightColor(GXLightId lightID, ushort rgb)
						{
							NNS_G3dGlb.lightColor[(int)lightID] = rgb;
						}

						internal static void NNS_G3dGlbMaterialColorDiffAmb(ushort diffuse, ushort ambient, int IsSetVtxColor)
						{
							NNS_G3dGlb.prmMatColor0 = (uint)(diffuse | (ambient << 16));
						}

						internal static void NNS_G3dGlbMaterialColorSpecEmi(ushort specular, ushort emission, int IsShininess)
						{
							NNS_G3dGlb.prmMatColor0 = (uint)(specular | (emission << 16));
						}

						internal static void NNS_G3dGlbPolygonAttr(int light, GXPolygonMode polyMode, GXCull cullMode, int polygonID, int alpha, int misc)
						{
						}

						internal static void NNS_G3dGlbLookAt(VecFx32 camPos, VecFx32 camUp, VecFx32 target)
						{
							VecFx32 vecFx = nitro_reuse_x;
							VecFx32 vecFx2 = nitro_reuse_y;
							VecFx32 vecFx3 = nitro_reuse_z;
							VEC_Subtract(camPos, target, vecFx3);
							VEC_Normalize(vecFx3, vecFx3);
							VEC_CrossProduct(camUp, vecFx3, vecFx);
							VEC_Normalize(vecFx, vecFx);
							VEC_CrossProduct(vecFx3, vecFx, vecFx2);
							VEC_Normalize(vecFx2, vecFx2);
							MtxFx43 mtxFx = nitro_reuse_m43;
							mtxFx.a[0] = vecFx.x;
							mtxFx.a[1] = vecFx2.x;
							mtxFx.a[2] = vecFx3.x;
							mtxFx.a[3] = vecFx.y;
							mtxFx.a[4] = vecFx2.y;
							mtxFx.a[5] = vecFx3.y;
							mtxFx.a[6] = vecFx.z;
							mtxFx.a[7] = vecFx2.z;
							mtxFx.a[8] = vecFx3.z;
							mtxFx.a[9] = 0;
							mtxFx.a[10] = 0;
							mtxFx.a[11] = 0;
							MTX_TransApply43(mtxFx, NNS_G3dGlb.cameraMtx, -camPos.x, -camPos.y, -camPos.z);
							NNS_G3dGlb.camPos.copy(camPos);
							NNS_G3dGlb.camUp.copy(camUp);
							NNS_G3dGlb.camTarget.copy(target);
							MtxFx43 mtxFx2 = nitro_reuse_m43;
							mtxFx2.a[0] = vecFx.x;
							mtxFx2.a[1] = vecFx.y;
							mtxFx2.a[2] = vecFx.z;
							mtxFx2.a[3] = vecFx2.x;
							mtxFx2.a[4] = vecFx2.y;
							mtxFx2.a[5] = vecFx2.z;
							mtxFx2.a[6] = vecFx3.x;
							mtxFx2.a[7] = vecFx3.y;
							mtxFx2.a[8] = vecFx3.z;
							mtxFx2.a[9] = camPos.x;
							mtxFx2.a[10] = camPos.y;
							mtxFx2.a[11] = camPos.z;
							NNS_G3dGlb.invCameraMtx.copy(mtxFx2);
						}

						internal static void NNS_G3dGlbPerspective(int fovySin, int fovyCos, int aspect, int n, int f)
						{
							n /= 2;
							int num = FX_Div(fovyCos, FX_Mul(fovySin, aspect));
							MtxFx44 projMtx = NNS_G3dGlb.projMtx;
							projMtx._00 = num * LCD_WIDTH / PERSPECTIVE_WIDTH;
							projMtx._01 = 0;
							projMtx._02 = 0;
							projMtx._03 = 0;
							projMtx._10 = 0;
							projMtx._11 = num * LCD_WIDTH / PERSPECTIVE_HEIGHT;
							projMtx._12 = 0;
							projMtx._13 = 0;
							projMtx._20 = 0;
							projMtx._21 = 0;
							projMtx._22 = FX_Div(-f - n, f - n);
							projMtx._23 = -4096;
							projMtx._30 = 0;
							projMtx._31 = 0;
							projMtx._32 = FX_Div(-2 * FX_Mul(f, n), f - n);
							projMtx._33 = 0;
						}

						internal static void NNS_G3dGlbFrustum(int t, int b, int l, int r, int n, int f)
						{
							int num = (b + t) / 2;
							int num2 = (r - l) * PERSPECTIVE_HEIGHT / (LCD_WIDTH * 2);
							t = num + num2;
							b = num - num2;
							int num3 = (l + r) / 2;
							int num4 = (r - l) * PERSPECTIVE_WIDTH / (LCD_WIDTH * 2);
							r = num3 + num4;
							l = num3 - num4;
							MtxFx44 projMtx = NNS_G3dGlb.projMtx;
							projMtx._00 = FX_Div(2 * n, r - l);
							projMtx._01 = 0;
							projMtx._02 = 0;
							projMtx._03 = 0;
							projMtx._10 = 0;
							projMtx._11 = FX_Div(2 * n, t - b);
							projMtx._12 = 0;
							projMtx._13 = 0;
							projMtx._20 = FX_Div(r + l, r - l);
							projMtx._21 = FX_Div(t + b, t - b);
							projMtx._22 = FX_Div(-f - n, f - n);
							projMtx._23 = -4096;
							projMtx._30 = 0;
							projMtx._31 = 0;
							projMtx._32 = FX_Div(-2 * FX_Mul(f, n), f - n);
							projMtx._33 = 0;
						}

						internal static MtxFx43 NNS_G3dGlbGetCameraMtx()
						{
							return NNS_G3dGlb.cameraMtx;
						}

						internal static VecFx32 NNS_G3dGlbGetCameraPos()
						{
							return NNS_G3dGlb.camPos;
						}

						internal static MtxFx43 NNS_G3dGlbGetInvCameraMtx()
						{
							return NNS_G3dGlb.invCameraMtx;
						}

						internal static void NNS_G3dAnmObjInit(NNSG3dAnmObj pAnmObj, NNSG3dResAnmHeader pResAnm, NNSG3dResMdl pResMdl, NNSG3dResTex pResTex)
						{
							pAnmObj.setDefault();
							pAnmObj.resAnm = pResAnm;
							pAnmObj.resTex = pResTex;
							pAnmObj.ratio = 4096;
						}

						internal static void NNS_G3dAnmObjSetFrame(NNSG3dAnmObj pAnmObj, int frame)
						{
							pAnmObj.frame = frame;
						}

						internal static void NNS_G3dAnmObjSetBlendRatio(NNSG3dAnmObj pAnmObj, int ratio)
						{
							pAnmObj.ratio = ratio;
						}

						internal static int NNS_G3dAnmObjGetNumFrame(NNSG3dAnmObj pAnmObj)
						{
							NNSG3dResMatCAnm nNSG3dResMatCAnm = pAnmObj.resAnm.m_Anm as NNSG3dResMatCAnm;
							NNSG3dResTexSRTAnm nNSG3dResTexSRTAnm = pAnmObj.resAnm.m_Anm as NNSG3dResTexSRTAnm;
							NNSG3dResJntAnm nNSG3dResJntAnm = pAnmObj.resAnm.m_Anm as NNSG3dResJntAnm;
							if (nNSG3dResMatCAnm != null)
							{
								return nNSG3dResMatCAnm.numFrame * 4096;
							}
							if (nNSG3dResTexSRTAnm != null)
							{
								return nNSG3dResTexSRTAnm.numFrame * 4096;
							}
							if (nNSG3dResJntAnm != null)
							{
								return nNSG3dResJntAnm.numFrame * 4096;
							}
							return 0;
						}

						internal static void NNS_G3dRenderObjInit(NNSG3dRenderObj pRenderObj, NNSG3dResMdl pResMdl)
						{
							pRenderObj.setDefault();
							pRenderObj.resMdl = pResMdl;
						}

						internal static NNSG3dResMdl NNS_G3dRenderObjGetResMdl(NNSG3dRenderObj pRenderObj)
						{
							return pRenderObj.resMdl;
						}

						internal static void NNS_G3dRenderObjAddAnmObj(NNSG3dRenderObj pRenderObj, NNSG3dAnmObj pAnmObj)
						{
							NNSG3dResAnmHeader resAnm = pAnmObj.resAnm;
							NNSG3dAnmObj next = null;
							if (resAnm.category0 == 74)
							{
								next = pRenderObj.anmJnt;
							}
							if (resAnm.category0 == 77)
							{
								next = pRenderObj.anmMat;
							}
							pAnmObj.next = next;
							if (resAnm.category0 == 74)
							{
								pRenderObj.anmJnt = pAnmObj;
							}
							if (resAnm.category0 == 77)
							{
								pRenderObj.anmMat = pAnmObj;
							}
						}

						internal static void NNS_G3dRenderObjRemoveAnmObj(NNSG3dRenderObj pRenderObj, NNSG3dAnmObj pAnmObj)
						{
							if (pAnmObj == null)
							{
								return;
							}
							int num = 0;
							NNSG3dResAnmHeader resAnm = pAnmObj.resAnm;
							NNSG3dAnmObj nNSG3dAnmObj = null;
							if (resAnm.category0 == 74)
							{
								nNSG3dAnmObj = pRenderObj.anmJnt;
							}
							if (resAnm.category0 == 77)
							{
								nNSG3dAnmObj = pRenderObj.anmMat;
							}
							while (nNSG3dAnmObj != null && nNSG3dAnmObj != null)
							{
								if (nNSG3dAnmObj == pAnmObj)
								{
									if (num > 0)
									{
										if (resAnm.category0 == 74)
										{
											nNSG3dAnmObj = pRenderObj.anmJnt;
										}
										if (resAnm.category0 == 77)
										{
											nNSG3dAnmObj = pRenderObj.anmMat;
										}
										for (int i = 1; i < num; i++)
										{
											nNSG3dAnmObj = nNSG3dAnmObj.next;
										}
										nNSG3dAnmObj.next = pAnmObj.next;
									}
									else
									{
										if (resAnm.category0 == 74)
										{
											pRenderObj.anmJnt = pAnmObj.next;
										}
										if (resAnm.category0 == 77)
										{
											pRenderObj.anmMat = pAnmObj.next;
										}
									}
									pAnmObj.next = null;
									break;
								}
								nNSG3dAnmObj = nNSG3dAnmObj.next;
								num++;
							}
						}

						internal static void NNS_G3dRenderObjSetJntAnmBuffer(NNSG3dRenderObj pRenderObj, NNSG3dJntAnmResult[] buf)
						{
							pRenderObj.recJntAnm = buf;
						}

						internal static void NNS_G3dRenderObjReleaseJntAnmBuffer(NNSG3dRenderObj pRenderObj)
						{
							pRenderObj.recJntAnm = null;
						}

						internal static void NNS_G3dRenderObjSetCallBack(NNSG3dRenderObj pRenderObj, NNSG3dSbcCallBackFunc func, byte[] unuse0, byte cmd, NNSG3dSbcCallBackTiming timing)
						{
							pRenderObj.cbFunc = func;
							pRenderObj.cbCmd = cmd;
							pRenderObj.cbTiming = (byte)timing;
						}

						internal static void NNS_G3dRenderObjResetCallBack(NNSG3dRenderObj pRenderObj)
						{
							pRenderObj.cbFunc = null;
							pRenderObj.cbCmd = 0;
							pRenderObj.cbTiming = 0;
						}

						internal static void NNS_G3dRenderObjSetFlag(NNSG3dRenderObj pRenderObj, NNSG3dRenderObjFlag flag)
						{
							pRenderObj.flag |= (uint)flag;
						}

						internal static void NNS_G3dRenderObjResetFlag(NNSG3dRenderObj pRenderObj, NNSG3dRenderObjFlag flag)
						{
							pRenderObj.flag &= (uint)(~flag);
						}

						internal static uint LoadTexture(uint fmt, uint sizeS, uint sizeT, uint color0, ushort[] texData, int iTexDataOffset, uint[] tex4x4Data, int iTex4x4DataOffset, ushort[] tex4x4IdxData, int iTex4x4IdxDataOffset, ushort[] plttData, int iPlttDataOffset, uint wrap)
						{
							int num = 8 << (int)sizeS;
							int num2 = 8 << (int)sizeT;
							byte[] array = new byte[num * num2 * 4];
							int num3 = 0;
							uint format = 5121u;
							switch (fmt)
							{
							case 6u:
							{
								int num4 = 0;
								while (num4 < num * num2)
								{
									int num30 = (texData[iTexDataOffset + (num4 >> 1)] >> (num4 & 1) * 8) & 0xFF;
									int num31 = plttData[iPlttDataOffset + (num30 & 7)];
									array[num3] = (byte)((num31 & 0x1F) << 3);
									array[num3 + 1] = (byte)(((num31 >> 5) & 0x1F) << 3);
									array[num3 + 2] = (byte)(((num31 >> 10) & 0x1F) << 3);
									array[num3 + 3] = (byte)((num30 >> 3) * 255 / 31);
									num4++;
									num3 += 4;
								}
								break;
							}
							case 1u:
							{
								int num4 = 0;
								while (num4 < num * num2)
								{
									int num19 = (texData[iTexDataOffset + (num4 >> 1)] >> (num4 & 1) * 8) & 0xFF;
									int num20 = plttData[iPlttDataOffset + (num19 & 0x1F)];
									array[num3] = (byte)((num20 & 0x1F) << 3);
									array[num3 + 1] = (byte)(((num20 >> 5) & 0x1F) << 3);
									array[num3 + 2] = (byte)(((num20 >> 10) & 0x1F) << 3);
									array[num3 + 3] = (byte)((num19 >> 5) * 255 / 7);
									num4++;
									num3 += 4;
								}
								break;
							}
							case 2u:
							{
								int num4 = 0;
								while (num4 < num * num2)
								{
									int num22 = (texData[iTexDataOffset + (num4 >> 3)] >> (num4 & 7) * 2) & 3;
									int num23 = plttData[iPlttDataOffset + num22];
									array[num3] = (byte)((num23 & 0x1F) << 3);
									array[num3 + 1] = (byte)(((num23 >> 5) & 0x1F) << 3);
									array[num3 + 2] = (byte)(((num23 >> 10) & 0x1F) << 3);
									array[num3 + 3] = (byte)((num22 != 0 || color0 == 0) ? 255u : 0u);
									num4++;
									num3 += 4;
								}
								break;
							}
							case 3u:
							{
								int num4 = 0;
								while (num4 < num * num2)
								{
									int num32 = (texData[iTexDataOffset + (num4 >> 2)] >> (num4 & 3) * 4) & 0xF;
									int num33 = plttData[iPlttDataOffset + num32];
									array[num3] = (byte)((num33 & 0x1F) << 3);
									array[num3 + 1] = (byte)(((num33 >> 5) & 0x1F) << 3);
									array[num3 + 2] = (byte)(((num33 >> 10) & 0x1F) << 3);
									array[num3 + 3] = (byte)((num32 != 0 || color0 == 0) ? 255u : 0u);
									num4++;
									num3 += 4;
								}
								break;
							}
							case 4u:
							{
								int num4 = 0;
								while (num4 < num * num2)
								{
									int num24 = (texData[iTexDataOffset + (num4 >> 1)] >> (num4 & 1) * 8) & 0xFF;
									int num25 = plttData[iPlttDataOffset + num24];
									int num26 = num25 & 0x1F;
									int num27 = (num25 >> 5) & 0x1F;
									int num28 = (num25 >> 10) & 0x1F;
									int num29 = ((num24 != 0 || color0 == 0) ? 1 : 0);
									array[num3] = (byte)(num26 * 255 / 31);
									array[num3 + 1] = (byte)(num27 * 255 / 31);
									array[num3 + 2] = (byte)(num28 * 255 / 31);
									array[num3 + 3] = (byte)(((num29 & 0xFF) != 0) ? 255u : 0u);
									num4++;
									num3 += 4;
								}
								break;
							}
							case 7u:
							{
								int num4 = 0;
								while (num4 < num * num2)
								{
									int num21 = texData[iTexDataOffset + num4];
									array[num3] = (byte)((num21 & 0x1F) << 3);
									array[num3 + 1] = (byte)(((num21 >> 5) & 0x1F) << 3);
									array[num3 + 2] = (byte)(((num21 >> 10) & 0x1F) << 3);
									array[num3 + 3] = (byte)(((num21 & 0x8000) != 0) ? 255u : 0u);
									num4++;
									num3 += 4;
								}
								break;
							}
							case 5u:
							{
								int i = 0;
								int num4 = 0;
								for (; i < num2 >> 2; i++)
								{
									int num5 = 0;
									while (num5 < num >> 2)
									{
										int num6 = tex4x4IdxData[iTex4x4IdxDataOffset + num4];
										int num7 = (int)tex4x4Data[iTex4x4DataOffset + num4];
										int num8 = (num6 & 0xFFF) << 1;
										byte[] array2 = new byte[16];
										num3 = 0;
										if ((num6 & 0x4000) == 0)
										{
											int num9 = 0;
											while (num9 < 4)
											{
												if ((num6 & 0x8000) == 0 && num9 == 3)
												{
													array2[num3] = (array2[num3 + 1] = (array2[num3 + 2] = (array2[num3 + 3] = 0)));
												}
												else
												{
													int num10 = plttData[iPlttDataOffset + num8 + num9];
													array2[num3] = (byte)(num10 << 3);
													array2[num3 + 1] = (byte)(num10 >> 5 << 3);
													array2[num3 + 2] = (byte)(num10 >> 10 << 3);
													array2[num3 + 3] = byte.MaxValue;
												}
												num9++;
												num3 += 4;
											}
										}
										else
										{
											int[] array3 = new int[16]
											{
												8, 0, 0, 8, 4, 4, 0, 0, 8, 0,
												0, 8, 5, 3, 3, 5
											};
											int num11 = plttData[iPlttDataOffset + num8];
											int num12 = plttData[iPlttDataOffset + num8 + 1];
											int num13 = (num6 & 0x8000) >> 12;
											int num14 = 0;
											while (num14 < 4)
											{
												int num15 = array3[num13];
												int num16 = array3[num13 + 1];
												array2[num3] = (byte)((num11 & 0x1F) * num15 + (num12 & 0x1F) * num16);
												array2[num3 + 1] = (byte)(((num11 >> 5) & 0x1F) * num15 + ((num12 >> 5) & 0x1F) * num16);
												array2[num3 + 2] = (byte)(((num11 >> 10) & 0x1F) * num15 + ((num12 >> 10) & 0x1F) * num16);
												array2[num3 + 3] = (byte)(31 * num15 + 31 * num16);
												num14++;
												num13 += 2;
												num3 += 4;
											}
										}
										for (int j = 0; j < 4; j++)
										{
											num3 = (((i << 2) + j) * num + (num5 << 2)) * 4;
											int num17 = 0;
											while (num17 < 4)
											{
												int num18 = (num7 & 3) * 4;
												array[num3] = array2[num18];
												array[num3 + 1] = array2[num18 + 1];
												array[num3 + 2] = array2[num18 + 2];
												array[num3 + 3] = array2[num18 + 3];
												num7 >>= 2;
												num17++;
												num3 += 4;
											}
										}
										num5++;
										num4++;
									}
								}
								break;
							}
							}
							uint num34 = GenTexture(num, num2, array, wrap, 9728u, format);
							if (num34 != 0)
							{
								texCount++;
							}
							return num34;
						}

						internal static void LoadTexture(TexVram vramKey, GXTexFmt fmt, GXTexSizeS sizeS, GXTexSizeT sizeT, int color0, ushort[] texData, ushort[] plttData)
						{
							if (vramKey.list == null)
							{
								vramKey.size = 1;
								vramKey.list = new TexVramList[1];
								vramKey.list[0] = new TexVramList();
							}
							if (vramKey.list[0].tex == 0)
							{
								vramKey.list[0].fmt = (uint)fmt;
								vramKey.list[0].tex = LoadTexture((uint)fmt, (uint)sizeS, (uint)sizeT, (uint)color0, texData, 0, null, 0, null, 0, plttData, 0, 33071u);
							}
						}

						internal static void SendTextureParam(TexVram vramKey, GXTexSizeS sizeS, GXTexSizeT sizeT)
						{
							if (vramKey != null && vramKey.list != null)
							{
								glEnable(3553u);
								BindTexture(3553u, vramKey.list[0].tex);
								texScaleU = 1f / (float)(8 << (int)sizeS);
								texScaleV = 1f / (float)(8 << (int)sizeT);
							}
							else
							{
								glDisable(3553u);
								texScaleU = (texScaleV = 0f);
							}
						}

						internal static uint NNS_G3dTexGetRequiredSize(NNSG3dResTex pTex)
						{
							NNSG3dResDict dict = pTex.texInfo.dict;
							return dict.numEntry;
						}

						internal static uint NNS_G3dTex4x4GetRequiredSize(NNSG3dResTex pTex)
						{
							NNSG3dResDict dict = pTex.tex4x4Info.dict;
							return dict.numEntry;
						}

						internal static void NNS_G3dTexSetTexKey(NNSG3dResTex pTex, TexVram texKey, TexVram tex4x4Key)
						{
							pTex.texInfo.vramKey = texKey;
							pTex.tex4x4Info.vramKey = tex4x4Key;
						}

						internal static void NNS_G3dTexLoad(NNSG3dResTex pTex, int exec_begin_end)
						{
							NNSG3dResDict dict = pTex.texInfo.dict;
							NNSG3dResDict dict2 = pTex.plttInfo.dict;
							TexVram vramKey = pTex.texInfo.vramKey;
							if (vramKey.list == null)
							{
								vramKey.size = dict.numEntry;
								vramKey.list = new TexVramList[vramKey.size];
								for (int i = 0; i < vramKey.size; i++)
								{
									vramKey.list[i] = new TexVramList();
								}
							}
							if (dict.numEntry == 1 && dict2.numEntry == 1 && vramKey.list[0].tex == 0)
							{
								NNSG3dResDictEntryHeader entry = dict.entry;
								NNSG3dResDictEntryHeader entry2 = dict2.entry;
								uint[] array = new uint[entry.sizeUnit / 4];
								ushort[] array2 = new ushort[entry2.sizeUnit / 2];
								ArrayReader arrayReader = new ArrayReader(entry.data);
								ArrayReader arrayReader2 = arrayReader;
								_ = entry.sizeUnit;
								arrayReader2.setPosition(0L);
								arrayReader.read(array, 0, array.Length);
								arrayReader = new ArrayReader(entry2.data);
								ArrayReader arrayReader3 = arrayReader;
								_ = entry2.sizeUnit;
								arrayReader3.setPosition(0L);
								arrayReader.read(array2, 0, array2.Length);
								int num = (int)(array[0] & 0xFFFFF);
								vramKey.list[0].pTex = pTex;
								vramKey.list[0].dictTexData = array;
								vramKey.list[0].fmt = (array[0] >> 26) & 7;
								vramKey.list[0].tex = LoadTexture((array[0] >> 26) & 7, (array[0] >> 20) & 7, (array[0] >> 23) & 7, (array[0] >> 29) & 1, pTex.texInfo.tex, (num << 3) / 2, pTex.tex4x4Info.tex, (num << 3) / 4, pTex.tex4x4Info.pal, (num << 2) / 2, pTex.plttInfo.pal, (array2[0] << 3) / 2, 10497u);
							}
						}

						internal static void NNS_G3dTexReleaseTexKey(NNSG3dResTex pTex, out TexVram texKey, out TexVram tex4x4Key)
						{
							texKey = pTex.texInfo.vramKey;
							tex4x4Key = pTex.tex4x4Info.vramKey;
							pTex.texInfo.vramKey = null;
							pTex.tex4x4Info.vramKey = null;
						}

						internal static uint NNS_G3dPlttGetRequiredSize(NNSG3dResTex pTex)
						{
							return 0u;
						}

						internal static void NNS_G3dPlttSetPlttKey(NNSG3dResTex pTex, TexVram plttKey)
						{
						}

						internal static void NNS_G3dPlttLoad(NNSG3dResTex pTex, int exec_begin_end)
						{
						}

						internal static TexVram NNS_G3dPlttReleasePlttKey(NNSG3dResTex pTex)
						{
							return null;
						}

						internal static int NNS_G3dBindMdlTex(NNSG3dResMdl pMdl, NNSG3dResTex pTex)
						{
							return NNS_G3dBindMdlTexEx(pMdl, pTex, null);
						}

						internal static int NNS_G3dBindMdlTexEx(NNSG3dResMdl pMdl, NNSG3dResTex pTex, NNSG3dResName pResName)
						{
							NNSG3dResDictEntryHeader entry = pTex.dict.entry;
							NNSG3dResName[] name = entry.name;
							NNSG3dResDict dict = pTex.plttInfo.dict;
							TexVram vramKey = pTex.texInfo.vramKey;
							if (vramKey.list == null)
							{
								vramKey.size = pTex.dict.numEntry;
								vramKey.list = new TexVramList[vramKey.size];
								for (int i = 0; i < vramKey.size; i++)
								{
									vramKey.list[i] = new TexVramList();
								}
							}
							NNSG3dResMat nNSG3dResMat = NNS_G3dGetMat(pMdl);
							NNSG3dResDict dictTex = nNSG3dResMat.dictTex;
							NNSG3dResDict dictPal = nNSG3dResMat.dictPal;
							_ = nNSG3dResMat.dict.entry;
							for (int j = 0; j < pTex.dict.numEntry; j++)
							{
								if (pResName != null && !NNSG3dResNamecmp(name[j], pResName, 16))
								{
									continue;
								}
								byte[] array = (byte[])NNS_G3dGetResDataByName(dictTex, name[j]);
								if (array == null)
								{
									continue;
								}
								ushort[] array2 = new ushort[array.Length / 2];
								for (int k = 0; k < array2.Length; k++)
								{
									array2[k] = 0;
									array2[k] |= array[k * 2 + 1];
									array2[k] <<= 8;
									array2[k] |= array[k * 2];
								}
								for (int l = 0; l < array2[1]; l++)
								{
									int num = NNS_G3dGetResDictIdxByName(dictTex, name[j]);
									if (num == -1)
									{
										continue;
									}
									int num2 = nNSG3dResMat.listT[num][l];
									NNSG3dResMatData nNSG3dResMatData = nNSG3dResMat.mat[num2];
									if (vramKey.list[j].tex == 0)
									{
										uint[] array3 = new uint[entry.sizeUnit / 4];
										ArrayReader arrayReader = new ArrayReader(entry.data);
										arrayReader.setPosition(j * entry.sizeUnit);
										arrayReader.read(array3, 0, array3.Length);
										ushort[] array4 = null;
										uint num3 = (array3[0] >> 26) & 7;
										for (int m = 0; m < dictPal.numEntry; m++)
										{
											NNSG3dResDictEntryHeader entry2 = dictPal.entry;
											ushort[] array5 = new ushort[entry2.sizeUnit / 2];
											arrayReader = new ArrayReader(entry2.data);
											arrayReader.setPosition(m * entry2.sizeUnit);
											arrayReader.read(array5, 0, array5.Length);
											for (int n = 0; n < array5[1]; n++)
											{
												if (nNSG3dResMat.listP[m][n] != num2)
												{
													continue;
												}
												NNSG3dResName[] name2 = entry2.name;
												byte[] array6 = (byte[])NNS_G3dGetResDataByName(dict, name2[m]);
												if (array6 != null)
												{
													array4 = new ushort[array6.Length / 2];
													for (int num4 = 0; num4 < array4.Length; num4++)
													{
														array4[num4] = 0;
														array4[num4] |= array6[num4 * 2 + 1];
														array4[num4] <<= 8;
														array4[num4] |= array6[num4 * 2];
													}
												}
											}
										}
										if (array4 != null || num3 == 7)
										{
											int num5 = (int)(array3[0] & 0xFFFFF);
											vramKey.list[j].pTex = pTex;
											vramKey.list[j].dictTexData = array3;
											vramKey.list[j].fmt = (array3[0] >> 26) & 7;
											vramKey.list[j].tex = LoadTexture((array3[0] >> 26) & 7, (array3[0] >> 20) & 7, (array3[0] >> 23) & 7, (array3[0] >> 29) & 1, pTex.texInfo.tex, (num5 << 3) / 2, pTex.tex4x4Info.tex, (num5 << 3) / 4, pTex.tex4x4Info.pal, (num5 << 2) / 2, pTex.plttInfo.pal, ((array4 != null) ? (array4[0] << 3) : 0) / 2, 10497u);
										}
									}
									if (nNSG3dResMatData.texImageParam != null && nNSG3dResMatData.texImageParamMask == 0)
									{
										nNSG3dResMatData.texImageParam.release();
									}
									vramKey.list[j].@ref++;
									nNSG3dResMatData.texImageParam = vramKey.list[j];
									nNSG3dResMatData.texImageParamMask = 0u;
								}
							}
							return 1;
						}

						internal static void NNS_G3dReleaseMdlTex(NNSG3dResMdl pMdl)
						{
							NNS_G3dReleaseMdlTexEx(pMdl, null);
						}

						internal static int NNS_G3dReleaseMdlTexEx(NNSG3dResMdl pMdl, NNSG3dResName pResName)
						{
							NNSG3dResMat nNSG3dResMat = NNS_G3dGetMat(pMdl);
							NNSG3dResDict dictTex = nNSG3dResMat.dictTex;
							NNSG3dResDictEntryHeader entry = dictTex.entry;
							NNSG3dResName[] name = entry.name;
							_ = nNSG3dResMat.dict.entry;
							for (int i = 0; i < dictTex.numEntry; i++)
							{
								if (pResName != null && !NNSG3dResNamecmp(name[i], pResName, 16))
								{
									continue;
								}
								ushort[] array = new ushort[2]
								{
									ArrayReader.packUInt16(entry.data, i * entry.sizeUnit),
									ArrayReader.packUInt16(entry.data, i * entry.sizeUnit + 2)
								};
								for (int j = 0; j < array[1]; j++)
								{
									int num = nNSG3dResMat.listT[i][j];
									NNSG3dResMatData nNSG3dResMatData = nNSG3dResMat.mat[num];
									if (nNSG3dResMatData.texImageParam != null && nNSG3dResMatData.texImageParamMask == 0)
									{
										nNSG3dResMatData.texImageParam.release();
									}
									nNSG3dResMatData.texImageParam = null;
									nNSG3dResMatData.texImageParamMask = 0u;
								}
							}
							return 1;
						}

						internal static int NNS_G3dBindMdlPltt(NNSG3dResMdl pMdl, NNSG3dResTex pTex)
						{
							NNSG3dResDict dict = pTex.plttInfo.dict;
							NNSG3dResDictEntryHeader entry = dict.entry;
							NNSG3dResName[] name = entry.name;
							NNSG3dResMat nNSG3dResMat = NNS_G3dGetMat(pMdl);
							_ = nNSG3dResMat.dict.entry;
							NNSG3dResDict dictPal = nNSG3dResMat.dictPal;
							for (int i = 0; i < dict.numEntry; i++)
							{
								byte[] array = (byte[])NNS_G3dGetResDataByName(dictPal, name[i]);
								if (array == null)
								{
									continue;
								}
								ushort[] array2 = new ushort[array.Length / 2];
								for (int j = 0; j < array2.Length; j++)
								{
									array2[j] = 0;
									array2[j] |= array[j * 2 + 1];
									array2[j] <<= 8;
									array2[j] |= array[j * 2];
								}
								for (int k = 0; k < array2[1]; k++)
								{
									int num = nNSG3dResMat.listP[i][k];
									NNSG3dResMatData nNSG3dResMatData = nNSG3dResMat.mat[num];
									if (nNSG3dResMatData.texImageParam != null && nNSG3dResMatData.texImageParamMask == 0)
									{
										TexVramList texImageParam = nNSG3dResMatData.texImageParam;
										NNSG3dResTex pTex2 = texImageParam.pTex;
										ushort[] array3 = new ushort[entry.sizeUnit / 2];
										ArrayReader arrayReader = new ArrayReader(entry.data);
										arrayReader.setPosition(i * entry.sizeUnit);
										arrayReader.read(array3, 0, array3.Length);
										uint[] dictTexData = texImageParam.dictTexData;
										int num2 = (int)(dictTexData[0] & 0xFFFFF);
										if (texImageParam.tex != 0)
										{
											DeleteTexture(texImageParam.tex);
											texCount--;
										}
										texImageParam.tex = 0u;
										texImageParam.tex = LoadTexture((dictTexData[0] >> 26) & 7, (dictTexData[0] >> 20) & 7, (dictTexData[0] >> 23) & 7, (dictTexData[0] >> 29) & 1, pTex2.texInfo.tex, (num2 << 3) / 2, pTex2.tex4x4Info.tex, (num2 << 3) / 4, pTex2.tex4x4Info.pal, (num2 << 2) / 2, pTex.plttInfo.pal, (array3[0] << 3) / 2, 10497u);
									}
								}
							}
							return 1;
						}

						internal static int NNS_G3dBindMdlPlttEx(NNSG3dResMdl pMdl, NNSG3dResTex pTex, NNSG3dResName pResName)
						{
							return 0;
						}

						internal static void NNS_G3dReleaseMdlPltt(NNSG3dResMdl pMdl)
						{
						}

						internal static int NNS_G3dReleaseMdlPlttEx(NNSG3dResMdl pMdl, NNSG3dResName pResName)
						{
							return 0;
						}

						internal static int NNS_G3dBindMdlSet(NNSG3dResMdlSet pMdlSet, NNSG3dResTex pTex)
						{
							for (int i = 0; i < pMdlSet.dict.numEntry; i++)
							{
								NNS_G3dBindMdlTex(NNS_G3dGetMdlByIdx(pMdlSet, (uint)i), pTex);
							}
							return 1;
						}

						internal static void NNS_G3dReleaseMdlSet(NNSG3dResMdlSet pMdlSet)
						{
							for (int i = 0; i < pMdlSet.dict.numEntry; i++)
							{
								NNS_G3dReleaseMdlTex(NNS_G3dGetMdlByIdx(pMdlSet, (uint)i));
							}
						}

						internal static void transVertex(Vertex v0, Vertex v1, Vertex v2, float[] p, float[] n, float[] t, byte[] c, ref int d)
						{
							int num = d * 3;
							int num2 = d * 2;
							int num3 = d * 4;
							p[num++] = v0.pos0;
							p[num++] = v0.pos1;
							p[num++] = v0.pos2;
							t[num2++] = v0.tex0;
							t[num2++] = v0.tex1;
							c[num3++] = v0.clr0;
							c[num3++] = v0.clr1;
							c[num3++] = v0.clr2;
							c[num3++] = v0.clr3;
							p[num++] = v1.pos0;
							p[num++] = v1.pos1;
							p[num++] = v1.pos2;
							t[num2++] = v1.tex0;
							t[num2++] = v1.tex1;
							c[num3++] = v1.clr0;
							c[num3++] = v1.clr1;
							c[num3++] = v1.clr2;
							c[num3++] = v1.clr3;
							p[num++] = v2.pos0;
							p[num++] = v2.pos1;
							p[num++] = v2.pos2;
							t[num2++] = v2.tex0;
							t[num2++] = v2.tex1;
							c[num3++] = v2.clr0;
							c[num3++] = v2.clr1;
							c[num3++] = v2.clr2;
							c[num3++] = v2.clr3;
							d += 3;
						}

						internal static void transVertex(Vertex v0, Vertex v1, Vertex v2, VertexPositionColorTexture[] ptc, float[] n, ref int d)
						{
							ptc[d].Position.X = v0.pos0;
							ptc[d].Position.Y = v0.pos1;
							ptc[d].Position.Z = v0.pos2;
							ptc[d].TextureCoordinate.X = v0.tex0;
							ptc[d].TextureCoordinate.Y = v0.tex1;
							ptc[d].Color.R = v0.clr0;
							ptc[d].Color.G = v0.clr1;
							ptc[d].Color.B = v0.clr2;
							ptc[d].Color.A = v0.clr3;
							d++;
							ptc[d].Position.X = v1.pos0;
							ptc[d].Position.Y = v1.pos1;
							ptc[d].Position.Z = v1.pos2;
							ptc[d].TextureCoordinate.X = v1.tex0;
							ptc[d].TextureCoordinate.Y = v1.tex1;
							ptc[d].Color.R = v1.clr0;
							ptc[d].Color.G = v1.clr1;
							ptc[d].Color.B = v1.clr2;
							ptc[d].Color.A = v1.clr3;
							d++;
							ptc[d].Position.X = v2.pos0;
							ptc[d].Position.Y = v2.pos1;
							ptc[d].Position.Z = v2.pos2;
							ptc[d].TextureCoordinate.X = v2.tex0;
							ptc[d].TextureCoordinate.Y = v2.tex1;
							ptc[d].Color.R = v2.clr0;
							ptc[d].Color.G = v2.clr1;
							ptc[d].Color.B = v2.clr2;
							ptc[d].Color.A = v2.clr3;
							d++;
						}

						internal static int checkComponent(Matrix mtx)
						{
							int num = 0;
							if (mtx.M11 != 1f || mtx.M22 != 1f || mtx.M33 != 1f)
							{
								num |= 1;
							}
							if (mtx.M12 != 0f || mtx.M13 != 0f || mtx.M21 != 0f || mtx.M23 != 0f || mtx.M31 != 0f || mtx.M32 != 0f)
							{
								num |= 2;
							}
							if (mtx.M41 != 0f || mtx.M42 != 0f || mtx.M43 != 0f)
							{
								num |= 4;
							}
							return num;
						}

						internal static void convertMatrix(ref Matrix dst, MtxFx43 src)
						{
							dst.M11 = FX_FX32_TO_F32(src.a[0]);
							dst.M12 = FX_FX32_TO_F32(src.a[1]);
							dst.M13 = FX_FX32_TO_F32(src.a[2]);
							dst.M14 = 0f;
							dst.M21 = FX_FX32_TO_F32(src.a[3]);
							dst.M22 = FX_FX32_TO_F32(src.a[4]);
							dst.M23 = FX_FX32_TO_F32(src.a[5]);
							dst.M24 = 0f;
							dst.M31 = FX_FX32_TO_F32(src.a[6]);
							dst.M32 = FX_FX32_TO_F32(src.a[7]);
							dst.M33 = FX_FX32_TO_F32(src.a[8]);
							dst.M34 = 0f;
							dst.M41 = FX_FX32_TO_F32(src.a[9]);
							dst.M42 = FX_FX32_TO_F32(src.a[10]);
							dst.M43 = FX_FX32_TO_F32(src.a[11]);
							dst.M44 = 1f;
						}

						internal static void DrawModel(NNSG3dRenderObj pRenderObj, NNSG3dResMdl mdl)
						{
							if (enable3D == 0)
							{
								return;
							}
							NNSG3dRS nNSG3dRS = fnd_reuse_rs;
							nNSG3dRS.pRenderObj = pRenderObj;
							NNSG3dResMat nNSG3dResMat = NNS_G3dGetMat(mdl);
							NNSG3dResDictEntryHeader entry = nNSG3dResMat.dict.entry;
							byte[] sbc = mdl.sbc;
							int num = 0;
							for (int i = 0; 4096 << i < mdl.info.posScale; i++)
							{
							}
							_ = fnd_reuse_pos;
							float tex = 0f;
							float tex2 = 0f;
							byte clr = byte.MaxValue;
							byte clr2 = byte.MaxValue;
							byte clr3 = byte.MaxValue;
							byte clr4 = byte.MaxValue;
							int num2 = 0;
							MtxFx43 mtxFx = fnd_reuse_rootMtx;
							MtxFx43 mtxFx2 = fnd_reuse_currentMtxN;
							mtxFx.copy(currentMtx);
							mtxFx2.copy(currentMtx);
							int num3 = 1;
							MtxFx33 mtxFx3 = fnd_reuse_bbMtx;
							MtxFx33 mtxFx4 = fnd_reuse_bbyMtx;
							int num4 = 0;
							int num5 = 0;
							if (skipFrame == 0)
							{
								_ = mdl.info.numTriangle;
								_ = mdl.info.numQuad;
								glEnableClientState(32884u);
								glEnableClientState(32888u);
								glEnableClientState(32886u);
								glEnable(3553u);
								glEnable(2884u);
								glCullFace(1029u);
							}
							int num6 = 1;
							int num7 = 1;
							uint num8 = 1u;
							int num9 = 2;
							int j;
							for (j = 0; j < nNSG3dResMat.dict.numEntry; j++)
							{
								if (skipFrame != 0)
								{
									break;
								}
								NNSG3dResMatData nNSG3dResMatData = nNSG3dResMat.mat[j];
								NNSG3dResName name = entry.name[j];
								texMtx[j].M11 = 1f / (float)(int)nNSG3dResMatData.origWidth;
								texMtx[j].M22 = 1f / (float)(int)nNSG3dResMatData.origHeight;
								texMtx[j].M33 = (texMtx[j].M44 = 1f);
								texMtx[j].M12 = (texMtx[j].M21 = (texMtx[j].M41 = (texMtx[j].M42 = 0f)));
								for (NNSG3dAnmObj nNSG3dAnmObj = pRenderObj?.anmMat; nNSG3dAnmObj != null; nNSG3dAnmObj = nNSG3dAnmObj.next)
								{
									ushort category = nNSG3dAnmObj.resAnm.category1;
									if (category == 21569)
									{
										NNSG3dResTexSRTAnm nNSG3dResTexSRTAnm = (NNSG3dResTexSRTAnm)nNSG3dAnmObj.resAnm.m_Anm;
										int num10 = NNS_G3dGetResDictIdxByName(nNSG3dResTexSRTAnm.dict, name);
										if (num10 == -1)
										{
											continue;
										}
										NNSG3dResTexSRTAnm.NNSG3dResTexSRTAnmP nNSG3dResTexSRTAnmP = nNSG3dResTexSRTAnm.p[num10];
										if (nNSG3dResTexSRTAnmP == null)
										{
											continue;
										}
										int num11 = MATH_MIN(nNSG3dAnmObj.frame >> 12, nNSG3dResTexSRTAnm.numFrame - 1);
										int[] array = fnd_reuse_m_ta;
										for (int k = 0; k < 5; k++)
										{
											uint num12 = nNSG3dResTexSRTAnmP.flag[k];
											uint num13 = nNSG3dResTexSRTAnmP.ex[k];
											if ((num12 & 0x20000000) != 0)
											{
												array[k] = (int)num13;
												continue;
											}
											int num14 = num11 >> (int)(num12 >> 30);
											array[k] = (((num12 & 0x10000000) != 0) ? nNSG3dResTexSRTAnmP.table16[k][num14] : nNSG3dResTexSRTAnmP.table32[k][num14]);
										}
										float num15 = FX_FX32_TO_F32((short)(array[2] >> 16));
										float num16 = FX_FX32_TO_F32((short)array[2]);
										float num17 = texMtx[j].M11 * FX_FX32_TO_F32(array[0]);
										float num18 = texMtx[j].M22 * FX_FX32_TO_F32(array[1]);
										texMtx[j].M11 = num15 * num17;
										texMtx[j].M12 = num16 * num17;
										texMtx[j].M21 = (0f - num16) * num18;
										texMtx[j].M22 = num15 * num18;
										texMtx[j].M41 = 0f - FX_FX32_TO_F32(array[3]) - (texMtx[j].M11 * (float)(int)nNSG3dResMatData.origWidth + texMtx[j].M21 * (float)(int)nNSG3dResMatData.origHeight) * 0.5f + 0.5f;
										texMtx[j].M42 = FX_FX32_TO_F32(array[4]) - (texMtx[j].M12 * (float)(int)nNSG3dResMatData.origWidth + texMtx[j].M22 * (float)(int)nNSG3dResMatData.origHeight) * 0.5f + 0.5f;
									}
									if (category != 19777)
									{
										continue;
									}
									NNSG3dResMatCAnm nNSG3dResMatCAnm = (NNSG3dResMatCAnm)nNSG3dAnmObj.resAnm.m_Anm;
									int num19 = NNS_G3dGetResDictIdxByName(nNSG3dResMatCAnm.dict, name);
									if (num19 == -1)
									{
										continue;
									}
									NNSG3dResMatCAnm.NNSG3dResMatCAnmP nNSG3dResMatCAnmP = nNSG3dResMatCAnm.p[num19];
									if (nNSG3dResMatCAnmP == null)
									{
										continue;
									}
									int num20 = MATH_MIN(nNSG3dAnmObj.frame >> 12, nNSG3dResMatCAnm.numFrame - 1);
									ushort[] array2 = fnd_reuse_m_ma;
									for (int k = 0; k < 5; k++)
									{
										uint num21 = nNSG3dResMatCAnmP.flag[k];
										if ((num21 & 0x20000000) != 0)
										{
											array2[k] = (ushort)num21;
											continue;
										}
										int num22 = num20 >> (int)(num21 >> 30);
										array2[k] = ((k == 4) ? nNSG3dResMatCAnmP.table8[k][num22] : nNSG3dResMatCAnmP.table16[k][num22]);
									}
									nNSG3dResMatData.polyAttr = (uint)((nNSG3dResMatData.polyAttr & -2031617) | (array2[4] << 16));
								}
							}
							int d;
							j = (d = 0);
							while (true)
							{
								switch (sbc[num] & 0x1F)
								{
								case 0:
									num++;
									break;
								case 2:
									num3 = (((sbc[num + 2] & 1) != 0) ? 1 : 0);
									num += 3;
									break;
								case 9:
								{
									int num103 = sbc[num + 1];
									int num104 = sbc[num + 2];
									num += 3;
									MtxFx43 mtxFx9 = fnd_reuse_m;
									mtxFx9.setDefault();
									for (int k = 0; k < num104; k++)
									{
										int num105 = sbc[num];
										int num106 = sbc[num + 1];
										int num107 = sbc[num + 2];
										MtxFx43 a = mdl.mtx[num106];
										MtxFx43 mtxFx10 = fnd_reuse_m2;
										MTX_Concat43(a, stackMtx[num105], mtxFx10);
										for (int n = 0; n < 12; n++)
										{
											mtxFx9.a[n] += mtxFx10.a[n] * num107;
										}
										num += 3;
									}
									for (int num108 = 0; num108 < 12; num108++)
									{
										stackMtx[num103].a[num108] = mtxFx9.a[num108] >> 8;
									}
									convertMatrix(ref _matrix_stack[num103], stackMtx[num103]);
									currentMtx.copy(stackMtx[num103]);
									mtxFx2.copy(stackMtx[num103]);
									stackMtxN[num103].copy(stackMtx[num103]);
									break;
								}
								case 3:
									currentMtx.copy(stackMtx[sbc[num + 1]]);
									mtxFx2.copy(stackMtxN[sbc[num + 1]]);
									num += 2;
									break;
								case 6:
								{
									int num60 = sbc[num] & 0xE0;
									int num61 = sbc[num + 1];
									int num62 = sbc[num + 3];
									nNSG3dRS.currentNode = (byte)num61;
									MtxFx43 mtxFx6 = fnd_reuse_baseMtx;
									MTX_Identity43(mtxFx6);
									int[] array5 = fnd_reuse_baseScale;
									array5[0] = 4096;
									array5[1] = 4096;
									array5[2] = 4096;
									NNSG3dResNodeInfo nodeInfo = mdl.nodeInfo;
									NNSG3dResDict dict = nodeInfo.dict;
									_ = dict.entry;
									uint[] array6 = nodeInfo.p[num61];
									int num63 = 0;
									ushort num64 = (ushort)(array6[num63] & 0xFFFF);
									mtxFx6._00 = (short)((array6[num63] >> 16) & 0xFFFF);
									num63++;
									if ((num64 & 1) == 0)
									{
										mtxFx6._30 = (int)array6[num63];
										mtxFx6._31 = (int)array6[num63 + 1];
										mtxFx6._32 = (int)array6[num63 + 2];
										num63 += 3;
									}
									if ((num64 & 2) == 0)
									{
										if ((num64 & 8) == 0)
										{
											mtxFx6._01 = (short)(array6[num63] & 0xFFFF);
											mtxFx6._02 = (short)((array6[num63] >> 16) & 0xFFFF);
											mtxFx6._10 = (short)(array6[num63 + 1] & 0xFFFF);
											mtxFx6._11 = (short)((array6[num63 + 1] >> 16) & 0xFFFF);
											mtxFx6._12 = (short)(array6[num63 + 2] & 0xFFFF);
											mtxFx6._20 = (short)((array6[num63 + 2] >> 16) & 0xFFFF);
											mtxFx6._21 = (short)(array6[num63 + 3] & 0xFFFF);
											mtxFx6._22 = (short)((array6[num63 + 3] >> 16) & 0xFFFF);
											num63 += 4;
										}
										else
										{
											int num65 = (short)(array6[num63] & 0xFFFF);
											int num66 = (short)((array6[num63] >> 16) & 0xFFFF);
											int num67 = (((num64 & 0x200) != 0) ? (-num66) : num66);
											int num68 = (((num64 & 0x400) != 0) ? (-num65) : num65);
											int num69 = (((num64 & 0x100) != 0) ? (-4096) : 4096);
											int num70 = (mtxFx6._22 = 0);
											num70 = (mtxFx6._11 = num70);
											mtxFx6._00 = num70;
											switch ((num64 >> 4) & 0xF)
											{
											case 0:
												mtxFx6._00 = num69;
												mtxFx6._11 = num65;
												mtxFx6._12 = num66;
												mtxFx6._21 = num67;
												mtxFx6._22 = num68;
												break;
											case 1:
												mtxFx6._01 = num69;
												mtxFx6._10 = num65;
												mtxFx6._12 = num66;
												mtxFx6._20 = num67;
												mtxFx6._22 = num68;
												break;
											case 2:
												mtxFx6._02 = num69;
												mtxFx6._10 = num65;
												mtxFx6._11 = num66;
												mtxFx6._20 = num67;
												mtxFx6._21 = num68;
												break;
											case 3:
												mtxFx6._10 = num69;
												mtxFx6._01 = num65;
												mtxFx6._02 = num66;
												mtxFx6._21 = num67;
												mtxFx6._22 = num68;
												break;
											case 4:
												mtxFx6._11 = num69;
												mtxFx6._00 = num65;
												mtxFx6._02 = num66;
												mtxFx6._20 = num67;
												mtxFx6._22 = num68;
												break;
											case 5:
												mtxFx6._12 = num69;
												mtxFx6._00 = num65;
												mtxFx6._01 = num66;
												mtxFx6._20 = num67;
												mtxFx6._21 = num68;
												break;
											case 6:
												mtxFx6._20 = num69;
												mtxFx6._01 = num65;
												mtxFx6._02 = num66;
												mtxFx6._11 = num67;
												mtxFx6._12 = num68;
												break;
											case 7:
												mtxFx6._21 = num69;
												mtxFx6._00 = num65;
												mtxFx6._02 = num66;
												mtxFx6._10 = num67;
												mtxFx6._12 = num68;
												break;
											case 8:
												mtxFx6._22 = num69;
												mtxFx6._00 = num65;
												mtxFx6._01 = num66;
												mtxFx6._10 = num67;
												mtxFx6._11 = num68;
												break;
											}
											num63++;
										}
									}
									if ((num64 & 4) == 0)
									{
										array5[0] = (int)array6[num63];
										array5[1] = (int)array6[num63 + 1];
										array5[2] = (int)array6[num63 + 2];
										num63 += 6;
									}
									int[] array7 = fnd_reuse__scale;
									MtxFx43 mtxFx7 = fnd_reuse_mtx;
									array7[0] = 0;
									array7[1] = 0;
									array7[2] = 0;
									mtxFx7.setDefault();
									int num73 = 4096;
									for (NNSG3dAnmObj nNSG3dAnmObj2 = pRenderObj?.anmJnt; nNSG3dAnmObj2 != null; nNSG3dAnmObj2 = nNSG3dAnmObj2.next)
									{
										NNSG3dResJntAnm nNSG3dResJntAnm = (NNSG3dResJntAnm)nNSG3dAnmObj2.resAnm.m_Anm;
										if (nNSG3dResJntAnm.anmHeader.category1 == 17217)
										{
											MtxFx43 mtxFx8 = fnd_reuse_anmMtx;
											int[] array8 = fnd_reuse_anmScale;
											mtxFx8.copy(mtxFx6);
											array8[0] = array5[0];
											array8[1] = array5[1];
											array8[2] = array5[2];
											int num74 = MATH_MIN(nNSG3dAnmObj2.frame >> 12, nNSG3dResJntAnm.numFrame - 1);
											NNSG3dResJntAnm.NNSG3dResJntAnmP nNSG3dResJntAnmP = nNSG3dResJntAnm.p[num61];
											int num75 = 0;
											uint flag = nNSG3dResJntAnmP.flag;
											if ((flag & 1) == 0)
											{
												if ((flag & 2) != 0)
												{
													int num70 = (mtxFx8._32 = 0);
													num70 = (mtxFx8._31 = num70);
													mtxFx8._30 = num70;
												}
												else if ((flag & 4) == 0)
												{
													for (int k = 0; k < 3; k++)
													{
														if ((flag & (uint)(8 << k)) != 0)
														{
															mtxFx8.a[9 + k] = (int)nNSG3dResJntAnmP.p[num75];
															num75++;
															continue;
														}
														uint num78 = nNSG3dResJntAnmP.p[num75];
														short[] array9 = nNSG3dResJntAnmP.tableT16[k];
														int[] array10 = nNSG3dResJntAnmP.tableT32[k];
														int num79 = num74 >> (int)(num78 >> 30);
														mtxFx8.a[9 + k] = (((num78 & 0x20000000) != 0) ? array9[num79] : array10[num79]);
														num75 += 2;
													}
												}
												if ((flag & 0x40) != 0)
												{
													int num70 = (mtxFx8._22 = 4096);
													num70 = (mtxFx8._11 = num70);
													mtxFx8._00 = num70;
													num70 = (mtxFx8._21 = 0);
													num70 = (mtxFx8._20 = num70);
													num70 = (mtxFx8._12 = num70);
													num70 = (mtxFx8._10 = num70);
													num70 = (mtxFx8._02 = num70);
													mtxFx8._01 = num70;
												}
												else if ((flag & 0x80) == 0)
												{
													ushort num87;
													if ((flag & 0x100) != 0)
													{
														num87 = (ushort)nNSG3dResJntAnmP.p[num75];
														num75++;
													}
													else
													{
														uint num88 = nNSG3dResJntAnmP.p[num75];
														ushort[] tableR = nNSG3dResJntAnmP.tableR;
														num87 = tableR[num74 >> (int)(num88 >> 30)];
														num75 += 2;
													}
													if ((num87 & 0x8000) != 0)
													{
														short[] rot = nNSG3dResJntAnm.rot3;
														int num89 = (num87 & 0x7FFF) * 6 / 2;
														short num90 = rot[num89];
														int num91 = rot[num89 + 1];
														int num92 = rot[num89 + 2];
														int num93 = (((num90 & 0x20) != 0) ? (-num92) : num92);
														int num94 = (((num90 & 0x40) != 0) ? (-num91) : num91);
														int num95 = (((num90 & 0x10) != 0) ? (-4096) : 4096);
														mtxFx8._00 = 0;
														mtxFx8._01 = 0;
														mtxFx8._02 = 0;
														mtxFx8._10 = 0;
														mtxFx8._11 = 0;
														mtxFx8._12 = 0;
														mtxFx8._20 = 0;
														mtxFx8._21 = 0;
														mtxFx8._22 = 0;
														switch (num90 & 0xF)
														{
														case 0:
															mtxFx8._00 = num95;
															mtxFx8._11 = num91;
															mtxFx8._12 = num92;
															mtxFx8._21 = num93;
															mtxFx8._22 = num94;
															break;
														case 1:
															mtxFx8._01 = num95;
															mtxFx8._10 = num91;
															mtxFx8._12 = num92;
															mtxFx8._20 = num93;
															mtxFx8._22 = num94;
															break;
														case 2:
															mtxFx8._02 = num95;
															mtxFx8._10 = num91;
															mtxFx8._11 = num92;
															mtxFx8._20 = num93;
															mtxFx8._21 = num94;
															break;
														case 3:
															mtxFx8._10 = num95;
															mtxFx8._01 = num91;
															mtxFx8._02 = num92;
															mtxFx8._21 = num93;
															mtxFx8._22 = num94;
															break;
														case 4:
															mtxFx8._11 = num95;
															mtxFx8._00 = num91;
															mtxFx8._02 = num92;
															mtxFx8._20 = num93;
															mtxFx8._22 = num94;
															break;
														case 5:
															mtxFx8._12 = num95;
															mtxFx8._00 = num91;
															mtxFx8._01 = num92;
															mtxFx8._20 = num93;
															mtxFx8._21 = num94;
															break;
														case 6:
															mtxFx8._20 = num95;
															mtxFx8._01 = num91;
															mtxFx8._02 = num92;
															mtxFx8._11 = num93;
															mtxFx8._12 = num94;
															break;
														case 7:
															mtxFx8._21 = num95;
															mtxFx8._00 = num91;
															mtxFx8._02 = num92;
															mtxFx8._10 = num93;
															mtxFx8._12 = num94;
															break;
														case 8:
															mtxFx8._22 = num95;
															mtxFx8._00 = num91;
															mtxFx8._01 = num92;
															mtxFx8._10 = num93;
															mtxFx8._11 = num94;
															break;
														}
													}
													else
													{
														short[] rot2 = nNSG3dResJntAnm.rot5;
														int num96 = (num87 & 0x7FFF) * 10 / 2;
														mtxFx8._00 = rot2[num96] >> 3;
														mtxFx8._01 = rot2[num96 + 1] >> 3;
														mtxFx8._02 = rot2[num96 + 2] >> 3;
														mtxFx8._10 = rot2[num96 + 3] >> 3;
														mtxFx8._11 = rot2[num96 + 4] >> 3;
														mtxFx8._12 = (short)(((rot2[num96] & 7) << 9) | ((rot2[num96 + 1] & 7) << 6) | ((rot2[num96 + 2] & 7) << 3) | (rot2[num96 + 3] & 7) | ((rot2[num96 + 4] & 1) * 61440));
														_vec32_0.x = mtxFx8._00;
														_vec32_0.y = mtxFx8._01;
														_vec32_0.z = mtxFx8._02;
														_vec32_1.x = mtxFx8._10;
														_vec32_1.y = mtxFx8._11;
														_vec32_1.z = mtxFx8._12;
														_vec32_2.x = mtxFx8._20;
														_vec32_2.y = mtxFx8._21;
														_vec32_2.z = mtxFx8._22;
														VEC_CrossProduct(_vec32_0, _vec32_1, _vec32_2);
														mtxFx8._20 = _vec32_2.x;
														mtxFx8._21 = _vec32_2.y;
														mtxFx8._22 = _vec32_2.z;
													}
												}
												if ((flag & 0x200) != 0)
												{
													array8[0] = (array8[1] = (array8[2] = 4096));
												}
												else if ((flag & 0x400) == 0)
												{
													for (int k = 0; k < 3; k++)
													{
														if ((flag & (uint)(2048 << k)) != 0)
														{
															array8[k] = (int)nNSG3dResJntAnmP.p[num75];
															num75 += 2;
															continue;
														}
														uint num97 = nNSG3dResJntAnmP.p[num75];
														short[] array11 = nNSG3dResJntAnmP.tableS16[k];
														int[] array12 = nNSG3dResJntAnmP.tableS32[k];
														int num98 = num74 >> (int)(num97 >> 30);
														array8[k] = (((num97 & 0x20000000) != 0) ? array11[num98 * 2] : array12[num98 * 2]);
														num75 += 2;
													}
												}
											}
											else
											{
												array8[0] = (array8[1] = (array8[2] = 4096));
												MTX_Identity43(mtxFx8);
											}
											int num99 = ((nNSG3dAnmObj2.next == null) ? num73 : nNSG3dAnmObj2.ratio);
											for (int k = 0; k < 3; k++)
											{
												array7[k] += FX_Mul(array8[k], num99);
											}
											for (int k = 0; k < 12; k++)
											{
												mtxFx7.a[k] += FX_Mul(mtxFx8.a[k], num99);
											}
											num73 -= num99;
										}
									}
									if (num73 != 0)
									{
										for (int k = 0; k < 3; k++)
										{
											array7[k] += FX_Mul(array5[k], num73);
										}
										for (int k = 0; k < 12; k++)
										{
											mtxFx7.a[k] += FX_Mul(mtxFx6.a[k], num73);
										}
									}
									num += 4;
									int num100 = 0;
									if ((num60 & 0x20) != 0)
									{
										num100 = sbc[num++];
									}
									if ((num60 & 0x40) != 0)
									{
										if (sbc[num] < 64)
										{
											currentMtx.copy(stackMtx[sbc[num]]);
										}
										if (sbc[num] < 64)
										{
											mtxFx2.copy(stackMtxN[sbc[num]]);
										}
										num++;
									}
									MTX_Concat43(mtxFx7, mtxFx2, mtxFx2);
									if ((num62 & 1) != 0)
									{
										currentMtx.copy(mtxFx2);
									}
									else
									{
										MTX_Concat43(mtxFx7, currentMtx, currentMtx);
									}
									MTX_ScaleApply43(currentMtx, currentMtx, array7[0], array7[1], array7[2]);
									if ((num60 & 0x20) != 0)
									{
										stackMtx[num100].copy(currentMtx);
										stackMtxN[num100].copy(mtxFx2);
										convertMatrix(ref _matrix_stack[num100], stackMtx[num100]);
									}
									if (pRenderObj != null && pRenderObj.cbCmd == 6)
									{
										pRenderObj.cbFunc(nNSG3dRS);
									}
									break;
								}
								case 11:
									num++;
									break;
								case 7:
								{
									int num101 = sbc[num] & 0xE0;
									num += 2;
									int num102 = 0;
									if ((num101 & 0x20) != 0)
									{
										num102 = sbc[num++];
									}
									if ((num101 & 0x40) != 0)
									{
										currentMtx.copy(stackMtx[sbc[num++]]);
									}
									if (num4 == 0)
									{
										_mtx33.copy(currentMtx2);
										MTX_Inverse33(_mtx33, mtxFx3);
										num4 = 1;
									}
									_mtx33.copy(currentMtx);
									MTX_Concat33(_mtx33, mtxFx3, _mtx33);
									currentMtx.copy(_mtx33);
									mtxFx2.copy(currentMtx);
									if ((num101 & 0x20) != 0)
									{
										stackMtx[num102].copy(currentMtx);
										stackMtxN[num102].copy(currentMtx);
										convertMatrix(ref _matrix_stack[num102], stackMtx[num102]);
									}
									break;
								}
								case 8:
								{
									int num58 = sbc[num] & 0xE0;
									num += 2;
									int num59 = 0;
									if ((num58 & 0x20) != 0)
									{
										num59 = sbc[num++];
									}
									if ((num58 & 0x40) != 0)
									{
										currentMtx.copy(stackMtx[sbc[num++]]);
									}
									if (num5 == 0)
									{
										VecFx32 vecFx = nitro_reuse_x;
										VecFx32 vecFx2 = nitro_reuse_y;
										VecFx32 vecFx3 = nitro_reuse_z;
										vecFx2.x = 0;
										vecFx2.y = 4096;
										vecFx2.z = 0;
										_vec32_2.x = NNS_G3dGlb.invCameraMtx._20;
										_vec32_2.y = NNS_G3dGlb.invCameraMtx._21;
										_vec32_2.z = NNS_G3dGlb.invCameraMtx._22;
										VEC_CrossProduct(vecFx2, _vec32_2, vecFx);
										VEC_Normalize(vecFx, vecFx);
										VEC_CrossProduct(vecFx, vecFx2, vecFx3);
										VEC_Normalize(vecFx3, vecFx3);
										MtxFx33 mtxFx5 = nitro_reuse_mtx33Src;
										mtxFx5._00 = vecFx.x;
										mtxFx5._01 = vecFx.y;
										mtxFx5._02 = vecFx.z;
										mtxFx5._10 = vecFx2.x;
										mtxFx5._11 = vecFx2.y;
										mtxFx5._12 = vecFx2.z;
										mtxFx5._20 = vecFx3.x;
										mtxFx5._21 = vecFx3.y;
										mtxFx5._22 = vecFx3.z;
										_mtx33.copy(NNS_G3dGlb.invCameraMtx);
										MTX_Concat33(_mtx33, mtxFx5, mtxFx5);
										_mtx33.copy(NNS_G3dGlb.cameraMtx);
										MTX_Concat33(mtxFx5, _mtx33, mtxFx4);
										num5 = 1;
									}
									_mtx33.copy(currentMtx);
									MTX_Concat33(_mtx33, mtxFx4, _mtx33);
									currentMtx.copy(_mtx33);
									mtxFx2.copy(currentMtx);
									if ((num58 & 0x20) != 0)
									{
										stackMtx[num59].copy(currentMtx);
										stackMtxN[num59].copy(currentMtx);
										convertMatrix(ref _matrix_stack[num59], stackMtx[num59]);
									}
									break;
								}
								case 4:
								{
									j = sbc[num + 1];
									num += 2;
									NNSG3dResMatData nNSG3dResMatData5 = nNSG3dResMat.mat[j];
									ushort num109 = (ushort)nNSG3dResMatData5.diffAmb;
									int num110 = (int)((nNSG3dResMatData5.polyAttr >> 16) & 0x1F);
									clr = (byte)(num109 << 3);
									clr2 = (byte)(num109 >> 5 << 3);
									clr3 = (byte)(num109 >> 10 << 3);
									clr4 = (byte)((num110 != 31) ? ((uint)(num110 << 3)) : 255u);
									if ((nNSG3dResMatData5.polyAttr & 0x30) >> 4 == 3)
									{
										clr = (clr2 = (clr3 = 0));
									}
									break;
								}
								case 5:
								{
									int num43 = sbc[num + 1];
									num += 2;
									if (num3 == 0 || skipFrame != 0)
									{
										break;
									}
									NNSG3dResMatData nNSG3dResMatData4 = nNSG3dResMat.mat[j];
									TexVramList texImageParam2 = nNSG3dResMatData4.texImageParam;
									int num44 = (int)((nNSG3dResMatData4.polyAttr >> 16) & 0x1F);
									if (nNSG3dResMatData4.texImageParamMask != 0 && texImageParam2 != null)
									{
										break;
									}
									int num45 = ((num44 < 31 || (texImageParam2 != null && (texImageParam2.fmt == 1 || texImageParam2.fmt == 6))) ? 1 : 0);
									if ((drawMask & ((num45 != 0) ? 1 : 2)) != 0 || num44 == 0)
									{
										break;
									}
									NNSG3dResShp shp = mdl.shp;
									_ = shp.dict.entry;
									NNSG3dResShpData nNSG3dResShpData = shp.shp[num43];
									int num46 = 0;
									uint[] cmd = nNSG3dResShpData.cmd;
									uint num47 = cmd[num46];
									int num48 = num46 + 1;
									int num49 = (int)(num46 + nNSG3dResShpData.sizeDL / 4);
									int k = 0;
									uint num50 = 0u;
									int num51 = 0;
									int num52 = d;
									int num53 = 0;
									int num54 = 0;
									int num55 = 0;
									byte[] cmd_color_r = nNSG3dResShpData.cmd_color_r;
									byte[] cmd_color_g = nNSG3dResShpData.cmd_color_g;
									byte[] cmd_color_b = nNSG3dResShpData.cmd_color_b;
									float[] cmd_coord_u = nNSG3dResShpData.cmd_coord_u;
									float[] cmd_coord_v = nNSG3dResShpData.cmd_coord_v;
									float[] cmd_vertex_x = nNSG3dResShpData.cmd_vertex_x;
									float[] cmd_vertex_y = nNSG3dResShpData.cmd_vertex_y;
									float[] cmd_vertex_z = nNSG3dResShpData.cmd_vertex_z;
									convertMatrix(ref _matrix_current, currentMtx);
									int num56 = checkComponent(_matrix_current);
									while (num46 < num49)
									{
										switch (num47 & 0xFF)
										{
										case 20u:
											_matrix_current = _matrix_stack[cmd[num48]];
											num56 = checkComponent(_matrix_current);
											num48++;
											break;
										case 27u:
											num48 += 3;
											break;
										case 32u:
											clr = cmd_color_r[num53];
											clr2 = cmd_color_g[num53];
											clr3 = cmd_color_b[num53];
											num53++;
											num48++;
											break;
										case 33u:
											num48++;
											break;
										case 34u:
											tex = cmd_coord_u[num54];
											tex2 = cmd_coord_v[num54];
											num54++;
											if (texMtx[j].M12 != 0f || texMtx[j].M21 != 0f)
											{
												float pos = tex;
												float pos2 = tex2;
												tex = pos * texMtx[j].M11 + pos2 * texMtx[j].M21 + texMtx[j].M41;
												tex2 = pos * texMtx[j].M12 + pos2 * texMtx[j].M22 + texMtx[j].M42;
											}
											else
											{
												if (texMtx[j].M11 != 1f || texMtx[j].M22 != 1f)
												{
													tex *= texMtx[j].M11;
													tex2 *= texMtx[j].M22;
												}
												tex += texMtx[j].M41;
												tex2 += texMtx[j].M42;
											}
											num48++;
											break;
										case 35u:
										case 36u:
										case 37u:
										case 38u:
										case 39u:
										case 40u:
											if ((num47 & 0xFF) == 35)
											{
												num48++;
												num48++;
											}
											else
											{
												num48++;
											}
											vertex[num51].pos0 = cmd_vertex_x[num55];
											vertex[num51].pos1 = cmd_vertex_y[num55];
											vertex[num51].pos2 = cmd_vertex_z[num55];
											num55++;
											if ((num56 & 2) != 0)
											{
												float pos = vertex[num51].pos0;
												float pos2 = vertex[num51].pos1;
												float pos3 = vertex[num51].pos2;
												vertex[num51].pos0 = pos * _matrix_current.M11 + pos2 * _matrix_current.M21 + pos3 * _matrix_current.M31 + _matrix_current.M41;
												vertex[num51].pos1 = pos * _matrix_current.M12 + pos2 * _matrix_current.M22 + pos3 * _matrix_current.M32 + _matrix_current.M42;
												vertex[num51].pos2 = pos * _matrix_current.M13 + pos2 * _matrix_current.M23 + pos3 * _matrix_current.M33 + _matrix_current.M43;
											}
											else
											{
												if ((num56 & 1) != 0)
												{
													vertex[num51].pos0 *= _matrix_current.M11;
													vertex[num51].pos1 *= _matrix_current.M22;
													vertex[num51].pos2 *= _matrix_current.M33;
												}
												vertex[num51].pos0 += _matrix_current.M41;
												vertex[num51].pos1 += _matrix_current.M42;
												vertex[num51].pos2 += _matrix_current.M43;
											}
											vertex[num51].tex0 = tex;
											vertex[num51].tex1 = tex2;
											vertex[num51].clr0 = clr;
											vertex[num51].clr1 = clr2;
											vertex[num51].clr2 = clr3;
											vertex[num51].clr3 = clr4;
											num51++;
											break;
										case 64u:
											num50 = cmd[num48];
											num48++;
											break;
										case 65u:
										{
											int num57 = num51;
											num51 = 0;
											switch (num50)
											{
											case 0u:
											{
												for (int m = 0; m < num57; m += 3)
												{
													transVertex(vertex[m], vertex[m + 1], vertex[m + 2], vtc, ref d);
												}
												break;
											}
											case 2u:
											{
												for (int m = 2; m < num57; m++)
												{
													transVertex(vertex[m - 2 + (m & 1)], vertex[m - 1 - (m & 1)], vertex[m], vtc, ref d);
												}
												break;
											}
											case 1u:
											{
												for (int m = 0; m < num57; m += 4)
												{
													transVertex(vertex[m], vertex[m + 1], vertex[m + 2], vtc, ref d);
													transVertex(vertex[m + 2], vertex[m + 3], vertex[m], vtc, ref d);
												}
												break;
											}
											case 3u:
											{
												for (int m = 2; m < num57; m += 2)
												{
													transVertex(vertex[m - 2], vertex[m - 1], vertex[m + 1], vtc, ref d);
													transVertex(vertex[m + 1], vertex[m], vertex[m - 2], vtc, ref d);
												}
												break;
											}
											}
											break;
										}
										default:
											OS_Terminate();
											break;
										case 0u:
											break;
										}
										num46++;
										num47 >>= 8;
										if (++k == 4)
										{
											k = 0;
											num46 = num48;
											num47 = cmd[num46];
											num48 = num46 + 1;
										}
									}
									GlobalScope.shape[num2].size = d - num52;
									GlobalScope.shape[num2].iMat = j;
									num2++;
									break;
								}
								case 1:
									if (skipFrame == 0)
									{
										for (int k = 0; k < 2; k++)
										{
											j = -1;
											d = 0;
											num3 = 0;
											for (int l = 0; l < num2; l++)
											{
												Shape shape = GlobalScope.shape[l];
												if (shape.iMat != j)
												{
													j = shape.iMat;
													NNSG3dResMatData nNSG3dResMatData2 = nNSG3dResMat.mat[j];
													TexVramList texImageParam = nNSG3dResMatData2.texImageParam;
													int num23 = (int)((nNSG3dResMatData2.polyAttr >> 16) & 0x1F);
													int num24 = ((num23 <= 16 || (texImageParam != null && (texImageParam.fmt == 1 || texImageParam.fmt == 6))) ? 1 : 0);
													num3 = ((num24 != 0 == (k == 1)) ? 1 : 0);
													if (num3 != 0)
													{
														if (texImageParam != null)
														{
															BindTexture(3553u, texImageParam.tex);
														}
														int num25 = ((texImageParam != null) ? 1 : 0);
														if (num7 != num25)
														{
															num7 = num25;
															if (num7 != 0)
															{
																glEnable(3553u);
															}
															else
															{
																glDisable(3553u);
															}
														}
														int num26 = ((num23 < 31 || (texImageParam != null && (texImageParam.fmt == 1 || texImageParam.fmt == 6))) ? 1 : 0);
														if (num6 != num26)
														{
															num6 = num26;
															if (num6 != 0)
															{
																glEnable(3042u);
															}
															else
															{
																glDisable(3042u);
															}
														}
														uint num27 = ((num24 == 0) ? 1u : 0u);
														if (num8 != num27)
														{
															num8 = num27;
															glDepthMask((byte)num27);
														}
														int num28 = (int)((nNSG3dResMatData2.polyAttr & 0xC0) >> 6);
														if (num9 != num28)
														{
															num9 = num28;
															switch (num9)
															{
															case 3:
																glDisable(2884u);
																break;
															case 1:
																glEnable(2884u);
																glCullFace(1028u);
																break;
															default:
																glEnable(2884u);
																glCullFace(1029u);
																break;
															}
														}
													}
												}
												if (num3 != 0)
												{
													NNSG3dResMatData nNSG3dResMatData3 = nNSG3dResMat.mat[j];
													if ((nNSG3dResMatData3.polyAttrMask & 0x30) == 0 && toonTable != 32767)
													{
														int num29 = (byte)(toonTable << 3);
														int num30 = (byte)(toonTable >> 5 << 3);
														int num31 = (byte)(toonTable >> 10 << 3);
														int num32 = d;
														int num33 = 0;
														while (num33 < shape.size)
														{
															vtc[num32].Color.R = (byte)(vtc[num32].Color.R * num29 >> 8);
															vtc[num32].Color.G = (byte)(vtc[num32].Color.G * num30 >> 8);
															vtc[num32].Color.B = (byte)(vtc[num32].Color.B * num31 >> 8);
															num33++;
															num32++;
														}
													}
													if ((nNSG3dResMatData3.polyAttr & 1) != 0)
													{
														float[] array3 = new float[3]
														{
															(float)((NNS_G3dGlb.lightColor[0] & 0x1F) * (nNSG3dResMatData3.diffAmb & 0x1F)) * 0.25f,
															(float)(((NNS_G3dGlb.lightColor[0] >> 5) & 0x1F) * ((nNSG3dResMatData3.diffAmb >> 5) & 0x1F)) * 0.25f,
															(float)(((NNS_G3dGlb.lightColor[0] >> 10) & 0x1F) * ((nNSG3dResMatData3.diffAmb >> 10) & 0x1F)) * 0.25f
														};
														float[] array4 = new float[3]
														{
															(float)((NNS_G3dGlb.lightColor[0] & 0x1F) * ((nNSG3dResMatData3.diffAmb >> 16) & 0x1F)) * 0.25f + (float)((nNSG3dResMatData3.specEmi >> 16) & 0x1F) * 8f,
															(float)(((NNS_G3dGlb.lightColor[0] >> 5) & 0x1F) * ((nNSG3dResMatData3.diffAmb >> 21) & 0x1F)) * 0.25f + (float)((nNSG3dResMatData3.specEmi >> 21) & 0x1F) * 8f,
															(float)(((NNS_G3dGlb.lightColor[0] >> 10) & 0x1F) * ((nNSG3dResMatData3.diffAmb >> 26) & 0x1F)) * 0.25f + (float)((nNSG3dResMatData3.specEmi >> 26) & 0x1F) * 8f
														};
														int num34 = d * 3;
														int num35 = d;
														int num36 = 0;
														while (num36 < shape.size)
														{
															int num37 = (int)(array4[0] + array3[0]);
															vtc[num35].Color.R = (byte)((num37 > 255) ? 255u : ((uint)num37));
															num37 = (int)(array4[1] + array3[1]);
															vtc[num35].Color.G = (byte)((num37 > 255) ? 255u : ((uint)num37));
															num37 = (int)(array4[2] + array3[2]);
															vtc[num35].Color.B = (byte)((num37 > 255) ? 255u : ((uint)num37));
															num36++;
															num35++;
															num34 += 3;
														}
													}
													else if ((nNSG3dResMatData3.specEmi & 0xFFFF0000u) != 0)
													{
														int num38 = (byte)(nNSG3dResMatData3.specEmi >> 16 << 3);
														int num39 = (byte)(nNSG3dResMatData3.specEmi >> 21 << 3);
														int num40 = (byte)(nNSG3dResMatData3.specEmi >> 26 << 3);
														int num41 = d;
														int num42 = 0;
														while (num42 < shape.size)
														{
															vtc[num41].Color.R = (byte)num38;
															vtc[num41].Color.G = (byte)num39;
															vtc[num41].Color.B = (byte)num40;
															num42++;
															num41++;
														}
													}
													glDrawArrays(4u, d, shape.size, vtc);
													polyCount += shape.size;
												}
												d += shape.size;
											}
										}
										glDisableClientState(32888u);
										glDisableClientState(32886u);
										if (num6 == 0)
										{
											glEnable(3042u);
										}
										if (num7 != 0)
										{
											glDisable(3553u);
										}
										if (num9 != 2)
										{
											glEnable(2884u);
											glCullFace(1029u);
										}
										if (num8 != 1)
										{
											glDepthMask(1);
										}
										glMatrixMode(5890u);
										glLoadIdentity();
										glMatrixMode(5888u);
									}
									currentMtx.copy(mtxFx);
									return;
								default:
									OS_Terminate();
									break;
								}
							}
						}

						internal static void NNS_G3dDraw(NNSG3dRenderObj pRenderObj)
						{
							DrawModel(pRenderObj, pRenderObj.resMdl);
						}

						internal static void NNS_G3dDraw1Mat1Shp(NNSG3dResMdl pResMdl, uint matID, uint shpID, int sendMat)
						{
							DrawModel(null, pResMdl);
						}

						internal static NNSG3dRenderObj NNS_G3dRSGetRenderObj(NNSG3dRS rs)
						{
							return rs.pRenderObj;
						}

						internal static int NNS_G3dRSGetCurrentNodeDescID(NNSG3dRS rs)
						{
							return rs.currentNode;
						}

						internal static void NNS_G3dSetDrawMask(uint mask)
						{
							drawMask = mask;
						}

						internal static void NNS_G3dGeFlushBuffer()
						{
						}

						internal static void NNS_G3dMdlUseGlbPolygonMode(NNSG3dResMdl pMdl)
						{
							NNSG3dResMat nNSG3dResMat = NNS_G3dGetMat(pMdl);
							for (int i = 0; i < nNSG3dResMat.dict.numEntry; i++)
							{
								NNSG3dResMatData nNSG3dResMatData = NNS_G3dGetMatByIdx(nNSG3dResMat, (uint)i);
								nNSG3dResMatData.polyAttrMask &= 4294967247u;
							}
						}

						internal static void NNS_G3dMdlUseMdlDiff(NNSG3dResMdl pMdl)
						{
						}

						internal static void NNS_G3dMdlUseMdlAmb(NNSG3dResMdl pMdl)
						{
						}

						internal static void NNS_G3dMdlUseMdlSpec(NNSG3dResMdl pMdl)
						{
						}

						internal static void NNS_G3dMdlUseMdlEmi(NNSG3dResMdl pMdl)
						{
						}

						internal static void NNS_G3dMdlUseMdlAlpha(NNSG3dResMdl pMdl)
						{
						}

						internal static void NNS_G3dMdlUseMdlPolygonID(NNSG3dResMdl pMdl)
						{
						}

						internal static void NNS_G3dMdlUseMdlPolygonMode(NNSG3dResMdl pMdl)
						{
							NNSG3dResMat nNSG3dResMat = NNS_G3dGetMat(pMdl);
							for (int i = 0; i < nNSG3dResMat.dict.numEntry; i++)
							{
								NNSG3dResMatData nNSG3dResMatData = NNS_G3dGetMatByIdx(nNSG3dResMat, (uint)i);
								nNSG3dResMatData.polyAttrMask |= 48u;
							}
						}

						internal static void NNS_G3dMdlSetMdlLightEnableFlag(NNSG3dResMdl pMdl, uint matID, int light)
						{
							NNSG3dResMatData nNSG3dResMatData = NNS_G3dGetMatByIdx(NNS_G3dGetMat(pMdl), matID);
							nNSG3dResMatData.polyAttr = (uint)((nNSG3dResMatData.polyAttr & -16) | light);
						}

						internal static void NNS_G3dMdlSetMdlPolygonMode(NNSG3dResMdl pMdl, uint matID, GXPolygonMode polyMode)
						{
							NNSG3dResMatData nNSG3dResMatData = NNS_G3dGetMatByIdx(NNS_G3dGetMat(pMdl), matID);
							nNSG3dResMatData.polyAttr = (uint)((nNSG3dResMatData.polyAttr & -49) | ((int)polyMode << 4));
						}

						internal static void NNS_G3dMdlSetMdlCullMode(NNSG3dResMdl pMdl, uint matID, GXCull cullMode)
						{
							NNSG3dResMatData nNSG3dResMatData = NNS_G3dGetMatByIdx(NNS_G3dGetMat(pMdl), matID);
							nNSG3dResMatData.polyAttr = (uint)((nNSG3dResMatData.polyAttr & -193) | ((int)cullMode << 6));
						}

						internal static void NNS_G3dMdlSetMdlAlpha(NNSG3dResMdl pMdl, uint matID, int alpha)
						{
							NNSG3dResMatData nNSG3dResMatData = NNS_G3dGetMatByIdx(NNS_G3dGetMat(pMdl), matID);
							nNSG3dResMatData.polyAttr = (uint)((nNSG3dResMatData.polyAttr & -2031617) | (alpha << 16));
						}

						internal static void NNS_G3dMdlSetMdlPolygonID(NNSG3dResMdl pMdl, uint matID, int polygonID)
						{
							NNSG3dResMatData nNSG3dResMatData = NNS_G3dGetMatByIdx(NNS_G3dGetMat(pMdl), matID);
							nNSG3dResMatData.polyAttr = (uint)((nNSG3dResMatData.polyAttr & -1056964609) | (polygonID << 24));
						}

						internal static int NNS_G3dMdlGetMdlLightEnableFlag(NNSG3dResMdl pMdl, uint matID)
						{
							NNSG3dResMatData nNSG3dResMatData = NNS_G3dGetMatByIdx(NNS_G3dGetMat(pMdl), matID);
							return (int)(nNSG3dResMatData.polyAttr & 0xF);
						}

						internal static int NNS_G3dMdlGetMdlAlpha(NNSG3dResMdl pMdl, uint matID)
						{
							NNSG3dResMatData nNSG3dResMatData = NNS_G3dGetMatByIdx(NNS_G3dGetMat(pMdl), matID);
							return (int)(nNSG3dResMatData.polyAttr & 0x1F0000) >> 16;
						}

						internal static void NNS_G3dMdlSetMdlPolygonIDAll(NNSG3dResMdl pMdl, int polygonID)
						{
						}

						internal static void NNS_G3dMdlSetMdlXLDepthUpdateAll(NNSG3dResMdl pMdl, int flag)
						{
						}

						internal static void NNS_G3dMdlSetMdlLightEnableFlagAll(NNSG3dResMdl pMdl, int light)
						{
							NNSG3dResMat nNSG3dResMat = NNS_G3dGetMat(pMdl);
							for (int i = 0; i < nNSG3dResMat.dict.numEntry; i++)
							{
								NNSG3dResMatData nNSG3dResMatData = NNS_G3dGetMatByIdx(nNSG3dResMat, (uint)i);
								nNSG3dResMatData.polyAttr = (uint)((uint)((int)nNSG3dResMatData.polyAttr & -16) | light);
							}
						}

						internal static void NNS_G3dMdlSetMdlDiffAll(NNSG3dResMdl pMdl, ushort col)
						{
							NNSG3dResMat nNSG3dResMat = NNS_G3dGetMat(pMdl);
							for (int i = 0; i < nNSG3dResMat.dict.numEntry; i++)
							{
								NNSG3dResMatData nNSG3dResMatData = NNS_G3dGetMatByIdx(nNSG3dResMat, (uint)i);
								nNSG3dResMatData.diffAmb = (nNSG3dResMatData.diffAmb & 0xFFFF0000u) | col;
							}
						}

						internal static void NNS_G3dMdlSetMdlAmbAll(NNSG3dResMdl pMdl, ushort col)
						{
							NNSG3dResMat nNSG3dResMat = NNS_G3dGetMat(pMdl);
							for (int i = 0; i < nNSG3dResMat.dict.numEntry; i++)
							{
								NNSG3dResMatData nNSG3dResMatData = NNS_G3dGetMatByIdx(nNSG3dResMat, (uint)i);
								nNSG3dResMatData.diffAmb = (uint)((nNSG3dResMatData.diffAmb & 0xFFFF) | (col << 16));
							}
						}

						internal static void NNS_G3dMdlSetMdlSpecAll(NNSG3dResMdl pMdl, ushort col)
						{
							NNSG3dResMat nNSG3dResMat = NNS_G3dGetMat(pMdl);
							for (int i = 0; i < nNSG3dResMat.dict.numEntry; i++)
							{
								NNSG3dResMatData nNSG3dResMatData = NNS_G3dGetMatByIdx(nNSG3dResMat, (uint)i);
								nNSG3dResMatData.specEmi = (nNSG3dResMatData.specEmi & 0xFFFF0000u) | col;
							}
						}

						internal static void NNS_G3dMdlSetMdlEmiAll(NNSG3dResMdl pMdl, ushort col)
						{
							NNSG3dResMat nNSG3dResMat = NNS_G3dGetMat(pMdl);
							for (int i = 0; i < nNSG3dResMat.dict.numEntry; i++)
							{
								NNSG3dResMatData nNSG3dResMatData = NNS_G3dGetMatByIdx(nNSG3dResMat, (uint)i);
								nNSG3dResMatData.specEmi = (uint)((nNSG3dResMatData.specEmi & 0xFFFF) | (col << 16));
							}
						}

						internal static void NNS_G3dMdlSetMdlAlphaAll(NNSG3dResMdl pMdl, int alpha)
						{
							NNSG3dResMat nNSG3dResMat = NNS_G3dGetMat(pMdl);
							for (int i = 0; i < nNSG3dResMat.dict.numEntry; i++)
							{
								NNS_G3dMdlSetMdlAlpha(pMdl, (uint)i, alpha);
							}
						}

						internal static NNSG3dResAnmHeader NNS_G3dGetAnmByIdx(NNSG3dResFileHeader pRes, uint idx)
						{
							_ = pRes.offset;
							NNSG3dResAnmSet nNSG3dResAnmSet = (NNSG3dResAnmSet)pRes.m_Res[0];
							NNSG3dResDict dict = nNSG3dResAnmSet.dict;
							_ = dict.entry;
							return nNSG3dResAnmSet.anm[idx];
						}

						internal static void NNS_G3dInit()
						{
						}

						internal static void NNS_G3dGetCurrentMtx(MtxFx43 m, MtxFx33 n)
						{
							m?.copy(currentMtx);
						}

						internal static int NNS_G3dWorldPosToScrPos(VecFx32 pWorld, out int px, out int py)
						{
							VecFx32 vecFx = fnd_reuse_pos;
							MTX_MultVec43(pWorld, NNS_G3dGlb.cameraMtx, vecFx);
							MtxFx44 projMtx = NNS_G3dGlb.projMtx;
							int num = FX_Mul(vecFx.x, projMtx._00) + FX_Mul(vecFx.y, projMtx._10) + FX_Mul(vecFx.z, projMtx._20) + projMtx._30;
							int num2 = FX_Mul(vecFx.x, projMtx._01) + FX_Mul(vecFx.y, projMtx._11) + FX_Mul(vecFx.z, projMtx._21) + projMtx._31;
							int num3 = FX_Mul(vecFx.x, projMtx._03) + FX_Mul(vecFx.y, projMtx._13) + FX_Mul(vecFx.z, projMtx._23) + projMtx._33;
							px = num * (LCD_WIDTH / 2) / num3 + 240;
							py = -num2 * (LCD_HEIGHT / 2) / num3 + 160;
							return 0;
						}

						internal static int NNS_G3dScrPosToWorldLine(int px, int py, VecFx32 pNear, VecFx32 pFar)
						{
							int num = -40960;
							int num2 = -2048000;
							px = -(px - 240) * 1097 / (LCD_WIDTH / 3);
							py = (py - 160) * 1097 / (LCD_WIDTH / 3);
							if (pNear != null)
							{
								VecFx32 vecFx = fnd_reuse_pos;
								vecFx.x = FX_Mul(px, num);
								vecFx.y = FX_Mul(py, num);
								vecFx.z = num;
								MTX_MultVec43(vecFx, NNS_G3dGlb.invCameraMtx, pNear);
							}
							if (pFar != null)
							{
								VecFx32 vecFx2 = fnd_reuse_pos;
								vecFx2.x = FX_Mul(px, num2);
								vecFx2.y = FX_Mul(py, num2);
								vecFx2.z = num2;
								MTX_MultVec43(vecFx2, NNS_G3dGlb.invCameraMtx, pFar);
							}
							return 0;
						}

						internal static Array NNS_G3dGetResDataByName(NNSG3dResDict dict, NNSG3dResName name)
						{
							int num = NNS_G3dGetResDictIdxByName(dict, name);
							if (num == -1)
							{
								return null;
							}
							NNSG3dResDictEntryHeader entry = dict.entry;
							ArrayReader arrayReader = new ArrayReader(entry.data);
							byte[] array = new byte[entry.sizeUnit];
							arrayReader.setPosition(num * entry.sizeUnit);
							arrayReader.read(array, 0, array.Length);
							return array;
						}

						internal static int NNS_G3dGetResDictIdxByName(NNSG3dResDict dict, NNSG3dResName name)
						{
							NNSG3dResDictEntryHeader entry = dict.entry;
							NNSG3dResName[] name2 = entry.name;
							int num = 0;
							int num2;
							while (true)
							{
								int refBit = dict.node[num].refBit;
								num2 = (((name.getNameByte(refBit >> 3) & (1 << (refBit & 7))) != 0) ? dict.node[num].idxRight : dict.node[num].idxLeft);
								if (num2 <= num)
								{
									break;
								}
								num = num2;
							}
							int idxEntry = dict.node[num2].idxEntry;
							if (idxEntry >= name2.Length)
							{
								return -1;
							}
							if (!NNSG3dResNamecmp(name, name2[idxEntry], name.name.Length))
							{
								return -1;
							}
							return idxEntry;
						}

						internal static NNSG3dResMdlSet NNS_G3dGetMdlSet(NNSG3dResFileHeader header)
						{
							return (NNSG3dResMdlSet)header.m_Res[0];
						}

						internal static NNSG3dResTex NNS_G3dGetTex(NNSG3dResFileHeader header)
						{
							int num = ((memcmp(header.signature, "BTX0", 4) != 0) ? 1 : 0);
							if (num >= header.dataBlocks)
							{
								return null;
							}
							return (NNSG3dResTex)header.m_Res[num];
						}

						internal static NNSG3dResMdl NNS_G3dGetMdlByIdx(NNSG3dResMdlSet mdlSet, uint idx)
						{
							return mdlSet.mdl[idx];
						}

						internal static NNSG3dResMat NNS_G3dGetMat(NNSG3dResMdl mdl)
						{
							return mdl.mat;
						}

						internal static NNSG3dResMdlInfo NNS_G3dGetMdlInfo(NNSG3dResMdl mdl)
						{
							return mdl.info;
						}

						internal static int NNS_G3dGetMatIdxByName(NNSG3dResMat mat, NNSG3dResName name)
						{
							return NNS_G3dGetResDictIdxByName(mat.dict, name);
						}

						internal static NNSG3dResNodeInfo NNS_G3dGetNodeInfo(NNSG3dResMdl mdl)
						{
							return mdl.nodeInfo;
						}

						internal static int NNS_G3dGetNodeIdxByName(NNSG3dResNodeInfo nodeinfo, NNSG3dResName name)
						{
							return NNS_G3dGetResDictIdxByName(nodeinfo.dict, name);
						}

						internal static NNSG3dResMatData NNS_G3dGetMatByIdx(NNSG3dResMat mat, uint idx)
						{
							return mat.mat[idx];
						}

						internal static NNSG3dAnmObj NNS_G3dAllocAnmObj(NNSFndAllocator pAlloc, NNSG3dResAnmHeader pAnm, NNSG3dResMdl pMdl)
						{
							return new NNSG3dAnmObj();
						}

						internal static void NNS_G3dFreeAnmObj(NNSFndAllocator pAlloc, NNSG3dAnmObj pAnmObj)
						{
							pAnmObj.destruct();
						}

						internal static NNSG3dJntAnmResult[] NNS_G3dAllocRecBufferJnt(NNSFndAllocator pAlloc, NNSG3dResMdl pResMdl)
						{
							return new NNSG3dJntAnmResult[pResMdl.info.numNode];
						}

						internal static void NNS_G3dFreeRecBufferJnt(NNSFndAllocator pAlloc, NNSG3dJntAnmResult[] pRecBuf)
						{
							pRecBuf = null;
						}

						internal static void UpdateVolume(NNSSndHandle handle)
						{
							float volume = (soundMute[handle.player] ? 0f : ((float)(handle.volume * baseVolume[handle.player]) * 6.200012E-05f));
							MainActivity.setSoundVolume((int)handle.source, volume);
						}

						internal static void StopSound(NNSSndHandle handle)
						{
							MainActivity.stopSound((int)handle.source);
							handle.flag = 0u;
							handle.fadeFrame = 0;
						}

						internal static int PlaySound(NNSSndHandle handle, string _filename)
						{
							if (handle.source == 0)
							{
								for (int i = 1; i < 32; i++)
								{
									if ((channelMask & (1 << i)) == 0)
									{
										channelMask |= 1 << i;
										handle.source = (uint)i;
										break;
									}
								}
							}
							if (handle.source == 0)
							{
								return 0;
							}
							UpdateVolume(handle);
							MainActivity.playSound((int)handle.source, _filename);
							handle.flag |= 2u;
							return 1;
						}

						internal static void NNS_SndInit()
						{
						}

						internal static void NNS_SndMain()
						{
							for (NNSSndHandle next = pSoundTop; next != null; next = next.next)
							{
								if ((next.flag & 2) != 0)
								{
									if (next.fadeFrame != 0)
									{
										next.fadeTime++;
										float num = (next.fadeVolume[0] * (next.fadeFrame - next.fadeTime) + next.fadeVolume[1] * next.fadeTime) / next.fadeFrame;
										MainActivity.setSoundVolume((int)next.source, soundMute[next.player] ? 0f : (num * (float)baseVolume[next.player] * 6.200012E-05f));
										if (next.fadeTime == next.fadeFrame)
										{
											next.fadeFrame = 0;
											if ((next.flag & 4) != 0)
											{
												next.request = 16777216;
											}
										}
									}
									if (MainActivity.getSoundState((int)next.source) == 0)
									{
										next.flag = 0u;
									}
								}
							}
						}

						internal static void NNS_SndUpdate()
						{
							int num = 0;
							for (NNSSndHandle next = pSoundTop; next != null; next = next.next)
							{
								if (next.request == 0)
								{
									continue;
								}
								string arg;
								switch (next.request & -16777216)
								{
								case 33554432:
									sprintf(out arg, "BGM%.2d", next.request & 0xFFFF);
									PlaySound(next, arg);
									break;
								case 50331648:
								{
									if ((next.flag & 2) != 0)
									{
										StopSound(next);
										continue;
									}
									int num2 = next.request & 0xFFFF;
									int num3 = (next.request >> 16) & 0xFF;
									if (num2 == 0 && num3 < 4)
									{
										if ((num & (1 << num3)) != 0)
										{
											StopSound(next);
											break;
										}
										num |= 1 << num3;
									}
									sprintf(out arg, "SE%.3d_%.2d", num2, num3);
									PlaySound(next, arg);
									break;
								}
								default:
									StopSound(next);
									break;
								}
								next.request = 0;
							}
						}

						internal static void NNS_SndSetMasterVolume(int volume)
						{
						}

						internal static void NNS_SndStopSoundAll()
						{
							for (NNSSndHandle next = pSoundTop; next != null; next = next.next)
							{
								NNS_SndPlayerStopSeq(next, 0);
							}
						}

						internal static int NNS_SndAllocAlarm()
						{
							return 0;
						}

						internal static void NNS_SndFreeAlarm(int alarmNo)
						{
						}

						internal static int NNS_SndArcPlayerSetup(NNSSndHeap heap)
						{
							return 0;
						}

						internal static void NNS_SndArcPlayerStartSeq(NNSSndHandle handle, int seqNo)
						{
							handle.fadeFrame = 0;
							handle.request = 0x2000000 | seqNo;
							handle.player = 0;
						}

						internal static int NNS_SndArcPlayerStartSeqArc(NNSSndHandle handle, int seqArcNo, int index)
						{
							handle.fadeFrame = 0;
							handle.request = 0x3000000 | seqArcNo | (index << 16);
							handle.player = 1;
							return 1;
						}

						internal static int NNS_SndArcPlayerStartSeqEx(NNSSndHandle handle, int playerNo, int bankNo, int playerPrio, int seqNo)
						{
							return 0;
						}

						internal static void NNS_SndPlayerStopSeq(NNSSndHandle handle, int fadeFrame)
						{
							if (fadeFrame == 0)
							{
								handle.fadeFrame = 0;
								handle.request = 16777216;
								return;
							}
							handle.fadeFrame = fadeFrame;
							handle.fadeTime = 0;
							handle.fadeVolume[0] = handle.volume;
							handle.fadeVolume[1] = 0;
							handle.flag |= 4u;
						}

						internal static void NNS_SndPlayerStopSeqByPlayerNo(int playerNo, int fadeFrame)
						{
							for (NNSSndHandle next = pSoundTop; next != null; next = next.next)
							{
								if (((next.flag & 2) != 0 || next.request != 0) && next.player == playerNo)
								{
									NNS_SndPlayerStopSeq(next, fadeFrame);
								}
							}
						}

						internal static void NNS_SndPlayerStopLastSeqByPlayerNo(int playerNo)
						{
							NNSSndHandle nNSSndHandle = null;
							for (NNSSndHandle next = pSoundTop; next != null; next = next.next)
							{
								if ((next.buffer[0] != 0 || next.buffer[1] != 0 || next.request != 0) && next.player == playerNo)
								{
									nNSSndHandle = next;
								}
							}
							if (nNSSndHandle != null)
							{
								NNS_SndPlayerStopSeq(nNSSndHandle, 0);
							}
						}

						internal static void NNS_SndPlayerPause(NNSSndHandle handle, int flag)
						{
							if ((handle.flag & 2) != 0)
							{
								MainActivity.pauseSound((int)handle.source, flag);
							}
						}

						internal static void NNS_SndPlayerPauseAll(int flag)
						{
							for (NNSSndHandle next = pSoundTop; next != null; next = next.next)
							{
								NNS_SndPlayerPause(next, flag);
							}
						}

						internal static void NNS_SndPlayerSetVolume(NNSSndHandle handle, int volume)
						{
							handle.volume = volume;
							if ((handle.flag & 2) != 0)
							{
								UpdateVolume(handle);
							}
						}

						internal static void NNS_SndPlayerMoveVolume(NNSSndHandle handle, int targetVolume, int frames)
						{
							handle.fadeFrame = frames;
							handle.fadeTime = 0;
							handle.fadeVolume[0] = handle.volume;
							handle.fadeVolume[1] = targetVolume;
							handle.volume = targetVolume;
							handle.flag &= 4294967291u;
						}

						internal static void NNS_SndPlayerSetTrackPan(NNSSndHandle handle, ushort trackBitMask, int pan)
						{
						}

						internal static void NNS_SndPlayerSetPlayerVolume(int playerNo, int volume)
						{
							baseVolume[playerNo] = volume;
							for (NNSSndHandle next = pSoundTop; next != null; next = next.next)
							{
								if ((next.flag & 2) != 0 && next.player == playerNo)
								{
									UpdateVolume(next);
								}
							}
						}

						internal static void NNS_SndPlayerMute(int playerNo, bool mute)
						{
							soundMute[playerNo] = mute;
							for (NNSSndHandle next = pSoundTop; next != null; next = next.next)
							{
								if ((next.flag & 2) != 0 && next.player == playerNo)
								{
									UpdateVolume(next);
								}
							}
						}

						internal static void NNS_SndHandleInit(NNSSndHandle handle)
						{
							handle.setDefault();
							handle.next = pSoundTop;
							pSoundTop = handle;
						}

						internal static int NNS_SndHandleIsValid(NNSSndHandle handle)
						{
							if ((handle.flag & 4) != 0 || handle.request == 16777216)
							{
								return 0;
							}
							if ((handle.flag & 2) == 0 && handle.request == 0)
							{
								return 0;
							}
							return 1;
						}

						internal static void NNS_SndHandleReleaseSeq(NNSSndHandle handle)
						{
							StopSound(handle);
							channelMask &= ~(1 << (int)handle.source);
							handle.source = 0u;
						}

						internal static void NNS_SndArcStrmInit(uint threadPrio, NNSSndHeap heap)
						{
						}

						internal static void NNS_SndArcStrmStop(NNSSndStrmHandle handle, int fadeFrame)
						{
						}

						internal static void NNS_SndArcStrmStopAll(int fadeFrame)
						{
						}

						internal static int NNS_SndArcStrmPrepareEx2(NNSSndStrmHandle handle, int playerNo, int playerPrio, int strmNo, uint offset, NNSSndStrmCallback strmCallback, Array strmCallbackArg, NNSSndArcStrmCallback sndArcStrmCallback, object sndArcStrmCallbackArg)
						{
							return 0;
						}

						internal static void NNS_SndArcStrmStartPrepared(NNSSndStrmHandle handle)
						{
						}

						internal static int NNS_SndArcStrmIsPrepared(NNSSndStrmHandle handle)
						{
							return 0;
						}

						internal static uint NNS_SndArcStrmGetCurrentPlayingPos(NNSSndStrmHandle handle)
						{
							return 0u;
						}

						internal static void NNS_SndArcStrmMoveVolume(NNSSndStrmHandle handle, int volume, int frames)
						{
						}

						internal static void NNS_SndArcStrmSetChannelPan(NNSSndStrmHandle handle, int chNo, int pan)
						{
						}

						internal static void NNS_SndStrmHandleInit(NNSSndStrmHandle handle)
						{
						}

						internal static int NNS_SndStrmHandleIsValid(NNSSndStrmHandle handle)
						{
							return 0;
						}

						internal static void NNS_SndStrmHandleRelease(NNSSndStrmHandle handle)
						{
						}

						internal static void NNS_SndArcInit(NNSSndArc arc, string filePath, NNSSndHeap heap, int symbolLoadFlag)
						{
						}

						internal static int NNS_SndArcSetup(NNSSndArc arc, NNSSndHeap heap, int symbolLoadFlag)
						{
							return 0;
						}

						internal static NNSSndArc NNS_SndArcGetCurrent()
						{
							return null;
						}

						internal static NNSSndArc NNS_SndArcSetCurrent(NNSSndArc arc)
						{
							return null;
						}

						internal static int NNS_SndArcLoadGroup(int groupNo, NNSSndHeap heap)
						{
							NNS_SndPlayerStopSeqByPlayerNo(1, 0);
							return 0;
						}

						internal static int NNS_SndArcLoadSeqArc(int seqArcNo, NNSSndHeap heap)
						{
							return 0;
						}

						internal static int NNS_SndArcLoadWaveArc(int waveArcNo, NNSSndHeap heap)
						{
							return 0;
						}

						internal static int NNS_SndArcLoadSeqEx(int seqNo, uint loadFlag, NNSSndHeap heap)
						{
							return 0;
						}

						internal static int NNS_SndArcLoadBankEx(int bankNo, uint loadFlag, NNSSndHeap heap)
						{
							return 0;
						}

						internal static NNSSndArcSeqInfo NNS_SndArcGetSeqInfo(int seqNo)
						{
							return null;
						}

						internal static NNSSndArcSeqArcInfo NNS_SndArcGetSeqArcInfo(int seqNo)
						{
							return null;
						}

						internal static NNSSndArcBankInfo NNS_SndArcGetBankInfo(int bankNo)
						{
							return null;
						}

						internal static NNSSndArcWaveArcInfo NNS_SndArcGetWaveArcInfo(int waveArcNo)
						{
							return null;
						}

						internal static NNSSndArcGroupInfo NNS_SndArcGetGroupInfo(int groupNo)
						{
							return null;
						}

						internal static void NNS_SndArcSetFileAddress(uint fileId, Array address)
						{
						}

						internal static int NNS_SndArcGetFileAddress(uint fileId)
						{
							return 0;
						}

						internal static uint NNS_SndArcGetFileSize(uint fileId)
						{
							return 0u;
						}

						internal static uint NNS_SndArcGetFileOffset(uint fileId)
						{
							return 0u;
						}

						internal static NNSSndHeap NNS_SndHeapCreate(Array startAddress, uint size)
						{
							return new NNSSndHeap();
						}

						internal static void NNS_SndHeapDestroy(NNSSndHeap heap)
						{
							heap.destruct();
						}

						internal static Array NNS_SndHeapAlloc(NNSSndHeap heap, uint size, NNSSndHeapDisposeCallback callback, uint data1, uint data2)
						{
							return null;
						}

						internal static void NNS_SndHeapClear(NNSSndHeap heap)
						{
						}

						internal static int NNS_SndHeapSaveState(NNSSndHeap heap)
						{
							return 0;
						}

						internal static void NNS_SndHeapLoadState(NNSSndHeap heap, int level)
						{
						}

						internal static uint NNS_SndHeapGetSize(NNSSndHeap heap)
						{
							return 0u;
						}

						internal static uint NNS_SndHeapGetFreeSize(NNSSndHeap heap)
						{
							return 0u;
						}

						internal static int NNS_SndCaptureStartReverb(Array buffer_p, uint bufSize, int format, int sampleRate, int volume)
						{
							return 0;
						}

						internal static void NNS_SndCaptureStopReverb(int frames)
						{
						}

						internal static void NNS_SndCaptureSetReverbVolume(int volume, int frames)
						{
						}

						internal static void NNS_McsInit()
						{
						}

						public static int NNS_McsGetMaxCaps()
						{
							return 0;
						}

						public static bool NNS_McsOpen(int arg0)
						{
							return false;
						}

						public static bool NNS_McsIsServerConnect()
						{
							return false;
						}

						internal static void NNS_McsClose()
						{
						}

						internal static void NNS_McsInitFileIO()
						{
						}

						public static uint NNS_McsOpenFile(int arg0, string arg1, int arg2)
						{
							return 0u;
						}

						internal static void NNS_McsCloseFile(int arg0)
						{
						}

						public static uint NNS_McsReadFile(int arg0, Array arg1, int arg2, int[] arg3)
						{
							return 0u;
						}

						public static uint NNS_McsWriteFile(int arg0, Array arg1, int arg2)
						{
							return 0u;
						}

						internal static bool SWC_IsValidFriendData(int data)
						{
							return false;
						}

						internal static bool SWC_IsEqualFriendData(int arg0, int arg1)
						{
							return false;
						}

						internal static bool SWC_IsBuddyFriendData(int arg0)
						{
							return false;
						}

						internal static void SWC_ClearBuddyFlagFriendData(int arg0)
						{
						}

						internal static bool SWC_CheckUserData(SWCUserData arg0)
						{
							return false;
						}

						internal static void SWC_ReportUserData(SWCUserData arg0)
						{
						}

						internal static void SWC_CreateUserData(SWCUserData arg0, int arg1)
						{
						}

						internal static void SWC_ClearDirtyFlag(SWCUserData arg0)
						{
						}

						internal static bool SWC_CheckDirtyFlag(SWCUserData arg0)
						{
							return false;
						}

						internal static int SWC_Init(byte[] arg0)
						{
							return 0;
						}

						internal static void SWC_SetMemFunc(_func1 func1, _DeallocatorForDWC DeallocatorForDWC)
						{
						}

						internal static void SWC_StartUtilityEx(int arg0, int arg1)
						{
						}

						internal static void SWC_ShutdownFriendsMatch()
						{
						}

						internal static void SWC_CleanupInet()
						{
						}

						internal static void SWC_CreateFriendKeyToken(int arg0, long arg1)
						{
						}

						internal static void SWC_SetGsProfileId(int arg0, int arg1)
						{
						}

						internal static void SWC_DeleteBuddyFriendData(int arg0)
						{
						}

						internal static bool SWC_CheckFriendKey(SWCUserData arg0, int arg1)
						{
							return false;
						}

						internal static ulong SWC_CreateFriendKey(SWCUserData arg0)
						{
							return 0uL;
						}

						internal static int SWC_GetLastErrorEx(int[] arg0, int arg1)
						{
							return 0;
						}

						internal static bool SWC_CheckValidConsole(SWCUserData arg0)
						{
							return false;
						}

						internal static void SWC_InitInet(int arg0)
						{
						}

						internal static void SWC_SetAuthServer(int arg0)
						{
						}

						internal static void SWC_ConnectInetAsync()
						{
						}

						internal static void SWC_ProcessInet()
						{
						}

						internal static bool SWC_CheckInet()
						{
							return false;
						}

						internal static int SWC_GetLastError(Array arg0)
						{
							return 0;
						}

						internal static void SWC_InitFriendsMatch(int arg0, SWCUserData arg1, int arg2, sbyte[] arg3, sbyte[] arg4, int arg5, int arg6, Array arg7, int arg8)
						{
						}

						internal static void SWC_LoginAsync(ushort[] arg0, Array arg1, _cb_login cb_login, Array arg2)
						{
						}

						internal static void SWC_ProcessFriendsMatch()
						{
						}

						internal static int SWC_GetIngamesnCheckResult()
						{
							return 0;
						}

						internal static void SWC_ConnectToAnybodyAsync(int arg0, sbyte[] arg1, _cb_connect cb_connect, Array arg2, Array arg3, Array arg4)
						{
						}

						internal static void SWC_CancelMatching()
						{
						}

						internal static int SWC_GetMyAID()
						{
							return 0;
						}

						internal static void SWC_SendReliable(int arg0, Array arg1, int arg2)
						{
						}

						internal static bool SWC_CleanupInetAsync()
						{
							return false;
						}

						internal static void SWC_ClearError()
						{
						}

						internal static void SWC_AddMatchKeyString(int arg0, Array arg1, Array arg2)
						{
						}

						internal static void SWC_SetConnectionClosedCallback(_cb_close cb_close, Array arg0)
						{
						}

						internal static void SWC_SetUserSendCallback(_cb_send cb_send)
						{
						}

						internal static void SWC_SetUserRecvCallback(_cb_recv cb_recv)
						{
						}

						internal static int SWC_GetNumConnectionHost()
						{
							return 0;
						}

						internal static void SWC_SetRecvBuffer(byte arg0, Array arg1, int arg2)
						{
						}

						internal static void SWC_SetServerPortNo(int arg0)
						{
						}

						internal static void SWC_SetServerDomain(sbyte[] arg0)
						{
						}

						internal static void SWC_SetContentsId(uint arg0)
						{
						}

						internal static void SWC_FileStorageExportAsync(int arg0, int arg1, _cb_export cb_export, Array arg2)
						{
						}

						internal static void SWC_FileStorageLogout()
						{
						}

						internal static bool SWC_FileStorageImportAsync(int arg0, _cb_import cb_import, Array arg1)
						{
							return false;
						}

						internal static void SWC_FileStorageLoadAsync(int arg0, Array arg1, int arg2, _cb_load cb_load, Array arg3)
						{
						}

						internal static void SWC_FileStorageSaveAsync(int arg0, Array arg1, int arg2, SWCFileStorageCallback arg3, Array arg4)
						{
						}

						internal static void SWC_FileStorageImportCountAsync(int arg0, _cb_import_count cb_import_count, Array arg1)
						{
						}

						internal static int SWC_GetInetStatus()
						{
							return 0;
						}

						internal static void SWC_LogoutFromStorageServer()
						{
						}

						internal static int SWC_UpdateServersAsync(Array arg0, _cb_friendlist_update_end cb_friendlist_update_end, Array arg1, _cb_friendstatus_update cb_friendstatus_update, Array arg2, _cb_friendlist_delete cb_friendlist_delete, Array arg3)
						{
							return 0;
						}

						internal static bool SWC_SetBuddyFriendCallback(_cb_friendlist_set_buddy cb_friendlist_set_buddy, Array arg0)
						{
							return false;
						}

						internal static bool SWC_LoginToStorageServerAsync(_cb_gsLogin cb_gsLogin, Array arg0)
						{
							return false;
						}

						internal static bool SWC_SetStorageServerCallback(_cb_gsSave cb_gsSave, _cb_gsLoad cb_gsLoad)
						{
							return false;
						}

						internal static bool SWC_SavePublicDataAsync(Array arg0, Array arg1)
						{
							return false;
						}

						internal static bool SWC_LoadOthersDataAsync(sbyte[] arg0, int arg1, Array arg2)
						{
							return false;
						}

						internal static bool SWC_FileStorageLoginAsync(_cb_storagelogin cb_storagelogin, Array arg0)
						{
							return false;
						}

						internal static void SWC_CreateExchangeToken(SWCUserData arg0, int arg1)
						{
						}

						internal static int DWC_GetGsProfileId(SWCUserData user, int friends)
						{
							return 0;
						}

						internal static void DWC_SetAuthServer(int arg0)
						{
						}

						internal static int DWC_GetNumFriend(int arg0, int arg1)
						{
							return 0;
						}

						internal static bool DWC_IsValidFriendData(int arg0)
						{
							return false;
						}

						internal static ulong DWC_Acc_NumericKeyToFriendKey(sbyte[] arg0)
						{
							return 0uL;
						}

						internal static void DWC_CreateExchangeToken(Array arg0, Array arg1)
						{
						}

						internal static int Java_com_square_1enix_FFIII_1J_MainActivity_init(JNIEnv _env, object thiz)
						{
							return 0;
						}

						internal static void Java_com_square_1enix_FFIII_1J_MainActivity_quit(JNIEnv _env, object thiz)
						{
						}

						internal static void Java_com_square_1enix_FFIII_1J_MainActivity_pause(JNIEnv _env, object thiz)
						{
							pause(_env, thiz);
						}

						internal static void Java_com_square_1enix_FFIII_1J_MainActivity_resume(JNIEnv _env, object thiz)
						{
							resume(_env, thiz);
						}

						internal static void Java_com_square_1enix_FFIII_1J_MainActivity_touch(JNIEnv _env, object thiz, int count, int peak, float x0, float y0, float x1, float y1)
						{
							touch(_env, thiz, count, peak, x0, y0, x1, y1);
						}

						internal static void Java_com_square_1enix_FFIII_1J_MainActivity_render(JNIEnv _env, object thiz)
						{
							render(_env, thiz);
						}

						internal static void Java_com_square_1enix_FFIII_1J_MainActivity_encode(JNIEnv _env, Type cls, Array array, int mask)
						{
							sbyte[] byteArrayElements = _env.GetByteArrayElements(array, null);
							_env.GetArrayLength(array);
							_env.ReleaseByteArrayElements(array, byteArrayElements, 0);
						}

						internal static void getCompanyDirectory(string directory)
						{
						}

						internal static void changeCompanyDirectory()
						{
						}

						internal static void changeGlobalDirectory()
						{
						}

						internal static LANGUAGE_CODE languageCode()
						{
							LANGUAGE_CODE lANGUAGE_CODE = LANGUAGE_CODE.LANGUAGE_CODE_ENGLISH;
							OS_GetOwnerInfo(owner_info);
							return owner_info.language switch
							{
								1 => LANGUAGE_CODE.LANGUAGE_CODE_ENGLISH, 
								3 => LANGUAGE_CODE.LANGUAGE_CODE_GERMAN, 
								5 => LANGUAGE_CODE.LANGUAGE_CODE_SPANISH, 
								2 => LANGUAGE_CODE.LANGUAGE_CODE_FRENCH, 
								4 => LANGUAGE_CODE.LANGUAGE_CODE_ITALIAN, 
								_ => LANGUAGE_CODE.LANGUAGE_CODE_ENGLISH, 
							};
						}

						private static void G3DDemo_InitDisplay()
						{
							G3X_InitMtxStack();
							GX_SetBankForTex(GXVRamTex.GX_VRAM_TEX_0123_ABCD);
							GX_SetBankForTexPltt(GXVRamTexPltt.GX_VRAM_TEXPLTT_0123_E);
							GX_SetBankForBG(GXVRamBG.GX_VRAM_BG_16_G);
							GX_SetGraphicsMode(GXDispMode.GX_DISPMODE_GRAPHICS, GXBGMode.GX_BGMODE_0, 0);
							GX_SetVisiblePlane(3);
							GX_SetBGCharOffset(0);
							GX_SetBGScrOffset(0);
							G2_SetBG1Control(0, 0, GXBGScrBase.GX_BG_SCRBASE_0x3800, GXBGCharBase.GX_BG_CHARBASE_0x00000, 0);
							G2_SetBG0Priority(1);
							G2_SetBG1Priority(0);
							G3X_SetShading(0);
							G3X_AntiAlias(1);
							G2_BlendNone();
							G3_SwapBuffers(0, 0);
							G3X_AlphaTest(0, 0);
							G3X_AlphaBlend(1);
							G3_ViewPort(0, 0, 255, 191);
							GX_DispOn();
							GXS_DispOn();
						}

						private static void DrawGroundMesh()
						{
							int num = 8191;
							short x = short.MaxValue;
							short num2 = (short)(32767 - num);
							for (int i = 0; i < 8; i++)
							{
								short z = short.MaxValue;
								short num3 = (short)(32767 - num);
								G3_Begin(GXBegin.GX_BEGIN_QUAD_STRIP);
								G3_Vtx(x, 0, z);
								G3_Vtx(num2, 0, z);
								for (int j = 0; j < 8; j++)
								{
									z = num3;
									num3 -= (short)num;
									G3_Vtx(x, 0, num3);
									G3_Vtx(num2, 0, num3);
								}
								G3_End();
								x = num2;
								num2 -= (short)num;
							}
						}

						private static void G3DDemo_InitGround(G3DDemoGround ground, int scale)
						{
							ground.groundEnable = 1;
							ground.wireColor = GX_RGB(31, 31, 31);
							ground.backColor = GX_RGB(0, 10, 31);
							ground.backAlpha = 31;
							ground.scale = scale;
						}

						private static void G3DDemo_SetTrans(G3DDemoGround ground, int x, int y, int z)
						{
							ground.trans_x = x;
							ground.trans_y = y;
							ground.trans_z = z;
						}

						private static void G3DDemo_DrawGround(G3DDemoGround ground)
						{
							if (ground.groundEnable != 0)
							{
								G3X_SetClearColor(ground.backColor, ground.backAlpha, 32767, 63, 0);
								G3_MaterialColorDiffAmb(ground.wireColor, GX_RGB(0, 0, 0), 1);
								G3_PolygonAttr(0, GXPolygonMode.GX_POLYGONMODE_MODULATE, GXCull.GX_CULL_NONE, 0, 0, 0);
								G3_PushMtx();
								G3_Scale(ground.scale, ground.scale, ground.scale);
								G3_Translate(ground.trans_x, ground.trans_y, ground.trans_z);
								DrawGroundMesh();
								G3_PopMtx(1);
							}
						}

						internal static void G3DDemo_InitConsole()
						{
							G3DDemo_ClearConsole();
						}

						internal static void G3DDemo_InitConsoleSub()
						{
							G3DDemo_ClearConsole();
						}

						internal static void G3DDemo_ClearConsole()
						{
						}

						internal static void G3DDemo_Print(int x, int y, int color, string @string)
						{
							m_Graphics.DrawStringStart();
							drawString(@string, x * 12, y * 12, -1, 12);
							m_Graphics.DrawStringEnd();
						}

						internal static void G3DDemo_Printf(int x, int y, int color, string format, params object[] args)
						{
							string text = StringUtil.format(format, args);
							G3DDemo_Print(x, y, color, text);
						}

						internal static void G3DDemo_PrintApplyToHW()
						{
						}

						internal static void G3DDemo_PrintApplyToHWSub()
						{
						}

						internal static int divr(int number, int denom)
						{
							return (number + (denom - 1)) / denom;
						}

						internal static Array VX_Malloc(uint size)
						{
							return ds.g_pVXAllocFunc(size);
						}

						internal static void VX_Free(Array p_mem)
						{
							ds.g_pVXFreeFunc(p_mem);
						}

						internal static void NitroMain()
						{
							GAMEPART start_part = GAMEPART.GAMEPART_DEBUG_MENU;
							if (init)
							{
								sys.GGlobal.loop(start_part);
								return;
							}
							init = true;
							sys.GGlobal.initialize();
							sys.GGlobal.run(start_part);
						}

						public static string getChrNameStr(int n, int nSlot)
						{
							if (3 > nSlot && 0 <= nSlot)
							{
								card.CSaveData cSaveData = SaveDataMng.getSingleton().SaveData(nSlot);
								if (cSaveData != null)
								{
									return cSaveData.composit.getChrName(n);
								}
							}
							return null;
						}

						public static string getChrLvStr(int n, int nSlot)
						{
							if (3 > nSlot && 0 <= nSlot)
							{
								dgs.DGSMessageManager dGSMessageManager = dgs.msg.CMessageSys.getInstance().Sub();
								dgs.DGSMessage dGSMessage = dGSMessageManager.createMessage(50414u, menu.MenuManager.getSingleton().GetMenuDataTextNo(), 1);
								card.CSaveData cSaveData = SaveDataMng.getSingleton().SaveData(nSlot);
								if (cSaveData != null)
								{
									string arg = "";
									if (dGSMessage != null)
									{
										int num = OS_GetLanguage();
										if (num == 0 || num == 6 || num == 7)
										{
											sprintf(out arg, "%s%2d", dGSMessage.getString(), cSaveData.composit.getLV(n));
										}
										else
										{
											sprintf(out arg, "%s  %2d", dGSMessage.getString(), cSaveData.composit.getLV(n));
										}
										dGSMessage.release();
									}
									dGSMessage = null;
									return arg;
								}
							}
							return null;
						}

						public static string getChrJobStr(int n, int nSlot)
						{
							if (3 > nSlot && 0 <= nSlot)
							{
								card.CSaveData cSaveData = SaveDataMng.getSingleton().SaveData(nSlot);
								if (cSaveData != null)
								{
									int jobID = cSaveData.composit.getJobID(n);
									OS_Printf("job ID = %d.\n", jobID);
									dgs.DGSMessageManager dGSMessageManager = dgs.msg.CMessageSys.getInstance().Sub();
									dgs.DGSMessage dGSMessage = dGSMessageManager.createMessage((uint)(50105 + jobID), menu.MenuManager.getSingleton().GetMenuDataTextNo(), 1);
									string arg = "";
									if (dGSMessage != null)
									{
										sprintf(out arg, "%s", dGSMessage.getString());
										dGSMessage.release();
									}
									dGSMessage = null;
									return arg;
								}
							}
							return null;
						}

						public static string getChrSkillStr(int n, int nSlot)
						{
							if (3 > nSlot && 0 <= nSlot)
							{
								card.CSaveData cSaveData = SaveDataMng.getSingleton().SaveData(nSlot);
								string arg = "";
								if (cSaveData != null)
								{
									sprintf(out arg, "%d", cSaveData.composit.getSkill(n));
								}
								return arg;
							}
							return null;
						}

						public static string getChrHPStr(int n, int nSlot)
						{
							if (3 > nSlot && 0 <= nSlot)
							{
								card.CSaveData cSaveData = SaveDataMng.getSingleton().SaveData(nSlot);
								string arg = "";
								if (cSaveData != null)
								{
									sprintf(out arg, "%d", cSaveData.composit.getHPLimit(n));
								}
								return arg;
							}
							return null;
						}

						public static string getGoldStr(int nSlot)
						{
							if (3 > nSlot && 0 <= nSlot)
							{
								dgs.DGSMessageManager dGSMessageManager = dgs.msg.CMessageSys.getInstance().Sub();
								dgs.DGSMessage dGSMessage = dGSMessageManager.createMessage(50429u, menu.MenuManager.getSingleton().GetMenuDataTextNo(), 1);
								string arg = "";
								card.CSaveData cSaveData = SaveDataMng.getSingleton().SaveData(nSlot);
								if (cSaveData != null && dGSMessage != null)
								{
									sprintf(out arg, "%7d  %s", cSaveData.composit.getGold(), dGSMessage.getString());
									dGSMessage.release();
								}
								dGSMessage = null;
								return arg;
							}
							return null;
						}

						public static string getPlayTimeStr(int nSlot)
						{
							if (3 > nSlot && 0 <= nSlot)
							{
								card.CSaveData cSaveData = SaveDataMng.getSingleton().SaveData(nSlot);
								if (cSaveData != null)
								{
									string arg = "";
									uint num = cSaveData.composit.getPlayTime();
									if (359999 < num)
									{
										num = 359999u;
									}
									sprintf(out arg, "%02d:%02d:%02d", ds.secondToHH(num), ds.secondToMM(num), ds.secondToSS(num));
									return arg;
								}
							}
							return null;
						}

						public static int isChrEnable(int n, int nSlot)
						{
							card.CSaveData cSaveData = SaveDataMng.getSingleton().SaveData(nSlot);
							if (cSaveData != null)
							{
								if (cSaveData.composit.isChrEnable(n) != 1)
								{
									return 0;
								}
								return 1;
							}
							return 2;
						}

						internal static int getJobID(int n, int nSlot)
						{
							return SaveDataMng.getSingleton().SaveData(nSlot)?.composit.getJobID(n) ?? (-1);
						}

						internal static int getChrID(int n, int nSlot)
						{
							return SaveDataMng.getSingleton().SaveData(nSlot)?.composit.getCharID(n) ?? (-1);
						}

						internal static int getFormation(int n, int nSlot)
						{
							return SaveDataMng.getSingleton().SaveData(nSlot)?.composit.getFormation(n) ?? (-1);
						}

						internal static bool isClearedData(int nSlot)
						{
							return SaveDataMng.getSingleton().SaveData(nSlot)?.composit.checkFlag(0, 997) ?? false;
						}

						internal static string getModelLocate(string pname)
						{
							return pname[0] switch
							{
								'j' => "/OBJ/PC", 
								'n' => "/OBJ/NPC", 
								'f' => "/OBJ/MONSTER", 
								'w' => "/OBJ/WEAPON", 
								_ => ".", 
							};
						}

						internal static string getMotionLocate(string pname)
						{
							if (pname[0] == 'b' && pname[1] == '_')
							{
								return "/MOTION/BATTLE";
							}
							if (pname[0] == 'w' && pname[1] == '_')
							{
								return "/MOTION/WORLD";
							}
							return "/MOTION/OTHERS";
						}

						internal static bool validateIDCode(byte[] pCode, string pID)
						{
							if ((byte)pID[0] == pCode[0] && (byte)pID[1] == pCode[1] && (byte)pID[2] == pCode[2])
							{
								return (byte)pID[3] == pCode[3];
							}
							return false;
						}

						internal static void ff3Command_EventStart(ScriptEngine engine)
						{
							engine.getWord();
							_ = 1;
							CCastCommandTransit.getInstance().cast_BaseSystem().State();
							int playCharacterIndex = wld.CWorldOutSideData.getInstance().PlayerData().getPlayCharacterIndex();
							CCastCommandTransit.getInstance().cast_PlayerMng().setPlayerStop(playCharacterIndex);
							pl.CPlayerHuman cPlayerHuman = CCastCommandTransit.getInstance().cast_PlayerMng().PlayerHuman(0);
							if (cPlayerHuman.getCharacterId() >= 0)
							{
								cPlayerHuman.setAction(pl.CPlayerHuman.ACTION_ID.ACTION_ID_WAIT);
							}
							evt.CEventManager.getInstance().setEvent(_Event: true);
							dv.CDeviceManager.getInstance().Pad().setActivity(b: false);
							CCastCommandTransit.getInstance().cast_PlayerMng().setAllPlayerAutoPilot(_Flag: true);
							for (int i = 0; (long)i < 24L; i++)
							{
								pl.CPlayerHuman cPlayerHuman2 = CCastCommandTransit.getInstance().cast_PlayerMng().PlayerHuman(i);
								if (cPlayerHuman2.getCharacterId() != -1 && pl.CNPCAiManager.AI_KIND.AI_KIND_RANDOM_MOVE == cPlayerHuman2.NPCAiManager().AiKind() && 4 != cPlayerHuman2.getActionId())
								{
									cPlayerHuman2.setAction(pl.CPlayerHuman.ACTION_ID.ACTION_ID_WAIT);
								}
							}
							for (byte b = 0; b < pl.MAP_OBJECT_NUM; b++)
							{
								CCastCommandTransit.getInstance().cast_PlayerMng().MapObject(b)
									.enableSignEffect(enable: false);
								CCastCommandTransit.getInstance().cast_PlayerMng().MapObject(b)
									.eraseSignEffect();
							}
							CCastCommandTransit.getInstance().cast_Field2D().setButtonShow(show: false);
							CCastCommandTransit.getInstance().cast_Field2D().refMapNameWindow()
								.close();
						}

						internal static void ff3Command_EventEnd(ScriptEngine engine)
						{
							engine.getWord();
							int playCharacterIndex = wld.CWorldOutSideData.getInstance().PlayerData().getPlayCharacterIndex();
							if (CCastCommandTransit.getInstance().cast_PlayerMng().Player(playCharacterIndex)
								.CharaKind() == chr.CHARACTER_KIND.CHARACTER_KIND_PLAYER_HUMAN)
							{
								CCastCommandTransit.getInstance().cast_PlayerMng().setPlayerStart(playCharacterIndex);
							}
							else if (CCastCommandTransit.getInstance().cast_PlayerMng().Player(playCharacterIndex)
								.CharaKind() == chr.CHARACTER_KIND.CHARACTER_KIND_PLAYER_VEHICLE)
							{
								pl.CPlayerVehicle cPlayerVehicle = static_cast<pl.CPlayerVehicle>(CCastCommandTransit.getInstance().cast_PlayerMng().Player(playCharacterIndex));
								cPlayerVehicle.setAutoPilot(_AutoPilot: false);
								VecFx32 directionForRotY = cPlayerVehicle.getDirectionForRotY();
								VecFx32 position = cPlayerVehicle.getPosition();
								VecFx32 vecFx = ff3Command_reuse_v0;
								VEC_MultAdd(4096, directionForRotY, position, vecFx);
								cPlayerVehicle.MoveSys().setTargetPoint(0, position);
								cPlayerVehicle.MoveSys().setTargetPoint(1, vecFx);
								VecFx32 vecFx2 = ff3Command_reuse_v1;
								vecFx2.copy(directionForRotY);
								vecFx2.x /= 682;
								vecFx2.y /= 682;
								vecFx2.z /= 682;
								cPlayerVehicle.setDirection(vecFx2);
								cPlayerVehicle.InputPermission_set(arg0: true);
							}
							CCastCommandTransit.getInstance().cast_PlayerMng().Player(playCharacterIndex)
								.getParamMove()
								.init();
							CCastCommandTransit.getInstance().cast_PlayerMng().Player(playCharacterIndex)
								.getParamTurn()
								.init();
							evt.CEventManager.getInstance().setEvent(_Event: false);
							evt.CEventManager.getInstance().setPartyTalkEvent(_PartyTalkEvent: false);
							evt.CEventManager.getInstance().setItemEvent(_ItemEvent: false);
							CCastCommandTransit.getInstance().cast_FieldCamera().Mode_set(cmr.CWorldCamera.MODE.MODE_AUTOFOLLOW_DEFAULT);
							CCastCommandTransit.getInstance().cast_PlayerMng().setAllPlayerAutoPilot(_Flag: false);
							dv.CDeviceManager.getInstance().Pad().setActivity(b: true);
							pl.CPlayerHuman cPlayerHuman = CCastCommandTransit.getInstance().cast_PlayerMng().PlayerHuman(0);
							if (!CCastCommandTransit.getInstance().cast_BaseSystem().IsShop() && !CCastCommandTransit.getInstance().cast_BaseSystem().IsMogNet())
							{
								CCastCommandTransit.getInstance().cast_Field2D().MenuStartButton()
									.create();
								CCastCommandTransit.getInstance().cast_Field2D().MenuStartButton()
									.setStateShow();
								CCastCommandTransit.getInstance().cast_Field2D().CameraButton()
									.create();
								if (CCastCommandTransit.getInstance().cast_Field2D().visibleMap())
								{
									CCastCommandTransit.getInstance().cast_Field2D().CameraButton()
										.setStateShow();
								}
								if (cPlayerHuman != null && cPlayerHuman.getNpc() != null)
								{
									CCastCommandTransit.getInstance().cast_Field2D().TalkButton()
										.create();
									bool flag = false;
									byte b = 0;
									while ((uint)b < 4u)
									{
										if (CCastCommandTransit.getInstance().cast_PlayerMng().PlayerVehicle(b)
											.getBoardPlayer() != null)
										{
											flag = true;
											break;
										}
										b++;
									}
									if (3 != pl.PlayerParty.instance().npc().npcId() && !flag)
									{
										CCastCommandTransit.getInstance().cast_Field2D().TalkButton()
											.setStateShow();
									}
								}
							}
							for (byte b2 = 0; b2 < pl.MAP_OBJECT_NUM; b2++)
							{
								CCastCommandTransit.getInstance().cast_PlayerMng().MapObject(b2)
									.enableSignEffect(enable: true);
							}
						}

						internal static void ff3Command_FadeIn(ScriptEngine engine)
						{
							uint dword = engine.getDword();
							uint word = engine.getWord();
							engine.getWord();
							switch (dword)
							{
							case 0u:
								dgs.CFade.Main().fadeIn((int)word);
								break;
							case 1u:
								dgs.CFade.Sub().fadeIn((int)word);
								break;
							case 2u:
								dgs.CFade.Main().fadeIn((int)word);
								dgs.CFade.Sub().fadeIn((int)word);
								break;
							}
						}

						internal static void ff3Command_FadeOut(ScriptEngine engine)
						{
							uint dword = engine.getDword();
							int word = engine.getWord();
							dgs.CFade.FADE_TYPE word2 = (dgs.CFade.FADE_TYPE)engine.getWord();
							switch (dword)
							{
							case 0u:
								dgs.CFade.Main().fadeOut(word, word2);
								break;
							case 1u:
								dgs.CFade.Sub().fadeOut(word, word2);
								break;
							case 2u:
								dgs.CFade.Main().fadeOut(word, word2);
								dgs.CFade.Sub().fadeOut(word, word2);
								break;
							}
						}

						internal static void ff3Command_DisplayMaskOn(ScriptEngine engine)
						{
							dgs.CCurtain.CURTAIN_LOCATION dword = (dgs.CCurtain.CURTAIN_LOCATION)engine.getDword();
							uint word = engine.getWord();
							uint word2 = engine.getWord();
							uint word3 = engine.getWord();
							uint word4 = engine.getWord();
							uint word5 = engine.getWord();
							dgs.CCurtain.Curtain(dword).setColor((int)word, GX_RGB((int)word2, (int)word3, (int)word4));
							dgs.CCurtain.Curtain(dword).setAlpha((int)word, (int)word5);
							dgs.CCurtain.Curtain(dword).setEnable(enable: true);
						}

						internal static void ff3Command_DisplayMaskOff(ScriptEngine engine)
						{
							dgs.CCurtain.CURTAIN_LOCATION dword = (dgs.CCurtain.CURTAIN_LOCATION)engine.getDword();
							uint word = engine.getWord();
							dgs.CCurtain.Curtain(dword).setAlpha((int)word, 0);
						}

						internal static void bootCharacterImp(int hichIdx, uint logicIdx, VecFx32 pos, VecFx32 rot, VecFx32 scl, VecFx32 shadowScl, string pChrName)
						{
							if (pChrName[0] == 'j')
							{
								char[] array = new char[2];
								int num = 0;
								array[0] = pChrName[1];
								array[1] = '\0';
								num = atoi(new string(array)) - 1;
								sprintf(out pChrName, "j%d%02d", pl.PlayerParty.instance().playerForId((byte)num).playerId() + 1, pl.PlayerParty.instance().playerForId((byte)num).jobManager()
									.nowJob() + 1);
							}
							TexDivideLoader.getSingleton().tdlForceLoad();
							changeGlobalDirectory();
							int num2 = CCastCommandTransit.getInstance().cast_PlayerMng().setUpWorldCharacter(pos, rot, scl, shadowScl, pChrName, _AutoPilot: false, _Operater: false);
							TexDivideLoader.getSingleton().tdlForceLoad();
							characterMng.setupOrgTex(CCastCommandTransit.getInstance().cast_PlayerMng().Player(num2)
								.getCharacterId());
							CCastCommandTransit.getInstance().cast_PlayerMng().Player(num2)
								.into();
							CCastCommandTransit.getInstance().cast_PlayerMng().Player(num2)
								.setAutoPilot(_AutoPilot: true);
							CCastCommandTransit.getInstance().cast_PlayerMng().Player(num2)
								.LogicIndex_set(logicIdx);
							evt.CHichParameterManager.getInstance().setCharaIndex(hichIdx, num2);
							if (strcmp(pChrName, "n272") == 0)
							{
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num2)
									.setShadowAlpha(20);
							}
						}

						internal static void ff3Command_BootCharacter(ScriptEngine engine)
						{
							uint word = engine.getWord();
							engine.getByte();
							int index = -1;
							int manCastIndex = evt.CHichParameterManager.getInstance().getManCastIndex(word);
							_ = 0;
							string pChrName = const_cast<string>(evt.CHichParameterManager.getInstance().getSubParam(manCastIndex).m_CharaName);
							VecFx32 vecFx = ff3Command_reuse_v0;
							VecFx32 vecFx2 = ff3Command_reuse_v1;
							VecFx32 vecFx3 = ff3Command_reuse_v2;
							VecFx32 vecFx4 = ff3Command_reuse_v3;
							vecFx.set(4096 * static_cast<int>(evt.CHichParameterManager.getInstance().getSubParam(manCastIndex).m_Position[0]), 4096 * static_cast<int>(evt.CHichParameterManager.getInstance().getSubParam(manCastIndex).m_Position[1]), 4096 * static_cast<int>(evt.CHichParameterManager.getInstance().getSubParam(manCastIndex).m_Position[2]));
							vecFx2.set(4096 * FX_DEG_TO_IDX(static_cast<int>(evt.CHichParameterManager.getInstance().getSubParam(manCastIndex).m_Posture[0])) * -1, 4096 * FX_DEG_TO_IDX(static_cast<int>(evt.CHichParameterManager.getInstance().getSubParam(manCastIndex).m_Posture[1])) * -1, 4096 * FX_DEG_TO_IDX(static_cast<int>(evt.CHichParameterManager.getInstance().getSubParam(manCastIndex).m_Posture[2])) * -1);
							vecFx3.set(4096 * static_cast<int>(evt.CHichParameterManager.getInstance().getSubParam(manCastIndex).m_Scale[0]), 4096 * static_cast<int>(evt.CHichParameterManager.getInstance().getSubParam(manCastIndex).m_Scale[1]), 4096 * static_cast<int>(evt.CHichParameterManager.getInstance().getSubParam(manCastIndex).m_Scale[2]));
							vecFx4.set(4915, 4096, 4915);
							bootCharacterImp(manCastIndex, word, vecFx, vecFx2, vecFx3, vecFx4, pChrName);
							CCastCommandTransit.getInstance().cast_PlayerMng().Player(index)
								.flagOff(pl.CBasePlayer.CBP_FLAG.NPC_NOT_TURN_TALKED);
						}

						internal static void ff3Command_BootCharacter_AbsoluteCoordination(ScriptEngine engine)
						{
							uint word = engine.getWord();
							VecFx32 vecFx = ff3Command_reuse_v0;
							vecFx.set((int)engine.getDword(), (int)engine.getDword(), (int)engine.getDword());
							engine.getByte();
							int manCastIndex = evt.CHichParameterManager.getInstance().getManCastIndex(word);
							_ = 0;
							string pChrName = const_cast<string>(evt.CHichParameterManager.getInstance().getSubParam(manCastIndex).m_CharaName);
							VecFx32 pos = vecFx;
							VecFx32 vecFx2 = ff3Command_reuse_v1;
							VecFx32 vecFx3 = ff3Command_reuse_v2;
							VecFx32 vecFx4 = ff3Command_reuse_v3;
							vecFx2.set(4096 * FX_DEG_TO_IDX(static_cast<int>(evt.CHichParameterManager.getInstance().getSubParam(manCastIndex).m_Posture[0])) * -1, 4096 * FX_DEG_TO_IDX(static_cast<int>(evt.CHichParameterManager.getInstance().getSubParam(manCastIndex).m_Posture[1])) * -1, 4096 * FX_DEG_TO_IDX(static_cast<int>(evt.CHichParameterManager.getInstance().getSubParam(manCastIndex).m_Posture[2])) * -1);
							vecFx3.set(4096 * static_cast<int>(evt.CHichParameterManager.getInstance().getSubParam(manCastIndex).m_Scale[0]), 4096 * static_cast<int>(evt.CHichParameterManager.getInstance().getSubParam(manCastIndex).m_Scale[1]), 4096 * static_cast<int>(evt.CHichParameterManager.getInstance().getSubParam(manCastIndex).m_Scale[2]));
							vecFx4.set(4915, 4096, 4915);
							bootCharacterImp(manCastIndex, word, pos, vecFx2, vecFx3, vecFx4, pChrName);
						}

						internal static void ff3Command_BootCharacter_RelativeCoordination(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint word2 = engine.getWord();
							VecFx32 vecFx = ff3Command_reuse_v0;
							vecFx.set((int)engine.getDword(), (int)engine.getDword(), (int)engine.getDword());
							engine.getByte();
							int manCastIndex = evt.CHichParameterManager.getInstance().getManCastIndex(word);
							_ = 0;
							string pChrName = const_cast<string>(evt.CHichParameterManager.getInstance().getSubParam(manCastIndex).m_CharaName);
							VecFx32 vecFx2 = vecFx;
							VecFx32 vecFx3 = ff3Command_reuse_v1;
							VecFx32 vecFx4 = ff3Command_reuse_v2;
							VecFx32 vecFx5 = ff3Command_reuse_v3;
							vecFx3.set(4096 * FX_DEG_TO_IDX(static_cast<int>(evt.CHichParameterManager.getInstance().getSubParam(manCastIndex).m_Posture[0])) * -1, 4096 * FX_DEG_TO_IDX(static_cast<int>(evt.CHichParameterManager.getInstance().getSubParam(manCastIndex).m_Posture[1])) * -1, 4096 * FX_DEG_TO_IDX(static_cast<int>(evt.CHichParameterManager.getInstance().getSubParam(manCastIndex).m_Posture[2])) * -1);
							vecFx4.set(4096 * static_cast<int>(evt.CHichParameterManager.getInstance().getSubParam(manCastIndex).m_Scale[0]), 4096 * static_cast<int>(evt.CHichParameterManager.getInstance().getSubParam(manCastIndex).m_Scale[1]), 4096 * static_cast<int>(evt.CHichParameterManager.getInstance().getSubParam(manCastIndex).m_Scale[2]));
							vecFx5.set(4915, 4096, 4915);
							int num = CCastCommandTransit.getInstance().changeHichNumber(word2);
							if (num != -1)
							{
								VecFx32 position = CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.getPosition();
								VEC_Add(vecFx2, position, vecFx2);
							}
							bootCharacterImp(manCastIndex, word, vecFx2, vecFx3, vecFx4, vecFx5, pChrName);
						}

						internal static void ff3Command_DeleteCharacter(ScriptEngine engine)
						{
							uint word = engine.getWord();
							byte b = engine.getByte();
							TexDivideLoader.getSingleton().tdlForceLoad();
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num != -1)
							{
								if (b == 0)
								{
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.terminate();
								}
								else
								{
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.setAutoDeleteFrame(b);
								}
							}
						}

						internal static void ff3Command_DisplayCharacter(ScriptEngine engine)
						{
							uint word = engine.getWord();
							bool hidden = engine.getDword() == 0;
							engine.getWord();
							int num = (int)(word & 0x3FFF);
							int num2 = num;
							if (num2 == 4)
							{
								if (pl.PlayerParty.instance().npc().isEnable())
								{
									CCastCommandTransit.getInstance().cast_BaseSystem().PlayerMng()
										.Player(1)?.setHidden(hidden);
								}
								return;
							}
							int num3 = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num3 != -1)
							{
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num3)
									.setHidden(hidden);
							}
						}

						internal static void ff3Command_GravityCharacter(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint dword = engine.getDword();
							engine.getWord();
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num != -1)
							{
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.isGrv_set((dword != 0) ? true : false);
							}
						}

						internal static void ff3Command_ChangeColorCharacter(ScriptEngine engine)
						{
							uint word = engine.getWord();
							string text = engine.getString();
							int index = CCastCommandTransit.getInstance().changeHichNumber(word);
							int characterId = CCastCommandTransit.getInstance().cast_PlayerMng().Player(index)
								.getCharacterId();
							int manCastIndex = evt.CHichParameterManager.getInstance().getManCastIndex(word);
							string charaName = evt.CHichParameterManager.getInstance().getSubParam(manCastIndex).m_CharaName;
							string arg = "";
							sprintf(out arg, "%s_%s", charaName, text);
							characterMng.releaseTex(characterId);
							characterMng.bindReplaceTex(characterId, arg);
							TexDivideLoader.getSingleton().tdlForceLoad();
							characterMng.setupReplaceTex(characterId);
							characterMng.releaseMdlTexRes(characterId);
						}

						internal static void ff3Command_StartMotionCharacter(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint dword = engine.getDword();
							uint dword2 = engine.getDword();
							uint word2 = engine.getWord();
							uint word3 = engine.getWord();
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num != -1)
							{
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.startMotion((int)dword, (dword2 != 0) ? true : false, word2);
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setCurrentFrame(word3);
								for (int i = 0; i < 8; i++)
								{
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.resetStockMotionParameter(i);
								}
							}
						}

						internal static void ff3Command_EndMotionCharacter(ScriptEngine engine)
						{
							uint word = engine.getWord();
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num != -1 && !CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
								.isEndOfMotion())
							{
								engine.suspendRedo();
							}
						}

						internal static void MoveCharaImp(int playerIdx, int frame, VecFx32 targetPos, int moveSpeed)
						{
							CCastCommandTransit.getInstance().cast_PlayerMng().Player(playerIdx)
								.setAutoPilot(_AutoPilot: true);
							if (CCastCommandTransit.getInstance().cast_PlayerMng().Player(playerIdx)
								.CharaKind() == chr.CHARACTER_KIND.CHARACTER_KIND_PLAYER_HUMAN)
							{
								CCastCommandTransit.getInstance().cast_PlayerMng().PlayerHuman(playerIdx)
									.setNextAct(0);
								CCastCommandTransit.getInstance().cast_PlayerMng().PlayerHuman(playerIdx)
									.NPCAiManager()
									.AiKind_set(pl.CNPCAiManager.AI_KIND.AI_KIND_DEFAULT);
							}
							if (frame == 0)
							{
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(playerIdx)
									.setPosition(const_cast<VecFx32>(targetPos));
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(playerIdx)
									.getPrePosition_set(CCastCommandTransit.getInstance().cast_PlayerMng().Player(playerIdx)
										.getPosition());
							}
							else
							{
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(playerIdx)
									.MoveSys()
									.setFlag(_Flag: true);
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(playerIdx)
									.MoveSys()
									.setTargetPoint(0, CCastCommandTransit.getInstance().cast_PlayerMng().Player(playerIdx)
										.getPosition());
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(playerIdx)
									.MoveSys()
									.setTargetPoint(1, targetPos);
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(playerIdx)
									.MoveSys()
									.setMoveFrame(frame);
								VecFx32 vecFx = ff3Command_reuse_va;
								vecFx.set(moveSpeed, moveSpeed, moveSpeed);
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(playerIdx)
									.setMoveMax(moveSpeed);
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(playerIdx)
									.setMoveAcc(moveSpeed);
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(playerIdx)
									.setMoveDec(0);
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(playerIdx)
									.setMove(vecFx);
								int id = (int)CCastCommandTransit.getInstance().cast_PlayerMng().Player(playerIdx)
									.PlayerMoveType();
								pl.CPlayerWorldMoveParameter cPlayerWorldMoveParameter = pl.CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter(id);
								int turnAcc = (int)cPlayerWorldMoveParameter.NTrnAcc();
								int turnDec = 0;
								int turnMax = (int)cPlayerWorldMoveParameter.NTrnMax();
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(playerIdx)
									.setTurnAcc(turnAcc);
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(playerIdx)
									.setTurnDec(turnDec);
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(playerIdx)
									.setTurnMax(turnMax);
							}
							CCastCommandTransit.getInstance().cast_PlayerMng().Player(playerIdx)
								.MoveSys()
								.setLinearMove();
						}

						internal static void ff3Command_MoveCharacter_AbsoluteCoordination(ScriptEngine engine)
						{
							uint word = engine.getWord();
							VecFx32 vecFx = ff3Command_reuse_v0;
							vecFx.set((int)engine.getDword(), (int)engine.getDword(), (int)engine.getDword());
							int word2 = engine.getWord();
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num != -1)
							{
								int num2 = VEC_Distance(CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.getPosition(), vecFx);
								int moveSpeed = (int)(((word2 == 0) ? num2 : ((long)num2 / (long)word2)) / 6);
								MoveCharaImp(num, word2, vecFx, moveSpeed);
							}
						}

						internal static void ff3Command_MoveCharacter_AbsoluteCoordination2(ScriptEngine engine)
						{
							uint word = engine.getWord();
							VecFx32 vecFx = ff3Command_reuse_v0;
							vecFx.set((int)engine.getDword(), (int)engine.getDword(), (int)engine.getDword());
							int word2 = engine.getWord();
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (-1 != num)
							{
								int num2 = VEC_Distance(CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.getPosition(), vecFx);
								int num3 = num2;
								if (word2 != 0)
								{
									num3 /= word2;
								}
								if (word2 == 0)
								{
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.setPosition(vecFx);
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.getPrePosition_set(vecFx);
								}
								else
								{
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.MoveSys()
										.setFlag(_Flag: true);
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.MoveSys()
										.setAutoMoveType(1);
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.MoveSys()
										.setTargetPoint(0, CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
											.getPosition());
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.MoveSys()
										.setTargetPoint(1, vecFx);
									VecFx32 vecFx2 = ff3Command_reuse_v1;
									vecFx2.set(num3, num3, num3);
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.setMoveMax(num3);
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.setMoveAcc(num3);
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.setMoveDec(0);
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.setMove(vecFx2);
									int turnAcc = (int)pl.CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.PlayerMoveType()).NTrnAcc();
									int turnDec = 0;
									int turnMax = (int)pl.CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.PlayerMoveType()).NTrnMax();
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.setTurnAcc(turnAcc);
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.setTurnDec(turnDec);
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.setTurnMax(turnMax);
								}
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.MoveSys()
									.setCorrectLinearMove();
							}
						}

						internal static void ff3Command_MoveCharacter_RelativeCoordination(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint word2 = engine.getWord();
							VecFx32 vecFx = ff3Command_reuse_v0;
							vecFx.set((int)engine.getDword(), (int)engine.getDword(), (int)engine.getDword());
							uint word3 = engine.getWord();
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num != -1)
							{
								int num2 = CCastCommandTransit.getInstance().changeHichNumber(word2);
								if (num2 != -1)
								{
									VecFx32 vecFx2 = ff3Command_reuse_v1;
									vecFx2.copy(CCastCommandTransit.getInstance().cast_PlayerMng().Player(num2)
										.getPosition());
									VEC_Add(vecFx2, vecFx, vecFx2);
									int num3 = VEC_Distance(CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.getPosition(), vecFx2);
									int moveSpeed = (int)(((word3 == 0) ? num3 : (num3 / word3)) / 6);
									MoveCharaImp(num, (int)word3, vecFx2, moveSpeed);
								}
							}
						}

						internal static void ff3Command_MoveFixedSpeedCharacter_AbsoluteCoordination(ScriptEngine engine)
						{
							uint hichIndex = static_cast<uint>(engine.getWord());
							VecFx32 vecFx = ff3Command_reuse_v0;
							vecFx.set(static_cast<int>(engine.getDword()), static_cast<int>(engine.getDword()), static_cast<int>(engine.getDword()));
							int num = static_cast<int>(engine.getDword());
							int num2 = CCastCommandTransit.getInstance().changeHichNumber(hichIndex);
							if (num2 != -1)
							{
								int denom = VEC_Distance(CCastCommandTransit.getInstance().cast_PlayerMng().Player(num2)
									.getPosition(), vecFx);
								int num3 = FX_Div(4096 * num, denom);
								MoveCharaImp(num2, num3 / 4096, vecFx, num);
							}
						}

						internal static void ff3Command_MoveFixedSpeedCharacter_RelativeCoordination(ScriptEngine engine)
						{
							uint hichIndex = static_cast<uint>(engine.getWord());
							uint hichIndex2 = static_cast<uint>(engine.getWord());
							VecFx32 vecFx = ff3Command_reuse_v0;
							vecFx.set(static_cast<int>(engine.getDword()), static_cast<int>(engine.getDword()), static_cast<int>(engine.getDword()));
							int num = static_cast<int>(engine.getDword());
							int num2 = CCastCommandTransit.getInstance().changeHichNumber(hichIndex);
							int num3 = CCastCommandTransit.getInstance().changeHichNumber(hichIndex2);
							if (num2 != -1 && num3 != -1)
							{
								VecFx32 vecFx2 = ff3Command_reuse_v1;
								vecFx2.copy(CCastCommandTransit.getInstance().cast_PlayerMng().Player(num3)
									.getPosition());
								VEC_Add(vecFx2, vecFx, vecFx2);
								int denom = VEC_Distance(CCastCommandTransit.getInstance().cast_PlayerMng().Player(num2)
									.getPosition(), vecFx2);
								int num4 = FX_Div(4096 * num, denom);
								MoveCharaImp(num2, num4 / 4096, vecFx2, num);
							}
						}

						internal static void ff3Command_MoveCharacter_EndAutoIdle(ScriptEngine engine)
						{
							uint word = engine.getWord();
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num != -1 && CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
								.MoveSys()
								.isFlag())
							{
								engine.suspendRedo();
							}
						}

						internal static void ff3Command_MoveCharacter_StartRandom(ScriptEngine engine)
						{
							uint word = engine.getWord();
							byte arg = engine.getByte();
							engine.getByte();
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num != -1 && (long)num < 24L)
							{
								CCastCommandTransit.getInstance().cast_PlayerMng().PlayerHuman(num)
									.setAutoPilot(_AutoPilot: false);
								CCastCommandTransit.getInstance().cast_PlayerMng().PlayerHuman(num)
									.setOperater(_Operater: false);
								CCastCommandTransit.getInstance().cast_PlayerMng().PlayerHuman(num)
									.NPCAiManager()
									.AiKind_set(pl.CNPCAiManager.AI_KIND.AI_KIND_RANDOM_MOVE);
								CCastCommandTransit.getInstance().cast_PlayerMng().PlayerHuman(num)
									.NPCRandomMoveType_set((pl.NPC_RANDOM_MOVE_TYPE)arg);
							}
						}

						internal static void ff3Command_MoveCharacter_EndRandom(ScriptEngine engine)
						{
							uint word = engine.getWord();
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num != -1 && (long)num < 24L)
							{
								CCastCommandTransit.getInstance().cast_PlayerMng().PlayerHuman(num)
									.setOperater(_Operater: false);
								CCastCommandTransit.getInstance().cast_PlayerMng().PlayerHuman(num)
									.NPCAiManager()
									.AiKind_set(pl.CNPCAiManager.AI_KIND.AI_KIND_DEFAULT);
							}
						}

						internal static void ff3Command_TurnCharacter_AbsoluteAngle(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint dword = engine.getDword();
							uint word2 = engine.getWord();
							uint dword2 = engine.getDword();
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num == -1)
							{
								return;
							}
							CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
								.setAutoPilot(_AutoPilot: true);
							dword /= 4096;
							VecFx32 vecFx = ff3Command_reuse_v0;
							vecFx.set(0, 0, 0);
							VecFx32 rotation = CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
								.getRotation();
							int num2 = 4096 * FX_DEG_TO_IDX(static_cast<int>(dword)) * -1;
							int num3 = 0;
							switch (dword2)
							{
							case 0u:
								rotation.y = num2;
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setRotation(rotation);
								break;
							case 1u:
							case 2u:
							case 3u:
								vecFx.x = static_cast<int>(FX_SinFx64c((int)dword));
								vecFx.z = static_cast<int>(FX_CosFx64c((int)dword));
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setTargetDirection(vecFx);
								num3 = rotation.y - num2;
								if (num3 < 0)
								{
									num3 *= -1;
								}
								num3 /= (int)word2;
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setTurnAcc(num3);
								break;
							}
						}

						internal static void ff3Command_TurnCharacter_RelativeAngle(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint dword = engine.getDword();
							uint word2 = engine.getWord();
							uint dword2 = engine.getDword();
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num == -1)
							{
								return;
							}
							CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
								.setAutoPilot(_AutoPilot: true);
							dword /= 4096;
							VecFx32 vecFx = ff3Command_reuse_v0;
							vecFx.set(0, 0, 0);
							VecFx32 rotation = CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
								.getRotation();
							int num2 = 4096 * FX_DEG_TO_IDX(static_cast<int>(dword)) * -1;
							int num3 = 0;
							num2 += rotation.y;
							switch (dword2)
							{
							case 0u:
								rotation.y = num2;
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setRotation(rotation);
								break;
							case 1u:
							case 2u:
							case 3u:
								vecFx.x = static_cast<int>(FX_SinFx64c((int)dword));
								vecFx.z = static_cast<int>(FX_CosFx64c((int)dword));
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setTargetDirection(vecFx);
								num3 = rotation.y - num2;
								if (num3 < 0)
								{
									num3 *= -1;
								}
								num3 /= (int)word2;
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setTurnAcc(num3);
								break;
							}
						}

						internal static void ff3Command_TurnCharacter_AbsoluteCoordination(ScriptEngine engine)
						{
							uint word = engine.getWord();
							VecFx32 vecFx = ff3Command_reuse_v0;
							vecFx.set((int)engine.getDword(), (int)engine.getDword(), (int)engine.getDword());
							engine.getWord();
							engine.getDword();
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num != -1 && (long)num < 24L)
							{
								VecFx32 position = CCastCommandTransit.getInstance().cast_PlayerMng().PlayerHuman(num)
									.getPosition();
								VecFx32 vecFx2 = ff3Command_reuse_v1;
								vecFx2.copy(vecFx);
								VEC_Subtract(vecFx2, position, vecFx2);
								VEC_Normalize(vecFx2, vecFx2);
								vecFx2.x /= 682;
								vecFx2.y /= 682;
								vecFx2.z /= 682;
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setTargetDirection(vecFx2);
								int turnAcc = (int)pl.CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.PlayerMoveType()).NTrnAcc();
								int turnDec = 0;
								int turnMax = (int)pl.CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.PlayerMoveType()).NTrnMax();
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setTurnAcc(turnAcc);
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setTurnDec(turnDec);
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setTurnMax(turnMax);
							}
						}

						internal static void ff3Command_TurnCharacter_LookCharacter(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint word2 = engine.getWord();
							engine.getWord();
							engine.getDword();
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num != -1)
							{
								int num2 = CCastCommandTransit.getInstance().changeHichNumber(word2);
								if (num2 != -1)
								{
									VecFx32 position = CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.getPosition();
									VecFx32 vecFx = ff3Command_reuse_v0;
									vecFx.copy(CCastCommandTransit.getInstance().cast_PlayerMng().Player(num2)
										.getPosition());
									VEC_Subtract(vecFx, position, vecFx);
									VEC_Normalize(vecFx, vecFx);
									vecFx.x /= 682;
									vecFx.y /= 682;
									vecFx.z /= 682;
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.setTargetDirection(vecFx);
									int turnAcc = (int)pl.CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.PlayerMoveType()).NTrnAcc();
									int turnDec = 0;
									int turnMax = (int)pl.CPlayerWorldParameterManager.Instance().PlayerWorldMoveParameter((int)CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.PlayerMoveType()).NTrnMax();
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.setTurnAcc(turnAcc);
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.setTurnDec(turnDec);
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.setTurnMax(turnMax);
								}
							}
						}

						internal static void ff3Command_TurnCharacter_Init(ScriptEngine engine)
						{
							engine.getWord();
							engine.getWord();
							engine.getDword();
						}

						internal static void StartLoopImp(int playerIdx, uint frame, chr.CCharacterTurnSys.TURN_TYPE turnType)
						{
							int num = (int)(65536 / frame);
							if (chr.CCharacterTurnSys.TURN_TYPE.TURN_TYPE_LEFT == turnType)
							{
								num *= -1;
							}
							CCastCommandTransit.getInstance().cast_PlayerMng().Player(playerIdx)
								.TurnSys()
								.startLastingTurn((ushort)frame, num);
						}

						internal static void ff3Command_TurnCharacter_StartLoop(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint word2 = engine.getWord();
							uint dword = engine.getDword();
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num != -1)
							{
								StartLoopImp(num, word2, (chr.CCharacterTurnSys.TURN_TYPE)dword);
							}
						}

						internal static void ff3Command_TurnCharacter_EndLoop(ScriptEngine engine)
						{
							uint word = engine.getWord();
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num != -1)
							{
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.TurnSys()
									.endLastingTurn();
							}
						}

						internal static void ff3Command_TurnCharacter_EndAutoIdle(ScriptEngine engine)
						{
							engine.getWord();
						}

						internal static void ff3Command_SetNextMessageAlignment(ScriptEngine engine)
						{
							int num = (int)engine.getDword();
							switch (num)
							{
							case 0:
								num = 512;
								break;
							case 1:
								num = 2048;
								break;
							case 2:
								num = 1024;
								break;
							}
							CCastCommandTransit.getInstance().cast_Field2D().MessageWindow()
								.setMessageAlignment(num);
						}

						internal static void ff3Command_ChangeFontSize(ScriptEngine engine)
						{
							uint word = engine.getWord();
							CCastCommandTransit.getInstance().cast_Field2D().MessageWindow()
								.setMessageFontSize((int)word);
						}

						internal static void ff3Command_EndMessage(ScriptEngine engine)
						{
							engine.getWord();
							engine.getWord();
							if (CCastCommandTransit.getInstance().cast_Field2D().MessageWindow()
								.isMadeWindow())
							{
								engine.suspendRedo();
							}
						}

						internal static void ff3Command_CheckPlayer_State(ScriptEngine engine)
						{
							uint dword = engine.getDword();
							uint dword2 = engine.getDword();
							if (CCastCommandTransit.getInstance().cast_PlayerMng().checkPlayerType((pl.PLAYER_TYPE)dword))
							{
								engine.jump(dword2);
							}
						}

						internal static void ff3Command_AlterPlayer_State(ScriptEngine engine)
						{
							engine.getDword();
							engine.getWord();
						}

						internal static void ff3Command_BootEventBattle(ScriptEngine engine)
						{
							uint word = engine.getWord();
							byte battleMapId = engine.getByte();
							btl.OutsideToBattle.getInstance().initializeMonster().setMonsterPartyId((short)word);
							btl.OutsideToBattle.getInstance().initializeBattleMap().setBattleMapId(battleMapId);
							wld.CBaseSystem.setBattle(b: true);
						}

						internal static void ff3Command_MapWarp(ScriptEngine engine)
						{
							string arg = engine.getString();
							VecFx32 vecFx = ff3Command_reuse_v0;
							vecFx.set((int)engine.getDword(), (int)engine.getDword(), (int)engine.getDword());
							uint dword = engine.getDword();
							uint dword2 = engine.getDword();
							engine.getWord();
							VecFx32 pos = vecFx;
							VecFx32 vecFx2 = ff3Command_reuse_v1;
							vecFx2.set(0, 0, 0);
							switch (dword)
							{
							case 0u:
								vecFx2.y = 0;
								break;
							case 1u:
								vecFx2.y = 8192;
								break;
							case 2u:
								vecFx2.y = 16384;
								break;
							case 3u:
								vecFx2.y = 24576;
								break;
							case 4u:
								vecFx2.y = 32768;
								break;
							case 5u:
								vecFx2.y = 40960;
								break;
							case 6u:
								vecFx2.y = 49152;
								break;
							case 7u:
								vecFx2.y = 57344;
								break;
							}
							CCastCommandTransit.getInstance().castParam_MapJump().initialize();
							CCastCommandTransit.getInstance().castParam_MapJump().setUp(const_cast<string>(arg), (sbyte)dword2, pos, vecFx2, _Flag: true);
							CCastCommandTransit.getInstance().cast_BaseSystem().setMapJump(b: true);
						}

						internal static void ff3Command_AddItem(ScriptEngine engine)
						{
							uint word = engine.getWord();
							byte b = engine.getByte();
							int num = (int)word;
							int itemNum = b;
							pl.PlayerParty.instance().addItem(num, itemNum);
							dgs.CCtrlCodeInterface.instance().setItemId(num);
						}

						internal static void ff3Command_SetTreasureItem(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint word2 = engine.getWord();
							uint word3 = engine.getWord();
							uint word4 = engine.getWord();
							uint word5 = engine.getWord();
							byte b = engine.getByte();
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num == -1)
							{
								return;
							}
							num -= (int)(pl.FIELD_CHARACTER_NUM - pl.MAP_OBJECT_NUM);
							CCastCommandTransit.getInstance().cast_PlayerMng().MapObject(num)
								.setFlag(word3, word4);
							if (FlagManager.singleton().get(word3, word4) == 1)
							{
								if (b == 0)
								{
									CCastCommandTransit.getInstance().cast_PlayerMng().MapObject(num)
										.setNowAct(6);
								}
								return;
							}
							CCastCommandTransit.getInstance().cast_PlayerMng().MapObject(num)
								.setEnCountIndex(word5);
							CCastCommandTransit.getInstance().cast_PlayerMng().MapObject(num)
								.setItemId(word2);
							int itemNum = 1;
							if (itm.ItemManager.instance().itemCategory((short)word2) == itm.CATEGORY.CATEGORY_WEAPON && itm.ItemManager.instance().itemParameter((short)word2).system() == 8)
							{
								itemNum = 20;
							}
							CCastCommandTransit.getInstance().cast_PlayerMng().MapObject(num)
								.setItemNum(itemNum);
							if (b == 0)
							{
								CCastCommandTransit.getInstance().cast_PlayerMng().MapObject(num)
									.startMotion(1003, _Loop: false, 5u);
								CCastCommandTransit.getInstance().cast_PlayerMng().MapObject(num)
									.setCurrentFrame(CCastCommandTransit.getInstance().cast_PlayerMng().MapObject(num)
										.getMaxFrame());
								CCastCommandTransit.getInstance().cast_PlayerMng().MapObject(num)
									.setNowAct(2);
							}
						}

						internal static void ff3Command_SetRecovery(ScriptEngine engine)
						{
							engine.getDword();
							engine.getByte();
						}

						internal static void ff3Command_BootShop(ScriptEngine engine)
						{
							byte arg = engine.getByte();
							engine.getByte();
							shop.CShopManager.Instance().ShopIndex_set(arg);
							CCastCommandTransit.getInstance().cast_BaseSystem().setShop(b: true);
						}

						internal static void ff3Command_MoveCamera_AbsoluteCoordination(ScriptEngine engine)
						{
							VecFx32 vecFx = ff3Command_reuse_v0;
							vecFx.set(static_cast<int>(engine.getDword()), static_cast<int>(engine.getDword()), static_cast<int>(engine.getDword()));
							engine.getWord();
							engine.getDword();
							if (vecFx.x == 0 && vecFx.y == 0 && vecFx.z == 0)
							{
								vecFx.y = 4096;
							}
							CCastCommandTransit.getInstance().cast_FieldCamera().Mode_set(cmr.CWorldCamera.MODE.MODE_FREE);
							CCastCommandTransit.getInstance().cast_FieldCamera().Pos_set(vecFx);
							VEC_Set(CCastCommandTransit.getInstance().cast_FieldCamera().PosOffset(), 0, 0, 0);
						}

						internal static void ff3Command_MoveCamera_RelativeCoordination(ScriptEngine engine)
						{
							VecFx32 vecFx = ff3Command_reuse_v0;
							vecFx.set(static_cast<int>(engine.getDword()), static_cast<int>(engine.getDword()), static_cast<int>(engine.getDword()));
							engine.getWord();
							engine.getDword();
							if (vecFx.x == 0 && vecFx.y == 0 && vecFx.z == 0)
							{
								vecFx.y = 4096;
							}
							CCastCommandTransit.getInstance().cast_FieldCamera().Mode_set(cmr.CWorldCamera.MODE.MODE_FREE);
							CCastCommandTransit.getInstance().cast_FieldCamera().Pos_set(vecFx);
							VEC_Set(CCastCommandTransit.getInstance().cast_FieldCamera().PosOffset(), 0, 0, 0);
						}

						internal static void ff3Command_MoveCamera_LookPlayer(ScriptEngine engine)
						{
							uint word = engine.getWord();
							int num = 0;
							int manCastIndex = evt.CHichParameterManager.getInstance().getManCastIndex(word);
							_ = 0;
							CCastCommandTransit.getInstance().cast_FieldCamera().Mode_set(cmr.CWorldCamera.MODE.MODE_AUTOFOLLOW);
							num = ((word != 0) ? evt.CHichParameterManager.getInstance().CharaIndex(manCastIndex) : 0);
							chr.CBaseCharacter.setLookIndex(num);
							CCastCommandTransit.getInstance().cast_FieldCamera().setTrg(CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
								.getPosition());
						}

						internal static void ff3Command_BootMovei(ScriptEngine engine)
						{
							engine.getWord();
							engine.getByte();
						}

						internal static void ff3Command_WithInCharacterJump(ScriptEngine engine)
						{
							uint word = engine.getWord();
							VecFx32 vecFx = ff3Command_reuse_v0;
							VecFx32 vecFx2 = ff3Command_reuse_v1;
							vecFx.set((int)engine.getDword(), (int)engine.getDword(), (int)engine.getDword());
							vecFx2.set((int)engine.getDword(), (int)engine.getDword(), (int)engine.getDword());
							uint dword = engine.getDword();
							if (wld.CBaseSystem.WORLD_STATE.WORLD_STATE_MOVE != CCastCommandTransit.getInstance().cast_BaseSystem().State())
							{
								engine.suspendRedo();
							}
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num != -1)
							{
								VecFx32 position = CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.getPosition();
								if (vecFx.x <= position.x && vecFx.z <= position.z && position.x <= vecFx2.x && position.z <= vecFx2.z)
								{
									engine.jump(dword);
								}
							}
						}

						internal static void ff3Command_WithOutCharacterJump(ScriptEngine engine)
						{
							uint word = engine.getWord();
							VecFx32 vecFx = ff3Command_reuse_v0;
							VecFx32 vecFx2 = ff3Command_reuse_v1;
							vecFx.set((int)engine.getDword(), (int)engine.getDword(), (int)engine.getDword());
							vecFx2.set((int)engine.getDword(), (int)engine.getDword(), (int)engine.getDword());
							uint dword = engine.getDword();
							if (wld.CBaseSystem.WORLD_STATE.WORLD_STATE_MOVE != CCastCommandTransit.getInstance().cast_BaseSystem().State())
							{
								engine.suspendRedo();
							}
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num != -1)
							{
								VecFx32 position = CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.getPosition();
								if (vecFx.x > position.x || vecFx.z > position.z || position.x > vecFx2.x || position.z > vecFx2.z)
								{
									engine.jump(dword);
								}
							}
						}

						internal static void ff3Command_WithInCharacterJump2(ScriptEngine engine)
						{
							uint word = engine.getWord();
							VecFx32 vecFx = ff3Command_reuse_v0;
							VecFx32 vecFx2 = ff3Command_reuse_v1;
							vecFx.set((int)engine.getDword(), (int)engine.getDword(), (int)engine.getDword());
							vecFx2.set((int)engine.getDword(), (int)engine.getDword(), (int)engine.getDword());
							uint dword = engine.getDword();
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num != -1)
							{
								VecFx32 position = CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.getPosition();
								if (vecFx.x <= position.x && vecFx.z <= position.z && position.x <= vecFx2.x && position.z <= vecFx2.z)
								{
									engine.jump(dword);
								}
							}
						}

						internal static void ff3Command_WithOutCharacterJump2(ScriptEngine engine)
						{
							uint word = engine.getWord();
							VecFx32 vecFx = ff3Command_reuse_v0;
							VecFx32 vecFx2 = ff3Command_reuse_v1;
							vecFx.set((int)engine.getDword(), (int)engine.getDword(), (int)engine.getDword());
							vecFx2.set((int)engine.getDword(), (int)engine.getDword(), (int)engine.getDword());
							uint dword = engine.getDword();
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num != -1)
							{
								VecFx32 position = CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.getPosition();
								if (vecFx.x > position.x || vecFx.z > position.z || position.x > vecFx2.x || position.z > vecFx2.z)
								{
									engine.jump(dword);
								}
							}
						}

						internal static void ff3Command_BootEffect_AbsoluteCoordination(ScriptEngine engine)
						{
							int word = engine.getWord();
							int word2 = engine.getWord();
							engine.getWord();
							engine.getDword();
							VecFx32 vecFx = ff3Command_reuse_v0;
							vecFx.set((int)engine.getDword(), (int)engine.getDword(), (int)engine.getDword());
							int num = -1;
							num = eff.CEffectMng.instance().create(word, word2);
							if (num != -1)
							{
								eff.CEffectMng.instance().setPosition(num, vecFx);
							}
						}

						internal static void ff3Command_BootEffect_RelativeCoordination_Foolow(ScriptEngine engine)
						{
							int word = engine.getWord();
							int word2 = engine.getWord();
							engine.getWord();
							engine.getWord();
							uint word3 = engine.getWord();
							VecFx32 vecFx = ff3Command_reuse_v0;
							vecFx.set((int)engine.getDword(), (int)engine.getDword(), (int)engine.getDword());
							int num = CCastCommandTransit.getInstance().changeHichNumber(word3);
							if (num != -1)
							{
								int num2 = -1;
								num2 = eff.CEffectMng.instance().create(word, word2);
								if (num2 != -1)
								{
									VecFx32 vecFx2 = vecFx;
									VEC_Add(vecFx2, CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.getPosition(), vecFx2);
									eff.CEffectMng.instance().setPosition(num2, vecFx2);
								}
							}
						}

						internal static void ff3Command_DeleteEffect(ScriptEngine engine)
						{
							int word = engine.getWord();
							engine.getWord();
							if (eff.CEffectMng.instance().isEffectObject(word))
							{
								eff.CEffectMng.instance().deleteEffect(word);
							}
						}

						internal static void ff3Command_PauseEffect(ScriptEngine engine)
						{
							int id = static_cast<int>(engine.getWord());
							bool flag = ((engine.getWord() != 0) ? true : false);
							if (eff.CEffectMng.instance().isEffectObject(id))
							{
								eff.CEffectMng.instance().setPause(id, flag);
							}
						}

						internal static void ff3Command_SetEffect_Scale(ScriptEngine engine)
						{
							int id = static_cast<int>(engine.getWord());
							engine.getWord();
							VecFx32 vecFx = ff3Command_reuse_v0;
							vecFx.set(static_cast<int>(engine.getDword()), static_cast<int>(engine.getDword()), static_cast<int>(engine.getDword()));
							vecFx.z *= -1;
							if (eff.CEffectMng.instance().isEffectObject(id))
							{
								eff.CEffectMng.instance().setScale(id, vecFx);
							}
						}

						internal static void ff3Command_StartTalkWait(ScriptEngine engine)
						{
							engine.getWord();
							engine.getWord();
						}

						internal static void ff3Command_EndTalkJump(ScriptEngine engine)
						{
							engine.getWord();
							engine.getWord();
							engine.getDword();
						}

						internal static void ff3Command_Spring(ScriptEngine engine)
						{
							switch (engine.getDword())
							{
							case 0u:
							{
								for (int l = 0; l < 4; l++)
								{
									if (pl.PlayerParty.instance().player((byte)l).isEnable() && !pl.PlayerParty.instance().player((byte)l).condition()
										.isDeath())
									{
										pl.PlayerParty.instance().player((byte)l).recoverHPandMP();
									}
								}
								break;
							}
							case 1u:
							{
								for (int j = 0; j < 4; j++)
								{
									if (pl.PlayerParty.instance().player((byte)j).isEnable() && pl.PlayerParty.instance().player((byte)j).condition()
										.isDeath())
									{
										pl.PlayerParty.instance().player((byte)j).hp()
											.maxNow();
										pl.PlayerParty.instance().player((byte)j).condition()
											.offDeath();
									}
								}
								break;
							}
							case 2u:
							{
								for (int k = 0; k < 4; k++)
								{
									if (!pl.PlayerParty.instance().player((byte)k).isEnable())
									{
										continue;
									}
									bool flag = pl.PlayerParty.instance().player((byte)k).condition()
										.isDeath();
									bool flag2 = pl.PlayerParty.instance().player((byte)k).condition()
										.isFrog();
									bool flag3 = pl.PlayerParty.instance().player((byte)k).condition()
										.isLilliput();
									pl.PlayerParty.instance().player((byte)k).condition()
										.clearCondition();
									if (flag)
									{
										pl.PlayerParty.instance().player((byte)k).condition()
											.onDeath();
									}
									if (k == 0 && (flag2 || flag3))
									{
										bool model_change = flag2;
										int frontPlayerID = wld.CWorldOutSideData.getInstance().PlayerData().getFrontPlayerID();
										CCastCommandTransit.getInstance().cast_BaseSystem().PlayerMng()
											.PlayerHuman(0)
											.returnHuman(model_change, frontPlayerID, -1);
										bool flag4 = true;
										int num = CCastCommandTransit.getInstance().cast_BaseSystem().npcEntryId();
										if (-1 == num || 3 == pl.PlayerParty.instance().npc().npcId())
										{
											flag4 = false;
										}
										pl.CPlayerHuman cPlayerHuman = CCastCommandTransit.getInstance().cast_PlayerMng().PlayerHuman(num);
										if (cPlayerHuman == null)
										{
											flag4 = false;
										}
										if (flag4)
										{
											cPlayerHuman.returnHumanForNpc(null);
										}
									}
								}
								break;
							}
							case 3u:
							{
								for (int i = 0; i < 4; i++)
								{
									if (pl.PlayerParty.instance().player((byte)i).isEnable() && pl.PlayerParty.instance().player((byte)i).condition()
										.isDeath())
									{
										pl.PlayerParty.instance().player((byte)i).condition()
											.offDeath();
									}
								}
								break;
							}
							}
						}

						internal static void ff3Command_FadeEndWait(ScriptEngine engine)
						{
							switch (engine.getDword())
							{
							case 0u:
								if (!dgs.CFade.Main().isCleared())
								{
									engine.suspendRedo();
								}
								break;
							case 1u:
								if (!dgs.CFade.Sub().isCleared())
								{
									engine.suspendRedo();
								}
								break;
							case 2u:
								if (!dgs.CFade.Main().isCleared() || !dgs.CFade.Sub().isCleared())
								{
									engine.suspendRedo();
								}
								break;
							}
						}

						internal static void ff3Command_SetCamera_AbsoluteGaze(ScriptEngine engine)
						{
							VecFx32 vecFx = ff3Command_reuse_v0;
							vecFx.set(static_cast<int>(engine.getDword()), static_cast<int>(engine.getDword()), static_cast<int>(engine.getDword()));
							engine.getWord();
							engine.getDword();
							if (vecFx.x == 0 && vecFx.y == 0 && vecFx.z == 0)
							{
								vecFx.y = 4096;
							}
							CCastCommandTransit.getInstance().cast_FieldCamera().Mode_set(cmr.CWorldCamera.MODE.MODE_FREE);
							CCastCommandTransit.getInstance().cast_FieldCamera().Trg_set(vecFx);
							VEC_Set(CCastCommandTransit.getInstance().cast_FieldCamera().TrgOffset(), 0, 0, 0);
						}

						internal static void ff3Command_SetCamera_RelativeGaze(ScriptEngine engine)
						{
							VecFx32 vecFx = ff3Command_reuse_v0;
							vecFx.set(static_cast<int>(engine.getDword()), static_cast<int>(engine.getDword()), static_cast<int>(engine.getDword()));
							engine.getWord();
							engine.getDword();
							if (vecFx.x == 0 && vecFx.y == 0 && vecFx.z == 0)
							{
								vecFx.y = 4096;
							}
							CCastCommandTransit.getInstance().cast_FieldCamera().Mode_set(cmr.CWorldCamera.MODE.MODE_FREE);
							VEC_Add(CCastCommandTransit.getInstance().cast_FieldCamera().Trg(), vecFx, CCastCommandTransit.getInstance().cast_FieldCamera().Trg());
							VEC_Set(CCastCommandTransit.getInstance().cast_FieldCamera().TrgOffset(), 0, 0, 0);
						}

						internal static void ff3Command_ChangeCamera_Mode(ScriptEngine engine)
						{
							uint dword = engine.getDword();
							CCastCommandTransit.getInstance().cast_FieldCamera().Mode_set(static_cast<cmr.CWorldCamera.MODE>(dword));
						}

						internal static void ff3Command_SetCamera_Type(ScriptEngine engine)
						{
							uint dword = engine.getDword();
							CCastCommandTransit.getInstance().cast_FieldCamera().Type_set((cmr.CWorldCamera.TYPE)dword);
						}

						internal static void ff3Command_SetCamera_Angle(ScriptEngine engine)
						{
							VecFx32 vecFx = ff3Command_reuse_v0;
							vecFx.set((int)engine.getDword(), (int)engine.getDword(), (int)engine.getDword());
							engine.getWord();
							VecFx32 vecFx2 = vecFx;
							if (vecFx2.x == 0 && vecFx2.y == 0 && vecFx2.z == 0)
							{
								vecFx2.y = 4096;
							}
							vecFx2.x /= 4096;
							vecFx2.y /= 4096;
							vecFx2.z /= 4096;
							vecFx2.x *= 128;
							vecFx2.y *= 128;
							vecFx2.z *= 128;
							CCastCommandTransit.getInstance().cast_FieldCamera().Mode_set(cmr.CWorldCamera.MODE.MODE_FREE);
							CCastCommandTransit.getInstance().cast_FieldCamera().Angle_set(vecFx2);
							VEC_Set(CCastCommandTransit.getInstance().cast_FieldCamera().AngleOffset(), 0, 0, 0);
						}

						internal static void ff3Command_SetCamera_NearFarClip(ScriptEngine engine)
						{
							int near = engine.getWord() * 4096;
							int far = engine.getWord() * 4096;
							CCastCommandTransit.getInstance().cast_FieldCamera().setClip(near, far);
						}

						internal static void ff3Command_SetCharacter_Scale(ScriptEngine engine)
						{
							uint word = engine.getWord();
							VecFx32 vecFx = ff3Command_reuse_v0;
							vecFx.set((int)engine.getDword(), (int)engine.getDword(), (int)engine.getDword());
							int word2 = engine.getWord();
							vecFx.z *= -1;
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num != -1)
							{
								if (word2 == 0)
								{
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.setScale(vecFx);
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.setShadowScale(vecFx);
									return;
								}
								VecFx32 scale = CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.getScale();
								VecFx32 vecFx2 = ff3Command_reuse_v1;
								vecFx2.set((scale.x - vecFx.x) / static_cast<int>(word2), (scale.y - vecFx.y) / static_cast<int>(word2), (scale.z - vecFx.z) / static_cast<int>(word2));
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setAutoScaleSpeed(vecFx2);
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setAutoScaleFrame(word2);
							}
						}

						internal static void ff3Command_PlayBGM(ScriptEngine engine)
						{
							int word = engine.getWord();
							byte volume = engine.getByte();
							uint word2 = engine.getWord();
							MatrixSound.MtxSoundBGM.getSingleton().stop(0, MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT0);
							MatrixSound.MtxSoundBGM.getSingleton().play(word, volume, (int)word2, MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT0);
						}

						internal static void ff3Command_StopBGM(ScriptEngine engine)
						{
							int word = engine.getWord();
							MatrixSound.MtxSoundBGM.getSingleton().stop(word, MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT0);
						}

						internal static void ff3Command_PlaySE(ScriptEngine engine)
						{
							int word = engine.getWord();
							int word2 = engine.getWord();
							int word3 = engine.getWord();
							int word4 = engine.getWord();
							MatrixSound.MtxSEHandle arg = MatrixSound.MtxSENDS_Play(word, word2, word3, word4);
							if (g_SePlayRecord.size() >= 4)
							{
								g_SePlayRecord.erase(0);
							}
							g_SePlayRecord.push_back(new RECORD(arg, word, word2));
						}

						internal static void ff3Command_StopSE(ScriptEngine engine)
						{
							int word = engine.getWord();
							int word2 = engine.getWord();
							int word3 = engine.getWord();
							for (int num = g_SePlayRecord.size() - 1; num >= 0; num--)
							{
								if (g_SePlayRecord[num].SeqArcNo == word && g_SePlayRecord[num].SeqNo == word2)
								{
									MatrixSound.MtxSENDS_Stop(g_SePlayRecord[num].handle, word3);
									g_SePlayRecord.erase(num);
									break;
								}
							}
						}

						internal static void ff3Command_StartCamera_Vibration(ScriptEngine engine)
						{
							uint num = engine.getByte();
							int word = engine.getWord();
							int word2 = engine.getWord();
							VecFx32 vecFx = ff3Command_reuse_v0;
							vecFx.set((int)engine.getDword(), (int)engine.getDword(), (int)engine.getDword());
							uint num2 = engine.getByte();
							if (num == 0)
							{
								CCastCommandTransit.getInstance().cast_FieldCamera().composit2.startVibration(cmr.CCameraVibration.VIBRATION_STATE.VIBRATION_EXE_1, word, word2, vecFx.x, vecFx.y, vecFx.z, (num2 != 0) ? true : false);
							}
							else
							{
								CCastCommandTransit.getInstance().cast_FieldCamera().composit2.startVibration(cmr.CCameraVibration.VIBRATION_STATE.VIBRATION_EXE_2, word, word2, vecFx.x, vecFx.y, vecFx.z, (num2 != 0) ? true : false);
							}
						}

						internal static void ff3Command_StartMessageWindow(ScriptEngine engine)
						{
							int word = engine.getWord();
							CCastCommandTransit.getInstance().cast_Field2D().MessageWindow()
								.createWindow(word);
						}

						internal static void ff3Command_DeleteMessageWindow(ScriptEngine engine)
						{
							engine.getWord();
							CCastCommandTransit.getInstance().cast_Field2D().MessageWindow()
								.release();
						}

						internal static void ff3Command_EndMessageWindow(ScriptEngine engine)
						{
							engine.getWord();
						}

						internal static void ff3Command_WaitEndOfMessageJump(ScriptEngine engine)
						{
							engine.getWord();
							uint dword = engine.getDword();
							engine.getWord();
							if (CCastCommandTransit.getInstance().cast_Field2D().MessageWindow()
								.isMessageProgressEnded())
							{
								engine.jump(dword);
							}
							else
							{
								engine.suspendRedo();
							}
						}

						internal static void ff3Command_DeleteNameWindow(ScriptEngine engine)
						{
							engine.getWord();
						}

						internal static void ff3Command_StartMessage2(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint dword = engine.getDword();
							byte b = engine.getByte();
							uint num = engine.getByte();
							if (evt.CEventManager.getInstance().MessageStop())
							{
								if (CCastCommandTransit.getInstance().cast_Field2D().MessageWindow()
									.isMadeMessage())
								{
									engine.suspendRedo();
								}
								else
								{
									evt.CEventManager.getInstance().MessageStop_set(arg0: false);
								}
								return;
							}
							CCastCommandTransit.getInstance().cast_BaseSystem().lastMessage_set((int)dword);
							CCastCommandTransit.getInstance().cast_Field2D().MessageWindow()
								.createMessage((int)dword, 0, (int)word);
							if (num != 0)
							{
								CCastCommandTransit.getInstance().cast_Field2D().MessageWindow()
									.setMesDeleteFrame((int)num);
							}
							if (b != 2)
							{
								CCastCommandTransit.getInstance().cast_Field2D().MessageWindow()
									.setProgressIconActivity(_SendMessage: true);
							}
							if (b == 2)
							{
								CCastCommandTransit.getInstance().cast_Field2D().MessageWindow()
									.setSendMessage(_SendMessage: false);
								return;
							}
							evt.CEventManager.getInstance().setMessageStop(_MessageStop: true);
							engine.suspendRedo();
						}

						internal static void ff3Command_DeleteMessage2(ScriptEngine engine)
						{
							engine.getWord();
							CCastCommandTransit.getInstance().cast_Field2D().MessageWindow()
								.releaseMessage();
						}

						internal static void ff3Command_SetTreasureMoney(ScriptEngine engine)
						{
							uint word = engine.getWord();
							int word2 = engine.getWord();
							uint word3 = engine.getWord();
							uint word4 = engine.getWord();
							engine.getWord();
							byte b = engine.getByte();
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num == -1)
							{
								return;
							}
							num -= (int)(pl.FIELD_CHARACTER_NUM - pl.MAP_OBJECT_NUM);
							CCastCommandTransit.getInstance().cast_PlayerMng().MapObject(num)
								.setFlag(word3, word4);
							if (FlagManager.singleton().get(word3, word4) == 1)
							{
								if (b == 0)
								{
									CCastCommandTransit.getInstance().cast_PlayerMng().MapObject(num)
										.setNowAct(6);
								}
								return;
							}
							CCastCommandTransit.getInstance().cast_PlayerMng().MapObject(num)
								.setGold(word2);
							if (b == 0)
							{
								CCastCommandTransit.getInstance().cast_PlayerMng().MapObject(num)
									.startMotion(1003, _Loop: false, 5u);
								CCastCommandTransit.getInstance().cast_PlayerMng().MapObject(num)
									.setCurrentFrame(CCastCommandTransit.getInstance().cast_PlayerMng().MapObject(num)
										.getMaxFrame());
								CCastCommandTransit.getInstance().cast_PlayerMng().MapObject(num)
									.setNowAct(2);
							}
						}

						internal static void ff3Command_SetNPCAiType(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint word2 = engine.getWord();
							byte b = engine.getByte();
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num == -1)
							{
								return;
							}
							int num2 = CCastCommandTransit.getInstance().changeHichNumber(word2);
							if (num2 == -1)
							{
								return;
							}
							pl.CBasePlayer cBasePlayer = null;
							pl.CNPCAiManager.AI_KIND aI_KIND = (pl.CNPCAiManager.AI_KIND)b;
							cBasePlayer = CCastCommandTransit.getInstance().cast_PlayerMng().Player(num2);
							if ((long)num >= 24L)
							{
								return;
							}
							CCastCommandTransit.getInstance().cast_PlayerMng().PlayerHuman(num)
								.NPCAiManager()
								.AiKind_set(aI_KIND);
							if (strcmp(CCastCommandTransit.getInstance().cast_PlayerMng().PlayerHuman(num)
								.getModelName(), "n272") != 0)
							{
								if (pl.PlayerParty.instance().isFrogAll() && !pl.PlayerParty.instance().npc().isFrog())
								{
									CCastCommandTransit.getInstance().cast_PlayerMng().PlayerHuman(num)
										.changeFrogForNpc();
								}
								if (pl.PlayerParty.instance().isLilliputAll() && !pl.PlayerParty.instance().npc().isLilliput())
								{
									CCastCommandTransit.getInstance().cast_PlayerMng().PlayerHuman(num)
										.changeLilliputForNpc();
								}
							}
							if (pl.CNPCAiManager.AI_KIND.AI_KIND_AUTO_FOLLOW == aI_KIND)
							{
								CCastCommandTransit.getInstance().cast_PlayerMng().PlayerHuman(num)
									.NPCAiManager()
									.NPC()
									.setLookPlayer(cBasePlayer);
								CCastCommandTransit.getInstance().cast_PlayerMng().PlayerHuman(num)
									.setAutoPilot(_AutoPilot: false);
								if (CCastCommandTransit.getInstance().cast_BaseSystem().Mode() == wld.CBaseSystem.WORLD_MODE.WORLD_MODE_FIELD)
								{
									CCastCommandTransit.getInstance().cast_PlayerMng().PlayerHuman(num)
										.setInPutMode(pl.INPUT_MODE.INPUT_MODE_FIELD);
									CCastCommandTransit.getInstance().cast_PlayerMng().PlayerHuman(num)
										.PlayerMoveType_set(pl.PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_HERO_FIELD);
								}
								else if (CCastCommandTransit.getInstance().cast_BaseSystem().Mode() == wld.CBaseSystem.WORLD_MODE.WORLD_MODE_TOWN)
								{
									CCastCommandTransit.getInstance().cast_PlayerMng().PlayerHuman(num)
										.setInPutMode(pl.INPUT_MODE.INPUT_MODE_TOWN);
									CCastCommandTransit.getInstance().cast_PlayerMng().PlayerHuman(num)
										.PlayerMoveType_set(pl.PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_HERO_TOWN);
								}
								CCastCommandTransit.getInstance().cast_PlayerMng().PlayerHuman(num2)
									.setNpc(CCastCommandTransit.getInstance().cast_PlayerMng().PlayerHuman(num));
								CCastCommandTransit.getInstance().cast_BaseSystem().setNpcEntryId(num);
							}
						}

						internal static void ff3Command_AddPartyPC(ScriptEngine engine)
						{
							uint word = engine.getWord();
							engine.getByte();
							byte b = (byte)word;
							b -= 5;
							pl.PlayerParty.instance().addPlayer(b);
							pl.PlayerParty.instance().clearMemory();
							CCastCommandTransit.getInstance().cast_BaseSystem().changePlayerCharDisplay();
						}

						internal static void ff3Command_SubPartyPC(ScriptEngine engine)
						{
							uint word = engine.getWord();
							engine.getByte();
							byte b = (byte)word;
							b -= 5;
							for (int i = 0; i < 4; i++)
							{
								if (pl.PlayerParty.instance().player((byte)i).isEnable())
								{
									if (pl.PlayerParty.instance().player((byte)i).playerId() != b)
									{
									}
									break;
								}
							}
							pl.PlayerParty.instance().releasePlayer(b);
							CCastCommandTransit.getInstance().cast_BaseSystem().changePlayerCharDisplay();
						}

						internal static void ff3Command_AddPartyNPC(ScriptEngine engine)
						{
							uint word = engine.getWord();
							engine.getByte();
							byte b = (byte)word;
							b -= 9;
							pl.PlayerParty.instance().addNpc(b);
							pl.CPlayerHuman cPlayerHuman = CCastCommandTransit.getInstance().cast_PlayerMng().PlayerHuman(0);
							cPlayerHuman.setTalkIcon(CCastCommandTransit.getInstance().cast_Field2D().TalkButton());
						}

						internal static void ff3Command_SubPartyNPC(ScriptEngine engine)
						{
							uint word = engine.getWord();
							engine.getByte();
							byte b = (byte)word;
							b -= 9;
							pl.PlayerParty.instance().releaseNpc(b);
							pl.CPlayerHuman cPlayerHuman = CCastCommandTransit.getInstance().cast_PlayerMng().PlayerHuman(0);
							if (cPlayerHuman != null)
							{
								pl.CBasePlayer cBasePlayer = cPlayerHuman.getNpc();
								if (cBasePlayer != null)
								{
									cBasePlayer.terminate();
									cPlayerHuman.setNpc(null);
									cPlayerHuman.setTalkIcon(null);
									CCastCommandTransit.getInstance().cast_BaseSystem().setNpcEntryId(-1);
								}
							}
						}

						internal static void ff3Command_ChangePartyPC(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint word2 = engine.getWord();
							engine.getByte();
							pl.PlayerParty.instance().changePlayer(static_cast<byte>(word), static_cast<byte>(word2));
						}

						internal static void ff3Command_CheckPartyAverageLevel(ScriptEngine engine)
						{
							uint dword = engine.getDword();
							byte b = engine.getByte();
							uint dword2 = engine.getDword();
							switch (dword)
							{
							case 0u:
								if (pl.PlayerParty.instance().averageLevel() > b)
								{
									engine.jump(dword2);
								}
								break;
							case 1u:
								if (pl.PlayerParty.instance().averageLevel() >= b)
								{
									engine.jump(dword2);
								}
								break;
							case 2u:
								if (pl.PlayerParty.instance().averageLevel() < b)
								{
									engine.jump(dword2);
								}
								break;
							case 3u:
								if (pl.PlayerParty.instance().averageLevel() <= b)
								{
									engine.jump(dword2);
								}
								break;
							case 4u:
								if (pl.PlayerParty.instance().averageLevel() == b)
								{
									engine.jump(dword2);
								}
								break;
							}
						}

						internal static void ff3Command_CheckPartyMemberNum(ScriptEngine engine)
						{
							uint dword = engine.getDword();
							byte b = engine.getByte();
							uint dword2 = engine.getDword();
							int num = 0;
							for (byte b2 = 0; b2 < 4; b2++)
							{
								if (pl.PlayerParty.instance().player(b2).isEnable())
								{
									num++;
								}
							}
							switch (dword)
							{
							case 0u:
								if (num > b)
								{
									engine.jump(dword2);
								}
								break;
							case 1u:
								if (num >= b)
								{
									engine.jump(dword2);
								}
								break;
							case 2u:
								if (num < b)
								{
									engine.jump(dword2);
								}
								break;
							case 3u:
								if (num <= b)
								{
									engine.jump(dword2);
								}
								break;
							case 4u:
								if (num == b)
								{
									engine.jump(dword2);
								}
								break;
							}
						}

						internal static void ff3Command_CheckPartyMemberRow(ScriptEngine engine)
						{
							uint word = engine.getWord();
							byte b = engine.getByte();
							uint dword = engine.getDword();
							if (pl.PlayerParty.instance().playerOrder(static_cast<byte>(word)) == b)
							{
								engine.jump(dword);
							}
						}

						internal static void ff3Command_PartyMemberFine(ScriptEngine engine)
						{
							switch (static_cast<int>(engine.getWord()))
							{
							case 0:
								pl.PlayerParty.instance().fineAll();
								break;
							case 4:
								pl.PlayerParty.instance().player(3).fine();
								break;
							case 3:
								pl.PlayerParty.instance().player(2).fine();
								break;
							case 2:
								pl.PlayerParty.instance().player(1).fine();
								break;
							case 1:
								pl.PlayerParty.instance().player(0).fine();
								break;
							}
						}

						internal static void ff3Command_BootInn(ScriptEngine engine)
						{
							uint word = engine.getWord();
							CCastCommandTransit.getInstance().cast_setInnValue((int)word);
							wld.WorldPart.getInstance().getWorldSystem().setInn(b: true);
							engine.wait(1u);
						}

						internal static void ff3Command_SetCharacter_CheckTurnType(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint dword = engine.getDword();
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num != -1)
							{
								switch (dword)
								{
								case 1u:
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.flagOn(pl.CBasePlayer.CBP_FLAG.NPC_NOT_TURN_TALKED);
									break;
								case 2u:
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.flagOn(pl.CBasePlayer.CBP_FLAG.NPC_RETURN_TALK_ENDS);
									break;
								}
							}
						}

						internal static void ff3Command_SubItem(ScriptEngine engine)
						{
							uint word = engine.getWord();
							byte b = engine.getByte();
							int itemid = (int)word;
							int num = b;
							pl.PlayerParty.instance().addItem(itemid, -num);
						}

						internal static void ff3Command_UseItem_FlagOnJump(ScriptEngine engine)
						{
							uint dword = engine.getDword();
							if (evt.CEventManager.getInstance().isItemEvent())
							{
								engine.jump(dword);
							}
						}

						internal static void ff3Command_UseItem_FlagOffJump(ScriptEngine engine)
						{
							uint dword = engine.getDword();
							if (!evt.CEventManager.getInstance().isItemEvent())
							{
								engine.jump(dword);
							}
						}

						internal static void ff3Command_CheckUseItem_Id(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint dword = engine.getDword();
							if (evt.CEventManager.getInstance().getUseItemId() == word)
							{
								engine.jump(dword);
							}
						}

						internal static void ff3Command_CheckItem_Num(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint dword = engine.getDword();
							byte b = engine.getByte();
							uint dword2 = engine.getDword();
							itm.PossessionItem possessionItem = pl.PlayerParty.instance().item().serchNormalItem((short)word);
							byte b2 = 0;
							if (possessionItem != null)
							{
								b2 = possessionItem.itemNumber();
							}
							bool flag = false;
							switch (dword)
							{
							case 0u:
								if (b2 > b)
								{
									flag = true;
								}
								break;
							case 1u:
								if (b2 >= b)
								{
									flag = true;
								}
								break;
							case 2u:
								if (b2 < b)
								{
									flag = true;
								}
								break;
							case 3u:
								if (b2 <= b)
								{
									flag = true;
								}
								break;
							case 4u:
								if (b2 == b)
								{
									flag = true;
								}
								break;
							}
							if (flag)
							{
								engine.jump(dword2);
							}
						}

						internal static void ff3Command_TouchOnJump(ScriptEngine engine)
						{
							uint dword = engine.getDword();
							engine.jump(dword);
						}

						internal static void ff3Command_TouchOffJump(ScriptEngine engine)
						{
							uint dword = engine.getDword();
							engine.jump(dword);
						}

						internal static void ff3Command_ButtonOnJump(ScriptEngine engine)
						{
							uint dword = engine.getDword();
							engine.jump(dword);
						}

						internal static void ff3Command_ButtonOffJump(ScriptEngine engine)
						{
							uint dword = engine.getDword();
							engine.jump(dword);
						}

						internal static void ff3Command_SelectEndWait(ScriptEngine engine)
						{
							uint dword = engine.getDword();
							if (CCastCommandTransit.getInstance().cast_getInnConfirm())
							{
								engine.jump(dword);
							}
						}

						internal static void ff3Command_SetCharacter_TalkMotion(ScriptEngine engine)
						{
							uint word = engine.getWord();
							bool invalidActionMotion = engine.getDword() == 0;
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num != -1)
							{
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setInvalidActionMotion(invalidActionMotion);
							}
						}

						internal static void ff3Command_MoveCamera_LookPlayer2(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint word2 = engine.getWord();
							engine.getDword();
							int num = 0;
							int manCastIndex = evt.CHichParameterManager.getInstance().getManCastIndex(word);
							_ = 0;
							CCastCommandTransit.getInstance().cast_FieldCamera().Mode_set(cmr.CWorldCamera.MODE.MODE_AUTOFOLLOW);
							if (word == 0)
							{
								byte b = 0;
								while (b < 4 && (pl.PLAYER_VEHICLE_TYPE.PLAYER_VEHICLE_TYPE_ERR == CCastCommandTransit.getInstance().cast_PlayerMng().PlayerVehicle(b)
									.getVehicleType() || CCastCommandTransit.getInstance().cast_PlayerMng().PlayerVehicle(b)
									.getBoardPlayer() == null))
								{
									b++;
								}
								num = ((b < 4) ? (b + 24) : 0);
							}
							else
							{
								num = evt.CHichParameterManager.getInstance().CharaIndex(manCastIndex);
							}
							chr.CBaseCharacter.setLookIndex(num);
							if (word2 == 0)
							{
								CCastCommandTransit.getInstance().cast_FieldCamera().setPos(CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.getPosition());
								CCastCommandTransit.getInstance().cast_FieldCamera().setTrg(CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.getPosition());
							}
							CCastCommandTransit.getInstance().cast_FieldCamera().setSucTrg(CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
								.getPosition());
						}

						internal static void ff3Command_SetCamera_PositionOffset(ScriptEngine engine)
						{
							VecFx32 vecFx = ff3Command_reuse_v0;
							vecFx.set((int)engine.getDword(), (int)engine.getDword(), (int)engine.getDword());
							bool flag = ((engine.getDword() != 0) ? true : false);
							bool flag2 = ((engine.getDword() != 0) ? true : false);
							bool flag3 = ((engine.getDword() != 0) ? true : false);
							VecFx32 vecFx2 = ff3Command_reuse_v1;
							vecFx2.set(0, 0, 0);
							if (flag)
							{
								vecFx2.x = vecFx.x;
							}
							if (flag2)
							{
								vecFx2.y = vecFx.y;
							}
							if (flag3)
							{
								vecFx2.z = vecFx.z;
							}
							CCastCommandTransit.getInstance().cast_FieldCamera().setPosOffset(vecFx2);
							CCastCommandTransit.getInstance().cast_FieldCamera().setSucPosOffset(vecFx2);
							CCastCommandTransit.getInstance().cast_FieldCamera().setPosOffsetSpeed(0);
							CCastCommandTransit.getInstance().cast_FieldCamera().setPosOffsetMoveType(cmr.CWorldCamera.MOVE_TYPE.MOVE_TYPE_ERR);
						}

						internal static void ff3Command_SetCamera_TargetOffset(ScriptEngine engine)
						{
							VecFx32 vecFx = ff3Command_reuse_v0;
							vecFx.set((int)engine.getDword(), (int)engine.getDword(), (int)engine.getDword());
							bool flag = ((engine.getDword() != 0) ? true : false);
							bool flag2 = ((engine.getDword() != 0) ? true : false);
							bool flag3 = ((engine.getDword() != 0) ? true : false);
							VecFx32 vecFx2 = ff3Command_reuse_v1;
							vecFx2.set(0, 0, 0);
							if (flag)
							{
								vecFx2.x = vecFx.x;
							}
							if (flag2)
							{
								vecFx2.y = vecFx.y;
							}
							if (flag3)
							{
								vecFx2.z = vecFx.z;
							}
							CCastCommandTransit.getInstance().cast_FieldCamera().setTrgOffset(vecFx2);
							CCastCommandTransit.getInstance().cast_FieldCamera().setSucTrgOffset(vecFx2);
							CCastCommandTransit.getInstance().cast_FieldCamera().setTrgOffsetSpeed(0);
							CCastCommandTransit.getInstance().cast_FieldCamera().setTrgOffsetMoveType(cmr.CWorldCamera.MOVE_TYPE.MOVE_TYPE_ERR);
						}

						internal static void ff3Command_SetCamera_ZoomOnOff(ScriptEngine engine)
						{
							bool zoomEnable = ((engine.getDword() != 0) ? true : false);
							CCastCommandTransit.getInstance().cast_FieldCamera().composit.setZoomEnable(zoomEnable);
						}

						internal static void ff3Command_SetCamera_ZoomState(ScriptEngine engine)
						{
							uint dword = engine.getDword();
							CCastCommandTransit.getInstance().cast_FieldCamera().composit.setZoomState((cmr.CCameraZoom.ZOOM_STATE)dword);
						}

						internal static void ff3Command_SetCamera_ZoomDegree(ScriptEngine engine)
						{
							int num = engine.getWord();
							if (engine.getWord() != 0)
							{
								num *= -1;
							}
							CCastCommandTransit.getInstance().cast_FieldCamera().composit.setZoom(4096 * num);
						}

						internal static void ff3Command_SetCamera_ZoomMax(ScriptEngine engine)
						{
							int num = engine.getWord();
							if (engine.getWord() != 0)
							{
								num *= -1;
							}
							CCastCommandTransit.getInstance().cast_FieldCamera().composit.setZoomMax(4096 * num);
						}

						internal static void ff3Command_SetCamera_ZoomMin(ScriptEngine engine)
						{
							int num = engine.getWord();
							if (engine.getWord() != 0)
							{
								num *= -1;
							}
							CCastCommandTransit.getInstance().cast_FieldCamera().composit.setZoomMin(4096 * num);
						}

						internal static void ff3Command_SetCamera_ZoomSpeed(ScriptEngine engine)
						{
							int num = static_cast<int>(engine.getWord());
							if (engine.getWord() != 0)
							{
								num *= -1;
							}
							CCastCommandTransit.getInstance().cast_FieldCamera().composit.setZoomSpd(4096 * num);
						}

						internal static void ff3Command_SetCamera_BeforeEvent(ScriptEngine engine)
						{
							engine.getDword();
							engine.getDword();
							engine.getDword();
							CCastCommandTransit.getInstance().cast_BaseSystem().setupCamera();
						}

						internal static void ff3Command_SetCharacter_Collision(ScriptEngine engine)
						{
							uint word = engine.getWord();
							bool flag = ((engine.getDword() != 0) ? true : false);
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num != -1)
							{
								if (!flag)
								{
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.getColFlag_not_and(2);
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.getColFlag_not_and(4);
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.getColFlag_not_and(8);
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.getColFlag_not_and(16);
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.getColFlag_not_and(4096);
								}
								else
								{
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.getColFlag_or(2);
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.getColFlag_or(4);
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.getColFlag_or(8);
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.getColFlag_or(16);
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.getColFlag_or(4096);
								}
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setMCLCol(flag);
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setScriptWallCollision(flag);
							}
						}

						internal static void ff3Command_SetCharacter_GroundCollision(ScriptEngine engine)
						{
							uint word = engine.getWord();
							bool flag = ((engine.getDword() != 0) ? true : false);
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num != -1)
							{
								if (!flag)
								{
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.getColFlag_not_and(8);
								}
								else
								{
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.getColFlag_or(8);
								}
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setMCLCol(flag);
							}
						}

						internal static void ff3Command_SetCharacter_WallCollision(ScriptEngine engine)
						{
							uint word = engine.getWord();
							bool flag = ((engine.getDword() != 0) ? true : false);
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num != -1)
							{
								if (!flag)
								{
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.getColFlag_not_and(16);
								}
								else
								{
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.getColFlag_or(16);
								}
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setMCLCol(flag);
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setScriptWallCollision(flag);
							}
						}

						internal static void ff3Command_SetCharacter_CharaCollision(ScriptEngine engine)
						{
							uint word = engine.getWord();
							bool flag = ((engine.getDword() != 0) ? true : false);
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num != -1)
							{
								if (!flag)
								{
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.getColFlag_not_and(4);
								}
								else
								{
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.getColFlag_or(4);
								}
							}
						}

						internal static void ff3Command_SetCharacter_MapJump(ScriptEngine engine)
						{
							uint word = engine.getWord();
							bool flag = ((engine.getDword() != 0) ? true : false);
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num != -1)
							{
								if (!flag)
								{
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.getColFlag_not_and(16);
								}
								else
								{
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.getColFlag_or(16);
								}
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setMCLCol(flag);
							}
						}

						internal static void ff3Command_SetCharacter_FixedTurn(ScriptEngine engine)
						{
							uint word = engine.getWord();
							bool flag = ((engine.getDword() != 0) ? true : false);
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num != -1)
							{
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.TurnSys()
									.setFixed(flag);
							}
						}

						internal static void ff3Command_StartBalloon(ScriptEngine engine)
						{
							engine.getWord();
							engine.getDword();
							CCastCommandTransit.getInstance().cast_PlayerMng().Player(0)
								.setBalloon(_Balloon: true);
						}

						internal static void ff3Command_EndBalloon(ScriptEngine engine)
						{
							uint word = engine.getWord();
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num != -1)
							{
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setBalloon(_Balloon: false);
							}
						}

						internal static void ff3Command_PartyTalkEventOnJump(ScriptEngine engine)
						{
							uint dword = engine.getDword();
							if (evt.CEventManager.getInstance().isPartyTalkEvent())
							{
								engine.jump(dword);
							}
						}

						internal static void ff3Command_PartyTalkEventOffJump(ScriptEngine engine)
						{
							uint dword = engine.getDword();
							if (!evt.CEventManager.getInstance().isPartyTalkEvent())
							{
								engine.jump(dword);
							}
						}

						internal static void ff3Command_CheckCharacterStatusJump(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint dword = engine.getDword();
							uint dword2 = engine.getDword();
							if (dword == 0)
							{
								return;
							}
							uint num = (uint)(1 << (int)(dword - 1));
							switch (word)
							{
							case 0u:
							case 1u:
							case 2u:
							case 3u:
							{
								for (byte b = 0; b < 4; b++)
								{
									if (pl.PlayerParty.instance().player(b).isEnable() && !pl.PlayerParty.instance().player(b).condition()
										.isNotBattleCondition() && (pl.PlayerParty.instance().player(b).condition()
										.normalCondition() & num) != 0)
									{
										engine.jump(dword2);
									}
								}
								break;
							}
							default:
								word -= 5;
								if ((pl.PlayerParty.instance().playerForId((byte)word).condition()
									.normalCondition() & num) != 0)
								{
									engine.jump(dword2);
								}
								break;
							case 4u:
								break;
							}
						}

						internal static void ff3Command_CheckPartyPCMemberEnaleOnJump(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint dword = engine.getDword();
							byte b = static_cast<byte>(word);
							b -= 5;
							if (pl.PlayerParty.instance().playerForId(b).isEnable())
							{
								engine.jump(dword);
							}
						}

						internal static void ff3Command_CheckPartyPCMemberEnaleOffJump(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint dword = engine.getDword();
							byte b = static_cast<byte>(word);
							b -= 5;
							if (!pl.PlayerParty.instance().playerForId(b).isEnable())
							{
								engine.jump(dword);
							}
						}

						internal static void ff3Command_CheckParty_NPCMemberEnaleOnJump(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint dword = engine.getDword();
							byte b = static_cast<byte>(word);
							b -= 9;
							if (pl.PlayerParty.instance().npc().isEnable())
							{
								engine.jump(dword);
							}
						}

						internal static void ff3Command_CheckParty_NPCMemberEnaleOffJump(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint dword = engine.getDword();
							byte b = static_cast<byte>(word);
							b -= 9;
							if (!pl.PlayerParty.instance().npc().isEnable())
							{
								engine.jump(dword);
							}
						}

						internal static void ff3Command_AddJob(ScriptEngine engine)
						{
							engine.getDword();
						}

						internal static void ff3Command_SubJob(ScriptEngine engine)
						{
							engine.getDword();
						}

						internal static void ff3Command_SetCharacter_MotionSpeed(ScriptEngine engine)
						{
							uint word = engine.getWord();
							float num = engine.getDword();
							int num2 = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num2 != -1)
							{
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num2)
									.setMotionSpeed((int)num);
							}
						}

						internal static void ff3Command_SetCharacter_Alpha(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint word2 = engine.getWord();
							uint word3 = engine.getWord();
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num != -1)
							{
								word2 = word2 * 100 / 32;
								if (word3 == 0)
								{
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.setTransparencyRate((int)word2);
									return;
								}
								int transparencyRate = CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.getTransparencyRate();
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setSucAlpha((int)word2);
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setAutoAlphaFrame((int)word3);
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setWorkAutoAlphaFrame((int)((transparencyRate >= word2) ? word3 : 0));
							}
						}

						internal static void ff3Command_BootNameEntry(ScriptEngine engine)
						{
							uint word = engine.getWord();
							NameEntry.getSingleton().setTargetPlayer((pl.PLAYER_ID)word);
							wld.WorldPart.getInstance().getWorldSystem().appendPWS(NameEntry.getSingleton());
						}

						internal static void ff3Command_CheckCharacter_StatusJump(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint dword = engine.getDword();
							uint dword2 = engine.getDword();
							uint dword3 = engine.getDword();
							uint dword4 = engine.getDword();
							uint dword5 = engine.getDword();
							int[] array = new int[4]
							{
								(int)dword,
								(int)dword2,
								(int)dword3,
								(int)dword4
							};
							uint num = 0u;
							for (int i = 0; i < 4; i++)
							{
								if (array[i] != 0)
								{
									num |= (uint)(1 << array[i] - 1);
								}
							}
							switch (word)
							{
							case 0u:
							case 1u:
							case 2u:
							case 3u:
								if ((pl.PlayerParty.instance().player((byte)word).condition()
									.normalCondition() & num) == num)
								{
									engine.jump(dword5);
								}
								return;
							case 4u:
								return;
							}
							word -= 5;
							if ((pl.PlayerParty.instance().playerForId((byte)word).condition()
								.normalCondition() & num) == num)
							{
								engine.jump(dword5);
							}
						}

						internal static void ff3Command_StartPartyTalk(ScriptEngine engine)
						{
							engine.getByte();
							evt.CEventManager.getInstance().setPartyTalkEvent(_PartyTalkEvent: true);
						}

						internal static void ff3Command_EndPartyTalk(ScriptEngine engine)
						{
							engine.getByte();
							evt.CEventManager.getInstance().setPartyTalkEvent(_PartyTalkEvent: false);
						}

						internal static void ff3Command_TurnCharacter_AbsoluteAngle2(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint dword = engine.getDword();
							uint word2 = engine.getWord();
							uint dword2 = engine.getDword();
							byte b = engine.getByte();
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num == -1)
							{
								return;
							}
							CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
								.setAutoPilot(_AutoPilot: true);
							dword /= 4096;
							VecFx32 vecFx = ff3Command_reuse_v0;
							VecFx32 rotation = CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
								.getRotation();
							int i = FX_DEG_TO_IDX((int)dword) * 4096;
							int num2 = 0;
							switch (dword2)
							{
							case 0u:
								rotation.y = FX_DEG_TO_IDX((int)((360 - dword) * 4096));
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setRotation(rotation);
								break;
							case 1u:
							case 2u:
							case 3u:
							{
								short x = (short)(FX_SinIdx((ushort)i) * -1);
								short z = FX_CosIdx((ushort)i);
								VEC_Set(vecFx, x, 0, z);
								vecFx.x /= 682;
								vecFx.y /= 682;
								vecFx.z /= 682;
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setTargetDirection(vecFx);
								for (; i < -chr.PI; i += chr.PI * 2)
								{
								}
								while (chr.PI < i)
								{
									i -= chr.PI * 2;
								}
								num2 = ((rotation.y > i) ? (rotation.y - i) : (i - rotation.y));
								if (word2 != 0)
								{
									num2 = FX_Div(num2, (int)(word2 << 12));
								}
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setTurnAcc(num2);
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setTurnDec(0);
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setTurnMax(num2);
								break;
							}
							}
							if (b == 0)
							{
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.startMotion(1011, _Loop: true, 5u);
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.TurnSys()
									.setEndWait(_EndWait: true);
							}
						}

						internal static void ff3Command_TurnCharacter_RelativeAngle2(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint dword = engine.getDword();
							uint word2 = engine.getWord();
							uint dword2 = engine.getDword();
							byte b = engine.getByte();
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num == -1)
							{
								return;
							}
							CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
								.setAutoPilot(_AutoPilot: true);
							dword /= 4096;
							VecFx32 vecFx = ff3Command_reuse_v0;
							VecFx32 rotation = CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
								.getRotation();
							int i = rotation.y + FX_DEG_TO_IDX((int)dword) * 4096;
							int num2 = 0;
							switch (dword2)
							{
							case 0u:
								rotation.y = FX_DEG_TO_IDX((int)((360 - dword) * 4096));
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setRotation(rotation);
								break;
							case 1u:
							case 2u:
							case 3u:
							{
								short x = (short)(FX_SinIdx((ushort)i) * -1);
								short z = FX_CosIdx((ushort)i);
								VEC_Set(vecFx, x, 0, z);
								vecFx.x /= 682;
								vecFx.y /= 682;
								vecFx.z /= 682;
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setTargetDirection(vecFx);
								for (; i < -chr.PI; i += chr.PI * 2)
								{
								}
								while (chr.PI < i)
								{
									i -= chr.PI * 2;
								}
								num2 = ((rotation.y > i) ? (rotation.y - i) : (i - rotation.y));
								if (word2 != 0)
								{
									num2 = FX_Div(num2, (int)(word2 << 12));
								}
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setTurnAcc(num2);
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setTurnDec(0);
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setTurnMax(num2);
								break;
							}
							}
							if (b == 0)
							{
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.startMotion(1011, _Loop: true, 5u);
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.TurnSys()
									.setEndWait(_EndWait: true);
							}
						}

						internal static void ff3Command_TurnCharacter_AbsoluteCoordination2(ScriptEngine engine)
						{
							uint word = engine.getWord();
							VecFx32 vecFx = ff3Command_reuse_v0;
							vecFx.set((int)engine.getDword(), (int)engine.getDword(), (int)engine.getDword());
							uint word2 = engine.getWord();
							engine.getDword();
							byte b = engine.getByte();
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num != -1 && (long)num < 24L)
							{
								VecFx32 position = CCastCommandTransit.getInstance().cast_PlayerMng().PlayerHuman(num)
									.getPosition();
								VecFx32 rotation = CCastCommandTransit.getInstance().cast_PlayerMng().PlayerHuman(num)
									.getRotation();
								VecFx32 vecFx2 = vecFx;
								VEC_Subtract(vecFx2, position, vecFx2);
								VEC_Normalize(vecFx2, vecFx2);
								vecFx2.x /= 682;
								vecFx2.y /= 682;
								vecFx2.z /= 682;
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setTargetDirection(vecFx2);
								int i = FX_Atan2Idx(vecFx2.x, vecFx2.z);
								int num2 = 0;
								for (; i < -chr.PI; i += chr.PI * 2)
								{
								}
								while (chr.PI < i)
								{
									i -= chr.PI * 2;
								}
								num2 = ((rotation.y > i) ? (rotation.y - i) : (i - rotation.y));
								if (word2 != 0)
								{
									num2 = FX_Div(num2, (int)(word2 << 12));
								}
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setTurnAcc(num2);
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setTurnDec(0);
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setTurnMax(num2);
								if (b == 0)
								{
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.startMotion(1011, _Loop: true, 5u);
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.TurnSys()
										.setEndWait(_EndWait: true);
								}
							}
						}

						internal static void ff3Command_TurnCharacter_LookCharacter2(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint word2 = engine.getWord();
							uint word3 = engine.getWord();
							engine.getDword();
							byte b = engine.getByte();
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num == -1 || (long)num >= 24L)
							{
								return;
							}
							int num2 = CCastCommandTransit.getInstance().changeHichNumber(word2);
							if (num2 != -1)
							{
								VecFx32 position = CCastCommandTransit.getInstance().cast_PlayerMng().PlayerHuman(num)
									.getPosition();
								VecFx32 rotation = CCastCommandTransit.getInstance().cast_PlayerMng().PlayerHuman(num)
									.getRotation();
								VecFx32 vecFx = ff3Command_reuse_v2;
								vecFx.copy(CCastCommandTransit.getInstance().cast_PlayerMng().Player(num2)
									.getPosition());
								VEC_Subtract(vecFx, position, vecFx);
								VEC_Normalize(vecFx, vecFx);
								vecFx.x /= 682;
								vecFx.y /= 682;
								vecFx.z /= 682;
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setTargetDirection(vecFx);
								int i = FX_Atan2Idx(vecFx.x, vecFx.z);
								int num3 = 0;
								for (; i < -chr.PI; i += chr.PI * 2)
								{
								}
								while (chr.PI < i)
								{
									i -= chr.PI * 2;
								}
								num3 = ((rotation.y > i) ? (rotation.y - i) : (i - rotation.y));
								if (word3 != 0)
								{
									num3 = FX_Div(num3, (int)(word3 << 12));
								}
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setTurnAcc(num3);
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setTurnDec(0);
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setTurnMax(num3);
								if (b == 0)
								{
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.startMotion(1011, _Loop: true, 5u);
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.TurnSys()
										.setEndWait(_EndWait: true);
								}
							}
						}

						internal static void ff3Command_TurnCharacter_StartLoop2(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint word2 = engine.getWord();
							uint dword = engine.getDword();
							byte b = engine.getByte();
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num != -1)
							{
								StartLoopImp(num, word2, (chr.CCharacterTurnSys.TURN_TYPE)dword);
								if (b == 0)
								{
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.startMotion(1011, _Loop: true, 5u);
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.TurnSys()
										.setEndWait(_EndWait: true);
								}
							}
						}

						internal static void ff3Command_SetCharacter_ShadowAlpha(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint word2 = engine.getWord();
							uint word3 = engine.getWord();
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num != -1)
							{
								if (word3 == 0)
								{
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.setShadowAlpha((int)word2);
									return;
								}
								int shadowAlpha = CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.getShadowAlpha();
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setSucShadowAlpha((int)word2);
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setAutoShadowAlphaFrame((int)word3);
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setWorkAutoShadowAlphaFrame((int)((shadowAlpha >= word2) ? word3 : 0));
							}
						}

						internal static void ff3Command_SetCharacter_TurnTargetCharacter(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint word2 = engine.getWord();
							uint word3 = engine.getWord();
							bool targetLookFlag = ((engine.getDword() != 0) ? true : false);
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num != -1)
							{
								int num2 = CCastCommandTransit.getInstance().changeHichNumber(word2);
								if (num2 != -1)
								{
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.setTarget(static_cast<chr.CCharacterEureka>(CCastCommandTransit.getInstance().cast_PlayerMng().Player(num2)));
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.setTargetLookFlag(targetLookFlag);
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.setTargetLookFrame((int)word3);
								}
							}
						}

						internal static void ff3Command_StartMotionCharacterDX(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint dword = engine.getDword();
							bool loop = ((engine.getDword() != 0) ? true : false);
							uint word2 = engine.getWord();
							uint word3 = engine.getWord();
							uint word4 = engine.getWord();
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num == -1)
							{
								return;
							}
							if (word4 == 0)
							{
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setStockMotionIndex(0);
								for (int i = 0; i < 8; i++)
								{
									CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
										.resetStockMotionParameter(i);
								}
							}
							else
							{
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.resetStockMotionParameter((int)word4);
							}
							CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
								.setStockMotionParameter((int)word4, (int)dword, (int)word3, loop, (int)word2);
						}

						internal static void ff3Command_WorldMapWarp(ScriptEngine engine)
						{
							string arg = engine.getString();
							VecFx32 vecFx = ff3Command_reuse_v0;
							vecFx.set((int)engine.getDword(), (int)engine.getDword(), (int)engine.getDword());
							uint dword = engine.getDword();
							uint dword2 = engine.getDword();
							engine.getWord();
							VecFx32 pos = vecFx;
							VecFx32 vecFx2 = ff3Command_reuse_v1;
							vecFx2.set(0, 0, 0);
							switch (dword)
							{
							case 0u:
								vecFx2.y = 0;
								break;
							case 1u:
								vecFx2.y = 8192;
								break;
							case 2u:
								vecFx2.y = 16384;
								break;
							case 3u:
								vecFx2.y = 24576;
								break;
							case 4u:
								vecFx2.y = 32768;
								break;
							case 5u:
								vecFx2.y = 40960;
								break;
							case 6u:
								vecFx2.y = 49152;
								break;
							case 7u:
								vecFx2.y = 57344;
								break;
							}
							CCastCommandTransit.getInstance().castParam_MapJump().initialize();
							CCastCommandTransit.getInstance().castParam_MapJump().setUp(const_cast<string>(arg), (sbyte)dword2, pos, vecFx2, _Flag: true);
							CCastCommandTransit.getInstance().cast_BaseSystem().setMapJump(b: true);
						}

						internal static void ff3Command_SetMapJumpFlag(ScriptEngine engine)
						{
							byte b = engine.getByte();
							bool flag = engine.getDword() != 0;
							int[] array = new int[12]
							{
								4096, 8192, 16384, 32768, 65536, 131072, 262144, 524288, 1048576, 2097152,
								4194304, 8388608
							};
							if (flag)
							{
								wld.CWorldOutSideData.getInstance().MapData().isColFlag_or(array[b]);
							}
							else
							{
								wld.CWorldOutSideData.getInstance().MapData().isColFlag_not_and(array[b]);
							}
						}

						internal static void ff3Command_SetMapJumpFlagJump(ScriptEngine engine)
						{
							byte b = engine.getByte();
							uint dword = engine.getDword();
							int[] array = new int[12]
							{
								4096, 8192, 16384, 32768, 65536, 131072, 262144, 524288, 1048576, 2097152,
								4194304, 8388608
							};
							if ((wld.CWorldOutSideData.getInstance().MapData().isColFlag() & array[b]) != 0)
							{
								engine.jump(dword);
							}
						}

						internal static void ff3Command_StartWaorldPartyTalk(ScriptEngine engine)
						{
							string arg = engine.getString();
							byte b = engine.getByte();
							engine.getByte();
							engine.getByte();
							engine.getByte();
							engine.getByte();
							wld.CWorldOutSideData.getInstance().MapData().setNowMapName(const_cast<string>(arg));
							if (b > 0)
							{
								wld.CBaseSystem.CONTENT c = new wld.CBaseSystem.CONTENT(1414551379u, b);
								CCastCommandTransit.getInstance().cast_BaseSystem().putContent(c);
							}
							CCastCommandTransit.getInstance().cast_BaseSystem().setTalk(b: true);
						}

						internal static void ff3Command_EndWaorldPartyTalk(ScriptEngine engine)
						{
							CCastCommandTransit.getInstance().cast_BaseSystem().setTalk(b: true);
						}

						internal static void ff3Command_SetPartyPCEquipItem(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint word2 = engine.getWord();
							uint word3 = engine.getWord();
							uint word4 = engine.getWord();
							uint word5 = engine.getWord();
							uint word6 = engine.getWord();
							byte b = (byte)word;
							b -= 5;
							if (pl.PlayerParty.instance().playerForId(b).isEnable())
							{
								pl.EquipItemInfo equipItemInfo = new pl.EquipItemInfo();
								if (word2 != 0)
								{
									equipItemInfo.itemId_ = (short)word2;
									equipItemInfo.itemNumber_ = 1;
									pl.PlayerParty.instance().playerForId(b).equipParameter()
										.equipHand(pl.HAND_TYPE.LEFT_HAND)
										.equip(equipItemInfo);
								}
								if (word3 != 0)
								{
									equipItemInfo.itemId_ = (short)word3;
									equipItemInfo.itemNumber_ = 1;
									pl.PlayerParty.instance().playerForId(b).equipParameter()
										.equipHand(pl.HAND_TYPE.RIGHT_HAND)
										.equip(equipItemInfo);
								}
								if (word4 != 0)
								{
									equipItemInfo.itemId_ = (short)word4;
									equipItemInfo.itemNumber_ = 1;
									pl.PlayerParty.instance().playerForId(b).equipParameter()
										.equipHead()
										.equip(equipItemInfo);
								}
								if (word5 != 0)
								{
									equipItemInfo.itemId_ = (short)word5;
									equipItemInfo.itemNumber_ = 1;
									pl.PlayerParty.instance().playerForId(b).equipParameter()
										.equipBody()
										.equip(equipItemInfo);
								}
								if (word6 != 0)
								{
									equipItemInfo.itemId_ = (short)word6;
									equipItemInfo.itemNumber_ = 1;
									pl.PlayerParty.instance().playerForId(b).equipParameter()
										.equipArm()
										.equip(equipItemInfo);
								}
							}
						}

						internal static void ff3Command_SetMapRotation(ScriptEngine engine)
						{
							uint dword = engine.getDword();
							uint dword2 = engine.getDword();
							uint dword3 = engine.getDword();
							VecFx32 vecFx = ff3Command_reuse_v0;
							vecFx.set((int)(4096 * dword), (int)(4096 * dword2), (int)(4096 * dword3));
							FX_DEG_TO_IDX(vecFx.x);
							ushort rotationY = (ushort)FX_DEG_TO_IDX(vecFx.y);
							FX_DEG_TO_IDX(vecFx.z);
							stageMng.setRotationY(rotationY);
						}

						internal static void ff3Command_ChangeFaceEye(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint num = engine.getByte();
							int num2 = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num2 == -1)
							{
								return;
							}
							int characterId = CCastCommandTransit.getInstance().cast_PlayerMng().Player(num2)
								.getCharacterId();
							if (characterId != -1)
							{
								if (!TexDivideLoader.getSingleton().tdlIsEmpty())
								{
									engine.suspendRedo();
									return;
								}
								characterMng.bindChainTexel(characterId, static_cast<uint>(num), "eye");
								characterMng.bindChainPltt(characterId, static_cast<uint>(num), "eye_pl");
							}
						}

						internal static void ff3Command_ChangeFaceMouth(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint num = engine.getByte();
							int num2 = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num2 == -1)
							{
								return;
							}
							int characterId = CCastCommandTransit.getInstance().cast_PlayerMng().Player(num2)
								.getCharacterId();
							if (characterId != -1)
							{
								if (!TexDivideLoader.getSingleton().tdlIsEmpty())
								{
									engine.suspendRedo();
									return;
								}
								characterMng.bindChainTexel(characterId, static_cast<uint>(num), "mouth");
								characterMng.bindChainPltt(characterId, static_cast<uint>(num), "mouth_pl");
							}
						}

						internal static void ff3Command_MessagePermission(ScriptEngine engine)
						{
							engine.getWord();
							CCastCommandTransit.getInstance().cast_Field2D().MessageWindow()
								.setSendMessage(_SendMessage: true);
						}

						internal static void ff3Command_MessageWait(ScriptEngine engine)
						{
							if (CCastCommandTransit.getInstance().cast_Field2D().MessageWindow()
								.isMadeMessage())
							{
								engine.suspendRedo();
							}
						}

						internal static void ff3Command_EndMotionCharacterDX(ScriptEngine engine)
						{
							uint word = engine.getWord();
							byte b = engine.getByte();
							uint dword = engine.getDword();
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num == -1)
							{
								return;
							}
							int stockMotionIndex = CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
								.getStockMotionIndex();
							switch (dword)
							{
							case 0u:
								if (stockMotionIndex > b)
								{
									engine.suspendRedo();
								}
								break;
							case 1u:
								if (stockMotionIndex >= b)
								{
									engine.suspendRedo();
								}
								break;
							case 2u:
								if (stockMotionIndex < b)
								{
									engine.suspendRedo();
								}
								break;
							case 3u:
								if (stockMotionIndex <= b)
								{
									engine.suspendRedo();
								}
								break;
							case 4u:
								if (stockMotionIndex == b)
								{
									engine.suspendRedo();
								}
								break;
							}
						}

						internal static void ff3Command_SetVehiclePosition(ScriptEngine engine)
						{
							uint dword = engine.getDword();
							VecFx32 vecFx = ff3Command_reuse_v0;
							VecFx32 vecFx2 = ff3Command_reuse_v1;
							vecFx.set((int)engine.getDword(), (int)engine.getDword(), (int)engine.getDword());
							vecFx2.set((int)engine.getDword(), (int)engine.getDword(), (int)(engine.getDword() * -1));
							dword -= 3;
							int fieldNo = wld.CWorldOutSideData.getInstance().VehicleData().getHoldData((int)dword)
								.m_FieldNo;
							bool canBoard = wld.CWorldOutSideData.getInstance().VehicleData().getHoldData((int)dword)
								.m_CanBoard;
							wld.CWorldOutSideData.getInstance().VehicleData().setHoldData((int)dword, (sbyte)fieldNo, vecFx, vecFx2, canBoard);
						}

						internal static void ff3Command_SetCharacter_ItemEvent(ScriptEngine engine)
						{
							uint word = engine.getWord();
							bool itemEvent = ((engine.getDword() != 0) ? true : false);
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num != -1)
							{
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setItemEvent(itemEvent);
							}
						}

						internal static void ff3Command_SetBattleEscape(ScriptEngine engine)
						{
							if ((engine.getDword() != 0) ? true : false)
							{
								btl.OutsideToBattle.getInstance().onEscape();
							}
							else
							{
								btl.OutsideToBattle.getInstance().offEscape();
							}
						}

						internal static void ff3Command_SetUpCharacter_FaceData(ScriptEngine engine)
						{
							uint word = engine.getWord();
							string text = engine.getString();
							changeGlobalDirectory();
							evt.CHichParameterManager.getInstance().getManCastIndex(word);
							_ = 0;
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num == -1)
							{
								return;
							}
							int num2 = -1;
							sprintf(out var arg, "%s.face", text);
							if (arg[0] >= 'A' && arg[0] <= 'Z')
							{
								char[] array = arg.ToCharArray();
								array[0] += ' ';
								arg = new string(array);
							}
							if (ds.g_File.getSize(arg) != 0)
							{
								num2 = CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.getCharacterId();
								if (num2 != -1)
								{
									TexDivideLoader.getSingleton().tdlForceLoad();
									characterMng.setChainTexture(num2, arg);
									TexDivideLoader.getSingleton().tdlForceLoad();
								}
							}
						}

						internal static void ff3Command_CleanUpCharacter_FaceData(ScriptEngine engine)
						{
							uint word = engine.getWord();
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num != -1)
							{
								int num2 = -1;
								num2 = CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.getCharacterId();
								characterMng.delChainTexture(num2);
							}
						}

						internal static void ff3Command_SetCameraMargin(ScriptEngine engine)
						{
							if (engine.getDword() != 0)
							{
								CCastCommandTransit.getInstance().cast_FieldCamera().applyMargin(f: true);
							}
							else
							{
								CCastCommandTransit.getInstance().cast_FieldCamera().applyMargin(f: false);
							}
						}

						internal static void ff3Command_SetCharacterDetectionRadius(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint word2 = engine.getWord();
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num != -1)
							{
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.getParamCol()
									.m_CckRadius = (int)(word2 << 12);
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.getParamCol()
									.m_TchRadius = (int)(word2 << 12);
							}
						}

						internal static void ff3Command_SetPartyMember_AllParameter(ScriptEngine engine)
						{
							uint word = engine.getWord();
							byte b = (byte)word;
							b -= 5;
							bool flag = pl.PlayerParty.instance().player(b).isEnable();
							pl.PlayerParty.instance().player(b).initialize(b);
							if (flag)
							{
								pl.PlayerParty.instance().player(b).onIsEnable();
							}
							else
							{
								pl.PlayerParty.instance().player(b).offIsEnable();
							}
						}

						internal static void ff3Command_SetPartyMember_DefaultParameter(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint dword = engine.getDword();
							uint dword2 = engine.getDword();
							uint word2 = engine.getWord();
							byte b = static_cast<byte>(word);
							int num = 0;
							b -= 5;
							switch (dword2)
							{
							case 0u:
								num = (int)word2;
								break;
							case 1u:
								num = 0;
								break;
							case 2u:
								num = 9999999;
								break;
							}
							switch (dword)
							{
							case 0u:
								pl.PlayerParty.instance().playerForId(b).level()
									.set((byte)num);
								break;
							case 1u:
								pl.PlayerParty.instance().playerForId(b).exp()
									.set(num);
								break;
							case 2u:
								pl.PlayerParty.instance().playerForId(b).capacity()
									.set((byte)num);
								break;
							case 3u:
								pl.PlayerParty.instance().playerForId(b).hp()
									.setNow(num);
								break;
							case 4u:
							{
								for (int i = 0; i < 8; i++)
								{
									pl.PlayerParty.instance().playerForId(b).mp(i)
										.setNow(num);
									pl.PlayerParty.instance().playerForId(b).setJobChangeMp(i, (byte)pl.PlayerParty.instance().playerForId(b).mp(i)
										.getNow());
								}
								break;
							}
							case 5u:
								pl.PlayerParty.instance().playerForId(b).body()
									.strength()
									.set(num);
								break;
							case 6u:
								pl.PlayerParty.instance().playerForId(b).body()
									.vitality()
									.set(num);
								break;
							case 7u:
								pl.PlayerParty.instance().playerForId(b).body()
									.dexterity()
									.set(num);
								break;
							case 8u:
								pl.PlayerParty.instance().playerForId(b).body()
									.intelligence()
									.set(num);
								break;
							case 9u:
								pl.PlayerParty.instance().playerForId(b).body()
									.mind()
									.set(num);
								break;
							case 10u:
								pl.PlayerParty.instance().playerForId(b).formationType_set((byte)num);
								break;
							}
						}

						internal static void ff3Command_SetPartyMember_PhysicsAttackParameter(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint dword = engine.getDword();
							uint dword2 = engine.getDword();
							uint word2 = engine.getWord();
							byte b = static_cast<byte>(word);
							int num = 0;
							b -= 5;
							switch (dword2)
							{
							case 0u:
								num = (int)word2;
								break;
							case 1u:
								num = 0;
								break;
							case 2u:
								num = 9999999;
								break;
							}
							for (int i = 0; i < 2; i++)
							{
								switch (dword)
								{
								case 0u:
									pl.PlayerParty.instance().playerForId(b).handAttack((pl.HAND_TYPE)i)
										.aggressivity()
										.set(num);
									break;
								case 1u:
									pl.PlayerParty.instance().playerForId(b).handAttack((pl.HAND_TYPE)i)
										.hitProbability_set((byte)num);
									break;
								case 2u:
									pl.PlayerParty.instance().playerForId(b).handAttack((pl.HAND_TYPE)i)
										.optionProbability_set((byte)num);
									break;
								case 3u:
									pl.PlayerParty.instance().playerForId(b).handAttack((pl.HAND_TYPE)i)
										.optionMagicId_set((short)num);
									break;
								case 4u:
									pl.PlayerParty.instance().playerForId(b).handAttack((pl.HAND_TYPE)i)
										.armsAttribute_set((short)num);
									break;
								case 5u:
									pl.PlayerParty.instance().playerForId(b).handAttack((pl.HAND_TYPE)i)
										.attackType_set((short)num);
									break;
								case 6u:
									pl.PlayerParty.instance().playerForId(b).handAttack((pl.HAND_TYPE)i)
										.attackOption_set((short)num);
									break;
								case 7u:
									pl.PlayerParty.instance().playerForId(b).handAttack((pl.HAND_TYPE)i)
										.equipOption_set((short)num);
									break;
								}
							}
						}

						internal static void ff3Command_SetPartyMember_PhysicsDefenseParameter(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint dword = engine.getDword();
							uint dword2 = engine.getDword();
							uint word2 = engine.getWord();
							byte b = static_cast<byte>(word);
							int num = 0;
							b -= 5;
							switch (dword2)
							{
							case 0u:
								num = (int)word2;
								break;
							case 1u:
								num = 0;
								break;
							case 2u:
								num = 9999999;
								break;
							}
							switch (dword)
							{
							case 0u:
								pl.PlayerParty.instance().playerForId(b).physicsDefense()
									.phylacticPower()
									.set(num);
								break;
							case 1u:
								pl.PlayerParty.instance().playerForId(b).physicsDefense()
									.avoidanceNumber_set(num);
								break;
							case 2u:
								pl.PlayerParty.instance().playerForId(b).physicsDefense()
									.armsAttribute_set((short)num);
								break;
							case 3u:
								pl.PlayerParty.instance().playerForId(b).physicsDefense()
									.antiType_set((short)num);
								break;
							case 4u:
								pl.PlayerParty.instance().playerForId(b).physicsDefense()
									.antiOption_set((short)num);
								break;
							case 5u:
								pl.PlayerParty.instance().playerForId(b).physicsDefense()
									.equipOption_set((short)num);
								break;
							}
						}

						internal static void ff3Command_SetPartyMember_MagicDefenseParameter(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint dword = engine.getDword();
							uint dword2 = engine.getDword();
							uint word2 = engine.getWord();
							byte b = static_cast<byte>(word);
							int num = 0;
							b -= 5;
							switch (dword2)
							{
							case 0u:
								num = (int)word2;
								break;
							case 1u:
								num = 0;
								break;
							case 2u:
								num = 9999999;
								break;
							}
							switch (dword)
							{
							case 0u:
								pl.PlayerParty.instance().playerForId(b).magicDefense()
									.weakType_set((short)num);
								break;
							case 1u:
								pl.PlayerParty.instance().playerForId(b).magicDefense()
									.magicPhylacticPower_set((short)num);
								break;
							}
						}

						internal static void ff3Command_SetPartyMember_SkillParameter(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint dword = engine.getDword();
							uint dword2 = engine.getDword();
							uint word2 = engine.getWord();
							byte b = static_cast<byte>(word);
							int num = 0;
							b -= 5;
							switch (dword2)
							{
							case 0u:
								num = (int)word2;
								break;
							case 1u:
								num = 0;
								break;
							case 2u:
								num = 9999999;
								break;
							}
							switch (dword)
							{
							case 0u:
								pl.PlayerParty.instance().playerForId(b).skillManager()
									.skill(pl.GET_SKILL_TYPE.GET_RIGHT_HAND)
									.skillLevel()
									.set((byte)num);
								break;
							case 1u:
								pl.PlayerParty.instance().playerForId(b).skillManager()
									.skill(pl.GET_SKILL_TYPE.GET_LEFT_HAND)
									.skillLevel()
									.set((byte)num);
								break;
							case 2u:
								pl.PlayerParty.instance().playerForId(b).skillManager()
									.skill(pl.GET_SKILL_TYPE.GET_MAGIC)
									.skillLevel()
									.set((byte)num);
								break;
							}
						}

						internal static void ff3Command_SetPartyMember_JobLevel(ScriptEngine engine)
						{
							uint word = engine.getWord();
							byte type = (byte)engine.getDword();
							byte value = engine.getByte();
							if (pl.PlayerParty.instance().playerForId((byte)word).isEnable())
							{
								pl.PlayerParty.instance().playerForId((byte)word).jobManager()
									.job((pl.JOB_TYPE)type)
									.skill()
									.skillLevel()
									.set(value);
							}
						}

						internal static void ff3Command_SetCharacterCollisionRadius_Sphere(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint word2 = engine.getWord();
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num != -1)
							{
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.getColRadius_set((int)(word2 << 12));
							}
						}

						internal static void ff3Command_SetCharacterCollisionRadius_Box(ScriptEngine engine)
						{
							uint word = engine.getWord();
							VecFx32 vecFx = ff3Command_reuse_v0;
							vecFx.set((int)engine.getDword(), (int)engine.getDword(), (int)engine.getDword() * -1);
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num != -1)
							{
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.getColAabbRadius_set(vecFx);
							}
						}

						internal static void ff3Command_SetCharacterTouchRadius_Sphere(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint word2 = engine.getWord();
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (num != -1)
							{
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.getTchRadius_set((int)(word2 << 12));
							}
						}

						internal static void ff3Command_SetCamera_Collision(ScriptEngine engine)
						{
							bool flag = ((engine.getDword() != 0) ? true : false);
							if (flag)
							{
								bool flag2 = map.CMapParameterManager.Instance().MapCameraParameter(0).Collision() != 0;
							}
							CCastCommandTransit.getInstance().cast_FieldCamera().setMCLCollision(flag);
						}

						internal static void ff3Command_SetUpBGM_Data(ScriptEngine engine)
						{
							uint word = engine.getWord();
							MatrixSound.MtxBGMNDS_Load((int)word);
						}

						internal static void ff3Command_CleanUpBGM_Data(ScriptEngine engine)
						{
							MatrixSound.MtxSoundBGM.getSingleton().stop(0, MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT0);
							MatrixSound.MtxBGMNDS_Unload();
							NNS_SndPlayerStopLastSeqByPlayerNo(0);
						}

						internal static void ff3Command_SetHalfWayBGM_Save(ScriptEngine engine)
						{
							MatrixSound.MtxSENDS_Unload();
							bgm = MatrixSound.MtxBGMNDS_GetPlayBGMNo(MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT1);
							MatrixSound.MtxSoundBGM.getSingleton().setVolume(0, 0, MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT1);
							MatrixSound.MtxSoundBGM.getSingleton().pause(Flag: true, MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT1);
							MatrixSound.MtxBGMNDS_Unload();
							MatrixSound.MtxSENDS_Load(1);
						}

						internal static void ff3Command_SetHalfWayBGM_Play(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint word2 = engine.getWord();
							MatrixSound.MtxSENDS_Unload();
							MatrixSound.MtxBGMNDS_LoadEx(bgm, 0);
							MatrixSound.MtxSoundBGM.getSingleton().pause(Flag: false, MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT1);
							MatrixSound.MtxSoundBGM.getSingleton().setVolume((int)word, (int)word2, MatrixSound.enMtxBGMSlot.enMTX_BGM_SLOT1);
							MatrixSound.MtxSENDS_Load(1);
						}

						internal static void ff3Command_SetUpSE_Data(ScriptEngine engine)
						{
							uint word = engine.getWord();
							MatrixSound.MtxSENDS_Load((int)word);
						}

						internal static void ff3Command_CleanUpSE_Data(ScriptEngine engine)
						{
							MatrixSound.MtxSENDS_Unload();
						}

						internal static void ff3Command_SetBGM_Volume(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint word2 = engine.getWord();
							MatrixSound.enMtxBGMSlot word3 = (MatrixSound.enMtxBGMSlot)engine.getWord();
							MatrixSound.MtxSoundBGM.getSingleton().setVolume((int)word, (int)word2, word3);
						}

						internal static void ff3Command_SetHalfWayBGM_Start(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint word2 = engine.getWord();
							uint word3 = engine.getWord();
							MatrixSound.enMtxBGMSlot word4 = (MatrixSound.enMtxBGMSlot)engine.getWord();
							MatrixSound.MtxSENDS_Unload();
							MatrixSound.MtxSoundBGM.getSingleton().stop(0, word4);
							MatrixSound.MtxBGMNDS_Unload();
							MatrixSound.MtxBGMNDS_Unload();
							MatrixSound.MtxBGMNDS_LoadEx((int)word, 0);
							MatrixSound.MtxBGMNDS_LoadEx((int)word, 0);
							MatrixSound.MtxSoundBGM.getSingleton().play((int)word, (int)word2, (int)word3, word4);
							wld.CWorldOutSideData.getInstance().MapData().PreBGMIndex_set((sbyte)map.CMapParameterManager.Instance().MapSoundParameter(0).BGMIndex());
							if (-1 != map.CMapParameterManager.Instance().MapSoundParameter(0).CheckFlag() && 1 == evt.CEventManager.getInstance().FlagMng().get(0u, (uint)map.CMapParameterManager.Instance().MapSoundParameter(0).CheckFlag()))
							{
								wld.CWorldOutSideData.getInstance().MapData().PreBGMIndex_set((sbyte)map.CMapParameterManager.Instance().MapSoundParameter(0).ChangeBGMIndex());
							}
							map.CMapParameterManager.Instance().MapSoundParameter(0).BGMIndex_set((short)word);
							MatrixSound.MtxSENDS_Load(1);
						}

						internal static void ff3Command_SetHalfWayBGM_Start2(ScriptEngine engine)
						{
							short bgmNo = (short)engine.getWord();
							short volume = (short)engine.getWord();
							short frame = (short)engine.getWord();
							wld.MapSound.playBGM(bgmNo, volume, frame);
						}

						internal static void ff3Command_SetUpEffectData(ScriptEngine engine)
						{
							string text = engine.getString();
							string arg2;
							if (strcmp(text, "now_map") == 0)
							{
								int num = 1;
								for (int i = 2; i < 8 && evt.CEventManager.getInstance().FlagMng().get(0u, (uint)evt.EVENT_CHAPTER_FLAG[i - 2]) != 0; i++)
								{
									num = i;
								}
								strncpy(out var arg, sceneMng.getStage(), 3);
								sprintf(out arg2, "/EFFECT/event%02d_%s.efp", num, arg);
								if (ds.g_File.getSize(arg2) == 0)
								{
									num = 1;
									sprintf(out arg2, "/EFFECT/event%02d_%s.efp", num, arg);
								}
								eff.CEffectMng.instance().loadEfp(arg2);
							}
							else
							{
								sprintf(out arg2, "/EFFECT/%s.efp", text);
								eff.CEffectMng.instance().loadEfp(const_cast<string>(arg2));
							}
						}

						internal static void ff3Command_CleanUpEffectData(ScriptEngine engine)
						{
							uint word = engine.getWord();
							engine.getWord();
							switch (word)
							{
							case 0u:
								eff.CEffectMng.instance().allUnLoadEfp();
								break;
							case 1u:
								if (eff.CEffectMng.instance().getLoadedEfpNum() > 1)
								{
									eff.CEffectMng.instance().unLoadEfp2();
								}
								break;
							}
						}

						internal static void ff3Command_LabelRandomJump(ScriptEngine engine)
						{
							uint dword = engine.getDword();
							uint dword2 = engine.getDword();
							uint dword3 = engine.getDword();
							uint dword4 = engine.getDword();
							uint dword5 = engine.getDword();
							uint dword6 = engine.getDword();
							uint[] array = new uint[6] { dword, dword2, dword3, dword4, dword5, dword6 };
							engine.jump(array[ds.RandomNumber.rand32(6u)]);
						}

						internal static void ff3Command_SetRecovery2(ScriptEngine engine)
						{
							uint dword = engine.getDword();
							uint dword2 = engine.getDword();
							uint dword3 = engine.getDword();
							uint word = engine.getWord();
							bool flag = false;
							bool flag2 = false;
							bool flag3 = false;
							int num = 0;
							int num2 = 0;
							if (dword == 0)
							{
								flag = true;
							}
							if (dword2 == 0)
							{
								num = 0;
								num2 = 4;
							}
							else
							{
								num = (int)(dword2 - 1);
								num2 = (int)dword2;
							}
							switch (dword3)
							{
							case 0u:
								flag2 = true;
								flag3 = true;
								break;
							case 1u:
								flag2 = true;
								break;
							case 2u:
								flag3 = true;
								break;
							}
							for (int i = num; i < num2; i++)
							{
								if (flag && pl.PlayerParty.instance().playerForId((byte)i).condition()
									.isDeath())
								{
									continue;
								}
								if (flag2)
								{
									if (9999 == word)
									{
										pl.PlayerParty.instance().playerForId((byte)i).hp()
											.maxNow();
									}
									else
									{
										pl.PlayerParty.instance().playerForId((byte)i).hp()
											.addNow((int)word);
									}
								}
								if (!flag3)
								{
									continue;
								}
								for (int j = 0; j < 8; j++)
								{
									if (9999 == word)
									{
										pl.PlayerParty.instance().playerForId((byte)i).mp(j)
											.maxNow();
										pl.PlayerParty.instance().playerForId((byte)i).setJobChangeMp(j, (byte)pl.PlayerParty.instance().playerForId((byte)i).mp(j)
											.getNow());
									}
									else
									{
										pl.PlayerParty.instance().playerForId((byte)i).mp(j)
											.addNow((int)word);
										pl.PlayerParty.instance().playerForId((byte)i).setJobChangeMp(j, (byte)pl.PlayerParty.instance().playerForId((byte)i).mp(j)
											.getNow());
									}
								}
							}
						}

						internal static void ff3Command_SetConditionRecovery(ScriptEngine engine)
						{
							uint dword = engine.getDword();
							uint dword2 = engine.getDword();
							uint dword3 = engine.getDword();
							uint dword4 = engine.getDword();
							uint dword5 = engine.getDword();
							uint dword6 = engine.getDword();
							int[] array = new int[5]
							{
								(int)dword2,
								(int)dword3,
								(int)dword4,
								(int)dword5,
								(int)dword6
							};
							int num = 0;
							for (int i = 0; i < 5; i++)
							{
								if (array[i] != 0)
								{
									num |= 1 << array[i] - 1;
								}
							}
							switch (dword)
							{
							case 0u:
							{
								for (int j = 0; j < 4; j++)
								{
									pl.PlayerParty.instance().player((byte)j).condition()
										.normalCondition_and((byte)num);
								}
								break;
							}
							case 1u:
							case 2u:
							case 3u:
							case 4u:
								pl.PlayerParty.instance().playerForId((byte)(dword - 1)).condition()
									.normalCondition_and((byte)num);
								break;
							}
							CCastCommandTransit.getInstance().cast_BaseSystem().changePlayerCharDisplay();
						}

						internal static void ff3Command_SetMessagePosition(ScriptEngine engine)
						{
							uint dword = engine.getDword();
							uint word = engine.getWord();
							uint dword2 = engine.getDword();
							uint word2 = engine.getWord();
							ds.Vector2<short> messagePosition = CCastCommandTransit.getInstance().cast_Field2D().MessageWindow()
								.getMessagePosition();
							if (dword != 0)
							{
								messagePosition.vx = (short)(word + 112);
							}
							if (dword2 != 0)
							{
								messagePosition.vy = (short)(word2 + 64);
							}
							CCastCommandTransit.getInstance().cast_Field2D().MessageWindow()
								.setMessagePosition(messagePosition);
						}

						internal static void ff3Command_SetMessageShadow(ScriptEngine engine)
						{
							bool messageShadow = ((engine.getDword() != 0) ? true : false);
							CCastCommandTransit.getInstance().cast_Field2D().MessageWindow()
								.setMessageShadow(messageShadow);
						}

						internal static void ff3Command_SetMessageStyle(ScriptEngine engine)
						{
							uint dword = engine.getDword();
							engine.getDword();
							CCastCommandTransit.getInstance().cast_Field2D().MessageWindow()
								.setMessageStyle((int)dword);
						}

						internal static void ff3Command_CheckMapName(ScriptEngine engine)
						{
							string arg = engine.getString();
							bool flag = ((engine.getDword() != 0) ? true : false);
							uint dword = engine.getDword();
							int arg2 = strlen(arg);
							if ((!flag && strncmp(arg, wld.CWorldOutSideData.getInstance().MapData().getNowMapName(), arg2) != 0) || (flag && strncmp(arg, wld.CWorldOutSideData.getInstance().MapData().getNowMapName(), arg2) == 0))
							{
								engine.jump(dword);
							}
						}

						internal static void ff3Command_SetMapJump_SE(ScriptEngine engine)
						{
							uint num = engine.getByte();
							bool flag = ((engine.getDword() != 0) ? true : false);
							OS_Printf("[CAST_COMMAND] (マップジャンプＳＥＯＮＯＦＦ設定) : ff3Command_SetMapJump_SE \n");
							OS_Printf("_Index1 : %d \n", num);
							OS_Printf("_Index2 : %s \n", flag ? "ON" : "OFF");
							int num2 = wld.CWorldOutSideData.getInstance().SoundData().getSoundFlag();
							switch (num)
							{
							case 0u:
								num2 = 0;
								if (flag)
								{
									num2 |= 1;
									num2 |= 2;
								}
								break;
							case 1u:
								num2 = ((!flag) ? (num2 & -2) : (num2 | 1));
								break;
							case 2u:
								num2 = ((!flag) ? (num2 & -3) : (num2 | 2));
								break;
							}
							wld.CWorldOutSideData.getInstance().SoundData().setSoundFlag(num2);
						}

						internal static void ff3Command_SetCamera_PositionOffset2(ScriptEngine engine)
						{
							VecFx32 vecFx = ff3Command_reuse_v0;
							vecFx.set((int)engine.getDword(), (int)engine.getDword(), (int)engine.getDword());
							uint word = engine.getWord();
							cmr.CWorldCamera.MOVE_TYPE dword = (cmr.CWorldCamera.MOVE_TYPE)engine.getDword();
							engine.getWord();
							engine.getWord();
							engine.getWord();
							VecFx32 vecFx2 = ff3Command_reuse_v1;
							VEC_Subtract(vecFx, CCastCommandTransit.getInstance().cast_FieldCamera().PosOffset(), vecFx2);
							CCastCommandTransit.getInstance().cast_FieldCamera().setPosOffsetMoveType(dword);
							CCastCommandTransit.getInstance().cast_FieldCamera().setPosOffsetSpeed(FX_Div(VEC_Mag(vecFx2), (int)(word << 12)));
							CCastCommandTransit.getInstance().cast_FieldCamera().setSucPosOffset(vecFx);
						}

						internal static void ff3Command_SetCamera_TargetOffset2(ScriptEngine engine)
						{
							VecFx32 vecFx = ff3Command_reuse_v0;
							vecFx.set((int)engine.getDword(), (int)engine.getDword(), (int)engine.getDword());
							uint word = engine.getWord();
							cmr.CWorldCamera.MOVE_TYPE dword = (cmr.CWorldCamera.MOVE_TYPE)engine.getDword();
							engine.getWord();
							engine.getWord();
							engine.getWord();
							VecFx32 vecFx2 = ff3Command_reuse_v1;
							VEC_Subtract(vecFx, CCastCommandTransit.getInstance().cast_FieldCamera().TrgOffset(), vecFx2);
							CCastCommandTransit.getInstance().cast_FieldCamera().setTrgOffsetMoveType(dword);
							CCastCommandTransit.getInstance().cast_FieldCamera().setTrgOffsetSpeed(FX_Div(VEC_Mag(vecFx2), (int)(word << 12)));
							CCastCommandTransit.getInstance().cast_FieldCamera().setSucTrgOffset(vecFx);
						}

						internal static void ff3Command_SetCamera_FOV(ScriptEngine engine)
						{
							int word = engine.getWord();
							int word2 = engine.getWord();
							int num = word << 12;
							int num2 = word2 << 12;
							CCastCommandTransit.getInstance().cast_FieldCamera().setFOV(num, num2);
						}

						internal static void ff3Command_SetDispSelect(ScriptEngine engine)
						{
							GX_SetDispSelect((engine.getByte() == 0) ? GXDispSelect.GX_DISP_SELECT_MAIN_SUB : GXDispSelect.GX_DISP_SELECT_SUB_MAIN);
						}

						internal static void ff3Command_SetMessageColor(ScriptEngine engine)
						{
							uint messageColor = engine.getByte();
							CCastCommandTransit.getInstance().cast_Field2D().MessageWindow()
								.setMessageColor((int)messageColor);
						}

						internal static void ff3Command_StartConnectionMenu(ScriptEngine engine)
						{
							engine.getByte();
							wld.WorldPart.getInstance().getWorldSystem().setMapSoundSetting(b: true);
							wld.WorldPart.getInstance().getWorldSystem().appendPWS(MogNet.getSingleton());
							engine.wait(1u);
						}

						internal static void ff3Command_DisplayMapName(ScriptEngine engine)
						{
							uint dword = engine.getDword();
							CCastCommandTransit.getInstance().cast_Field2D().refMapNameWindow()
								.open((int)dword);
							CCastCommandTransit.getInstance().cast_Field2D().refMapNameWindow()
								.setCloseCounter(60);
						}

						internal static void ff3Command_HideMapName(ScriptEngine engine)
						{
							CCastCommandTransit.getInstance().cast_Field2D().refMapNameWindow()
								.close();
						}

						internal static void ff3Command_LoadWorldBG(ScriptEngine engine)
						{
							string text = engine.getString();
							if (strcmp("end_40", text) == 0)
							{
								string arg = "";
								string text2 = "";
								getCompanyDirectory(text2);
								sprintf(out arg, "/DS/%s", text2);
								FS_ChangeDir(arg);
								CCastCommandTransit.getInstance().cast_BaseSystem().WorldBGCtrl()
									.wbcSetup(text);
								UserInfo.AwardAchievement(4);
							}
							else
							{
								FS_ChangeDir("/DS");
								CCastCommandTransit.getInstance().cast_BaseSystem().WorldBGCtrl()
									.wbcSetup(text);
							}
							FS_ChangeDir("/");
							CCastCommandTransit.getInstance().cast_Field2D().refWorldMap()
								.hideMapMarker();
						}

						internal static void ff3Command_SetPositionWorldBG(ScriptEngine engine)
						{
							NNSG2dBGSelect nNSG2dBGSelect = (NNSG2dBGSelect)engine.getByte();
							int x = (int)(engine.getDword() / 4096);
							int y = (int)(engine.getDword() / 4096);
							engine.getDword();
							CCastCommandTransit.getInstance().cast_BaseSystem().WorldBGCtrl()
								.wbcSetPosition(nNSG2dBGSelect, x, y);
						}

						internal static void ff3Command_AddPositionWorldBG(ScriptEngine engine)
						{
							NNSG2dBGSelect nNSG2dBGSelect = static_cast<NNSG2dBGSelect>(engine.getByte());
							int x = static_cast<int>(engine.getDword() / 4096);
							int y = static_cast<int>(engine.getDword() / 4096);
							engine.getDword();
							CCastCommandTransit.getInstance().cast_BaseSystem().WorldBGCtrl()
								.wbcAddPosition(nNSG2dBGSelect, x, y);
						}

						internal static void ff3Command_SetScrollWorldBG(ScriptEngine engine)
						{
							NNSG2dBGSelect nNSG2dBGSelect = static_cast<NNSG2dBGSelect>(engine.getByte());
							ushort word = engine.getWord();
							int x = static_cast<int>(engine.getDword() / 4096);
							int y = static_cast<int>(engine.getDword() / 4096);
							engine.getDword();
							CCastCommandTransit.getInstance().cast_BaseSystem().WorldBGCtrl()
								.wbcSetScroll(nNSG2dBGSelect, word, x, y);
						}

						internal static void ff3Command_SetVisibleWorldBG(ScriptEngine engine)
						{
							NNSG2dBGSelect nNSG2dBGSelect = (NNSG2dBGSelect)engine.getByte();
							bool visible = engine.getByte() != 0;
							CCastCommandTransit.getInstance().cast_BaseSystem().WorldBGCtrl()
								.wbcSetVisible(nNSG2dBGSelect, visible);
						}

						internal static void ff3Command_FadeoutSub(ScriptEngine engine)
						{
							ushort word = engine.getWord();
							byte b = engine.getByte();
							dgs.CFade.Sub().fadeOut(word, static_cast<dgs.CFade.FADE_TYPE>(b));
						}

						internal static void ff3Command_FadeinSub(ScriptEngine engine)
						{
							ushort word = engine.getWord();
							dgs.CFade.Sub().fadeIn(word);
						}

						internal static void ff3Command_InputCheckJump(ScriptEngine engine)
						{
							uint dword = engine.getDword();
							if (ds.g_Pad.pad() != 0)
							{
								engine.jump(dword);
							}
							else if (ds.g_TouchPanel.isTouch())
							{
								engine.jump(dword);
							}
						}

						internal static void ff3Command_LoadWorldOBJ(ScriptEngine engine)
						{
							byte idx = engine.getByte();
							string filename = engine.getString();
							GXS_SetVisiblePlane(31);
							CCastCommandTransit.getInstance().cast_Field2D().refWorldMap()
								.hideMapMarker();
							CCastCommandTransit.getInstance().cast_BaseSystem().WorldOBJCtrl()
								.wocSetup(idx, filename);
						}

						internal static void ff3Command_SetPositionWorldOBJ(ScriptEngine engine)
						{
							byte idx = engine.getByte();
							int num = (int)(engine.getDword() / 4096);
							int num2 = (int)(engine.getDword() / 4096);
							engine.getDword();
							CCastCommandTransit.getInstance().cast_BaseSystem().WorldOBJCtrl()
								.wocSetPosition(idx, num + 112, num2 + 64);
						}

						internal static void ff3Command_AddPositionWorldOBJ(ScriptEngine engine)
						{
							byte idx = engine.getByte();
							int x = static_cast<int>(engine.getDword() / 4096);
							int y = static_cast<int>(engine.getDword() / 4096);
							engine.getDword();
							CCastCommandTransit.getInstance().cast_BaseSystem().WorldOBJCtrl()
								.wocAddPosition(idx, x, y);
						}

						internal static void ff3Command_SetAlphaWorldOBJ(ScriptEngine engine)
						{
							byte alpha = engine.getByte();
							CCastCommandTransit.getInstance().cast_BaseSystem().WorldOBJCtrl()
								.wocSetAlpha(alpha);
						}

						internal static void ff3Command_SetAutoAlphaWorldOBJ(ScriptEngine engine)
						{
							ushort word = engine.getWord();
							byte alpha = engine.getByte();
							if (word > 0)
							{
								CCastCommandTransit.getInstance().cast_BaseSystem().WorldOBJCtrl()
									.wocSetAutoAlpha(word, alpha);
							}
							else
							{
								CCastCommandTransit.getInstance().cast_BaseSystem().WorldOBJCtrl()
									.wocSetAlpha(alpha);
							}
						}

						internal static void ff3Command_SetVisibleWorldOBJ(ScriptEngine engine)
						{
							byte idx = engine.getByte();
							bool visible = static_cast<bool>(engine.getByte());
							CCastCommandTransit.getInstance().cast_BaseSystem().WorldOBJCtrl()
								.wocSetVisible(idx, visible);
						}

						internal static void ff3Command_AssginBGForMessage(ScriptEngine engine)
						{
							byte b = engine.getByte();
							byte bGNumber = engine.getByte();
							byte x = engine.getByte();
							byte y = engine.getByte();
							byte w = engine.getByte();
							byte h = engine.getByte();
							switch (b)
							{
							case 0:
								dgs.msg.CMessageSys.getInstance().Main().assignBG(bGNumber, x, y, w, h);
								break;
							case 1:
								dgs.msg.CMessageSys.getInstance().Sub().assignBG(bGNumber, x, y, w, h);
								break;
							}
						}

						internal static void ff3Command_SetAutoAlphaMessage(ScriptEngine engine)
						{
							byte b = engine.getByte();
							byte b2 = engine.getByte();
							ushort word = engine.getWord();
							byte b3 = engine.getByte();
							byte b4 = engine.getByte();
							int num = 0;
							int num2 = 0;
							num2 = 63;
							int num3 = 0;
							switch (b2)
							{
							case 0:
								num3 = 1;
								break;
							case 1:
								num3 = 2;
								break;
							case 2:
								num3 = 4;
								break;
							case 3:
								num3 = 8;
								break;
							}
							num = num3;
							num2 &= ~num3;
							CCastCommandTransit.getInstance().cast_BaseSystem().WorldBGCtrl()
								.wbcSetEffect((WBE_SCREENSELECT)b, WBE_EFFECTTYPE.WBE_EFFECTTYPE_ALPHA, num, num2, (sbyte)b3, (sbyte)b4, word);
						}

						internal static void ff3Command_DisplayAllMessage(ScriptEngine engine)
						{
							CCastCommandTransit.getInstance().cast_Field2D().MessageWindow()
								.displayAllMessage();
						}

						internal static void ff3Command_SetAutoBrightnessWorldBG(ScriptEngine engine)
						{
							byte b = engine.getByte();
							byte planeA = engine.getByte();
							ushort word = engine.getWord();
							short num = (short)(engine.getByte() - 16);
							short num2 = (short)(engine.getByte() - 16);
							CCastCommandTransit.getInstance().cast_BaseSystem().WorldBGCtrl()
								.wbcSetEffect((WBE_SCREENSELECT)b, WBE_EFFECTTYPE.WBE_EFFECTTYPE_BRIGHTNESS, planeA, 0, (sbyte)num, (sbyte)num2, word);
						}

						internal static void ff3Command_SetBattleType(ScriptEngine engine)
						{
							byte battleType = engine.getByte();
							btl.OutsideToBattle.getInstance().setBattleType((btl.BATTLE_TYPE)battleType);
						}

						internal static void ff3Command_ChangeStage(ScriptEngine engine)
						{
							string stageName = const_cast<string>(engine.getString());
							stageMng.delStage();
							CCastCommandTransit.getInstance().cast_BaseSystem().setupStage(stageName, wld.CBaseSystem.WORLD_MODE.WORLD_MODE_TALK);
						}

						internal static void ff3Command_GoToTitle(ScriptEngine engine)
						{
							wld.CBaseSystem.setTitle(b: true);
							SuspendSaveDataGlobal.getSingleton().setup();
							SuspendSaveDataGlobal.getSingleton().invalidate();
							SuspendSaveDataGlobal.getSingleton().release();
						}

						internal static void ff3Command_SetCanBoardVehicle(ScriptEngine engine)
						{
							int dword = (int)engine.getDword();
							bool flag = engine.getByte() != 0;
							dword -= 3;
							wld.SHoldVehicleData holdData = wld.CWorldOutSideData.getInstance().VehicleData().getHoldData(dword);
							holdData.m_CanBoard = flag;
							wld.CWorldOutSideData.getInstance().VehicleData().setHoldData(dword, holdData.m_FieldNo, holdData.m_Position, holdData.m_Rotation, holdData.m_CanBoard);
							pl.CPlayerVehicle cPlayerVehicle = null;
							byte b = 0;
							b = 0;
							while ((uint)b < 4u)
							{
								cPlayerVehicle = CCastCommandTransit.getInstance().cast_PlayerMng().PlayerVehicle(b);
								if (cPlayerVehicle.getVehicleType() == (pl.PLAYER_VEHICLE_TYPE)dword)
								{
									break;
								}
								b++;
							}
							if ((uint)b < 4u)
							{
								cPlayerVehicle.setCanBoard(flag);
								if (flag)
								{
									cPlayerVehicle.getCckRadius_set(73728);
									cPlayerVehicle.getColFlag_or(2);
								}
							}
						}

						internal static void ff3Command_SetSendMessage(ScriptEngine engine)
						{
							bool sendMessage = engine.getByte() != 0;
							CCastCommandTransit.getInstance().cast_Field2D().MessageWindow()
								.setSendMessage(sendMessage);
						}

						internal static void ff3Command_WaitInputSendMessage(ScriptEngine engine)
						{
							if (!CCastCommandTransit.getInstance().cast_Field2D().MessageWindow()
								.isNextPageButton())
							{
								engine.suspendRedo();
							}
						}

						internal static void ff3Command_SetSignEffect(ScriptEngine engine)
						{
							ushort word = engine.getWord();
							ushort word2 = engine.getWord();
							ushort word3 = engine.getWord();
							VecFx32 vecFx = ff3Command_reuse_v0;
							vecFx.set((int)engine.getDword(), (int)engine.getDword(), (int)engine.getDword());
							engine.getDword();
							engine.getDword();
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (-1 != num)
							{
								num -= (int)(pl.FIELD_CHARACTER_NUM - pl.MAP_OBJECT_NUM);
								CCastCommandTransit.getInstance().cast_PlayerMng().MapObject(num)
									.setSignEffect(CCastCommandTransit.getInstance().cast_FieldCamera(), vecFx, word2, word3);
							}
						}

						internal static void ff3Command_SetAreaChangeShutterFlag(ScriptEngine engine)
						{
							byte b = engine.getByte();
							CCastCommandTransit.getInstance().cast_BaseSystem().setAreaChangeShutterFlag(b != 0);
						}

						internal static void ff3Command_CustomFadeSetting(ScriptEngine engine)
						{
							short frame = (short)engine.getWord();
							dgs.CFade.FADE_TYPE word = (dgs.CFade.FADE_TYPE)engine.getWord();
							CCastCommandTransit.getInstance().cast_BaseSystem().customFadeSetting(frame, word);
						}

						internal static void ff3Command_StartRotateMove(ScriptEngine engine)
						{
							ushort word = engine.getWord();
							VecFx32 vecFx = ff3Command_reuse_v0;
							VecFx32 vecFx2 = ff3Command_reuse_v1;
							vecFx.set((int)engine.getDword(), (int)engine.getDword(), (int)engine.getDword());
							vecFx2.set((int)engine.getDword(), (int)engine.getDword(), (int)engine.getDword());
							ushort word2 = engine.getWord();
							byte dir = engine.getByte();
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (-1 != num)
							{
								VecFx32 vecFx3 = ff3Command_reuse_v2;
								vecFx3.copy(CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.getPosition());
								VEC_Add(vecFx3, vecFx, vecFx3);
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.MoveSys()
									.setCircleMove(vecFx3, vecFx2.x, vecFx2.z, dir, word2);
							}
						}

						internal static void ff3Command_StartRotateMoveSyncRotate(ScriptEngine engine)
						{
							ushort word = engine.getWord();
							VecFx32 vecFx = ff3Command_reuse_v0;
							VecFx32 vecFx2 = ff3Command_reuse_v1;
							vecFx.set((int)engine.getDword(), (int)engine.getDword(), (int)engine.getDword());
							vecFx2.set((int)engine.getDword(), (int)engine.getDword(), (int)engine.getDword());
							ushort word2 = engine.getWord();
							byte dir = engine.getByte();
							ushort yOrgRotate = ds.DEGto65536(engine.getWord());
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (-1 != num)
							{
								VecFx32 vecFx3 = ff3Command_reuse_v2;
								vecFx3.copy(CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.getPosition());
								VEC_Add(vecFx3, vecFx, vecFx3);
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.MoveSys()
									.setCircleMoveSyncRotate(vecFx3, vecFx2.x, vecFx2.z, dir, word2, yOrgRotate);
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.TurnSys()
									.setFixed(_Fixed: true);
							}
						}

						internal static void ff3Command_EndRotateMove(ScriptEngine engine)
						{
							ushort word = engine.getWord();
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (-1 != num)
							{
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.MoveSys()
									.setLinearMove();
							}
						}

						internal static void ff3Command_StartWaveGravity(ScriptEngine engine)
						{
							ushort word = engine.getWord();
							ushort word2 = engine.getWord();
							int dword = (int)engine.getDword();
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (-1 != num)
							{
								int y = CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.getPosition()
									.y;
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.MoveSys()
									.setWaveGravity(y, dword, word2);
							}
						}

						internal static void ff3Command_EndWaveGravity(ScriptEngine engine)
						{
							ushort word = engine.getWord();
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (-1 != num)
							{
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.MoveSys()
									.setNormalGravity();
							}
						}

						internal static void ff3Command_SetPlayerLevel(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint word2 = engine.getWord();
							byte b = (byte)word;
							byte level = (byte)word2;
							b -= 5;
							pl.PlayerParty.instance().playerForId(b).growParameter(level);
						}

						internal static void ff3Command_ReleaseStageMap(ScriptEngine engine)
						{
							stageMng.delStage();
						}

						internal static void ff3Command_SetVehicleFieldNo(ScriptEngine engine)
						{
							int dword = (int)engine.getDword();
							byte b = engine.getByte();
							dword -= 3;
							wld.SHoldVehicleData holdData = wld.CWorldOutSideData.getInstance().VehicleData().getHoldData(dword);
							wld.CWorldOutSideData.getInstance().VehicleData().setHoldData(dword, (sbyte)b, holdData.m_Position, holdData.m_Rotation, holdData.m_CanBoard);
						}

						internal static void ff3Command_SetItemUseMenuManager(ScriptEngine engine)
						{
							switch (state_)
							{
							case 0:
								menu.MenuManager.getSingleton().SetUsingMenuType(1);
								CCastCommandTransit.getInstance().cast_Field2D().ItemUseMenuManager()
									.create();
								menu.MenuManager.getSingleton().ClearTargetItemNo();
								menu.MenuManager.getSingleton().SetDecideButtonState(1);
								menu.MenuManager.getSingleton().SetCancelButtonState(1);
								state_ = 1;
								engine.suspendRedo();
								break;
							case 1:
							{
								bool flag = false;
								if (menu.MenuManager.getSingleton().GetDecideButtonState() == 0)
								{
									if (menu.MenuManager.getSingleton().checkTouchState() != menu.MenuManager.TOUCH_STATE.NOT_TOUCH)
									{
										menu.MenuManager.getSingleton().playSEDecide();
									}
									int targetItemNo = menu.MenuManager.getSingleton().GetTargetItemNo();
									evt.CEventManager.getInstance().setUseItemId(targetItemNo);
									evt.CEventManager.getInstance().setItemEvent(_ItemEvent: true);
									flag = true;
								}
								else
								{
									ds.g_TouchPanel.getPoint(out var x, out var y);
									if (menu.MenuManager.getSingleton().GetCancelButtonState() == 0 || (ds.g_TouchPanel.isTap() & menu.MenuManager.getSingleton().TouchWindowOutArea(x, y)))
									{
										flag = true;
									}
								}
								if (!flag)
								{
									engine.suspendRedo();
									break;
								}
								CCastCommandTransit.getInstance().cast_Field2D().ItemUseMenuManager()
									.erase();
								state_ = 0;
								break;
							}
							}
						}

						internal static void ff3Command_RestrictionItemUse(ScriptEngine engine)
						{
							int dword = (int)engine.getDword();
							evt.CEventRestriction.getSingleton().entry(dword);
						}

						internal static void ff3Command_ChocoboBank(ScriptEngine engine)
						{
							wld.WorldPart.getInstance().getWorldSystem().appendPWS(ChocoboBank.getSingleton());
							engine.wait(1u);
						}

						internal static void ff3Command_BootPlainCharacter(ScriptEngine engine)
						{
							uint word = engine.getWord();
							byte b = engine.getByte();
							string text = engine.getString();
							int manCastIndex = evt.CHichParameterManager.getInstance().getManCastIndex(word);
							string text2 = "";
							text2 = text;
							VecFx32 vecFx = ff3Command_reuse_v0;
							VecFx32 vecFx2 = ff3Command_reuse_v1;
							VecFx32 vecFx3 = ff3Command_reuse_v2;
							VecFx32 vecFx4 = ff3Command_reuse_v3;
							vecFx.set(4096 * static_cast<int>(evt.CHichParameterManager.getInstance().getSubParam(manCastIndex).m_Position[0]), 4096 * static_cast<int>(evt.CHichParameterManager.getInstance().getSubParam(manCastIndex).m_Position[1]), 4096 * static_cast<int>(evt.CHichParameterManager.getInstance().getSubParam(manCastIndex).m_Position[2]));
							vecFx2.set(4096 * FX_DEG_TO_IDX(static_cast<int>(evt.CHichParameterManager.getInstance().getSubParam(manCastIndex).m_Posture[0])) * -1, 4096 * FX_DEG_TO_IDX(static_cast<int>(evt.CHichParameterManager.getInstance().getSubParam(manCastIndex).m_Posture[1])) * -1, 4096 * FX_DEG_TO_IDX(static_cast<int>(evt.CHichParameterManager.getInstance().getSubParam(manCastIndex).m_Posture[2])) * -1);
							vecFx3.set(4096 * static_cast<int>(evt.CHichParameterManager.getInstance().getSubParam(manCastIndex).m_Scale[0]), 4096 * static_cast<int>(evt.CHichParameterManager.getInstance().getSubParam(manCastIndex).m_Scale[1]), 4096 * static_cast<int>(evt.CHichParameterManager.getInstance().getSubParam(manCastIndex).m_Scale[2]));
							vecFx4.set(4915, 4096, 4915);
							if (text2[0] == 'j' && b != 0)
							{
								char[] array = new char[2];
								int num = 0;
								array[0] = text2[1];
								array[1] = '\0';
								num = atoi(new string(array)) - 1;
								sprintf(out text2, "j%d%02d", pl.PlayerParty.instance().playerForId((byte)num).playerId() + 1, pl.PlayerParty.instance().playerForId((byte)num).jobManager()
									.nowJob() + 1);
							}
							TexDivideLoader.getSingleton().tdlForceLoad();
							pl.PLAYER_HUMAN_TYPE arg = pl.PLAYER_HUMAN_TYPE.PLAYER_HUMAN_TYPE_HUMAN;
							if (strcmp(text2, "n441") == 0)
							{
								arg = pl.PLAYER_HUMAN_TYPE.PLAYER_HUMAN_TYPE_CHOKOBO;
							}
							if (strcmp(text2, "n551") == 0)
							{
								VEC_Set(vecFx3, 5324, 5324, 5324);
							}
							else if (strcmp(text2, "n031") == 0 || strcmp(text2, "n041") == 0)
							{
								VEC_Set(vecFx3, 3276, 3276, 3276);
								VEC_Set(vecFx4, 2730, 2730, 2730);
							}
							else if (strcmp(text2, "n431") == 0)
							{
								arg = pl.PLAYER_HUMAN_TYPE.PLAYER_HUMAN_TYPE_FROG;
								VEC_Set(vecFx3, 1228, 1228, 1228);
								VEC_Set(vecFx4, 2730, 2730, 2730);
							}
							else if (strcmp(text2, "n251") == 0)
							{
								VEC_Set(vecFx3, 3276, 3276, 3276);
								VEC_Set(vecFx4, 2730, 2730, 2730);
							}
							else if (strcmp(text2, "n261") == 0)
							{
								arg = pl.PLAYER_HUMAN_TYPE.PLAYER_HUMAN_TYPE_SHEEP;
								VEC_Set(vecFx3, 3072, 3072, 3072);
								VEC_Set(vecFx4, 2730, 2730, 2730);
							}
							else if (strcmp(text2, "n221") == 0)
							{
								arg = pl.PLAYER_HUMAN_TYPE.PLAYER_HUMAN_TYPE_FAIRY;
								VEC_Set(vecFx3, 1638, 1638, 1638);
								VEC_Set(vecFx4, 2730, 2730, 2730);
							}
							else if (strcmp(text2, "n351") == 0)
							{
								VEC_Set(vecFx3, 2867, 2867, 2867);
								VEC_Set(vecFx4, 2867, 2867, 2867);
							}
							int num2 = CCastCommandTransit.getInstance().cast_PlayerMng().setupPlainCharacter(text2, vecFx3, vecFx4);
							TexDivideLoader.getSingleton().tdlForceLoad();
							characterMng.setupOrgTex(CCastCommandTransit.getInstance().cast_PlayerMng().Player(num2)
								.getCharacterId());
							CCastCommandTransit.getInstance().cast_PlayerMng().Player(num2)
								.into();
							CCastCommandTransit.getInstance().cast_PlayerMng().Player(num2)
								.setAutoPilot(_AutoPilot: true);
							CCastCommandTransit.getInstance().cast_PlayerMng().Player(num2)
								.LogicIndex_set(word);
							CCastCommandTransit.getInstance().cast_PlayerMng().Player(num2)
								.setPosition(vecFx);
							CCastCommandTransit.getInstance().cast_PlayerMng().Player(num2)
								.setRotation(vecFx2);
							CCastCommandTransit.getInstance().cast_PlayerMng().Player(num2)
								.setScale(vecFx3);
							CCastCommandTransit.getInstance().cast_PlayerMng().Player(num2)
								.setShadowScale(vecFx4);
							CCastCommandTransit.getInstance().cast_PlayerMng().Player(num2)
								.setTargetDirectionFromRotation();
							CCastCommandTransit.getInstance().cast_PlayerMng().Player(num2)
								.getPreParamObj_set(CCastCommandTransit.getInstance().cast_PlayerMng().Player(num2)
									.getParamObj());
							CCastCommandTransit.getInstance().cast_PlayerMng().Player(num2)
								.setMainPos(CCastCommandTransit.getInstance().cast_PlayerMng().Player(num2)
									.getPosition());
							evt.CHichParameterManager.getInstance().setCharaIndex(manCastIndex, num2);
							CCastCommandTransit.getInstance().cast_PlayerMng().PlayerHuman(num2)
								.HumanType_set(arg);
							CCastCommandTransit.getInstance().cast_PlayerMng().PlayerHuman(num2)
								.PlayerMoveType_set(pl.PLAYER_MOVE_TYPE.PLAYER_MOVE_TYPE_DEFAULT_NPC);
							CCastCommandTransit.getInstance().cast_PlayerMng().PlayerHuman(num2)
								.NPCRandomMoveType_set(pl.NPC_RANDOM_MOVE_TYPE.NPC_RANDOM_MOVE_TYPE_DEFAULT);
							CCastCommandTransit.getInstance().cast_PlayerMng().PlayerHuman(num2)
								.NPCAutoFollowType_set(pl.NPC_AUTO_FOLLOW_TYPE.NPC_AUTO_FOLLOW_TYPE_DEFAULT);
						}

						internal static bool isClaw(string pObjName)
						{
							if (pObjName[0] != 'w')
							{
								return false;
							}
							int num = atoi(pObjName.Substring(1));
							if (num != 120 && num != 121 && num != 122 && num != 123 && num != 124 && num != 125)
							{
								return num == 127;
							}
							return true;
						}

						internal static void ff3Command_CreateBindObject(ScriptEngine engine)
						{
							ushort word = engine.getWord();
							pl.CPlayerHuman.BIND_LOCATE bIND_LOCATE = (pl.CPlayerHuman.BIND_LOCATE)engine.getByte();
							string text = engine.getString();
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (-1 != num && (long)num < 24L)
							{
								pl.CPlayerHuman cPlayerHuman = CCastCommandTransit.getInstance().cast_PlayerMng().PlayerHuman(num);
								pl.BindObject bindObject = pl.BindObject.createBindObject(text, cPlayerHuman);
								cPlayerHuman.setBindObject(bindObject, bIND_LOCATE);
								ushort rot = 0;
								int _ = 0;
								switch (bIND_LOCATE)
								{
								case pl.CPlayerHuman.BIND_LOCATE.BIND_LOCATE_RHAND:
									rot = ds.DEGto65536(270);
									_ = -2457;
									break;
								case pl.CPlayerHuman.BIND_LOCATE.BIND_LOCATE_LHAND:
									rot = ds.DEGto65536(90);
									_ = -2457;
									break;
								}
								MtxFx43 mtxFx = new MtxFx43();
								MtxFx43 mtxFx2 = new MtxFx43();
								if (isClaw(text))
								{
									ds.CpuMatrix.setRotateY(mtxFx2, rot);
								}
								else
								{
									ds.CpuMatrix.setRotateZ(mtxFx2, rot);
								}
								MTX_Identity43(mtxFx);
								mtxFx._30 = _;
								MtxFx43 mtxFx3 = new MtxFx43();
								MTX_Concat43(mtxFx2, mtxFx, mtxFx3);
								bindObject.setOffsetMtx(mtxFx3);
							}
						}

						internal static void ff3Command_SetVisibleBindObject(ScriptEngine engine)
						{
							ushort word = engine.getWord();
							byte locate = engine.getByte();
							bool visible = engine.getByte() != 0;
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (-1 != num && (long)num < 24L)
							{
								pl.CPlayerHuman cPlayerHuman = CCastCommandTransit.getInstance().cast_PlayerMng().PlayerHuman(num);
								pl.BindObject bindObject = cPlayerHuman.getBindObject((pl.CPlayerHuman.BIND_LOCATE)locate);
								bindObject.setVisible(visible);
							}
						}

						internal static void ff3Command_BindMotion(ScriptEngine engine)
						{
							ushort word = engine.getWord();
							string pMotname = engine.getString();
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (-1 != num && (long)num < 24L)
							{
								pl.CPlayerHuman cPlayerHuman = CCastCommandTransit.getInstance().cast_PlayerMng().PlayerHuman(num);
								cPlayerHuman.addMotion(pMotname);
							}
						}

						internal static void ff3Command_RemoveMotion(ScriptEngine engine)
						{
							ushort word = engine.getWord();
							string motname = engine.getString();
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (-1 != num && (long)num < 24L)
							{
								pl.CPlayerHuman cPlayerHuman = CCastCommandTransit.getInstance().cast_PlayerMng().PlayerHuman(num);
								characterMng.removeMotion(cPlayerHuman.getCharacterId(), motname);
							}
						}

						internal static void ff3Command_DeleteBindObject(ScriptEngine engine)
						{
							ushort word = engine.getWord();
							byte locate = engine.getByte();
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (-1 != num && (long)num < 24L)
							{
								pl.CPlayerHuman cPlayerHuman = CCastCommandTransit.getInstance().cast_PlayerMng().PlayerHuman(num);
								cPlayerHuman.deleteBindObject((pl.CPlayerHuman.BIND_LOCATE)locate);
							}
						}

						internal static void ff3Command_PlayStream(ScriptEngine engine)
						{
							engine.getDword();
							engine.getDword();
							engine.getDword();
						}

						internal static void ff3Command_StopStream(ScriptEngine engine)
						{
							engine.getDword();
						}

						internal static void ff3Command_ResetFollowNPCState(ScriptEngine engine)
						{
							if (pl.PlayerParty.instance().npc().isEnable())
							{
								pl.CPlayerHuman cPlayerHuman = CCastCommandTransit.getInstance().cast_PlayerMng().PlayerHuman(0);
								pl.CPlayerHuman cPlayerHuman2 = cPlayerHuman.getNpc();
								if (cPlayerHuman != null && cPlayerHuman2 != null)
								{
									VecFx32 position = cPlayerHuman.getPosition();
									VecFx32 rotation = cPlayerHuman.getRotation();
									VecFx32 vecFx = ff3Command_reuse_v0;
									VecFx32 vecFx2 = ff3Command_reuse_v1;
									VEC_Set(vecFx2, FX_CosIdx((ushort)rotation.x), 0, FX_SinIdx((ushort)rotation.z));
									VEC_Normalize(vecFx2, vecFx2);
									VEC_MultAdd(-8192, vecFx2, position, vecFx);
									cPlayerHuman2.setPosition(vecFx);
									cPlayerHuman2.setRotation(rotation);
									cPlayerHuman2.setTargetDirectionFromRotation();
									cPlayerHuman2.setTransparencyRate(0);
									cPlayerHuman2.setShadowAlpha(0);
									cPlayerHuman2.setPreAct(0);
									cPlayerHuman2.setNowAct(0);
									cPlayerHuman2.setNextAct(0);
								}
							}
						}

						internal static void ff3Command_CallSpecialMode(ScriptEngine engine)
						{
							wld.WorldPart.getInstance().getWorldSystem().setSpecial(b: true);
							engine.wait(1u);
						}

						internal static void ff3Command_CallSave(ScriptEngine engine)
						{
							VecFx32 vecFx = ff3Command_reuse_v0;
							VecFx32 vecFx2 = ff3Command_reuse_v1;
							vecFx.set(4374528, 0, -3297280);
							vecFx2.set(0, 0, 0);
							CCastCommandTransit.getInstance().cast_PlayerMng().Player(0)
								.setPosition(vecFx);
							CCastCommandTransit.getInstance().cast_PlayerMng().Player(0)
								.setRotation(vecFx2);
							sceneMng.setStage("f03_C3");
							ds.GlobalPlayTimeCounter.getSingleton().pause(b: true);
							if (0 >= pl.PlayerParty.instance().mania().clearTime()
								.get())
							{
								pl.PlayerParty.instance().mania().clearTime()
									.set((int)ds.GlobalPlayTimeCounter.getSingleton().get());
							}
							wld.WorldPart.getInstance().getWorldSystem().setSave(b: true);
							engine.wait(1u);
						}

						internal static void ff3Command_CancelSetupMapSound(ScriptEngine engine)
						{
							wld.WorldPart.getInstance().getWorldSystem().setMapSoundSetting(b: false);
						}

						internal static void ff3Command_BootCharacterAsTopPlayer(ScriptEngine engine)
						{
							uint word = engine.getWord();
							int num = -1;
							for (byte b = 0; b < 4; b++)
							{
								if (pl.PlayerParty.instance().player(b).isEnable() && !pl.PlayerParty.instance().player(b).condition()
									.isDeath() && !pl.PlayerParty.instance().player(b).condition()
									.isStone())
								{
									num = pl.PlayerParty.instance().player(b).playerId();
									break;
								}
							}
							string arg = "";
							sprintf(out arg, "j%d01", num + 1);
							int manCastIndex = evt.CHichParameterManager.getInstance().getManCastIndex(word);
							VecFx32 vecFx = ff3Command_reuse_v0;
							VecFx32 vecFx2 = ff3Command_reuse_v1;
							VecFx32 vecFx3 = ff3Command_reuse_v2;
							VecFx32 vecFx4 = ff3Command_reuse_v3;
							vecFx.set(0, 0, 0);
							vecFx2.set(0, 0, 0);
							vecFx3.set(4096, 4096, 4096);
							vecFx4.set(4915, 4096, 4915);
							bootCharacterImp(manCastIndex, word, vecFx, vecFx2, vecFx3, vecFx4, arg);
						}

						internal static int getTopPlayerId()
						{
							for (int i = 0; i < 4; i++)
							{
								if (pl.PlayerParty.instance().player((byte)i).isEnable())
								{
									return pl.PlayerParty.instance().player((byte)i).playerId();
								}
							}
							return -1;
						}

						internal static void ff3Command_CheckTopPlayerJob(ScriptEngine engine)
						{
							uint dword = engine.getDword();
							uint dword2 = engine.getDword();
							int topPlayerId = getTopPlayerId();
							if (topPlayerId > -1 && dword == pl.PlayerParty.instance().playerForId((byte)topPlayerId).jobManager()
								.nowJob())
							{
								engine.jump(dword2);
							}
						}

						internal static void ff3Command_CheckJob(ScriptEngine engine)
						{
							bool flag = engine.getByte() != 0;
							uint dword = engine.getDword();
							uint dword2 = engine.getDword();
							bool flag2 = false;
							for (int i = 0; i < 4; i++)
							{
								if (pl.PlayerParty.instance().player((byte)i).isEnable() && dword == pl.PlayerParty.instance().player((byte)i).jobManager()
									.nowJob())
								{
									flag2 = true;
									break;
								}
							}
							if (flag)
							{
								if (flag2)
								{
									engine.jump(dword2);
								}
							}
							else if (!flag2)
							{
								engine.jump(dword2);
							}
						}

						internal static void ff3Command_CheckToPlayerJobLevel(ScriptEngine engine)
						{
							byte b = engine.getByte();
							uint dword = engine.getDword();
							int topPlayerId = getTopPlayerId();
							if (topPlayerId > -1 && b <= pl.PlayerParty.instance().playerForId((byte)topPlayerId).jobManager()
								.nowJobParameter()
								.skill()
								.skillLevel()
								.get())
							{
								engine.jump(dword);
							}
						}

						internal static void ff3Command_CheckJobLevel(ScriptEngine engine)
						{
							bool flag = engine.getByte() != 0;
							byte b = (byte)engine.getDword();
							byte b2 = engine.getByte();
							uint dword = engine.getDword();
							bool flag2 = false;
							for (byte b3 = 0; b3 < 4; b3++)
							{
								if (pl.PlayerParty.instance().player(b3).isEnable() && b == pl.PlayerParty.instance().player(b3).jobManager()
									.nowJob() && b2 <= pl.PlayerParty.instance().player(b3).jobManager()
									.nowJobParameter()
									.skill()
									.skillLevel()
									.get())
								{
									flag2 = true;
								}
							}
							if (flag)
							{
								if (flag2)
								{
									engine.jump(dword);
								}
							}
							else if (!flag2)
							{
								engine.jump(dword);
							}
						}

						internal static void ff3Command_CheckTopPlayer(ScriptEngine engine)
						{
							byte b = engine.getByte();
							uint dword = engine.getDword();
							int num = -1;
							for (byte b2 = 0; b2 < 4; b2++)
							{
								if (pl.PlayerParty.instance().player(b2).isEnable() && !pl.PlayerParty.instance().player(b2).condition()
									.isDeath() && !pl.PlayerParty.instance().player(b2).condition()
									.isStone())
								{
									num = pl.PlayerParty.instance().player(b2).playerId();
									break;
								}
							}
							if (-1 != num && num == b)
							{
								engine.jump(dword);
							}
						}

						internal static void ff3Command_SetShowWorldMap(ScriptEngine engine)
						{
							bool showOnePicture = engine.getByte() != 0;
							CCastCommandTransit.getInstance().cast_Field2D().setShowOnePicture(showOnePicture);
						}

						internal static void ff3Command_SetRestartAfterBattle(ScriptEngine engine)
						{
							if (engine.getByte() != 0)
							{
								btl.OutsideToBattle.getInstance().onRestart();
							}
							else
							{
								btl.OutsideToBattle.getInstance().offRestart();
							}
						}

						public static pl.CPlayerVehicle getPlayerVehicle()
						{
							byte b = 0;
							while ((uint)b < 4u)
							{
								pl.CPlayerVehicle cPlayerVehicle = CCastCommandTransit.getInstance().cast_PlayerMng().PlayerVehicle(b);
								if (cPlayerVehicle != null && cPlayerVehicle.getBoardPlayer() != null)
								{
									return cPlayerVehicle;
								}
								b++;
							}
							return null;
						}

						internal static void ff3Command_PlayNaviSE(ScriptEngine engine)
						{
							bool flag = engine.getByte() != 0;
							byte frame = engine.getByte();
							pl.CPlayerVehicle playerVehicle = getPlayerVehicle();
							if (playerVehicle != null)
							{
								playerVehicle.setEnablePlayNaviSE(flag);
								if (flag)
								{
									playerVehicle.playNaviSE();
								}
								else
								{
									playerVehicle.stopNaviSE(frame);
								}
							}
						}

						internal static void ff3Command_SetEnableVehicleCamHeight(ScriptEngine engine)
						{
							bool enableCalcCamHeight = engine.getByte() != 0;
							getPlayerVehicle()?.setEnableCalcCamHeight(enableCalcCamHeight);
						}

						internal static void ff3Command_SetPreRideVehicle(ScriptEngine engine)
						{
							int num = (int)(engine.getDword() - 3);
							if (num < 0)
							{
								num = -1;
							}
							wld.CWorldOutSideData.getInstance().VehicleData().setPreRidingOnVehicleNo((pl.PLAYER_VEHICLE_TYPE)num);
						}

						internal static void ff3Command_SetBeforeFieldNameJumpIndex(ScriptEngine engine)
						{
							string beforeFieldMapName = const_cast<string>(engine.getString());
							sbyte beforeFieldMapJumpIndex = (sbyte)engine.getByte();
							wld.CWorldOutSideData.getInstance().MapData().setBeforeFieldMapName(beforeFieldMapName);
							wld.CWorldOutSideData.getInstance().MapData().setBeforeFieldMapJumpIndex(beforeFieldMapJumpIndex);
						}

						internal static void ff3Command_SetEnableViewVolumeClip(ScriptEngine engine)
						{
							uint word = engine.getWord();
							bool enable = engine.getByte() != 0;
							int index = CCastCommandTransit.getInstance().changeHichNumber(word);
							characterMng.setViewVolumeClip(CCastCommandTransit.getInstance().cast_PlayerMng().Player(index)
								.getCharacterId(), enable);
						}

						internal static void ff3Command_SetPosition(ScriptEngine engine)
						{
							uint word = engine.getWord();
							VecFx32 vecFx = ff3Command_reuse_v0;
							vecFx.set((int)engine.getDword(), (int)engine.getDword(), (int)engine.getDword());
							int num = CCastCommandTransit.getInstance().changeHichNumber(word);
							if (-1 != num)
							{
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.setPosition(vecFx);
								CCastCommandTransit.getInstance().cast_PlayerMng().Player(num)
									.getPrePosition_set(vecFx);
							}
						}

						internal static void ff3Command_AllEffectFinishWait(ScriptEngine engine)
						{
							if (0 < eff.CEffectMng.instance().getEffectObjectNum())
							{
								engine.suspendRedo();
							}
						}

						internal static void ff3Command_StartMotionToRideVehicle(ScriptEngine engine)
						{
							int word = engine.getWord();
							pl.CPlayerVehicle cPlayerVehicle = null;
							byte b = 0;
							b = 0;
							while ((uint)b < 4u)
							{
								cPlayerVehicle = CCastCommandTransit.getInstance().cast_PlayerMng().PlayerVehicle(b);
								if (cPlayerVehicle.getBoardPlayer() != null)
								{
									break;
								}
								b++;
							}
							if ((uint)b < 4u)
							{
								cPlayerVehicle.startMotion(word, _Loop: true, 0u);
								characterMng.setMotionSpeed(cPlayerVehicle.getCharacterId(), 4096);
							}
						}

						internal static void ff3Command_InAppPurchase(ScriptEngine engine)
						{
						}

						internal static uint getFlagByte(uint index)
						{
							return index >> 3;
						}

						internal static byte getBitPattern(uint index)
						{
							return (byte)(1 << (int)(byte)(index & 7));
						}

						internal static byte[,] getFlagImage()
						{
							return flags;
						}

						internal static void setFlagImage(byte[,] pFlag, int nGroup, int nFlag)
						{
							memcpy(flags, pFlag, nGroup * nFlag);
						}

						internal static ushort getFromByteDatas_ushort(byte[] top, ref uint index)
						{
							ushort num = top[index];
							uint num2 = index + 2;
							index++;
							uint num3 = 8u;
							while (index < num2)
							{
								num |= (ushort)(top[index] << (int)num3);
								index++;
								num3 += 8;
							}
							return num;
						}

						internal static uint getFromByteDatas_uint(byte[] top, ref uint index)
						{
							uint num = top[index];
							uint num2 = index + 4;
							index++;
							uint num3 = 8u;
							while (index < num2)
							{
								num |= (uint)(top[index] << (int)num3);
								index++;
								num3 += 8;
							}
							return num;
						}

						internal static void NOPCommand(ScriptEngine unuse0)
						{
						}

						internal static void endCommand(ScriptEngine engine)
						{
							engine.end();
						}

						internal static void flagOnEndCommand(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint word2 = engine.getWord();
							if (FlagManager.singleton().get(word, word2) != 0)
							{
								engine.end();
							}
						}

						internal static void flagOffEndCommand(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint word2 = engine.getWord();
							if (FlagManager.singleton().get(word, word2) == 0)
							{
								engine.end();
							}
						}

						internal static void waitCommand(ScriptEngine engine)
						{
							engine.wait(engine.getWord());
						}

						internal static void waitFlagOnCommand(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint word2 = engine.getWord();
							if (FlagManager.singleton().get(word, word2) == 0)
							{
								engine.suspendRedo();
							}
						}

						internal static void waitFlagOffCommand(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint word2 = engine.getWord();
							if (FlagManager.singleton().get(word, word2) != 0)
							{
								engine.suspendRedo();
							}
						}

						internal static void jumpCommand(ScriptEngine engine)
						{
							engine.jump(engine.getDword());
						}

						internal static void flagOnJumpCommand(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint word2 = engine.getWord();
							uint dword = engine.getDword();
							if (FlagManager.singleton().get(word, word2) != 0)
							{
								engine.jump(dword);
							}
						}

						internal static void flagOffJumpCommand(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint word2 = engine.getWord();
							uint dword = engine.getDword();
							if (FlagManager.singleton().get(word, word2) == 0)
							{
								engine.jump(dword);
							}
						}

						internal static void callCommand(ScriptEngine engine)
						{
							uint dword = engine.getDword();
							engine.call(dword, engine.getDword());
						}

						internal static void flagOnCallCommand(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint word2 = engine.getWord();
							uint dword = engine.getDword();
							uint dword2 = engine.getDword();
							if (FlagManager.singleton().get(word, word2) != 0)
							{
								engine.call(dword, dword2);
							}
						}

						internal static void flagOffCallCommand(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint word2 = engine.getWord();
							uint dword = engine.getDword();
							uint dword2 = engine.getDword();
							if (FlagManager.singleton().get(word, word2) == 0)
							{
								engine.call(dword, dword2);
							}
						}

						internal static void returnCommand(ScriptEngine engine)
						{
							engine.scriptReturn();
						}

						internal static void flagOnReturnCommand(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint word2 = engine.getWord();
							if (FlagManager.singleton().get(word, word2) != 0)
							{
								engine.scriptReturn();
							}
						}

						internal static void flagOffReturnCommand(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint word2 = engine.getWord();
							if (FlagManager.singleton().get(word, word2) == 0)
							{
								engine.scriptReturn();
							}
						}

						internal static void flagOnCommand(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint word2 = engine.getWord();
							FlagManager.singleton().set(word, word2);
						}

						internal static void flagOffCommand(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint word2 = engine.getWord();
							FlagManager.singleton().reset(word, word2);
						}

						internal static void flagReverseCommand(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint word2 = engine.getWord();
							FlagManager.singleton().reverse(word, word2);
						}

						internal static void setValueCommand(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint word2 = engine.getWord();
							int dword = (int)engine.getDword();
							ValueManager.singleton().set(word, word2, dword);
						}

						internal static void incValueCommand(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint word2 = engine.getWord();
							ValueManager.singleton().inc(word, word2);
						}

						internal static void decValueCommand(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint word2 = engine.getWord();
							ValueManager.singleton().dec(word, word2);
						}

						internal static void addValueCommand(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint word2 = engine.getWord();
							int num = static_cast<int>(engine.getDword());
							int num2 = ValueManager.singleton().get(word, word2);
							ValueManager.singleton().set(word, word2, num2 + num);
						}

						internal static void mulValueCommand(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint word2 = engine.getWord();
							int num = static_cast<int>(engine.getDword());
							int num2 = ValueManager.singleton().get(word, word2);
							ValueManager.singleton().set(word, word2, num2 * num);
						}

						internal static void divValueCommand(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint word2 = engine.getWord();
							int num = static_cast<int>(engine.getDword());
							int num2 = ValueManager.singleton().get(word, word2);
							ValueManager.singleton().set(word, word2, num2 / num);
						}

						internal static void ifValueJumpCommand(ScriptEngine engine)
						{
							uint word = engine.getWord();
							uint word2 = engine.getWord();
							int num = ValueManager.singleton().get(word, word2);
							uint dword = engine.getDword();
							int dword2 = (int)engine.getDword();
							uint dword3 = engine.getDword();
							int num2 = 0;
							switch (dword)
							{
							case 0u:
								num2 = ((num > dword2) ? 1 : 0);
								break;
							case 1u:
								num2 = ((num >= dword2) ? 1 : 0);
								break;
							case 2u:
								num2 = ((num < dword2) ? 1 : 0);
								break;
							case 3u:
								num2 = ((num <= dword2) ? 1 : 0);
								break;
							case 4u:
								num2 = ((num == dword2) ? 1 : 0);
								break;
							case 5u:
								num2 = ((num != dword2) ? 1 : 0);
								break;
							}
							if (num2 != 0)
							{
								engine.jump(dword3);
							}
						}

						internal static void startLogicCommand(ScriptEngine engine)
						{
							uint word = engine.getWord();
							engine.startLogic(word);
						}

						internal static void stopLogicCommand(ScriptEngine engine)
						{
							uint word = engine.getWord();
							engine.stopLogic(word);
						}

						internal static void jumpIfLogicEnableCommand(ScriptEngine engine)
						{
							engine.getWord();
							engine.getDword();
						}

						internal static void jumpIfLogicDisableCommand(ScriptEngine engine)
						{
							engine.getWord();
							engine.getDword();
						}

						internal static int convertIDXWeaponSysToIcon(int idx)
						{
							SmallIcon.TYPE[] array = new SmallIcon.TYPE[19]
							{
								SmallIcon.TYPE.SI_ICON_KNUCKLE,
								SmallIcon.TYPE.SI_ITEM_KNIFE,
								SmallIcon.TYPE.SI_ITEM_SWORD,
								SmallIcon.TYPE.SI_ITEM_HAMMER,
								SmallIcon.TYPE.SI_ITEM_HAMMER,
								SmallIcon.TYPE.SI_ITEM_ROD,
								SmallIcon.TYPE.SI_ITEM_ROD,
								SmallIcon.TYPE.SI_ITEM_BOW,
								SmallIcon.TYPE.SI_ICON_ARROW,
								SmallIcon.TYPE.SI_ITEM_BOOK,
								SmallIcon.TYPE.SI_ICON_KNUCKLE,
								SmallIcon.TYPE.SI_ITEM_HAMMER,
								SmallIcon.TYPE.SI_ITEM_AXE,
								SmallIcon.TYPE.SI_ITEM_SPEAR,
								SmallIcon.TYPE.SI_ITEM_BOOMERANG,
								SmallIcon.TYPE.SI_ITEM_BELL,
								SmallIcon.TYPE.SI_ITEM_HARP,
								SmallIcon.TYPE.SI_ITEM_KATANA,
								SmallIcon.TYPE.SI_ITEM_SYURIKEN
							};
							return (int)array[idx];
						}

						internal static int convertIDXProtectionSysToIcon(int idx)
						{
							SmallIcon.TYPE[] array = new SmallIcon.TYPE[4]
							{
								SmallIcon.TYPE.SI_ITEM_SHIELD,
								SmallIcon.TYPE.SI_ITEM_HELMET,
								SmallIcon.TYPE.SI_ITEM_ARMOR,
								SmallIcon.TYPE.SI_ITEM_GAUNTLET
							};
							return (int)array[idx];
						}

						internal static int convertIDXMagicSysToIcon(int idx)
						{
							SmallIcon.TYPE[] array = new SmallIcon.TYPE[6]
							{
								SmallIcon.TYPE.SI_MAGIC_WHITE,
								SmallIcon.TYPE.SI_MAGIC_BLACK,
								SmallIcon.TYPE.SI_MAGIC_UNION,
								SmallIcon.TYPE.SI_ITEM_HARP,
								SmallIcon.TYPE.SI_ITEM_BELL,
								SmallIcon.TYPE.SI_MONSTER_UNDEAD
							};
							return (int)array[idx];
						}

						internal static bool checkCard()
						{
							bool result = true;
							byte[] array = new byte[4];
							card.Manager.GetInstance().LoadData(array, (uint)array.Length, 0u);
							if (card.Manager.GetInstance().GetResult() != card.RESULT.RESULT_SUCCESS)
							{
								result = false;
							}
							return result;
						}

						internal static void cardAcceccFailedSetting()
						{
							dgs.DGSMessageManager dGSMessageManager = dgs.msg.CMessageSys.getInstance().Sub();
							dgs.DGSMessage dGSMessage = dGSMessageManager.createMessage(50065u, dgs.INVALID_MSDHANDLE, 0);
							if (dGSMessage != null)
							{
								ds.Vector2<short> vector = new ds.Vector2<short>(0, 0);
								dGSMessage.getCompleteTextSize(vector);
								dGSMessage.setPosition((short)(128 - vector.vx / 2), (short)(96 - vector.vy / 2), erase: true);
								dGSMessage.setStyle(1152u);
								dGSMessage.setDisplaySpeed(byte.MaxValue);
								dGSMessage.setVSpace(4);
								dGSMessage.setVisibility(b: true);
								dGSMessage.setCanvas(0);
							}
						}

						internal static void fastestClearTime(out int h, out int m, out int s)
						{
							int num = 359999;
							ys.ParameterPoint<int> parameterPoint = pl.PlayerParty.instance().mania().clearTime();
							if (num < parameterPoint.get())
							{
								parameterPoint.set(num);
								OS_Printf("%d.\n", parameterPoint.get());
							}
							h = (int)ds.secondToHH((uint)parameterPoint.get());
							m = (int)ds.secondToMM((uint)parameterPoint.get());
							s = (int)ds.secondToSS((uint)parameterPoint.get());
						}

						internal static int clearNumber()
						{
							return pl.PlayerParty.instance().mania().clearNumber()
								.get();
						}

						internal static int treasureHuntRate()
						{
							return static_cast<int>(pl.PlayerParty.instance().mania().treasureHuntRate()
								.get());
						}

						internal static int enemyBreakNumber()
						{
							int num = 0;
							int num2 = 65535;
							int num3 = pl.PlayerParty.instance().mania().enemyBreakNumber()
								.get();
							if (num > num3)
							{
								num3 = num;
							}
							else if (num2 < num3)
							{
								num3 = num2;
							}
							return num3;
						}

						internal static int escapeNumber()
						{
							int num = 0;
							int num2 = 999;
							int num3 = pl.PlayerParty.instance().mania().escapeNumber()
								.get();
							if (num > num3)
							{
								num3 = num;
							}
							else if (num2 < num3)
							{
								num3 = num2;
							}
							return num3;
						}

						internal static int maxDamage()
						{
							int num = 0;
							int num2 = 99999;
							int num3 = pl.PlayerParty.instance().mania().maxDamage()
								.get();
							if (num > num3)
							{
								num3 = num;
							}
							else if (num2 < num3)
							{
								num3 = num2;
							}
							return num3;
						}

						internal static int maxHitNumber()
						{
							int num = 0;
							int num2 = 32;
							int num3 = pl.PlayerParty.instance().mania().maxHitNumber()
								.get();
							if (num > num3)
							{
								num3 = num;
							}
							else if (num2 < num3)
							{
								num3 = num2;
							}
							return num3;
						}

						internal static int masterCardNumber()
						{
							int num = 0;
							int num2 = 99;
							int num3 = 0;
							for (int i = 0; i < 64; i++)
							{
								short num4 = pl.PlayerParty.instance().item().importantItem(i)
									.itemId();
								if (num4 >= 5218 && num4 < 5241)
								{
									num3 += pl.PlayerParty.instance().item().importantItem(i)
										.itemNumber();
								}
							}
							if (num > num3)
							{
								num3 = num;
							}
							else if (num2 < num3)
							{
								num3 = num2;
							}
							return num3;
						}

						internal static void glAlphaFunc(uint func, float @ref)
						{
							AlphaTestEffect alphaTestEffect = m_Graphics.getAlphaTestEffect();
							switch (func)
							{
							case 516u:
								if (alphaTestEffect.AlphaFunction != CompareFunction.Greater)
								{
									m_bApplyEffect = true;
								}
								alphaTestEffect.AlphaFunction = CompareFunction.Greater;
								break;
							case 514u:
								if (alphaTestEffect.AlphaFunction != CompareFunction.Equal)
								{
									m_bApplyEffect = true;
								}
								alphaTestEffect.AlphaFunction = CompareFunction.Equal;
								break;
							}
							if (alphaTestEffect.ReferenceAlpha != (int)(255f * @ref))
							{
								m_bApplyEffect = true;
							}
							alphaTestEffect.ReferenceAlpha = (int)(255f * @ref);
						}

						internal static void glBindTexture(uint target, uint texture)
						{
							FF3.Log.Count(FF3.LogChannel.Gl, "glBindTexture"); /*FF3LOG*/
							m_uiBindTexture = texture;
						}

						internal static void glBlendFunc(uint sfactor, uint dfactor)
						{
							m_Blend = ((dfactor != 1) ? Blend.InverseSourceAlpha : Blend.One);
						}

						internal static void glClear(uint mask)
						{
							FF3.Log.Sample(FF3.LogChannel.Gl, "glClear", 120, () => $"mask={mask} color={m_ClearColor}"); /*FF3LOG*/
							GraphicsDevice graphicsDevice = m_Graphics.GetGraphicsDeviceManager().GraphicsDevice;
							graphicsDevice.Clear((ClearOptions)((((mask & 0x4000) != 0) ? 1 : 0) | (((mask & 0x100) != 0) ? 2 : 0) | (((mask & 0x400) != 0) ? 4 : 0)), m_ClearColor, m_fClearDepth, 0);
						}

						internal static void glClearColor(float red, float green, float blue, float alpha)
						{
							m_ClearColor = new Color(red, green, blue, alpha);
						}

						internal static void glColor4ub(byte red, byte green, byte blue, byte alpha)
						{
							m_abyCurrentColor[0] = red;
							m_abyCurrentColor[1] = green;
							m_abyCurrentColor[2] = blue;
							m_abyCurrentColor[3] = alpha;
						}

						internal static void glColorPointer(int size, uint type, int stride, Array pointer)
						{
							m_iColorSize = size;
							m_iColorStride = stride;
							m_abyColor = (byte[])pointer;
						}

						internal static void glCullFace(uint mode)
						{
							_ = m_Graphics.GetGraphicsDeviceManager().GraphicsDevice;
							switch (mode)
							{
							case 1028u:
								m_RasterizerState = RasterizerState.CullCounterClockwise;
								break;
							case 1029u:
								m_RasterizerState = RasterizerState.CullClockwise;
								break;
							case 1032u:
								m_RasterizerState = RasterizerState.CullNone;
								break;
							case 1030u:
							case 1031u:
								break;
							}
						}

						internal static void glDeleteTextures(int n, uint[] textures)
						{
							for (int i = 0; i < n; i++)
							{
								int num = (int)textures[i];
								if (m_aGlTexture[num] != null)
								{
									if (m_aGlTexture[num].m_Texture2D != null)
									{
										m_aGlTexture[num].m_Texture2D.Dispose();
									}
									m_aGlTexture[num] = null;
								}
							}
						}

						internal static void glDepthFunc(uint func)
						{
							switch (func)
							{
							case 513u:
								m_DepthFunc = CompareFunction.Less;
								break;
							case 514u:
								m_DepthFunc = CompareFunction.Equal;
								break;
							case 515u:
								m_DepthFunc = CompareFunction.LessEqual;
								break;
							case 516u:
								m_DepthFunc = CompareFunction.Greater;
								break;
							case 518u:
								m_DepthFunc = CompareFunction.GreaterEqual;
								break;
							case 519u:
								m_DepthFunc = CompareFunction.Always;
								break;
							case 517u:
								break;
							}
						}

						internal static void glDepthMask(byte flag)
						{
							m_bDepthMask = flag != 0;
						}

						internal static void glDisable(uint cap)
						{
							BasicEffect basicEffect = m_Graphics.getBasicEffect();
							AlphaTestEffect alphaTestEffect = m_Graphics.getAlphaTestEffect();
							switch (cap)
							{
							case 3553u:
								basicEffect.TextureEnabled = false;
								break;
							case 2929u:
								m_bDepthTest = false;
								break;
							case 2912u:
								if (basicEffect.FogEnabled)
								{
									m_bApplyEffect = true;
								}
								basicEffect.FogEnabled = false;
								alphaTestEffect.FogEnabled = false;
								break;
							case 3008u:
								if (m_bAlphaTest)
								{
									m_bApplyEffect = true;
								}
								m_bAlphaTest = false;
								break;
							case 2884u:
								m_bCullFace = false;
								break;
							}
						}

						internal static void glDisableClientState(uint array)
						{
							switch (array)
							{
							case 32884u:
								m_bVertexArray = false;
								break;
							case 32886u:
								m_bColorArray = false;
								break;
							case 32888u:
								m_bCoordArray = false;
								break;
							case 32885u:
							case 32887u:
								break;
							}
						}

						internal static void glDrawArrays(uint mode, int first, int count)
						{
							FF3.Log.First(FF3.LogChannel.Gl, "glDrawArrays3", 40, () => $"mode={mode} count={count} texEnabled={m_Graphics.getBasicEffect().TextureEnabled} bind={m_uiBindTexture}"); /*FF3LOG*/
							if (count <= 0)
							{
								return;
							}
							GraphicsDevice graphicsDevice = m_Graphics.GetGraphicsDeviceManager().GraphicsDevice;
							BasicEffect basicEffect = m_Graphics.getBasicEffect();
							bool flag = true;
							PrimitiveType primitiveType = PrimitiveType.TriangleList;
							int primitiveCount = 0;
							AlphaTestEffect alphaTestEffect = m_Graphics.getAlphaTestEffect();
							switch (mode)
							{
							case 2u:
							{
								primitiveType = PrimitiveType.LineStrip;
								primitiveCount = count;
								for (int k = 0; k < count; k++)
								{
									m_asIndex[k] = (short)k;
								}
								m_asIndex[count] = 0;
								break;
							}
							case 4u:
								primitiveCount = count / 3;
								flag = false;
								break;
							case 5u:
							{
								primitiveType = PrimitiveType.TriangleStrip;
								primitiveCount = count - 2;
								for (int j = 0; j < count; j++)
								{
									m_asIndex[j] = (short)j;
								}
								break;
							}
							case 6u:
							{
								primitiveType = PrimitiveType.TriangleList;
								primitiveCount = count - 2;
								int i = 2;
								int num = 0;
								for (; i < count; i++)
								{
									m_asIndex[num++] = 0;
									m_asIndex[num++] = (short)(i - 1);
									m_asIndex[num++] = (short)i;
								}
								break;
							}
							default:
								OS_Terminate();
								break;
							}
							// PORT: the original only bound the texture when the texture INDEX changed, and
							// only re-applied the effect when a state flag was set. Neither notices the draw
							// switching between BasicEffect and AlphaTestEffect. The newly chosen effect then
							// runs with whatever texture it happened to be holding - usually none - and a
							// textureless AlphaTestEffect samples alpha 0, so every fragment is discarded and
							// the whole 3D scene renders black. Track the chosen effect and keep it in sync.
							if (FF3.RenderOverrides.NoTextures && basicEffect.TextureEnabled)
							{
								basicEffect.TextureEnabled = false;
								m_bApplyEffect = true;
							}
							bool _useAlphaTest = FF3.RenderOverrides.AlphaTest(m_bAlphaTest)
								&& basicEffect.TextureEnabled && m_aGlTexture[m_uiBindTexture] != null;
							Effect _effect = _useAlphaTest ? (Effect)alphaTestEffect : (Effect)basicEffect;
							if (!object.ReferenceEquals(_effect, FF3.RenderOverrides.LastEffect))
							{
								FF3.RenderOverrides.LastEffect = _effect;
								m_uiApplyTexture = uint.MaxValue;
								m_bApplyEffect = true;
							}
							if (basicEffect.TextureEnabled && m_aGlTexture[m_uiBindTexture] != null)
							{
								if (m_uiApplyTexture != m_uiBindTexture)
								{
									if (_useAlphaTest)
									{
										alphaTestEffect.Texture = m_aGlTexture[m_uiBindTexture].m_Texture2D;
									}
									else
									{
										basicEffect.Texture = m_aGlTexture[m_uiBindTexture].m_Texture2D;
									}
									m_uiApplyTexture = m_uiBindTexture;
									m_bApplyEffect = true;
								}
							}
							else if (m_uiApplyTexture != 0)
							{
								basicEffect.Texture = null;
								m_uiApplyTexture = 0u;
								m_bApplyEffect = true;
							}
							RasterizerState _rs = FF3.RenderOverrides.Rasterizer(m_bCullFace ? m_RasterizerState : RasterizerState.CullNone); /*FF3LOG*/
							if (graphicsDevice.RasterizerState != _rs)
							{
								m_bApplyEffect = true;
							}
							graphicsDevice.RasterizerState = _rs;
							VertexPositionColorTexture[] array = new VertexPositionColorTexture[count];
							float[] array2 = new float[3];
							float[] array3 = new float[2];
							byte[] array4 = new byte[4];
							for (int l = 0; l < count; l++)
							{
								if (m_bVertexArray && m_afVertex != null)
								{
									int num2 = m_iVertexStride + l * m_iVertexSize;
									for (int m = 0; m < m_iVertexSize; m++)
									{
										array2[m] = m_afVertex[num2 + m];
									}
								}
								else
								{
									for (int n = 0; n < m_iVertexSize; n++)
									{
										array2[n] = 0f;
									}
								}
								if (m_bCoordArray && m_afCoord != null && basicEffect.TextureEnabled && m_aGlTexture[m_uiBindTexture] != null)
								{
									int num3 = m_iCoordStride + l * m_iCoordSize;
									for (int num4 = 0; num4 < m_iCoordSize; num4++)
									{
										array3[num4] = m_afCoord[num3 + num4];
									}
								}
								else
								{
									for (int num5 = 0; num5 < m_iCoordSize; num5++)
									{
										array3[num5] = 0f;
									}
								}
								if (m_bColorArray && m_abyColor != null)
								{
									int num6 = m_iColorStride + l * m_iColorSize;
									for (int num7 = 0; num7 < m_iColorSize; num7++)
									{
										array4[num7] = m_abyColor[num6 + num7];
									}
								}
								else
								{
									for (int num8 = 0; num8 < m_iColorSize; num8++)
									{
										array4[num8] = m_abyCurrentColor[num8];
									}
								}
								array[l].Position.X = array2[0];
								array[l].Position.Y = array2[1];
								array[l].Position.Z = array2[2];
								array[l].TextureCoordinate.X = array3[0];
								array[l].TextureCoordinate.Y = array3[1];
								array[l].Color.R = array4[0];
								array[l].Color.G = array4[1];
								array[l].Color.B = array4[2];
								array[l].Color.A = array4[3];
							}
							BlendState blendState = graphicsDevice.BlendState;
							if (blendState.ColorSourceBlend != Blend.SourceAlpha || blendState.AlphaDestinationBlend != m_Blend || blendState.ColorWriteChannels != ColorWriteChannels.All)
							{
								blendState = new BlendState();
								blendState.ColorSourceBlend = Blend.SourceAlpha;
								blendState.AlphaSourceBlend = Blend.SourceAlpha;
								blendState.ColorDestinationBlend = m_Blend;
								blendState.AlphaDestinationBlend = m_Blend;
								blendState.ColorWriteChannels = ColorWriteChannels.All;
								blendState.ColorWriteChannels1 = ColorWriteChannels.None;
								blendState.ColorWriteChannels2 = ColorWriteChannels.None;
								blendState.ColorWriteChannels3 = ColorWriteChannels.None;
								graphicsDevice.BlendState = blendState;
								m_bApplyEffect = true;
							}
							if (basicEffect.TextureEnabled && m_aGlTexture[m_uiBindTexture] != null)
							{
								SamplerState samplerState = graphicsDevice.SamplerStates[0];
								if (samplerState.AddressU != m_aGlTexture[m_uiBindTexture].m_TextureAddressModeS || samplerState.AddressV != m_aGlTexture[m_uiBindTexture].m_TextureAddressModeT || samplerState.Filter != m_aGlTexture[m_uiBindTexture].m_TextureFilter)
								{
									samplerState = new SamplerState();
									samplerState.AddressU = m_aGlTexture[m_uiBindTexture].m_TextureAddressModeS;
									samplerState.AddressV = m_aGlTexture[m_uiBindTexture].m_TextureAddressModeT;
									samplerState.Filter = m_aGlTexture[m_uiBindTexture].m_TextureFilter;
									graphicsDevice.SamplerStates[0] = samplerState;
									m_bApplyEffect = true;
								}
							}
							DepthStencilState depthStencilState = graphicsDevice.DepthStencilState;
							if (depthStencilState.DepthBufferEnable != m_bDepthTest || depthStencilState.DepthBufferWriteEnable != m_bDepthMask || depthStencilState.DepthBufferFunction != m_DepthFunc)
							{
								graphicsDevice.DepthStencilState = FF3.RenderOverrides.DepthState(m_bDepthTest, m_bDepthMask, m_DepthFunc); /*FF3LOG*/
								m_bApplyEffect = true;
							}
							if (m_bApplyEffect)
							{
								_effect.CurrentTechnique.Passes[0].Apply();
							}
							m_bApplyEffect = false;
							if (flag)
							{
								graphicsDevice.DrawUserIndexedPrimitives(primitiveType, array, 0, count, m_asIndex, 0, primitiveCount);
							}
							else
							{
								graphicsDevice.DrawUserPrimitives(primitiveType, array, first, primitiveCount);
							}
						}

						internal static void glDrawArrays(uint mode, int first, int count, VertexPositionColorTexture[] v)
						{
							FF3.Log.Sample(FF3.LogChannel.Gl, "draw", FF3.GlDiag.Burst() ? 1 : 60, () => { var _be = m_Graphics.getBasicEffect(); var _gd = m_Graphics.GetGraphicsDeviceManager().GraphicsDevice; return $"mode={mode} first={first} count={count} tex={(_be.TextureEnabled ? (m_aGlTexture[m_uiBindTexture]?.m_Texture2D == null ? "BOUND-NULL" : m_aGlTexture[m_uiBindTexture].m_Texture2D.Width + "x" + m_aGlTexture[m_uiBindTexture].m_Texture2D.Height) : "off")} " + $"alphaTest={m_bAlphaTest} apply={m_bApplyEffect} depth={m_bDepthTest}/{m_bDepthMask} cull={(m_bCullFace ? m_RasterizerState.CullMode.ToString() : "none")} blendDst={m_Blend} " + $"v0=({v[first].Position.X:F1},{v[first].Position.Y:F1},{v[first].Position.Z:F1}) v1=({v[first+1].Position.X:F1},{v[first+1].Position.Y:F1},{v[first+1].Position.Z:F1}) col0={v[first].Color} " + $"W=[{_be.World.M11:F3} {_be.World.M22:F3} {_be.World.M33:F3} | {_be.World.M41:F1} {_be.World.M42:F1} {_be.World.M43:F1}] " + $"P=[{_be.Projection.M11:F4} {_be.Projection.M22:F4} {_be.Projection.M33:F4} {_be.Projection.M34:F4} | {_be.Projection.M43:F3} {_be.Projection.M44:F3}] " + $"vp={_gd.Viewport.Width}x{_gd.Viewport.Height} " + FF3.GlDiag.Describe(v, first, count, _be.World, _be.View, _be.Projection); }); /*FF3LOG*/
							if (count <= 0)
							{
								return;
							}
							GraphicsDevice graphicsDevice = m_Graphics.GetGraphicsDeviceManager().GraphicsDevice;
							BasicEffect basicEffect = m_Graphics.getBasicEffect();
							bool flag = true;
							PrimitiveType primitiveType = PrimitiveType.TriangleList;
							int primitiveCount = 0;
							AlphaTestEffect alphaTestEffect = m_Graphics.getAlphaTestEffect();
							switch (mode)
							{
							case 2u:
							{
								primitiveType = PrimitiveType.LineStrip;
								primitiveCount = count;
								for (int k = 0; k < count; k++)
								{
									m_asIndex[k] = (short)k;
								}
								m_asIndex[count] = 0;
								break;
							}
							case 4u:
								primitiveCount = count / 3;
								flag = false;
								break;
							case 5u:
							{
								primitiveType = PrimitiveType.TriangleStrip;
								primitiveCount = count - 2;
								for (int j = 0; j < count; j++)
								{
									m_asIndex[j] = (short)j;
								}
								break;
							}
							case 6u:
							{
								primitiveType = PrimitiveType.TriangleList;
								primitiveCount = count - 2;
								int i = 2;
								int num = 0;
								for (; i < count; i++)
								{
									m_asIndex[num++] = 0;
									m_asIndex[num++] = (short)(i - 1);
									m_asIndex[num++] = (short)i;
								}
								break;
							}
							default:
								OS_Terminate();
								break;
							}
							// PORT: the original only bound the texture when the texture INDEX changed, and
							// only re-applied the effect when a state flag was set. Neither notices the draw
							// switching between BasicEffect and AlphaTestEffect. The newly chosen effect then
							// runs with whatever texture it happened to be holding - usually none - and a
							// textureless AlphaTestEffect samples alpha 0, so every fragment is discarded and
							// the whole 3D scene renders black. Track the chosen effect and keep it in sync.
							FF3.ModelCapture.Offer(v, first, count, m_uiBindTexture < m_aGlTexture.Length && m_aGlTexture[m_uiBindTexture] != null ? m_aGlTexture[m_uiBindTexture].m_Texture2D : null, basicEffect.Projection); /*FF3LOG*/
							if (FF3.RenderOverrides.NoTextures && basicEffect.TextureEnabled)
							{
								basicEffect.TextureEnabled = false;
								m_bApplyEffect = true;
							}
							bool _useAlphaTest = FF3.RenderOverrides.AlphaTest(m_bAlphaTest)
								&& basicEffect.TextureEnabled && m_aGlTexture[m_uiBindTexture] != null;
							Effect _effect = _useAlphaTest ? (Effect)alphaTestEffect : (Effect)basicEffect;
							if (!object.ReferenceEquals(_effect, FF3.RenderOverrides.LastEffect))
							{
								FF3.RenderOverrides.LastEffect = _effect;
								m_uiApplyTexture = uint.MaxValue;
								m_bApplyEffect = true;
							}
							if (basicEffect.TextureEnabled && m_aGlTexture[m_uiBindTexture] != null)
							{
								if (m_uiApplyTexture != m_uiBindTexture)
								{
									if (_useAlphaTest)
									{
										alphaTestEffect.Texture = m_aGlTexture[m_uiBindTexture].m_Texture2D;
									}
									else
									{
										basicEffect.Texture = m_aGlTexture[m_uiBindTexture].m_Texture2D;
									}
									m_uiApplyTexture = m_uiBindTexture;
									m_bApplyEffect = true;
								}
							}
							else if (m_uiApplyTexture != 0)
							{
								basicEffect.Texture = null;
								m_uiApplyTexture = 0u;
								m_bApplyEffect = true;
							}
							RasterizerState _rs = FF3.RenderOverrides.Rasterizer(m_bCullFace ? m_RasterizerState : RasterizerState.CullNone); /*FF3LOG*/
							if (graphicsDevice.RasterizerState != _rs)
							{
								m_bApplyEffect = true;
							}
							graphicsDevice.RasterizerState = _rs;
							BlendState blendState = graphicsDevice.BlendState;
							if (blendState.ColorSourceBlend != Blend.SourceAlpha || blendState.AlphaDestinationBlend != m_Blend || blendState.ColorWriteChannels != ColorWriteChannels.All)
							{
								blendState = new BlendState();
								blendState.ColorSourceBlend = Blend.SourceAlpha;
								blendState.AlphaSourceBlend = Blend.SourceAlpha;
								blendState.ColorDestinationBlend = m_Blend;
								blendState.AlphaDestinationBlend = m_Blend;
								blendState.ColorWriteChannels = ColorWriteChannels.All;
								blendState.ColorWriteChannels1 = ColorWriteChannels.None;
								blendState.ColorWriteChannels2 = ColorWriteChannels.None;
								blendState.ColorWriteChannels3 = ColorWriteChannels.None;
								graphicsDevice.BlendState = blendState;
								m_bApplyEffect = true;
							}
							if (basicEffect.TextureEnabled && m_aGlTexture[m_uiBindTexture] != null)
							{
								SamplerState samplerState = graphicsDevice.SamplerStates[0];
								if (samplerState.AddressU != m_aGlTexture[m_uiBindTexture].m_TextureAddressModeS || samplerState.AddressV != m_aGlTexture[m_uiBindTexture].m_TextureAddressModeT || samplerState.Filter != m_aGlTexture[m_uiBindTexture].m_TextureFilter)
								{
									samplerState = new SamplerState();
									samplerState.AddressU = m_aGlTexture[m_uiBindTexture].m_TextureAddressModeS;
									samplerState.AddressV = m_aGlTexture[m_uiBindTexture].m_TextureAddressModeT;
									samplerState.Filter = m_aGlTexture[m_uiBindTexture].m_TextureFilter;
									graphicsDevice.SamplerStates[0] = samplerState;
									m_bApplyEffect = true;
								}
							}
							DepthStencilState depthStencilState = graphicsDevice.DepthStencilState;
							if (depthStencilState.DepthBufferEnable != m_bDepthTest || depthStencilState.DepthBufferWriteEnable != m_bDepthMask || depthStencilState.DepthBufferFunction != m_DepthFunc)
							{
								graphicsDevice.DepthStencilState = FF3.RenderOverrides.DepthState(m_bDepthTest, m_bDepthMask, m_DepthFunc); /*FF3LOG*/
								m_bApplyEffect = true;
							}
							if (m_bApplyEffect)
							{
								_effect.CurrentTechnique.Passes[0].Apply();
							}
							m_bApplyEffect = false;
							if (flag)
							{
								graphicsDevice.DrawUserIndexedPrimitives(primitiveType, v, 0, count, m_asIndex, 0, primitiveCount);
							}
							else
							{
								graphicsDevice.DrawUserPrimitives(primitiveType, v, first, primitiveCount);
							}
						}

						internal static void glEnable(uint cap)
						{
							FF3.Log.First(FF3.LogChannel.Gl, "glEnable", 24, () => $"cap={cap}"); /*FF3LOG*/
							BasicEffect basicEffect = m_Graphics.getBasicEffect();
							AlphaTestEffect alphaTestEffect = m_Graphics.getAlphaTestEffect();
							switch (cap)
							{
							case 3553u:
								basicEffect.TextureEnabled = true;
								break;
							case 2929u:
								m_bDepthTest = true;
								break;
							case 2912u:
								if (!basicEffect.FogEnabled)
								{
									m_bApplyEffect = true;
								}
								basicEffect.FogEnabled = true;
								alphaTestEffect.FogEnabled = true;
								break;
							case 3008u:
								if (!m_bAlphaTest)
								{
									m_bApplyEffect = true;
								}
								m_bAlphaTest = true;
								break;
							case 2884u:
								m_bCullFace = true;
								break;
							}
						}

						internal static void glEnableClientState(uint array)
						{
							switch (array)
							{
							case 32884u:
								m_bVertexArray = true;
								break;
							case 32886u:
								m_bColorArray = true;
								break;
							case 32888u:
								m_bCoordArray = true;
								break;
							case 32885u:
							case 32887u:
								break;
							}
						}

						internal static void glGenTextures(int n, uint[] textures)
						{
							for (int i = 0; i < n; i++)
							{
								for (int j = 1; j < m_aGlTexture.Length; j++)
								{
									if (m_aGlTexture[j] == null)
									{
										m_aGlTexture[j] = new GlTexture();
										textures[i] = (uint)j;
										break;
									}
								}
							}
						}

						internal static void glLightfv(uint light, uint pname, float[] @params)
						{
						}

						internal static void glLoadIdentity()
						{
							BasicEffect basicEffect = m_Graphics.getBasicEffect();
							AlphaTestEffect alphaTestEffect = m_Graphics.getAlphaTestEffect();
							switch (m_eMatrixMode)
							{
							case 5890u:
								m_TextureMatrix = Matrix.Identity;
								break;
							case 5888u:
								basicEffect.World = Matrix.Identity;
								alphaTestEffect.World = basicEffect.World;
								m_bApplyEffect = true;
								break;
							case 5889u:
								basicEffect.Projection = Matrix.Identity;
								alphaTestEffect.Projection = basicEffect.Projection;
								m_bApplyEffect = true;
								break;
							}
						}

						internal static void glLoadMatrixf(float[] m)
						{
							BasicEffect basicEffect = m_Graphics.getBasicEffect();
							Matrix matrix = default(Matrix);
							AlphaTestEffect alphaTestEffect = m_Graphics.getAlphaTestEffect();
							matrix.M11 = m[0];
							matrix.M12 = m[1];
							matrix.M13 = m[2];
							matrix.M14 = m[3];
							matrix.M21 = m[4];
							matrix.M22 = m[5];
							matrix.M23 = m[6];
							matrix.M24 = m[7];
							matrix.M31 = m[8];
							matrix.M32 = m[9];
							matrix.M33 = m[10];
							matrix.M34 = m[11];
							matrix.M41 = m[12];
							matrix.M42 = m[13];
							matrix.M43 = m[14];
							matrix.M44 = m[15];
							switch (m_eMatrixMode)
							{
							case 5890u:
								m_TextureMatrix = matrix;
								break;
							case 5888u:
								basicEffect.World = matrix;
								alphaTestEffect.World = basicEffect.World;
								m_bApplyEffect = true;
								break;
							case 5889u:
								basicEffect.Projection = matrix;
								alphaTestEffect.Projection = basicEffect.Projection;
								m_bApplyEffect = true;
								break;
							}
						}

						internal static void glMatrixMode(uint mode)
						{
							m_eMatrixMode = mode;
						}

						internal static void glMultMatrixf(float[] m)
						{
							BasicEffect basicEffect = m_Graphics.getBasicEffect();
							Matrix matrix = new Matrix(m[0], m[1], m[2], m[3], m[4], m[5], m[6], m[7], m[8], m[9], m[10], m[11], m[12], m[13], m[14], m[15]);
							AlphaTestEffect alphaTestEffect = m_Graphics.getAlphaTestEffect();
							switch (m_eMatrixMode)
							{
							case 5890u:
								m_TextureMatrix = Matrix.Multiply(matrix, m_TextureMatrix);
								break;
							case 5888u:
								basicEffect.World = Matrix.Multiply(matrix, basicEffect.World);
								alphaTestEffect.World = basicEffect.World;
								m_bApplyEffect = true;
								break;
							case 5889u:
								basicEffect.Projection = Matrix.Multiply(matrix, basicEffect.Projection);
								alphaTestEffect.Projection = basicEffect.Projection;
								m_bApplyEffect = true;
								break;
							}
						}

						internal static void glOrthof(float left, float right, float bottom, float top, float zNear, float zFar)
						{
							FF3.Log.First(FF3.LogChannel.Gl, "glOrthof", 8, () => $"l={left} r={right} b={bottom} t={top}"); /*FF3LOG*/
							float num = 1f / (right - left);
							float num2 = right + left;
							float num3 = 1f / (top - bottom);
							float num4 = top + bottom;
							float num5 = 1f / (zFar - zNear);
							float num6 = zFar + zNear;
							m_afOrthoMatrix[0] = 2f * num;
							m_afOrthoMatrix[1] = 0f;
							m_afOrthoMatrix[2] = 0f;
							m_afOrthoMatrix[3] = 0f;
							m_afOrthoMatrix[4] = 0f;
							m_afOrthoMatrix[5] = 2f * num3;
							m_afOrthoMatrix[6] = 0f;
							m_afOrthoMatrix[7] = 0f;
							m_afOrthoMatrix[8] = 0f;
							m_afOrthoMatrix[9] = 0f;
							m_afOrthoMatrix[10] = -2f * num5;
							m_afOrthoMatrix[11] = 0f;
							m_afOrthoMatrix[12] = -1f * num2 * num;
							m_afOrthoMatrix[13] = -1f * num4 * num3;
							m_afOrthoMatrix[14] = -1f * num6 * num5;
							m_afOrthoMatrix[15] = 1f;
							glMultMatrixf(m_afOrthoMatrix);
						}

						internal static void glPopMatrix()
						{
							BasicEffect basicEffect = m_Graphics.getBasicEffect();
							AlphaTestEffect alphaTestEffect = m_Graphics.getAlphaTestEffect();
							switch (m_eMatrixMode)
							{
							case 5890u:
								m_TextureMatrix = m_MatrixStack.Pop();
								break;
							case 5888u:
								basicEffect.World = m_MatrixStack.Pop();
								alphaTestEffect.World = basicEffect.World;
								m_bApplyEffect = true;
								break;
							case 5889u:
								basicEffect.Projection = m_MatrixStack.Pop();
								alphaTestEffect.Projection = basicEffect.Projection;
								m_bApplyEffect = true;
								break;
							}
						}

						internal static void glPushMatrix()
						{
							BasicEffect basicEffect = m_Graphics.getBasicEffect();
							switch (m_eMatrixMode)
							{
							case 5890u:
								m_MatrixStack.Push(m_TextureMatrix);
								break;
							case 5888u:
								m_MatrixStack.Push(basicEffect.World);
								break;
							case 5889u:
								m_MatrixStack.Push(basicEffect.Projection);
								break;
							}
						}

						internal static void glTexCoordPointer(int size, uint type, int stride, Array pointer)
						{
							m_iCoordSize = size;
							m_iCoordStride = stride;
							m_afCoord = (float[])pointer;
						}

						internal static void glTexImage2D(uint target, int level, int internalformat, int width, int height, int border, uint format, uint type, Array pixels)
						{
							FF3.Log.First(FF3.LogChannel.Gl, "glTexImage2D", 60, () => $"{width}x{height} fmt={format} type={type} bind={m_uiBindTexture} slot={(m_uiBindTexture < m_aGlTexture.Length && m_aGlTexture[m_uiBindTexture] != null ? "ok" : "NULL")} src={(pixels == null ? "null" : pixels.GetType().Name + "[" + pixels.Length + "]")}"); /*FF3LOG*/
							if (width == 0 || height == 0)
							{
								return;
							}
							GraphicsDevice graphicsDevice = m_Graphics.GetGraphicsDeviceManager().GraphicsDevice;
							try
							{
								ArrayReader arrayReader = new ArrayReader(pixels);
								uint[] array = new uint[width * height];
								arrayReader.read(array, 0, array.Length);
								m_aGlTexture[m_uiBindTexture].m_Texture2D = new Texture2D(graphicsDevice, width, height);
								m_aGlTexture[m_uiBindTexture].m_Texture2D.SetData(array);
							}
							catch (Exception)
							{
							}
						}

						internal static void glTexParameteri(uint target, uint pname, int param)
						{
							switch (pname)
							{
							case 10240u:
								switch (param)
								{
								case 9728:
									m_aGlTexture[m_uiBindTexture].m_TextureFilter = TextureFilter.Point;
									break;
								case 9729:
									m_aGlTexture[m_uiBindTexture].m_TextureFilter = TextureFilter.Linear;
									break;
								}
								break;
							case 10242u:
								switch (param)
								{
								case 10497:
									m_aGlTexture[m_uiBindTexture].m_TextureAddressModeS = TextureAddressMode.Wrap;
									break;
								case 33071:
									m_aGlTexture[m_uiBindTexture].m_TextureAddressModeS = TextureAddressMode.Clamp;
									break;
								}
								break;
							case 10243u:
								switch (param)
								{
								case 10497:
									m_aGlTexture[m_uiBindTexture].m_TextureAddressModeT = TextureAddressMode.Wrap;
									break;
								case 33071:
									m_aGlTexture[m_uiBindTexture].m_TextureAddressModeT = TextureAddressMode.Clamp;
									break;
								}
								break;
							case 10241u:
								break;
							}
						}

						internal static void glTranslatef(float x, float y, float z)
						{
						}

						internal static void glVertexPointer(int size, uint type, int stride, Array pointer)
						{
							m_iVertexSize = size;
							m_iVertexStride = stride;
							m_afVertex = (float[])pointer;
						}

						internal static void glViewport(int x, int y, int width, int height)
						{
							FF3.Log.First(FF3.LogChannel.Gl, "glViewport", 8, () => $"{x},{y} {width}x{height}"); /*FF3LOG*/
							GraphicsDevice graphicsDevice = m_Graphics.GetGraphicsDeviceManager().GraphicsDevice;
							Viewport viewport = graphicsDevice.Viewport;
							viewport.X = x;
							viewport.Y = y;
							viewport.Width = width;
							viewport.Height = height;
							graphicsDevice.Viewport = viewport;
						}

						public static void setBuildTimeStamp()
						{
							__DATE__ = "2013/12/16";
							__TIME__ = "11:04:21";
						}

						public static void setMessage(int iId, string strMessage)
						{
							m_astrMessage[iId] = strMessage;
						}

						public static void drawMessage()
						{
							m_Graphics.SetColor(255, 255, 255);
							m_Graphics.SetImageOrigin(0f, 0f);
							m_Graphics.SetImageRotation(0f);
							float x = 0f;
							float num = 0f;
							for (int i = 0; i < 16; i++)
							{
								if (m_astrMessage[i] != null)
								{
									m_Graphics.DrawString(m_astrMessage[i], x, num, 16);
								}
								num += 24f;
							}
						}

						public static void resetTimer(int iId)
						{
							m_aTestTimer[iId].m_iCount = 0;
							m_aTestTimer[iId].m_lDiff = 0L;
						}

						public static void startTimer(int iId)
						{
							m_aTestTimer[iId].m_lStart = JavaSystem.currentTimeMillis();
						}

						public static void stopTimer(int iId, bool bAppend)
						{
							long num = JavaSystem.currentTimeMillis() - m_aTestTimer[iId].m_lStart;
							if (bAppend)
							{
								m_aTestTimer[iId].m_iCount++;
								m_aTestTimer[iId].m_lDiff += num;
							}
							else
							{
								m_aTestTimer[iId].m_lDiff = num;
							}
						}

						public static void setResourceCulture(int iLanguage)
						{
							string[] array = new string[9] { "ja-JP", "en", "fr-FR", "de-DE", "it-IT", "es-ES", "zh-CN", "zh-TW", "ko-KR" };
							CultureInfo culture = (language.Culture = new CultureInfo(array[iLanguage]));
							strings.Culture = culture;
						}

						static GlobalScope()
						{
							bool[] array = new bool[2];
							soundMute = array;
							owner_info = new OSOwnerInfo();
							sScreenBuf = new ushort[24, 32];
							LIMIT_OF_FONT = 4;
							dgsmFontVector = new ds.Vector<NNSG2dFont, ds.FastErasePolicy<NNSG2dFont>>(LIMIT_OF_FONT);
							init = false;
							g_BGMInfoTable = new BGMInfo[64]
							{
								new BGMInfo(100, 0),
								new BGMInfo(101, 1),
								new BGMInfo(102, 2),
								new BGMInfo(103, 3),
								new BGMInfo(104, 4),
								new BGMInfo(105, 5),
								new BGMInfo(106, 6),
								new BGMInfo(107, 7),
								new BGMInfo(108, 8),
								new BGMInfo(109, 9),
								new BGMInfo(110, 10),
								new BGMInfo(111, 11),
								new BGMInfo(112, 12),
								new BGMInfo(113, 13),
								new BGMInfo(114, 14),
								new BGMInfo(115, 15),
								new BGMInfo(116, 16),
								new BGMInfo(117, 17),
								new BGMInfo(118, 18),
								new BGMInfo(119, 19),
								new BGMInfo(120, 20),
								new BGMInfo(121, 21),
								new BGMInfo(122, 22),
								new BGMInfo(123, 23),
								new BGMInfo(124, 24),
								new BGMInfo(125, 25),
								new BGMInfo(126, 26),
								new BGMInfo(127, 27),
								new BGMInfo(128, 28),
								new BGMInfo(129, 29),
								new BGMInfo(130, 30),
								new BGMInfo(131, 31),
								new BGMInfo(132, 32),
								new BGMInfo(133, 33),
								new BGMInfo(134, 34),
								new BGMInfo(135, 35),
								new BGMInfo(136, 36),
								new BGMInfo(137, 37),
								new BGMInfo(138, 38),
								new BGMInfo(139, 39),
								new BGMInfo(140, 40),
								new BGMInfo(141, 41),
								new BGMInfo(142, 42),
								new BGMInfo(143, 43),
								new BGMInfo(144, 44),
								new BGMInfo(145, 45),
								new BGMInfo(146, 46),
								new BGMInfo(147, 47),
								new BGMInfo(148, 48),
								new BGMInfo(149, 49),
								new BGMInfo(150, 50),
								new BGMInfo(151, 51),
								new BGMInfo(152, 52),
								new BGMInfo(153, 53),
								new BGMInfo(154, 54),
								new BGMInfo(155, 55),
								new BGMInfo(156, 56),
								new BGMInfo(157, 57),
								new BGMInfo(158, 58),
								new BGMInfo(159, 59),
								new BGMInfo(),
								new BGMInfo(),
								new BGMInfo(),
								new BGMInfo()
							};
							g_SEInfoTable = new SEInfo[4]
							{
								new SEInfo(0, 0),
								new SEInfo(0, 0),
								new SEInfo(0, 0),
								new SEInfo(0, 0)
							};
							PROGRESS_SPEED = 256;
							FX32_16_SCALE = 1048576;
							EFFECT_AREA_HEIGHT = 589824;
							DIV_STEPS = 8;
							DIV_PROGRESS_SPEED = 512;
							GAP_LIMIT = 64;
							GAP_PER_FRAME = 2;
							g_Targets = new ds.Vector<ICTARGET, ds.FastErasePolicy<ICTARGET>>(8);
							idList = new ds.Vector<int, ds.FastErasePolicy<int>>(12);
							characterMng = new CCharacterMng();
							sceneMng = new CSceneMng();
							stageMng = new stg.CStageMng();
							ReplaceTextureCode = "NRTP";
							commandTable = new SCRIPT_COMMAND[298]
							{
								NOPCommand, NOPCommand, NOPCommand, NOPCommand, NOPCommand, endCommand, flagOnEndCommand, flagOffEndCommand, waitCommand, waitFlagOnCommand,
								waitFlagOffCommand, jumpCommand, flagOnJumpCommand, flagOffJumpCommand, callCommand, flagOnCallCommand, flagOffCallCommand, returnCommand, flagOnReturnCommand, flagOffReturnCommand,
								flagOnCommand, flagOffCommand, flagReverseCommand, setValueCommand, incValueCommand, decValueCommand, addValueCommand, mulValueCommand, divValueCommand, ifValueJumpCommand,
								startLogicCommand, stopLogicCommand, jumpIfLogicEnableCommand, jumpIfLogicDisableCommand, ff3Command_EventStart, ff3Command_EventEnd, ff3Command_FadeIn, ff3Command_FadeOut, ff3Command_DisplayMaskOn, ff3Command_DisplayMaskOff,
								ff3Command_BootCharacter, ff3Command_BootCharacter_AbsoluteCoordination, ff3Command_BootCharacter_RelativeCoordination, ff3Command_DeleteCharacter, ff3Command_DisplayCharacter, ff3Command_GravityCharacter, ff3Command_StartMotionCharacter, ff3Command_EndMotionCharacter, ff3Command_MoveCharacter_AbsoluteCoordination, ff3Command_MoveCharacter_RelativeCoordination,
								ff3Command_MoveCharacter_EndAutoIdle, ff3Command_MoveCharacter_StartRandom, ff3Command_MoveCharacter_EndRandom, ff3Command_TurnCharacter_AbsoluteAngle, ff3Command_TurnCharacter_RelativeAngle, ff3Command_TurnCharacter_AbsoluteCoordination, ff3Command_TurnCharacter_LookCharacter, ff3Command_TurnCharacter_Init, ff3Command_TurnCharacter_StartLoop, ff3Command_TurnCharacter_EndLoop,
								ff3Command_TurnCharacter_EndAutoIdle, ff3Command_SetNextMessageAlignment, ff3Command_EndMessage, ff3Command_ChangeFontSize, ff3Command_CheckPlayer_State, ff3Command_AlterPlayer_State, ff3Command_BootEventBattle, ff3Command_MapWarp, ff3Command_AddItem, ff3Command_SetTreasureItem,
								ff3Command_SetRecovery, ff3Command_BootShop, ff3Command_MoveCamera_AbsoluteCoordination, ff3Command_MoveCamera_RelativeCoordination, ff3Command_MoveCamera_LookPlayer, ff3Command_BootMovei, ff3Command_WithInCharacterJump, ff3Command_WithOutCharacterJump, ff3Command_BootEffect_AbsoluteCoordination, ff3Command_BootEffect_RelativeCoordination_Foolow,
								ff3Command_DeleteEffect, ff3Command_PauseEffect, ff3Command_SetEffect_Scale, ff3Command_Spring, ff3Command_FadeEndWait, ff3Command_SetCamera_AbsoluteGaze, ff3Command_SetCamera_RelativeGaze, ff3Command_ChangeCamera_Mode, ff3Command_SetCamera_Type, ff3Command_SetCamera_Angle,
								ff3Command_SetCharacter_Scale, ff3Command_PlayBGM, ff3Command_StopBGM, ff3Command_PlaySE, ff3Command_StartCamera_Vibration, ff3Command_StartMessageWindow, ff3Command_DeleteMessageWindow, ff3Command_EndMessageWindow, ff3Command_WaitEndOfMessageJump, ff3Command_DeleteNameWindow,
								ff3Command_StartMessage2, ff3Command_DeleteMessage2, ff3Command_SetTreasureMoney, ff3Command_SetNPCAiType, ff3Command_AddPartyPC, ff3Command_SubPartyPC, ff3Command_AddPartyNPC, ff3Command_SubPartyNPC, ff3Command_ChangePartyPC, ff3Command_CheckPartyAverageLevel,
								ff3Command_CheckPartyMemberNum, ff3Command_CheckPartyMemberRow, ff3Command_PartyMemberFine, ff3Command_BootInn, ff3Command_SetCharacter_CheckTurnType, ff3Command_SubItem, ff3Command_UseItem_FlagOnJump, ff3Command_UseItem_FlagOffJump, ff3Command_CheckUseItem_Id, ff3Command_CheckItem_Num,
								ff3Command_TouchOnJump, ff3Command_TouchOffJump, ff3Command_ButtonOnJump, ff3Command_ButtonOffJump, ff3Command_SelectEndWait, ff3Command_SetCharacter_TalkMotion, ff3Command_MoveCamera_LookPlayer2, ff3Command_SetCamera_PositionOffset, ff3Command_SetCamera_TargetOffset, ff3Command_SetCamera_ZoomOnOff,
								ff3Command_SetCamera_ZoomState, ff3Command_SetCamera_ZoomDegree, ff3Command_SetCamera_ZoomMax, ff3Command_SetCamera_ZoomMin, ff3Command_SetCamera_ZoomSpeed, ff3Command_SetCamera_BeforeEvent, ff3Command_SetCharacter_Collision, ff3Command_SetCharacter_GroundCollision, ff3Command_SetCharacter_WallCollision, ff3Command_SetCharacter_CharaCollision,
								ff3Command_SetCharacter_MapJump, ff3Command_SetCharacter_FixedTurn, ff3Command_StartBalloon, ff3Command_EndBalloon, ff3Command_PartyTalkEventOnJump, ff3Command_PartyTalkEventOffJump, ff3Command_CheckCharacterStatusJump, ff3Command_CheckPartyPCMemberEnaleOnJump, ff3Command_CheckPartyPCMemberEnaleOffJump, ff3Command_CheckParty_NPCMemberEnaleOnJump,
								ff3Command_CheckParty_NPCMemberEnaleOffJump, ff3Command_AddJob, ff3Command_SubJob, ff3Command_SetCharacter_MotionSpeed, ff3Command_SetCharacter_Alpha, ff3Command_BootNameEntry, ff3Command_CheckCharacter_StatusJump, ff3Command_StartPartyTalk, ff3Command_EndPartyTalk, ff3Command_TurnCharacter_AbsoluteAngle2,
								ff3Command_TurnCharacter_RelativeAngle2, ff3Command_TurnCharacter_AbsoluteCoordination2, ff3Command_TurnCharacter_LookCharacter2, ff3Command_TurnCharacter_StartLoop2, ff3Command_SetCharacter_ShadowAlpha, ff3Command_SetCharacter_TurnTargetCharacter, ff3Command_StartMotionCharacterDX, ff3Command_WorldMapWarp, ff3Command_SetMapJumpFlag, ff3Command_SetMapJumpFlagJump,
								ff3Command_StartWaorldPartyTalk, ff3Command_EndWaorldPartyTalk, ff3Command_SetPartyPCEquipItem, ff3Command_SetMapRotation, ff3Command_ChangeFaceEye, ff3Command_ChangeFaceMouth, ff3Command_MessagePermission, ff3Command_MessageWait, ff3Command_EndMotionCharacterDX, ff3Command_SetVehiclePosition,
								ff3Command_SetCharacter_ItemEvent, ff3Command_SetBattleEscape, ff3Command_SetUpCharacter_FaceData, ff3Command_CleanUpCharacter_FaceData, ff3Command_SetCameraMargin, ff3Command_SetCharacterDetectionRadius, ff3Command_StopSE, ff3Command_SetPartyMember_AllParameter, ff3Command_SetPartyMember_DefaultParameter, ff3Command_SetPartyMember_PhysicsAttackParameter,
								ff3Command_SetPartyMember_PhysicsDefenseParameter, ff3Command_SetPartyMember_MagicDefenseParameter, ff3Command_SetPartyMember_SkillParameter, ff3Command_SetCharacterCollisionRadius_Sphere, ff3Command_SetCharacterCollisionRadius_Box, ff3Command_SetCharacterTouchRadius_Sphere, ff3Command_SetCamera_Collision, ff3Command_SetUpBGM_Data, ff3Command_CleanUpBGM_Data, ff3Command_SetHalfWayBGM_Save,
								ff3Command_SetHalfWayBGM_Play, ff3Command_SetUpSE_Data, ff3Command_CleanUpSE_Data, ff3Command_SetBGM_Volume, ff3Command_SetHalfWayBGM_Start, ff3Command_SetUpEffectData, ff3Command_CleanUpEffectData, ff3Command_LabelRandomJump, ff3Command_SetRecovery2, ff3Command_SetConditionRecovery,
								ff3Command_SetMessagePosition, ff3Command_SetMessageShadow, ff3Command_SetMessageStyle, ff3Command_CheckMapName, ff3Command_SetMapJump_SE, ff3Command_SetCamera_PositionOffset2, ff3Command_SetCamera_TargetOffset2, ff3Command_SetCamera_FOV, ff3Command_SetDispSelect, ff3Command_SetMessageColor,
								ff3Command_StartConnectionMenu, ff3Command_DisplayMapName, ff3Command_HideMapName, ff3Command_LoadWorldBG, ff3Command_SetPositionWorldBG, ff3Command_AddPositionWorldBG, ff3Command_SetScrollWorldBG, ff3Command_SetVisibleWorldBG, ff3Command_FadeoutSub, ff3Command_FadeinSub,
								ff3Command_InputCheckJump, ff3Command_LoadWorldOBJ, ff3Command_SetPositionWorldOBJ, ff3Command_AddPositionWorldOBJ, ff3Command_SetAlphaWorldOBJ, ff3Command_SetAutoAlphaWorldOBJ, ff3Command_SetVisibleWorldOBJ, ff3Command_AssginBGForMessage, ff3Command_SetAutoAlphaMessage, ff3Command_SetAutoBrightnessWorldBG,
								ff3Command_SetBattleType, ff3Command_SetCamera_NearFarClip, ff3Command_ChangeStage, ff3Command_DisplayAllMessage, ff3Command_ChangeColorCharacter, ff3Command_GoToTitle, ff3Command_MoveFixedSpeedCharacter_AbsoluteCoordination, ff3Command_MoveFixedSpeedCharacter_RelativeCoordination, ff3Command_SetCanBoardVehicle, ff3Command_SetSendMessage,
								ff3Command_WaitInputSendMessage, ff3Command_SetSignEffect, ff3Command_SetAreaChangeShutterFlag, ff3Command_CustomFadeSetting, ff3Command_StartRotateMove, ff3Command_EndRotateMove, ff3Command_StartWaveGravity, ff3Command_EndWaveGravity, ff3Command_SetPlayerLevel, ff3Command_ReleaseStageMap,
								ff3Command_SetVehicleFieldNo, ff3Command_MoveCharacter_AbsoluteCoordination2, ff3Command_SetItemUseMenuManager, ff3Command_RestrictionItemUse, ff3Command_ChocoboBank, ff3Command_BootPlainCharacter, ff3Command_CreateBindObject, ff3Command_BindMotion, ff3Command_SetVisibleBindObject, ff3Command_PlayStream,
								ff3Command_StopStream, ff3Command_ResetFollowNPCState, ff3Command_CallSpecialMode, ff3Command_CallSave, ff3Command_CancelSetupMapSound, ff3Command_RemoveMotion, ff3Command_BootCharacterAsTopPlayer, ff3Command_CheckTopPlayerJob, ff3Command_CheckToPlayerJobLevel, ff3Command_CheckTopPlayer,
								ff3Command_SetShowWorldMap, ff3Command_SetRestartAfterBattle, ff3Command_CheckJob, ff3Command_CheckJobLevel, ff3Command_PlayNaviSE, ff3Command_SetEnableVehicleCamHeight, ff3Command_SetPartyMember_JobLevel, ff3Command_SetHalfWayBGM_Start2, ff3Command_SetPreRideVehicle, ff3Command_SetBeforeFieldNameJumpIndex,
								ff3Command_SetEnableViewVolumeClip, ff3Command_WithInCharacterJump2, ff3Command_WithOutCharacterJump2, ff3Command_SetPosition, ff3Command_StartRotateMoveSyncRotate, ff3Command_AllEffectFinishWait, ff3Command_StartMotionToRideVehicle, ff3Command_InAppPurchase
							};
							ff3Command_reuse_v0 = new VecFx32();
							ff3Command_reuse_v1 = new VecFx32();
							ff3Command_reuse_v2 = new VecFx32();
							ff3Command_reuse_v3 = new VecFx32();
							ff3Command_reuse_va = new VecFx32();
							g_SePlayRecord = new ds.Vector<RECORD, ds.OrderSavedErasePolicy<RECORD>>(4);
							bgm = 0;
							state_ = 0;
							flags = new byte[3u, 1000u];
							NUM_VALUE = 100u;
							values = new int[NUM_VALUE];
							g_ActiveScrollBars = new ds.Vector<ScrollBar, ds.FastErasePolicy<ScrollBar>>(4);
							LIMIT_OF_NODELIST = 32;
							CameraBattlePosition = new VecFx32(537084, 142272, 191833);
							CameraBattleTarget = new VecFx32(478028, 131072, 165657);
							CameraBattleAngle = new VecFx32(1792, -20736, 0);
							CameraBattleDistance = 65536;
							MOBOOK_LIFE_X = 128;
							MOBOOK_LIFE_Y = 144;
							MOBOOK_EXP_X = MOBOOK_LIFE_X;
							MOBOOK_EXP_Y = 154;
							MOBOOK_MONEY_X = MOBOOK_LIFE_X;
							MOBOOK_MONEY_Y = 164;
							__MBChocoboBankFactory__ = new MBChocoboBankFactory();
							VEHICLE_MOTIONNO_WAIT = 1001;
							VEHICLE_HEIGHT_AIR = 40960;
							VEHICLE_HEIGHT_GROUND = 0;
							VEHICLE_HEIGHT_ONSEA = -24576;
							BATTLE_MAP_ONSEA = 5;
							BATTLE_MAP_ONAIR = 7;
							BATTLE_MAP_INDEEPSEA = 8;
							MONSTER_GROUP_ONAIR = 5;
							EFFECT_CATEGORY_VEHICLE = 102;
							EFFECT_MEMBER_DROP = 9;
							EFFECT_MEMBER_SPRAY = 8;
							BGM_FADEOUT_FRAME = 5;
							g_Light = new ds.sys3d.CLightObject();
							m_pMedget = null;
							pPlayerGold = null;
							dummyCursor_ = new sys2d.Cell();
							_waitCounter = 0;
							m_MatrixStack = new Stack<Matrix>();
							m_ClearColor = Color.Black;
							m_fClearDepth = float.MaxValue;
							m_Blend = Blend.InverseSourceAlpha;
							m_DepthFunc = CompareFunction.Always;
							m_uiApplyTexture = 0u;
							m_bApplyEffect = true;
							m_asIndex = new short[16];
							m_abyCurrentColor = new byte[4];
							m_aGlTexture = new GlTexture[256];
							m_TextureMatrix = Matrix.Identity;
							m_afOrthoMatrix = new float[16];
							__DATE__ = "N/A";
							__TIME__ = "N/A";
							m_astrMessage = new string[16];
							m_aTestTimer = new TestTimer[16]
							{
								new TestTimer(),
								new TestTimer(),
								new TestTimer(),
								new TestTimer(),
								new TestTimer(),
								new TestTimer(),
								new TestTimer(),
								new TestTimer(),
								new TestTimer(),
								new TestTimer(),
								new TestTimer(),
								new TestTimer(),
								new TestTimer(),
								new TestTimer(),
								new TestTimer(),
								new TestTimer()
							};
						}

}
