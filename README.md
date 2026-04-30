# Church Admin — Frontend

React 18 + TypeScript + Vite

## Environment Variables

| Variable | Description |
|---|---|
| `VITE_API_BASE_URL` | Backend API URL (e.g. `https://church-admin-api.onrender.com`) |

## Local Development

```bash
npm install
npm run dev
```

## Deploy on Render (Static Site)

1. Create a **Static Site** on Render pointing to this repo
2. Build command: `npm install && npm run build`
3. Publish directory: `dist`
4. Set `VITE_API_BASE_URL` to your backend URL

## Notes

- The `public/_redirects` file handles SPA routing (all paths → `index.html`)
- Church slug defaults to `laborne` — update `DEFAULT_CHURCH_SLUG` in `src/app/App.tsx` if needed
