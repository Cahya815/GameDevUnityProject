# ─── Stage: Base ──────────────────────────────────────────────────────────────
FROM node:22-alpine AS base

# Install curl for health checks
RUN apk add --no-cache curl

WORKDIR /app

# Copy package files
COPY package*.json ./

# ─── Stage: Development ───────────────────────────────────────────────────────
FROM base AS development

# Install all dependencies (including devDependencies for nodemon)
RUN npm ci

# Copy source code
COPY . .

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

# Development mode (with nodemon hot-reload)
CMD ["npm", "run", "dev"]

# ─── Stage: Production ────────────────────────────────────────────────────────
FROM base AS production

# Install only production dependencies
RUN npm ci --only=production && npm cache clean --force

# Copy source code
COPY . .

# Create non-root user for security
RUN addgroup -g 1001 -S nodejs && \
    adduser -S nodejs -u 1001 && \
    chown -R nodejs:nodejs /app

USER nodejs

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

# Production mode
CMD ["npm", "start"]
