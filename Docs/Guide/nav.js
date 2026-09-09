// The guide's side navigation, the same on every page; the current page is marked.
(function () {
  const pages = [
    { h: 'Start here' },
    { file: 'index.html', label: 'Overview' },
    { file: 'getting-started.html', label: 'Getting started' },
    { h: 'Tutorials' },
    { file: 'map-editor.html', label: 'The map editor' },
    { file: 'own-map.html', label: 'A map of your own' },
    { file: 'items.html', label: 'Items and weapons' },
    { file: 'cutscenes.html', label: 'Cutscenes' },
    { file: 'assets.html', label: 'Models, sounds, fonts' },
    { file: 'code.html', label: 'C# code' },
    { file: 'steam.html', label: 'Steam mods' },
    { h: 'Reference' },
    { file: 'reference.html', label: 'Components and files' },
    { file: 'reference.html#timeline', label: 'Timeline clips' },
    { file: 'reference.html#settings', label: 'Settings and command line' },
    { file: 'reference.html#shortcuts', label: 'Shortcuts' },
  ];
  const here = location.pathname.split('/').pop() || 'index.html';
  const nav = document.createElement('nav');
  nav.className = 'side';
  const brand = document.createElement('div');
  brand.className = 'brand';
  brand.innerHTML = '<span style="color:#6ea8fe">◆</span> OpenFF guide <span>Crystal &amp; the client</span>';
  nav.append(brand);
  for (const p of pages) {
    if (p.h) { const h = document.createElement('h4'); h.textContent = p.h; nav.append(h); continue; }
    const a = document.createElement('a');
    a.href = p.file;
    a.textContent = p.label;
    if (p.file.split('#')[0] === here && !p.file.includes('#')) a.className = 'on';
    nav.append(a);
  }
  const wrap = document.querySelector('.wrap');
  if (wrap) wrap.prepend(nav);
})();
