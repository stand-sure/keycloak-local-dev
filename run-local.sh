#! /bin/bash
open http://localhost:8180/auth
docker run --name keycloak -p 8180:8180 \
  -e KEYCLOAK_ADMIN=admin -e KEYCLOAK_ADMIN_PASSWORD=admin \
  quay.io/keycloak/keycloak \
  start-dev \
    --http-port 8180 \
    --http-relative-path /auth || docker start keycloak

