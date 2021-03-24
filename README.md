# Client Compatibility Tests

This repository contains Github Actions workflow files and
utility scripts used by them to test the following scenarios:

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

## 3.x Client Compatibility Test Suite

Please use the ``master-3.x`` branch for the old client
compatibility test suite. 
