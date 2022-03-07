#include "util.hpp"

#include <gtest/gtest.h>

#include <hazelcast/client/hazelcast_client.h>

namespace {

class SomeFixture : public ::testing::Test
{
public:
    static void SetUpTestSuite() {}

    SomeFixture() {}

    void SetUp() {}
    void TearDown() final {}

    static void TearDownTestSuite() {}
};

TEST_F(SomeFixture, test1) {}

TEST_F(SomeFixture, test2) {}

class SomeParametrizedTest : public ::testing::TestWithParam<std::vector<bool>>
{
public:
    static void SetUpTestSuite()
    {
        hazelcast::util::rc_cli.getHazelcastCloudCluster(cc, "1552");
    }

    hazelcast::client::client_config create_client_config()
    {
        const auto is_smart = GetParam()[0];
        const auto ssl_enabled = GetParam()[1];

        hazelcast::client::client_config cfg;
        cfg.get_connection_strategy_config()
          .get_retry_config()
          .set_cluster_connect_timeout(std::chrono::seconds(2));
        cfg.get_network_config().set_smart_routing(is_smart);
        cfg.set_cluster_name(cc.nameForConnect);
        cfg.get_network_config().get_cloud_config().enabled = true;
        cfg.get_network_config().get_cloud_config().discovery_token = cc.token;
        cfg.set_property("hazelcast.client.cloud.url", "uat.hazelcast.cloud");
        cfg.get_network_config().get_ssl_config().set_enabled(ssl_enabled);
        return std::move(cfg);
    }

    SomeParametrizedTest()
      : cli{ hazelcast::new_client(create_client_config()).get() }
    {}

    void SetUp() {}
    void TearDown() final {}

    static void TearDownTestSuite()
    {
        // rc_cli.deleteHazelcastCloudCluster(cc.id);
    }

    static hazelcast::util::rc::CloudCluster cc;
    hazelcast::client::hazelcast_client cli;
};

hazelcast::util::rc::CloudCluster SomeParametrizedTest::cc;

TEST_P(SomeParametrizedTest, test1)
{
    auto map = cli.get_map("some-map").get();

    map->put<std::string, std::string>("foo", "bar").get();

    EXPECT_EQ(std::string("bar"),
              (map->get<std::string, std::string>("foo").get()));
}
TEST_P(SomeParametrizedTest, test2)
{
    auto map = cli.get_map("some-map").get();

    map->put<std::string, std::string>("foo", "bar").get();

    EXPECT_EQ(std::string("bar"),
              (map->get<std::string, std::string>("foo").get()));
}

INSTANTIATE_TEST_SUITE_P(SomeParametrizedTest,
                         SomeParametrizedTest,
                         ::testing::Values(std::vector<bool>{ false, false },
                                           std::vector<bool>{ true, false },
                                           std::vector<bool>{ false, true },
                                           std::vector<bool>{ true, true }));

TEST(SomeTest, SomeCase)
{
    hazelcast::util::rc::CloudCluster cc;
    // rc.createHazelcastCloudStandardCluster(cc, "5.0.2-2", false);
    hazelcast::util::rc_cli.getHazelcastCloudCluster(cc, "1552");
    //    std::cout << "name: " << cc.nameForConnect << std::endl;
    //    std::cout << "token: " << cc.token << std::endl;

    hazelcast::client::client_config cfg;
    cfg.set_cluster_name(cc.nameForConnect);
    cfg.get_network_config().get_cloud_config().enabled = true;
    cfg.get_network_config().get_cloud_config().discovery_token = cc.token;
    cfg.set_property("hazelcast.client.cloud.url", "uat.hazelcast.cloud");

    auto cli = hazelcast::new_client(std::move(cfg)).get();

    auto map = cli.get_map("some-map").get();

    map->put<std::string, std::string>("foo", "bar").get();

    EXPECT_EQ(std::string("bar"),
              (map->get<std::string, std::string>("foo").get()));
}

} // namespace
