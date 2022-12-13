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
	sslEnabledCluster, err = it.CreateCloudCluster(context.Background(), os.Getenv("HZ_VERSION"), true)
	sslDisabledCluster, err = it.CreateCloudCluster(context.Background(), os.Getenv("HZ_VERSION"), false)
	return err
}

func shutdownForStandardClusterTests() {
	err := it.DeleteCloudCluster(context.Background(), sslEnabledCluster.ID)
	if err != nil {
		panic(err.Error())
	}
	err = it.DeleteCloudCluster(context.Background(), sslDisabledCluster.ID)
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
			client, _ := hazelcast.StartNewClientWithConfig(ctx, CreateClientConfigWithSsl(sslEnabledCluster.ReleaseName, sslEnabledCluster.Token, sslEnabledCluster.CertificatePath, sslEnabledCluster.TlsPassword, tc.isSmartClient))
			defer client.Shutdown(ctx)
			givenMap, _ := client.GetMap(ctx, "MapFor"+tc.name)
			MapPutGetAndVerify(t, givenMap)
			fmt.Println("Stopping cluster")
			_, err = it.StopCloudCluster(context.Background(), sslEnabledCluster.ID)
			if err != nil {
				panic(err.Error())
			}
			fmt.Println("Resuming cluster")
			_, err = it.ResumeCloudCluster(context.Background(), sslEnabledCluster.ID)
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
			client, _ := hazelcast.StartNewClientWithConfig(ctx, CreateClientConfigWithoutSsl(sslDisabledCluster.ReleaseName, sslDisabledCluster.Token, tc.isSmartClient))
			defer client.Shutdown(ctx)
			givenMap, _ := client.GetMap(ctx, "MapFor"+tc.name)
			MapPutGetAndVerify(t, givenMap)
			fmt.Println("Stopping cluster")
			_, err = it.StopCloudCluster(context.Background(), sslDisabledCluster.ID)
			if err != nil {
				panic(err.Error())
			}
			fmt.Println("Resuming cluster")
			_, err = it.ResumeCloudCluster(context.Background(), sslDisabledCluster.ID)
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
			config = CreateClientConfigWithoutSsl(sslEnabledCluster.ReleaseName, sslEnabledCluster.Token, tc.isSmartClient)
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
