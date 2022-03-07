#include "util.hpp"

#include <gtest/gtest.h>

#include <hazelcast/client/hazelcast_client.h>

namespace {


TEST(SomeTest, SomeCase)
{
    auto rc = hazelcast::util::make_remote_controller_client();

    hazelcast::util::rc::CloudCluster cc;
    rc.createHazelcastCloudStandardCluster(cc, "5.0.2-2", false);

    //    std::cout << "name: " << cc.nameForConnect << std::endl;
    //    std::cout << "token: " << cc.token << std::endl;

    hazelcast::client::client_config cfg;
    cfg.set_cluster_name("ua-1551");
    cfg.get_network_config().get_cloud_config().enabled = true;
    cfg.get_network_config().get_cloud_config().discovery_token =
      "bFRPjepP1LWXvoQ62mS6gu6Qwm28uG4wMd6Ra1SCwHQrBE3RKw";
    cfg.set_property("hazelcast.client.cloud.url", "uat.hazelcast.cloud");

    auto cli = hazelcast::new_client(std::move(cfg)).get();

    auto map = cli.get_map("some-map").get();

    map->put<std::string, std::string>("foo", "bar").get();

    EXPECT_EQ(std::string("bar"),
              (map->get<std::string, std::string>("foo").get()));
}



} // namespace
