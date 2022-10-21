# Chart configuration

The Prometheus server is configured to run in agent mode in order to allocate the least amount of resources from the cluster. Thereby, it acts as a forwarder as long as the remote backend endpoint is available. If not, it stores the scraped metrics for a while and then drops.

## Scraping parameters

By default, a `ClusterRole` is assigned which basically lets Prometheus scrape everything possible. If you are interested in only some metrics, applying `keep` or `drop` rules still does not change the fact of Prometheus scraping everything first and applying the regex afterwards. Of course, this represents an unnecessary memory usage and therefore is costly.

On the other hand, it is possible to limit the reach of Prometheus by manipulating it's given RBAC so that it does not scrape more than what you actually require. To do that, 2 parameters (`newrelic.scrape_case` & `newrelic.namespaces`) are introduced. Principally, they identify what to scrape and depending on the given input various Kubernetes objects (among `ClusterRole`, `ClusterRoleBinding`, `Role` and `RoleBinding`) with specific configurations are created.

## Additional deployments

If you want to install kube-state-metrics and node exporter, you can set the variables `kubeStateMetrics.enabled` and `nodeExporter.enabled` to true, respectively.

## Forwarding to New Relic

To forward the metrics to New Relic accounts, you should set the following variables:
```yaml
server:
  remoteWrite:
    - url: <new_relic_prometheus_endpoint_1>
      bearer_token: "NEWRELIC_LICENSE_KEY_1"
    - url: <new_relic_prometheus_endpoint_1>
      bearer_token: "NEWRELIC_LICENSE_KEY_2"
```

where the endpoints for US and EU accounts are:
- `https://metric-api.newrelic.com/prometheus/v1/write?prometheus_server=<name>"`
- `https://metric-api.eu.newrelic.com/prometheus/v1/write?prometheus_server=<name>"`

## Metric processing

If you want to enrich, transform, drop or keep some metrics, the typical regular expressions (regex) are applicable just as in any Prometheus configuration.

```yaml
server:
  remoteWrite:
    - url: <new_relic_prometheus_endpoint_1>
      bearer_token: "NEWRELIC_LICENSE_KEY_1"
      write_relabel_configs:
        - source_labels: [namespace]
          regex: mynamespace
          action: keep
```

## Example Scraping Cases

### Example 1

You want to scrape from every Kubernetes resource:
- nodes
- nodes/proxy
- nodes/metrics
- services
- endpoints
- pods
- ingresses
- configmaps

**Input**
```yaml
newrelic:
  scrape_case: nodes_and_namespaces
  namespaces: []
```

**Output**
- a `ClusterRole` with access on all resources
- a `ClusterRoleBinding` for the `ClusterRole`

### Example 2

You want to scrape from nodes:
- nodes
- nodes/proxy
- nodes/metrics

and only from specific namespaces:
- services
- endpoints
- pods
- ingresses
- configmaps

**Input**
```yaml
newrelic:
  scrape_case: nodes_and_namespaces
  namespaces:
    - mynamespace1
    - mynamespace2
```

**Output**
- a `ClusterRole` with access on nodes, nodes/proxy and nodes/metrics
- a `ClusterRoleBinding` for the `ClusterRole`
- a `Role` in each given namespace with access on services, endpoints, pods, ingresses and configmaps
- a `RoleBinding` in each given namespace for the corresponding `Role`
- a filter per each Prometheus namespaced `job` in `prometheus.yml`

### Example 3

You want to scrape only the nodes:
- nodes
- nodes/proxy
- nodes/metrics

**Input**
```yaml
newrelic:
  scrape_case: just_nodes
  namespaces: # will be ignored
```

**Output**
- a `ClusterRole` with access on just nodes, nodes/proxy and nodes/metrics
- a `ClusterRoleBinding` for the `ClusterRole`

### Example 4

You want to scrape from all:
- services
- endpoints
- pods
- ingresses
- configmaps

**Input**
```yaml
newrelic:
  scrape_case: just_namespaces
  namespaces: []
```

**Output**
- a `ClusterRole` with access on services, endpoints, pods, ingresses and configmaps
- a `ClusterRoleBinding` for the `ClusterRole`

### Example 5

You want to scrape from specific namespaces:
- services
- endpoints
- pods
- ingresses
- configmaps

