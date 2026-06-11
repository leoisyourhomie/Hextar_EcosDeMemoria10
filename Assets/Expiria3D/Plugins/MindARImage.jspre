Module["MindARImage"] = {
  this: this,

  video: null,

  controller: null,

  isFacingUser: false,

  onStartARPtr: null,

  onCameraConfigChangePtr: null,

  onUpdatePtr: null,

  onErrorPtr: null,

  isRunning: false,

  memory: {},

  markerDimensions: null,

  texPtr: null,

  invisibleMatrix: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],

  maxTrack: 1,

  mindFilePath: null,

  filterMinCF: 0.001,

  filterBeta: 0.001,

  missTolerance: 5, // number of miss before considered target lost. default is 5

  warmupTolerance: 5, // number of track before considered target found. default is 5

  SetMindFilePath: function (mindFilePath) {
    this.mindFilePath = mindFilePath;
  },

  SetMaxTrack: function (maxTrack) {
    this.maxTrack = maxTrack;
  },

  SetFilterMinCF: function (filterMinCF) {
    this.filterMinCF = filterMinCF;
  },

  SetFilterBeta: function (filterBeta) {
    this.filterBeta = filterBeta;
  },

  SetIsFacingUser: function (isFacingUser) {
    this.isFacingUser = isFacingUser;
  },

  GetExternalJS: function () {
    return new Promise((resolve) => {
      const SCRIPT_URL =
        "https://cdn.jsdelivr.net/npm/mind-ar@1.2.3/dist/mindar-image.prod.js";
      // console.log("get external js...", SCRIPT_URL);

      let script = document.querySelector(`script[src="${SCRIPT_URL}"]`);
      if (!script) {
        script = document.createElement("script");
        script.src = SCRIPT_URL;
        script.type = "module";
        script.async = true;
        document.body.appendChild(script);

        script.addEventListener("load", () => {
          // console.log("window mindar: ", window.MINDAR);
          resolve();
        });
      } else {
        resolve();
      }
    });
  },

  TextureUpdate: function (tex) {
    this.texPtr = tex;
  },

  StartAR: async function () {
    await this.GetExternalJS();
    await this.StartVideo();
    await this.StartMindAR();
    this.isRunning = true;

    if (this.onStartARPtr) {
      Module.dynCall_v(this.onStartARPtr);
    }
  },

  StopAR: function () {
    if (!this.isRunning) return;

    this.controller.stopProcessVideo();

    const tracks = this.video.srcObject.getTracks();
    tracks.forEach(function (track) {
      track.stop();
    });
    this.video.remove();
    this.isRunning = false;
  },

  IsRunning: function () {
    return this.isRunning;
  },

  StartVideo: function () {
    return new Promise((resolve, reject) => {
      navigator.mediaDevices
        .getUserMedia({
          audio: false,
          video: {
            facingMode: this.isFacingUser ? "user" : "environment",
          },
        })
        .then((stream) => {
          this.video = document.createElement("video");
          this.video.playsInline = true;
          this.video.srcObject = stream;
          this.video.addEventListener("loadedmetadata", () => {
            this.video.play();
            this.video.width = this.video.videoWidth;
            this.video.height = this.video.videoHeight;
            resolve();
          });
        })
        .catch((error) => {
          console.error('Error accessing media devices.', error);
          if (this.onErrorPtr) {
            Module.dynCall_v(this.onErrorPtr);
          }
          reject(error);
        });
    });
  },

  StartMindAR: async function () {
    const input = this.video;

    const controller = new window.MINDAR.IMAGE.Controller({
      inputWidth: input.width,
      inputHeight: input.height,
      maxTrack: this.maxTrack,
      filterMinCF: this.filterMinCF,
      filterBeta: this.filterBeta,
      missTolerance: this.missTolerance,
      warmupTolerance: this.warmupTolerance,

      onUpdate: (data) => {
        if (data.type === "updateMatrix") {
          const { targetIndex, worldMatrix } = data;

          if (worldMatrix == null) {
            HEAPF32.set(
              this.invisibleMatrix,
              this.memory.targetMatrixPtrs[targetIndex] >> 2
            );
          } else {
            HEAPF32.set(
              worldMatrix,
              this.memory.targetMatrixPtrs[targetIndex] >> 2
            );
          }

          if (this.onUpdatePtr) {
            Module.dynCall_vii(
              this.onUpdatePtr,
              targetIndex,
              worldMatrix == null ? 0 : 1
            );
          }
        }
      },
    });

    this.controller = controller;

    this.memory.cameraParamsPtr = _malloc(4 * 4);

    const { dimensions } = await controller.addImageTargets(this.mindFilePath);
    this.markerDimensions = dimensions;

    this.memory.targetMatrixPtrs = [];
    for (let i = 0; i < dimensions.length; i++) {
      this.memory.targetMatrixPtrs[i] = _malloc(16 * 4);
    }

    const drawWebcamTexture = () => {
      if (this.video === null) return;

      GLctx.bindTexture(GLctx.TEXTURE_2D, GL.textures[this.texPtr]);
      GLctx.pixelStorei(GLctx.UNPACK_FLIP_Y_WEBGL, true);
      GLctx.texSubImage2D(
        GLctx.TEXTURE_2D,
        0,
        0,
        0,
        GLctx.RGBA,
        GLctx.UNSIGNED_BYTE,
        this.video
      );
      GLctx.pixelStorei(GLctx.UNPACK_FLIP_Y_WEBGL, false);

      window.requestAnimationFrame(drawWebcamTexture);
    };
    window.requestAnimationFrame(drawWebcamTexture);

    const onResize = () => {
      this.video.width = this.video.videoWidth;
      this.video.height = this.video.videoHeight;

      const controllerProjectionMatrix = controller.getProjectionMatrix();

      const fovAdjust = this.video.width / controller.inputWidth;

      const far =
        controllerProjectionMatrix[14] / (controllerProjectionMatrix[10] + 1.0);
      const near =
        controllerProjectionMatrix[14] / (controllerProjectionMatrix[10] - 1.0);
      const fov =
        (2 * Math.atan(1 / controllerProjectionMatrix[5] / fovAdjust) * 180) /
        Math.PI;
      const aspect =
        controllerProjectionMatrix[5] / controllerProjectionMatrix[0];
      HEAPF32.set([fov, aspect, near, far], this.memory.cameraParamsPtr >> 2);

      if (this.onCameraConfigChangePtr) {
        Module.dynCall_v(this.onCameraConfigChangePtr);
      }
    };
    window.addEventListener("resize", onResize);

    onResize();

    controller.processVideo(input);
  },

  GetCameraParamsPtr() {
    return this.memory.cameraParamsPtr;
  },

  GetTargetWorldMatrixPtr(targetIndex) {
    return this.memory.targetMatrixPtrs[targetIndex];
  },

  GetVideoWidth: function () {
    return this.video.width;
  },

  GetVideoHeight: function () {
    return this.video.height;
  },

  GetNumTargets: function () {
    return this.markerDimensions.length;
  },

  GetNumTargetWidth: function (targetIndex) {
    return this.markerDimensions[targetIndex][0];
  },

  GetNumTargetHeight: function (targetIndex) {
    return this.markerDimensions[targetIndex][1];
  },
};