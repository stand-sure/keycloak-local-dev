#! /bin/bash
curl localhost:8181/v1/data -d @- <<< $(cat sample-input.json)| jq

