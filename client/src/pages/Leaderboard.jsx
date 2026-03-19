import React, { useEffect, useState } from 'react'
import { getLeaderboard } from '../api/leaderboard'

const regions = ['GLOBAL', 'LATAM', 'NA', 'EU']

function Spinner() {
    return <div className="loader border-t-4 border-white/30 border-solid rounded-full w-8 h-8 animate-spin"></div>
}

export default function Leaderboard() {
    const [region, setRegion] = useState('GLOBAL')
    const [loading, setLoading] = useState(false)
    const [entries, setEntries] = useState([])
    const [error, setError] = useState(null)

    useEffect(() => {
        setLoading(true)
        setError(null)
        getLeaderboard('xlerion-arena', region, 20)
            .then(data => setEntries(data))
            .catch(err => setError(err.message))
            .finally(() => setLoading(false))
    }, [region])

    return (
        <div>
            <h2 className="text-2xl font-bold mb-4">Leaderboard Global</h2>

            <div className="flex items-center gap-4 mb-4">
                <label className="text-sm text-gray-400">Región</label>
                <select value={region} onChange={e => setRegion(e.target.value)} className="bg-[#0f0f0f] border border-[#2a2a2a] px-3 py-1 rounded">
                    {regions.map(r => <option key={r} value={r}>{r}</option>)}
                </select>
            </div>

            {loading ? (
                <div className="flex items-center justify-center py-8"><Spinner /></div>
            ) : error ? (
                <div className="text-red-400">{error}</div>
            ) : entries.length === 0 ? (
                <div className="text-gray-400">No hay datos disponibles.</div>
            ) : (
                <table className="min-w-full bg-[#1a1a1a] border border-[#2a2a2a] rounded overflow-hidden">
                    <thead className="bg-[#0f0f0f] text-left">
                        <tr>
                            <th className="p-3">Rank</th>
                            <th className="p-3">Jugador</th>
                            <th className="p-3">Región</th>
                            <th className="p-3">Mejor Score</th>
                            <th className="p-3">Nivel</th>
                        </tr>
                    </thead>
                    <tbody>
                        {entries.map((e, idx) => (
                            <tr key={e.playerId} className={idx % 2 === 0 ? 'bg-[#0f0f0f]' : 'bg-[#151515]'}>
                                <td className="p-3">{e.rank === 1 || idx === 0 ? <span className="text-yellow-400">🏆</span> : idx + 1}</td>
                                <td className="p-3">{e.displayName || e.username}</td>
                                <td className="p-3 text-gray-400">{e.region}</td>
                                <td className="p-3">{e.bestScore}</td>
                                <td className="p-3">{e.level}</td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            )}
        </div>
    )
}
