package gohazelcastcloudtests

import (
	"context"
	"crypto/tls"
	"fmt"
	"github.com/hazelcast/hazelcast-go-client"
	"github.com/hazelcast/hazelcast-go-client/types"
	"github.com/stretchr/testify/assert"
	"math/rand"
	"path/filepath"
	"testing"
	"time"
)

func MapPutGetAndVerify(t *testing.T, givenMap *hazelcast.Map){
	fmt.Println("Putting random keys and values to map and then verify")
	ctx := context.Background()
	givenMap.Clear(ctx)
	rand.Seed(time.Now().UTC().UnixNano())
	iterationCounter := 0
	for iterationCounter < 20{
		randKey := rand.Intn(100000)
		_, err := givenMap.Put(ctx, fmt.Sprintf("key%d", randKey), fmt.Sprintf("value%d", randKey))
		if err != nil {
			panic(err)
		}
		value, err := givenMap.Get(ctx, fmt.Sprintf("key%d", randKey))
		if err != nil {
			panic(err)
		}
		assert.Equal(t, fmt.Sprintf("value%d", randKey), value, "Gotten value is not expected, another value was put the map")
		iterationCounter++
	}
	size, err := givenMap.Size(ctx)
	if err != nil {
		panic(err)
	}
	assert.Equal(t, size, 20, "map size should be 20")
}

func CreateClientConfigWithSsl(clusterName string, token string, certificatesPath string, password string, smartRouting bool) hazelcast.Config {
	config := hazelcast.NewConfig()
	config = CreateClientConfigWithoutSsl(clusterName, token, smartRouting)
	caFile, err := filepath.Abs(filepath.Join(certificatesPath, "ca.pem"))
	if err != nil {
		panic(err)
	}
	certFile, err := filepath.Abs(filepath.Join(certificatesPath, "cert.pem"))
	if err != nil {
		panic(err)
	}
	keyFile, err := filepath.Abs(filepath.Join(certificatesPath, "key.pem"))
	if err != nil {
		panic(err)
	}
	config.Cluster.Network.SSL.Enabled = true
	config.Cluster.Network.SSL.SetTLSConfig(&tls.Config{ServerName: "hazelcast.cloud"})
	err = config.Cluster.Network.SSL.SetCAPath(caFile)
	if err != nil {
		panic(err)
	}
	err = config.Cluster.Network.SSL.AddClientCertAndEncryptedKeyPath(certFile, keyFile, password)
	return config
}

func CreateClientConfigWithoutSsl(clusterName string, token string, smartRouting bool) hazelcast.Config {
	config := hazelcast.NewConfig()
	config.Cluster.Unisocket = !smartRouting
	config.Cluster.Name = clusterName
	config.Cluster.Cloud.Enabled = true
	config.Cluster.Cloud.Token = token
	config.Stats.Enabled = true
	config.Stats.Period = types.Duration(time.Second)
	return config
}