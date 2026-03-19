const API_BASE = "https://xlerion-leaderboard-api-production.up.railway.app";

export async function getLeaderboard(gameId = "xlerion-arena", region = null, top = 10) {
    const url = region && region !== "GLOBAL"
        ? `${API_BASE}/api/Leaderboard/${gameId}?region=${region}&top=${top}`
        : `${API_BASE}/api/Leaderboard/${gameId}?top=${top}`;
    const res = await fetch(url);
    if (!res.ok) throw new Error("Error cargando leaderboard");
    return res.json();
}

export async function getPlayerStats(playerId) {
    const res = await fetch(`${API_BASE}/api/Players/${playerId}/stats`);
    if (!res.ok) throw new Error("Jugador no encontrado");
    return res.json();
}

export async function submitScore(data) {
    const res = await fetch(`${API_BASE}/api/Scores`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(data)
    });
    if (!res.ok) throw new Error("Error enviando score");
    return res.json();
}
