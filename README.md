# Client Compatibility Tests

This repository contains Github Actions workflow files and
utility scripts used by them to test the following suites:

## Backward Compatibility Tests

1. When a new IMDG server is ready to be released, and we want to 
verify that it is backward compatible with released clients of 
different languages.  

2. When a new client is ready to be released, and we want to verify
that it is backward compatible with released IMDG servers.
  
Github Actions for the following test scenarios are meant to be
started manually. To use them, navigate to the ``Actions`` tab of 
the repository, select the workflow you are interested in to test, 
supply the necessary inputs and run the workflow.

The scripts will use [rel-scripts](https://github.com/hazelcast/rel-scripts)
repository to get information regarding the released clients or servers.

## Kubernetes Compatibility Tests

1. When a new client is ready to release, we can verify it is working
properly as a docker image in Kubernetes. Docker image is created with the
branch set as an input to workflow. Cluster is created with
helm chart. Workflow is also verifying cluster 
scale up, down, delete scenarios.

2. When a new client is ready to release, we can verify it is able to connect
a cluster which is running on GKE.
