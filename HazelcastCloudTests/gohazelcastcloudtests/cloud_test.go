package gohazelcastcloudtests

import (
	"context"
	"fmt"
	"github.com/hazelcast/hazelcast-go-client"
	"github.com/hazelcast/hazelcast-go-client/types"
	"github.com/stretchr/testify/assert"
	it "gohazelcastcloudtests/remote-controller"
	"os"
	"testing"
	"time"
)

var sslEnabledCluster *it.CloudCluster
var sslDisabledCluster *it.CloudCluster

func TestMain(m *testing.M) {
	it.Rc = it.CreateDefaultRemoteController()
	err := it.LoginToHazelcastCloudUsingEnvironment(context.Background())
	if err != nil {
		return
	}
	if err := setupForStandardClusterTests(); err != nil {
		panic(err.Error())
	}
	defer shutdownForStandardClusterTests()
	m.Run()
}

func setupForStandardClusterTests() error {
	var err error
	sslEnabledCluster, err = it.CreateHazelcastCloudStandardCluster(context.Background(), os.Getenv("HZ_VERSION"), true)
	sslDisabledCluster, err = it.CreateHazelcastCloudStandardCluster(context.Background(), os.Getenv("HZ_VERSION"), false)
	return err
}

func shutdownForStandardClusterTests() {
	err := it.DeleteHazelcastCloudCluster(context.Background(), sslEnabledCluster.ID)
	if err != nil {
		panic(err.Error())
	}
	err = it.DeleteHazelcastCloudCluster(context.Background(), sslDisabledCluster.ID)
	if err != nil {
		panic(err.Error())
	}
}

func TestForSslEnabledCluster(t *testing.T) {
	table := []struct {
		name          string
		isSmartClient bool
	}{
		{"SmartClientWithSslCluster", true},
		{"UniscoketClientWithSslCluster", false},
	}

	for _, tc := range table {
		t.Run(tc.name, func(t *testing.T) {
			ctx := context.Background()
			client, _ := hazelcast.StartNewClientWithConfig(ctx, CreateClientConfigWithSsl(sslEnabledCluster.NameForConnect, sslEnabledCluster.Token, sslEnabledCluster.CertificatePath, sslEnabledCluster.TlsPassword, tc.isSmartClient))
			defer client.Shutdown(ctx)
			givenMap, _ := client.GetMap(ctx, "MapFor"+tc.name)
			MapPutGetAndVerify(t, givenMap)
			fmt.Println("Scaling up cluster from 2 node to 4")
			err := it.SetHazelcastCloudClusterMemberCount(context.Background(), sslEnabledCluster.ID, 4)
			if err != nil {
				panic(err.Error())
			}
			MapPutGetAndVerify(t, givenMap)
			fmt.Println("Scaling down cluster from 4 node to 2")
			err = it.SetHazelcastCloudClusterMemberCount(context.Background(), sslEnabledCluster.ID, 2)
			if err != nil {
				panic(err.Error())
			}
			MapPutGetAndVerify(t, givenMap)
			fmt.Println("Stopping cluster")
			_, err = it.StopHazelcastCloudCluster(context.Background(), sslEnabledCluster.ID)
			if err != nil {
				panic(err.Error())
			}
			fmt.Println("Resuming cluster")
			_, err = it.ResumeHazelcastCloudCluster(context.Background(), sslEnabledCluster.ID)
			if err != nil {
				panic(err.Error())
			}
			MapPutGetAndVerify(t, givenMap)
		})
	}
}

func TestForSslDisabledCluster(t *testing.T) {
	table := []struct {
		name          string
		isSmartClient bool
	}{
		{"SmartClientWithoutSslCluster", true},
		{"UniscoketClientWithoutSslCluster", false},
	}

	for _, tc := range table {
		t.Run(tc.name, func(t *testing.T) {
			ctx := context.Background()
			client, _ := hazelcast.StartNewClientWithConfig(ctx, CreateClientConfigWithoutSsl(sslDisabledCluster.NameForConnect, sslDisabledCluster.Token, tc.isSmartClient))
			defer client.Shutdown(ctx)
			givenMap, _ := client.GetMap(ctx, "MapFor"+tc.name)
			MapPutGetAndVerify(t, givenMap)
			fmt.Println("Scaling up cluster from 2 node to 4")
			err := it.SetHazelcastCloudClusterMemberCount(context.Background(), sslDisabledCluster.ID, 4)
			if err != nil {
				panic(err.Error())
			}
			MapPutGetAndVerify(t, givenMap)
			fmt.Println("Scaling down cluster from 4 node to 2")
			err = it.SetHazelcastCloudClusterMemberCount(context.Background(), sslDisabledCluster.ID, 2)
			if err != nil {
				panic(err.Error())
			}
			MapPutGetAndVerify(t, givenMap)
			fmt.Println("Stopping cluster")
			_, err = it.StopHazelcastCloudCluster(context.Background(), sslDisabledCluster.ID)
			if err != nil {
				panic(err.Error())
			}
			fmt.Println("Resuming cluster")
			_, err = it.ResumeHazelcastCloudCluster(context.Background(), sslDisabledCluster.ID)
			if err != nil {
				panic(err.Error())
			}
			time.Sleep(5 * time.Second)
			MapPutGetAndVerify(t, givenMap)
		})
	}
}

func TestForSslEnabledClusterWithoutCertificates(t *testing.T) {
	table := []struct {
		name          string
		isSmartClient bool
	}{
		{"SmartClientConnectionTestToSslClusterWithoutCertificates", true},
		{"UnisocketClientConnectionTestToSslClusterWithoutCertificates", false},
	}
	for _, tc := range table {
		t.Run(tc.name, func(t *testing.T) {
			ctx := context.Background()
			config := hazelcast.NewConfig()
			config = CreateClientConfigWithoutSsl(sslEnabledCluster.NameForConnect, sslEnabledCluster.Token, tc.isSmartClient)
			config.Cluster.ConnectionStrategy.Timeout = types.Duration(10 * time.Second)
			_, err := hazelcast.StartNewClientWithConfig(ctx, config)
			value := true
			if err != nil {
				value = true
			}
			assert.True(t, value, "Client shouldn't be able to connect ssl enabled cluster without certificates")
		})
	}
}
