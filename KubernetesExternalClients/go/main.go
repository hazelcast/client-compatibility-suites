package main

import (
	"context"
	"fmt"
	"log"
	"math/rand"
	"time"

	"github.com/hazelcast/hazelcast-go-client"
)

func main() {
	config := hazelcast.Config{}
	config.Cluster.Discovery.UsePublicIP = true
	config.Cluster.Network.SetAddresses("<EXTERNAL-IP>:5701")
	ctx := context.Background()
	client, err := hazelcast.StartNewClientWithConfig(ctx, config)
	if err != nil {
		log.Fatal(err)
	}
	log.Println("Successful connection!")
	log.Println("Starting to fill the map with random entries.")
	m, err := client.GetMap(ctx, "mapForGo")
	if err != nil {
		log.Fatal(err)
	}
	numberOfLoop := 0
	for numberOfLoop < 120{
		num := rand.Intn(100_000)
		key := fmt.Sprintf("key-%d", num)
		value := fmt.Sprintf("value-%d", num)
		if _, err = m.Put(ctx, key, value); err != nil {
			log.Println("ERR:", err.Error())
		} else {
			if mapSize, err := m.Size(ctx); err != nil {
				log.Println("ERR:", err.Error())
			} else {
				log.Println("Current map size:", mapSize)
				time.Sleep(1 * time.Second)
			}
		}
		numberOfLoop++
	}
}
