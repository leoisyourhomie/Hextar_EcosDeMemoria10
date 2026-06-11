mergeInto(LibraryManager.library, {
  InitializeVideoTexture: function (
    textureID,
    url,
    width,
    height,
    playbackRate,
    loop,
    autoPlay
  ) {
    textureID = textureID >>> 0;

    var canvas = document.createElement("canvas");
    canvas.width = width;
    canvas.height = height;
    var ctx = canvas.getContext("2d");

    playbackRate = playbackRate || 1.0;
    loop = !!loop;
    autoPlay = !!autoPlay;

    // Crear objeto global para controlar el video
    window.MyUnityVideoPlayer = {
      video: null,
      isPlaying: false,
      isSeeking: false,
      showFirstFrame: false,
      isUpdateLoopRunning: false, // Nueva propiedad
      textureID: textureID,
      width: width,
      height: height,
      ctx: ctx,
      updateTexture: function () {
        var self = this;

        if (!self.isUpdateLoopRunning) {
          self.isUpdateLoopRunning = true;
        }

        if (!this.isPlaying && !this.showFirstFrame) {
          self.isUpdateLoopRunning = false;
          return;
        }

        if (this.video.readyState >= this.video.HAVE_CURRENT_DATA) {
          this.ctx.save();
          this.ctx.scale(1, -1);
          this.ctx.drawImage(this.video, 0, 0, this.width, -this.height);
          this.ctx.restore();

          var imageData = this.ctx.getImageData(0, 0, this.width, this.height);
          GLctx.bindTexture(GLctx.TEXTURE_2D, GL.textures[this.textureID]);
          GLctx.texSubImage2D(
            GLctx.TEXTURE_2D,
            0,
            0,
            0,
            this.width,
            this.height,
            GLctx.RGBA,
            GLctx.UNSIGNED_BYTE,
            imageData.data
          );

          if (this.showFirstFrame) {
            console.log("First frame updated");
            this.showFirstFrame = false;
          }
        } else {
          console.log("Video not ready, readyState:", this.video.readyState);
        }

        window.requestAnimationFrame(function () {
          self.updateTexture();
        });
      },
    };

    var video = document.createElement("video");
    video.src = UTF8ToString(url);
    video.crossOrigin = "anonymous";
    video.loop = loop;
    video.muted = true;
    video.autoplay = autoPlay;
    video.playsInline = true;
    video.style.display = "none";
    video.playbackRate = playbackRate;

    window.MyUnityVideoPlayer.video = video;

    video.addEventListener("loadeddata", function () {
      console.log("Video loadeddata event fired");
      if (!autoPlay) {
        video.currentTime = 0.1;
        video.pause();
        console.log("CurrentTime after setting:", video.currentTime);
        window.MyUnityVideoPlayer.showFirstFrame = true;
        if (!window.MyUnityVideoPlayer.isUpdateLoopRunning) {
          window.MyUnityVideoPlayer.updateTexture();
        }
      }
    });

    video.addEventListener("play", function () {
      window.MyUnityVideoPlayer.isPlaying = true;
      if (!window.MyUnityVideoPlayer.isUpdateLoopRunning) {
        window.MyUnityVideoPlayer.updateTexture();
      }
    });

    video.addEventListener("pause", function () {
      window.MyUnityVideoPlayer.isPlaying = false;
    });

    document.body.appendChild(video);
    if (autoPlay) {
      video.play().catch(function (error) {
        console.log("No se pudo reproducir el video automáticamente:", error);
      });
    }
  },

  PauseVideo: function () {
    if (window.MyUnityVideoPlayer && window.MyUnityVideoPlayer.video) {
      window.MyUnityVideoPlayer.video.pause();
    }
  },

  PlayVideo: function () {
    if (window.MyUnityVideoPlayer && window.MyUnityVideoPlayer.video) {
      window.MyUnityVideoPlayer.video.play().catch(function (error) {
        console.log("Error al reproducir el video:", error);
      });
      window.MyUnityVideoPlayer.isPlaying = true;
      if (!window.MyUnityVideoPlayer.isUpdateLoopRunning) {
        window.MyUnityVideoPlayer.updateTexture();
      }
    }
  },

  SetVolume: function (volume) {
    if (window.MyUnityVideoPlayer && window.MyUnityVideoPlayer.video) {
      window.MyUnityVideoPlayer.video.volume = volume;
      window.MyUnityVideoPlayer.video.muted = volume === 0;
    }
  },

  SetPlaybackRate: function (rate) {
    if (window.MyUnityVideoPlayer && window.MyUnityVideoPlayer.video) {
      window.MyUnityVideoPlayer.video.playbackRate = rate;
    }
  },

  SetLoopJS: function (loop) {
    if (window.MyUnityVideoPlayer && window.MyUnityVideoPlayer.video) {
      window.MyUnityVideoPlayer.video.loop = !!loop;
    }
  },

  ChangeVideoURL: function (url, autoPlay) {
    if (window.MyUnityVideoPlayer && window.MyUnityVideoPlayer.video) {
      var video = window.MyUnityVideoPlayer.video;

      autoPlay = !!autoPlay; // Convertir a booleano
      video.autoplay = autoPlay; // Establecer autoplay en video

      // Agregar listener para 'loadeddata' antes de cambiar la src
      var onLoadedData = function () {
        console.log("Video loadeddata event fired after URL change");
        video.removeEventListener("loadeddata", onLoadedData);
        if (!video.autoplay) {
          video.currentTime = 0.1;
          video.pause();
          console.log("CurrentTime after setting:", video.currentTime);
          window.MyUnityVideoPlayer.showFirstFrame = true;
          if (!window.MyUnityVideoPlayer.isUpdateLoopRunning) {
            window.MyUnityVideoPlayer.updateTexture();
          }
        } else {
          video.play().catch(function (error) {
            console.log("Error al reproducir el video:", error);
          });
          window.MyUnityVideoPlayer.isPlaying = true;
          if (!window.MyUnityVideoPlayer.isUpdateLoopRunning) {
            window.MyUnityVideoPlayer.updateTexture();
          }
        }
      };
      video.addEventListener("loadeddata", onLoadedData);

      video.src = UTF8ToString(url);
      video.load();
    }
  },

  GetVideoDuration: function () {
    if (window.MyUnityVideoPlayer && window.MyUnityVideoPlayer.video) {
      return window.MyUnityVideoPlayer.video.duration;
    }
    return 0;
  },

  GetVideoCurrentTime: function () {
    if (window.MyUnityVideoPlayer && window.MyUnityVideoPlayer.video) {
      return window.MyUnityVideoPlayer.video.currentTime;
    }
    return 0;
  },

  SeekVideo: function (time) {
    if (window.MyUnityVideoPlayer && window.MyUnityVideoPlayer.video) {
      window.MyUnityVideoPlayer.isSeeking = true;

      window.MyUnityVideoPlayer.video.currentTime = time;

      window.MyUnityVideoPlayer.video.addEventListener(
        "seeked",
        function onSeeked() {
          window.MyUnityVideoPlayer.isSeeking = false;
          window.MyUnityVideoPlayer.video.removeEventListener(
            "seeked",
            onSeeked
          );
        }
      );
    }
  },

  IsVideoLoaded: function () {
    if (window.MyUnityVideoPlayer && window.MyUnityVideoPlayer.video) {
      return window.MyUnityVideoPlayer.video.readyState >=
        window.MyUnityVideoPlayer.video.HAVE_ENOUGH_DATA
        ? 1
        : 0;
    }
    return 0;
  },

  IsVideoSeeking: function () {
    if (window.MyUnityVideoPlayer && window.MyUnityVideoPlayer.isSeeking) {
      return 1;
    }
    return 0;
  },

  UpdateTextureWithFirstFrame: function () {
    if (window.MyUnityVideoPlayer && window.MyUnityVideoPlayer.video) {
      if (
        window.MyUnityVideoPlayer.video.readyState >=
        window.MyUnityVideoPlayer.video.HAVE_CURRENT_DATA
      ) {
        window.MyUnityVideoPlayer.showFirstFrame = true;
        window.MyUnityVideoPlayer.updateTexture();
        window.MyUnityVideoPlayer.showFirstFrame = false;
      } else {
        // Si el video no está listo, esperamos al evento 'loadeddata'
        window.MyUnityVideoPlayer.video.addEventListener(
          "loadeddata",
          function onLoadedData() {
            window.MyUnityVideoPlayer.video.removeEventListener(
              "loadeddata",
              onLoadedData
            );
            window.MyUnityVideoPlayer.showFirstFrame = true;
            window.MyUnityVideoPlayer.updateTexture();
            window.MyUnityVideoPlayer.showFirstFrame = false;
          }
        );
      }
    }
  },
});
