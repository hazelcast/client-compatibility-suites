cmake_minimum_required(VERSION 3.10)

project(CppHazelcastCloudTest)

set(CMAKE_CXX_STANDARD 11)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

set(CMAKE_MODULE_PATH ${CMAKE_MODULE_PATH}
                      "${CMAKE_CURRENT_SOURCE_DIR}/cmake/Modules/")

find_package(hazelcast-cpp-client REQUIRED)
message(
  STATUS
    "Found hazelcast-cpp-client version \"${hazelcast-cpp-client_VERSION}\"")

find_package(GTest 1.10 REQUIRED)
find_package(Thrift 0.13 REQUIRED)

# Generated Remote Controller sources
set(SOURCES
    rc/RemoteController.h rc/RemoteController.cpp
    rc/remote_controller_types.cpp rc/remote_controller_types.h
    rc/remote_controller_constants.h rc/remote_controller_constants.cpp)

# Project sources
list(APPEND SOURCES util.hpp util.cpp some_test.cpp other_test.cpp)

add_executable(cloud_tests ${SOURCES})
target_link_libraries(
  cloud_tests PRIVATE hazelcast-cpp-client::hazelcast-cpp-client thrift::thrift
                      GTest::gtest GTest::gtest_main)
