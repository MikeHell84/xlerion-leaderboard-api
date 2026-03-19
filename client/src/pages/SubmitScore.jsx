import React, { useState } from 'react'
import { submitScore } from '../api/leaderboard'

export default function SubmitScore() {
    const [playerId, setPlayerId] = useState(0)
    const [gameId, setGameId] = useState('xlerion-arena')
    const [value, setValue] = useState(0)
    const [level, setLevel] = useState(1)
    const [duration, setDuration] = useState(0)
    const [status, setStatus] = useState('idle')
    const [message, setMessage] = useState('')

    async function handleSubmit(e) {
        e.preventDefault()
        setStatus('loading')
        setMessage('')
        try {
            const payload = {
                playerId: Number(playerId),
                gameId,
                value: Number(value),
                level: Number(level),
                sessionDurationSeconds: Number(duration),
                metadata: null,
            }
            await submitScore(payload)
            setStatus('success')
            setMessage(`Score de ${payload.value} puntos registrado correctamente`)
        } catch (err) {
            setStatus('error')
            setMessage(err.message || 'Error al enviar score')
        }
    }

    return (
        <div>
            <h2 className="text-2xl font-bold mb-4">Registrar Score</h2>

            <form className="space-y-4 max-w-md" onSubmit={handleSubmit}>
                <div>
                    <label className="block text-sm text-gray-400">PlayerId</label>
                    <input type="number" value={playerId} onChange={e => setPlayerId(e.target.value)} className="w-full p-2 bg-[#0f0f0f] border border-[#2a2a2a] rounded" />
                </div>

                <div>
                    <label className="block text-sm text-gray-400">GameId</label>
                    <input value={gameId} onChange={e => setGameId(e.target.value)} className="w-full p-2 bg-[#0f0f0f] border border-[#2a2a2a] rounded" />
                </div>

                <div>
                    <label className="block text-sm text-gray-400">Score</label>
                    <input type="number" value={value} onChange={e => setValue(e.target.value)} className="w-full p-2 bg-[#0f0f0f] border border-[#2a2a2a] rounded" />
                </div>

                <div>
                    <label className="block text-sm text-gray-400">Level</label>
                    <input type="number" value={level} onChange={e => setLevel(e.target.value)} className="w-full p-2 bg-[#0f0f0f] border border-[#2a2a2a] rounded" />
                </div>

                <div>
                    <label className="block text-sm text-gray-400">Duración en segundos</label>
                    <input type="number" value={duration} onChange={e => setDuration(e.target.value)} className="w-full p-2 bg-[#0f0f0f] border border-[#2a2a2a] rounded" />
                </div>

                <div>
                    <button type="submit" className="px-4 py-2 bg-[#7f77dd] rounded">Enviar Score</button>
                </div>
            </form>

            <div className="mt-4">
                {status === 'loading' && <div className="text-gray-400">Enviando...</div>}
                {status === 'success' && <div className="text-[#1d9e75]">{message}</div>}
                {status === 'error' && <div className="text-[#e24b4a]">{message}</div>}
            </div>
        </div>
    )
}
