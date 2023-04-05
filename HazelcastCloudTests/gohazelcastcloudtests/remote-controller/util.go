/*
 * Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License")
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

package it

import (
	"context"
	"encoding/json"
	"math/rand"
	"time"

	"github.com/apache/thrift/lib/go/thrift"
	"github.com/hazelcast/hazelcast-go-client/serialization"
)

var Rc *RemoteControllerClient

func init() {
	rand.Seed(time.Now().UnixNano())
}

const SamplePortableFactoryID = 1
const SamplePortableClassID = 1

type SamplePortable struct {
	A string
	B int32
}

func (s SamplePortable) FactoryID() int32 {
	return SamplePortableFactoryID
}

func (s SamplePortable) ClassID() int32 {
	return SamplePortableClassID
}

func (s SamplePortable) WritePortable(writer serialization.PortableWriter) {
	writer.WriteString("A", s.A)
	writer.WriteInt32("B", s.B)
}

func (s *SamplePortable) ReadPortable(reader serialization.PortableReader) {
	s.A = reader.ReadString("A")
	s.B = reader.ReadInt32("B")
}

func (s SamplePortable) Json() serialization.JSON {
	byteArr, err := json.Marshal(s)
	if err != nil {
		panic(err)
	}
	return byteArr
}

type SamplePortableFactory struct {
}

func (f SamplePortableFactory) Create(classID int32) serialization.Portable {
	if classID == SamplePortableClassID {
		return &SamplePortable{}
	}
	return nil
}

func (f SamplePortableFactory) FactoryID() int32 {
	return SamplePortableFactoryID
}

// Must panics if err is not nil
func Must(err error) {
	if err != nil {
		panic(err)
	}
}

// MustValue returns value if err is nil, otherwise it panics.
func MustValue(value interface{}, err error) interface{} {
	if err != nil {
		panic(err)
	}
	return value
}

func CreateDefaultRemoteController() *RemoteControllerClient {
	return CreateRemoteController("localhost:9701")
}

func CreateRemoteController(addr string) *RemoteControllerClient {
	transport := MustValue(thrift.NewTSocketConf(addr, nil)).(*thrift.TSocket)
	bufferedTransport := thrift.NewTBufferedTransport(transport, 4096)
	protocol := thrift.NewTBinaryProtocolConf(bufferedTransport, nil)
	client := thrift.NewTStandardClient(protocol, protocol)
	rc := NewRemoteControllerClient(client)
	Must(transport.Open())
	return rc
}

type TestCluster struct {
	RC          *RemoteControllerClient
	ClusterID   string
	MemberUUIDs []string
	Port        int
}

// Cloud APIs

func LoginToCloudUsingEnvironment(ctx context.Context) error {
	return Rc.LoginToCloudUsingEnvironment(ctx)
}

func LoginToCloud(ctx context.Context, uri string, apiKey string, apiSecret string) error {
	return Rc.LoginToCloud(ctx, uri, apiKey, apiSecret)
}

func CreateCloudCluster(ctx context.Context, hzVersion string, isTlsEnabled bool) (*CloudCluster, error) {
	return Rc.CreateCloudCluster(ctx, hzVersion, isTlsEnabled)
}

func GetCloudCluster(ctx context.Context, clusterID string) (*CloudCluster, error) {
	return Rc.GetCloudCluster(ctx, clusterID)
}

func StopCloudCluster(ctx context.Context, clusterID string) (*CloudCluster, error) {
	return Rc.StopCloudCluster(ctx, clusterID)
}

func ResumeCloudCluster(ctx context.Context, clusterID string) (*CloudCluster, error) {
	return Rc.ResumeCloudCluster(ctx, clusterID)
}

func DeleteCloudCluster(ctx context.Context, clusterID string) error {
	return Rc.DeleteCloudCluster(ctx, clusterID)
}