**Input**
```yaml
newrelic:
  scrape_case: just_namespaces
  namespaces:
    - mynamespace1
    - mynamespace2
```

**Output**
- a `Role` in each given namespace with access on services, endpoints, pods, ingresses and configmaps
- a `RoleBinding` in each given namespace for the corresponding `Role`
- a filter per each Prometheus namespaced `job` in `prometheus.yml`

## Example Forwarding Cases

### Example 1

You have 1 ops team and 2 dev teams where each of them have their apps running in dedicated namespaces. You have already installed New Relic infrastructure agent which is reporting to your ops team.

**What?**

The dev teams want to know:
- their infrastructure related performance metrics (CPU, MEM, STO...)

**Why?**

- Container metrics are to be scraped from the cAdvisor which is on the node level

**How?**

- scrape only from the nodes
- filter the metrics according to individual namespaces
- forward the filtered metrics to corresponding accounts

```yaml
newrelic:
  scrape_case: just_nodes
server:
  remoteWrite:
    - url: <new_relic_prometheus_endpoint>
      bearer_token: "NEWRELIC_LICENSE_KEY_DEV_TEAM_1"
      write_relabel_configs:
        - source_labels: [namespace]
          regex: dev-team-1
          action: keep
    - url: <new_relic_prometheus_endpoint>
      bearer_token: "NEWRELIC_LICENSE_KEY_DEV_TEAM_2"
      write_relabel_configs:
        - source_labels: [namespace]
          regex: dev-team-2
          action: keep
```

## Example 2

You have 2 dev teams where each of them have their apps running in dedicated namespaces.

**What?**

The dev teams want to know:
- the custom metrics that they expose from their applications.

**Why?**

- Applications metrics are to be scraped from the namespaces and their Kubernetes manifests (`service`, `pod` or `endpoint`) are to be updated with Prometheus annotations (`prometheus.io/scrape: 'true'`, `prometheus.io/path: '/metrics'`, `prometheus.io/port: '8080'`)

**How?**

- scrape only from the relevant namespaces 
- filter the metrics according to individual namespaces
- forward the filtered metrics to corresponding accounts

```yaml
newrelic:
  scrape_case: just_namespaces
  namespaces:
    - dev-team-1
    - dev-team-2
server:
  remoteWrite:
    - url: <new_relic_prometheus_endpoint>
      bearer_token: "NEWRELIC_LICENSE_KEY_DEV_TEAM_1"
      write_relabel_configs:
        - source_labels: [namespace]
          regex: dev-team-1
          action: keep
    - url: <new_relic_prometheus_endpoint>
      bearer_token: "NEWRELIC_LICENSE_KEY_DEV_TEAM_2"
      write_relabel_configs:
        - source_labels: [namespace]
          regex: dev-team-2
          action: keep
```

## Example 3

You have 1 ops team and 2 dev teams where each of them have their apps running in dedicated namespaces. You have already installed New Relic infrastructure agent which is reporting to your ops team.

**What?**

The dev teams want to know:
- their infrastructure related performance metrics (CPU, MEM, STO...)
- the custom metrics that they expose from their applications

**Why?**

- Container metrics are to be scraped from the cAdvisor which is on the node level
- Applications metrics are to be scraped from the namespaces and their Kubernetes manifests (`service`, `pod` or `endpoint`) are to be updated with Prometheus annotations (`prometheus.io/scrape: 'true'`, `prometheus.io/path: '/metrics'`, `prometheus.io/port: '8080'`)

**How?**

- scrape from the nodes
- scrape only from the relevant namespaces 
- filter the metrics according to individual namespaces
- forward the filtered metrics to corresponding accounts

```yaml
newrelic:
  scrape_case: nodes_and_namespaces
  namespaces:
    - dev-team-1
    - dev-team-2
server:
  remoteWrite:
    - url: <new_relic_prometheus_endpoint>
      bearer_token: "NEWRELIC_LICENSE_KEY_DEV_TEAM_1"
      write_relabel_configs:
        - source_labels: [namespace]
          regex: dev-team-1
          action: keep
    - url: <new_relic_prometheus_endpoint>
      bearer_token: "NEWRELIC_LICENSE_KEY_DEV_TEAM_2"
      write_relabel_configs:
        - source_labels: [namespace]
          regex: dev-team-2
          action: keep
```
