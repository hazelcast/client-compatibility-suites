module kubernetesTest/project

go 1.16

require (
	github.com/hazelcast/hazelcast-go-client v1.0.0
)

replace github.com/hazelcast/hazelcast-go-client => ../go/clientSourceCode
