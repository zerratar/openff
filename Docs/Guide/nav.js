// The guide's side navigation, the same on every page; the current page is marked. On a
// phone the side becomes a drawer behind a menu button in a top bar, with the page's
// sections listed too. Every picture opens in a lightbox: tap to fill the screen, tap
// again for full size to pan and pinch, tap outside (or Esc) to close.
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
    { h: 'Project' },
    { file: 'https://github.com/zerratar/openff/releases/latest', label: 'Download' },
    { file: 'https://github.com/zerratar/openff', label: 'GitHub' },
    { file: 'https://github.com/zerratar/openff/blob/main/Docs/Releases.md', label: 'What changed' },
  ];
  const here = location.pathname.split('/').pop() || 'index.html';
  const wrap = document.querySelector('.wrap');
  const main = document.querySelector('main');
  if (!wrap || !main) return;

  // ---- the side (a drawer on a phone)
  const nav = document.createElement('nav');
  nav.className = 'side';
  const brand = document.createElement('div');
  brand.className = 'brand';
  brand.innerHTML = '<span style="color:#6ea8fe">◆</span> OpenFF guide <span>Crystal &amp; the client</span>';
  nav.append(brand);
  let current = null;
  for (const p of pages) {
    if (p.h) { const h = document.createElement('h4'); h.textContent = p.h; nav.append(h); continue; }
    const a = document.createElement('a');
    a.href = p.file;
    a.textContent = p.label;
    if (/^https?:/.test(p.file)) { a.target = '_blank'; a.rel = 'noopener'; }
    if (p.file.split('#')[0] === here && !p.file.includes('#')) { a.className = 'on'; current = p; }
    nav.append(a);
    // On this page: its sections, indented under it, for a phone to jump within the page.
    if (a.className === 'on') {
      const sections = [...main.querySelectorAll('h2')].filter(h => h.textContent.trim());
      if (sections.length > 1) {
        const list = document.createElement('div');
        list.className = 'sections';
        sections.forEach((h, i) => {
          if (!h.id) h.id = 's' + i + '-' + h.textContent.trim().toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/(^-|-$)/g, '');
          const s = document.createElement('a');
          s.href = '#' + h.id;
          s.textContent = h.textContent.trim();
          s.onclick = () => closeDrawer();
          list.append(s);
        });
        nav.append(list);
      }
    }
  }
  wrap.prepend(nav);

  // ---- the top bar (phones): title, menu button, previous / next page
  const bar = document.createElement('header');
  bar.className = 'topbar';
  const menu = document.createElement('button');
  menu.className = 'menu';
  menu.setAttribute('aria-label', 'Menu');
  menu.innerHTML = '<span></span><span></span><span></span>';
  const title = document.createElement('a');
  title.className = 'title';
  title.href = 'index.html';
  title.textContent = 'OpenFF guide';
  const where = document.createElement('span');
  where.className = 'where';
  where.textContent = current ? current.label : '';
  bar.append(menu, title, where);
  document.body.prepend(bar);
  const veil = document.createElement('div');
  veil.className = 'veil';
  document.body.append(veil);
  const openDrawer = () => { document.body.classList.add('drawer'); };
  const closeDrawer = () => { document.body.classList.remove('drawer'); };
  menu.onclick = () => document.body.classList.contains('drawer') ? closeDrawer() : openDrawer();
  veil.onclick = closeDrawer;
  nav.querySelectorAll('a').forEach(a => a.addEventListener('click', () => { if (!a.target) closeDrawer(); }));

  // Previous / next at the foot of the page, in the guide's order.
  const order = pages.filter(p => p.file && !p.file.includes('#') && !/^https?:/.test(p.file));
  const at = order.findIndex(p => p.file === here);
  if (at >= 0) {
    const foot = document.createElement('div');
    foot.className = 'pager';
    if (at > 0) { const a = document.createElement('a'); a.href = order[at - 1].file; a.textContent = '← ' + order[at - 1].label; foot.append(a); }
    const gap = document.createElement('span'); foot.append(gap);
    if (at < order.length - 1) { const a = document.createElement('a'); a.href = order[at + 1].file; a.textContent = order[at + 1].label + ' →'; foot.append(a); }
    const footer = main.querySelector('footer');
    if (footer) main.insertBefore(foot, footer); else main.append(foot);
  }

  // ---- the lightbox
  const box = document.createElement('div');
  box.className = 'lightbox';
  const pic = document.createElement('img');
  pic.alt = '';
  const hint = document.createElement('div');
  hint.className = 'hint';
  const close = document.createElement('button');
  close.className = 'close';
  close.textContent = '×';
  close.setAttribute('aria-label', 'Close');
  box.append(pic, hint, close);
  document.body.append(box);
  let full = false;
  const setMode = (isFull) => {
    full = isFull;
    box.classList.toggle('full', full);
    hint.textContent = full ? 'Drag or pinch to look around · tap the picture to fit the screen' : 'Tap the picture for full size · tap outside to close';
  };
  const show = (src, alt) => {
    pic.src = src; pic.alt = alt || '';
    box.classList.add('open');
    document.body.classList.add('lit');
    setMode(false);
    box.scrollTo(0, 0);
  };
  const hide = () => { box.classList.remove('open'); document.body.classList.remove('lit'); pic.src = ''; };
  pic.onclick = (e) => { e.stopPropagation(); setMode(!full); if (full) { requestAnimationFrame(() => { box.scrollTo((pic.scrollWidth - box.clientWidth) / 2, (pic.scrollHeight - box.clientHeight) / 3); }); } };
  box.onclick = (e) => { if (e.target === box || e.target === hint) hide(); };
  close.onclick = hide;
  document.addEventListener('keydown', e => { if (e.key === 'Escape' && box.classList.contains('open')) hide(); });
  for (const img of document.querySelectorAll('main figure img, main .hero img, main .cards img')) {
    img.classList.add('zoomable');
    img.title = img.title || 'Click to enlarge';
    img.addEventListener('click', () => show(img.currentSrc || img.src, img.alt));
  }
})();
