#!/usr/bin/env bash
# Start the backend API and frontend dev server in parallel.
# Both processes are killed when this script exits (Ctrl+C).

set -euo pipefail

ROOT="$(cd "$(dirname "$0")" && pwd)"

echo "Starting API on http://localhost:5000 ..."
dotnet run --urls http://localhost:5000 \
  --project "$ROOT/backend/Flowingly.Import.Api" &
API_PID=$!

echo "Starting frontend on http://localhost:5173 ..."
(cd "$ROOT/frontend" && npm run dev) &
FRONTEND_PID=$!

# Kill both on exit
trap 'echo "Stopping..."; kill $API_PID $FRONTEND_PID 2>/dev/null' EXIT INT TERM

echo ""
echo "Both servers running. Press Ctrl+C to stop."
echo "  API:      http://localhost:5000"
echo "  Swagger:  http://localhost:5000/swagger"
echo "  Frontend: http://localhost:5173"
echo ""

wait
