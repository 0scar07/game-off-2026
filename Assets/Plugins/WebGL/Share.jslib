// Compartir desde la web: en el móvil abre el menú de compartir del sistema;
// en el ordenador copia el texto al portapapeles. Devuelve 1 si se compartió, 2 si se copió, 0 si falló.
mergeInto(LibraryManager.library, {
  FaseShare: function (textPtr) {
    var text = UTF8ToString(textPtr);
    try {
      if (navigator.share && /Android|iPhone|iPad|iPod/i.test(navigator.userAgent)) {
        navigator.share({ text: text }).catch(function () {});
        return 1;
      }
      if (navigator.clipboard && navigator.clipboard.writeText) {
        navigator.clipboard.writeText(text);
        return 2;
      }
      var area = document.createElement("textarea");
      area.value = text;
      document.body.appendChild(area);
      area.select();
      var ok = document.execCommand("copy");
      document.body.removeChild(area);
      return ok ? 2 : 0;
    } catch (e) {
      return 0;
    }
  },

  // Descarga una imagen PNG (en base64) como archivo.
  FaseDownload: function (namePtr, dataPtr) {
    try {
      var a = document.createElement("a");
      a.href = "data:image/png;base64," + UTF8ToString(dataPtr);
      a.download = UTF8ToString(namePtr);
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
    } catch (e) {}
  },

  FaseVibrate: function (ms) {
    try { if (navigator.vibrate) navigator.vibrate(ms); } catch (e) {}
  },
});
