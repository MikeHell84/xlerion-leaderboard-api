import React, { useState } from 'react'
import { getPlayerStats } from '../api/leaderboard'
import { useParams } from 'react-router-dom'

function Spinner() { return <div className="loader border-t-4 border-white/30 border-solid rounded-full w-8 h-8 animate-spin"></div> }

export default function PlayerStats() {
    const params = useParams()
    const [playerId, setPlayerId] = useState(params.id || '')
    const [loading, setLoading] = useState(false)
    const [data, setData] = useState(null)
    const [error, setError] = useState(null)

    async function handleSearch(id) {
        setLoading(true); setError(null); setData(null)
        try {
            const parsed = Number(id)
            if (Number.isNaN(parsed)) throw new Error('PlayerId debe ser un número')
            const res = await getPlayerStats(parsed)
            setData(res)
        } catch (err) { setError(err.message || String(err)) }
        finally { setLoading(false) }
    }

    return (
        <div>
            <h2 className="text-2xl font-bold mb-4">Stats del Jugador</h2>

            <div className="flex gap-2 mb-4">
                <input type="number" className="p-2 bg-[#0f0f0f] border border-[#2a2a2a] rounded" value={playerId} onChange={e => setPlayerId(e.target.value)} />
                <button className="px-3 py-2 bg-[#7f77dd] rounded" onClick={() => handleSearch(playerId)}>Buscar</button>
            </div>

            {loading && <div className="py-6"><Spinner /></div>}
            {error && <div className="text-[#e24b4a]">{error}</div>}

            {data && (
                <div className="space-y-4">
                    <div className="p-4 bg-[#1a1a1a] rounded">
                        <h3 className="text-2xl font-bold">{data.displayName}</h3>
                        <div className="text-gray-400">{data.username} — {data.region}</div>
                    </div>

                    <div className="grid grid-cols-4 gap-4">
                        <div className="p-4 bg-[#1a1a1a] rounded">Total Partidas<br /><strong>{data.totalGames}</strong></div>
                        <div className="p-4 bg-[#1a1a1a] rounded">Mejor Score<br /><strong>{data.bestScore}</strong></div>
                        <div className="p-4 bg-[#1a1a1a] rounded">Score Promedio<br /><strong>{Math.round(data.averageScore)}</strong></div>
                        <div className="p-4 bg-[#1a1a1a] rounded">Puntos de Logros<br /><strong>{data.achievementPoints}</strong></div>
                    </div>

                    <div>
                        <h4 className="font-bold mt-4 mb-2">Últimos 5 Scores</h4>
                        <table className="w-full text-sm bg-[#1a1a1a] rounded overflow-hidden">
                            <thead>
                                <tr className="text-left text-white/80 border-b border-[#2a2a2a]">
                                    <th className="p-2">GameId</th>
                                    <th className="p-2">Score</th>
                                    <th className="p-2">Nivel</th>
                                    <th className="p-2">Fecha</th>
                                </tr>
                            </thead>
                            <tbody>
                                {data.recentScores.slice(0, 5).map((s, i) => (
                                    <tr key={i} className={i % 2 === 0 ? 'bg-[#0f0f0f]' : 'bg-[#151515]'}>
                                        <td className="p-2">{s.gameId}</td>
                                        <td className="p-2">{s.value}</td>
                                        <td className="p-2">{s.level}</td>
                                        <td className="p-2">{new Date(s.recordedAt).toLocaleString()}</td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>

                    <div>
                        <h4 className="font-bold mt-4 mb-2">Achievements</h4>
                        <ul className="space-y-2">
                            {data.achievements.map(a => (
                                <li key={a.code} className="p-3 bg-[#1a1a1a] rounded">
                                    <div className="font-semibold">{a.name} <span className="text-gray-400">({a.points} pts)</span></div>
                                    <div className="text-gray-400">{a.description}</div>
                                </li>
                            ))}
                        </ul>
                    </div>
                </div>
            )}
        </div>
    )
}
