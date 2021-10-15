#include <iostream>
#include <thread>
#include <chrono>

#include <hazelcast/client/hazelcast_client.h>

int main() {
    hazelcast::client::client_config config;
    
    config.get_network_config().use_public_address(true);
    config.get_network_config().add_address({"<EXTERNAL-IP>", 5701});

    auto hz = hazelcast::new_client(std::move(config)).get();

    std::cout << "Successful connection!" << std::endl;
    std::cout << "Starting to fill the map with random entries." << std::endl;

    auto map = hz.get_map("mapForCpp").get();
    for (int i = 0; i < 120; i++) {
        int random_key = rand() % 100000;
        try {
            map->put("key-" + std::to_string(random_key), "value-" + std::to_string(random_key));
            
            std::cout << "Current map size: " << map->size().get() << std::endl;
        } catch (const std::exception& e) {
            std::cout << e.what() << std::endl;
        }
    }

    return 0;
}
