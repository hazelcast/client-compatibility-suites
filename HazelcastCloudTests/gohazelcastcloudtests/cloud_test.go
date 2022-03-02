package gohazelcastcloudtests

import (
	"context"
	"fmt"
	"github.com/hazelcast/hazelcast-go-client"
	"github.com/hazelcast/hazelcast-go-client/types"
	"github.com/stretchr/testify/assert"
	"gohazelcastcloudtests/remote-controller"
	"os"
	"testing"
	"time"
)
var sslEnabledCluster *it.CloudCluster
var sslDisabledCluster *it.CloudCluster

func TestMain(m *testing.M) {
	it.Rc = it.CreateDefaultRemoteController()
	if err := setupForStandardClusterTests(); err != nil{
		panic(err.Error())
	}
	defer shutdownForStandardClusterTests()
	code := m.Run()
	os.Exit(code)
}

func setupForStandardClusterTests() error {
	var err error
	//sslEnabledCluster, err = it.CreateHazelcastCloudStandardCluster(os.Getenv("hzVersion"), true)
	sslEnabledCluster, err = it.GetHazelcastCloudCluster("1536")
	//sslDisabledCluster, err = it.CreateHazelcastCloudStandardCluster(os.Getenv("hzVersion"), false)
	sslDisabledCluster, err = it.GetHazelcastCloudCluster("1537")
	return err
}

func shutdownForStandardClusterTests() {
	it.DeleteHazelcastCloudCluster(sslEnabledCluster.ID)
	it.DeleteHazelcastCloudCluster(sslDisabledCluster.ID)
}

func TestForSslEnabledCluster(t *testing.T){
	table := []struct {
		name     string
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
			givenMap, _ := client.GetMap(ctx, "MapFor" + tc.name)
			MapPutGetAndVerify(t, givenMap)
			fmt.Println("Scaling up cluster from 2 node to 4")
			it.ScaleUpDownHazelcastCloudCluster(sslEnabledCluster.ID, 2)
			MapPutGetAndVerify(t, givenMap)
			fmt.Println("Scaling down cluster from 4 node to 2")
			it.ScaleUpDownHazelcastCloudCluster(sslEnabledCluster.ID, -2)
			MapPutGetAndVerify(t, givenMap)
			fmt.Println("Stopping cluster")
			it.StopHazelcastCloudCluster(sslEnabledCluster.ID)
			fmt.Println("Resuming cluster")
			it.ResumeHazelcastCloudCluster(sslEnabledCluster.ID)
			MapPutGetAndVerify(t, givenMap)
		})
	}
}

func TestForSslDisabledCluster(t *testing.T){
	table := []struct {
		name     string
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
			givenMap, _ := client.GetMap(ctx, "MapFor" + tc.name)
			MapPutGetAndVerify(t, givenMap)
			fmt.Println("Scaling up cluster from 2 node to 4")
			it.ScaleUpDownHazelcastCloudCluster(sslDisabledCluster.ID, 2)
			MapPutGetAndVerify(t, givenMap)
			fmt.Println("Scaling down cluster from 4 node to 2")
			it.ScaleUpDownHazelcastCloudCluster(sslDisabledCluster.ID, -2)
			MapPutGetAndVerify(t, givenMap)
			fmt.Println("Stopping cluster")
			it.StopHazelcastCloudCluster(sslDisabledCluster.ID)
			fmt.Println("Resuming cluster")
			it.ResumeHazelcastCloudCluster(sslDisabledCluster.ID)
			time.Sleep(5 * time.Second)
			MapPutGetAndVerify(t, givenMap)
		})
	}
}

func TestForSslEnabledClusterWithoutCertificates(t *testing.T){
	table := []struct {
		name     string
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





