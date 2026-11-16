// Chat de Twitch en la web: se conecta como invitado anónimo (solo lectura, sin cuenta) por
// WebSocket y guarda los mensajes; el juego los recoge con FaseTwitchPoll cada fotograma.
mergeInto(LibraryManager.library, {
  FaseTwitchOpen: function (channelPtr) {
    var channel = UTF8ToString(channelPtr).toLowerCase().replace(/^#/, "");
    try {
      if (window.faseTwitch && window.faseTwitch.socket) window.faseTwitch.socket.close();
      var state = { socket: null, lines: [], status: 1 };
      window.faseTwitch = state;
      var socket = new WebSocket("wss://irc-ws.chat.twitch.tv:443");
      state.socket = socket;
      socket.onopen = function () {
        socket.send("PASS SCHMOOPIIE");
        socket.send("NICK justinfan" + Math.floor(10000 + Math.random() * 80000));
        socket.send("JOIN #" + channel);
      };
      socket.onmessage = function (event) {
        var parts = String(event.data).split("\r\n");
        for (var i = 0; i < parts.length; i++) {
          var line = parts[i];
          if (!line) continue;
          if (line.indexOf("PING") === 0) { socket.send("PONG :tmi.twitch.tv"); continue; }
          if (line.indexOf(" JOIN #") >= 0) state.status = 2;
          if (state.lines.length < 500) state.lines.push(line);
        }
      };
      socket.onerror = function () { state.status = 3; };
      socket.onclose = function () { if (state.status !== 0) state.status = 3; };
    } catch (e) {
      window.faseTwitch = { socket: null, lines: [], status: 3 };
    }
  },

  FaseTwitchClose: function () {
    try {
      if (window.faseTwitch) {
        window.faseTwitch.status = 0;
        if (window.faseTwitch.socket) window.faseTwitch.socket.close();
      }
    } catch (e) {}
  },

  // 0 apagado, 1 conectando, 2 dentro del canal, 3 error.
  FaseTwitchStatus: function () {
    return window.faseTwitch ? window.faseTwitch.status : 0;
  },

  // Devuelve las líneas nuevas separadas por "\n" (o una cadena vacía).
  FaseTwitchPoll: function () {
    var text = "";
    if (window.faseTwitch && window.faseTwitch.lines.length) {
      text = window.faseTwitch.lines.join("\n");
      window.faseTwitch.lines = [];
    }
    var size = lengthBytesUTF8(text) + 1;
    var buffer = _malloc(size);
    stringToUTF8(text, buffer, size);
    return buffer;
  },
});
