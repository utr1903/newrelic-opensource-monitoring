#!/bin/bash

# Get commandline arguments
while (( "$#" )); do
  case "$1" in
    --cluster-name)
      clusterName="$2"
      shift
      ;;
    *)
      shift
      ;;
  esac
done

if [[ $clusterName == "" ]]; then
  echo -e "Cluster name is not given. (--cluster-name)\n"
  exit 1
fi

##################
### Apps Setup ###
##################

### Set variables
newRelicOtlpGrpcEndpoint="https://otlp.eu01.nr-data.net:4317"

# Otel Collector
declare -A otelcollector
otelcollector["name"]="otelcollector"
otelcollector["namespace"]="monitoring"
otelcollector["grpcPort"]=4317
otelcollector["httpPort"]=4318
otelcollector["fluentPort"]=8006
otelcollector["grpcEndpoint"]="http://${otelcollector[name]}.${otelcollector[namespace]}.svc.cluster.local:${otelcollector[grpcPort]}"

# Fluent Bit
declare -A fluentbit
fluentbit["name"]="fluentbit"
fluentbit["namespace"]="monitoring"

# Node Exporter
declare -A nodexporter
nodexporter["name"]="nodexporter"
nodexporter["namespace"]="monitoring"

# Prometheus
declare -A prometheus
prometheus["name"]="prometheus"
prometheus["namespace"]="monitoring"
prometheus["newrelicEndpoint"]="https://metric-api.eu.newrelic.com/prometheus/v1/write?prometheus_server=${clusterName}"

### Java ###

# First
declare -A javafirst
javafirst["name"]="java-first"
javafirst["namespace"]="java"
javafirst["replicas"]=2
javafirst["port"]=8080

# Second
declare -A javasecond
javasecond["name"]="java-second"
javasecond["namespace"]="java"
javasecond["replicas"]=2
javasecond["port"]=8080

### Java ###

# First
declare -A dotnetfirst
dotnetfirst["name"]="dotnet-first"
dotnetfirst["namespace"]="dotnet"
dotnetfirst["replicas"]=2
dotnetfirst["port"]=8080
dotnetfirst["portPrometheus"]=5000

# Second
declare -A dotnetsecond
dotnetsecond["name"]="dotnet-second"
dotnetsecond["namespace"]="dotnet"
dotnetsecond["replicas"]=2
dotnetsecond["port"]=8080
dotnetsecond["portPrometheus"]=5000

### Simulator ###

# Bash
declare -A simulatorbash
simulatorbash["name"]="simulatorbash"
simulatorbash["namespace"]="simulator"
simulatorbash["replicas"]=5

# Go
declare -A simulatorgo
simulatorgo["name"]="simulatorgo"
simulatorgo["namespace"]="simulator"
simulatorgo["port"]=8080
#########

####################
### Build & Push ###
####################

### Java ###

# First
docker build \
  --tag "${DOCKERHUB_NAME}/${javafirst[name]}" \
  "../../apps/java-first/."
docker push "${DOCKERHUB_NAME}/${javafirst[name]}"

# Second
docker build \
  --tag "${DOCKERHUB_NAME}/${javasecond[name]}" \
  "../../apps/java-second/."
docker push "${DOCKERHUB_NAME}/${javasecond[name]}"

### NET ###

# First
docker build \
  --tag "${DOCKERHUB_NAME}/${dotnetfirst[name]}" \
  "../../apps/dotnet-first/dotnet-first/."
docker push "${DOCKERHUB_NAME}/${dotnetfirst[name]}"

# Second
docker build \
  --tag "${DOCKERHUB_NAME}/${dotnetsecond[name]}" \
  "../../apps/dotnet-second/dotnet-second/."
docker push "${DOCKERHUB_NAME}/${dotnetsecond[name]}"

### Simulator ###

# Bash
docker build \
  --tag "${DOCKERHUB_NAME}/${simulatorbash[name]}" \
  "../../apps/simulator-bash/."
docker push "${DOCKERHUB_NAME}/${simulatorbash[name]}"

# # Go
# docker build \
#   --tag "${DOCKERHUB_NAME}/${simulator[name]}" \
#   "../../apps/simulator-go/."
# docker push "${DOCKERHUB_NAME}/${simulator[name]}"
#######

# #############
# ### Pixie ###
# #############
# helm repo add newrelic https://helm-charts.newrelic.com && helm repo update && \
# kubectl create namespace "monitoring" ; helm upgrade newrelic-bundle newrelic/nri-bundle \
#   --install \
#   --wait \
#   --debug \
#   --set global.licenseKey=$NEWRELIC_LICENSE_KEY \
#   --set global.cluster=$clusterName \
#   --namespace="monitoring" \
#   --set newrelic-infrastructure.privileged=true \
#   --set global.lowDataMode=true \
#   --set ksm.enabled=true \
#   --set kubeEvents.enabled=true \
#   --set newrelic-pixie.enabled=true \
#   --set newrelic-pixie.apiKey=$PIXIE_API_KEY \
#   --set pixie-chart.enabled=true \
#   --set pixie-chart.deployKey=$PIXIE_DEPLOY_KEY \
#   --set pixie-chart.clusterName=$clusterName 
# #########

################################
### NGINX Ingress Controller ###
################################
helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx
helm repo update
helm upgrade nginx-ingress \
  --install \
  --wait \
  --debug \
  --create-namespace \
  --namespace "nginx" \
  --set controller.service.annotations."service\.beta\.kubernetes\.io/azure-load-balancer-health-probe-request-path"=/healthz \
  "ingress-nginx/ingress-nginx"
