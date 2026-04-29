# Stage 1: build the React app
FROM node:20-alpine AS build
WORKDIR /app

# Copy package files first to leverage layer caching on install
COPY package.json package-lock.json* ./

RUN npm ci

# Copy source and build
COPY . .

RUN npm run build

# Stage 2: serve with nginx
FROM nginx:alpine AS runtime

# Remove default nginx config
RUN rm /etc/nginx/conf.d/default.conf

COPY nginx.conf /etc/nginx/conf.d/app.conf
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 8080

CMD ["nginx", "-g", "daemon off;"]
