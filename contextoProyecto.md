Xlerion Leaderboard API — Agent Prompt
Contexto del proyecto
Estoy construyendo un portafolio profesional para una oferta de trabajo como Full Stack Developer con Unity.
El proyecto se llama Xlerion Leaderboard API — un backend REST en ASP.NET Core 8 + EF Core + SQL Server
que gestiona scores, rankings y logros de videojuegos.
El API ya está corriendo en <http://localhost:5000> con Swagger UI funcional.
SQL Server corre en Docker local en el puerto 1433.
La base de datos XlerionLeaderboard ya existe y tiene datos seed.
Stack actual

ASP.NET Core 8 Web API
Entity Framework Core 8 + SQL Server (Docker)
Swagger / OpenAPI
CORS configurado para localhost:3000 y xlerion.com

Estructura del proyecto
XlerionLeaderboardAPI/
├── Controllers/Controllers.cs   → ScoresController, LeaderboardController, PlayersController, AchievementsController
├── Data/AppDbContext.cs          → AppDbContext + SeedData
├── DTOs/Dtos.cs                  → ScoreSubmitDto, LeaderboardEntryDto, PlayerStatsDto, etc.
├── Models/Models.cs              → Player, Score, Achievement, PlayerAchievement
├── Services/LeaderboardService.cs → ILeaderboardService + LeaderboardService
├── Program.cs                    → DI, middleware, Swagger, migrations
└── appsettings.json              → ConnectionString SQL Server
Endpoints disponibles

POST   /api/Players                    → crear jugador
GET    /api/Players/{id}/stats         → stats completas de un jugador
POST   /api/Scores                     → registrar score
GET    /api/Leaderboard/{gameId}       → top players (con filtro ?region=LATAM&top=10)
POST   /api/Achievements/unlock        → desbloquear logro

Tareas que necesito que hagas ahora
FASE 1 — Verificación y corrección del API actual

Revisa todos los archivos del proyecto y verifica que compilan sin errores.
Asegúrate que las migrations de EF Core están aplicadas correctamente.
Agrega manejo global de errores con app.UseExceptionHandler en Program.cs
que retorne JSON consistente: { "error": "mensaje", "statusCode": 500 }.
Agrega logging con ILogger<T> en LeaderboardService para los métodos principales.
Verifica que el SeedData no falla si los registros ya existen (idempotente).

FASE 2 — Mini cliente React para demostrar el API
Crea una app React mínima en /Projects/client/ con Vite que consuma el API y muestre:

Página de Leaderboard (/leaderboard):

Selector de gameId (input o dropdown con "xlerion-arena" como default)
Selector de región (GLOBAL, LATAM, NA, EU)
Tabla con: Rank, DisplayName, Region, BestScore, Level, RecordedAt
Se actualiza al cambiar gameId o región
Llamada: GET /api/Leaderboard/{gameId}?region=X&top=10

Página de Submit Score (/submit):

Formulario con: PlayerId (number), GameId (text), Score (number), Level (number)
Botón Submit → POST /api/Scores
Muestra respuesta: éxito con el score guardado, o error claro

Página de Player Stats (/player/:id):

Input para ingresar PlayerId
Muestra: nombre, región, total de partidas, mejor score, promedio, logros desbloqueados
Llamada: GET /api/Players/{id}/stats

Requisitos del cliente React:

Usa fetch nativo (no axios)
Tailwind CSS para estilos (via CDN en index.html si es necesario)
API_BASE_URL = "<http://localhost:5000>" en un archivo de config o .env
Manejo de estados: loading, error, success en cada llamada
No necesita autenticación
Estructura limpia: /src/pages/, /src/components/, /src/api/

FASE 3 — Script de datos demo
Crea un script seed-demo.http (formato REST Client de VS Code) con requests
de ejemplo para poblar la base de datos con datos realistas:

5 jugadores de diferentes regiones
20 scores variados para "xlerion-arena"
3 logros desbloqueados

También crea seed-demo.ps1 con los mismos requests usando Invoke-RestMethod
para correr directamente desde PowerShell.
FASE 4 — README actualizado
Actualiza README.md con:

Sección "Demostración rápida" con screenshots o descripción del flujo
Instrucciones para correr el cliente React
Tabla de datos de prueba (jugadores y scores del seed)
Sección "Arquitectura" con descripción de cada capa
Badge de tecnologías: .NET 8, EF Core, SQL Server, React, Swagger

Consideraciones importantes

Mantén el código en español donde sea comentarios, en inglés donde sea código/variables
No cambies los nombres de los endpoints existentes (ya están en Swagger y funcionando)
El cliente React debe correr en puerto 3000 (ya está en el CORS del API)
Usa async/await en todo el código C# y JS
Prefiere record types para DTOs nuevos en C#

Resultado esperado al terminar

API corriendo en <http://localhost:5000> con Swagger
Cliente React corriendo en <http://localhost:3000> mostrando leaderboard en vivo
Script para poblar con datos demo de un solo comando
README profesional para el portafolio

Empieza por la Fase 1, dime si hay errores de compilación o de migración,
y luego avanza fase por fase confirmando cada una.
