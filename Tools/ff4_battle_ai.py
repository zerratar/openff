"""FF4's monster AI scripts (battle_ai.bbd), disassembled.

    python Tools/ff4_battle_ai.py <battle_ai.bbd> [script ids...]

The file (btl::BattleScriptManager::load): "HESD", then at 0x10 an info header (+4 the command count, +0xC the
script count), at 0x18 the offset of the command table (16 bytes a command: id, length in bytes, ...), at 0x20 the
offset of the script table (12 bytes a script: id, offset, how many commands). A script is a run of commands, each its id (u32)
and its operands (s32), as long as the table says. The names are the classes BattleScriptEngine's constructor puts
in its table, in command order (read off libff4.so's code).
"""
import struct
import sys

NAMES = [
    'Start', 'End', 'Start', 'Start', 'GetRandomNumbers', 'Start', 'SetAction', 'SetTargetRandomEnemy', 'Start',
    'GetAttacker', 'Label', 'JumpLabel', 'DecideTurnAction', 'CheakOctManmosEraseLeg', 'ShowMessage', 'HideMessage',
    'Wait', 'GetBreed', 'GetPlayerId', 'GetMonsterId', 'Comparison', 'GetPlayerFlag', 'SetBattleFlag', 'GetActor',
    'GetCharacterVariable', 'SetCharacterVariable', 'AddCharacterVariable', 'GetAbility', 'GetMyCharacterId',
    'IsTargeted', 'EnemyAllTarget', 'FlagOn', 'FlagOff', 'FlagCheak', 'GetLegNumber', 'DecideCounterAction',
    'GetBattleTime', 'StartEventAction', 'SetEventActor', 'SetTarget', 'DeathCharacter', 'StartEventMode',
    'EndEventMode', 'CheakPlayerATP', 'EndBattle', 'GetPlayerHP', 'GetMonsterHP', 'ChangeBGM', 'ResetATG',
    'CreateModel', 'WaitLoadingModel', 'WaitLoadingTexture', 'WaitLoadingMotion', 'AddMotion', 'StartMotion',
    'SetPosition', 'SetRotation', 'SetAlpha', 'SetScale', 'SetShow', 'DeleteModel', 'SetShadowAlpha', 'SetShadowShow',
    'GetTiming', 'GetAttackerBreed', 'GetAttackerId', 'CheakPlayerATW', 'ChargePlayerATP', 'ChargePlayerATW',
    'DeathMonster', 'CheakMonsterATP', 'ResetMonsterATG', 'Calculation', 'GetBattleCharacterId', 'NotDeathFlagOn',
    'NotDeathFlagOff', 'SufferDamage', 'AddPlayer', 'EventActionTargetCheck', 'FriendAllTarget', 'AllTarget', 'SetATP',
    'PairMagic', 'RevivalParty', 'PlayerHpMax', 'SetVariable', 'AllPlayerReverse', 'AllPlayerCatch', 'CheckGameover',
    'SetConditionAllMonster', 'SufferPhysical', 'IsMonsterTargeted', 'OnForceMaxDamage', 'OffForceMaxDamage',
    'LoadAsyncSE', 'LoadingWaitSE', 'PlaySE', 'AllReleaseSE', 'IsCounter', 'IsConfusionForMonster',
    'OffInvokeProdaction', 'OnInvokeProdaction', 'OnSurelyCondition', 'OffSurelyCondition', 'ResetFrameCounter',
    'GetFrameCounter', 'HideGameoverMessage', 'CheckCondition', 'SetCharacterVariableForMonsterAll', 'OnReflecThrough',
    'OffReflecThrough',
]


def read(path):
    d = open(path, 'rb').read()
    assert d[:4] == b'HESD', 'not battle_ai.bbd'
    ncmd = struct.unpack_from('<i', d, 0x14)[0]
    nscr = struct.unpack_from('<i', d, 0x1C)[0]
    ctab = struct.unpack_from('<i', d, 0x18)[0]
    stab = struct.unpack_from('<i', d, 0x20)[0]
    lengths = [struct.unpack_from('<i', d, ctab + 16 * i + 4)[0] for i in range(ncmd)]
    scripts = {}
    for i in range(nscr):
        sid, off, size = struct.unpack_from('<3i', d, stab + 12 * i)
        scripts[sid] = (off, size)
    return d, lengths, scripts


def disassemble(d, lengths, off, count):
    out, p = [], off
    while len(out) < count:
        c = struct.unpack_from('<I', d, p)[0]
        n = lengths[c] if c < len(lengths) else 4
        args = struct.unpack_from('<%di' % (n // 4 - 1), d, p + 4) if n > 4 else ()
        out.append((p - off, c, args))
        p += n
    return out


if __name__ == '__main__':
    d, lengths, scripts = read(sys.argv[1])
    ids = [int(a) for a in sys.argv[2:]] or sorted(scripts)
    for sid in ids:
        off, size = scripts[sid]
        print('script %d (%d commands)' % (sid, size))
        for at, c, args in disassemble(d, lengths, off, size):
            print('  %4d  %-28s %s' % (at, NAMES[c] if c < len(NAMES) else '?%d' % c, ', '.join(map(str, args))))
