# Making a release

The notes a release carries live in `Docs/Releases.md`, one `## <version>` section each; the workflow puts only the tagged version's section in the release's description.

1. Set the version in the three project files (`OpenFF\OpenFF.csproj`,
   `Crystal.Editor\Crystal.Editor.csproj`, `OpenFF.Engine\OpenFF.Engine.csproj`) and the
   `VERSION` line of `publish.cmd`; add the version's section to `Docs/Releases.md`.
2. `publish.cmd` - builds `dist\OpenFF\` self-contained for win-x64 with `OpenFF.Updater.exe`
   (a Native AOT build - it needs Visual Studio's C++ tools, which the Windows runner has) and zips
   it as `dist\OpenFF-<version>-win-x64.zip` (about 45 MB), its SHA-256 beside it in
   `dist\OpenFF-<version>-win-x64.zip.sha256`.
3. Try the zip on a clean machine or folder: `OpenFF.exe` finds the Steam install and boots
   (`Docs\Drives\ff3-boot.drive` runs the opening unattended), `crystal.exe` opens the editor.
4. Tag and push: `git tag v<version>` and `git push origin v<version>`. The workflow in
   `.github/workflows/release.yml` builds the same zip on a Windows runner and attaches it to
   a draft release for the tag; or upload the local zip by hand on GitHub's *Releases* page
   and paste the notes. Upload the `.sha256` with it either way.
5. Publish the draft. Installed clients see it at their next start (GitHub's `releases/latest`
   is the newest published, non-prerelease release): they offer it at the start (before the logos), download the
   zip, check it against the `.sha256` - a release without one is never installed - and hand
   over to the updater. To try a release on a test install first, point it at a copy of the
   release's description: `OpenFF.exe --update-feed=latest.json` (its asset URLs may be files),
   and `--update-throttle=<KB/s>` slows the download to see the progress.
