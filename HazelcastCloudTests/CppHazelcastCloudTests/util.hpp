#pragma once

#include <RemoteController.h>

namespace hazelcast {
namespace util {

namespace rc = hazelcast::client::test::remote;

rc::RemoteControllerClient
make_remote_controller_client();

} // namespace util
} // namespace hazelcast
