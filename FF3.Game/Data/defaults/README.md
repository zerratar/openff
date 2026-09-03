# OpenFF defaults

The last content root in the client's chain. Whatever the install in front does not
have, and the game logic cannot do without, is answered from here. The install always
wins; a mod can override anything here the usual way.

Nothing in this folder is art, text or a level. It holds a handful of small parameter
tables the FF3 logic reads at start-up that the FF4 install never carried, because FF4's
engine compiled the same numbers into its executable:

| File | What it is | Size |
| --- | --- | --- |
| `files/player_world_move_parameter.pak` | field movement tuning: walk/run acceleration and top speed, turn rates, per-terrain entry flags, vehicle rise/descent, footstep effect timing | 1.5 KB |
| `files/npc_world_move_parameter.pak` | NPC wander timing and follow distances | 144 B |

These are the phone build's tables, used as the client's defaults until FF4's own
values are read out of its engine (`libff4.so`, the way `Tools/gen_opcodes_ff4.py` reads
the command table) and written here as FF4's. If shipping even these is not wanted, the
client should instead synthesise them from constants in `GameProfile`; the loaders already
tolerate a missing file (the movement managers fall back to their defaults).
