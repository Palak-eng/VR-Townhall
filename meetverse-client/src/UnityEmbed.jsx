import { useEffect, useState } from "react";
import { Unity, useUnityContext } from "react-unity-webgl";

export default function UnityEmbed({ mode, avatarData, onExit }) {
  const UNITY_URL = "https://pub-cacca82a567344458962d14fa504e338.r2.dev";

  const { unityProvider, isLoaded, loadingProgression, sendMessage } = useUnityContext({
    loaderUrl: `${UNITY_URL}/Meetverse.loader.js`,
    dataUrl: `${UNITY_URL}/Meetverse.data`,
    frameworkUrl: `${UNITY_URL}/Meetverse.framework.js`,
    codeUrl: `${UNITY_URL}/Meetverse.wasm`,
  });

  const [hasBootstrapped, setHasBootstrapped] = useState(false);

  useEffect(() => {
    if (!isLoaded) return;

    if (mode === "dashboard") {
      console.log("Sending LoadDashboardScene to Unity...");
      sendMessage("SceneLoader", "LoadDashboardScene");
    } else if (mode === "multiplayer" && !hasBootstrapped) {
      console.log("Sending StartMultiplayer to Unity with data:", avatarData);
      sendMessage("WebBootstrapper", "StartMultiplayer", avatarData || "");
      setHasBootstrapped(true);
    }
  }, [isLoaded, mode, avatarData, hasBootstrapped, sendMessage]);

  const loadPercent = Math.round(loadingProgression * 100);

  return (
    <div style={{
      position: "fixed",
      zIndex: 300,
      background: mode === "dashboard" ? "transparent" : "#0a0a0a",
      overflow: "hidden",
      display: "flex", 
      flexDirection: "column", 
      alignItems: "center", 
      justifyContent: "center",
      transition: "all 0.6s cubic-bezier(0.25, 1, 0.5, 1)",
      
      // Dynamic mode styling to perfectly float over the React Dashboard Avatar sidebar box
      top: mode === "dashboard" ? 100 : 0,
      left: mode === "dashboard" ? 90 : 0,
      width: mode === "dashboard" ? 120 : "100%",
      height: mode === "dashboard" ? 120 : "100%",
      borderRadius: mode === "dashboard" ? "50%" : 0,
      pointerEvents: mode === "dashboard" ? "none" : "auto",
      boxShadow: mode === "dashboard" ? "0 4px 12px rgba(0,0,0,0.15)" : "none",
    }}>
      {!isLoaded && mode !== "dashboard" && (
        <div style={{
          position: "absolute", inset: 0,
          display: "flex", flexDirection: "column",
          alignItems: "center", justifyContent: "center",
          gap: 16, zIndex: 1,
        }}>
          <div style={{
            fontFamily: "'Bebas Neue', sans-serif",
            fontSize: "1.5rem", letterSpacing: "0.1em",
            color: "#f5f5f0"
          }}>
            LOADING MEETVERSE
          </div>
          <div style={{
            width: 240, height: 2,
            background: "#242424", borderRadius: 2,
          }}>
            <div style={{
              height: "100%", background: "#d4522a",
              width: `${loadPercent}%`,
              transition: "width 0.3s ease",
              borderRadius: 2,
            }} />
          </div>
          <div style={{
            fontFamily: "monospace", fontSize: "0.75rem",
            color: "#555", letterSpacing: "0.08em"
          }}>
            {loadPercent}%
          </div>
        </div>
      )}

      <Unity
        unityProvider={unityProvider}
        style={{
          width: "100%", height: "100%",
          visibility: isLoaded ? "visible" : "hidden",
        }}
      />

      {mode !== "dashboard" && (
        <button
          onClick={onExit}
          style={{
            position: "absolute", top: 20, right: 20,
            background: "rgba(10,10,10,0.8)",
            border: "1px solid #242424",
            color: "#888", padding: "7px 16px",
            fontFamily: "sans-serif", fontSize: "0.75rem",
            letterSpacing: "0.08em", textTransform: "uppercase",
            cursor: "pointer", borderRadius: 3, zIndex: 2,
          }}
        >
          Exit ✕
        </button>
      )}
    </div>
  );
}