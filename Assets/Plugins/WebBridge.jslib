mergeInto(LibraryManager.library, {
  // Called from C# when the experience is complete. Tells the parent web page
  // (the landing/wrapper that hosts this WebGL build) to play the ending video.
  NotifyGameComplete: function () {
    try { window.parent.postMessage(JSON.stringify({ type: 'gameComplete' }), '*'); } catch (e) {}
    try { window.postMessage(JSON.stringify({ type: 'gameComplete' }), '*'); } catch (e) {}
  }
});
