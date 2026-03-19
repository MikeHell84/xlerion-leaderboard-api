import React from 'react'
import { NavLink } from 'react-router-dom'

const activeClass = 'text-white border-b-2 border-[#7f77dd]'

export default function Navbar() {
    return (
        <nav className="bg-[#1a1a1a] text-white px-6 py-4 flex items-center justify-between">
            <div className="flex items-center gap-4">
                <div style={{ color: '#7f77dd', fontWeight: 700, fontSize: 20 }}>XLERION</div>
            </div>

            <div className="space-x-6">
                <NavLink to="/" className={({ isActive }) => isActive ? activeClass : 'text-white/80'}>Leaderboard</NavLink>
                <NavLink to="/submit" className={({ isActive }) => isActive ? activeClass : 'text-white/80'}>Submit Score</NavLink>
                <NavLink to="/player" className={({ isActive }) => isActive ? activeClass : 'text-white/80'}>Player Stats</NavLink>
            </div>
        </nav>
    )
}
