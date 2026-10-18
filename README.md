# Tekus Provider Services

Full-stack technical test: .NET 8 Clean Architecture API + React/Vite frontend.

## Prerequisites
- .NET SDK 8.0+
- Node.js 20+
- SQL Server LocalDB (optional in tests)

## Backend
`ash
dotnet build Tekus.ProviderServices.sln
dotnet run --project src/Tekus.Api
`

## Frontend
`ash
cd frontend
npm i
npm run dev
`

## Tests
`ash
dotnet test
`

API: http://localhost:5199/swagger
Frontend: http://localhost:5173

