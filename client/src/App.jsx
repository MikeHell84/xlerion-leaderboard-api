import { BrowserRouter, Routes, Route } from "react-router-dom"
import Navbar from "./components/Navbar"
import Leaderboard from "./pages/Leaderboard"
import SubmitScore from "./pages/SubmitScore"
import PlayerStats from "./pages/PlayerStats"

export default function App() {
    return (
        <BrowserRouter>
            <div style={{ background: "#0f0f0f", minHeight: "100vh", color: "white" }}>
                <Navbar />
                <div style={{ maxWidth: "1100px", margin: "0 auto", padding: "2rem" }}>
                    <Routes>
                        <Route path="/" element={<Leaderboard />} />
                        <Route path="/submit" element={<SubmitScore />} />
                        <Route path="/player" element={<PlayerStats />} />
                    </Routes>
                </div>
            </div>
        </BrowserRouter>
    )
}
