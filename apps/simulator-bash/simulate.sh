#!/bin/bash

createValue() {

  local endpoint=$1

  local randomValue=$(openssl rand -base64 12)
  local randomTag=$(openssl rand -base64 12)

  echo -e "---\n"

  curl -X POST "http://${endpoint}" \
    -i \
    -H "Content-Type: application/json" \
    -d \
    '{
        "value": "'"${randomValue}"'",
        "tag": "'"${randomTag}"'"
    }'

  echo -e "\n"
  sleep $REQUEST_INTERVAL
}

####################
### SCRIPT START ###
####################

# Set variables
REQUEST_INTERVAL=1

nginxEndpoint="nginx-ingress-ingress-nginx-controller.nginx.svc.cluster.local:80"
dotnetEndpoint="${nginxEndpoint}/dotnet/dotnet/second"
dotnetEndpointError="${nginxEndpoint}/dotnet/dotnet/nginx"
javaEndpoint="${nginxEndpoint}/java/second"
javaEndpointError="${nginxEndpoint}/java/nginx"

# Start making requests
while true
do

  # Dotnet
  dotnetCount=$(echo $(( $RANDOM % 4 + 1 )))
  for i in $(eval echo "{1..$dotnetCount}")
  do
    createValue $dotnetEndpoint
  done

  errorCount=$(echo $(( $RANDOM % 2 )))
  if [[ $errorCount -eq 1 ]]; then
    createValue $dotnetEndpointError
  fi

  # Java
  javaCount=$(echo $(( $RANDOM % 3 + 1 )))
  for i in $(eval echo "{1..$javaCount}")
  do
    createValue $javaEndpoint
  done

  errorCount=$(echo $(( $RANDOM % 3 )))
  if [[ $errorCount -eq 1 ]]; then
    createValue $javaEndpointError
  fi
done
