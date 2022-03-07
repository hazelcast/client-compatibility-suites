cmake_minimum_required(VERSION 3.10)

project(CppHazelcastCloudTest)

find_package(hazelcast-cpp-client REQUIRED)
message(
  STATUS
    "Found hazelcast-cpp-client version \"${hazelcast-cpp-client_VERSION}\"")

find_package(GTest 1.11 REQUIRED)
find_package(Thrift 0.15 REQUIRED)

add_custom_command(
  OUTPUT gen-cpp/RemoteController.h gen-cpp/RemoteController.cpp
         gen-cpp/remote_controller_types.cpp gen-cpp/remote_controller_types.h
  COMMAND ${THRIFT_COMPILER} --gen cpp:no_skeleton
          ${CMAKE_CURRENT_SOURCE_DIR}/remote_controller.thrift
  DEPENDS ${CMAKE_CURRENT_SOURCE_DIR}/remote_controller.thrift
  COMMENT "Generating remote-controller"
  VERBATIM)

# Generated Remote Controller sources
set(SOURCES
    ${CMAKE_CURRENT_BINARY_DIR}/gen-cpp/RemoteController.h
    ${CMAKE_CURRENT_BINARY_DIR}/gen-cpp/RemoteController.cpp
    ${CMAKE_CURRENT_BINARY_DIR}/gen-cpp/remote_controller_types.cpp
    ${CMAKE_CURRENT_BINARY_DIR}/gen-cpp/remote_controller_types.h)

# Project sources
list(APPEND SOURCES util.hpp util.cpp some_test.cpp)

add_executable(cloud_tests ${SOURCES})
target_link_libraries(
  cloud_tests PRIVATE hazelcast-cpp-client::hazelcast-cpp-client thrift::thrift
                      GTest::gtest GTest::gtest_main)
target_include_directories(cloud_tests
                           PRIVATE ${CMAKE_CURRENT_BINARY_DIR}/gen-cpp)
