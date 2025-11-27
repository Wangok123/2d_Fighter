using UnityEngine;
using Quantum;
using TMPro;
using System.Linq;

public class QuantumPositionDebugger : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI debugText;
    
    [Header("Settings")]
    [SerializeField] private bool showDebug = true;
    [SerializeField] private float updateInterval = 0.1f;
    [SerializeField] private bool showPlayerInfo = true;
    [SerializeField] private bool showNetworkInfo = true;
    
    private float _nextUpdateTime;

    private void Start()
    {
        if (debugText != null)
        {
            debugText.gameObject.SetActive(showDebug);
        }
    }

    private void Update()
    {
        if (!showDebug || debugText == null) return;
        
        if (Time.time < _nextUpdateTime) return;
        _nextUpdateTime = Time.time + updateInterval;

        UpdateDebugInfo();
    }

    private void UpdateDebugInfo()
    {
        var game = QuantumRunner.Default?.Game;
        if (game == null)
        {
            debugText.text = "<color=red>❌ Quantum Game Not Running</color>";
            return;
        }

        var verifiedFrame = game.Frames.Verified;
        var predictedFrame = game.Frames.Predicted;

        if (verifiedFrame == null || predictedFrame == null)
        {
            debugText.text = "<color=yellow>⏳ Waiting for Frames...</color>";
            return;
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        
        // Frame Info
        sb.AppendLine("<b><size=20>⚡ Quantum Debug</size></b>");
        sb.AppendLine($"<color=cyan>Verified Frame:</color> {verifiedFrame.Number}");
        sb.AppendLine($"<color=green>Predicted Frame:</color> {predictedFrame.Number}");
        
        int frameDiff = predictedFrame.Number - verifiedFrame.Number;
        string diffColor = frameDiff > 3 ? "red" : frameDiff > 2 ? "yellow" : "white";
        sb.AppendLine($"<color={diffColor}>Frame Diff:</color> {frameDiff}");

        // Network Info
        if (showNetworkInfo && game.Session != null)
        {
            sb.AppendLine();
            sb.AppendLine("<b>🌐 Network</b>");
            var localPlayers = game.GetLocalPlayers();
            sb.AppendLine($"Local Players: {localPlayers.Count}");
        }

        // Player Info
        if (showPlayerInfo)
        {
            sb.AppendLine();
            sb.AppendLine("<b>👤 Players</b>");
            
            int playerCount = 0;
            for (int i = 0; i < verifiedFrame.PlayerCount; i++)
            {
                var playerData = verifiedFrame.GetPlayerData(i);
                if (playerData != null)
                {
                    playerCount++;
                }
            }
            sb.AppendLine($"Active Players: {playerCount}");
        }

        debugText.text = sb.ToString();
    }
}
