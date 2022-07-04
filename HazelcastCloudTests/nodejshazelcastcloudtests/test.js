'use strict';

const RC = require('./test/integration/RC');
const Helper = require("./helperFunctions");
const {Client} = require("./lib")
const { expect } = require('chai');

describe('SslEnabledStandardClusterTests', function () {

    let smartClient
    let unisocketClient
    let sslEnabledCluster
    before(async function (){
        //sslEnabledCluster = await RC.createHazelcastCloudStandardCluster(process.env.hzVersion, true)
        sslEnabledCluster = await  RC.getHazelcastCloudCluster("1532");
    });

    it('TryConnectSslClusterWithoutCertificatesSmartClient', async function() {
        let value = false;
        try {
            const wrongConfig = Helper.createClientConfigWithoutSsl(sslEnabledCluster.nameForConnect, sslEnabledCluster.token, process.env.baseUrl, true);
            wrongConfig.connectionStrategy = {connectionRetry: {clusterConnectTimeoutMillis: 10000}};
            await Client.newHazelcastClient(wrongConfig);
        }
        catch
        {
            value = true;
        }
        expect(value).to.be.true;
    });

    it('TryConnectSslClusterWithCertificatesSmartClient', async function (){
        const smartClientConfig = Helper.createClientConfigWithSsl(sslEnabledCluster.nameForConnect, sslEnabledCluster.token, sslEnabledCluster.certificatePath, sslEnabledCluster.tlsPassword, process.env.baseUrl, true);
        smartClient = await Client.newHazelcastClient(smartClientConfig);
        const map = await smartClient.getMap('mapFor_TryConnectSslClusterWithoutCertificatesSmartClient');
        await Helper.stopResumeScaleUpDownCluster(sslEnabledCluster.id, map);
        await smartClient.shutdown();
    });

    it('TryConnectSslClusterWithoutCertificatesUnisocketClient', async function() {
        let value = false;
        try {
            const wrongConfig = Helper.createClientConfigWithoutSsl(sslEnabledCluster.nameForConnect, sslEnabledCluster.token, process.env.baseUrl, false);
            wrongConfig.connectionStrategy = {connectionRetry: {clusterConnectTimeoutMillis: 10000}};
            await Client.newHazelcastClient(wrongConfig);
        }
        catch(e)
        {
            value = true;
        }
        expect(value).to.be.true;
    });

    it('TryConnectSslClusterWithCertificatesUnisocketClient', async  function (){
        const unisocketClientConfig = Helper.createClientConfigWithSsl(sslEnabledCluster.nameForConnect, sslEnabledCluster.token, sslEnabledCluster.certificatePath, sslEnabledCluster.tlsPassword, process.env.baseUrl, false);
        unisocketClient = await Client.newHazelcastClient(unisocketClientConfig);
        const map = await unisocketClient.getMap('mapFor_TryConnectSslClusterWithCertificatesUnisocketClient');
        await Helper.stopResumeScaleUpDownCluster(sslEnabledCluster.id, map);
        await unisocketClient.shutdown();
    });

    after(async function (){
    });
});

describe('SslDisabledStandardClusterTests', function () {
    let smartClient
    let unisocketClient
    let sslDisabledCluster
    before(async function (){
        //sslDisabledCluster = await RC.createHazelcastCloudStandardCluster(process.env.hzVersion, false)
        sslDisabledCluster = await RC.getHazelcastCloudCluster("1531");
    });

    it('TryConnectSslDisabledClusterWithCertificatesSmartClient', async function (){
        const smartClientConfig = Helper.createClientConfigWithoutSsl(sslDisabledCluster.nameForConnect, sslDisabledCluster.token, process.env.baseUrl, true);
        smartClient = await Client.newHazelcastClient(smartClientConfig);
        const map = await smartClient.getMap('mapFor_TryConnectSslDisabledClusterWithCertificatesSmartClient');
        await Helper.stopResumeScaleUpDownCluster(sslDisabledCluster.id, map);
        await smartClient.shutdown();
    });

    it('TryConnectSslDisabledClusterWithCertificatesUnisocketClient', async  function (){
        const unisocketClientConfig = Helper.createClientConfigWithoutSsl(sslDisabledCluster.nameForConnect, sslDisabledCluster.token, process.env.baseUrl, false);
        unisocketClient = await Client.newHazelcastClient(unisocketClientConfig);
        const map = await unisocketClient.getMap('mapFor_TryConnectSslDisabledClusterWithCertificatesUnisocketClient');
        await Helper.stopResumeScaleUpDownCluster(sslDisabledCluster.id, map);
        await unisocketClient.shutdown();
    });

    after(async function (){
    });
});
