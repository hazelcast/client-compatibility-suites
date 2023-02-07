const path = require("path");
const fs = require("fs");
const RC = require("./test/integration/RC");
const { expect } = require('chai');
const delay = ms => new Promise(resolve => setTimeout(resolve, ms));

function createClientConfigWithSsl(clusterName, token, __dirname, passphrase, url, smartRouting) {
    return {
        network: {
            hazelcastCloud: {
                discoveryToken: token
            },
            smartRouting: smartRouting,
            ssl: {
                enabled: true,
                sslOptions: {
                    ca: [fs.readFileSync(path.resolve(path.join(__dirname, 'ca.pem')))],
                    cert: [fs.readFileSync(path.resolve(path.join(__dirname, 'cert.pem')))],
                    key: [fs.readFileSync(path.resolve(path.join(__dirname, 'key.pem')))],
                    passphrase: passphrase,
                    rejectUnauthorized: false
                }
            }
        },
        clusterName: clusterName,
        properties: {
            'hazelcast.client.cloud.url': url,
            'hazelcast.client.statistics.enabled': true,
            'hazelcast.client.statistics.period.seconds': 1,
            'hazelcast.client.heartbeat.timeout': 10000
        }
    }
}

function createClientConfigWithoutSsl(clusterName, token, url, smartRouting) {
    return {
        network: {
            hazelcastCloud: {
                discoveryToken: token
            },
            smartRouting: smartRouting,
        },
        clusterName: clusterName,
        properties: {
            'hazelcast.client.cloud.url': url,
            'hazelcast.client.statistics.enabled': true,
            'hazelcast.client.statistics.period.seconds': 1,
            'hazelcast.client.heartbeat.timeout': 10000
        }
    }
}

async function mapPutGetAndVerify(map) {
    console.log("Given map will be filled with random entries and verify");
    await map.clear();
    let iterationCounter = 0;
    while (iterationCounter < 20) {
        const randomKey = Math.floor(Math.random() * 100000);
        const randomValue = Math.floor(Math.random() * 100000);
        await map.put('key' + randomKey, 'value' + randomValue);
        expect('value' + randomValue).to.be.equal(await map.get('key' + randomKey));
        iterationCounter++;
    }
    expect(await map.size()).to.be.equal(20, "Map size should be 20");
}

async function stopResumeCluster(clusterId, map)
{
    await mapPutGetAndVerify(map);
    console.log("Stopping cloud cluster");
    await RC.stopCloudCluster(clusterId);

    console.log("Starting cloud cluster");
    await RC.resumeCloudCluster(clusterId);
    console.log("Wait for 5 seconds to be sure client has enough time to connect");
    await delay(5000);

    await mapPutGetAndVerify(map);
}

exports.createClientConfigWithSsl = createClientConfigWithSsl;
exports.createClientConfigWithoutSsl =createClientConfigWithoutSsl;
exports.mapPutGetAndVerify = mapPutGetAndVerify;
exports.stopResumeCluster = stopResumeCluster;
