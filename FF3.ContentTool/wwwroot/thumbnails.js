// Pictures of assets, for the grids that let you choose one.
//
// A picture is easy for anything that already is one: an image, a texture, the sheet a
// cell bank cuts from. A model has to be drawn, so one hidden canvas renders each in
// turn and the result is kept as a data URL.
//
// Three things keep that from being a nuisance:
//
//   * one at a time, in a queue, so opening a folder of 833 models does not try to
//     make 833 WebGL contexts at once;
//   * only what has been asked for, which is only what is on screen, because the
//     grid asks lazily as its cells appear;
//   * kept for the session, so scrolling back is instant. They are not written to
//     disk - 833 of them is more than a browser will keep, and drawing one takes a
//     few hundred milliseconds, so paying that once per session is fair.

'use strict';

const THUMBS = new Map();          // name -> data URL, or null when it will not draw
const THUMB_WAITING = new Map();   // name -> the promise that is drawing it

let thumbCanvas = null;
let thumbViewer = null;

/// The hidden canvas everything is drawn on. Off the side of the page rather than
/// display:none, which would leave it with no size to render at.
function thumbnailStage() {
  if (thumbCanvas) return thumbCanvas;

  thumbCanvas = document.createElement('canvas');
  thumbCanvas.width = 160;
  thumbCanvas.height = 160;
  thumbCanvas.style.cssText =
    'position:fixed;left:-10000px;top:0;width:160px;height:160px;pointer-events:none';
  document.body.append(thumbCanvas);
  return thumbCanvas;
}

/// A picture of a model, drawn once and then remembered.
async function modelThumbnail(name) {
  if (THUMBS.has(name)) return THUMBS.get(name);
  if (THUMB_WAITING.has(name)) return THUMB_WAITING.get(name);

  const job = (async () => {
    try {
      const bundle = await api(`/api/model?name=${encodeURIComponent(name)}`);
      if (!bundle || bundle.error || !bundle.buffer || !bundle.buffer.length) {
        THUMBS.set(name, null);
        return null;
      }

      const canvas = thumbnailStage();
      if (!thumbViewer) {
        // preserveDrawingBuffer, or reading the canvas back gives an empty picture:
        // the browser is free to throw the buffer away after each frame otherwise.
        thumbViewer = makeModelViewer(canvas, () => {}, { preserveDrawingBuffer: true });
      }
      if (!thumbViewer) {
        THUMBS.set(name, null);
        return null;
      }

      await thumbViewer.show(bundle, name);
      thumbViewer.redraw();
      const url = canvas.toDataURL('image/png');
      THUMBS.set(name, url);
      return url;
    } catch (error) {
      THUMBS.set(name, null);
      return null;
    } finally {
      THUMB_WAITING.delete(name);
    }
  })();

  THUMB_WAITING.set(name, job);
  return job;
}

/// Models are drawn one at a time; two at once would fight over the one canvas.
let thumbQueue = Promise.resolve();

function queueModelThumbnail(name) {
  const next = thumbQueue.then(() => modelThumbnail(name));
  thumbQueue = next.catch(() => {});
  return next;
}

/// A picture for anything that is already one, without drawing anything.
function directThumbnail(kind, name) {
  if (kind === 'image') {
    return wsUrl(`/api/image?name=${encodeURIComponent(name)}`);
  }
  if (kind === 'texture') {
    return wsUrl(`/api/texture/png?name=${encodeURIComponent(name)}&index=0`);
  }
  return null;
}

/// Fills one cell with a picture when there is one to be had, leaving the icon it
/// already has when there is not.
function fillThumbnail(cell, kind, name) {
  const direct = directThumbnail(kind, name);
  if (direct) {
    showThumbnail(cell, direct);
    return;
  }
  if (kind !== 'model') return;

  const already = THUMBS.get(name);
  if (already) {
    showThumbnail(cell, already);
    return;
  }
  if (already === null && THUMBS.has(name)) return;   // tried, cannot be drawn

  queueModelThumbnail(name).then(url => {
    // The grid may have moved on to another folder while this was in the queue.
    if (url && cell.isConnected && cell.dataset.thumbFor === name) {
      showThumbnail(cell, url);
    }
  });
}

function showThumbnail(cell, url) {
  const icon = cell.querySelector('.icon');
  if (!icon) return;

  const picture = document.createElement('img');
  picture.className = 'thumb';
  picture.alt = '';
  // Swapped in straight away rather than on load. An image that is not in the document
  // and is marked lazy never loads at all, so waiting for onload waited for ever - and
  // a data URL is there the moment it is set anyway.
  picture.src = url;
  picture.onerror = () => picture.replaceWith(icon);
  icon.replaceWith(picture);
}

/// Asks for pictures only for the cells actually on screen, and again as more scroll
/// into view. Drawing 833 models to show twenty is the thing this avoids.
function watchThumbnails(root, kind) {
  if (!('IntersectionObserver' in window)) return null;

  const watcher = new IntersectionObserver((entries) => {
    for (const entry of entries) {
      if (!entry.isIntersecting) continue;
      watcher.unobserve(entry.target);
      fillThumbnail(entry.target, kind, entry.target.dataset.thumbFor);
    }
  }, { root, rootMargin: '120px' });

  return watcher;
}
