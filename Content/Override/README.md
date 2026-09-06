# Content overrides

Anything dropped in here replaces the copy inside `data*.bin`. `GameArchive.Read`
looks here first and falls back to the archives, so a changed file needs no
repacking and no rebuild - restart the game and it is in.

The layout has to match the archived names, which are path qualified:

```
Content/Override/en.lproj/ca_text_01.NCGR
Content/Override/files/some.script
```

Get a correct tree by extracting one:

```
dotnet run --project Crystal.Editor -- archives Content
dotnet run --project Crystal.Editor -- extract-archives Content <somewhere> "en.lproj/*"
```

Copy out only the files actually being changed - the rest just costs load time.
Another directory can be used instead, with `OpenFF.exe --content-override=<dir>`.

Files here are ignored by git (see `.gitignore`), so local experiments do not end
up in the repository. Content meant to ship goes in deliberately, with the
`.gitignore` rule relaxed for it.