#########

######################
### Otel Collector ###
######################
helm upgrade ${otelcollector[name]} \
  --install \
  --wait \
  --debug \
  --create-namespace \
  --namespace ${otelcollector[namespace]} \
  --set newRelicLicenseKey=$NEWRELIC_LICENSE_KEY \
  --set name=${otelcollector[name]} \
  --set namespace=${otelcollector[namespace]} \
  --set grpcPort=${otelcollector[grpcPort]} \
  --set httpPort=${otelcollector[httpPort]} \
  --set fluentPort=${otelcollector[fluentPort]} \
  --set newRelicOtlpGrpcEndpoint=$newRelicOtlpGrpcEndpoint \
  "../charts/otelcollector"
#########

##################
### Fluent Bit ###
##################
helm upgrade ${fluentbit[name]} \
  --install \
  --wait \
  --debug \
  --create-namespace \
  --namespace ${fluentbit[namespace]} \
  --set namespace=${fluentbit[namespace]} \
  "../charts/fluentbit"
#########

##################
### Prometheus ###
##################

# Install / upgrade Helm deployment
helm upgrade ${prometheus[name]} \
  --install \
  --wait \
  --debug \
  --create-namespace \
  --namespace ${prometheus[namespace]} \
  --set kubeStateMetrics.enabled=true \
  --set nodeExporter.enabled=true \
  --set nodeExporter.tolerations[0].effect="NoSchedule" \
  --set nodeExporter.tolerations[0].operator="Exists" \
  --set newrelic.scrape_case="nodes_and_namespaces" \
  --set server.remoteWrite[0].url=${prometheus[newrelicEndpoint]} \
  --set server.remoteWrite[0].bearer_token=$NEWRELIC_LICENSE_KEY \
  "../charts/prometheus"

#################
### Java Apps ###
#################

# First
helm upgrade ${javafirst[name]} \
  --install \
  --wait \
  --debug \
  --create-namespace \
  --namespace ${javafirst[namespace]} \
  --set dockerhubName=$DOCKERHUB_NAME \
  --set name=${javafirst[name]} \
  --set namespace=${javafirst[namespace]} \
  --set replicas=${javafirst[replicas]} \
  --set port=${javafirst[port]} \
  --set otelServiceName=${javafirst[name]} \
  --set otelExporterOtlpEndpoint=${otelcollector[grpcEndpoint]} \
  "../charts/java-first"

# Second
helm upgrade ${javasecond[name]} \
  --install \
  --wait \
  --debug \
  --create-namespace \
  --namespace ${javasecond[namespace]} \
  --set dockerhubName=$DOCKERHUB_NAME \
  --set name=${javasecond[name]} \
  --set namespace=${javasecond[namespace]} \
  --set replicas=${javasecond[replicas]} \
  --set port=${javasecond[port]} \
  --set otelServiceName=${javasecond[name]} \
  --set otelExporterOtlpEndpoint=${otelcollector[grpcEndpoint]} \
  "../charts/java-second"
#########

###################
### Dotnet Apps ###
###################

# First
helm upgrade ${dotnetfirst[name]} \
  --install \
  --wait \
  --debug \
  --create-namespace \
  --namespace ${dotnetfirst[namespace]} \
  --set dockerhubName=$DOCKERHUB_NAME \
  --set name=${dotnetfirst[name]} \
  --set namespace=${dotnetfirst[namespace]} \
  --set replicas=${dotnetfirst[replicas]} \
  --set port=${dotnetfirst[port]} \
  --set portPrometheus=${dotnetfirst[portPrometheus]} \
  --set otelServiceName=${dotnetfirst[name]} \
  --set otelExporterOtlpEndpoint=${otelcollector[grpcEndpoint]} \
  "../charts/dotnet-first"

# Second
helm upgrade ${dotnetsecond[name]} \
  --install \
  --wait \
  --debug \
  --create-namespace \
  --namespace ${dotnetsecond[namespace]} \
  --set dockerhubName=$DOCKERHUB_NAME \
  --set name=${dotnetsecond[name]} \
  --set namespace=${dotnetsecond[namespace]} \
  --set replicas=${dotnetsecond[replicas]} \
  --set port=${dotnetsecond[port]} \
  --set portPrometheus=${dotnetsecond[portPrometheus]} \
  --set otelServiceName=${dotnetsecond[name]} \
  --set otelExporterOtlpEndpoint=${otelcollector[grpcEndpoint]} \
  "../charts/dotnet-second"
#########

#################
### Simulator ###
#################

# Bash
helm upgrade ${simulatorbash[name]} \
  --install \
  --wait \
  --debug \
  --create-namespace \
  --namespace ${simulatorbash[namespace]} \
  --set dockerhubName=$DOCKERHUB_NAME \
  --set name=${simulatorbash[name]} \
  --set imageName=${simulatorbash[name]} \
  --set namespace=${simulatorbash[namespace]} \
  --set replicas=${simulatorbash[replicas]} \
  "../charts/simulator-bash"
#########

# # Go
# helm upgrade ${simulatorgo[name]} \
#   --install \
#   --wait \
#   --debug \
#   --create-namespace \
#   --namespace ${simulatorgo[namespace]} \
#   --set dockerhubName=$DOCKERHUB_NAME \
#   --set name=${simulatorgo[name]} \
#   --set imageName=${simulatorgo[name]} \
#   --set namespace=${simulatorgo[namespace]} \
#   --set port=${simulatorgo[port]} \
#   "../charts/simulator-go"
# #########