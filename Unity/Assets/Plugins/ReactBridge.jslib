mergeInto(LibraryManager.library, {
  NotifyAvatarComplete: function(jsonStr) {
    if (window.dispatchReactEvent) {
      window.dispatchReactEvent("AvatarComplete", UTF8ToString(jsonStr));
    }
  }
});
