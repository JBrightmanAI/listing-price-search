# syntax=docker/dockerfile:1
# Build context: client/

# ---- Build: static files with bun ---------------------------------------------------------
# Pinned to the same bun version as local development, so the lockfile is read identically.
FROM oven/bun:1.3.9 AS build
WORKDIR /app
# Dependencies first so this layer is cached until package.json or the lockfile changes.
COPY package.json bun.lock ./
# The cache mount keeps downloaded packages between builds, so rebuilds and retries are fast.
RUN --mount=type=cache,target=/root/.bun/install/cache \
    bun install --frozen-lockfile
COPY . .
RUN bun run build

# ---- Runtime: nginx serves the build and proxies the API ----------------------------------
# The unprivileged variant runs as a non-root user and listens on 8080.
FROM nginxinc/nginx-unprivileged:1.27-alpine AS runtime
COPY nginx/default.conf /etc/nginx/conf.d/default.conf
COPY nginx/security-headers.conf /etc/nginx/snippets/security-headers.conf
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 8080
HEALTHCHECK --interval=10s --timeout=3s --retries=3 \
    CMD wget -qO /dev/null http://127.0.0.1:8080/healthz || exit 1
