#include "util.hpp"

#include <thrift/protocol/TBinaryProtocol.h>
#include <thrift/transport/TBufferTransports.h>
#include <thrift/transport/TSocket.h>

namespace hazelcast {
namespace util {

rc::RemoteControllerClient
make_remote_controller_client()
{
    using apache::thrift::protocol::TBinaryProtocol;
    using apache::thrift::transport::TFramedTransport;
    using apache::thrift::transport::TSocket;

    auto socket = std::make_shared<TSocket>("localhost", 9701);
    auto transport = std::make_shared<TFramedTransport>(socket);
    auto protocol = std::make_shared<TBinaryProtocol>(transport);

    socket->open();

    return { protocol };
}

rc::RemoteControllerClient rc_cli{make_remote_controller_client()};

} // namespace util
} // namespace hazelcast
