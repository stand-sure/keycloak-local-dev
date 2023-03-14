#! /bin/bash
# start-envoy.sh
envoy --config-path ./envoy-config.yaml --log-level info # --component-log-level upstream:debug
