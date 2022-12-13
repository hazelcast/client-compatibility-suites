'use strict';

const RC = require('./test/integration/RC');
const Helper = require("./helperFunctions");
const {Client} = require("./lib")
const { expect } = require('chai');

describe('SslEnabledStandardClusterTests', function () {

    let client;
    let sslEnabledCluster;

    before(async function (){
        await RC.loginToCloudUsingEnvironment()
        sslEnabledCluster = await RC.createCloudCluster(process.env.HZ_VERSION, true)
    });

    afterEach(async function() {
        if (client) {
            await client.shutdown();
        }
    });

    it('TryConnectSslClusterWithoutCertificatesSmartClient', async function() {
        let value = false;
        try {
            const wrongConfig = Helper.createClientConfigWithoutSsl(sslEnabledCluster.releaseName, sslEnabledCluster.token, process.env.BASE_URL, true);
            wrongConfig.connectionStrategy = {connectionRetry: {clusterConnectTimeoutMillis: 10000}};
            client = await Client.newHazelcastClient(wrongConfig);
        } catch(e) {
            value = true;
        }
        expect(value).to.be.true;
    });

    it('TryConnectSslClusterWithCertificatesSmartClient', async function (){
        const smartClientConfig = Helper.createClientConfigWithSsl(sslEnabledCluster.releaseName, sslEnabledCluster.token, sslEnabledCluster.certificatePath, sslEnabledCluster.tlsPassword, process.env.BASE_URL, true);
        client = await Client.newHazelcastClient(smartClientConfig);
        const map = await client.getMap('mapFor_TryConnectSslClusterWithoutCertificatesSmartClient');
        await Helper.stopResumeCluster(sslEnabledCluster.id, map);
    });

    it('TryConnectSslClusterWithoutCertificatesUnisocketClient', async function() {
        let value = false;
        try {
            const wrongConfig = Helper.createClientConfigWithoutSsl(sslEnabledCluster.releaseName, sslEnabledCluster.token, process.env.BASE_URL, false);
            wrongConfig.connectionStrategy = {connectionRetry: {clusterConnectTimeoutMillis: 10000}};
            client = await Client.newHazelcastClient(wrongConfig);
        } catch(e) {
            value = true;
        }
        expect(value).to.be.true;
    });

    it('TryConnectSslClusterWithCertificatesUnisocketClient', async  function (){
        const unisocketClientConfig = Helper.createClientConfigWithSsl(sslEnabledCluster.releaseName, sslEnabledCluster.token, sslEnabledCluster.certificatePath, sslEnabledCluster.tlsPassword, process.env.BASE_URL, false);
        client = await Client.newHazelcastClient(unisocketClientConfig);
        const map = await client.getMap('mapFor_TryConnectSslClusterWithCertificatesUnisocketClient');
        await Helper.stopResumeCluster(sslEnabledCluster.id, map);
    });

    after(async function (){
        await RC.deleteCloudCluster(sslEnabledCluster.id)
    });
});

describe('SslDisabledStandardClusterTests', function () {
    let client
    let sslDisabledCluster
    before(async function (){
        await RC.loginToCloudUsingEnvironment()
        sslDisabledCluster = await RC.createCloudCluster(process.env.HZ_VERSION, false)
    });

    afterEach(async function() {
        if (client) {
            await client.shutdown();
        }
    });

    it('TryConnectSslDisabledClusterSmartClient', async function (){
        const smartClientConfig = Helper.createClientConfigWithoutSsl(sslDisabledCluster.releaseName, sslDisabledCluster.token, process.env.BASE_URL, true);
        client = await Client.newHazelcastClient(smartClientConfig);
        const map = await client.getMap('mapFor_TryConnectSslDisabledClusterSmartClient');
        await Helper.stopResumeCluster(sslDisabledCluster.id, map);
    });

    it('TryConnectSslDisabledClusterUnisocketClient', async  function (){
        const unisocketClientConfig = Helper.createClientConfigWithoutSsl(sslDisabledCluster.releaseName, sslDisabledCluster.token, process.env.BASE_URL, false);
        client = await Client.newHazelcastClient(unisocketClientConfig);
        const map = await client.getMap('mapFor_TryConnectSslDisabledClusterUnisocketClient');
        await Helper.stopResumeCluster(sslDisabledCluster.id, map);
    });

    after(async function (){
        await RC.deleteCloudCluster(sslDisabledCluster.id)
    });
});
